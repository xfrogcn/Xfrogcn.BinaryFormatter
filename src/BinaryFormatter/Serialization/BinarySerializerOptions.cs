﻿using System;
using System.Collections.Concurrent;
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

        private readonly ConcurrentDictionary<Type, TypeMap> _typeMapCache = new ConcurrentDictionary<Type, TypeMap>();


        //private IMetadataProvider _metaDataProvider = new DefaultMetadataProvider(;

        private bool _haveTypesBeenCreated;

        public IMetadataProvider MetadataProvider { get; }

        private MetadataGetterList _metadataGetterList = null;
        public IList<IMetadataGetter> MetadataGetterList => _metadataGetterList;

        public BinarySerializerOptions()
        {
            _metadataGetterList = new MetadataGetterList(this);
            MetadataProvider = new DefaultMetadataProvider(_metadataGetterList);
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

        internal TypeMap GetTypeMap(Type type)
        {
            if (_haveTypesBeenCreated == false)
            {
                // 初始化MetadataGetter
                _metadataGetterList.InitMetadataGetterList();
            }
            _haveTypesBeenCreated = true;

            return _typeMapCache.GetOrAdd(type, (t) =>
            {
                return MetadataProvider.GetTypeMap(t);
            });
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
