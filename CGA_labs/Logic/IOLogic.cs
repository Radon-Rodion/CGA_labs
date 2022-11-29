using CGA_labs.Entities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CGA_labs.Logic
{
    public static class IOLogic
    {
        public static string ChooseFile()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Object files (*.obj) | *.obj";

            if (openFileDialog.ShowDialog() is not null)
            {
                return openFileDialog.FileName;
            }

            return null;
        }
        public static Model ReadObjFile(string fileAddress)
        {
            try
            {
                var fileLines = File.ReadAllLines(fileAddress, Encoding.UTF8);
                if (fileLines is null)
                {
                    throw new ArgumentNullException(nameof(fileLines));
                }

                var points = new List<Vector4>();
                var faces = new List<List<Vector3>>();
                var texels = new List<Vector3>();
                var normals = new List<Vector3>();
                foreach (var line in fileLines)
                {
                    if(line.Length > 2)
                        switch (line.Substring(0, 2))
                        {
                            case "v ":
                                points.Add(ToPoint(line));
                                break;
                            case "f ":
                                faces.Add(ToFace(line));
                                break;
                            case "vt":
                                texels.Add(ToNormaleOrTexel(line));
                                break;
                            case "vn":
                                normals.Add(ToNormaleOrTexel(line));
                                break;
                        }
                }
                var texturesMap = FileToPixelsMap(fileAddress.Replace(".obj", "_tex.png"), 0, 255);
                var normalsMap = FileToPixelsMap(fileAddress.Replace(".obj", "_nor.png"), -1, 1);
                var reflectionsMap = FileToPixelsMap(fileAddress.Replace(".obj", "_ref.png"), 0, 1);

                texturesMap ??= FileToPixelsMap(Path.Combine(Path.GetDirectoryName(fileAddress), "BaseColor Map.png"), 0, 1);
                normalsMap ??= FileToPixelsMap(Path.Combine(Path.GetDirectoryName(fileAddress), "Normal Map.png"), -1, 1);
                var mraoMap = FileToPixelsMap(Path.Combine(Path.GetDirectoryName(fileAddress), "MRAO Map.png"), 0, 1);
                return new Model(points, faces, normals, texels, reflectionsMap, normalsMap, texturesMap, mraoMap);
            }
            catch (ApplicationException ex)
            {
                CommonVisualisationLogic.ShowErrorMessage(ex.Message);
                return null;
            }
        }

        private static Vector3[][] FileToPixelsMap(string filePath, float minValue, float maxValue)
        {
            try
            {
                Bitmap tempBmp; //https://habr.com/ru/post/196578/?ysclid=la8lulkcsh735462120
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    tempBmp = new Bitmap(fs);
                int width = tempBmp.Width,
                height = tempBmp.Height;
                var res = new Vector3[height][];
                BitmapData bd = tempBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                try
                {
                    unsafe
                    {
                        byte* curpos;
                        for (int h = 0; h < height; h++)
                        {
                            res[h] = new Vector3[width];
                            curpos = ((byte*)bd.Scan0) + h * bd.Stride;
                            for (int w = 0; w < width; w++)
                            {
                                var components = new float[3];
                                for (int i = 0; i < 3; i++)
                                {
                                    components[i] = (*(curpos++) / 255f) * (maxValue - minValue) + minValue;
                                }
                                res[h][w] = new Vector3(components[2], components[1], components[0]);
                            }
                        }
                    }
                }
                finally
                {
                    tempBmp.UnlockBits(bd);
                }
                return res;
            } catch (Exception e)
            {
                return null;
            }
        }

        private static List<Vector3> ToFace(string line)
        {
            var res = new List<Vector3>();
            string[] values = line.Replace("//","/0/").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < values.Length; i++)
            {
                string[] parameters = values[i].Split(new char[] { ' ', '/' }, StringSplitOptions.RemoveEmptyEntries);
                var v = new Vector3(float.Parse(parameters[0]) - 1, float.Parse(parameters[1]) - 1, float.Parse(parameters[2]) - 1);
                res.Add(v);
            }

            return res;
        }

        private static Vector4 ToPoint(string line)
        {
            string[] values = line.Replace('.',',').Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return new Vector4(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]), 1f);
        }

        private static Vector3 ToNormaleOrTexel(string line)
        {
            string[] values = line.Replace("  ", " ").Replace('.', ',').Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values.Length > 3 ? values[3] : "0"));
        }
    }
}
