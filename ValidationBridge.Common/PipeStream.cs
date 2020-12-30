using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ValidationBridge.Common
{
    public class PipeStream
    {
        private Stream _stream;

        public PipeStream(Stream stream)
        {
            this._stream = stream;
        }

        public byte[] Read()
        {
            var a = _stream.CanRead;
            int length = _stream.ReadByte() * 256 + _stream.ReadByte();
            if (length < 0) return null;

            byte[] inBuffer = new byte[length];
            _stream.Read(inBuffer, 0, length);

            return inBuffer;
        }

        public int Write(byte[] buffer)
        {
            int length = buffer.Length;
            if (length > ushort.MaxValue)
                length = ushort.MaxValue;

            _stream.WriteByte((byte)(length / 256));
            _stream.WriteByte((byte)(length & 255));
            _stream.Write(buffer, 0, length);
            _stream.Flush();

            return length + 2;
        }

        public string ReadString()
        {
            var inBuffer = Read();
            if (inBuffer == null) return null;

            return Constants.ServerEncoding.GetString(inBuffer);
        }

        public int WriteString(string message)
        {
            byte[] buffer = Constants.ServerEncoding.GetBytes(message);
            return Write(buffer);
        }
    }
}
