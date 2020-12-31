namespace Xfrogcn.BinaryFormatter
{
    /// <summary>
    /// 内部类型
    /// </summary>
    public enum TypeEnum : byte
    {
        None = 0,
        Byte = 1,
        Int16 = 2,
        Int32 = 3,
        Int64 = 4,
        SByte = 5,
        UInt16 = 6,
        UInt32 = 7,
        UInt64 = 8,
        Single = 9,
        Double = 10,
        Decimal = 11,
        BigInteger = 12,
        Complex = 13,
        Vector2 = 14,
        Vector3 = 15,
        Vector4 = 16,
        VectorT = 17,
        Matrix3x2 = 18,
        Matrix4x4 = 19,
        Plane = 20,
        Quaternion = 21,
        IntPtr = 22,
        Guid = 23,
        ByteArray = 24,
        Enum = 25,

        DateTime = 30,
        DateTimeOffset = 31,
        TimeZoneInfo = 32,
        AdjustmentRule = 33,
        TransitionTime = 34,
        TimeSpan = 35,
        

        Boolean = 60,
        Char = 61,
        String = 62,
        Uri = 63,
        Version = 64,
        DBNull = 65,

        Struct = 80,
        Nullable = 81,

        ValueTuple = 90,
       // ValueTupleT = 91,
        
        
        Tuple = 92,
       // TupleT = 93,

        Array = 100,
        List = 101,
        KeyValuePair = 102,
        Dictionary = 103,
        NameValueCollection = 104,

        Object = 126,
        Class = 127
    }

}
