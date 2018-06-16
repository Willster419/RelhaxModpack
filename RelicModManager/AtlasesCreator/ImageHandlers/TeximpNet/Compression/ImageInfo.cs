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
namespace RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet.Compression
{
    /// <summary>
    /// Represents details about a single image such as dimensions, byte sizes, and the position of the image in a larger collection of images.
    /// </summary>
    public struct ImageInfo
    {
        /// <summary>
        /// Width of the image, in texels.
        /// </summary>
        public int Width;

        /// <summary>
        /// Height of the image, in texels.
        /// </summary>
        public int Height;

        /// <summary>
        /// Depth of the image, in texels. (for 2D/Cube images, this should always be one).
        /// </summary>
        public int Depth;

        /// <summary>
        /// For array textures and cubemap, the index of the image. Cubemaps are in order of: +X, -X, +Y, -Y, +Z, -Z.
        /// </summary>
        public int ArrayIndex;

        /// <summary>
        /// Mip level index.
        /// </summary>
        public int MipLevel;

        /// <summary>
        /// The total # of bytes for each scanline (row) of the image. Data may be padded for alignment purposes, so this may not necessarily be <see cref="Width"/> x sizeof(<see cref="RGBAQuad"/>).
        /// </summary>
        public int RowPitch;

        /// <summary>
        /// The total # of bytes that represents each depth slice of a 3D image. Data may be padded for alignment purposes, so this may not necessarily be <see cref="Width"/> x <see cref="Height"/> x sizeof(<see cref="RGBAQuad"/>).
        /// </summary>
        public int SlicePitch;

        /// <summary>
        /// Constructs a new <see cref="ImageInfo"/>.
        /// </summary>
        /// <param name="width">Width of the image, in texels.</param>
        /// <param name="height">Height of the image, in texels.</param>
        /// <param name="depth">Depth of the image, in texels.</param>
        /// <param name="arrayIndex">Array index of the image.</param>
        /// <param name="mipLevel">Mipmap index of the image.</param>
        /// <param name="rowPitch"># of bytes for each scanline of the image.</param>
        /// <param name="slicePitch"># of bytes for each depth slice of the 3D image.</param>
        public ImageInfo(int width, int height, int depth, int arrayIndex, int mipLevel, int rowPitch, int slicePitch)
        {
            Width = width;
            Height = height;
            Depth = depth;
            ArrayIndex = arrayIndex;
            MipLevel = mipLevel;
            RowPitch = rowPitch;
            SlicePitch = slicePitch;
        }

        /// <summary>
        /// Initializes a <see cref="ImageInfo"/> for a 2D image. The row pitch is automatically calculated without any padding assumptions.
        /// </summary>
        /// <param name="width">Width of the image, in texels.</param>
        /// <param name="height">Height of the image, in texels.</param>
        /// <param name="mipLevel">Optional mipmap index of the image.</param>
        /// <param name="arrayIndex">Optional array index of the image.</param>
        /// <returns>Populated image info.</returns>
        public static ImageInfo From2D(int width, int height, int mipLevel = 0, int arrayIndex = 0)
        {
            return From2D(width, height, 0, mipLevel, arrayIndex);
        }

        /// <summary>
        /// Initializes a <see cref="ImageInfo"/> for a 2D image.
        /// </summary>
        /// <param name="width">Width of the image, in texels.</param>
        /// <param name="height">Height of the image, in texels.</param>
        /// <param name="rowPitch"># of bytes per scanline, may be padded.</param>
        /// <param name="mipLevel">Optional mipmap index of the image.</param>
        /// <param name="arrayIndex">Optional array index of the image.</param>
        /// <returns>Populated image info.</returns>
        public static ImageInfo From2D(int width, int height, int rowPitch, int mipLevel = 0, int arrayIndex = 0)
        {
            //Calculate a default row pitch w/o any padding, assume size of the pixel is a 4-component RGBA color, 32-bits total
            if (rowPitch == 0)
                rowPitch = width * 4;

            ImageInfo info;
            info.Width = width;
            info.Height = height;
            info.Depth = 1;
            info.ArrayIndex = arrayIndex;
            info.MipLevel = mipLevel;
            info.RowPitch = rowPitch;
            info.SlicePitch = 0;

            return info;
        }

        /// <summary>
        /// Initializes a <see cref="ImageInfo"/> for a Cubemap image. The row pitch is automatically calculated without any padding assumptions.
        /// </summary>
        /// <param name="size">Width/height of the image, in texels.</param>
        /// <param name="face">Which cubemap face this image corresponds to.</param>
        /// <param name="mipLevel">Optional mipmap index of the image.</param>
        /// <returns>Populated image info.</returns>
        public static ImageInfo FromCube(int size, CubeMapFace face, int mipLevel = 0)
        {
            return From2D(size, size, 0, mipLevel, (face == CubeMapFace.None) ? 0 : (int)face);
        }

        /// <summary>
        /// Initializes a <see cref="ImageInfo"/> for a Cubemap image.
        /// </summary>
        /// <param name="size">Width/height of the image, in texels.</param>
        /// <param name="face">Which cubemap face this image corresponds to.</param>
        /// <param name="rowPitch"># of bytes per scanline, may be padded.</param>
        /// <param name="mipLevel">Optional mipmap index of the image.</param>
        /// <returns>Populated image info.</returns>
        public static ImageInfo FromCube(int size, CubeMapFace face, int rowPitch, int mipLevel = 0)
        {
            return From2D(size, size, rowPitch, mipLevel, (face == CubeMapFace.None) ? 0 : (int)face);
        }

        /// <summary>
        /// Initializes a <see cref="ImageInfo"/> for a 3D image. The row and slice pitch are automatically calculated without any padding assumptions.
        /// </summary>
        /// <param name="width">Width of the image, in texels.</param>
        /// <param name="height">Height of the image, in texels.</param>
        /// <param name="depth">Depth of the image, in texels.</param>
        /// <param name="mipLevel">Optional mipmap index of the image.</param>
        /// <returns>Populated image info.</returns>
        public static ImageInfo From3D(int width, int height, int depth, int mipLevel = 0)
        {
            return From3D(width, height, depth, 0, 0, mipLevel);
        }

        /// <summary>
        /// Initializes a <see cref="ImageInfo"/> for a 3D image.
        /// </summary>
        /// <param name="width">Width of the image, in texels.</param>
        /// <param name="height">Height of the image, in texels.</param>
        /// <param name="depth">Depth of the image, in texels.</param>
        /// <param name="rowPitch"># of bytes per scanline, may be padded.</param>
        /// <param name="slicePitch"># of bytes per depth slice, may be padded.</param>
        /// <param name="mipLevel">Optional mipmap index of the image.</param>
        /// <returns>Populated image info.</returns>
        public static ImageInfo From3D(int width, int height, int depth, int rowPitch, int slicePitch, int mipLevel = 0)
        {
            //Calculate a default row pitch or slice pitch w/o any padding, assume size of the pixel is a 4-component RGBA color, 32-bits total
            if (rowPitch == 0)
                rowPitch = width * 4;

            if (slicePitch == 0)
                slicePitch = width * height * 4;

            ImageInfo info;
            info.Width = width;
            info.Height = height;
            info.Depth = depth;
            info.ArrayIndex = 0;
            info.MipLevel = mipLevel;
            info.RowPitch = rowPitch;
            info.SlicePitch = slicePitch;

            return info;
        }
    }
}
