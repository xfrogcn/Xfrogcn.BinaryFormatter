using System;

namespace Xfrogcn.BinaryFormatter
{
    public class BinaryMemberInfo
    {
        //public bool IsField { get; set; }

        public ushort Seq { get; internal set; }

        public byte[] Name { get; set; }

        public ushort TypeSeq { get; set; }

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
                ushort nameLen = (ushort)(Name == null ? 0 : Name.Length);
                int len = 2 + 2 + nameLen + 2;
               
                byte[] data = new byte[len];

                int position = 0;
                BitConverter.GetBytes(Seq).CopyTo(data, position);
                position += 2;
                BitConverter.GetBytes(nameLen).CopyTo(data, position);
                position += 2;
                if (nameLen > 0)
                {
                    Name.CopyTo(data, position);
                    position += nameLen;
                }
                BitConverter.GetBytes(TypeSeq).CopyTo(data, position);
                position += 2;

                _bytes = data;
            }
            return _bytes;


        }
    }
}
