﻿using System;
using System.Runtime.InteropServices;

#pragma warning disable IDE0059

namespace BSS.Random
{
    public static partial class HWRandom
    {
        [StructLayout(LayoutKind.Explicit)]
        private ref struct UIntByteView
        {
            [FieldOffset(0)]
            internal UInt64 UInt64;

            [FieldOffset(0)]
            internal UInt32 UInt32;

            [FieldOffset(0)]
            internal UInt16 UInt16;

            //

            [FieldOffset(0)]
            internal Byte Byte1;

            [FieldOffset(1)]
            internal Byte Byte2;

            [FieldOffset(2)]
            internal Byte Byte3;

            [FieldOffset(3)]
            internal Byte Byte4;

            [FieldOffset(4)]
            internal Byte Byte5;

            [FieldOffset(5)]
            internal Byte Byte6;

            [FieldOffset(6)]
            internal Byte Byte7;

            [FieldOffset(7)]
            internal Byte Byte8;
        }

        /// <summary>
        /// Fills the in-buffer with RDRAND
        /// </summary>
        /// <remarks>Uses random bits from the pool, which is seeded by the conditioner. An upper bound of 511 128-bit samples will be generated per seed. That is, no more than 511*2=1022 sequential DRNG random numbers will be generated from the same seed value. - <see href="https://www.intel.com/content/www/us/en/developer/articles/guide/intel-digital-random-number-generator-drng-software-implementation-guide.html#inpage-nav-3-3">intel.com</see></remarks>
        /// <param name="buffer">array to operate on</param>
        /// <param name="offset">offset</param>
        /// <param name="count">count</param>
        /// <returns>Indicated whether the operation succeeded or not (<see langword="bool"></see> success = <see langword="true"></see>) | will only return false if instruction fails 128 times in a row</returns>
        public static Boolean NextBytes(Byte[] buffer, Int32 offset, Int32 count)
        {
            if (offset + count > buffer.Length) return false;

            UIntByteView memoryView = new();

            for (Int32 i = 0; offset + i < offset + count;)
            {
                if (offset + i + 8 <= offset + count)
                {
                    if (!ReadRandom64(memoryView.UInt64)) return false;

                    buffer[offset + i] = memoryView.Byte1;
                    buffer[offset + i + 1] = memoryView.Byte2;
                    buffer[offset + i + 2] = memoryView.Byte3;
                    buffer[offset + i + 3] = memoryView.Byte4;
                    buffer[offset + i + 4] = memoryView.Byte5;
                    buffer[offset + i + 5] = memoryView.Byte6;
                    buffer[offset + i + 6] = memoryView.Byte7;
                    buffer[offset + i + 7] = memoryView.Byte8;

                    i += 8;
                    continue;
                }

                if (offset + i + 4 <= offset + count)
                {
                    if (!ReadRandom32(memoryView.UInt32)) return false;

                    buffer[offset + i] = memoryView.Byte1;
                    buffer[offset + i + 1] = memoryView.Byte2;
                    buffer[offset + i + 2] = memoryView.Byte3;
                    buffer[offset + i + 3] = memoryView.Byte4;

                    i += 4;
                    continue;
                }

                if (offset + i + 2 <= offset + count)
                {
                    if (!ReadRandom16(memoryView.UInt16)) return false;

                    buffer[offset + i] = memoryView.Byte1;
                    buffer[offset + i + 1] = memoryView.Byte2;

                    i += 2;
                    continue;
                }

                if (!ReadRandom16(memoryView.UInt16)) return false;
                buffer[i] = memoryView.Byte1;
                ++i;
            }

            return true;
        }

        /// <summary>
        /// Fills the in-buffer with RDRAND
        /// </summary>
        /// <remarks>Uses random bits from the pool, which is seeded by the conditioner. An upper bound of 511 128-bit samples will be generated per seed.<br/>That is, no more than 511*2=1022 sequential DRNG random numbers will be generated from the same seed value. - <see href="https://www.intel.com/content/www/us/en/developer/articles/guide/intel-digital-random-number-generator-drng-software-implementation-guide.html#inpage-nav-3-3">intel.com</see></remarks>
        /// <param name="buffer">array to operate on</param>
        /// <returns>Indicated whether the operation succeeded or not (<see langword="bool"></see> success = <see langword="true"></see>) | will only return false if instruction fails 128 times in a row</returns>
        public static Boolean NextBytes(Span<Byte> buffer)
        {
            Int32 length = buffer.Length;
            UIntByteView memoryView = new();

            for (Int32 i = 0; i < length;)
            {
                if (i + 9 < length)
                {
                    if (!ReadRandom64(memoryView.UInt64)) return false;

                    buffer[i] = memoryView.Byte1;
                    buffer[i + 1] = memoryView.Byte2;
                    buffer[i + 2] = memoryView.Byte3;
                    buffer[i + 3] = memoryView.Byte4;
                    buffer[i + 4] = memoryView.Byte5;
                    buffer[i + 5] = memoryView.Byte6;
                    buffer[i + 6] = memoryView.Byte7;
                    buffer[i + 7] = memoryView.Byte8;

                    i += 8;
                    continue;
                }

                if (i + 5 < length)
                {
                    if (!ReadRandom32(memoryView.UInt32)) return false;

                    buffer[i] = memoryView.Byte1;
                    buffer[i + 1] = memoryView.Byte2;
                    buffer[i + 2] = memoryView.Byte3;
                    buffer[i + 3] = memoryView.Byte4;

                    i += 4;
                    continue;
                }

                if (i + 3 < length)
                {
                    if (!ReadRandom16(memoryView.UInt16)) return false;

                    buffer[i] = memoryView.Byte1;
                    buffer[i + 1] = memoryView.Byte2;

                    i += 2;
                    continue;
                }

                if (!ReadRandom16(memoryView.UInt16)) return false;
                buffer[i] = memoryView.Byte1;
                ++i;
            }

            return true;
        }

        // ##########################################################################################################################################################

        /// <summary>
        /// Fills the in-buffer with RDSEED
        /// </summary>
        /// <remarks>The seed values come directly from the entropy conditioner - <see href="https://www.intel.com/content/www/us/en/developer/articles/guide/intel-digital-random-number-generator-drng-software-implementation-guide.html#inpage-nav-5-8">intel.com</see></remarks>
        /// <param name="buffer">array to operate on</param>
        /// <param name="offset">offset</param>
        /// <param name="count">count</param>
        /// <returns>Indicated whether the operation succeeded or not (<see langword="bool"></see> success = <see langword="true"></see>) | will only return false if instruction fails 128 times in a row</returns>
        public static Boolean SeedNextBytes(Byte[] buffer, Int32 offset, Int32 count)
        {
            if (offset + count > buffer.Length) return false;

            UIntByteView memoryView = new();

            for (Int32 i = 0; offset + i < offset + count;)
            {
                if (offset + i + 8 <= offset + count)
                {
                    if (!ReadSeed64(memoryView.UInt64)) return false;

                    buffer[offset + i] = memoryView.Byte1;
                    buffer[offset + i + 1] = memoryView.Byte2;
                    buffer[offset + i + 2] = memoryView.Byte3;
                    buffer[offset + i + 3] = memoryView.Byte4;
                    buffer[offset + i + 4] = memoryView.Byte5;
                    buffer[offset + i + 5] = memoryView.Byte6;
                    buffer[offset + i + 6] = memoryView.Byte7;
                    buffer[offset + i + 7] = memoryView.Byte8;

                    i += 8;
                    continue;
                }

                if (offset + i + 4 <= offset + count)
                {
                    if (!ReadSeed32(memoryView.UInt32)) return false;

                    buffer[offset + i] = memoryView.Byte1;
                    buffer[offset + i + 1] = memoryView.Byte2;
                    buffer[offset + i + 2] = memoryView.Byte3;
                    buffer[offset + i + 3] = memoryView.Byte4;

                    i += 4;
                    continue;
                }

                if (offset + i + 2 <= offset + count)
                {
                    if (!ReadSeed16(memoryView.UInt16)) return false;

                    buffer[offset + i] = memoryView.Byte1;
                    buffer[offset + i + 1] = memoryView.Byte2;

                    i += 2;
                    continue;
                }

                if (!ReadSeed16(memoryView.UInt16)) return false;
                buffer[i] = memoryView.Byte1;
                ++i;
            }

            return true;
        }

        /// <summary>
        /// Fills the in-buffer with RDSEED
        /// </summary>
        /// <remarks>The seed values come directly from the entropy conditioner - <see href="https://www.intel.com/content/www/us/en/developer/articles/guide/intel-digital-random-number-generator-drng-software-implementation-guide.html#inpage-nav-5-8">intel.com</see></remarks>
        /// <param name="buffer">array to operate on</param>
        /// <returns>Indicated whether the operation succeeded or not (<see langword="bool"></see> success = <see langword="true"></see>) | will only return false if instruction fails 128 times in a row</returns>
        public static Boolean SeedNextBytes(Span<Byte> buffer)
        {
            Int32 length = buffer.Length;
            UIntByteView memoryView = new();

            for (Int32 i = 0; i < length;)
            {
                if (i + 9 < length)
                {
                    if (!ReadSeed64(memoryView.UInt64)) return false;

                    buffer[i] = memoryView.Byte1;
                    buffer[i + 1] = memoryView.Byte2;
                    buffer[i + 2] = memoryView.Byte3;
                    buffer[i + 3] = memoryView.Byte4;
                    buffer[i + 4] = memoryView.Byte5;
                    buffer[i + 5] = memoryView.Byte6;
                    buffer[i + 6] = memoryView.Byte7;
                    buffer[i + 7] = memoryView.Byte8;

                    i += 8;
                    continue;
                }

                if (i + 5 < length)
                {
                    if (!ReadSeed32(memoryView.UInt32)) return false;

                    buffer[i] = memoryView.Byte1;
                    buffer[i + 1] = memoryView.Byte2;
                    buffer[i + 2] = memoryView.Byte3;
                    buffer[i + 3] = memoryView.Byte4;

                    i += 4;
                    continue;
                }

                if (i + 3 < length)
                {
                    if (!ReadSeed16(memoryView.UInt16)) return false;

                    buffer[i] = memoryView.Byte1;
                    buffer[i + 1] = memoryView.Byte2;

                    i += 2;
                    continue;
                }

                if (!ReadSeed16(memoryView.UInt16)) return false;
                buffer[i] = memoryView.Byte1;
                ++i;
            }

            return true;
        }
    }
}