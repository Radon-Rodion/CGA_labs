using CGA_labs.Entities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

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
                            case "vn":
                                normals.Add(ToNormale(line));
                                break;
                        }
                }

                return new Model(points, faces, normals);
            }
            catch (Exception ex)
            {
                CommonVisualisationLogic.ShowErrorMessage(ex.Message);
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

        private static Vector3 ToNormale(string line)
        {
            string[] values = line.Replace('.', ',').Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
        }
    }
}
