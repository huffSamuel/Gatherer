using Gather.Attributes;
using PLoader.Playground.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

[assembly: Gathered]

namespace PLoader
{
    [GatheredType]
    public class Loder
    {
        public Loder() 
        {
            
        }
        
    }

    public interface ILoaded
    {

    }

    [GatheredType]
    public class Foo : ITypeA, ITypeB, ITypeC
    {

    }

    [GatheredType]
    public class Bar : ITypeB, ITypeC
    {

    }

    [GatheredType]
    public class Baz : ITypeA
    {

    }
}
