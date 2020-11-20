using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Metadata
{
    public interface IMetadataGetter
    {
        bool CanProcess(Type type);

        bool GetTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context);
    }
}
