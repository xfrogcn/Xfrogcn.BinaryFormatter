using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters.Value
{
    internal sealed class BooleanConverter : BinaryConverter<bool>
    {
        public override void Write(BinaryWriter writer, bool value, BinarySerializerOptions options)
        {
            throw new NotImplementedException();
        }

        internal override object ReadCoreAsObject(ref BinaryReader reader, BinarySerializerOptions options, ref ReadStack state)
        {
            throw new NotImplementedException();
        }

        internal override bool TryReadAsObject(ref BinaryReader reader, BinarySerializerOptions options, ref ReadStack state, out object value)
        {
            throw new NotImplementedException();
        }

        internal override bool WriteCoreAsObject(BinaryWriter writer, object value, BinarySerializerOptions options, ref WriteStack state)
        {
            throw new NotImplementedException();
        }
    }
}
