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
        public List<Vector3> Texels { get; set; }
        public List<Vector3> Normals { get; set; }

        public Vector3[][] NormalsMap { get; set; }
        public Vector3[][] TexturesMap { get; set; }
        public Vector3[][] ReflectionsMap { get; set; }

        public float[][] MetalicMap { get; set; }
        public float[][] RoughnessMap { get; set; }
        public float[][] aoMap { get; set; }

        public Model(List<Vector4> points, List<List<Vector3>> faces, List<Vector3> normals, List<Vector3> texels, Vector3[][] reflectionsMap, Vector3[][] normalsMap, Vector3[][] texturesMap, Vector3[][] mraoMap)
        {
            Points = points;
            Faces = SplitFacesOnTriangles(faces);
            Normals = normals;
            Texels = texels;
            ReflectionsMap = reflectionsMap;
            TexturesMap = texturesMap;
            NormalsMap = normalsMap;

            MetalicMap = mraoMap.Select(maps => maps.Select(map => map.X).ToArray()).ToArray();
            RoughnessMap = mraoMap.Select(maps => maps.Select(map => map.Y).ToArray()).ToArray();
            aoMap = mraoMap.Select(maps => maps.Select(map => map.Z).ToArray()).ToArray();
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

            var newTexels = new List<Vector3>();
            foreach (var t in Texels)
            {
                newTexels.Add(t);
            }

            var newTextureMap = TexturesMap?.Select(tmList => tmList.Select(tm => new Vector3(tm.X, tm.Y, tm.Z)).ToArray())?.ToArray();
            var newNormalsMap = NormalsMap?.Select(nmList => nmList.Select(nm => new Vector3(nm.X, nm.Y, nm.Z)).ToArray())?.ToArray();
            var newReflectionsMap = ReflectionsMap?.Select(rmList => rmList.Select(rm => new Vector3(rm.X, rm.Y, rm.Z)).ToArray())?.ToArray();
            var mraoMap = MetalicMap.Zip(RoughnessMap, aoMap).Select(maps => maps.First.Zip(maps.Second, maps.Third).Select(map => new Vector3(map.First, map.Second, map.Third)).ToArray()).ToArray();

            return new Model(newPoints, newFaces, newNormals, newTexels, newReflectionsMap, newNormalsMap, newTextureMap, mraoMap);
        }
    }
}
