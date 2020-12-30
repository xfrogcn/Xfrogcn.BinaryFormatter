using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BF = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter;

namespace Xfrogcn.BinaryFormatter.Benchmark
{
    public class DeserializeBenchmark
    {
        internal TestApiResponse<TestApiRecordItem> data;
        private const int ListCount = 15;
        private byte[] _jsonBytes;
        private byte[] _binaryBytes;
        private Stream _jsonStream;
        private Stream _binaryStream;
        private Stream _sbStream;

        public DeserializeBenchmark()
        {
            data = new TestApiResponse<TestApiRecordItem>();
            data.Message = "SUCCESS";
            data.TotalCount = 3000;
            data.Items = new List<TestApiRecordItem>();

            for (int i = 0; i < ListCount; i++)
            {
                data.Items.Add(createItem());
            }

            _jsonBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(data);
            _binaryBytes = Xfrogcn.BinaryFormatter.BinarySerializer.Serialize(data);

            _jsonStream = new MemoryStream();
            System.Text.Json.JsonSerializer.SerializeAsync(_jsonStream, data).Wait();
            _jsonStream.Position = 0;
            _binaryStream = new MemoryStream();
            Xfrogcn.BinaryFormatter.BinarySerializer.SerializeAsync(_binaryStream, data).Wait();
            _binaryStream.Position = 0;

            _sbStream = new MemoryStream();
            BF formatter = new BF();
            formatter.Serialize(_sbStream, data);
            _sbStream.Position = 0;
        }

        private TestApiRecordItem createItem()
        {
            Random r = new Random();
            TestApiRecordItem item = new TestApiRecordItem()
            {
                CreatedTime = DateTime.Now,
                Desc = new string('A', 256),
                ProductName = new string('B', 50),
                Discount = r.Next(0, 100) / 100F,
                Price = r.Next(100, 50000),
                Address = new string('C', 500),
                Id = Guid.NewGuid(),
                Category = new CategoryItem()
                {
                    Id = "1",
                    Desc = "Desc",
                    Name = "Name"
                },
                Status = ProductStatus.Normal,
                Stock = r.Next(0, 1000),
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
            await System.Text.Json.JsonSerializer.DeserializeAsync<TestApiResponse<TestApiRecordItem>>(_jsonStream);
        }

        [BenchmarkCategory("Stream")]
        [Benchmark]
        public async Task XfrogcnBinary()
        {
            await Xfrogcn.BinaryFormatter.BinarySerializer.DeserializeAsync(_binaryStream);
        }

        [BenchmarkCategory("Stream")]
        [Benchmark]
        public void SystemBinaryFormatter()
        {
            BF formatter = new BF();
            formatter.Deserialize(_sbStream);
        }



        [BenchmarkCategory("Bytes")]
        [Benchmark]
        public void Json_Bytes()
        {
            System.Text.Json.JsonSerializer.Deserialize<TestApiResponse<TestApiRecordItem>>(_jsonBytes);
        }

        [BenchmarkCategory("Bytes")]
        [Benchmark]
        public void XfrogcnBinary_Bytes()
        {
            Xfrogcn.BinaryFormatter.BinarySerializer.Deserialize(_binaryBytes);
        }
    }
}
