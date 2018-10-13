using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Gather.Attributes;

namespace Gather.Core
{
    /// <summary>
    /// The result of a gather
    /// </summary>
    public class Harvest
    {
        /// <summary>
        /// The type that was gathered
        /// </summary>
        public Type GatheredType { get; }

        /// <summary>
        /// All the interfaces this type supports
        /// </summary>
        public IEnumerable<Type> SupportedInterfaces { get; }

        internal Harvest(Type gatheredType, Type supportedInterface)
        {
            this.GatheredType = gatheredType;
            this.SupportedInterfaces = new List<Type> { supportedInterface };
        }

        internal Harvest(Type gatheredType, IEnumerable<Type> supportedInterfaces)
        {
            this.GatheredType = gatheredType;
            this.SupportedInterfaces = supportedInterfaces;
        }
    }

    /// <summary>
    /// A condition to check before gathering a type
    /// </summary>
    public class GatherCondition
    {
        /// <summary>
        /// The conditional method for loading the type
        /// </summary>
        public Func<Type, bool> Condition { get; }

        /// <summary>
        /// The name of this condition
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Condition constructor
        /// </summary>
        /// <param name="name">Name of this condition</param>
        /// <param name="condition">Method to execute before gathering</param>
        public GatherCondition(string name, Func<Type, bool> condition)
        {
            this.Condition = condition;
            this.Name = name;
        }
    }

    /// <summary>
    /// Core gatherer for loading additional types from assemblies
    /// </summary>
    public class Gatherer
    {
        private IList<DirectoryInfo> directories;
        private bool verboseLogging;
        private bool diagnosticTiming;
        private Stopwatch stopwatch;
        Action<string> logMethod;
        IList<GatherCondition> conditions = new List<GatherCondition>();

        private static GatherCondition GatheredCondition => new GatherCondition("Gathered", (x) => x.GetCustomAttribute<GatheredType>() != null);

        private bool ImplementsLoaded(Type type)
        {
            return type.GetCustomAttribute<GatheredType>() != null;
        }

        public Gatherer() : this(new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))
        {

        }

        public Gatherer(DirectoryInfo directory)
        {
            this.directories = new List<DirectoryInfo>();
            this.directories.Add(directory);
            logMethod = (x) => Debug.WriteLine(x);
            conditions.Add(GatheredCondition);
        }

        public Gatherer(IList<DirectoryInfo> directories)
        {
            this.directories = directories;
            logMethod = (x) => Debug.WriteLine(x);
            conditions.Add(GatheredCondition);
        }

        /// <summary>
        /// Harvests types from <paramref name="directory"/>
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public Gatherer From(DirectoryInfo directory)
        {
            directories.Add(directory);
            return this;
        }

        /// <summary>
        /// Harvests types from <paramref name="directories"/>
        /// </summary>
        /// <param name="directories"></param>
        /// <returns></returns>
        public Gatherer From(IList<DirectoryInfo> directories)
        {
            for (int i = 0; i < directories.Count; ++i)
            {
                this.directories.Add(directories[i]);
            }

            return this;
        }

        /// <summary>
        /// Allows overriding log statements via <paramref name="logAction"/>.
        /// </summary>
        /// <param name="logAction"></param>
        /// <returns></returns>
        public Gatherer WithLogger(Action<string> logAction)
        {
            logMethod = logAction ?? throw new ArgumentNullException(nameof(logAction));
            return this;
        }

        /// <summary>
        /// Enables verbose logging
        /// </summary>
        /// <returns></returns>
        public Gatherer WithVerboseLogging()
        {
            this.verboseLogging = true;
            return this;
        }

        /// <summary>
        /// Enables diagnostic timing for future loads
        /// </summary>
        /// <returns></returns>
        public Gatherer WithDiagnosticTiming()
        {
            diagnosticTiming = true;
            return this;
        }

        /// <summary>
        /// Adds a condition for future loads
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public Gatherer WithCondition(GatherCondition condition)
        {
            conditions.Add(condition);
            return this;
        }

        /// <summary>
        /// Removes a condition from being checked from further loads
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public Gatherer WithoutCondition(GatherCondition condition)
        {
            if(conditions.Contains(condition))
            {
                conditions.Remove(condition);
            }

            return this;
        }

        /// <summary>
        /// Loads all types that declare <see cref="GatheredType"/> from <see cref="Gathered"/> assemblies
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Harvest> LoadAll()
        {
            StartDiagnosticTiming();
            var loadedTypes = new List<Type>();

            var files = LoadAssemblyFiles(this.directories);
            var assemblies = LoadAssemblies(files);
            var types = LoadPluginTypes(assemblies);

            StopDiagnosticTiming();

            return ConvertToHarvest(types);
        }

        /// <summary>
        /// Loads only types that are of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<Harvest> LoadOnly<T>()
        {
            var typeCondition = new GatherCondition("Type " + typeof(T).Name, (x) => x.GetInterfaces().Contains(typeof(T)));

            conditions.Add(typeCondition);

            var pluginTypes = LoadAll();

            conditions.Remove(typeCondition);

            return pluginTypes;
        }

        #region Internal Implementation

        private IEnumerable<Harvest> ConvertToHarvest(IList<Type> types)
        {
            var harvest = new List<Harvest>(types.Count);
            for(int i = 0; i < types.Count; ++i)
            {
                harvest.Add(new Harvest(types[i], types[i].GetInterfaces()));
            }
            return harvest;
        }

        void Log(string message, bool isVerbose = false)
        {
            if ((isVerbose && verboseLogging) || !isVerbose)
            {
                logMethod.Invoke(message);
            }
        }

        private void StartDiagnosticTiming()
        {
            if (diagnosticTiming)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }
        }

        private void StopDiagnosticTiming()
        {
            if (diagnosticTiming)
            {
                stopwatch.Stop();
                Log("Completed load in " + stopwatch.Elapsed);
            }
        }

        private IList<Type> LoadPluginTypes(IList<Assembly> assemblies)
        {
            var types = new List<Type>();

            for (int i = 0; i < assemblies.Count; ++i)
            {
                try
                {
                    var candidateTypes = assemblies[i].GetTypes();

                    foreach (var candidateType in candidateTypes)
                    {
                        Log(GetResourceString("ConsideringType", candidateType.Name), true);

                        bool isValid = true;
                        int j = 0;
                        for (/**/; j < conditions.Count && isValid; ++j)
                        {
                            if (!conditions[j].Condition.Invoke(candidateType))
                            {
                                Log(GetResourceString("DisqualifiedBy", conditions[j].Name), true);
                                isValid = false;
                                break;
                            }
                        }

                        if (isValid)
                        {
                            Log(GetResourceString("Accepted"), true);
                            types.Add(candidateType);
                        }
                    }
                }
                catch(ReflectionTypeLoadException typeLoadException)
                {
                    Log("Type load error: " + string.Join(", ", typeLoadException.LoaderExceptions.ToList()));
                }
                catch(Exception exc)
                {
                    Log("Uhandled exception when loading: " + exc.Message);
                }
            }

            return types;
        }

        private IList<FileInfo> LoadAssemblyFiles(IList<DirectoryInfo> directories)
        {
            var files = new List<FileInfo>();

            for (int i = 0; i < directories.Count; ++i)
            {
                if (!directories[i].Exists)
                {
                    Log(GetResourceString("UnableToLoadDirectory", directories[i].FullName));
                }
                else
                {
                    files.AddRange(directories[i].GetFiles("*.dll"));
                }
            }

            return files;
        }

        private IList<Assembly> LoadAssemblies(IList<FileInfo> files)
        {
            var assemblies = new List<Assembly>();

            for (int i = 0; i < files.Count; ++i)
            {
                try
                {
                    var candidateAssembly = Assembly.LoadFrom(files[i].FullName);
                    if(candidateAssembly.GetCustomAttribute<Gathered>() != null)
                    {
                        assemblies.Add(candidateAssembly);
                    }
                }
                catch (Exception)
                {
                    Log(GetResourceString("UnableToLoadAssembly", files[i].FullName));
                }
            }

            return assemblies;
        }

        private string GetResourceString(string name, params string[] parameters)
        {
            if(parameters != null && parameters.Length > 0)
            {
                return string.Format(LogMessages.ResourceManager.GetString(name), parameters);
            }

            return LogMessages.ResourceManager.GetString(name);
        }

        #endregion Internal Implementation
    }
}
