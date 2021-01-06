# 支持的类型

## 内置类型

- Boolean
- Byte
- byte[]
- char
- short
- int
- long
- ushort
- uint
- ulong
- float
- string
- DateTime
- DateTimeOffset
- TimeSpan
- Double
- Decimal
- BigInteger
- Complex
- Guid
- DBNull
- Uri
- Vector2
- Vector3
- Vector4
- Vector&lt;T&gt;
- Matrix3x2
- Matrix4x4
- Plane
- Quaternion
- TimeZoneInfo
- Enum
- Nullable&lt;T&gt;
- Tuple<...>
- ValueTuple<...>

## 集合

支持以下类型或实现了该接口的类型：

- Array
- IList
- IList&lt;T&gt;
- Stack&lt;T&gt;
- Queue&lt;T&gt;
- ISet&lt;T&gt;
- System.Collections.Immutable.*
- ICollection&lt;T&gt;
- IEnumerable (需Add方法)
- ConcurrentStack&lt;T&gt;
- ConcurrentQueue&lt;T&gt;
- ConcurrentBag&lt;T&gt;

## 字典

- NameValueCollection
- System.Collections.Immutable.*
- IDictionary&lt;TKey, TValue&gt;
- IDictionary
- IEnumerable&lt;KeyValuePair&lt;TKey, TValue&gt;&gt;

## 类

具有公共无参数构造函数或通过BinaryConstructorAttribute特性指定构造函数（参数名称需与）的类。