using System;
using System.Text;
using System.Linq;

namespace Xfrogcn.BinaryFormatter
{
    public class BinaryTypeInfo
    {
        internal ushort Seq { get;  set; }

        internal ClassType SerializeType { get; set; }

        internal bool IsGeneric { get; set; }

        internal byte GenericArgumentCount { get; set; }

        public TypeEnum Type { get; set; }
        public string FullName { get; set; }

        internal ushort[] GenericArguments { get; set; }

        internal BinaryMemberInfo[] MemberInfos { get; set; }


        private byte[] _bytes = null;
        private object _locker = new object();

        public byte[] GetBytes()
        {
            if (_bytes != null)
            {
                return _bytes;
            }

            lock (_locker)
            {
                if (_bytes != null)
                {
                    return _bytes;
                }
                int len = 2 + 1 + 1 + 1 + 1;
                if (GenericArguments != null && GenericArguments.Length > 0)
                {
                    len += (GenericArguments.Length * 2);
                }

                len += 2;
                byte[] fullNameBytes = null;
                if (!string.IsNullOrEmpty(FullName))
                {
                    fullNameBytes = Encoding.UTF8.GetBytes(FullName);
                    len += fullNameBytes.Length;
                }

                byte[] data = new byte[len];

                int position = 0;
                BitConverter.GetBytes(Seq).CopyTo(data, position);
                position += 2;

                data[position++] = (byte)SerializeType;
                data[position++] = IsGeneric ? 1 : 0;
                data[position++] = GenericArgumentCount;

                if (GenericArguments != null && GenericArguments.Length > 0)
                {
                    foreach (ushort gaSeq in GenericArguments)
                    {
                        BitConverter.GetBytes(gaSeq).CopyTo(data, position);
                        position += 2;
                    }
                }
                data[position++] = (byte)Type;
                if (fullNameBytes == null)
                {
                    BitConverter.GetBytes((ushort)0).CopyTo(data, position);
                }
                else
                {
                    BitConverter.GetBytes((ushort)fullNameBytes.Length).CopyTo(data, position);
                    position += 2;
                    fullNameBytes.CopyTo(data, position);
                }

                _bytes = data;
            }
            return _bytes;

        }

    }
}
