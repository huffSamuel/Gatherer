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
        private Action<ReflectionTypeLoadException> typeLoadHandler;
        private static GatherCondition GatheredCondition => new GatherCondition("Gathered", (x) => x.GetCustomAttribute<GatheredType>() != null);

        /// <summary>
        /// Constructs a new Gatherer
        /// </summary>
        public Gatherer() : this(new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))
        {

        }

        /// <summary>
        /// Constructs a new Gatherer
        /// </summary>
        /// <param name="directory">Directory that this gatherer should harvest.</param>
        public Gatherer(DirectoryInfo directory)
        {
            this.directories = new List<DirectoryInfo> { directory };
            logMethod = (x) => Debug.WriteLine(x);
            conditions.Add(GatheredCondition);
            typeLoadHandler = (e) =>
            {
                Log(GetResourceString("ReflectionTypeLoadException", string.Join(", ", e.LoaderExceptions.Select(x => x.Message).ToList())));
            };
        }

        /// <summary>
        /// Constructs a new Gatherer
        /// </summary>
        /// <param name="directories">List of directories this gatherer should harvest.</param>
        public Gatherer(IList<DirectoryInfo> directories)
        {
            this.directories = directories;
            logMethod = (x) => Debug.WriteLine(x);
            conditions.Add(GatheredCondition);
            typeLoadHandler = (e) =>
            {
                Log(GetResourceString("ReflectionTypeLoadException", string.Join(", ", e.LoaderExceptions.Select(x => x.Message).ToList())));
            };
        }

        /// <summary>
        /// Sets a custom handler when ReflectionTypeLoadExceptions occur during assembly loading.
        /// </summary>
        /// <param name="handleAction">Action to perform when an exception occurs</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        public Gatherer SetTypeLoadExceptionHandler(Action<ReflectionTypeLoadException> handleAction)
        {
            typeLoadHandler = handleAction ?? throw new ArgumentNullException(nameof(handleAction));
            return this;
        }

        /// <summary>
        /// Harvests types from <paramref name="directory"/>
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        public Gatherer From(DirectoryInfo directory)
        {
            if(directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            directories.Add(directory);
            return this;
        }

        /// <summary>
        /// Harvests types from <paramref name="directories"/>
        /// </summary>
        /// <param name="directories"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        public Gatherer From(IList<DirectoryInfo> directories)
        {
            if(directories == null)
            {
                throw new ArgumentNullException(nameof(directories));
            }

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
        /// <exception cref="ArgumentNullException"/>
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
        /// <exception cref="ArgumentNullException"/>
        public Gatherer WithCondition(GatherCondition condition)
        {
            if(condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            conditions.Add(condition);
            return this;
        }

        /// <summary>
        /// Removes a condition from being checked from further loads
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        public Gatherer WithoutCondition(GatherCondition condition)
        {
            if(condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

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
                Log(GetResourceString("CompletedLoad", stopwatch.Elapsed.ToString()));
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
                    typeLoadHandler?.Invoke(typeLoadException);
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
