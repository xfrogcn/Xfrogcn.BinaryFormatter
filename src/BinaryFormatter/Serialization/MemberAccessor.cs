using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static Xfrogcn.BinaryFormatter.BinaryClassInfo;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    internal abstract class MemberAccessor
    {
        public abstract ConstructorDelegate CreateConstructor(Type classType);

        public abstract ParameterizedConstructorDelegate<T> CreateParameterizedConstructor<T>(ConstructorInfo constructor);

        public abstract ParameterizedConstructorDelegate<T, TArg0, TArg1, TArg2, TArg3>
            CreateParameterizedConstructor<T, TArg0, TArg1, TArg2, TArg3>(ConstructorInfo constructor);

        public abstract Action<TCollection, object> CreateAddMethodDelegate<TCollection>();

        public abstract Func<IEnumerable<TElement>, TCollection> CreateImmutableEnumerableCreateRangeDelegate<TElement, TCollection>();

        public abstract Func<IEnumerable<KeyValuePair<string, TElement>>, TCollection> CreateImmutableDictionaryCreateRangeDelegate<TElement, TCollection>();

        public abstract Func<object, TProperty> CreatePropertyGetter<TProperty>(PropertyInfo propertyInfo);

        public abstract Action<object, TProperty> CreatePropertySetter<TProperty>(PropertyInfo propertyInfo);

        public abstract Func<object, TProperty> CreateFieldGetter<TProperty>(FieldInfo fieldInfo);

        public abstract Action<object, TProperty> CreateFieldSetter<TProperty>(FieldInfo fieldInfo);
    }
}
