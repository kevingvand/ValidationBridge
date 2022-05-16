using System;
using System.Collections.Generic;
using System.Text;

namespace ValidationBridge.Common
{
    public class Constants
    {
        public const string ServerName = "ValidationBridge-Server";
        public const string BridgePath = @"C:\Users\Asus\Documents\Development\ValidationBridge\ValidationBridge\ValidationBridge.Bridge\bin\Debug\ValidationBridge.Bridge.exe";
        public const bool ShowBridgeWindow = true;
        public static readonly Encoding ServerEncoding = new UTF8Encoding();

        public class Commands
        {
            public const string GetModules = "GET_MODULES";
            public const string GetModule = "GET_MODULE";
            public const string GetModuleInterfaces = "GET_MODULE_INTERFACES";
            public const string LoadModule = "LOAD_MODULE";
            public const string LoadModules = "LOAD_MODULES";
        }
    }
}
