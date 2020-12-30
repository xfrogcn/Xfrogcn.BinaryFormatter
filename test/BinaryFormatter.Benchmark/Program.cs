using BenchmarkDotNet.Running;

namespace Xfrogcn.BinaryFormatter.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BinaryVsJson vs = new BinaryVsJson();
            vs.SizeTest();
            
            var summary = BenchmarkRunner.Run<BinaryVsJson>();
        }
    }
}
