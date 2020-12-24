using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    public abstract partial class BinaryConverter
    {
        internal BinaryConverter() { }

        /// <summary>
        /// Determines whether the type can be converted.
        /// </summary>
        /// <param name="typeToConvert">The type is checked as to whether it can be converted.</param>
        /// <returns>True if the type can be converted, false otherwise.</returns>
        public abstract bool CanConvert(Type typeToConvert);

        internal abstract ClassType ClassType { get; }

        /// <summary>
        /// 默认的固定字节数, 返回0的数，表示固定字节，返回0，表示自带长度格式，返回小于0，表示不定，reader将根据当前TypeEnum类型自动尝试读取
        /// </summary>
        public virtual int GetBytesCount(ref BinaryReader reader,BinarySerializerOptions options)
        {
            return BinarySerializerConstants.BytesCount_Dynamic ;
        }

        /// <summary>
        /// Can direct Read or Write methods be called (for performance).
        /// </summary>
        //internal bool CanUseDirectReadOrWrite { get; set; }

        /// <summary>
        /// Can the converter have $id metadata.
        /// </summary>
        internal virtual bool CanHaveIdMetadata => true;

        internal virtual bool IncludeFields { get; set; } = false;

        internal bool CanBePolymorphic { get; set; }

        internal abstract BinaryPropertyInfo CreateBinaryPropertyInfo();

        internal abstract BinaryParameterInfo CreateBinaryParameterInfo();

        internal abstract Type ElementType { get; }


        public virtual MethodInfo[] GetAdditionalDataMethod() => null;

        /// <summary>
        /// 设置类型的元数据
        /// </summary>
        /// <param name="typeInfo">类型元数据</param>
        /// <param name="typeMap">当前序列化的TypeMap</param>
        /// <returns></returns>
        public abstract void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options);



        internal ushort GetTypeSeq(TypeMap typeMap, BinarySerializerOptions options)
        {
            Debug.Assert(RuntimeType != null);
            Debug.Assert(typeMap != null);

            bool isAdd = typeMap.TryAdd(RuntimeType, out BinaryTypeInfo ti);
            if (isAdd || ti.Type == TypeEnum.None)
            {
                ti.IsGeneric = RuntimeType.IsGenericType;
                ti.GenericArgumentCount = RuntimeType.GetGenericArgumentCount();
                if (ti.GenericArgumentCount > 0)
                {
                    ti.GenericArguments = RuntimeType.GetGenericArguments()
                        .Select(t =>
                        {
                            var converter = options.GetConverter(t);
                            Debug.Assert(converter != null);
                            return converter.GetTypeSeq(typeMap, options);
                        }).ToArray();
                }
                SetTypeMetadata(ti, typeMap, options);
            }
           
            return ti.Seq;
        }

        /// <summary>
        /// Cached value of TypeToConvert.IsValueType, which is an expensive call.
        /// </summary>
        internal bool IsValueType { get; set; }

        /// <summary>
        /// Whether the converter is built-in.
        /// </summary>
        internal bool IsInternalConverter { get; set; }

        ///// <summary>
        ///// Whether the converter is built-in and handles a number type.
        ///// </summary>
        //internal bool IsInternalConverterForNumberType;

        /// <summary>
        /// Loosely-typed ReadCore() that forwards to strongly-typed ReadCore().
        /// </summary>
        internal abstract object ReadCoreAsObject(ref BinaryReader reader, BinarySerializerOptions options, ref ReadStack state);

        // For polymorphic cases, the concrete type to create.
        internal virtual Type RuntimeType => TypeToConvert;

        internal bool ShouldFlush(BinaryWriter writer, ref WriteStack state)
        {
            // If surpassed flush threshold then return false which will flush stream.
            return (state.FlushThreshold > 0 && writer.BytesPending > state.FlushThreshold);
        }

        // This is used internally to quickly determine the type being converted for BinaryConverter<T>.
        internal abstract Type TypeToConvert { get; }

        internal abstract bool TryReadAsObject(ref BinaryReader reader, BinarySerializerOptions options, ref ReadStack state, out object value);

        internal abstract bool TryWriteAsObject(BinaryWriter writer, object value, BinarySerializerOptions options, ref WriteStack state);

        /// <summary>
        /// Loosely-typed WriteCore() that forwards to strongly-typed WriteCore().
        /// </summary>
        internal abstract bool WriteCoreAsObject(BinaryWriter writer, object value, BinarySerializerOptions options, ref WriteStack state);

        /// <summary>
        /// Loosely-typed WriteWithQuotes() that forwards to strongly-typed WriteWithQuotes().
        /// </summary>
        internal abstract void WriteWithQuotesAsObject(BinaryWriter writer, object value, BinarySerializerOptions options, ref WriteStack state);

        // Whether a type (ClassType.Object) is deserialized using a parameterized constructor.
        internal virtual bool ConstructorIsParameterized { get; }

        internal ConstructorInfo ConstructorInfo { get; set; }

        internal virtual void Initialize(BinarySerializerOptions options) { }

        /// <summary>
        /// Creates the instance and assigns it to state.Current.ReturnValue.
        /// </summary>
        internal virtual void CreateInstanceForReferenceResolver(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options) { }
    }
}
