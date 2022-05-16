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

        public Argument(EType type, object value)
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

        public Argument(Guid value)
            : this(EType.HANDLE, value) { }

        public Argument(Guid[] value)
            : this(EType.ARRAY | EType.HANDLE, value) { }

        #endregion

        public TType GetValue<TType>()
        {
            return (TType) Value;
        }

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

                var result = new byte[values.Length + 9];
                result[0] = Convert.ToByte(Type);

                var byteLengthBytes = BitConverter.GetBytes(values.Length + 4);
                byteLengthBytes.CopyTo(result, 1);

                var lengthBytes = BitConverter.GetBytes(array.Length);
                lengthBytes.CopyTo(result, 5);
                values.CopyTo(result, 9);

                return result;
            }
            else return GetValueArray(Type, Value);
        }

        private byte[] GetValueArray(EType type, dynamic value, bool isArrayElement = false)
        {
            var valueArray = GetBytes(type, value);
            var resultArray = new byte[valueArray.Length + (isArrayElement ? 4 : 5)];

            var byteLengthBytes = BitConverter.GetBytes(valueArray.Length);
            byteLengthBytes.CopyTo(resultArray, isArrayElement ? 0 : 1);

            if (!isArrayElement) resultArray[0] = Convert.ToByte(type);

            valueArray.CopyTo(resultArray, isArrayElement ? 4 : 5);
            return resultArray;
        }

        public static byte[] GetBytes(EType type, dynamic value)
        {
            if(value == null)
                    return new byte[0];

            switch (type)
            {
                case EType.STRING:
                    return Constants.ServerEncoding.GetBytes(value);
                case EType.HANDLE:
                    return ((Guid)value).ToByteArray();
                case EType.NONE:
                    return new byte[0];
                default:
                    return BitConverter.GetBytes(value);
            }
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
                case EType.HANDLE:
                    return new Guid(bytes);
                default:
                    return null;
            }
        }

        public static Argument CreateFromBytes(byte[] bytes)
        {
            var type = (EType)bytes[0];

            if (type.HasFlag(EType.ARRAY))
            {
                var elementCount = BitConverter.ToInt32(bytes.Skip(5).Take(4).ToArray(), 0);
                var elementType = type & ~EType.ARRAY;

                var result = Array.CreateInstance(elementType.GetSystemType(), elementCount);

                var currentIndex = 9;
                for (var i = 0; i < elementCount; i++)
                {
                    var elementLengthBytes = bytes.Skip(currentIndex).Take(4).ToArray();
                    var elementLength = BitConverter.ToInt32(elementLengthBytes, 0);
                    var valueBytes = bytes.Skip(currentIndex + 4).Take(elementLength).ToArray();
                    currentIndex += elementLength + 4;

                    result.SetValue(GetValue(elementType, valueBytes), i);
                }

                return new Argument(type, result);
            }
            else
            {
                var lengthBytes = bytes.Skip(1).Take(4).ToArray();
                var valueBytes = bytes.Skip(5).Take(BitConverter.ToInt32(lengthBytes, 0)).ToArray();

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
