using System;

namespace Gather.Core
{
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
}
