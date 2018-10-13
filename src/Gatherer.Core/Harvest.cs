using System;
using System.Collections.Generic;

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
}
