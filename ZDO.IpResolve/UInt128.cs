using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ZDO.IpResolve
{
    /// <summary>
    /// A 128 bit unsigned integer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UInt128 : IComparable<UInt128>, IEquatable<UInt128>
    {
        UInt64 Hi;
        UInt64 Lo;

        public static readonly UInt128 Zero = new UInt128(0, 0);

        public UInt128(UInt64 hi, UInt64 lo)
        {
            Hi = hi;
            Lo = lo;
        }

        public bool Equals(UInt128 other)
        {
            return Hi == other.Hi && Lo == other.Lo;
        }

        public override bool Equals(object obj)
        {
            return (obj is UInt128) && Equals((UInt128)obj);
        }

        public int CompareTo(UInt128 other)
        {
            if (Hi != other.Hi) return Hi.CompareTo(other.Hi);
            return Lo.CompareTo(other.Lo);
        }

        public static bool operator ==(UInt128 value1, UInt128 value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator !=(UInt128 value1, UInt128 value2)
        {
            return !(value1 == value2);
        }

        public static bool operator <(UInt128 value1, UInt128 value2)
        {
            return value1.CompareTo(value2) < 0;
        }

        public static bool operator >(UInt128 value1, UInt128 value2)
        {
            return value1.CompareTo(value2) > 0;
        }

        public static bool operator <=(UInt128 value1, UInt128 value2)
        {
            return value1.CompareTo(value2) <= 0;
        }

        public static bool operator >=(UInt128 value1, UInt128 value2)
        {
            return value1.CompareTo(value2) >= 0;
        }

        public static UInt128 operator >>(UInt128 value, int numberOfBits)
        {
            return RightShift(value, numberOfBits);
        }
    
        public static UInt128 operator <<(UInt128 value, int numberOfBits)
        {
            return LeftShift(value, numberOfBits);
        }

        public static UInt128 RightShift(UInt128 value, int numberOfBits)
        {
            if (numberOfBits >= 128)
                return Zero;
            if (numberOfBits >= 64)
                return new UInt128(0, value.Hi >> (numberOfBits - 64));
            if (numberOfBits == 0)
                return value;
            return new UInt128(value.Hi >> numberOfBits, (value.Lo >> numberOfBits) + (value.Hi << (64 - numberOfBits)));
        }

        public static UInt128 LeftShift(UInt128 value, int numberOfBits)
        {
            numberOfBits %= 128;
            if (numberOfBits >= 64)
                return new UInt128(value.Lo << (numberOfBits - 64), 0);
            if (numberOfBits == 0)
                return value;
            return new UInt128((value.Hi << numberOfBits) + (value.Lo >> (64 - numberOfBits)), value.Lo << numberOfBits);
        }

        public override int GetHashCode()
        {
            return Hi.GetHashCode() + Lo.GetHashCode();
        }
    }
}
