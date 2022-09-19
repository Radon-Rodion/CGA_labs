using CGA_labs.Entities;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CGA_labs.Logic
{
    public static class VisualisationLogic
    {
        public static void ShowErrorMessage(string errorMessage)
        {
            string messageBoxText = $"Ошибка! {errorMessage}";
            string caption = "Error";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Error;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }

        public static void DrawModel(WriteableBitmap bitmap, Model model)
        {
            foreach(var face in model.Faces)
            DrawFace(bitmap, model, face);
        }

        private static void DrawFace(WriteableBitmap bitmap, Model model, List<Vector3> faces)
        {
            for (int i = 0; i < faces.Count - 1; i++)
            {
                DrawSide(bitmap, model, faces, i, i + 1);
            }

            DrawSide(bitmap, model, faces, 0, faces.Count - 1);
        }

        private static void DrawSide(WriteableBitmap bitmap, Model model, List<Vector3> face, int index1, int index2)
        {
            Pixel point1 = GetFacePoint(model, face, index1);
            Pixel point2 = GetFacePoint(model, face, index2);

            DrawLine(bitmap, point1, point2);
        }

        private static void DrawLine(WriteableBitmap bitmap, Pixel src, Pixel dest)
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
                DrawPixel(bitmap, p.X, p.Y, curZ);

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

             DrawPixel(bitmap, dest.X, dest.Y, dest.Z);
        }

        private static Pixel GetFacePoint(Model model, List<Vector3> face, int i)
        {
            int indexPoint = (int)face[i].X;
            Vector4 point = model.Points[indexPoint];

            return new Pixel((int)point.X, (int)point.Y, point.Z);
        }

        private static void DrawPixel(WriteableBitmap bitmap, int x, int y, float z)
        {
            byte blue = 0;
            byte green = 255;
            byte red = 0;
            byte alpha = 255;
            byte[] colorData = { blue, green, red, alpha };

            if (x > 0 && x < bitmap.PixelWidth &&
                y > 0 && y < bitmap.PixelHeight &&
                z > 0 && z < 1)
            {
                bitmap.WritePixels(new Int32Rect(x, y, 1, 1), colorData, 4, 0);
            }
        }
    }
}
