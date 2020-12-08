using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public abstract class TypeResolver
    {
        public abstract bool TryResolveType(TypeMap typeMap, BinaryTypeInfo typeInfo, out Type type);
    }
}
