using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CGA_labs.Entities
{
    public struct Pixel
    {
        public int X { get; set; }
        public int Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public Pixel(int x, int y, float z, float w = 0)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }
}
