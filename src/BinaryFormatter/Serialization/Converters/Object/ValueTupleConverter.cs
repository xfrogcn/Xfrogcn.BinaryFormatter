namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    /// <summary>
    /// ValueTuple转换器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ValueTupleConverter<T> : ObjectDefaultConverter<T> where T : notnull
    {
        public ValueTupleConverter()
        {
            IncludeFields = true;
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            base.SetTypeMetadata(typeInfo, typeMap, options);
            typeInfo.Type = TypeEnum.ValueTuple;
            typeInfo.FullName = null;
        }
        
    }
}
