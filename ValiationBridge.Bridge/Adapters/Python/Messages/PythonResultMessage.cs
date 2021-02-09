﻿using System;
using System.Linq;
using ValiationBridge.Bridge.Adapters.Python.Enumerations;
using ValidationBridge.Common.Enumerations;

namespace ValiationBridge.Bridge.Adapters.Python.Messages
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
            result[2] = (byte)(Value.Length / 256);
            result[3] = (byte)(Value.Length & 255);
            Value.CopyTo(result, 4);

            return result;
        }

        public dynamic GetValue()
        {
            if (Type.HasFlag(EType.ARRAY))
            {
                var elementType = (Type & ~EType.ARRAY);
                var arrayLength = Value[0] * 256 + Value[1];
                var result = Array.CreateInstance(elementType.GetSystemType(), arrayLength);

                var currentIndex = 2;
                for (var i = 0; i < arrayLength; i++)
                {
                    var elementLength = Value[currentIndex] * 256 + Value[currentIndex + 1];
                    var valueBytes = Value.Skip(currentIndex + 2).Take(elementLength).ToArray();
                    result.SetValue(elementType.GetValue(valueBytes), i);

                    currentIndex += elementLength + 2;
                }

                return result;
            }

            return Type.GetValue(Value);
        }
    }
}
