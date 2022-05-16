using System;
using System.Collections.Generic;
using System.Linq;
using ValidationBridge.Common.Enumerations;

namespace ValidationBridge.Common.Messages
{
    public class InvokeMessage : PipeMessage
    {
        public override EMessageType MessageType => EMessageType.INVOKE;

        public Guid InstanceId { get; set; }
        public string MethodName { get; set; }
        public Argument[] Arguments { get; set; }

        public InvokeMessage(string methodName)
            : base()
        {
            this.InstanceId = Guid.Empty;
            this.MethodName = methodName;
        }

        public InvokeMessage(string methodName, params Argument[] arguments)
            : this(methodName)
        {
            this.Arguments = arguments;
        }

        public InvokeMessage(Guid instanceId, string methodName)
            : this(methodName)
        {
            this.InstanceId = instanceId;
        }

        public InvokeMessage(Guid instanceId, string methodName, params Argument[] arguments)
            : this(instanceId, methodName)
        {
            this.Arguments = arguments;
        }

        public InvokeMessage(Guid messageId, Guid instanceId, string methodName, params Argument[] arguments)
            : this(instanceId, methodName, arguments)
        {
            this.MessageId = messageId;
        }

        public override byte[] GetBytes()
        {
            var messageHeader = base.GetBytes();

            var instanceIdArray = InstanceId == null ? new byte[16] : InstanceId.ToByteArray();

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

            var result = new byte[messageHeader.Length + instanceIdArray.Length + methodNameArray.Length + 2 + argumentArray.Length];
            messageHeader.CopyTo(result, 0);
            instanceIdArray.CopyTo(result, messageHeader.Length);
            methodNameArray.CopyTo(result, messageHeader.Length + instanceIdArray.Length);

            result[messageHeader.Length + instanceIdArray.Length + methodNameArray.Length] = (byte)(argumentLength / 256);
            result[messageHeader.Length + instanceIdArray.Length + methodNameArray.Length + 1] = (byte)(argumentLength & 255);

            if (argumentLength > 0)
                argumentArray.CopyTo(result, messageHeader.Length + instanceIdArray.Length + methodNameArray.Length + 2);

            return result;
        }

        public static InvokeMessage CreateFromBytes(byte[] bytes)
        {
            var messageType = (EMessageType)bytes[0];
            if (messageType != EMessageType.INVOKE) throw new Exception($"Specified message is not of type {EMessageType.INVOKE}, but of type {messageType}");

            var messageIdBytes = bytes.Skip(1).Take(16).ToArray();
            var messageId = new Guid(messageIdBytes);

            var instanceIdBytes = bytes.Skip(messageIdBytes.Length + 1).Take(16).ToArray();
            var instanceId = new Guid(instanceIdBytes);

            var methodNameLength = bytes[messageIdBytes.Length + instanceIdBytes.Length + 1] * 256 + bytes[messageIdBytes.Length + instanceIdBytes.Length + 2];
            var methodNameBytes = bytes.Skip(messageIdBytes.Length + instanceIdBytes.Length + 3).Take(methodNameLength).ToArray();
            var methodName = Constants.ServerEncoding.GetString(methodNameBytes);

            var argumentIndex = 3 + messageIdBytes.Length + instanceIdBytes.Length + methodNameBytes.Length;
            var argumentCount = bytes[argumentIndex] * 256 + bytes[argumentIndex + 1];
            Argument[] arguments = new Argument[argumentCount];
            argumentIndex += 2;
            for (int i = 0; i < argumentCount; i++)
            {
                var argumentLengthBytes = bytes.Skip(argumentIndex + 1).Take(4).ToArray();
                var argumentLength = BitConverter.ToInt32(argumentLengthBytes, 0) + 5;
                arguments[i] = Argument.CreateFromBytes(bytes.Skip(argumentIndex).Take(argumentLength).ToArray());
                argumentIndex += argumentLength;
            }

            return new InvokeMessage(messageId, instanceId, methodName, arguments);
        }
    }
}
