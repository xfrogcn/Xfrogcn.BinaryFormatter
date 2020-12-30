using System;
using System.Collections.Generic;

namespace Xfrogcn.BinaryFormatter.Benchmark
{
    [Serializable]
    public class TestApiRecordItem
    {
        public Guid Id { get; set; }

        public DateTime CreatedTime { get; set; }
        
        public DateTime? UpdateTime { get; set; }

        public string ProductName { get; set; }

        public string Desc { get; set; }

        public decimal Price { get; set; }

        public float Discount { get; set; }

        public int Stock { get; set; }

        public string Address { get; set; }

        public CategoryItem Category { get; set; }

        public Dictionary<string, string> Properties { get; set; }

        public ProductStatus Status { get; set; }
    }

    [Serializable]
    public class CategoryItem
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Desc { get; set; }
    }

    public enum ProductStatus
    {
        Normal,
        Invalid
    }
}
