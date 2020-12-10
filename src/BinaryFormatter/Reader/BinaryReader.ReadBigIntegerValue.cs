using System.Numerics;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        public BigInteger GetBigInteger()
        {
            return new BigInteger(ValueSpan);
        }
    }
}
