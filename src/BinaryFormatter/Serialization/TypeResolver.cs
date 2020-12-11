using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public abstract class TypeResolver
    {
        public abstract string TryGetTypeFullName(Type type);

        public abstract bool TryResolveType(TypeMap typeMap, BinaryTypeInfo typeInfo, out Type type);
    }
}
