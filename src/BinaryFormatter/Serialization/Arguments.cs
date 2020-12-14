using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    /// <summary>
    ///  用于传递小型参数化构造参数
    /// </summary>
    /// <typeparam name="TArg0"></typeparam>
    /// <typeparam name="TArg1"></typeparam>
    /// <typeparam name="TArg2"></typeparam>
    /// <typeparam name="TArg3"></typeparam>
    internal sealed class Arguments<TArg0, TArg1, TArg2, TArg3>
    {
        public TArg0 Arg0 = default!;
        public TArg1 Arg1 = default!;
        public TArg2 Arg2 = default!;
        public TArg3 Arg3 = default!;
    }
}
