using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Xfrogcn.BinaryFormatter.Resources;

namespace Xfrogcn.BinaryFormatter
{
    internal static partial class ThrowHelper
    {
        public const string ExceptionSourceValueToRethrowAsBinaryException = "Xfrogcn.BinaryFormatter.Rethrowable";

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NeedLargerSpan()
        {
            throw GetInvalidOperationException(Strings.FailedToGetLargerSpan);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static InvalidOperationException GetInvalidOperationException(string message)
        {
            var ex = new InvalidOperationException(message);
            ex.Source = ExceptionSourceValueToRethrowAsBinaryException;
            return ex;
        }
    }
}
