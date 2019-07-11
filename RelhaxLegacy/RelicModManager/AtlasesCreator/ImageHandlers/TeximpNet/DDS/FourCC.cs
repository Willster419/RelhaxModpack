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
    /// Represents a four character code (32-bit unsigned integer), usually used as a "magic number" to identify the contents of a file format.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 4)]
    internal struct FourCC : IEquatable<FourCC>
    {
        private static readonly FourCC s_empty = new FourCC(0);
        private static readonly int s_sizeInBytes = MemoryHelper.SizeOf<FourCC>();

        private uint m_packedValue;

        /// <summary>
        /// Gets the empty (a value of zero) four character code.
        /// </summary>
        public static FourCC Empty
        {
            get
            {
                return s_empty;
            }
        }

        /// <summary>
        /// Gets the size of the <see cref="FourCC"/> structure in bytes.
        /// </summary>
        public static int SizeInBytes
        {
            get
            {
                return s_sizeInBytes;
            }
        }

        /// <summary>
        /// Gets the first character.
        /// </summary>
        public char First
        {
            get
            {
                return (char)(m_packedValue & 255);
            }
        }

        /// <summary>
        /// Gets the second character.
        /// </summary>
        public char Second
        {
            get
            {
                return (char)((m_packedValue >> 8) & 255);
            }
        }

        /// <summary>
        /// Gets the third character.
        /// </summary>
        public char Third
        {
            get
            {
                return (char)((m_packedValue >> 16) & 255);
            }
        }

        /// <summary>
        /// Gets the fourth character.
        /// </summary>
        public char Fourth
        {
            get
            {
                return (char)((m_packedValue >> 24) & 255);
            }
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="FourCC"/> struct.
        /// </summary>
        /// <param name="fourCharacterCode">The string representation of a four character code.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the string representing a four character code does not infact have four characters.</exception>
        public FourCC(String fourCharacterCode)
        {
            if (fourCharacterCode != null)
            {
                if (fourCharacterCode.Length != 4)
                    throw new ArgumentOutOfRangeException("fourCharacterCode", "FourCC must have four characters only.");

                m_packedValue = (uint)((fourCharacterCode[3] << 24) | (fourCharacterCode[2] << 16) | (fourCharacterCode[1] << 8) | fourCharacterCode[0]);
            }
            else
            {
                m_packedValue = 0;
            }
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="FourCC"/> struct.
        /// </summary>
        /// <param name="first">First character</param>
        /// <param name="second">Second character</param>
        /// <param name="third">Third character</param>
        /// <param name="fourth">Fourth character</param>
        public FourCC(char first, char second, char third, char fourth)
        {
            m_packedValue = (uint)((((fourth << 24) | (third << 16)) | (second << 8)) | first);
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="FourCC"/> struct.
        /// </summary>
        /// <param name="packedValue">Packed value represent the four character code.</param>
        public FourCC(uint packedValue)
        {
            m_packedValue = packedValue;
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="FourCC"/> struct.
        /// </summary>
        /// <param name="packedValue">Packed value represent the four character code.</param>
        public FourCC(int packedValue)
        {
            m_packedValue = (uint)packedValue;
        }

        /// <summary>
        /// Implicitly converts the <see cref="FourCC"/> instance to an unsigned integer.
        /// </summary>
        /// <param name="fourCharacterCode">Character code</param>
        /// <returns>Unsigned integer representation.</returns>
        public static implicit operator uint(FourCC fourCharacterCode)
        {
            return fourCharacterCode.m_packedValue;
        }

        /// <summary>
        /// Implicitly converts the <see cref="FourCC"/> instance to an integer.
        /// </summary>
        /// <param name="fourCharacterCode">Character code</param>
        /// <returns>Integer representation</returns>
        public static implicit operator int(FourCC fourCharacterCode)
        {
            return (int)fourCharacterCode.m_packedValue;
        }

        /// <summary>
        /// Implicitly converts the <see cref="FourCC"/> instance to a String.
        /// </summary>
        /// <param name="fourCharacterCode">Character code</param>
        /// <returns>String representation</returns>
        public static implicit operator String(FourCC fourCharacterCode)
        {
            return new String(new char[] { fourCharacterCode.First, fourCharacterCode.Second, fourCharacterCode.Third, fourCharacterCode.Fourth });
        }

        /// <summary>
        /// Implicitly converts an unsigned integer to a <see cref="FourCC"/> instance.
        /// </summary>
        /// <param name="packedValue">Packed value representing the four character code.</param>
        /// <returns>The FourCC instance.</returns>
        public static implicit operator FourCC(uint packedValue)
        {
            return new FourCC(packedValue);
        }

        /// <summary>
        /// Implicitly converts an integer to a <see cref="FourCC"/> instance.
        /// </summary>
        /// <param name="packedValue">Packed value representing the four character code.</param>
        /// <returns>The FourCC instance.</returns>
        public static implicit operator FourCC(int packedValue)
        {
            return new FourCC(packedValue);
        }

        /// <summary>
        /// Implicitly converts a String to a <see cref="FourCC"/> instance.
        /// </summary>
        /// <param name="fourCharacterCode">String representing the four character code.</param>
        /// <returns>The FourCC instance.</returns>
        public static implicit operator FourCC(String fourCharacterCode)
        {
            return new FourCC(fourCharacterCode);
        }

        /// <summary>
        /// Tests equality between two character codes.
        /// </summary>
        /// <param name="a">First character code</param>
        /// <param name="b">Second character code</param>
        /// <returns>True if both are equal, false otherwise.</returns>
        public static bool operator ==(FourCC a, FourCC b)
        {
            return a.m_packedValue == b.m_packedValue;
        }

        /// <summary>
        /// Tests inequality between two character codes.
        /// </summary>
        /// <param name="a">First character code</param>
        /// <param name="b">Second character code</param>
        /// <returns>True if both are not equal, false otherwise.</returns>
        public static bool operator !=(FourCC a, FourCC b)
        {
            return a.m_packedValue != b.m_packedValue;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>True if the specified <see cref="System.Object" /> is equal to this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is FourCC)
                return Equals((FourCC)obj);

            return false;
        }

        /// <summary>
        /// Tests equality between this character code and another.
        /// </summary>
        /// <param name="other">Other character code</param>
        /// <returns>True if both are equal, false otherwise.</returns>
        public bool Equals(FourCC other)
        {
            return m_packedValue == other.m_packedValue;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (int)m_packedValue;
            }
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>A <see cref="T:System.String" /> containing a fully qualified type name.</returns>
        public override String ToString()
        {
            if (m_packedValue == 0)
                return "0";

            return new String(new char[] { First, Second, Third, Fourth });
        }
    }
}
