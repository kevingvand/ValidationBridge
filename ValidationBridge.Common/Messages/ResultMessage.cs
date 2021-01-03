using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ValidationBridge.Common.Enumerations;

namespace ValidationBridge.Common.Messages
{

    public class ResultMessage : PipeMessage
    {
        public override EMessageType MessageType => EMessageType.RESULT;

        public Argument Result { get; set; }

        public ResultMessage(Argument result)
            :base()
        {
            Result = result;
        }
        
        public ResultMessage(Guid messageId, Argument result)
            :this(result)
        {
            this.MessageId = messageId;
        }

        public override byte[] GetBytes()
        {
            var messageHeader = base.GetBytes();

            var argumentArray = Result.GetBytes();

            var result = new byte[messageHeader.Length + argumentArray.Length];
            messageHeader.CopyTo(result, 0);
            argumentArray.CopyTo(result, messageHeader.Length);

            return result;
        }

        public static ResultMessage CreateFromBytes(byte[] bytes)
        {
            var messageType = (EMessageType)bytes[0];
            if (messageType != EMessageType.RESULT) throw new Exception($"Specified message is not of type {EMessageType.RESULT}, but of type {messageType}");

            var messageIdBytes = bytes.Skip(1).Take(16).ToArray();
            var messageId = new Guid(messageIdBytes);

            var argument = Argument.CreateFromBytes(bytes.Skip(1 + messageIdBytes.Length).ToArray());

            return new ResultMessage(messageId, argument);
        }

        public override string ToString()
        {
            return $"{MessageType} - {Result}";
        }
    }
}
