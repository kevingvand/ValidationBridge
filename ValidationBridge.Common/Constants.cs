using System;
using System.Collections.Generic;
using System.Text;

namespace ValidationBridge.Common
{
    public class Constants
    {
        public const string ServerName = "ValidationBridge-Server";
        public static readonly Encoding ServerEncoding = new UnicodeEncoding();

        public class Commands
        {
            public const string GetModules = "GET_MODULES";
            public const string GetModule = "GET_MODULE";
        }
    }
}
