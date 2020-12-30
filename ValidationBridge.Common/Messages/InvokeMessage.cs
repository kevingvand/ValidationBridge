using System;
using System.Collections.Generic;
using System.Linq;
using ValidationBridge.Common.Enumerations;

namespace ValidationBridge.Common.Messages
{
    public class InvokeMessage : PipeMessage
    {
        public override EMessageType MessageType => EMessageType.INVOKE;

        public string MethodName { get; set; }
        public Argument[] Arguments { get; set; }

        public InvokeMessage(string methodName)
            : base()
        {
            this.MethodName = methodName;
        }

        public InvokeMessage(string methodName, params Argument[] arguments)
            : this(methodName)
        {
            this.Arguments = arguments;
        }

        public InvokeMessage(Guid messageId, string methodName, params Argument[] arguments)
            : this(methodName, arguments)
        {
            this.MessageId = messageId;
        }

        public override byte[] GetBytes()
        {
            var messageHeader = base.GetBytes();

            var methodNameBytes = Constants.ServerEncoding.GetBytes(MethodName);

            var methodNameArray = new byte[methodNameBytes.Length + 2];
            methodNameArray[0] = (byte)(methodNameBytes.Length / 256);
            methodNameArray[1] = (byte)(methodNameBytes.Length & 255);
            methodNameBytes.CopyTo(methodNameArray, 2);

            var argumentLength = Arguments?.Length ?? 0;
            byte[] argumentArray = new byte[0];

            if (argumentLength > 0)
            {
                List<byte[]> argumentArrays = new List<byte[]>();
                foreach (var argument in Arguments)
                    argumentArrays.Add(argument.GetBytes());

                argumentArray = argumentArrays.SelectMany(array => array).ToArray();
            }

            var result = new byte[messageHeader.Length + methodNameArray.Length + 2 + argumentArray.Length];
            messageHeader.CopyTo(result, 0);
            methodNameArray.CopyTo(result, messageHeader.Length);

            result[messageHeader.Length + methodNameArray.Length] = (byte)(argumentLength / 256);
            result[messageHeader.Length + methodNameArray.Length + 1] = (byte)(argumentLength & 255);

            if (argumentLength > 0)
                argumentArray.CopyTo(result, messageHeader.Length + methodNameArray.Length + 2);

            return result;
        }

        public static InvokeMessage CreateFromBytes(byte[] bytes)
        {
            var messageType = (EMessageType)bytes[0];
            if (messageType != EMessageType.INVOKE) throw new Exception($"Specified message is not of type {EMessageType.INVOKE}, but of type {messageType}");

            var messageIdBytes = bytes.Skip(1).Take(16).ToArray();
            var messageId = new Guid(messageIdBytes);

            var methodNameLength = bytes[messageIdBytes.Length + 1] * 256 + bytes[messageIdBytes.Length + 2];
            var methodNameBytes = bytes.Skip(messageIdBytes.Length + 3).Take(methodNameLength).ToArray();
            var methodName = Constants.ServerEncoding.GetString(methodNameBytes);

            var argumentIndex = 3 + messageIdBytes.Length + methodNameBytes.Length;
            var argumentCount = bytes[argumentIndex] * 256 + bytes[argumentIndex + 1];
            Argument[] arguments = new Argument[argumentCount];
            argumentIndex += 2;
            for (int i = 0; i < argumentCount; i++)
            {
                var argumentLength = bytes[argumentIndex + 1] * 256 + bytes[argumentIndex + 2] + 3;
                arguments[i] = Argument.CreateFromBytes(bytes.Skip(argumentIndex).Take(argumentLength).ToArray());
                argumentIndex += argumentLength;
            }

            return new InvokeMessage(messageId, methodName, arguments);
        }
    }
}
