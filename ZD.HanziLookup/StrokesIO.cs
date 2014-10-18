using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZD.HanziLookup
{
    internal class StrokesIO
    {
        static public void WriteDirection(double direction, BinaryWriter w)
        {
            short directionShort = convertDirectionToShort(direction);
            w.Write(directionShort);
        }

        static public double ReadDirection(BinaryReader r)
        {
            short directionShort = (short)r.ReadInt16();
            double direction = convertDirectionFromShort(directionShort);
            return direction;
        }

        static public void WriteLength(double length, BinaryWriter w)
        {
            short lengthShort = convertLengthToShort(length);
            w.Write(lengthShort);
        }

        static public double ReadLength(BinaryReader r)
        {
            short lengthShort = (short)r.ReadInt16();
            double length = convertLengthFromShort(lengthShort);
            return length;
        }

        /*
          * Convert a short direction value written by StrokesParser.convertDirectionToShort.
          * We store directions with shorts to save a bit of memory since we don't need much percision.
          * Now we need to convert that value back to its original double.
          */
        static private double convertDirectionFromShort(short directionShort)
        {
            double directionRatio = ((double)directionShort + (double)short.MaxValue) / (double)short.MaxValue;
            double direction = directionRatio * 2 * Math.PI;
            return direction;
        }

        /*
         * Convert a short length value written by StrokesParser convertLengthToShort. 
         */
        static private double convertLengthFromShort(double lengthShort)
        {
            double length = (lengthShort + (double)short.MaxValue) / (double)short.MaxValue;
            return length;
        }

        /*
     * Converts the direction double to a short value that StrokesRepository#convertDirectionFromShort can read.
     */
        static private short convertDirectionToShort(double direction)
        {
            double ratio = direction / (2 * Math.PI);

            short directionShort = (short)((ratio * short.MaxValue) - short.MaxValue);
            return directionShort;
        }

        /*
         * Converts the length double to a short value that StrokesRepository#convertLengthFromShort can read.
         */
        static private short convertLengthToShort(double length)
        {
            short lengthShort = (short)((length * short.MaxValue) - short.MaxValue);
            return lengthShort;
        }
    }
}

