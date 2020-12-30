using System;
using System.Collections.Generic;
using System.Text;

namespace ValidationBridge.Common.Enumerations
{
    [Flags]
    public enum EType
    {
        NONE = 0,
        INT = 1,
        BOOL = 2,
        DOUBLE = 4,
        CHAR = 8,
        STRING = 16,
        ARRAY = 32
    }

    public static class ETypeExtension
    {
        public static Type GetSystemType(this EType type)
        {
            switch (type)
            {
                case EType.INT:
                    return typeof(int);
                case EType.BOOL:
                    return typeof(bool);
                case EType.DOUBLE:
                    return typeof(double);
                case EType.CHAR:
                    return typeof(char);
                case EType.STRING:
                    return typeof(string);
                case EType.ARRAY:
                    return typeof(Array);
                case EType.NONE:
                default:
                    throw new Exception("Specified type is invalid.");
            }
        }
    }
}
