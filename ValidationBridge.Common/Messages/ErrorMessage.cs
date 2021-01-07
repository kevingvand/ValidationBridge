using System;
using System.Linq;
using ValidationBridge.Common.Enumerations;

namespace ValidationBridge.Common.Messages
{
    public class ErrorMessage : PipeMessage
    {
        public override EMessageType MessageType => EMessageType.ERROR;

        public string Message { get; }

        public ErrorMessage(string message)
            : base()
        {
            Message = message;
        }

        public ErrorMessage(Guid messageId, string message)
            : this(message)
        {
            this.MessageId = messageId;
        }

        public override byte[] GetBytes()
        {
            var messageHeader = base.GetBytes();

            var messageBytes = Constants.ServerEncoding.GetBytes(Message);

            var result = new byte[messageHeader.Length + messageBytes.Length];
            messageHeader.CopyTo(result, 0);
            messageBytes.CopyTo(result, messageHeader.Length);

            return result;
        }

        public static ErrorMessage CreateFromBytes(byte[] bytes)
        {
            var messageType = (EMessageType)bytes[0];
            if (messageType != EMessageType.ERROR) throw new Exception($"Specified message is not of type {EMessageType.ERROR}, but of type {messageType}");

            var messageIdBytes = bytes.Skip(1).Take(16).ToArray();
            var messageId = new Guid(messageIdBytes);

            var message = Constants.ServerEncoding.GetString(bytes.Skip(1 + messageIdBytes.Length).ToArray());

            return new ErrorMessage(messageId, message);
        }

        public override string ToString()
        {
            return $"{MessageType} - {Message}";
        }
    }
}
