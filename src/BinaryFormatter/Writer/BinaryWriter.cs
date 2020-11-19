using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;

namespace Xfrogcn.BinaryFormatter.Writer
{
    public sealed partial class BinaryWriter : IDisposable, IAsyncDisposable
    {
        private const int DefaultGrowthSize = 4096;
        private const int InitialGrowthSize = 256;

        private IBufferWriter<byte>? _output;
        private Stream? _stream;
        private ArrayBufferWriter<byte>? _arrayBufferWriter;

        private Memory<byte> _memory;


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
