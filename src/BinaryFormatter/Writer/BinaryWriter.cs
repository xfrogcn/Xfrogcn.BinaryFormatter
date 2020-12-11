﻿using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

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

        private byte[] versionBytes = new byte[] { 1 };

        public BinaryWriter(IBufferWriter<byte> bufferWriter, BinarySerializerOptions options)
        {
            _output = bufferWriter ?? throw new ArgumentNullException(nameof(bufferWriter));
            _options = options;
        }

        public void WriteHeader()
        {
            int required = 4;
            if (_memory.Length - BytesPending < required)
            {
                Grow(required);
            }

            Span<byte> span = _memory.Span;
            span[BytesPending++] = (byte)'X';
            span[BytesPending++] = (byte)'B';
            span[BytesPending++] = (byte)'F';
            span[BytesPending++] = 1;
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

        internal void WriteTypeSeq(ushort typeSeq)
        {
            if(_memory.Length - BytesPending < 2)
            {
                Grow(2);
            }
            byte[] typeBytes = BitConverter.GetBytes(typeSeq);
            Span<byte> output = _memory.Span;
            output[BytesPending++] = typeBytes[0];
            output[BytesPending++] = typeBytes[1];
        }

        internal void WriteTypeSeqAndBytes(ushort typeSeq, ReadOnlySpan<byte> bytes)
        {
            int maxRequired = 2 + bytes.Length;
            if(_memory.Length - BytesPending < maxRequired)
            {
                Grow(maxRequired);
            }

            byte[] typeBytes =  BitConverter.GetBytes(typeSeq);
            Span<byte> output = _memory.Span;
            output[BytesPending++] = typeBytes[0];
            output[BytesPending++] = typeBytes[1];

            Span<byte> bytesOutput = output.Slice(BytesPending);
            bytes.CopyTo(bytesOutput);
        }

        internal void WriteBytes(ReadOnlySpan<byte> bytes)
        {
            if (_memory.Length - BytesPending < bytes.Length)
            {
                Grow(bytes.Length);
            }

            var output = _memory.Span.Slice(BytesPending);
            bytes.CopyTo(output);
            BytesPending += bytes.Length;
        }

        internal Span<byte> TryGetWriteSpan(int len)
        {
            Debug.Assert(len > 0);
            if (_memory.Length - BytesPending < len)
            {
                Grow(len);
            }

            var output = _memory.Span.Slice(BytesPending);
            BytesPending += len;
            return output;
        }


        internal void WriteTypeInfos(IList<BinaryTypeInfo> typeList, ushort primaryTypeSeq)
        {
            //元数据
            Debug.Assert(typeList != null);
            long startPosition = BytesCommitted + BytesPending;
            WriteBytes(BitConverter.GetBytes((ushort)(typeList.Count)));
            foreach(BinaryTypeInfo ti in typeList)
            {
                WriteTypeInfo(ti);
            }

            // 主类型
            WriteUInt16Value(primaryTypeSeq);
            long endPosition = BytesCommitted + BytesPending;
            WriteUInt32Value((uint)(endPosition - startPosition));
        }

        internal void WritePropertyStringSeq()
        {
            WriteByteValue((byte)80);
        }

        internal void WritePropertySeq(ushort seq)
        {
            // 写属性索引
            if (seq > (ushort)0x7FFF)
            {
                ThrowHelper.ThrowBinaryException();
            }
            // 反位写
            if (_memory.Length - BytesPending < 2)
            {
                Grow(2);
            }

            var output = _memory.Span;
            output[BytesPending++] = (byte)(seq >> 8);
            output[BytesPending++] = (byte)(seq & 0xFF);
        }

        private void WriteTypeInfo(BinaryTypeInfo typeInfo)
        {
            byte[] typeData = typeInfo.GetBytes();
            WriteBytes(typeData);
            ushort memberCount = (ushort)(typeInfo.MemberInfos == null ? 0 : typeInfo.MemberInfos.Length);
            WriteUInt16Value(memberCount);
            if (memberCount > 0)
            {
                foreach(BinaryMemberInfo mi in typeInfo.MemberInfos)
                {
                    byte[] memberData = mi.GetBytes();
                    WriteBytes(memberData);
                }
            }

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

            if (_memory.Length == 0)
            {
                FirstCallToGetMemory(requiredSize);
                return;
            }

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

        private void FirstCallToGetMemory(int requiredSize)
        {
            Debug.Assert(_memory.Length == 0);
            Debug.Assert(BytesPending == 0);

            int sizeHint = Math.Max(InitialGrowthSize, requiredSize);

            Debug.Assert(_output != null);
            _memory = _output.GetMemory(sizeHint);

            if (_memory.Length < sizeHint)
            {
                ThrowHelper.ThrowInvalidOperationException_NeedLargerSpan();
            }

        }

    }
}
