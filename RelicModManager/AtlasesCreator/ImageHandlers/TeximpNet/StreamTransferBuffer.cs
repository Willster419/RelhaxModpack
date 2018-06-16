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

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet
{
    /// <summary>
    /// A buffer that facilitates transfer between managed/unmanaged memory and <see cref="Stream"/> objects. An intermediate byte buffer is created and pinned,
    /// allow for copying between it and unmanaged memory.
    /// </summary>
    public class StreamTransferBuffer : IDisposable
    {
        private bool m_isDisposed;
        private byte[] m_buffer;
        private GCHandle m_pinHandle;
        private int m_lastReadByteCount;

        /// <summary>
        /// Gets the pointer to the byte array.
        /// </summary>
        public IntPtr Pointer
        {
            get
            {
                return m_pinHandle.AddrOfPinnedObject();
            }
        }

        /// <summary>
        /// Gets the byte array used by the buffer.
        /// </summary>
        public byte[] ByteArray
        {
            get
            {
                return m_buffer;
            }
        }

        /// <summary>
        /// Gets the size of the buffer in bytes.
        /// </summary>
        public int Length
        {
            get
            {
                return m_buffer.Length;
            }
        }

        /// <summary>
        /// Gets the number of bytes last read.
        /// </summary>
        public int LastReadByteCount
        {
            get
            {
                return m_lastReadByteCount;
            }
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="StreamTransferBuffer"/> class. The buffer size will be 81,920 bytes.
        /// </summary>
        public StreamTransferBuffer() : this(81920) { }

        /// <summary>
        /// Constructs a new instance of the <see cref="StreamTransferBuffer"/> class.
        /// </summary>
        /// <param name="numBytes">Size of the buffer, but cannot exceed LOH allocation of 85,000 bytes unless specified.</param>
        /// <param name="avoidLOH">True to avoid Large Object Heap allocation by allocating a buffer slightly larger than 85k bytes, false to allow any sized buffer.</param>
        public StreamTransferBuffer(int numBytes, bool avoidLOH = true)
        {
            m_isDisposed = false;
            Resize(numBytes, avoidLOH);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="StreamTransferBuffer"/> class.
        /// </summary>
        ~StreamTransferBuffer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Reads the specified number of bytes from the stream into the buffer.
        /// </summary>
        /// <param name="input">Stream to read from.</param>
        /// <param name="numBytes">Number of bytes to read.</param>
        /// <returns>Actual read number of bytes (<see cref="LastReadByteCount"/> is also set to this).</returns>
        public int ReadBytes(Stream input, int numBytes)
        {
            m_lastReadByteCount = input.Read(m_buffer, 0, numBytes);
            return m_lastReadByteCount;
        }

        /// <summary>
        /// Reads the value from the stream. First the buffer is filled from the stream by the data's size in bytes, then the data is copied.
        /// </summary>
        /// <typeparam name="T">Type of data to read.</typeparam>
        /// <param name="input">Stream to read from.</param>
        /// <param name="value">The value to read.</param>
        /// <returns>True if the value was read from the stream, false if there was not enough bytes to read.</returns>
        public unsafe bool Read<T>(Stream input, out T value) where T : struct
        {
            int size = MemoryInterop.SizeOfInline<T>();

            //Fill buffer...validate we read t he expected # of bytes
            if (ReadBytes(input, size) != size)
            {
                value = default(T);
                return false;
            }

            //Copy data
            value = MemoryInterop.ReadInline<T>((void*)m_pinHandle.AddrOfPinnedObject());
            return true;
        }

        /// <summary>
        /// Reads the value from the stream. First the buffer is filled from the stream by the data's size in bytes, then the data is copied.
        /// </summary>
        /// <typeparam name="T">Type of data to read.</typeparam>
        /// <param name="input">Stream to read from.</param>
        /// <returns>The read value.</returns>
        public unsafe T Read<T>(Stream input) where T : struct
        {
            int size = MemoryInterop.SizeOfInline<T>();

            //Fill buffer...validate we read t he expected # of bytes
            if (ReadBytes(input, size) != size)
                return default(T);

            //Copy data
            return MemoryInterop.ReadInline<T>((void*)m_pinHandle.AddrOfPinnedObject());
        }

        /// <summary>
        /// Writes the specified number of bytes from the buffer into the stream.
        /// </summary>
        /// <param name="output">Stream to write to.</param>
        /// <param name="numBytes">Number of bytes to write.</param>
        public void WriteBytes(Stream output, int numBytes)
        {
            m_lastReadByteCount = 0;
            output.Write(m_buffer, 0, numBytes);
        }

        /// <summary>
        /// Writes the value to the stream.
        /// </summary>
        /// <typeparam name="T">Type of data to write.</typeparam>
        /// <param name="output">Stream to write to.</param>
        /// <param name="value">Value to write.</param>
        public unsafe void Write<T>(Stream output, in T value) where T : struct
        {
            int size = MemoryInterop.SizeOfInline<T>();

            //Fill buffer
            MemoryInterop.WriteInline<T>((void*)m_pinHandle.AddrOfPinnedObject(), in value);

            //Write to stream
            WriteBytes(output, size);
        }

        /// <summary>
        /// Resizes the transfer buffer to be the specified number of bytes.
        /// </summary>
        /// <param name="numBytes">Size of the buffer, but cannot exceed LOH allocation of 85,000 bytes unless specified.</param>
        /// <param name="avoidLOH">True to avoid Large Object Heap allocation by allocating a buffer slightly larger than 85k bytes, false to allow any sized buffer.</param>
        public void Resize(int numBytes, bool avoidLOH = true)
        {
            if (m_isDisposed)
                throw new ObjectDisposedException("StreamTransferBuffer");

            //If already had a buffer...ensure its unpinned
            if (m_pinHandle.IsAllocated)
                m_pinHandle.Free();

            if (numBytes <= 0)
                numBytes = 81920;

            //Restrict buffer size to be less than LOH min size < 85,000 bytes
            numBytes = (avoidLOH) ? Math.Min(numBytes, 85000) : numBytes;
            m_buffer = new byte[numBytes];
            m_pinHandle = GCHandle.Alloc(m_buffer, GCHandleType.Pinned);
            m_lastReadByteCount = 0;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="isDisposing"><c>True</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected void Dispose(bool isDisposing)
        {
            if (!m_isDisposed)
            {
                if (m_pinHandle.IsAllocated)
                    m_pinHandle.Free();

                m_buffer = null;
                m_lastReadByteCount = 0;

                m_isDisposed = true;
            }
        }
    }
}
