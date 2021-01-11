# High Performance Binary Serialization Libraries

[中文](README.zh.md) | [Quick Start](doc/GettingStart.md) | [Support Types](doc/SupportTypes.md)

Xfrogcn BinaryFormatter is a high performance binary serialization libraries in .NET, it through the  Span and Emit to achieve high performance, BinarySerializer uses an API interface that is consistent with System.Text.JSON, so easy to use.

## Features

- [x] High performance
- [x] Smaller size
- [x] Simple and easy to use
- [x] Keep instance refer
- [x] Supports deserialization to a different type
- [x] Support the type of dynamic loading assemblies
- [x] Complete built-in type support
- [ ] Extended attributes
- [ ] Binary document model

## Performance

Compare with the .NET built-in binaryFormatter serializer (Runtime.Serialization.Formatters.Binary), this library performance can be more than 4 times, and the size of the Serialization results only 75% of it.
The following by ` test/BinaryFormatter Benchmark ` performance test project to obtain performance data, including:

- Json refers to the System.Text.Json, you can see the performance really tough
- XfrogcnBinary refers to this library
- SystemBinaryFormatter refers to .NET built-in Binary Serialization library (System.Runtime.Serialization.Formatters.Binary.BinaryFormatter)
- category serialization Stream for adopting fluidization way
- category Bytes for direct serialization to Byte array or deserialized from a Byte array
- all of the tests are based on the default configuration, (in fluidization mode the default buffer size will significantly affect the serialization performance)

### Serializes

![img](doc/s.png)

``` ini
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.1237 (1909/November2018Update/19H2)
Intel Core i7-7500U CPU 2.70GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=5.0.101
  [Host]     : .NET Core 3.1.10 (CoreCLR 4.700.20.51601, CoreFX 4.700.20.51901), X64 RyuJIT
  DefaultJob : .NET Core 3.1.10 (CoreCLR 4.700.20.51601, CoreFX 4.700.20.51901), X64 RyuJIT


```

|                Method | Categories |      Mean |    Error |    StdDev |
|---------------------- |----------- |----------:|---------:|----------:|
|                  Json |     Stream |  61.41 μs | 1.212 μs |  2.154 μs |
|         `XfrogcnBinary` |     Stream |  92.97 μs | 1.691 μs |  2.425 μs |
| SystemBinaryFormatter |     Stream | 291.37 μs | 5.729 μs | 11.174 μs |
|            Json_Bytes |      Bytes |  59.79 μs | 1.160 μs |  1.907 μs |
|   `XfrogcnBinary_Bytes` |      Bytes |  88.67 μs | 1.437 μs |  1.274 μs |

### Deserializes

![img](doc/ds.png)

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.1237 (1909/November2018Update/19H2)
Intel Core i7-7500U CPU 2.70GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=5.0.101
  [Host]     : .NET Core 3.1.10 (CoreCLR 4.700.20.51601, CoreFX 4.700.20.51901), X64 RyuJIT
  DefaultJob : .NET Core 3.1.10 (CoreCLR 4.700.20.51601, CoreFX 4.700.20.51901), X64 RyuJIT


```

|                Method |      Mean |    Error |   StdDev |
|---------------------- |----------:|---------:|---------:|
|                  Json | 100.12 μs | 1.933 μs | 2.374 μs |
|         `XfrogcnBinary` |  96.34 μs | 1.631 μs | 1.362 μs |
| SystemBinaryFormatter | 334.68 μs | 2.319 μs | 1.936 μs |
|            Json_Bytes |  80.13 μs | 1.572 μs | 1.989 μs |
|   `XfrogcnBinary_Bytes` |  92.14 μs | 1.814 μs | 3.623 μs |

## Solve the problem of dynamic loading assemblies

If the serialization type in dynamically loaded assembly, encapsulating and serialization methods in the dynamic loading assemblies, .NET official BinaryFormatter serialization libraries cannot deserialize, will trigger a assembly errors cannot be found.  

This serialization libraries have solve this problem
