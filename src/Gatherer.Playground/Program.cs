using Gather.Core;
using Ninject;
using System;
using PLoader.Playground.Interfaces;
using System.Linq;
using System.Reflection;
using System.IO;

namespace PLoader.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var l = new Gatherer()
                .From(new DirectoryInfo(Assembly.GetExecutingAssembly().Location))
                .WithDiagnosticTiming()
                .WithVerboseLogging()
                .WithLogger(x => Console.WriteLine(x));
            var loadedTypes = l.LoadAll();

            Console.WriteLine("Load All");
            foreach(var type in loadedTypes)
            {
                PrintTypeInfo(type);
            }

            Console.WriteLine("Adding to Ninject container");

            var container = new StandardKernel();

            foreach(var harvest in loadedTypes)
            {
                if(harvest.SupportedInterfaces.Contains(typeof(ITypeA)))
                {
                    container.Bind<ITypeA>().To(harvest.GatheredType);
                }
                if (harvest.SupportedInterfaces.Contains(typeof(ITypeB)))
                {
                    container.Bind<ITypeB>().To(harvest.GatheredType);
                }
                if (harvest.SupportedInterfaces.Contains(typeof(ITypeC)))
                {
                    container.Bind<ITypeC>().To(harvest.GatheredType);
                }
            }

            var typeAs = container.GetAll<ITypeA>();
            var typeBs = container.GetAll<ITypeB>();
            var typeCs = container.GetAll<ITypeC>();

            Console.WriteLine($"Found {typeAs.Count()} type a");
            Console.WriteLine($"Found {typeBs.Count()} type b");
            Console.WriteLine($"Found {typeCs.Count()} type c");

            Console.ReadLine();
        }

        static void PrintTypeInfo(Harvest harvest)
        {
            Console.WriteLine("Discovered type " + harvest.GatheredType.Name);
            Console.WriteLine("Implements: ");
            foreach(var i in harvest.SupportedInterfaces)
            {
                Console.WriteLine("  + " + i.Name);
            }
        }
    }
}
