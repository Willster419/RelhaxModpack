/*
* Copyright (c) 2016-2018 TeximpNet - Nicholas Woodfield
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System.IO;

namespace RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet
{
    //Helpers for reading/writing from/to an unknown stream (thus has error checking).
    internal static class StreamHelper
    {
        public static bool CanReadBytes(Stream stream, long numBytes)
        {
            long remainingCount = stream.Length - stream.Position;
            return numBytes <= remainingCount;
        }

        public static bool ReadUInt64(Stream stream, out ulong value)
        {
            value = 0;

            int b1 = stream.ReadByte();
            int b2 = stream.ReadByte();
            int b3 = stream.ReadByte();
            int b4 = stream.ReadByte();

            if (b1 < 0 || b2 < 0 || b3 < 0 || b4 < 0)
                return false;

            int b5 = stream.ReadByte();
            int b6 = stream.ReadByte();
            int b7 = stream.ReadByte();
            int b8 = stream.ReadByte();

            if (b5 < 0 || b6 < 0 || b7 < 0 || b8 < 0)
                return false;

            uint num1 = (uint)(b1 | (b2 << 8) | (b3 << 16) | (b4 << 24));
            uint num2 = (uint)(b5 | (b6 << 8) | (b7 << 16) | (b8 << 24));

            value = (ulong)(num2 << 32 | num1);
            return true;
        }

        public static void WriteUInt64(ulong value, Stream stream)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));

            stream.WriteByte((byte)(value >> 32));
            stream.WriteByte((byte)(value >> 40));
            stream.WriteByte((byte)(value >> 48));
            stream.WriteByte((byte)(value >> 56));
        }

        public static bool ReadUInt32(Stream stream, out uint value)
        {
            value = 0;

            int b1 = stream.ReadByte();
            int b2 = stream.ReadByte();
            int b3 = stream.ReadByte();
            int b4 = stream.ReadByte();

            if (b1 < 0 || b2 < 0 || b3 < 0 || b4 < 0)
                return false;

            value = (uint)(b1 | (b2 << 8) | (b3 << 16) | (b4 << 24));
            return true;
        }

        public static void WriteUInt32(uint value, Stream stream)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));
        }

        public static bool ReadUInt16(Stream stream, out ushort value)
        {
            value = 0;

            int b1 = stream.ReadByte();
            int b2 = stream.ReadByte();

            if (b1 < 0 || b2 < 0)
                return false;

            value = (ushort)(b1 | (b2 << 8));
            return true;
        }

        public static void WriteUInt16(ushort value, Stream stream)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
        }
    }
}
