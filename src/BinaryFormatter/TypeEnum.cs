namespace Xfrogcn.BinaryFormatter
{
    /// <summary>
    /// 内部类型
    /// </summary>
    public enum TypeEnum : sbyte
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
        DateTime = 30,
        DateTimeOffset = 31,
        TimeZoneInfo = 32,
        TimeSpan = 33,
        Boolean = 34,
        Char = 35,
        ValueTuple = 36,
        ValueTupleT = 37,
        Struct = 40,
        DBNull = 41,
        IntPtr = 42,
        Nullable = 45,
        String = 50,
        Uri = 51,
        Tuple = 60,
        Version = 61,
        Array = 100,
        List = 101,
        Dictionary = 102,
        Object = 126,
        Class = 127
    }

}
