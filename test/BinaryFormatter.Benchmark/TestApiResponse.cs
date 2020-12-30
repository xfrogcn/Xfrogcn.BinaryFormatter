using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Benchmark
{
    [Serializable]
    public class TestApiResponse<TItem>
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public int TotalCount { get; set; }

        public List<TItem> Items { get; set; }
    }


}
