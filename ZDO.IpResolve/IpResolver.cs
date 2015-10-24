using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Net;
using System.Net.Sockets;

namespace ZDO.IpResolve
{
    public class IpResolver
    {
        /// <summary>
        /// Country codes; position in array is country ID.
        /// </summary>
        private readonly string[] countries;

        /// <summary>
        /// IPv4 ranges, sorted by start of range.
        /// </summary>
        private readonly IPv4Range[] ip4Ranges;

        /// <summary>
        /// Initializes resolver by parsing IPv4 and IPv6 range files.
        /// </summary>
        /// <param name="srIp4"></param>
        /// <param name="srIp6"></param>
        public IpResolver(StreamReader srIp4, StreamReader srIp6)
        {
            countries = loadCountries();
            ip4Ranges = loadIp4(srIp4);
        }

        /// <summary>
        /// Ctor: initializes from embedded resource (binary).
        /// </summary>
        public IpResolver()
        {
            countries = loadCountries();
            // Deserialization: counterpart of WriteBin.
            Assembly a = Assembly.GetExecutingAssembly();
            using (Stream s = a.GetManifestResourceStream("ZDO.IpResolve.Resources.ipv4.bin"))
            using (BinaryReader br = new BinaryReader(s))
            {
                UInt32 count = br.ReadUInt32();
                ip4Ranges = new IPv4Range[count];
                IPv4Range range;
                for (UInt32 i = 0; i != count; ++i)
                {
                    range.RangeFirst = br.ReadUInt32();
                    range.RangeLast = br.ReadUInt32();
                    range.CountryId = br.ReadByte();
                    ip4Ranges[i] = range;
                }
            }
        }

        /// <summary>
        /// Serializes IP range data in binary form.
        /// </summary>
        public void WriteBin(BinaryWriter bw)
        {
            // Number of ranges
            UInt32 count = (UInt32)ip4Ranges.Length;
            bw.Write(count);
            // Write each range
            for (UInt32 i = 0; i != count; ++i)
            {
                IPv4Range range = ip4Ranges[i];
                bw.Write(range.RangeFirst);
                bw.Write(range.RangeLast);
                bw.Write(range.CountryId);
            }
        }

        /// <summary>
        /// Returns code for unknown coutry.
        /// </summary>
        public string GetNoCountry()
        {
            return countries[countries.Length - 1];
        }

        /// <summary>
        /// Gets country code from an IP address.
        /// </summary>
        public string GetContryCode(IPAddress addr)
        {
            // IPv4
            if (addr.AddressFamily == AddressFamily.InterNetwork)
                return getCountryCodeIPv4(addr);
            // IPv6
            else if (addr.AddressFamily == AddressFamily.InterNetworkV6)
                return "IP6";
            // Bollocks
            return "ZYX";
        }

        /// <summary>
        /// Gets country code from an IPv4 address.
        /// </summary>
        private string getCountryCodeIPv4(IPAddress addr)
        {
            // Get IP address as unsigned 32-bit
            byte[] bytes = addr.GetAddressBytes();
            UInt32 val = 0;
            for (int i = 0; i != bytes.Length; ++i)
            {
                val <<= 8;
                val += bytes[i];
            }
            // Find largest RangeFirst that's not greater than value
            // Binary search on ranges sorted by their first values
            int bottom = 0;
            int top = ip4Ranges.Length - 1;
            int middle = top >> 1;
            while (top >= bottom)
            {
                if (ip4Ranges[middle].RangeFirst == val) break;
                if (ip4Ranges[middle].RangeFirst > val) top = middle - 1;
                else bottom = middle + 1;
                middle = (bottom + top) >> 1;
            }
            // We're looking for equal or nearest smaller
            while (middle > 0 && ip4Ranges[middle].RangeFirst > val) --middle;
            IPv4Range range = ip4Ranges[middle];
            // We just have a larger one: no country
            if (range.RangeFirst > val) return GetNoCountry();
            // We're actually within range: return that country
            if (range.RangeFirst <= val && range.RangeLast >= val) return countries[range.CountryId];
            // No country
            return GetNoCountry();
        }

        /// <summary>
        /// Reads IPv4 range file; returns sorted array, with contiguous same-country ranges merged.
        /// </summary>
        private IPv4Range[] loadIp4(StreamReader sr)
        {
            // Reserve
            List<IPv4Range> res = new List<IPv4Range>(180000);
            // Parse file
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line == string.Empty || line.StartsWith("#")) continue;
                string[] parts = getParts(line);
                UInt32 first = UInt32.Parse(parts[0]);
                UInt32 last = UInt32.Parse(parts[1]);
                string country = parts[5];
                byte countryId = getCountryId(country);
                res.Add(new IPv4Range { RangeFirst = first, RangeLast = last, CountryId = countryId });
            }
            // Sort by range starts
            res.Sort((a, b) => a.RangeFirst.CompareTo(b.RangeFirst));
            // Eliminate duplicates
            List<IPv4Range> cpy = new List<IPv4Range>(res.Count);
            cpy.Add(res[0]);
            for (int i = 1; i < res.Count; ++i)
            {
                IPv4Range curr = res[i];
                IPv4Range prev = cpy[cpy.Count - 1];
                // Current range is contiguous to previous one; same country too
                if (curr.CountryId == prev.CountryId && curr.RangeFirst == prev.RangeLast + 1)
                {
                    prev.RangeLast = curr.RangeLast;
                    cpy[cpy.Count - 1] = prev;
                }
                // Nop, add new item
                else cpy.Add(curr);
            }
            // Return redcued array (with contiguous country ranges merged)
            return cpy.ToArray();
        }

        /// <summary>
        /// Gets ID of country code;
        /// </summary>
        private byte getCountryId(string country)
        {
            for (byte b = 0; b <= 255; ++b)
            {
                if (countries[b] == country) return b;
            }
            return (byte)(countries.Length - 1);
        }

        /// <summary>
        /// Gets parts of a line from IP ranges file.
        /// </summary>
        private static string[] getParts(string line)
        {
            line = line.Replace("\",\"", "|");
            line = line.Replace("\"", "");
            return line.Split(new char[] { '|' });
        }

        /// <summary>
        /// Loads countries from embedded text file.
        /// </summary>
        private static string[] loadCountries()
        {
            List<string> res = new List<string>(256);
            Assembly a = Assembly.GetExecutingAssembly();
            using (Stream s = a.GetManifestResourceStream("ZDO.IpResolve.Resources.countries.txt"))
            using (StreamReader sr = new StreamReader(s))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line == string.Empty) continue;
                    res.Add(line);
                }
            }
            return res.ToArray();
        }
    }
}
