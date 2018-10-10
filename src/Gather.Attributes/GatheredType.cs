using System;

namespace Gather.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class GatheredType : Attribute
    {
        /// <summary>
        /// Indicates this type should be gathered
        /// </summary>
        public GatheredType()
        {
        }
    }
}
