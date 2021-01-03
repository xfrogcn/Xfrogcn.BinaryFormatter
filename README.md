# 高性能的二进制序列化库

Xfrogcn.BinaryFormatter是一个.NET下的高性能二进制序列化库，它通过底层的Span以及Emit最大限度地提高性能，BinaryFormatter整体上采用了与System.Text.Json序列化一致的编程API接口，故简单易用，无需过多的学习成本。

## 性能对比

与.NET内置的System.Runtime.Serialization.Formatters.Binary.BinaryFormatter二进制序列化对比，性能最高可达到它的4倍以上，而序列化结果的大小仅只有它的75%。
以下为通过`test/BinaryFormatter.Benchmark`性能测试项目获取的性能数据，其中：
- Json指System.Text.Json，可以看到其性能的确强悍
- XfrogcnBinary指本库
- SystemBinaryFormatter指.NET内置二进制序列化库（System.Runtime.Serialization.Formatters.Binary.BinaryFormatter）
- 类别Stream为采用流化方式序列化
- 类别Bytes为直接序列化为Byte数组或从Byte数组反序列化
- 所有的测试都基于默认配置，（流化方式下默认的缓冲区大小将会明显影响序列化性能）

### 序列化
``` ini

BenchmarkDotNet=v0.12.1, OS=macOS Catalina 10.15.7 (19H114) [Darwin 19.6.0]
Intel Core i5-8257U CPU 1.40GHz (Coffee Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.101
  [Host]     : .NET Core 3.1.10 (CoreCLR 4.700.20.51601, CoreFX 4.700.20.51901), X64 RyuJIT
  DefaultJob : .NET Core 3.1.10 (CoreCLR 4.700.20.51601, CoreFX 4.700.20.51901), X64 RyuJIT


```
|                Method | Categories |      Mean |    Error |   StdDev |
|---------------------- |----------- |----------:|---------:|---------:|
|                  Json |     Stream |  55.30 μs | 0.314 μs | 0.279 μs |
|         XfrogcnBinary |     Stream |  84.13 μs | 0.574 μs | 0.509 μs |
| SystemBinaryFormatter |     Stream | 276.39 μs | 1.264 μs | 1.182 μs |
|                       |            |           |          |          |
|            Json_Bytes |      Bytes |  53.35 μs | 0.282 μs | 0.250 μs |
|   XfrogcnBinary_Bytes |      Bytes |  77.60 μs | 1.017 μs | 0.850 μs |

### 反序列化
``` ini

BenchmarkDotNet=v0.12.1, OS=macOS Catalina 10.15.7 (19H114) [Darwin 19.6.0]
Intel Core i5-8257U CPU 1.40GHz (Coffee Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.101
  [Host]     : .NET Core 3.1.10 (CoreCLR 4.700.20.51601, CoreFX 4.700.20.51901), X64 RyuJIT
  DefaultJob : .NET Core 3.1.10 (CoreCLR 4.700.20.51601, CoreFX 4.700.20.51901), X64 RyuJIT


```
|                Method |      Mean |    Error |   StdDev |
|---------------------- |----------:|---------:|---------:|
|                  Json | 451.94 μs | 3.204 μs | 2.997 μs |
|         XfrogcnBinary |  87.14 μs | 0.754 μs | 0.705 μs |
| SystemBinaryFormatter | 288.19 μs | 5.429 μs | 5.078 μs |
|            Json_Bytes | 416.15 μs | 2.442 μs | 2.284 μs |
|   XfrogcnBinary_Bytes |  82.74 μs | 0.332 μs | 0.311 μs |
