using System;

namespace Xfrogcn.BinaryFormatter
{
    public interface ITypeInfoGetter
    {
        bool CanProcess(Type type);

        BinaryTypeInfo GetTypeInfo(Type type, TypeInfoGetterContext context);
    }
}
