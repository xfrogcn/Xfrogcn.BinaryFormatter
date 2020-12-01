using System;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class BinaryPropertyNameAttribute : BinaryAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BinaryPropertyNameAttribute"/> with the specified property name.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        public BinaryPropertyNameAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; }
    }
}
