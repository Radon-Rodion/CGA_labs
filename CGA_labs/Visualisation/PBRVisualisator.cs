using CGA_labs.Entities;
using CGA_labs.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static CGA_labs.Logic.PBRLogic;

namespace CGA_labs.Visualisation
{

    public class PBRVisualisator : AbstractVisualisator
    {
        private Vector3[] _lightPositions;
        private Vector3[] _lightColors;
        private Vector3 _cameraVector;
        private float[,] _zBuffer;
        private ModelParams _modelParams;
        private Model _globalModel;
        public override void DrawModel(WriteableBitmap bitmap, Model model, ModelParams parameters, Model worldModel)
        {
                var lightsRingRadius = getModelRadius(worldModel, parameters) * 2;
                var modelPosition = new Vector3(parameters.TranslationX, parameters.TranslationY, parameters.TranslationZ);

                _lightPositions = new Vector3[] {
                new Vector3(lightsRingRadius, 0, 0) + modelPosition,
                new Vector3(-lightsRingRadius, 0, 0) + modelPosition,
                new Vector3(0, lightsRingRadius, 0) + modelPosition,
                new Vector3(0, -lightsRingRadius, 0) + modelPosition
            };
                _lightColors = new Vector3[]
                {
                new Vector3(1, 1, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 1),
                new Vector3(1, 1, 1)
                };

                _modelParams = parameters;
            _globalModel = worldModel;
            _cameraVector = new Vector3(parameters.CameraPositionX, parameters.CameraPositionY, parameters.CameraPositionZ);

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

            private byte[] GetColorInPoint(Vector3 albedo, Vector3 normal, Vector3 point, float metallic, float roughness, float ao)
            {
                Vector3 N = Vector3.Normalize(normal);
                Vector3 V = Vector3.Normalize(_cameraVector - point);
                float baseReflectivity = 0.04f;
                Vector3 C = Vector3.Normalize(_cameraVector - albedo);
                Vector3 F0 = new Vector3(baseReflectivity, baseReflectivity, baseReflectivity);
                F0 = F0 + albedo * metallic * (1 - baseReflectivity);

                // reflectance equation
                Vector3 Lo = Vector3.Zero;
                for (int i = 0; i < 4; ++i)
                {
                    // calculate per-light radiance
                    Vector3 L = Vector3.Normalize(_lightPositions[i] - point);
                    Vector3 H = Vector3.Normalize(V + L);
                    float distance = (_lightPositions[i] - point).Length();
                    float attenuation = 1.0f / (distance*distance);
                    Vector3 radiance = _lightColors[i] * attenuation;

                    // cook-torrance brdf
                    float NDF = DistributionGGX(N, H, roughness);
                    float G = GeometrySmith(N, V, L, roughness);
                    Vector3 F = FresnelSchlick(Math.Max(Vector3.Dot(H, V), 0.0f), F0);

                    Vector3 kS = F;
                    Vector3 kD = new Vector3(1.0f, 1.0f, 1.0f) - kS;
                    kD *= 1.0f - metallic;

                    Vector3 numerator = NDF * G * F;
                    float denominator = 4.0f * (float)Math.Max(Vector3.Dot(N, V), 0.0) * (float)Math.Max(Vector3.Dot(N, L), 0.0) + 0.0001f;
                    Vector3 specular = numerator / denominator;

                    // add to outgoing radiance Lo
                    float NdotL = (float)Math.Max(Vector3.Dot(N, L), 0.0);
                    Lo += (kD * albedo / (float)Math.PI + specular) * radiance * NdotL;
                }

                Vector3 ambient = 0.03f * albedo * ao;
                Vector3 color = ambient + Lo;

                color = color / (color + new Vector3(1, 1, 1));
                double power = 1.0 / 2.2;
                color = new Vector3((float)Math.Pow(color.X, power), (float)Math.Pow(color.Y, power), (float)Math.Pow(color.Z, power));

                byte blue = (byte)(Math.Min(Math.Max(color.X*255, 0), 255));
                byte green = (byte)(Math.Min(Math.Max(color.Y*255, 0), 255));
                byte red = (byte)(Math.Min(Math.Max(color.Z*255, 0), 255));
                byte alpha = 255;
                byte[] colorData = { blue, green, red, alpha };
                return colorData;
            }

            private struct PointTexel
        {
            public Vector3 Texel;
            public Vector4 Point;
            public Vector4 GlobalPoint;
        }

        private List<PointTexel> GetNormalsPointsAndCameraVectors(Model model, List<Vector3> triangle, Model globalModel)
        {
            var result = new List<PointTexel>();
            for (int i = 0; i < triangle.Count; i++)
            {
                var pNC = new PointTexel();
                pNC.Texel = model.Texels[(int)triangle[i].Y];
                pNC.Point = model.Points[(int)triangle[i].X];
                pNC.GlobalPoint = globalModel.Points[(int)triangle[i].X];
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
            public Vector4 dPoint;

            public Vector3 texelByZ;
            public float oneByZ;
            public Vector4 point;

            public LineParams(PointTexel from, PointTexel to)
            {
                dz = (to.Point.Z - from.Point.Z) / (to.Point.Y - from.Point.Y);
                z = from.Point.Z;
                x = from.Point.X;
                y = from.Point.Y;
                dy0 = to.Point.Y - from.Point.Y;
                dx0 = to.Point.X - from.Point.X;
                dTexelByZ = (to.Texel / to.Point.W - from.Texel / from.Point.W) / (to.Point.Y - from.Point.Y);
                dOneByZ = (1 / to.Point.W - 1 / from.Point.W) / (to.Point.Y - from.Point.Y);
                dPoint = (to.GlobalPoint - from.GlobalPoint) / (to.Point.Y - from.Point.Y);
                texelByZ = from.Texel / from.Point.W;
                oneByZ = 1 / from.Point.W;
                point = from.GlobalPoint;
            }

            public void IncrementY()
            {
                texelByZ += dTexelByZ;
                oneByZ += dOneByZ;
                point += dPoint;
                z += dz;
                x += dx0 / dy0;
                y++;
            }
        }

        protected override void DrawFace(WriteableBitmap bitmap, Model model, List<Vector3> face)
        {
            var pNCsList = GetNormalsPointsAndCameraVectors(model, face, _globalModel);
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
                var dOneByZ = (line01.x - line02.x) != 0 ? (line01.oneByZ - line02.oneByZ) / (line01.x - line02.x) : 0;
                var dPoint = (line01.x - line02.x) != 0 ? (line01.point - line02.point) / (line01.x - line02.x) : Vector4.Zero;
                int startX = (int)(dx < 0 ? Math.Floor(line01.x) : Math.Ceiling(line01.x));
                int endX = (int)(dx < 0 ? Math.Ceiling(line02.x) : Math.Floor(line02.x));
                for (int x = startX; dx * x <= dx * endX; x += dx)
                {
                    var z = line01.z + (x - line01.x) * dz;
                    var texelByZ = line01.texelByZ + (x - line01.x) * dTexelByZ;
                    var oneByZ = line01.oneByZ + (x - line01.x) * dOneByZ;
                    var point = line01.point + (x - line01.x) * dPoint;

                    if (x >= 0 && x < bitmap.Width && (int)line01.y >= 0 && (int)line01.y < bitmap.Height &&
                        z < _zBuffer[x, (int)line01.y])
                    {
                        _zBuffer[x, (int)line01.y] = z;
                        var (color, normal, metallic, roughness, ao) = GetByTexel(model, texelByZ / oneByZ);
                        GetPixelColor = () => GetColorInPoint(color, normal, new Vector3(point.X, point.Y, point.Z), metallic, roughness, ao);
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
                var dOneByZ = (line12.x - line02.x) != 0 ? (line12.oneByZ - line02.oneByZ) / (line12.x - line02.x) : 0;
                var dPoint = (line12.x - line02.x) != 0 ? (line12.point - line02.point) / (line12.x - line02.x) : Vector4.Zero;
                int startX = (int)(dx < 0 ? Math.Floor(line12.x) : Math.Ceiling(line12.x));
                int endX = (int)(dx < 0 ? Math.Ceiling(line02.x) : Math.Floor(line02.x));
                for (int x = startX; dx * x <= dx * endX; x += dx)
                {
                    var z = line12.z + (x - line12.x) * dz;
                    var texelByZ = line12.texelByZ + (x - line12.x) * dTexelByZ;
                    var oneByZ = line12.oneByZ + (x - line12.x) * dOneByZ;
                    var point = line12.point + (x - line12.x) * dPoint;

                    if (x >= 0 && x < bitmap.Width && (int)line12.y >= 0 && (int)line12.y < bitmap.Height &&
                        z < _zBuffer[x, (int)line12.y])
                    {
                        _zBuffer[x, (int)line12.y] = z;
                        var (color, normal, metallic, roughness, ao) = GetByTexel(model, texelByZ / oneByZ);
                        GetPixelColor = () => GetColorInPoint(color, normal, new Vector3(point.X,point.Y,point.Z), metallic, roughness, ao);
                        DrawPixel(bitmap, new Pixel(x, (int)line12.y, z));
                    }
                }

                line12.IncrementY();
                line02.IncrementY();
            }
        }

        private (Vector3 color, Vector3 normal, float metallic, float roughness, float ao) GetByTexel(Model model, Vector3 texel)
        {
            int x = (int)(Math.Min(Math.Max(texel.X * model.TexturesMap[0].Length, 0), model.TexturesMap[0].Length - 1));
            int y = (int)(Math.Min(Math.Max(model.TexturesMap.Length - texel.Y * model.TexturesMap.Length, 0), model.TexturesMap.Length - 1));
            return (model.TexturesMap[y][x], TransformationLogic.TransformVectorFromModelToWorld(model.NormalsMap[y][x], _modelParams), model.MetalicMap[y][x], model.RoughnessMap[y][x], model.aoMap[y][x]);
        }

        private float getModelRadius(Model model, ModelParams modelParams)
        {
                float radius = 0;
                model.Points.ForEach(p =>
                {
                    if (Math.Abs(p.X - modelParams.TranslationX) > radius)
                        radius = Math.Abs(p.X - modelParams.TranslationX);
                    if (Math.Abs(p.Z - modelParams.TranslationZ) > radius)
                        radius = Math.Abs(p.Z - -modelParams.TranslationZ);
                });
                return radius;
        }
    }
}
