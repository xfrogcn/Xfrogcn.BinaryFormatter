using System;
using Xfrogcn.BinaryFormatter.Metadata;

namespace Xfrogcn.BinaryFormatter
{
    /// <summary>
    /// 类型元数据获取器上下文
    /// </summary>
    public class MetadataGetterContext
    {
        readonly TypeMap _map;
        readonly IMetadataProvider _provider;
        public MetadataGetterContext(TypeMap map, IMetadataProvider provider)
        {
            if(map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }
            if(provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }
            _map = map;
            _provider = provider;
        }

        public TypeMap Map => _map;

        public IMetadataProvider MetadataProvider => _provider;


        public ushort GetTypeSeq(Type type, MetadataGetterContext context)
        {
            return _provider.GetTypeInfo(type, context);
        }
    }
}
