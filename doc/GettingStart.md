# 快速开始

## 序列化

序列化到流：

```c#
    MemoryStream ms = new MemoryStream();
    await Xfrogcn.BinaryFormatter.BinarySerializer.SerializeAsync(ms, data);
```

序列化到byte数组：

```c#
    var data = Xfrogcn.BinaryFormatter.BinarySerializer.Serialize(data);
```

## 反序列化

从流中反序列化：

```c#
    var obj = await Xfrogcn.BinaryFormatter.BinarySerializer.DeserializeAsync(stream);
```

从byte数组反序列化：

```c#
    var obj = Xfrogcn.BinaryFormatter.BinarySerializer.Deserialize(data);
```

反序列化为指定类型：

```c#
    var obj = await Xfrogcn.BinaryFormatter.BinarySerializer.DeserializeAsync<T>(stream);
    或者：
    var obj = Xfrogcn.BinaryFormatter.BinarySerializer.Deserialize<T>(data);
```

## 序列化控制

BinarySerializer通过特性来控制序列化，与JSON序列化类似，有以下可供使用的特性：

- BinaryConstructorAttribute：指定反序列化所使用的构造函数，根据惯例，构造函数参数的名称应该与对应的属性名称一样（不区分大小写）

- BinaryConverterAttribute: 为类型/结构/枚举/属性/字段指定特定的序列化转换器

- BinaryExtensionDataAttribute: 指定用于存储扩展属性的字段（预留，功能尚未实现）

- BinaryIgnoreAttribute：指定需要忽略的字段或属性
  
- BinaryIncludeAttribute：指定需要包含的字段或属性

- BinaryPropertyNameAttribute：指定序列化时的属性名称（预留，用于未来的二进制文档功能）

## 配置

在调用序列化与反序列化方法时，可以为各个方法指定序列化配置，如果为指定配置，将使用默认配置。

`注意:由于类型解析、序列化转换器等缓存都是以配置实例为基础，即每一个配置实例的缓存是独立的，故请使用共享的配置实例，请勿为每一次序列化分配新的配置实例`

|                属性 | 说明 |      默认值 |
|---------------------- |----------- |----------:|---------:|----------:|
|   MaxDepth |     指定序列化时限制的最大深度 |  64 |
|   DefaultBufferSize |     指定流模式下所使用的默认缓冲区大小 |  16KB |
|   IgnoreReadOnlyProperties |     指定序列化时是否忽略只读属性 | false |
|   IgnoreReadOnlyFields |      指定序列化时是否忽略只读字段 |  false |
|   DefaultIgnoreCondition |      默认的忽略条件 |  Never |
|   IgnoreNullValues |      是否忽略空值 |  false |
|   PropertyNameCaseInsensitive |      属性名称是否区分大小写（预留） |  false |
|   IncludeFields |      是否包含字段 |  false |
|   Converters |      添加自定义的序列化转换器 |
