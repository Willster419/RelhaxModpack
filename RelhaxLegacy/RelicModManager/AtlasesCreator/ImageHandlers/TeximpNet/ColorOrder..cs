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

namespace RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet
{
    /// <summary>
    /// Defines the default color order properties that FreeImage expects image data to conform to by default.
    /// </summary>
    public struct ColorOrder
    {
        /// <summary>
        /// Little endian BGRA red mask.
        /// </summary>
        public const uint LE_BGRA_RED_MASK = 0x00FF0000;

        /// <summary>
        /// Little endian BGRA green mask.
        /// </summary>
        public const uint LE_BGRA_GREEN_MASK = 0x0000FF00;

        /// <summary>
        /// Little endian BGRA blue mask.
        /// </summary>
        public const uint LE_BGRA_BLUE_MASK = 0x000000FF;

        /// <summary>
        /// Little endian BGRA alpha mask.
        /// </summary>
        public const uint LE_BGRA_ALPHA_MASK = 0xFF000000;

        /// <summary>
        /// Little endian RGBA red mask.
        /// </summary>
        public const uint LE_RGBA_RED_MASK = 0x000000FF;

        /// <summary>
        /// Little endian RGBA green mask.
        /// </summary>
        public const uint LE_RGBA_GREEN_MASK = 0x0000FF00;

        /// <summary>
        /// Little endian RGBA blue mask.
        /// </summary>
        public const uint LE_RGBA_BLUE_MASK = 0x00FF0000;

        /// <summary>
        /// Little endian RGBA alpha mask.
        /// </summary>
        public const uint LE_RGBA_ALPHA_MASK = 0xFF000000;

        /// <summary>
        /// Big endian BGRA red mask.
        /// </summary>
        public const uint BE_BGRA_RED_MASK = 0x000000FF;

        /// <summary>
        /// Big endian BGRA green mask.
        /// </summary>
        public const uint BE_BGRA_GREEN_MASK = 0x0000FF00;

        /// <summary>
        /// Big endian BGRA blue mask.
        /// </summary>
        public const uint BE_BGRA_BLUE_MASK = 0x00FF0000;

        /// <summary>
        /// Big endian BGRA alpha mask.
        /// </summary>
        public const uint BE_BGRA_ALPHA_MASK = 0xFF000000;

        /// <summary>
        /// Big endian RGBA red mask.
        /// </summary>
        public const uint BE_RGBA_RED_MASK = 0xFF000000;

        /// <summary>
        /// Big endian RGBA green mask.
        /// </summary>
        public const uint BE_RGBA_GREEN_MASK = 0x00FF0000;

        /// <summary>
        /// Big endian RGBA blue mask.
        /// </summary>
        public const uint BE_RGBA_BLUE_MASK = 0x0000FF00;

        /// <summary>
        /// Big endian RGBA alpha mask.
        /// </summary>
        public const uint BE_RGBA_ALPHA_MASK = 0x000000FF;

        /// <summary>
        /// Byte index of the Red channel.
        /// </summary>
        public uint RedIndex;

        /// <summary>
        /// Byte index of the Green channel.
        /// </summary>
        public uint GreenIndex;

        /// <summary>
        /// Byte index of the Blue channel.
        /// </summary>
        public uint BlueIndex;

        /// <summary>
        /// Byte index of the Alpha channel.
        /// </summary>
        public uint AlphaIndex;

        /// <summary>
        /// Mask to apply to 32-bit integer to get just the red component.
        /// </summary>
        public uint RedMask;

        /// <summary>
        /// Mask to apply to 32-bit integer to get just the green component.
        /// </summary>
        public uint GreenMask;

        /// <summary>
        /// Mask to apply to 32-bit integer to get just the blue component.
        /// </summary>
        public uint BlueMask;

        /// <summary>
        /// Mask to apply to 32-bit integer to get just the alpha component.
        /// </summary>
        public uint AlphaMask;

        /// <summary>
        /// Shift to apply to 32-bit integer to get just the red component.
        /// </summary>
        public uint RedShift;

        /// <summary>
        /// Shift to apply to 32-bit integer to get just the green component.
        /// </summary>
        public uint GreenShift;

        /// <summary>
        /// Shift to apply to 32-bit integer to get just the blue component.
        /// </summary>
        public uint BlueShift;

        /// <summary>
        /// Shift to apply to 32-bit integer to get just the alpha component.
        /// </summary>
        public uint AlphaShift;

        /// <summary>
        /// Whether the color order is BGRA or RGBA.
        /// </summary>
        public bool IsBGRAOrder;

        /// <summary>
        /// Constructs a new <see cref="ColorOrder"/>.
        /// </summary>
        /// <param name="isLittleEndian">True if the platform is little endian, false if big endian.</param>
        public ColorOrder(bool isLittleEndian)
        {
            IsBGRAOrder = isLittleEndian;

            //Assume endianness is coupled with BGRA order...it can be changed, but the library doesn't give us a nice way of checking it.
            if (isLittleEndian)
            {
                RedIndex = 2;
                GreenIndex = 1;
                BlueIndex = 0;
                AlphaIndex = 3;

                RedMask = LE_BGRA_RED_MASK;
                GreenMask = LE_BGRA_GREEN_MASK;
                BlueMask = LE_BGRA_BLUE_MASK;
                AlphaMask = LE_BGRA_ALPHA_MASK;

                RedShift = 16;
                GreenShift = 8;
                BlueShift = 0;
                AlphaShift = 24;
            }
            else
            {
                RedIndex = 2;
                GreenIndex = 1;
                BlueIndex = 0;
                AlphaIndex = 3;

                RedMask = BE_BGRA_RED_MASK;
                GreenMask = BE_BGRA_GREEN_MASK;
                BlueMask = BE_BGRA_BLUE_MASK;
                AlphaMask = BE_BGRA_ALPHA_MASK;

                RedShift = 8;
                GreenShift = 16;
                BlueShift = 24;
                AlphaShift = 0;
            }
        }
    }
}
