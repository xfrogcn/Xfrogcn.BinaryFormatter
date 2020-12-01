using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    /// <summary>
    /// 标记序列化时所使用的构造器方法
    /// </summary>
    [AttributeUsage( AttributeTargets.Constructor, AllowMultiple = false)]
    public sealed class BinaryConstructorAttribute : BinaryAttribute
    {
        public BinaryConstructorAttribute()
        {

        }
    }
}
