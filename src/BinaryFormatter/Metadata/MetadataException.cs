using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Metadata
{
    public class MetadataException : Exception
    {
        public Type Type { get; }

        public MetadataException(Type type, string message): base(message)
        {
            Type = type;
        }
    }
}
