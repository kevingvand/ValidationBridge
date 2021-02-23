using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValiationBridge.Bridge.Adapters.Python.Enumerations;
using ValidationBridge.Common;

namespace ValiationBridge.Bridge.Adapters.Python.Messages
{
    public class PythonTextMessage : BaseMessage
    {
        public string Text { get; set; }

        public PythonTextMessage(EPythonMessageType messageType, string text)
        {
            MessageType = messageType;
            Text = text;
        }

        public override byte[] GetBytes()
        {
            var text = Constants.ServerEncoding.GetBytes(Text);

            byte[] result = new byte[text.Length + 5];
            result[0] = (byte)MessageType;

            var lengthBytes = BitConverter.GetBytes(text.Length);
            lengthBytes.CopyTo(result, 1);
            text.CopyTo(result, 5);

            return result;
        }
    }
}
