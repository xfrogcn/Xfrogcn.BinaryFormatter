using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xfrogcn.BinaryFormatter.Metadata;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    public sealed partial class BinarySerializerOptions
    {
        internal const int BufferSizeDefault = 16 * 1024;
        private int _defaultBufferSize = BufferSizeDefault;
        internal static readonly BinarySerializerOptions s_defaultOptions = new BinarySerializerOptions();

       
        //private IMetadataProvider _metaDataProvider = new DefaultMetadataProvider(;

        private bool _haveTypesBeenCreated;

        public IMetadataProvider MetadataProvider { get; }

        public IList<IMetadataGetter> MetadataGetterList { get; }

        public BinarySerializerOptions()
        {
            MetadataGetterList = new MetadataGetterList(this);
        }

        public int DefaultBufferSize
        {
            get
            {
                return _defaultBufferSize;
            }
            set
            {
                VerifyMutable();

                if (value < 1)
                {
                    throw new ArgumentException("缓冲区值过小");
                }

                _defaultBufferSize = value;
            }
        }

        internal TypeMap GetTypeMap()
        {
            _haveTypesBeenCreated = true;


        }


        internal void VerifyMutable()
        {
          
            Debug.Assert(this != s_defaultOptions);

            if (_haveTypesBeenCreated)
            {
                throw new InvalidOperationException("序列化已经开始，不可再修改配置");
            }
        }
    }
}
