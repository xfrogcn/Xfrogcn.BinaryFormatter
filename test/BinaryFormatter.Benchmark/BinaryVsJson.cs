using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BF = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter;

namespace Xfrogcn.BinaryFormatter.Benchmark
{
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public class BinaryVsJson
    {
        internal TestApiResponse<TestApiRecordItem> data;
        private const int ListCount = 15;
        private readonly BF _formatter;
        
        public BinaryVsJson()
        {
            data = new TestApiResponse<TestApiRecordItem>
            {
                Message = "SUCCESS",
                TotalCount = 3000,
                Items = new List<TestApiRecordItem>()
            };

            for (int i = 0; i < ListCount; i++)
            {
                data.Items.Add(CreateItem());
            }

            _formatter = new BF();
        }

        private TestApiRecordItem CreateItem()
        {
            Random r = new Random();
            TestApiRecordItem item = new TestApiRecordItem()
            {
                CreatedTime = DateTime.Now,
                Desc = new string('A', 256),
                ProductName = new string('B', 50),
                Discount = r.Next(0, 100) / 100F,
                Price = r.Next(100, 50000),
                Address  = new string('C',500),
                Id = Guid.NewGuid(),
                Category = new CategoryItem()
                {
                    Id = "1",
                    Desc = "Desc",
                    Name = "Name"
                },
                Status = ProductStatus.Normal,
                Stock  =r.Next(0,1000),
                Properties = new Dictionary<string, string>()
                {
                    { "P1", "Test property P1" },
                    { "P2", "Test property P2" },
                }
            };
            return item;
        }

        [BenchmarkCategory("Stream")]
        [Benchmark]
        public async Task Json()
        {
            MemoryStream ms = new MemoryStream();
            await System.Text.Json.JsonSerializer.SerializeAsync(ms, data);
        }

        [BenchmarkCategory("Stream")]
        [Benchmark]
        public async Task XfrogcnBinary()
        {
            MemoryStream ms = new MemoryStream();
            await Xfrogcn.BinaryFormatter.BinarySerializer.SerializeAsync(ms, data);
        }

        [BenchmarkCategory("Stream")]
        [Benchmark]
        public void SystemBinaryFormatter()
        {
            MemoryStream ms = new MemoryStream();
            _formatter.Serialize(ms, data);
        }

        //-----------
        [BenchmarkCategory("Bytes")]
        [Benchmark]
        public void Json_Bytes()
        {
            _ = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(data);
        }

        [BenchmarkCategory("Bytes")]
        [Benchmark]
        public void XfrogcnBinary_Bytes()
        {
            _ = Xfrogcn.BinaryFormatter.BinarySerializer.Serialize(data);
        }


        public void SizeTest()
        {
            var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(data);
            Console.WriteLine($"JSON: {bytes.Length}");

            bytes = Xfrogcn.BinaryFormatter.BinarySerializer.Serialize(data);
            Console.WriteLine($"XfrogcnBinary: {bytes.Length}");

            MemoryStream ms = new MemoryStream();
            BF formatter = new BF();
            formatter.Serialize(ms, data);
            Console.WriteLine($"SystemBinaryFormatter: {ms.Length}");
        }
    }
}
