using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter : IDisposable, IAsyncDisposable
    {
        private const int DefaultGrowthSize = 4096;
        private const int InitialGrowthSize = 256;

        private IBufferWriter<byte> _output;
        private Stream _stream;
        private ArrayBufferWriter<byte> _arrayBufferWriter;

        private Memory<byte> _memory;
        private byte[] headerBytes = new byte[] { (byte)'X', (byte)'G', (byte)'B' };
        private byte[] versionBytes = new byte[] { 1 };

        public BinaryWriter(IBufferWriter<byte> writer)
        {
            _output = writer;
        }

        public void WriteHeader()
        {
            _output.Write(headerBytes);
            _output.Write(versionBytes);
        }

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
