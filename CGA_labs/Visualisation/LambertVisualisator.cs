using CGA_labs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CGA_labs.Visualisation
{
    public class LambertVisualisator: AbstractVisualisator
    {
        private float[,] _zBuffer;

        public override void DrawModel(WriteableBitmap bitmap, Model model)
        {
            _zBuffer = new float[(int)bitmap.Width, (int)bitmap.Height];
            for(int i = 0; i < _zBuffer.GetLength(0); i++)
            {
                for(int j = 0; j < _zBuffer.GetLength(1); j++)
                {
                    _zBuffer[i, j] = 10;
                }
            }

            foreach (var face in model.Faces)
            {
                var normal = GetNormal(model, face);
                if (IsTriangleVisible(normal))//
                {
                    DrawFace(bitmap, model, face, normal);
                }
            }
        }

        private struct BesenhamVariables
        {
            public int yGrowing;
            public float dz;
            public float z;
            public int err;
            public int y;
            public int dy;
            public int dx;

            public BesenhamVariables(Pixel pointFrom, Pixel pointTo)
            {
                yGrowing = pointTo.Y > pointFrom.Y ? 1 : -1;
                dz = (pointTo.Z - pointFrom.Z) / (pointTo.X - pointFrom.X);
                z = pointFrom.Z;
                err = 0;
                y = pointFrom.Y;
                dy = pointTo.Y - pointFrom.Y;
                dx = pointTo.X - pointFrom.X;
            }

            public void IncrementX()
            {
                err += yGrowing * dy;
                while (err > dx)
                {
                    err -= dx;
                    y+= yGrowing;
                }
            }
        }

        private byte[] GetColorFromNormale(Vector3 normal)
        {
                byte blue = 0;;
                byte green = (byte)(255* Math.Max(Vector3.Dot(normal, Vector3.UnitZ), 0));
                byte red = 0;
                byte alpha = 255;
                byte[] colorData = { blue, green, red, alpha };
                return colorData;
        }

        private void DrawFace(WriteableBitmap bitmap, Model model, List<Vector3> face, Vector3 normal)
        {
            GetPixelColor = () => GetColorFromNormale(normal);

            var points = new Pixel[3];
            for (int i = 0; i < 3; i++)
                points[i] = GetFacePoint(model, face, i);
            points = points.OrderBy(p => p.X).ToArray();

            var besenham01 = new BesenhamVariables(points[0], points[1]);
            var besenham02 = new BesenhamVariables(points[0], points[2]);
            var besenham12 = new BesenhamVariables(points[1], points[2]);

            var k2 = besenham02.dx != 0 ? ((float)besenham02.dy) / besenham02.dx : besenham02.dy;
            var k1 = besenham01.dx != 0 ? ((float)besenham01.dy) / besenham01.dx : besenham01.dy;
            var dy = k2 > k1 ? 1 : -1;

            for (int x = points[0].X; x < points[1].X; x++)
            {
                var z = besenham01.z;
                var dz = (besenham01.y - besenham02.y) != 0 ? (besenham01.z - besenham02.z) / (besenham01.y - besenham02.y) : 0;
                for (int y = besenham01.y; dy * y <= dy * besenham02.y; y += dy)
                {
                    z += dz;
                    if(x>=0 && x<bitmap.Width && y>=0 && y<bitmap.Height && z < _zBuffer[x, y])
                    {
                        _zBuffer[x, y] = z;
                        DrawPixel(bitmap, new Pixel(x, y, z));
                    }
                }

                besenham01.IncrementX();
                besenham02.IncrementX();
            }


            /**/
            for (int x = points[1].X; x < points[2].X; x++)
            {
                var z = besenham12.z;
                var dz = (besenham12.y - besenham02.y) != 0 ? (besenham12.z - besenham02.z) / (besenham12.y - besenham02.y) : 0;
                for (int y = besenham12.y; dy * y <= dy * besenham02.y; y += dy)
                {
                    z += dz;
                    if (x >= 0 && x < bitmap.Width && y >= 0 && y < bitmap.Height && z < _zBuffer[x, y])
                    {
                        _zBuffer[x, y] = z;
                        DrawPixel(bitmap, new Pixel(x, y, z));
                    }
                }

                besenham12.IncrementX();
                besenham02.IncrementX();
            }/**/
        }

        private Vector3 GetNormal(Model model, List<Vector3> triangle)
        {
            Vector3 normal1 = model.Normals[(int)triangle[0].Z];
            Vector3 normal2 = model.Normals[(int)triangle[1].Z];
            Vector3 normal3 = model.Normals[(int)triangle[2].Z];

            return Vector3.Normalize(normal1 + normal2 + normal3);

            /*Pixel point1 = GetFacePoint(model, triangle, 0);
            Pixel point2 = GetFacePoint(model, triangle, 1);
            Pixel point3 = GetFacePoint(model, triangle, 2);

            var side1 = new Vector3(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
            var side2 = new Vector3(point3.X - point1.X, point3.Y - point1.Y, point3.Z - point1.Z);
            return Vector3.Normalize(Vector3.Cross(side1, side2));*/
        }

        private bool IsTriangleVisible(Vector3 triangleNormal)
        {
            return triangleNormal.Z > 0;
        }
    }
}
