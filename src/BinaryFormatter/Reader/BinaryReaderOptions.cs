using System;

namespace Xfrogcn.BinaryFormatter
{
    public struct BinaryReaderOptions
    {
        internal const int DefaultMaxDepth = 64;

        private int _maxDepth;
        

        /// <summary>
        /// Gets or sets the maximum depth allowed when reading Binary, with the default (i.e. 0) indicating a max depth of 64.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the max depth is set to a negative value.
        /// </exception>
        /// <remarks>
        /// Reading past this depth will throw a <exception cref="BinaryException"/>.
        /// </remarks>
        public int MaxDepth
        {
            readonly get => _maxDepth;
            set
            {
                if (value < 0)
                    throw ThrowHelper.GetArgumentOutOfRangeException_MaxDepthMustBePositive(nameof(value));

                _maxDepth = value;
            }
        }

    }
}
