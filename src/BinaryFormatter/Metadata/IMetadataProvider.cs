using System;

namespace Xfrogcn.BinaryFormatter.Metadata
{
    public interface IMetadataProvider
    {
        TypeMap GetTypeMap(Type type);

        ushort GetTypeInfo(Type type, MetadataGetterContext context);
    }
}
