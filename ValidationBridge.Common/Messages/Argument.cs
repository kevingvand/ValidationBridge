using System;
using System.Collections.Generic;
using System.Linq;
using ValidationBridge.Common.Enumerations;

namespace ValidationBridge.Common.Messages
{
    public class Argument
    {
        public EType Type { get; set; }
        public object Value { get; set; }

        #region Constructors

        private Argument() { }

        private Argument(EType type, object value)
        {
            this.Type = type;
            this.Value = value;
        }

        public Argument(int value)
            : this(EType.INT, value) { }

        public Argument(int[] value)
            : this(EType.ARRAY | EType.INT, value) { }

        public Argument(char value)
            : this(EType.CHAR, value) { }

        public Argument(char[] value)
            : this(EType.ARRAY | EType.CHAR, value) { }

        public Argument(string value)
            : this(EType.STRING, value) { }

        public Argument(string[] value)
            : this(EType.ARRAY | EType.STRING, value) { }

        public Argument(double value)
            : this(EType.DOUBLE, value) { }

        public Argument(double[] value)
            : this(EType.ARRAY | EType.DOUBLE, value) { }

        public Argument(bool value)
            : this(EType.BOOL, value) { }

        public Argument(bool[] value)
            : this(EType.ARRAY | EType.BOOL, value) { }

        #endregion

        public byte[] GetBytes()
        {
            if (Type.HasFlag(EType.ARRAY))
            {
                var array = Value as Array;
                if (array == null) return null;

                List<byte[]> valueArrays = new List<byte[]>();

                var elementType = Type & ~EType.ARRAY;

                foreach (var element in array)
                    valueArrays.Add(GetValueArray(elementType, element, true));

                var values = valueArrays.SelectMany(valueArray => valueArray).ToArray();

                var result = new byte[values.Length + 3];
                result[0] = Convert.ToByte(Type);
                result[1] = (byte)(array.Length / 256);
                result[2] = (byte)(array.Length & 255);
                values.CopyTo(result, 3);

                return result;
            }
            else return GetValueArray(Type, Value);
        }

        private byte[] GetValueArray(EType type, dynamic value, bool isArrayElement = false)
        {
            var valueArray = type.HasFlag(EType.STRING) ? Constants.ServerEncoding.GetBytes(value) : BitConverter.GetBytes(value);
            var resultArray = new byte[valueArray.Length + (isArrayElement ? 2 : 3)];

            var blockLength = (byte)(valueArray.Length / 256);
            var byteLength = (byte)(valueArray.Length & 255);

            resultArray[0] = isArrayElement ? blockLength : Convert.ToByte(type);
            resultArray[1] = isArrayElement ? byteLength : blockLength;
            if (!isArrayElement) resultArray[2] = byteLength;

            valueArray.CopyTo(resultArray, isArrayElement ? 2 : 3);
            return resultArray;
        }

        public static object GetValue(EType type, byte[] bytes)
        {
            switch (type)
            {
                case EType.STRING:
                    return Constants.ServerEncoding.GetString(bytes);
                case EType.INT:
                    return BitConverter.ToInt32(bytes, 0);
                case EType.DOUBLE:
                    return BitConverter.ToDouble(bytes, 0);
                case EType.BOOL:
                    return BitConverter.ToBoolean(bytes, 0);
                case EType.CHAR:
                    return BitConverter.ToChar(bytes, 0);
                default:
                    return null;
            }
        }

        public static Argument CreateFromBytes(byte[] bytes)
        {
            var type = (EType)bytes[0];

            if (type.HasFlag(EType.ARRAY))
            {
                var elementCount = bytes[1] * 256 + bytes[2];
                var elementType = type & ~EType.ARRAY;

                var result = Array.CreateInstance(elementType.GetSystemType(), elementCount);

                var currentIndex = 3;
                for (var i = 0; i < elementCount; i++)
                {
                    var elementLength = bytes[currentIndex] * 256 + bytes[currentIndex + 1];
                    var valueBytes = bytes.Skip(currentIndex + 2).Take(elementLength).ToArray();
                    currentIndex += elementLength + 2;

                    result.SetValue(GetValue(elementType, valueBytes), i);
                }

                return new Argument(type, result);
            }
            else
            {
                var valueBytes = bytes.Skip(3).ToArray();

                return new Argument(type, GetValue(type, valueBytes));
            }
        }

        public override string ToString()
        {
            if (!Type.HasFlag(EType.ARRAY))
                return $"{Value} ({Type})";

            var elementType = Type & ~EType.ARRAY;
            var array = Value as Array;

            return $"{elementType}[{array.Length}]";
        }
    }
}
