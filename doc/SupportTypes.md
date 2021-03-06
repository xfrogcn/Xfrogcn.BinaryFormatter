# Support types

## build-in types

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

## Collection types

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

## Dictionay types

- NameValueCollection
- System.Collections.Immutable.*
- IDictionary&lt;TKey, TValue&gt;
- IDictionary
- ConcurrentDictionary&lt;TKey, TValue&gt;
- IEnumerable&lt;KeyValuePair&lt;TKey, TValue&gt;&gt;

## class

A class that has a common argumentless constructor or specifies the constructor (the parameter name needs to be matched) through the BinaryConstructor Attribute attribute。
