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

        // This function will convert an ExceptionResource enum value to the resource string.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetResourceString(ref BinaryReader binary, ExceptionResource resource, byte nextByte)
        {
            string character = GetPrintableString(nextByte);

            string message = "";
            switch (resource)
            {
                //case ExceptionResource.ArrayDepthTooLarge:
                //    message = SR.Format(SR.ArrayDepthTooLarge, json.CurrentState.Options.MaxDepth);
                //    break;
                //case ExceptionResource.MismatchedObjectArray:
                //    message = SR.Format(SR.MismatchedObjectArray, character);
                //    break;
                //case ExceptionResource.TrailingCommaNotAllowedBeforeArrayEnd:
                //    message = SR.TrailingCommaNotAllowedBeforeArrayEnd;
                //    break;
                //case ExceptionResource.TrailingCommaNotAllowedBeforeObjectEnd:
                //    message = SR.TrailingCommaNotAllowedBeforeObjectEnd;
                //    break;
                //case ExceptionResource.EndOfStringNotFound:
                //    message = SR.EndOfStringNotFound;
                //    break;
                //case ExceptionResource.RequiredDigitNotFoundAfterSign:
                //    message = SR.Format(SR.RequiredDigitNotFoundAfterSign, character);
                //    break;
                //case ExceptionResource.RequiredDigitNotFoundAfterDecimal:
                //    message = SR.Format(SR.RequiredDigitNotFoundAfterDecimal, character);
                //    break;
                //case ExceptionResource.RequiredDigitNotFoundEndOfData:
                //    message = SR.RequiredDigitNotFoundEndOfData;
                //    break;
                //case ExceptionResource.ExpectedEndAfterSingleJson:
                //    message = SR.Format(SR.ExpectedEndAfterSingleJson, character);
                //    break;
                //case ExceptionResource.ExpectedEndOfDigitNotFound:
                //    message = SR.Format(SR.ExpectedEndOfDigitNotFound, character);
                //    break;
                //case ExceptionResource.ExpectedNextDigitEValueNotFound:
                //    message = SR.Format(SR.ExpectedNextDigitEValueNotFound, character);
                //    break;
                //case ExceptionResource.ExpectedSeparatorAfterPropertyNameNotFound:
                //    message = SR.Format(SR.ExpectedSeparatorAfterPropertyNameNotFound, character);
                //    break;
                //case ExceptionResource.ExpectedStartOfPropertyNotFound:
                //    message = SR.Format(SR.ExpectedStartOfPropertyNotFound, character);
                //    break;
                //case ExceptionResource.ExpectedStartOfPropertyOrValueNotFound:
                //    message = SR.ExpectedStartOfPropertyOrValueNotFound;
                //    break;
                //case ExceptionResource.ExpectedStartOfPropertyOrValueAfterComment:
                //    message = SR.Format(SR.ExpectedStartOfPropertyOrValueAfterComment, character);
                //    break;
                //case ExceptionResource.ExpectedStartOfValueNotFound:
                //    message = SR.Format(SR.ExpectedStartOfValueNotFound, character);
                //    break;
                //case ExceptionResource.ExpectedValueAfterPropertyNameNotFound:
                //    message = SR.ExpectedValueAfterPropertyNameNotFound;
                //    break;
                //case ExceptionResource.FoundInvalidCharacter:
                //    message = SR.Format(SR.FoundInvalidCharacter, character);
                //    break;
                //case ExceptionResource.InvalidEndOfJsonNonPrimitive:
                //    message = SR.Format(SR.InvalidEndOfJsonNonPrimitive, json.TokenType);
                //    break;
                //case ExceptionResource.ObjectDepthTooLarge:
                //    message = SR.Format(SR.ObjectDepthTooLarge, json.CurrentState.Options.MaxDepth);
                //    break;
                //case ExceptionResource.ExpectedFalse:
                //    message = SR.Format(SR.ExpectedFalse, characters);
                //    break;
                //case ExceptionResource.ExpectedNull:
                //    message = SR.Format(SR.ExpectedNull, characters);
                //    break;
                //case ExceptionResource.ExpectedTrue:
                //    message = SR.Format(SR.ExpectedTrue, characters);
                //    break;
                //case ExceptionResource.InvalidCharacterWithinString:
                //    message = SR.Format(SR.InvalidCharacterWithinString, character);
                //    break;
                //case ExceptionResource.InvalidCharacterAfterEscapeWithinString:
                //    message = SR.Format(SR.InvalidCharacterAfterEscapeWithinString, character);
                //    break;
                //case ExceptionResource.InvalidHexCharacterWithinString:
                //    message = SR.Format(SR.InvalidHexCharacterWithinString, character);
                //    break;
                //case ExceptionResource.EndOfCommentNotFound:
                //    message = SR.EndOfCommentNotFound;
                //    break;
                //case ExceptionResource.ZeroDepthAtEnd:
                //    message = SR.Format(SR.ZeroDepthAtEnd);
                //    break;
                case ExceptionResource.ExpectedBinaryTokens:
                    message = Strings.ExpectedBinaryTokens;
                    break;
                //case ExceptionResource.NotEnoughData:
                //    message = SR.NotEnoughData;
                //    break;
                //case ExceptionResource.ExpectedOneCompleteToken:
                //    message = SR.ExpectedOneCompleteToken;
                //    break;
                //case ExceptionResource.InvalidCharacterAtStartOfComment:
                //    message = SR.Format(SR.InvalidCharacterAtStartOfComment, character);
                //    break;
                //case ExceptionResource.UnexpectedEndOfDataWhileReadingComment:
                //    message = SR.Format(SR.UnexpectedEndOfDataWhileReadingComment);
                //    break;
                //case ExceptionResource.UnexpectedEndOfLineSeparator:
                //    message = SR.Format(SR.UnexpectedEndOfLineSeparator);
                //    break;
                //case ExceptionResource.InvalidLeadingZeroInNumber:
                //    message = SR.Format(SR.InvalidLeadingZeroInNumber, character);
                //    break;
                default:
                    Debug.Fail($"The ExceptionResource enum value: {resource} is not part of the switch. Add the appropriate case and exception message.");
                    break;
            }

            return message;
        }

    }

    internal enum ExceptionResource
    {
        //ArrayDepthTooLarge,
        //EndOfCommentNotFound,
        //EndOfStringNotFound,
        //RequiredDigitNotFoundAfterDecimal,
        //RequiredDigitNotFoundAfterSign,
        //RequiredDigitNotFoundEndOfData,
        //ExpectedEndAfterSingleJson,
        //ExpectedEndOfDigitNotFound,
        //ExpectedFalse,
        //ExpectedNextDigitEValueNotFound,
        //ExpectedNull,
        //ExpectedSeparatorAfterPropertyNameNotFound,
        //ExpectedStartOfPropertyNotFound,
        //ExpectedStartOfPropertyOrValueNotFound,
        //ExpectedStartOfPropertyOrValueAfterComment,
        //ExpectedStartOfValueNotFound,
        //ExpectedTrue,
        //ExpectedValueAfterPropertyNameNotFound,
        //FoundInvalidCharacter,
        //InvalidCharacterWithinString,
        //InvalidCharacterAfterEscapeWithinString,
        //InvalidHexCharacterWithinString,
        //InvalidEndOfJsonNonPrimitive,
        //MismatchedObjectArray,
        //ObjectDepthTooLarge,
        //ZeroDepthAtEnd,
        //DepthTooLarge,
        //CannotStartObjectArrayWithoutProperty,
        //CannotStartObjectArrayAfterPrimitiveOrClose,
        //CannotWriteValueWithinObject,
        //CannotWriteValueAfterPrimitiveOrClose,
        //CannotWritePropertyWithinArray,
        ExpectedBinaryTokens,
        //TrailingCommaNotAllowedBeforeArrayEnd,
        //TrailingCommaNotAllowedBeforeObjectEnd,
        //InvalidCharacterAtStartOfComment,
        //UnexpectedEndOfDataWhileReadingComment,
        //UnexpectedEndOfLineSeparator,
        //ExpectedOneCompleteToken,
        //NotEnoughData,
        //InvalidLeadingZeroInNumber,
    }

}
