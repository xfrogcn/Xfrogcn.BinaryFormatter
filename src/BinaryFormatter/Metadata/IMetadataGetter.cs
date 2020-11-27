using System;

namespace Xfrogcn.BinaryFormatter
{
    public interface IMetadataGetter
    {
        bool CanProcess(Type type);

        bool GetTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context);
    }
}
