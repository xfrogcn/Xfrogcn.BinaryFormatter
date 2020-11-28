using System;

namespace Xfrogcn.BinaryFormatter
{
    public interface IMetadataProvider
    {
        TypeMap GetTypeMap(Type type);

        ushort GetTypeInfo(Type type, MetadataGetterContext context);
    }
}
