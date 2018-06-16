/*
* Copyright (c) 2017 MemoryInterop.ILPatcher - Nicholas Woodfield
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

namespace RelhaxModpack
{
    /// <summary>
    /// Internal stub type used by MemoryInterop.ILPatcher to inject fast-interop code. Depending on the type of method, the IL injection is either inline (replacing the call) or
    /// replaces the entire method body. Recommended types to use are generally blittable structs.
    /// </summary>
    internal class MemoryInterop
    {
        //
        // Below are replacement only, the method they are called in is cleared and replaced by new logic (argument types/order must match!)
        //

        /// <summary>
        /// Casts the by-ref value from one type to another.
        /// </summary>
        /// <typeparam name="TFrom">Type to cast from.</typeparam>
        /// <typeparam name="TTo">Type to cast to.</typeparam>
        /// <param name="source">By-ref value.</param>
        /// <returns>Ref to the value, as the new type.</returns>
        public static ref TTo As<TFrom, TTo>(ref TFrom source) { throw new NotImplementedException(); }

		/// <summary>
        /// Casts the readonly by-ref value from one type to another.
        /// </summary>
        /// <typeparam name="TFrom">Type to cast from.</typeparam>
        /// <typeparam name="TTo">Type to cast to.</typeparam>
        /// <param name="source">By-ref value.</param>
        /// <returns>Ref to the value, as the new type.</returns>
        public static ref readonly TTo AsReadonly<TFrom, TTo>(in TFrom source) { throw new NotImplementedException(); }

        /// <summary>
        /// Casts the pointer to a by-ref value of the specified type.
        /// </summary>
        /// <typeparam name="T">Type to cast to.</typeparam>
        /// <param name="pSrc">Pointer.</param>
        /// <returns>Ref to the value, as the new type.</returns>
        public static ref T AsRef<T>(IntPtr pSrc) { throw new NotImplementedException(); }

        /// <summary>
        /// Write data from the managed array to the memory location. This will temporarily pin the array and do a memcpy.
        /// </summary>
        /// <typeparam name="T">Type of data.</typeparam>
        /// <param name="pDest">Pointer to memory location to receive the data.</param>
        /// <param name="data">Array containing data to write.</param>
        /// <param name="startIndex">Zero-based index to start reading data from the array.</param>
        /// <param name="count">Number of elements to copy.</param>
        public static void WriteArray<T>(IntPtr pDest, T[] data, int startIndex, int count) where T : struct { throw new NotImplementedException(); }

        /// <summary>
        /// Write data from the managed array to the memory location. This will temporarily pin the array and do an unaligned memcpy.
        /// </summary>
        /// <typeparam name="T">Type of data.</typeparam>
        /// <param name="pDest">Pointer to memory location to write the data.</param>
        /// <param name="data">Array containing data to write.</param>
        /// <param name="startIndex">Zero-based index to start reading data from the array.</param>
        /// <param name="count">Number of elements to copy.</param>
        public static void WriteArrayUnaligned<T>(IntPtr pDest, T[] data, int startIndex, int count) where T : struct { throw new NotImplementedException(); }

        /// <summary>
        /// Read data from the memory location to the managed array. This will temporarily pin the array and do a memcpy.
        /// </summary>
        /// <typeparam name="T">Type of data.</typeparam>
        /// <param name="pSrc">Pointer to memory location to read the data.</param>
        /// <param name="data">Array to store the copied data.</param>
        /// <param name="startIndex">Zero-based index to start writing data to in the array.</param>
        /// <param name="count">Number of elements to copy.</param>
        public static void ReadArray<T>(IntPtr pSrc, T[] data, int startIndex, int count) where T : struct { throw new NotImplementedException(); }

        /// <summary>
        /// Read data from the memory location to the managed array. This will temporarily pin the array and do an unaligned memcpy.
        /// </summary>
        /// <typeparam name="T">Type of data.</typeparam>
        /// <param name="pSrc">Pointer to memory location to read the data.</param>
        /// <param name="data">Array to store the copied data.</param>
        /// <param name="startIndex">Zero-based index to start writing data to in the array.</param>
        /// <param name="count">Number of elements to copy.</param>
        public static void ReadArrayUnaligned<T>(IntPtr pSrc, T[] data, int startIndex, int count) where T : struct { throw new NotImplementedException(); }

        //
        // Below are inlined methods and will replace only the call instruction with appropiate IL (generally only 1-2 new instructions).
        //

        /// <summary>
        /// Computes the size of the type (inlined).
        /// </summary>
        /// <typeparam name="T">Type of data.</typeparam>
        /// <returns>Size of the type in bytes.</returns>
        public static int SizeOfInline<T>() { throw new NotImplementedException(); }

        /// <summary>
        /// Casts the by-ref value to a pointer (inlined). Note: This does not do any sort of pinning.
        /// </summary>
        /// <typeparam name="T">Type of data.</typeparam>
        /// <param name="src">Ref to a value.</param>
        /// <returns>Pointer to the memory location.</returns>
        public static IntPtr AsPointerInline<T>(ref T src) { throw new NotImplementedException(); }

		/// <summary>
        /// Casts the readonly by-ref value to a pointer (inlined). Note: This does not do any sort of pinning.
        /// </summary>
        /// <typeparam name="T">Type of data.</typeparam>
        /// <param name="src">Ref to a value.</param>
        /// <returns>Pointer to the memory location.</returns>
        public static IntPtr AsPointerReadonlyInline<T>(in T src) { throw new NotImplementedException(); }

        /// <summary>
        /// Writes a single element to the memory location (inlined).
        /// </summary>
        /// <typeparam name="T">Type of data.</typeparam>
        /// <param name="pDst">Pointer to memory location.</param>
        /// <param name="src">Value to be written.</param>
        public static unsafe void WriteInline<T>(void* pDst, in T src) { throw new NotImplementedException(); }

        /// <summary>
        /// Writes a single element to the memory location (inlined, unaligned copy).
        /// </summary>
        /// <typeparam name="T">Type of data.</typeparam>
        /// <param name="pDst">Pointer to memory location.</param>
        /// <param name="src">Value to be written.</param>
        public static unsafe void WriteUnalignedInline<T>(void* pDst, in T src) { throw new NotImplementedException(); }

        /// <summary>
        /// Reads a single element from the memory location (inlined).
        /// </summary>
        /// <typeparam name="T">Type of data.</typeparam>
        /// <param name="pSrc">Pointer to memory location.</param>
        /// <returns>Value read.</returns>
        public static unsafe T ReadInline<T>(void* pSrc) { throw new NotImplementedException(); }

        /// <summary>
        /// Reads a single element from the memory location (inlined, unaligned copy).
        /// </summary>
        /// <typeparam name="T">Type of data.</typeparam>
        /// <param name="pSrc">Pointer to memory location.</param>
        /// <returns>Value read.</returns>
        public static unsafe T ReadUnalignedInline<T>(void* pSrc) { throw new NotImplementedException(); }

        /// <summary>
        /// Copies the number of bytes from one pointer to the other (inlined).
        /// </summary>
        /// <param name="pDest">Pointer to the destination memory location.</param>
        /// <param name="pSrc">Pointer to the source memory location</param>
        /// <param name="byteCount">Number of bytes to copy</param>
        public static unsafe void MemCopyInline(void* pDest, void* pSrc, uint byteCount) { throw new NotImplementedException(); }

        /// <summary>
        /// Copies the number of bytes from one pointer to the other (inlined, unaligned copy).
        /// </summary>
        /// <param name="pDest">Pointer to the destination memory location.</param>
        /// <param name="pSrc">Pointer to the source memory location</param>
        /// <param name="byteCount">Number of bytes to copy</param>
        public static unsafe void MemCopyUnalignedInline(void* pDest, void* pSrc, uint byteCount) { throw new NotImplementedException(); }

        /// <summary>
        /// Clears the memory to a specified value (inlined).
        /// </summary>
        /// <param name="ptr">Pointer to the memory location.</param>
        /// <param name="clearValue">Value the memory will be cleared to.</param>
        /// <param name="byteCount">Number of bytes to to set.</param>
        public static unsafe void MemSetInline(void* ptr, byte clearValue, uint byteCount) { throw new NotImplementedException(); }

        /// <summary>
        /// Clears the memory to a specified value (inlined, unaligned init).
        /// </summary>
        /// <param name="ptr">Pointer to the memory location.</param>
        /// <param name="clearValue">Value the memory will be cleared to.</param>
        /// <param name="byteCount">Number of bytes to to set.</param>
        public static unsafe void MemSetUnalignedInline(void* ptr, byte clearValue, uint byteCount) { throw new NotImplementedException(); }
    }
}
