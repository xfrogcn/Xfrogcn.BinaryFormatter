# Getting Started

## Serializes

Using stream：

```c#
    MemoryStream ms = new MemoryStream();
    await Xfrogcn.BinaryFormatter.BinarySerializer.SerializeAsync(ms, data);
```

Using byte array：

```c#
    var data = Xfrogcn.BinaryFormatter.BinarySerializer.Serialize(data);
```

## Deserializes

from stream：

```c#
    var obj = await Xfrogcn.BinaryFormatter.BinarySerializer.DeserializeAsync(stream);
```

from byte array：

```c#
    var obj = Xfrogcn.BinaryFormatter.BinarySerializer.Deserialize(data);
```

deserializes to the specified type：

```c#
    var obj = await Xfrogcn.BinaryFormatter.BinarySerializer.DeserializeAsync<T>(stream);
    或者：
    var obj = Xfrogcn.BinaryFormatter.BinarySerializer.Deserialize<T>(data);
```

## Attributes

BinarySerializer controls serialization through attribute, similar to JSON serialization, and has the following features available:

- BinaryConstructorAttribute：Specify the constructor used for deserialization, and by convention, the name of the constructor argument should be the same as the corresponding property name (case insensior)
- BinaryConverterAttribute: Specify a specific serialized converter for the class/struct/enum/property/field
- BinaryExtensionDataAttribute: Specify the field to store the extended properties (reserved, feature not yet implemented)
- BinaryIgnoreAttribute：Specify the fields or properties that need to be ignored
- BinaryIncludeAttribute：Specify the fields or properties that need to be included
- BinaryPropertyNameAttribute：Specify the property name at serialization (reserved for future binary document functionality)

## Options

When you call serialization and deserialization methods, you can specify serialization configurations for each method, and if you specify configurations, the default configuration is used.

`Note: Because caches such as type resolution and serialization converters are based on configuration instances, i.e. the caches for each configuration instance are independent, use shared configuration instances, and do not assign new configuration instances for each serialization`

|                  属性 |        说明 |    默认值 |
|---------------------- |----------- |----------:|
|   MaxDepth |     Specifies the maximum depth limited when serialization |  64 |
|   DefaultBufferSize |     Specifies the default buffer size used in stream mode |  16KB |
|   IgnoreReadOnlyProperties |     Specify whether read-only properties are ignored when serializing | false |
|   IgnoreReadOnlyFields |      Specify whether read-only fields are ignored when serialization is specified |  false |
|   DefaultIgnoreCondition |      The default ignore condition |  Never |
|   IgnoreNullValues |      Whether to ignore the null value |  false |
|   PropertyNameCaseInsensitive |      Whether the property name is case sensitive (reserved) |  false |
|   IncludeFields |      Whether to include fields |  false |
|   Converters |      Add a custom serialized converter |
