using System;
namespace Xfrogcn.BinaryFormatter
{
    internal struct ReadStack
    {
        internal static readonly char[] SpecialCharacters = { '.', ' ', '\'', '/', '"', '[', ']', '(', ')', '\t', '\n', '\r', '\f', '\b', '\\', '\u0085', '\u2028', '\u2029' };

        internal byte Version { get; set; }

        internal TypeMap TypeMap { get; set; }

        internal ushort PrimaryTypeSeq { get; set; }
    }
}
