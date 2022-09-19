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

        public Model(List<Vector4> points, List<List<Vector3>> faces)
        {
            Points = points;
            Faces = faces;
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
            return new Model(newPoints, Faces);
        }
    }
}
