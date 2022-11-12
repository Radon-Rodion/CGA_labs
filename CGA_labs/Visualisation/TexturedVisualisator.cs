using CGA_labs.Entities;
using CGA_labs.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Media.Imaging;

namespace CGA_labs.Visualisation
{
    public class TexturedVisualisator: AbstractVisualisator
    {
        private Vector3 _lightVector;
        private Func<List<Vector3>, int, Vector3> _cameraVector;
        private float[,] _zBuffer;
        private ModelParams _modelParams;
        public override void DrawModel(WriteableBitmap bitmap, Model model, ModelParams parameters, Model worldModel)
        {
            _modelParams = parameters;
            var cameraGlobalVector = new Vector3(parameters.CameraPositionX, parameters.CameraPositionY, parameters.CameraPositionZ);
            _cameraVector = (face, index) =>
            {
                int indexPoint = (int)face[index].X;
                Vector4 point = worldModel.Points[indexPoint];
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

        private (float r, float g, float b) GetAmbientLighting(Vector3 color)
        {
            float k = 0.05f;
            return (color.X * k, color.Y * k, color.Z * k);
        }

        private (float r, float g, float b) GetDiffuseLighting(Vector3 normal, Vector3 color)
        {
            var k = Vector3.Dot(normal, _lightVector);
            k = k > 0 ? k : 0;
            k *= 0.95f;
            return (color.X * k, color.Y * k, color.Z * k);
        }

        private (double r, double g, double b) GetSpecularLighting(Vector3 normal, Vector3 cameraVector, float blick)
        {
            var vectorR = _lightVector - 2 * Vector3.Dot(_lightVector, normal) * normal;
            var production = Vector3.Dot(-vectorR, cameraVector);
            var k = production > 0 ? Math.Pow(production, blick) : 0;
            k *= 20;
            return (k, k, k);
        }

        private byte[] GetColorFromNormaleLightAndCamera(Vector3 color,Vector3 normal, Vector3 reflection, Vector3 cameraVector)
        {
            var ambient = GetAmbientLighting(color);
            var diffuse = GetDiffuseLighting(normal, color);
            var specular = GetSpecularLighting(normal, cameraVector, reflection.X);

            byte blue = (byte)(Math.Min(Math.Max(ambient.b + diffuse.b + specular.b, 0), 255));
            byte green = (byte)(Math.Min(Math.Max(ambient.g + diffuse.g + specular.g, 0), 255));
            byte red = (byte)(Math.Min(Math.Max(ambient.r + diffuse.r + specular.r, 0), 255));
            byte alpha = 255;
            byte[] colorData = { blue, green, red, alpha };
            return colorData;
        }

        private struct PointTexelCam
        {
            public Vector3 Texel;
            public Vector3 Point;
            public Vector3 Camera;
        }

        private List<PointTexelCam> GetNormalsPointsAndCameraVectors(Model model, List<Vector3> triangle)
        {
            var result = new List<PointTexelCam>();
            for (int i = 0; i < triangle.Count; i++)
            {
                var pNC = new PointTexelCam();
                pNC.Texel = model.Texels[(int)triangle[i].Y];
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
            public Vector3 dTexelByZ;
            public float dOneByZ;
            public Vector3 dCamera;

            public Vector3 texelByZ;
            public float oneByZ;
            public Vector3 camera;

            public LineParams(PointTexelCam from, PointTexelCam to)
            {
                dz = (to.Point.Z - from.Point.Z) / (to.Point.Y - from.Point.Y);
                z = from.Point.Z;
                x = from.Point.X;
                y = from.Point.Y;
                dy0 = to.Point.Y - from.Point.Y;
                dx0 = to.Point.X - from.Point.X;
                dTexelByZ = (to.Texel/to.Point.Z - from.Texel/from.Point.Z) / (to.Point.Y - from.Point.Y);
                dOneByZ = (1/to.Point.Z - 1/from.Point.Z) / (to.Point.Y - from.Point.Y);
                dCamera = (to.Camera - from.Camera) / (to.Point.Y - from.Point.Y);
                texelByZ = from.Texel/from.Point.Z;
                oneByZ = 1/from.Point.Z;
                camera = from.Camera;
            }

            public void IncrementY()
            {
                texelByZ += dTexelByZ;
                oneByZ += dOneByZ;
                camera += dCamera;
                z += dz;
                x += dx0 / dy0;
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

            var k2 = line02.dy0 != 0 ? line02.dx0 / line02.dy0 : line02.dx0;
            var k1 = line01.dy0 != 0 ? line01.dx0 / line01.dy0 : line01.dx0;
            var dx = k2 > k1 ? 1 : -1;

            while (line01.y <= Math.Floor(pNCsArr[1].Point.Y))
            {
                var dz = (line01.x - line02.x) != 0 ? (line01.z - line02.z) / (line01.x - line02.x) : 0;
                var dTexelByZ = (line01.x - line02.x) != 0 ? (line01.texelByZ - line02.texelByZ) / (line01.x - line02.x) : Vector3.Zero;
                var dOneByZ = (line01.x - line02.x) != 0 ? (line01.dOneByZ - line02.dOneByZ) / (line01.x - line02.x) : 0;
                var dCamera = (line01.x - line02.x) != 0 ? (line01.camera - line02.camera) / (line01.x - line02.x) : Vector3.Zero;
                int startX = (int)(dx < 0 ? Math.Floor(line01.x) : Math.Ceiling(line01.x));
                int endX = (int)(dx < 0 ? Math.Ceiling(line02.x) : Math.Floor(line02.x));
                for (int x = startX; dx * x <= dx * endX; x += dx)
                {
                    var z = line01.z + (x - line01.x) * dz;
                    var texelByZ = line01.texelByZ + (x - line01.x) * dTexelByZ;
                    var oneByZ = line01.oneByZ + (x - line01.x) * dOneByZ;
                    var camera = line01.camera + (x - line01.x) * dCamera;

                    if (x >= 0 && x < bitmap.Width && (int)line01.y >= 0 && (int)line01.y < bitmap.Height &&
                        z < _zBuffer[x, (int)line01.y])
                    {
                        _zBuffer[x, (int)line01.y] = z;
                        var (color, normal, reflection) = GetByTexel(model, texelByZ/oneByZ);
                        GetPixelColor = () => GetColorFromNormaleLightAndCamera(color, normal, reflection, camera);
                        DrawPixel(bitmap, new Pixel(x, (int)line01.y, z));
                    }
                }

                line01.IncrementY();
                line02.IncrementY();
            }
            while (line12.y <= Math.Floor(pNCsArr[2].Point.Y))
            {
                var dz = (line12.x - line02.x) != 0 ? (line12.z - line02.z) / (line12.x - line02.x) : 0;
                var dTexelByZ = (line12.x - line02.x) != 0 ? (line12.texelByZ - line02.texelByZ) / (line12.x - line02.x) : Vector3.Zero;
                var dOneByZ = (line12.x - line02.x) != 0 ? (line12.dOneByZ - line02.dOneByZ) / (line12.x - line02.x) : 0;
                var dCamera = (line12.x - line02.x) != 0 ? (line12.camera - line02.camera) / (line12.x - line02.x) : Vector3.Zero;
                int startX = (int)(dx < 0 ? Math.Floor(line12.x) : Math.Ceiling(line12.x));
                int endX = (int)(dx < 0 ? Math.Ceiling(line02.x) : Math.Floor(line02.x));
                for (int x = startX; dx * x <= dx * endX; x += dx)
                {
                    var z = line12.z + (x - line12.x) * dz;
                    var texelByZ = line12.texelByZ + (x - line12.x) * dTexelByZ;
                    var oneByZ = line12.oneByZ + (x - line12.x) * dOneByZ;
                    var camera = line12.camera + (x - line12.x) * dCamera;
                    
                    if (x >= 0 && x < bitmap.Width && (int)line12.y >= 0 && (int)line12.y < bitmap.Height &&
                        z < _zBuffer[x, (int)line12.y])
                    {
                        _zBuffer[x, (int)line12.y] = z;
                        var (color, normal, reflection) = GetByTexel(model, texelByZ / oneByZ);
                        GetPixelColor = () => GetColorFromNormaleLightAndCamera(color, normal, reflection, camera);
                        DrawPixel(bitmap, new Pixel(x, (int)line12.y, z));
                    }
                }

                line12.IncrementY();
                line02.IncrementY();
            }
        }

        private (Vector3 color, Vector3 normal, Vector3 reflection) GetByTexel(Model model, Vector3 texel)
        {
            int x = (int)(Math.Min(Math.Max(texel.X * model.TexturesMap[0].Length, 0), model.TexturesMap[0].Length - 1));
            int y = (int)(Math.Min(Math.Max(model.TexturesMap.Length - texel.Y * model.TexturesMap.Length, 0), model.TexturesMap.Length - 1));
            return (model.TexturesMap[y][x], TransformationLogic.TransformVectorFromModelToWorld(model.NormalsMap[y][x], _modelParams), model.ReflectionsMap[y][x]);
        }
    }
}
