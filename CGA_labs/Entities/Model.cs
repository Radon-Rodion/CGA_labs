using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CGA_labs.Entities
{
    public class Model : ICloneable
    {
        public List<Vector4> Points { get; set; }
        public List<List<Vector3>> Faces { get; set; }
        public List<Vector3> Normals { get; set; }

        public Model(List<Vector4> points, List<List<Vector3>> faces, List<Vector3> normals)
        {
            Points = points;
            Faces = SplitFacesOnTriangles(faces);
            Normals = normals;
        }

        private static List<List<Vector3>> SplitFacesOnTriangles(List<List<Vector3>> faces)
        {
            List<List<Vector3>> triangleFaces = new();
            foreach (List<Vector3> face in faces)
            {
                if (face.Count < 3)
                {
                    throw new ArgumentException("The face should include 3 parameters.");
                }

                for (int i = 1; i < face.Count - 1; i++)
                {
                    List<Vector3> triangleFace = new()
                    {
                        face[0],
                        face[i],
                        face[i + 1]
                    };

                    triangleFaces.Add(triangleFace);
                }
            }

            return triangleFaces;
        }

        public object Clone()
        {
            var newPoints = new List<Vector4>();
            foreach(var p in Points)
            {
                newPoints.Add(new Vector4(p.X, p.Y, p.Z, p.W));
            }

            var newFaces = new List<List<Vector3>>();
            foreach(var f in Faces)
            {
                var newList = new List<Vector3>();
                foreach(var v in f)
                {
                    newList.Add(new Vector3(v.X, v.Y, v.Z));
                }
                newFaces.Add(newList);
            }

            var newNormals = new List<Vector3>();
            foreach(var n in Normals)
            {
                newNormals.Add(n);
            }
            return new Model(newPoints, newFaces, newNormals);
        }
    }
}
