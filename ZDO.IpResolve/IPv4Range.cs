using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ZDO.IpResolve
{
    /// <summary>
    /// Represents one IPv4 range, and its associated country ID.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct IPv4Range
    {
        /// <summary>
        /// First address in range
        /// </summary>
        public UInt32 RangeFirst;
        /// <summary>
        /// Last address in range
        /// </summary>
        public UInt32 RangeLast;
        /// <summary>
        /// ID of country for this range
        /// </summary>
        public byte CountryId;
    }
}
