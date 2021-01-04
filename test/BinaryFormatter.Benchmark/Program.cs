using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace Xfrogcn.BinaryFormatter.Benchmark
{
    class Program
    {
        static void Main()
        {
            BinaryVsJson vs = new BinaryVsJson();
            vs.SizeTest();
            _ = BenchmarkRunner.Run<BinaryVsJson>();
            _ = BenchmarkRunner.Run<DeserializeBenchmark>();
        }
    }
}
