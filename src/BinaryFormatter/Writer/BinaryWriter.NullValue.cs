namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteNullValue()
        {
            WriteBytes(new byte[] { 0 });
        }
    }
}
