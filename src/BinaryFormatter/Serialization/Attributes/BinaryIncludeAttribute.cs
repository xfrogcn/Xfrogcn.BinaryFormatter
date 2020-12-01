using System;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    /// <summary>
    /// 包含，默认配置不包含字段，通过此特性可以指定包含某些字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | System.AttributeTargets.Field, AllowMultiple = false)]
    public sealed class BinaryIncludeAttribute : BinaryAttribute
    {
        public BinaryIncludeAttribute() { }
    }
}
