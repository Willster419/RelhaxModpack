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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet
{
    /// <summary>
    /// Helper class for dealing with memory, in particular unmanaged memory.
    /// </summary>
    public static class MemoryHelper
    {
        private static Dictionary<Object, GCHandle> s_pinnedObjects = new Dictionary<Object, GCHandle>();

        /// <summary>
        /// Pins an object in memory, which allows a pointer to it to be returned. While the object remains pinned the runtime
        /// cannot move the object around in memory, which may degrade performance.
        /// </summary>
        /// <param name="obj">Object to pin.</param>
        /// <returns>Pointer to pinned object's memory location.</returns>
        public static IntPtr PinObject(Object obj)
        {
            lock (s_pinnedObjects)
            {
                GCHandle handle;
                if (!s_pinnedObjects.TryGetValue(obj, out handle))
                {
                    handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
                    s_pinnedObjects.Add(obj, handle);
                }

                return handle.AddrOfPinnedObject();
            }
        }

        /// <summary>
        /// Unpins an object in memory, allowing it to once again freely be moved around by the runtime.
        /// </summary>
        /// <param name="obj">Object to unpin.</param>
        public static void UnpinObject(Object obj)
        {
            lock (s_pinnedObjects)
            {
                GCHandle handle;
                if (!s_pinnedObjects.TryGetValue(obj, out handle))
                {
                    handle.Free();
                    s_pinnedObjects.Remove(obj);
                }
            }
        }


        /// <summary>
        /// Convienence method to dispose all items in the collection
        /// </summary>
        /// <typeparam name="T">IDisposable type</typeparam>
        /// <param name="collection">Collection of disposables</param>
        public static void DisposeCollection<T>(ICollection<T> collection) where T : IDisposable
        {
            if (collection == null)
                return;

            //Check if it's a list, so we can avoid having to call the enumerator
            IList<T> list = collection as IList<T>;

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    IDisposable disposable = list[i];
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
            else
            {
                //Otherwise enumerate the collection
                foreach (IDisposable disposable in collection)
                {
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
        }

        /// <summary>
        /// Casts an underlying value type to an enum type, WITHOUT first casting the value to an Object. So this avoid boxing the value.
        /// </summary>
        /// <typeparam name="V">Underlying value type.</typeparam>
        /// <typeparam name="T">Enum type.</typeparam>
        /// <param name="value">Value to cast.</param>
        /// <returns>Enum value.</returns>
        public static T CastToEnum<V, T>(V value) where V : struct, IComparable, IFormattable, IConvertible where T : struct, IComparable, IFormattable, IConvertible
        {
            return MemoryInterop.As<V, T>(ref value);
        }

        /// <summary>
        /// Allocates unmanaged memory. This memory should only be freed by this helper.
        /// </summary>
        /// <param name="sizeInBytes">Size to allocate</param>
        /// <param name="alignment">Alignment of the memory, by default aligned along 16-byte boundary.</param>
        /// <returns>Pointer to the allocated unmanaged memory.</returns>
        public static unsafe IntPtr AllocateMemory(int sizeInBytes, int alignment = 16)
        {
            int mask = alignment - 1;
            IntPtr rawPtr = Marshal.AllocHGlobal(sizeInBytes + mask + IntPtr.Size);
            long ptr = (long)((byte*)rawPtr + sizeof(void*) + mask) & ~mask;
            ((IntPtr*)ptr)[-1] = rawPtr;

            return new IntPtr(ptr);
        }

        /// <summary>
        /// Allocates unmanaged memory that is cleared to a certain value. This memory should only be freed by this helper.
        /// </summary>
        /// <param name="sizeInBytes">Size to allocate</param>
        /// <param name="clearValue">Value the memory will be cleared to, by default zero.</param>
        /// <param name="alignment">Alignment of the memory, by default aligned along 16-byte boundary.</param>
        /// <returns>Pointer to the allocated unmanaged memory.</returns>
        public static unsafe IntPtr AllocateClearedMemory(int sizeInBytes, byte clearValue = 0, int alignment = 16)
        {
            IntPtr ptr = AllocateMemory(sizeInBytes, alignment);
            ClearMemory(ptr, clearValue, sizeInBytes);
            return ptr;
        }

        /// <summary>
        /// Frees unmanaged memory that was allocated by this helper.
        /// </summary>
        /// <param name="memoryPtr">Pointer to unmanaged memory to free.</param>
        public static unsafe void FreeMemory(IntPtr memoryPtr)
        {
            if (memoryPtr == IntPtr.Zero)
                return;

            Marshal.FreeHGlobal(((IntPtr*)memoryPtr)[-1]);
        }

        /// <summary>
        /// Checks if the memory is aligned to the specified alignment.
        /// </summary>
        /// <param name="memoryPtr">Pointer to the memory</param>
        /// <param name="alignment">Alignment value, by defauly 16-byte</param>
        /// <returns>True if is aligned, false otherwise.</returns>
        public static bool IsMemoryAligned(IntPtr memoryPtr, int alignment = 16)
        {
            int mask = alignment - 1;
            return (memoryPtr.ToInt64() & mask) == 0;
        }

        /// <summary>
        /// Swaps the value between two references.
        /// </summary>
        /// <typeparam name="T">Type of data to swap.</typeparam>
        /// <param name="left">First reference</param>
        /// <param name="right">Second reference</param>
        public static void Swap<T>(ref T left, ref T right)
        {
            T temp = left;
            left = right;
            right = temp;
        }

        /// <summary>
        /// Computes a hash code using the <a href="http://bretm.home.comcast.net/~bretm/hash/6.html">FNV modified algorithm</a>m.
        /// </summary>
        /// <param name="data">Byte data to hash.</param>
        /// <returns>Hash code for the data.</returns>
        public static int ComputeFNVModifiedHashCode(byte[] data)
        {
            if (data == null || data.Length == 0)
                return 0;

            unchecked
            {
                uint p = 16777619;
                uint hash = 2166136261;

                for (int i = 0; i < data.Length; i++)
                    hash = (hash ^ data[i]) * p;

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;

                return (int)hash;
            }
        }

        /// <summary>
        /// Reads a stream until the end is reached into a byte array. Based on
        /// <a href="http://www.yoda.arachsys.com/csharp/readbinary.html">Jon Skeet's implementation</a>.
        /// It is up to the caller to dispose of the stream.
        /// </summary>
        /// <param name="stream">Stream to read all bytes from</param>
        /// <param name="initialLength">Initial buffer length, default is 32K</param>
        /// <returns>The byte array containing all the bytes from the stream</returns>
        public static byte[] ReadStreamFully(Stream stream, int initialLength)
        {
            if (initialLength < 1)
            {
                initialLength = 32768; //Init to 32K if not a valid initial length
            }

            byte[] buffer = new byte[initialLength];
            int position = 0;
            int chunk;

            while ((chunk = stream.Read(buffer, position, buffer.Length - position)) > 0)
            {
                position += chunk;

                //If we reached the end of the buffer check to see if there's more info
                if (position == buffer.Length)
                {
                    int nextByte = stream.ReadByte();

                    //If -1 we reached the end of the stream
                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    //Not at the end, need to resize the buffer
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[position] = (byte)nextByte;
                    buffer = newBuffer;
                    position++;
                }
            }

            //Trim the buffer before returning
            byte[] toReturn = new byte[position];
            Array.Copy(buffer, toReturn, position);
            return toReturn;
        }

        /// <summary>
        /// Compares two arrays of bytes for equivalence. 
        /// </summary>
        /// <param name="firstData">First array of data.</param>
        /// <param name="secondData">Second array of data.</param>
        /// <returns>True if both arrays contain the same data, false otherwise.</returns>
        public static bool Compare(byte[] firstData, byte[] secondData)
        {
            if (Object.ReferenceEquals(firstData, secondData))
                return true;

            if (Object.ReferenceEquals(firstData, null) || Object.ReferenceEquals(secondData, null))
                return false;

            if (firstData.Length != secondData.Length)
                return false;

            for (int i = 0; i < firstData.Length; i++)
            {
                if (firstData[i] != secondData[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Clears the memory to the specified value.
        /// </summary>
        /// <param name="memoryPtr">Pointer to the memory.</param>
        /// <param name="clearValue">Value the memory will be cleared to.</param>
        /// <param name="sizeInBytesToClear">Number of bytes, starting from the memory pointer, to clear.</param>
        public static unsafe void ClearMemory(IntPtr memoryPtr, byte clearValue, int sizeInBytesToClear)
        {
            MemoryInterop.MemSetUnalignedInline((void*)memoryPtr, clearValue, (uint)sizeInBytesToClear);
        }

        /// <summary>
        /// Computes the size of the struct type.
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <returns>Size of the struct in bytes.</returns>
        public static unsafe int SizeOf<T>() where T : struct
        {
            return MemoryInterop.SizeOfInline<T>();
        }

        /// <summary>
        /// Casts the by-ref value into a pointer.
        /// </summary>
        /// <typeparam name="T">Struct type.</typeparam>
        /// <param name="src">By-ref value.</param>
        /// <returns>Pointer to the value.</returns>
        public static unsafe IntPtr AsPointer<T>(ref T src) where T : struct
        {
            return MemoryInterop.AsPointerInline<T>(ref src);
        }

        /// <summary>
        /// Casts the readonly by-ref value into a pointer.
        /// </summary>
        /// <typeparam name="T">Struct type.</typeparam>
        /// <param name="src">By-ref value.</param>
        /// <returns>Pointer to the value.</returns>
        public static unsafe IntPtr AsPointerReadonly<T>(in T src) where T : struct
        {
            return MemoryInterop.AsPointerReadonlyInline<T>(in src);
        }

        /// <summary>
        /// Casts the pointer into a by-ref value of the specified type.
        /// </summary>
        /// <typeparam name="T">Struct type.</typeparam>
        /// <param name="pSrc">Memory location.</param>
        /// <returns>By-ref value.</returns>
        public static unsafe ref T AsRef<T>(IntPtr pSrc) where T : struct
        {
            return ref MemoryInterop.AsRef<T>(pSrc);
        }

        /// <summary>
        /// Casts one by-ref type to another, unsafely.
        /// </summary>
        /// <typeparam name="TFrom">From struct type</typeparam>
        /// <typeparam name="TTo">To struct type</typeparam>
        /// <param name="src">Source by-ref value.</param>
        /// <returns>Reference as the from type.</returns>
        public static ref TTo As<TFrom, TTo>(ref TFrom src) where TFrom : struct where TTo : struct
        {
            return ref MemoryInterop.As<TFrom, TTo>(ref src);
        }

        /// <summary>
        /// Casts one readonly by-ref type to another, unsafely.
        /// </summary>
        /// <typeparam name="TFrom">From struct type</typeparam>
        /// <typeparam name="TTo">To struct type</typeparam>
        /// <param name="src">Source by-ref value.</param>
        /// <returns>Reference as the from type.</returns>
        public static ref readonly TTo AsReadonly<TFrom, TTo>(in TFrom src) where TFrom : struct where TTo : struct
        {
            return ref MemoryInterop.AsReadonly<TFrom, TTo>(in src);
        }

        /// <summary>
        /// Computes the size of the struct array.
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <param name="array">Array of structs</param>
        /// <returns>Total size, in bytes, of the array's contents.</returns>
        public static int SizeOf<T>(T[] array) where T : struct
        {
            return array == null ? 0 : array.Length * MemoryInterop.SizeOfInline<T>();
        }

        /// <summary>
        /// Adds an offset to the pointer.
        /// </summary>
        /// <param name="ptr">Pointer</param>
        /// <param name="offset">Offset</param>
        /// <returns>Pointer plus the offset</returns>
        public static IntPtr AddIntPtr(IntPtr ptr, int offset)
        {
            return new IntPtr(ptr.ToInt64() + offset);
        }

        /// <summary>
        /// Performs a memcopy that copies data from the memory pointed to by the source pointer to the memory pointer by the destination pointer.
        /// </summary>
        /// <param name="pDest">Destination memory location</param>
        /// <param name="pSrc">Source memory location</param>
        /// <param name="sizeInBytesToCopy">Number of bytes to copy</param>
        public static unsafe void CopyMemory(IntPtr pDest, IntPtr pSrc, int sizeInBytesToCopy)
        {
            MemoryInterop.MemCopyUnalignedInline((void*)pDest, (void*)pSrc, (uint)sizeInBytesToCopy);
        }

        /// <summary>
        /// Returns the number of elements in the enumerable.
        /// </summary>
        /// <typeparam name="T">Type of element in collection.</typeparam>
        /// <param name="source">Enumerable collection</param>
        /// <returns>The number of elements in the enumerable collection.</returns>
        public static int Count<T>(IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            ICollection<T> coll = source as ICollection<T>;
            if (coll != null)
                return coll.Count;

            ICollection otherColl = source as ICollection;
            if (otherColl != null)
                return otherColl.Count;

#if NETSTANDARD1_3
            IReadOnlyCollection<T> roColl = source as IReadOnlyCollection<T>;
            if(roColl != null)
                return roColl.Count;
#endif

            int count = 0;
            using (IEnumerator<T> enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Converts typed element array to a byte array.
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <param name="source">Element array</param>
        /// <returns>Byte array copy or null if the source array was not valid.</returns>
        public static unsafe byte[] ToByteArray<T>(T[] source) where T : struct
        {
            if (source == null || source.Length == 0)
                return null;

            byte[] buffer = new byte[MemoryInterop.SizeOfInline<T>() * source.Length];

            fixed (void* pBuffer = buffer)
            {
                Write<T>((IntPtr)pBuffer, source, 0, source.Length);
            }

            return buffer;
        }

        /// <summary>
        /// Converts a byte array to a typed element array.
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <param name="source">Byte array</param>
        /// <returns>Typed element array or null if the source array was not valid.</returns>
        public static unsafe T[] FromByteArray<T>(byte[] source) where T : struct
        {
            if (source == null || source.Length == 0)
                return null;

            T[] buffer = new T[(int)Math.Floor(((double)source.Length) / ((double)MemoryInterop.SizeOfInline<T>()))];

            fixed (void* pBuffer = source)
            {
                Read<T>((IntPtr)pBuffer, buffer, 0, buffer.Length);
            }

            return buffer;
        }

        /// <summary>
        /// Copies bytes from a byte array to an element array.
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <param name="srcArray">Source byte array</param>
        /// <param name="srcStartIndex">Starting index in destination array</param>
        /// <param name="destArray">Destination element array</param>
        /// <param name="destStartIndex">Starting index in destination array</param>
        /// <param name="count">Number of elements to copy</param>
        public static unsafe void CopyBytes<T>(byte[] srcArray, int srcStartIndex, T[] destArray, int destStartIndex, int count) where T : struct
        {
            if (srcArray == null || srcArray.Length == 0 || destArray == null || destArray.Length == 0)
                return;

            int byteCount = MemoryInterop.SizeOfInline<T>() * count;

            if (srcStartIndex < 0 || (srcStartIndex + byteCount) > srcArray.Length || destStartIndex < 0 || (destStartIndex + count) > destArray.Length)
                return;

            fixed (void* pBuffer = &srcArray[srcStartIndex])
            {
                Read<T>((IntPtr)pBuffer, destArray, destStartIndex, count);
            }
        }

        /// <summary>
        /// Copies bytes from an element array to a byte array.
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <param name="srcArray">Source element array</param>
        /// <param name="srcStartIndex">Starting index in source array</param>
        /// <param name="destArray">Destination byte array</param>
        /// <param name="destStartIndex">Starting index in destination array</param>
        /// <param name="count">Number of elements to copy</param>
        public static unsafe void CopyBytes<T>(T[] srcArray, int srcStartIndex, byte[] destArray, int destStartIndex, int count) where T : struct
        {
            if (srcArray == null || srcArray.Length == 0 || destArray == null || destArray.Length == 0)
                return;

            int byteCount = MemoryInterop.SizeOfInline<T>() * count;

            if (srcStartIndex < 0 || (srcStartIndex + count) > srcArray.Length || destStartIndex < 0 || (destStartIndex + byteCount) > destArray.Length)
                return;

            fixed (void* pBuffer = &destArray[destStartIndex])
            {
                Write<T>((IntPtr)pBuffer, srcArray, srcStartIndex, count);
            }
        }

        /// <summary>
        /// Reads data from the memory location into the array.
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <param name="pSrc">Pointer to memory location</param>
        /// <param name="data">Array to store the copied data</param>
        /// <param name="startIndexInArray">Zero-based element index to start writing data to in the element array.</param>
        /// <param name="count">Number of elements to copy</param>
        public static unsafe void Read<T>(IntPtr pSrc, T[] data, int startIndexInArray, int count) where T : struct
        {
            MemoryInterop.ReadArrayUnaligned<T>(pSrc, data, startIndexInArray, count);
        }

        /// <summary>
        /// Reads a single element from the memory location.
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <param name="pSrc">Pointer to memory location</param>
        /// <returns>The read value</returns>
        public static unsafe T Read<T>(IntPtr pSrc) where T : struct
        {
            return MemoryInterop.ReadInline<T>((void*)pSrc);
        }

        /// <summary>
        /// Reads a single element from the memory location.
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <param name="pSrc">Pointer to memory location</param>
        /// <param name="value">The read value.</param>
        public static unsafe void Read<T>(IntPtr pSrc, out T value) where T : struct
        {
            value = MemoryInterop.ReadInline<T>((void*)pSrc);
        }

        /// <summary>
        /// Writes data from the array to the memory location.
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <param name="pDest">Pointer to memory location</param>
        /// <param name="data">Array containing data to write</param>
        /// <param name="startIndexInArray">Zero-based element index to start reading data from in the element array.</param>
        /// <param name="count">Number of elements to copy</param>
        public static unsafe void Write<T>(IntPtr pDest, T[] data, int startIndexInArray, int count) where T : struct
        {
            MemoryInterop.WriteArrayUnaligned<T>(pDest, data, startIndexInArray, count);
        }

        /// <summary>
        /// Writes a single element to the memory location.
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <param name="pDest">Pointer to memory location</param>
        /// <param name="data">The value to write</param>
        public static unsafe void Write<T>(IntPtr pDest, in T data) where T : struct
        {
            MemoryInterop.WriteInline<T>((void*)pDest, in data);
        }
    }
}