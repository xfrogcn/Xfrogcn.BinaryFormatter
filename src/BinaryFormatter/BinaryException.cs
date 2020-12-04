using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{

    [Serializable]
    public class BinaryException : Exception
    {
        internal string _message;


        public BinaryException(string message, string path, long? bytePosition, Exception innerException) : base(message, innerException)
        {
            _message = message;
            BytePosition = bytePosition;
            Path = path;
        }


        public BinaryException(string message, string path, long? bytePosition) : base(message)
        {
            _message = message;
            BytePosition = bytePosition;
            Path = path;
        }


        public BinaryException(string message, Exception innerException) : base(message, innerException)
        {
            _message = message;
        }

        public BinaryException(string message) : base(message)
        {
            _message = message;
        }

        public BinaryException() : base() { }


        protected BinaryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            BytePosition = (long?)info.GetValue("BytePosition", typeof(long?));
            Path = info.GetString("Path");
            SetMessage(info.GetString("ActualMessage"));
        }


        internal bool AppendPathInformation { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("BytePosition", BytePosition, typeof(long?));
            info.AddValue("Path", Path, typeof(string));
            info.AddValue("ActualMessage", Message, typeof(string));
        }


        public long? BytePosition { get; internal set; }

  
        public string Path { get; internal set; }

 
        public override string Message
        {
            get
            {
                return _message ?? base.Message;
            }
        }

        internal void SetMessage(string message)
        {
            _message = message;
        }
    }
}
