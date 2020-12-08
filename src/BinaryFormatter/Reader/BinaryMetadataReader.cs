using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    /// <summary>
    /// 读取序列化元数据
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal ref struct BinaryMetadataReader
    {
        private ReadOnlySpan<byte> _metadataData;
        public BinaryMetadataReader(ReadOnlySpan<byte> metadataData)
        {
            _metadataData = metadataData;
        }

        public BinaryTypeInfo[] ReadTypeList()
        {

        }
    }
}
