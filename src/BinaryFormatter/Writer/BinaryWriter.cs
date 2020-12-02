using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Xfrogcn.BinaryFormatter
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed partial class BinaryWriter : IDisposable, IAsyncDisposable
    {
        private const int DefaultGrowthSize = 4096;
        private const int InitialGrowthSize = 256;

        private IBufferWriter<byte> _output;

        private Memory<byte> _memory;

        private bool _inObject;
        private BinaryTokenType _tokenType;
        //private BitStack _bitStack;


        private int _currentDepth;

        private BinarySerializerOptions _options;

        /// <summary>
        /// 尚未提交的字节数
        /// </summary>
        public int BytesPending { get; private set; }

        /// <summary>
        /// 已提交的字节数
        /// </summary>
        public long BytesCommitted { get; private set; }


        public BinarySerializerOptions Options => _options;



        public int CurrentDepth => _currentDepth;

        private byte[] headerBytes = new byte[] { (byte)'X', (byte)'G', (byte)'B' };
        private byte[] versionBytes = new byte[] { 1 };

        public BinaryWriter(IBufferWriter<byte> bufferWriter, BinarySerializerOptions options)
        {
            _output = bufferWriter ?? throw new ArgumentNullException(nameof(bufferWriter));
            _options = options;
        }

        public void WriteHeader()
        {
            _output.Write(headerBytes);
            _output.Write(versionBytes);
        }

        public void Reset()
        {
            CheckNotDisposed();
            ResetHelper();
        }

        public void Reset(IBufferWriter<byte> bufferWriter)
        {
            CheckNotDisposed();

            _output = bufferWriter ?? throw new ArgumentNullException(nameof(bufferWriter));
            ResetHelper();
        }


        public void Flush()
        {
            CheckNotDisposed();

            _memory = default;

            Debug.Assert(_output != null);
            if (BytesPending != 0)
            {
                _output.Advance(BytesPending);
                BytesCommitted += BytesPending;
                BytesPending = 0;
            }
        }

        private void ResetHelper()
        {
            BytesPending = default;
            BytesCommitted = default;
            _memory = default;

            _inObject = default;
            _tokenType = default;
            _currentDepth = default;

            // _bitStack = default;
        }

        private void CheckNotDisposed()
        {
            if (_output == null)
            {
                throw new ObjectDisposedException(nameof(BinaryWriter));
            }
        }

        public void Dispose()
        {
            if (_output == null)
            {
                return;
            }


            Flush();
            ResetHelper();

            _output = null;
        }

        public async ValueTask DisposeAsync()
        {

            if (_output == null)
            {
                return;
            }
            await FlushAsync().ConfigureAwait(false);
            ResetHelper();


            _output = null;
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            CheckNotDisposed();

            _memory = default;


            Debug.Assert(_output != null);
            if (BytesPending != 0)
            {
                _output.Advance(BytesPending);
                BytesCommitted += BytesPending;
                BytesPending = 0;
            }
            return Task.CompletedTask;
        }

        private void Grow(int requiredSize)
        {
            Debug.Assert(requiredSize > 0);


            int sizeHint = Math.Max(DefaultGrowthSize, requiredSize);

            Debug.Assert(BytesPending != 0);


            Debug.Assert(_output != null);

            _output.Advance(BytesPending);
            BytesCommitted += BytesPending;
            BytesPending = 0;

            _memory = _output.GetMemory(sizeHint);

            if (_memory.Length < sizeHint)
            {
                ThrowHelper.ThrowInvalidOperationException_NeedLargerSpan();
            }

        }

    }
}
