using System;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    /// <summary>
    /// 忽略
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class BinaryIgnoreAttribute : BinaryAttribute
    {
        /// <summary>
        /// Specifies the condition that must be met before a property or field will be ignored.
        /// </summary>
        /// <remarks>The default value is <see cref="BinaryIgnoreCondition.Always"/>.</remarks>
        public BinaryIgnoreCondition Condition { get; set; } = BinaryIgnoreCondition.Always;

        /// <summary>
        /// Initializes a new instance of <see cref="BinaryIgnoreAttribute"/>.
        /// </summary>
        public BinaryIgnoreAttribute() { }
    }
}
