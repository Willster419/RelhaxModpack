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
using System.Runtime.InteropServices;

namespace RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet
{
    /// <summary>
    /// Represents a 32-bit RGBA color in that order (8 bits per channel).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RGBAQuad
    {
        /// <summary>
        /// Red component.
        /// </summary>
        public byte R;

        /// <summary>
        /// Green component.
        /// </summary>
        public byte G;

        /// <summary>
        /// Blue component.
        /// </summary>
        public byte B;

        /// <summary>
        /// Alpha component.
        /// </summary>
        public byte A;

        /// <summary>
        /// Constructs a new instance of the <see cref="RGBAQuad"/> struct.
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        /// <param name="a">Alpha component.</param>
        public RGBAQuad(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Converts to BGRA order.
        /// </summary>
        /// <returns>BGRA ordered color value.</returns>
        public BGRAQuad ToBGRA()
        {
            return new BGRAQuad(B, G, R, A);
        }

        /// <summary>
        /// Converts to BGRA order.
        /// </summary>
        /// <param name="color">BGRA ordered color value</param>
        public void ToBGRA(out BGRAQuad color)
        {
            color = new BGRAQuad(B, G, R, A);
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>A <see cref="T:System.String" /> containing a fully qualified type name.</returns>
        public override String ToString()
        {
            return String.Format("R: {0}, G: {1}, B: {2}, A: {3}", R, G, B, A);
        }
    }

    /// <summary>
    /// Represents a 32-bit BGRA color in that order (8 bits per channel).
    /// </summary>
    public struct BGRAQuad
    {
        /// <summary>
        /// Blue component.
        /// </summary>
        public byte B;

        /// <summary>
        /// Green component.
        /// </summary>
        public byte G;

        /// <summary>
        /// Red component.
        /// </summary>
        public byte R;

        /// <summary>
        /// Alpha component.
        /// </summary>
        public byte A;

        /// <summary>
        /// Constructs a new instance of the <see cref="BGRAQuad"/> struct.
        /// </summary>
        /// <param name="b">Blue component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="r">Red component.</param>
        /// <param name="a">Alpha component.</param>
        public BGRAQuad(byte b, byte g, byte r, byte a)
        {
            B = b;
            G = g;
            R = r;
            A = a;
        }

        /// <summary>
        /// Converts to RGBA order.
        /// </summary>
        /// <returns>RGBA ordered color value.</returns>
        public RGBAQuad ToRGBA()
        {
            return new RGBAQuad(R, G, B, A);
        }

        /// <summary>
        /// Converts to RGBA order.
        /// </summary>
        /// <param name="color">RGBA ordered color value.</param>
        public void ToRGBA(out RGBAQuad color)
        {
            color = new RGBAQuad(R, G, B, A);
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>A <see cref="T:System.String" /> containing a fully qualified type name.</returns>
        public override String ToString()
        {
            return String.Format("B: {0}, G: {1}, R: {2}, A: {3}", B, G, R, A);
        }
    }
}
