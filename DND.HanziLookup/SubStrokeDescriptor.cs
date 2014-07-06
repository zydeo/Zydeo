using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.HanziLookup
{
    public class SubStrokeDescriptor
    {
        public SubStrokeDescriptor(double direction, double length)
        {
            Direction = direction;
            Length = length;
        }

        public readonly double Direction;
        public readonly double Length;
    }
}
