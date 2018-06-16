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
using System.IO;

namespace RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet.DDS
{
    /// <summary>
    /// Represents a set of texture images that was loaded from a DDS file format. A number of texture types are supported, such as 1D, 2D, and 3D image data. Each <see cref="MipChain"/>
    /// collection represents a complete mipmap chain of a single face (e.g. 6 of these chains make up a cubemap). Most textures will just have a single mipmap chain.
    /// </summary>
    [DebuggerDisplay("Dimension = {Dimension}, Format = {Format}, ArrayCount = {MipChains.Count}, MipCount = {MipChains.Count == 0 ? 0 : MipChains[0].Count}")]
    public sealed class DDSContainer : IDisposable
    {
        private bool m_isDisposed;
        private List<MipChain> m_mipChains;
        private DXGIFormat m_format;
        private TextureDimension m_dimension;

        /// <summary>
        /// Gets or sets the texture dimension. Cubemaps must have six entries in <see cref="MipChains"/>.
        /// </summary>
        public TextureDimension Dimension
        {
            get
            {
                return m_dimension;
            }
            set
            {
                m_dimension = value;
            }
        }

        /// <summary>
        /// Gets or sets the texture format. All surfaces must have the same format.
        /// </summary>
        public DXGIFormat Format
        {
            get
            {
                return m_format;
            }
            set
            {
                m_format = value;
            }
        }

        /// <summary>
        /// Gets the collection of mipmap chains. Typically there will be a single mipmap chain (sometimes just containing one image, if no mipmaps). Cubemaps must have six mipmap chains,
        /// and array textures may have any number.
        /// </summary>
        public List<MipChain> MipChains
        {
            get
            {
                return m_mipChains;
            }
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="DDSContainer"/> class.
        /// </summary>
        public DDSContainer() : this(new List<MipChain>(), DXGIFormat.Unknown, TextureDimension.Two) { }

        /// <summary>
        /// Constructs a new instance of the <see cref="DDSContainer"/> class.
        /// </summary>
        /// <param name="format">Format of the image data.</param>
        /// <param name="texDim">Identifies the dimensions of the image data.</param>
        public DDSContainer(DXGIFormat format, TextureDimension texDim) : this(new List<MipChain>(), format, texDim) { }

        /// <summary>
        /// Constructs a new instance of the <see cref="DDSContainer"/> class.
        /// </summary>
        /// <param name="mipChains">Collection of mipmap chains.</param>
        /// <param name="format">Format of the image data.</param>
        /// <param name="texDim">Identifies the dimensions of the image data.</param>
        public DDSContainer(List<MipChain> mipChains, DXGIFormat format, TextureDimension texDim)
        {
            m_mipChains = mipChains;
            m_format = format;
            m_dimension = texDim;
            m_isDisposed = false;
        }

        /// <summary>
        /// Validates the contained mipmap surfaces, e.g. all array main images must be the same dimensions and have the same number of mipmaps, cubemaps must have 6 faces, data/sizes must at least be in valid ranges, etc.
        /// </summary>
        /// <returns>True if the image data is not correctly initialized, false if it passes some basic checks.</returns>
        public bool Validate()
        {
            return DDSTypes.ValidateInternal(m_mipChains, m_format, m_dimension);
        }

        /// <summary>
        /// Writes images to a DDS file to disk.
        /// </summary>
        /// <param name="fileName">File to write to. If it doesn't exist, it will be created.</param>
        /// <param name="flags">Flags to control how the DDS data is saved.</param>
        /// <returns>True if writing the data was successful, false if otherwise.</returns>
        public bool Write(String fileName, DDSFlags flags = DDSFlags.None)
        {
            return DDSTypes.Write(fileName, m_mipChains, m_format, m_dimension, flags);
        }

        /// <summary>
        /// Writes images contained as DDS formatted data to a stream.
        /// </summary>
        /// <param name="output">Output stream.</param>
        /// <param name="flags">Flags to control how the DDS data is saved.</param>
        /// <returns>True if writing the data was successful, false if otherwise.</returns>
        public bool Write(Stream output, DDSFlags flags = DDSFlags.None)
        {
            return DDSTypes.Write(output, m_mipChains, m_format, m_dimension, flags);
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
                if (isDisposing)
                    DDSTypes.DisposeMipChains(m_mipChains);

                m_isDisposed = true;
            }
        }
    }
}
