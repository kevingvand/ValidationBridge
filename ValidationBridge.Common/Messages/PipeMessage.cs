using System;
using ValidationBridge.Common.Enumerations;

namespace ValidationBridge.Common.Messages
{
    public abstract class PipeMessage
    {
        public abstract EMessageType MessageType { get; }
        public Guid MessageId { get; set; }

        public PipeMessage()
        {
            MessageId = Guid.NewGuid();
        }

        public virtual byte[] GetBytes()
        {
            byte[] header = new byte[17];
            header[0] = Convert.ToByte(MessageType);
            var idArray = MessageId.ToByteArray();
            idArray.CopyTo(header, 1);

            return header;
        }
    }
}
