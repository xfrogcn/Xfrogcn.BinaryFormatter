using System;
using System.Collections.Generic;
using System.Reflection;
using static Xfrogcn.BinaryFormatter.BinaryClassInfo;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class IEnumerableKeyValuePairConverter<TCollection, TKey, TValue>
        : DictionaryEnumeratorConverter<TCollection, TKey, TValue>
        where TCollection : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly bool _useCreator = false;
        private readonly ConstructorInfo _ctor = null;

        public IEnumerableKeyValuePairConverter()
        {
            Type t = typeof(TCollection);
            Type ctorParameterType = typeof(Dictionary<TKey, TValue>);
            ConstructorInfo ci = t.GetConstructor(new Type[] { ctorParameterType });

            if(ci == null)
            {
                ctorParameterType = typeof(IDictionary<TKey, TValue>);
                ci = t.GetConstructor(new Type[] { ctorParameterType });
                if(ci == null)
                {
                    ctorParameterType = typeof(IEnumerable<KeyValuePair<TKey, TValue>>);
                    ci = t.GetConstructor(new Type[] { ctorParameterType });
                }
               
            }
            if (ci != null)
            {
                _useCreator = true;
                _ctor = ci;
            }
            
        }

        protected override void Add(TKey key, in TValue value, BinarySerializerOptions options, ref ReadStack state)
        {
            var addMethod = (Action<object, TKey, TValue>)state.Current.BinaryClassInfo.AddMethodDelegate;
            addMethod!(state.Current.ReturnValue, key, value);
        }

        protected override void ConvertCollection(ref ReadStack state, BinarySerializerOptions options)
        {
            if (_useCreator)
            {
                var creator = (ParameterizedConstructorDelegate<TCollection, Dictionary<TKey,TValue>, object, object, object>)state.Current.BinaryClassInfo.CreateObjectWithArgs;
                state.Current.ReturnValue = creator((Dictionary<TKey,TValue>)state.Current.ReturnValue, null, null, null);
            }
        }

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state)
        {
            BinaryClassInfo classInfo = state.Current.BinaryClassInfo;
            if (_useCreator)
            {
                state.Current.ReturnValue = new Dictionary<TKey, TValue>();
                if (classInfo.AddMethodDelegate == null)
                {
                    Action<object, TKey, TValue> addMethod = (dic, key, value) =>
                    {
                        ((Dictionary<TKey, TValue>)dic)[key] = value;
                    };
                    classInfo.AddMethodDelegate = addMethod;
                }
                if (classInfo.CreateObjectWithArgs == null)
                {
                    classInfo.CreateObjectWithArgs =
                        state.Options.MemberAccessorStrategy.CreateParameterizedConstructor<TCollection, Dictionary<TKey, TValue>, object, object, object>(_ctor);
                }
                
                
            }
            else
            {
                if(classInfo.CreateObject == null)
                {
                    ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(typeof(TCollection), ref reader, ref state);
                }
                TCollection returnValue = (TCollection)classInfo.CreateObject();
                state.Current.ReturnValue = returnValue;

                // Add方法
                if (classInfo.AddMethodDelegate == null)
                {
                    var addMethod = state.Options.MemberAccessorStrategy.CreateDictionaryAddMethod<TCollection, TKey, TValue>();
                    if (addMethod == null)
                    {
                        ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(typeof(TCollection), ref reader, ref state);
                    }
                    else
                    {
                        Action<object, TKey, TValue> realMethod = (dic, key, value) => addMethod((TCollection)dic, key, value);
                        classInfo.AddMethodDelegate = realMethod;
                    }
                }
            }
        }
    }
}
