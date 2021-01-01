using System;
using System.Diagnostics;
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

        public static ArgumentOutOfRangeException GetArgumentOutOfRangeException_MaxDepthMustBePositive(string parameterName)
        {
            return GetArgumentOutOfRangeException(parameterName, Strings.MaxDepthMustBePositive);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(string parameterName, string message)
        {
            return new ArgumentOutOfRangeException(parameterName, message);
        }

        [DoesNotReturn]
        public static void ThrowBinaryReaderException(ref BinaryReader binary, ExceptionResource resource, byte nextByte = default, ReadOnlySpan<byte> bytes = default)
        {
            throw GetBinaryReaderException(ref binary, resource, nextByte, bytes);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static BinaryException GetBinaryReaderException(ref BinaryReader binary, ExceptionResource resource, byte nextByte, ReadOnlySpan<byte> bytes)
        {
            string message = GetResourceString(ref binary, resource, nextByte);

            long bytePosition = binary.CurrentState._bytePosition;

            message += $" BytePosition: {bytePosition}.";
            return new BinaryReaderException(message, bytePosition);
        }

        private static bool IsPrintable(byte value) => value >= 0x20 && value < 0x7F;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetPrintableString(byte value)
        {
            return IsPrintable(value) ? ((char)value).ToString() : $"0x{value:X2}";
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException(ExceptionResource resource, int currentDepth, byte token, BinaryTokenType tokenType)
        {
            throw GetInvalidOperationException(resource, currentDepth, token, tokenType);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static InvalidOperationException GetInvalidOperationException(ExceptionResource resource, int currentDepth, byte token, BinaryTokenType tokenType)
        {
            string message = GetResourceString(resource, currentDepth, token, tokenType);
            InvalidOperationException ex = GetInvalidOperationException(message);
            ex.Source = ExceptionSourceValueToRethrowAsBinaryException;
            return ex;
        }

        // This function will convert an ExceptionResource enum value to the resource string.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetResourceString(ExceptionResource resource, int currentDepth, byte token, BinaryTokenType tokenType)
        {
            string message = "";
            switch (resource)
            {
                case ExceptionResource.DepthTooLarge:
                    message = string.Format(Strings.DepthTooLarge, currentDepth & BinarySerializerConstants.RemoveFlagsBitMask, BinarySerializerConstants.MaxWriterDepth);
                    break;
                
                default:
                    Debug.Fail($"The ExceptionResource enum value: {resource} is not part of the switch. Add the appropriate case and exception message.");
                    break;
            }

            return message;
        }

        // This function will convert an ExceptionResource enum value to the resource string.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetResourceString(ref BinaryReader binary, ExceptionResource resource, byte nextByte)
        {
           
            string message = "";
            switch (resource)
            {
                case ExceptionResource.ExpectedBinaryTokens:
                    message = Strings.ExpectedBinaryTokens;
                    break;
                default:
                    Debug.Fail($"The ExceptionResource enum value: {resource} is not part of the switch. Add the appropriate case and exception message.");
                    break;
            }

            return message;
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private static InvalidOperationException GetInvalidOperationException(string message, BinaryTokenType tokenType)
        {
            return GetInvalidOperationException(string.Format(Strings.InvalidCast, tokenType, message));
        }

        public static InvalidOperationException GetInvalidOperationException_ExpectedString(BinaryTokenType tokenType)
        {
            return GetInvalidOperationException("string", tokenType);
        }
    }

    internal enum ExceptionResource
    {
        InvalidByte,
        DepthTooLarge,
        ExpectedBinaryTokens,
    }

}
