using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Xfrogcn.BinaryFormatter.Metadata
{
    public class DefaultMetadataProvider : IMetadataProvider
    {
        readonly IList<IMetadataGetter> _getters;

        public DefaultMetadataProvider(
            IList<IMetadataGetter> getters)
        {
            _getters = getters;
        }

        public ushort GetTypeInfo([NotNull] Type type, [NotNull] MetadataGetterContext context)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (context.Map == null)
            {
                throw new ArgumentNullException(nameof(context.Map));
            }

            bool hasType = context.Map.HasType(type);
            BinaryTypeInfo typeInfo = context.Map.PushType(type);

            if (hasType)
            {
                return typeInfo.Seq;
            }

            for (int i = 0; i < _getters.Count; i++)
            {
                var getter = _getters[i];
                if (!getter.CanProcess(type))
                {
                    continue;
                }

                bool isOk = getter.GetTypeInfo(type, typeInfo, context);
                if (isOk)
                {
                    break;
                }
            }
            if(typeInfo.Type == TypeEnum.None)
            {
                throw new MetadataException(type, "获取序列化类型信息失败");
            }
            return typeInfo.Seq;
        }

        public TypeMap GetTypeMap([NotNull] Type type)
        {
            TypeMap map = new TypeMap();
            MetadataGetterContext context = new MetadataGetterContext(map, this);

            GetTypeInfo(type, context);

            return map;
        }

        
    }
}
