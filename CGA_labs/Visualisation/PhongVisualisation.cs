using CGA_labs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace CGA_labs.Visualisation
{
    public class PhongVisualisation : AbstractVisualisator
    {
        private Vector3 _lightVector;
        private Func<List<Vector3>, int, Vector3> _cameraVector;
        private float[,] _zBuffer;
        public override void DrawModel(WriteableBitmap bitmap, Model model, ModelParams parameters, Model worldModel)
        {
            var cameraGlobalVector = new Vector3(parameters.CameraPositionX, parameters.CameraPositionY, parameters.CameraPositionZ);
            _cameraVector = (face, index) =>
            {
                int indexPoint = (int)face[index].X;
                Vector4 point = worldModel.Points[indexPoint]; //TODO: tink about blicks
                var facePoint = new Vector3(point.X, point.Y, point.Z);

                return Vector3.Normalize(cameraGlobalVector - facePoint);
            };
            _lightVector = Vector3.UnitZ;

            _zBuffer = new float[(int)bitmap.Width, (int)bitmap.Height];
            for (int i = 0; i < _zBuffer.GetLength(0); i++)
            {
                for (int j = 0; j < _zBuffer.GetLength(1); j++)
                {
                    _zBuffer[i, j] = 10;
                }
            }
            foreach (var face in model.Faces)
            {
                DrawFace(bitmap, model, face);
            }
        }

        private (int r, int g, int b) GetAmbientLighting()
        {
            return (15, 50, 15);
        }

        private (int r, int g, int b) GetDiffuseLighting(Vector3 normalInPoint)
        {
            var k = Vector3.Dot(normalInPoint, _lightVector);
            k = k > 0 ? k : 0;
            return ((int)(50 * k), (int)(150 * k), (int)(50 * k));
        }

        private (int r, int g, int b) GetSpecularLighting(Vector3 normalInPoint, Vector3 cameraVector)
        {
            var vectorR = _lightVector - 2 * Vector3.Dot(_lightVector, normalInPoint) * normalInPoint;
            var k = Vector3.Dot(-vectorR, cameraVector)>0 ? Math.Pow(Vector3.Dot(-vectorR, cameraVector), 0.5) : 0;
            return ((int)(150 * k), (int)(130 * k), (int)(150 * k));
        }

        private byte[] GetColorFromNormaleLightAndCamera(Vector3 normalInPoint, Vector3 cameraVector)
        {
            var ambient = GetAmbientLighting();
            var diffuse = GetDiffuseLighting(normalInPoint);
            var specular = GetSpecularLighting(normalInPoint, cameraVector);

            byte blue = (byte)(Math.Min(Math.Max(ambient.b + diffuse.b + specular.b, 0), 255));
            byte green = (byte)(Math.Min(Math.Max(ambient.g + diffuse.g + specular.g, 0), 255));
            byte red = (byte)(Math.Min(Math.Max(ambient.r + diffuse.r + specular.r, 0), 255));
            byte alpha = 255;
            byte[] colorData = { blue, green, red, alpha };
            return colorData;
        }

        private struct PointNormalCam
        {
            public Vector3 Normal;
            public Vector3 Point;
            public Vector3 Camera;
        }

        private List<PointNormalCam> GetNormalsPointsAndCameraVectors(Model model, List<Vector3> triangle)
        {
            var result = new List<PointNormalCam>();
            for(int i = 0; i < triangle.Count; i++)
            {
                var pNC = new PointNormalCam();
                pNC.Normal = model.Normals[(int)triangle[i].Z];
                var point = model.Points[(int)triangle[i].X];
                pNC.Point = new Vector3(point.X, point.Y, point.Z);
                pNC.Camera = _cameraVector(triangle, i);
                result.Add(pNC);
            }

            return result;
        }

        private struct LineParams
        {
            public float dx0;
            public float dy0;
            public float dz;
            public float z;
            public float x;
            public float y;
            public Vector3 dNormal;
            public Vector3 dCamera;

            public Vector3 normal;
            public Vector3 camera;

            public LineParams(PointNormalCam from, PointNormalCam to)
            {
                dz = (to.Point.Z - from.Point.Z) / (to.Point.Y - from.Point.Y);
                z = from.Point.Z;
                x = from.Point.X;
                y = (float)Math.Ceiling(from.Point.Y);
                dy0 = to.Point.Y - from.Point.Y;
                dx0 = to.Point.X - from.Point.X;
                dNormal = (to.Normal - from.Normal) / (to.Point.Y - from.Point.Y);
                dCamera = (to.Camera - from.Camera) / (to.Point.Y - from.Point.Y);
                normal = from.Normal;
                camera = from.Camera;
            }

            public void IncrementY()
            {
                normal += dNormal;
                camera += dCamera;
                z += dz;
                x += dx0/dy0;
                y++;
            }
        }

        protected override void DrawFace(WriteableBitmap bitmap, Model model, List<Vector3> face)
        {
            var pNCsList = GetNormalsPointsAndCameraVectors(model, face);
            var pNCsArr = pNCsList.OrderBy(pNC => pNC.Point.Y).ToArray();

            var line01 = new LineParams(pNCsArr[0], pNCsArr[1]);
            var line02 = new LineParams(pNCsArr[0], pNCsArr[2]);
            var line12 = new LineParams(pNCsArr[1], pNCsArr[2]);

            var k2 = line02.dy0 != 0 ? ((float)line02.dx0) / line02.dy0 : line02.dx0;
            var k1 = line01.dy0 != 0 ? ((float)line01.dx0) / line01.dy0 : line01.dx0;
            var dx = k2 > k1 ? 1 : -1;

            while(line01.y <= Math.Floor(pNCsArr[1].Point.Y))
            {
                var dz = (line01.x - line02.x) != 0 ? (line01.z - line02.z) / (line01.x - line02.x) : 0;
                var dNormal = (line01.normal - line02.normal) != Vector3.Zero ? (line01.normal - line02.normal) / (line01.x - line02.x) : Vector3.Zero;
                var dCamera = (line01.camera - line02.camera) != Vector3.Zero ? (line01.camera - line02.camera) / (line01.x - line02.x) : Vector3.Zero;
                for (int x = (int)line01.x; dx * x <= dx * line02.x; x += dx)
                {
                    var z = line01.z+(x-line01.x)*dz;
                    var normal = line01.normal + (x - line01.x) * dNormal;
                    var camera = line01.camera + (x - line01.x) * dCamera;

                    if (x >= 0 && x < bitmap.Width && (int)line01.y >= 0 && (int)line01.y < bitmap.Height && 
                        z < _zBuffer[x, (int)line01.y] && IsPointVisible(normal, camera))
                    {
                        _zBuffer[x, (int)line01.y] = z;
                        GetPixelColor = () => GetColorFromNormaleLightAndCamera(normal, camera);
                        DrawPixel(bitmap, new Pixel(x, (int)line01.y, z));
                    }
                }

                line01.IncrementY();
                line02.IncrementY();
            }
            while(line12.y <= Math.Floor(pNCsArr[2].Point.Y))
            {
                var dz = (line12.x - line02.x) != 0 ? (line12.z - line02.z) / (line12.x - line02.x) : 0;
                var dNormal = (line12.normal - line02.normal) != Vector3.Zero ? (line12.normal - line02.normal) / (line12.x - line02.x) : Vector3.Zero;
                var dCamera = (line12.camera - line02.camera) != Vector3.Zero ? (line12.camera - line02.camera) / (line12.x - line02.x) : Vector3.Zero;
                for (int x = (int)line12.x; dx * x <= dx * line02.x; x += dx)
                {
                    var z = line12.z + (x - line12.x) * dz;
                    var normal = line12.normal + (x - line12.x) * dNormal;
                    var camera = line12.camera + (x - line12.x) * dCamera;
                    if (x >= 0 && x < bitmap.Width && (int)line12.y >= 0 && (int)line12.y < bitmap.Height && 
                        z < _zBuffer[x, (int)line12.y] && IsPointVisible(normal, camera))
                    {
                        _zBuffer[x, (int)line12.y] = z;
                        GetPixelColor = () => GetColorFromNormaleLightAndCamera(normal, camera);
                        DrawPixel(bitmap, new Pixel(x, (int)line12.y, z));
                    }
                }

                line12.IncrementY();
                line02.IncrementY();
            }
        }

        private bool IsPointVisible(Vector3 normalInPoint, Vector3 camera)
        {
            return true;//Vector3.Dot(normalInPoint, camera) >= 0f;
        }
    }
}
