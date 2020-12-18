using System;
namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal abstract class BinaryCollectionConverter<TCollection, TElement> : BinaryResumableConverter<TCollection>
    {
        internal sealed override ClassType ClassType => ClassType.Enumerable;
        internal override Type ElementType => typeof(TElement);

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.SerializeType = ClassType.Enumerable;
            typeInfo.Type = TypeEnum.Class;
            typeInfo.FullName = options.GetTypeFullName(typeof(TCollection));
        }
    }
}
