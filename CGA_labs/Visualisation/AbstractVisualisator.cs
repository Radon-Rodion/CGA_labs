using CGA_labs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;

namespace CGA_labs.Visualisation
{
    public abstract class AbstractVisualisator
    {
        public abstract void DrawModel(WriteableBitmap bitmap, Model model);

        protected virtual void DrawFace(WriteableBitmap bitmap, Model model, List<Vector3> faces)
        {
            for (int i = 0; i < faces.Count - 1; i++)
            {
                DrawSide(bitmap, model, faces, i, i + 1);
            }

            DrawSide(bitmap, model, faces, 0, faces.Count - 1);
        }

        protected virtual void DrawSide(WriteableBitmap bitmap, Model model, List<Vector3> face, int index1, int index2)
        {
            Pixel point1 = GetFacePoint(model, face, index1);
            Pixel point2 = GetFacePoint(model, face, index2);

            ActionWithLine((pix) => DrawPixel(bitmap, pix), point1, point2);
        }

        protected virtual Pixel GetFacePoint(Model model, List<Vector3> face, int i)
        {
            int indexPoint = (int)face[i].X;
            Vector4 point = model.Points[indexPoint];

            return new Pixel((int)point.X, (int)point.Y, point.Z);
        }

        protected virtual void ActionWithLine(Action<Pixel> Act,Pixel src, Pixel dest)
        {
            int dx = Math.Abs(dest.X - src.X);
            int dy = Math.Abs(dest.Y - src.Y);
            float dz = Math.Abs(dest.Z - src.Z);

            int signX = src.X < dest.X ? 1 : -1;
            int signY = src.Y < dest.Y ? 1 : -1;
            float signZ = src.Z < dest.Z ? 1 : -1;

            Pixel p = src;

            float curZ = src.Z;
            float deltaZ = dz / dy;
            int err = dx - dy;
            while (p.X != dest.X || p.Y != dest.Y)
            {
                Act(new Pixel(p.X, p.Y, curZ));

                int err2 = err * 2;
                if (err2 > -dy)
                {
                    p.X += signX;
                    err -= dy;
                }

                if (err2 < dx)
                {
                    p.Y += signY;
                    curZ += signZ * deltaZ;
                    err += dx;
                }
            }

            Act(dest);
        }

        protected Func<byte[]> GetPixelColor = () =>
        {
            byte blue = 0;
            byte green = 255;
            byte red = 0;
            byte alpha = 255;
            byte[] colorData = { blue, green, red, alpha };
            return colorData;
        };

        protected virtual void DrawPixel(WriteableBitmap bitmap, Pixel pix)
        {
            var colorData = GetPixelColor();

            if (pix.X > 0 && pix.X < bitmap.PixelWidth &&
                pix.Y > 0 && pix.Y < bitmap.PixelHeight &&
                pix.Z > 0 && pix.Z < 1)
            {
                bitmap.WritePixels(new Int32Rect(pix.X, pix.Y, 1, 1), colorData, 4, 0);
            }
        }
    }
}
