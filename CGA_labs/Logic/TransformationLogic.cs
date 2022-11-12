using CGA_labs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CGA_labs.Logic
{
    public static class TransformationLogic
    {
        public static Matrix4x4 ModelWorldMatrix { get; set; }

        public static Vector3 TransformVectorFromModelToWorld(Vector3 vector, ModelParams modelParams)
        {
            Matrix4x4 toWorldMatrix = GetTransformMatrix(modelParams);
            return Vector3.Normalize(Vector3.Transform(vector, toWorldMatrix));
        }

        public static Model TransformFromModelToWorld(Model model, ModelParams modelParams)
        {
            Matrix4x4 toWorldMatrix = GetTransformMatrix(modelParams);
            float[] w = new float[model.Points.Count];
            for (int i = 0; i < model.Points.Count; i++)
            {
                model.Points[i] = Vector4.Transform(model.Points[i], toWorldMatrix);

                w[i] = model.Points[i].W;
                model.Points[i] /= model.Points[i].W;
            }
            return model;
        }
        public static void TransformFromModelToView(Model model, ModelParams modelParams)
        {
            Matrix4x4 totalProjctionMatix = GetTotalMatrix(modelParams);
            float[] w = new float[model.Points.Count];
            for (int i = 0; i < model.Points.Count; i++)
            {
                model.Points[i] = Vector4.Transform(model.Points[i], totalProjctionMatix);

                w[i] = model.Points[i].W;
                model.Points[i] /= model.Points[i].W;
            }

            TransformNormals(model, modelParams);
            TransformToViewPort(model, modelParams, w);

            /*var newPoints = new List<Vector4>();
            foreach(var p in model.Points)
            {
                newPoints.Add(new Vector4(p.X / p.W, p.Y / p.W, p.Z, 1));
            }
            model.Points = newPoints;*/
        }

        private static Matrix4x4 GetTransformMatrix(ModelParams modelParams)
        {
            ModelWorldMatrix = Matrix4x4.CreateScale(modelParams.Scaling) 
                * Matrix4x4.CreateFromYawPitchRoll(modelParams.ModelYaw, modelParams.ModelPitch, modelParams.ModelRoll)
                * Matrix4x4.CreateTranslation(modelParams.TranslationX, modelParams.TranslationY, modelParams.TranslationZ);
            return ModelWorldMatrix;
        }


        private static Matrix4x4 GetCameraMatrix(ModelParams modelParams)
        {
            return
                 Matrix4x4.CreateTranslation(-new Vector3(modelParams.CameraPositionX, modelParams.CameraPositionY, modelParams.CameraPositionZ))
                 * Matrix4x4.Transpose(Matrix4x4.CreateFromYawPitchRoll(modelParams.CameraYaw, modelParams.CameraPitch, modelParams.CameraRoll));
        }

        private static Matrix4x4 GetProjectionMatrix(ModelParams modelParams)
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(modelParams.FieldOfView, modelParams.AspectRatio, modelParams.NearPlaneDistance, modelParams.FarPlaneDistance);
        }

        private static Matrix4x4 GetWindowMatrix(ModelParams modelParams)
        {
            return GetWindowMatrix(modelParams.XMin, modelParams.YMin, modelParams.Width, modelParams.Height);
        }

        public static Matrix4x4 GetWindowMatrix(int minX, int minY, int width, int height)
        {
            return new Matrix4x4(width/2, 0, 0, 0,
                                 0, -height/2, 0, 0,
                                 0, 0, 1, 0,
                                 minX+(width/2), minY+(height/2), 0, 1);
        }

        private static Matrix4x4 GetTotalMatrix(ModelParams modelParams)
        {
            return GetTransformMatrix(modelParams) * GetCameraMatrix(modelParams) * GetProjectionMatrix(modelParams);
        }

        private static void TransformToViewPort(Model model, ModelParams modelParams, float[] w)
        {
            for (int i = 0; i < model.Points.Count; i++)
            {
                model.Points[i] = Vector4.Transform(model.Points[i], GetWindowMatrix(modelParams));
                model.Points[i] = new Vector4(model.Points[i].X, model.Points[i].Y, model.Points[i].Z, w[i]);
            }
        }

        private static void TransformNormals(Model model, ModelParams modelParams)
        {
            for (int i = 0; i < model.Normals.Count; i++)
            {
                model.Normals[i] = Vector3.Normalize(Vector3.TransformNormal(model.Normals[i], GetTransformMatrix(modelParams)));
            }
        }
    }
}
