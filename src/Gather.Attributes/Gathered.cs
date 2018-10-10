using System;

namespace Gather.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class Gathered : Attribute
    {
        /// <summary>
        /// Indicates that this assembly can be gathered
        /// </summary>
        public Gathered()
        {
        }
    }
}
