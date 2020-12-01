using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    internal sealed class BinaryPropertyInfo<T> : BinaryPropertyInfo
    {
        public override bool GetMemberAndWriteJson(object obj, ref WriteStack state, Utf8JsonWriter writer)
        {
            throw new NotImplementedException();
        }

        public override bool GetMemberAndWriteJsonExtensionData(object obj, ref WriteStack state, Utf8JsonWriter writer)
        {
            throw new NotImplementedException();
        }

        public override object GetValueAsObject(object obj)
        {
            throw new NotImplementedException();
        }

        public override bool ReadJsonAndSetMember(object obj, ref ReadStack state, ref Utf8JsonReader reader)
        {
            throw new NotImplementedException();
        }

        public override bool ReadJsonAsObject(ref ReadStack state, ref Utf8JsonReader reader, out object value)
        {
            throw new NotImplementedException();
        }

        public override void SetExtensionDictionaryAsObject(object obj, object extensionDict)
        {
            throw new NotImplementedException();
        }
    }
}
