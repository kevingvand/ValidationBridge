using System;
using System.Linq;
using ValidationBridge.Bridge.Adapters.Python.Enumerations;
using ValidationBridge.Common.Enumerations;

namespace ValidationBridge.Bridge.Adapters.Python.Messages
{
    public class PythonResultMessage : BaseMessage
    {
        public EType Type { get; set; }
        public byte[] Value { get; set; }

        public PythonResultMessage(EType type, byte[] value)
        {
            MessageType = EPythonMessageType.RESULT;
            Type = type;
            Value = value;
        }

        public override byte[] GetBytes()
        {
            byte[] result = new byte[Value.Length + 4];
            result[0] = (byte)MessageType;
            result[1] = (byte)Type;
            var lengthBytes = BitConverter.GetBytes(Value.Length);
            lengthBytes.CopyTo(result, 2);
            Value.CopyTo(result, 6);

            return result;
        }

        public dynamic GetValue()
        {
            if (Type.HasFlag(EType.ARRAY))
            {
                var elementType = (Type & ~EType.ARRAY);
                var arrayLength = BitConverter.ToInt32(Value.Take(4).ToArray(), 0);
                var result = Array.CreateInstance(elementType.GetSystemType(), arrayLength);

                var currentIndex = 4;
                for (var i = 0; i < arrayLength; i++)
                {
                    var elementLength = BitConverter.ToInt32(Value.Skip(currentIndex).Take(4).ToArray(), 0);
                    var valueBytes = Value.Skip(currentIndex + 4).Take(elementLength).ToArray();
                    result.SetValue(elementType.GetValue(valueBytes), i);

                    currentIndex += elementLength + 4;
                }

                return result;
            }

            return Type.GetValue(Value);
        }
    }
}
