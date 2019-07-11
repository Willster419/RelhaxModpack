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
using System.Collections.Generic;
using System.Diagnostics;

namespace RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet.DDS
{
    /// <summary>
    /// Represents a collection of mipmap surfaces. A valid texture always has at least one surface. The first surface is the largest image, and each subsequent mipmap
    /// is a scaled down texture by a factor of 2.
    /// </summary>
    public sealed class MipChain : List<MipData>
    {
        /// <summary>
        /// Constructs a new <see cref="MipChain"/>.
        /// </summary>
        public MipChain() { }

        /// <summary>
        /// Constructs a new <see cref="MipChain"/>.
        /// </summary>
        /// <param name="capacity">Initial capacity.</param>
        public MipChain(int capacity) : base(capacity) { }

        /// <summary>
        /// Constructs a new <see cref="MipChain"/>.
        /// </summary>
        /// <param name="surfaces">Collection of images to add to this collection.</param>
        public MipChain(IEnumerable<MipData> surfaces) : base(surfaces) { }
    }

    /// <summary>
    /// Represents mipmap data for a single image in a mipchain. The image data may be a 1D, 2D, or 3D image.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    [DebuggerDisplay("Width = {Width}, Height = {Height}, Depth = {Depth}, RowPitch = {RowPitch}, SlicePitch = {SlicePitch}, SizeInBytes = {SizeInBytes}, OwnsData = {OwnsData}")]
    public sealed class MipData : IDisposable
    {
        private bool m_isDisposed;
        private bool m_ownData;

        /// <summary>
        /// Width of the image. At least one.
        /// </summary>
        public readonly int Width;

        /// <summary>
        /// Height of the image. At least one.
        /// </summary>
        public readonly int Height;

        /// <summary>
        /// Depth o the image. At least one.
        /// </summary>
        public readonly int Depth;

        /// <summary>
        /// Total # of bytes per scanline in the image. May be greater than the size of the pixel format times the <see cref="Width"/>, due to padding.
        /// </summary>
        public readonly int RowPitch;

        /// <summary>
        /// Total # of bytes per slice in the 3D image. Typically <see cref="RowPitch"/> * <see cref="Height"/>.
        /// </summary>
        public readonly int SlicePitch;

        /// <summary>
        /// Data pointer managed by this image.
        /// </summary>
        public readonly IntPtr Data;

        /// <summary>
        /// Gets the total size of the image data in bytes.
        /// </summary>
        public int SizeInBytes
        {
            get
            {
                return Depth * SlicePitch;
            }
        }

        /// <summary>
        /// Gets if this object owns the image memory.
        /// </summary>
        public bool OwnsData
        {
            get
            {
                return m_ownData;
            }
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MipData"/> class. Represents a 1D image.
        /// </summary>
        /// <param name="width">Width of the image, in texels.</param>
        /// <param name="rowPitch">Row pitch.</param>
        /// <param name="data">Pointer to image data.</param>
        /// <param name="ownData">If true, the pointer will be cleaned up by this object, if false then it is assumed to be managed externally.</param>
        public MipData(int width, int rowPitch, IntPtr data, bool ownData = true)
        {
            m_isDisposed = false;

            Width = width;
            Height = 1;
            Depth = 1;

            RowPitch = rowPitch;
            SlicePitch = rowPitch;

            m_ownData = ownData;
            Data = data;

            if (ownData)
                AddGCMemoryPressure();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MipData"/> class. Represents a 1D image. This allocates the owned memory.
        /// </summary>
        /// <param name="width">Width of the image, in texels.</param>
        /// <param name="rowPitch">Row pitch.</param>
        public MipData(int width, int rowPitch)
        {
            m_isDisposed = false;

            Width = width;
            Height = 1;
            Depth = 1;

            RowPitch = rowPitch;
            SlicePitch = rowPitch;

            m_ownData = true;
            Data = MemoryHelper.AllocateMemory(SizeInBytes);
            AddGCMemoryPressure();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MipData"/> class. Represents a 2D image.
        /// </summary>
        /// <param name="width">Width of the image, in texels.</param>
        /// <param name="height">Height of the image, in texels.</param>
        /// <param name="rowPitch">Row pitch.</param>
        /// <param name="data">Pointer to image data.</param>
        /// <param name="ownData">If true, the pointer will be cleaned up by this object, if false then it is assumed to be managed externally.</param>
        public MipData(int width, int height, int rowPitch, IntPtr data, bool ownData = true)
        {
            m_isDisposed = false;

            Width = width;
            Height = height;
            Depth = 1;

            RowPitch = rowPitch;
            SlicePitch = rowPitch * height;

            m_ownData = ownData;
            Data = data;

            if (ownData)
                AddGCMemoryPressure();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MipData"/> class. Represents a 2D image. This allocates the owned memory.
        /// </summary>
        /// <param name="width">Width of the image, in texels.</param>
        /// <param name="height">Height of the image, in texels.</param>
        /// <param name="rowPitch">Row pitch.</param>
        public MipData(int width, int height, int rowPitch)
        {
            m_isDisposed = false;

            Width = width;
            Height = height;
            Depth = 1;

            RowPitch = rowPitch;
            SlicePitch = rowPitch * height;

            m_ownData = true;
            Data = MemoryHelper.AllocateMemory(SizeInBytes);
            AddGCMemoryPressure();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MipData"/> class. Represents a 3D image. 
        /// </summary>
        /// <param name="width">Width of the image, in texels.</param>
        /// <param name="height">Height of the image, in texels.</param>
        /// <param name="depth">Depth of the image, in texels.</param>
        /// <param name="rowPitch">Row pitch.</param>
        /// <param name="slicePitch">Slice pitch.</param>
        /// <param name="data">Pointer to image data.</param>
        /// <param name="ownData">If true, the pointer will be cleaned up by this object, if false then it is assumed to be managed externally.</param>
        public MipData(int width, int height, int depth, int rowPitch, int slicePitch, IntPtr data, bool ownData = true)
        {
            m_isDisposed = false;

            Width = width;
            Height = height;
            Depth = depth;

            RowPitch = rowPitch;
            SlicePitch = slicePitch;

            m_ownData = ownData;
            Data = data;

            if (ownData)
                AddGCMemoryPressure();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MipData"/> class. Represents a 3D image. This allocates the owned memory.
        /// </summary>
        /// <param name="width">Width of the image, in texels.</param>
        /// <param name="height">Height of the image, in texels.</param>
        /// <param name="depth">Depth of the image, in texels.</param>
        /// <param name="rowPitch">Row pitch.</param>
        /// <param name="slicePitch">Slice pitch.</param>
        public MipData(int width, int height, int depth, int rowPitch, int slicePitch)
        {
            m_isDisposed = false;

            Width = width;
            Height = height;
            Depth = depth;

            RowPitch = rowPitch;
            SlicePitch = slicePitch;

            m_ownData = true;
            Data = MemoryHelper.AllocateMemory(SizeInBytes);
            AddGCMemoryPressure();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MipData"/> class.
        /// </summary>
        /// <param name="bitmap">FreeImage bitmap to wrap or copy from.</param>
        /// <param name="copyData">True to copy the image data, false to wrap the free image bitmap data.</param>
        /// <exception cref="ArgumentException">Thrown if the bitmap is not valid.</exception>
        public MipData(Surface bitmap, bool copyData = false)
        {
            if (bitmap == null || bitmap.IsDisposed)
                throw new ArgumentException("Surface is not valid.");

            m_isDisposed = false;

            Width = bitmap.Width;
            Height = bitmap.Height;
            Depth = 1;

            RowPitch = bitmap.Pitch;
            SlicePitch = RowPitch * Height;

            if (!copyData)
            {
                m_ownData = false;

                Data = bitmap.DataPtr;
            }
            else
            {
                m_ownData = true;

                int sizeInBytes = SizeInBytes;
                Data = MemoryHelper.AllocateMemory(sizeInBytes);
                MemoryHelper.CopyMemory(Data, bitmap.DataPtr, sizeInBytes);

                AddGCMemoryPressure();
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="MipData"/> class.
        /// </summary>
        ~MipData()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            if (!m_isDisposed)
            {
                if (m_ownData)
                {
                    MemoryHelper.FreeMemory(Data);
                    RemoveGCMemoryPressure();
                }

                m_isDisposed = true;
            }
        }

        private void AddGCMemoryPressure()
        {
            GC.AddMemoryPressure(SizeInBytes);
        }

        private void RemoveGCMemoryPressure()
        {
            GC.RemoveMemoryPressure(SizeInBytes);
        }
    }
}
