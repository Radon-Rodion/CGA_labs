using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CGA_labs.Logic
{
    public static class PBRLogic
    {
        public static Vector3 FresnelSchlick(float cosTheta, Vector3 F0)
        {
            return F0 + (new Vector3(1.0f, 1.0f, 1.0f) - F0) * (float)Math.Pow(Clamp(1.0f - cosTheta, 0.0f, 1.0f), 5.0f);
        }

        public static float DistributionGGX(Vector3 N, Vector3 H, float roughness)
        {
            float a = roughness * roughness;
            float a2 = a * a;
            float NdotH = Math.Max(Vector3.Dot(N, H), 0.0f);
            float NdotH2 = NdotH * NdotH;

            float num = a2;
            float denom = (NdotH2 * (a2 - 1.0f) + 1.0f);
            denom = (float)Math.PI * denom * denom;

            return num / denom;
        }

        public static float GeometrySchlickGGX(float NdotV, float roughness)
        {
            float r = (roughness + 1.0f);
            float k = (r * r) / 8.0f;

            float num = NdotV;
            float denom = NdotV * (1.0f - k) + k;

            return num / denom;
        }
        public static float GeometrySmith(Vector3 N, Vector3 V, Vector3 L, float roughness)
        {
            float NdotV = (float)Math.Max(Vector3.Dot(N, V), 0.0);
            float NdotL = (float)Math.Max(Vector3.Dot(N, L), 0.0);
            float ggx2 = GeometrySchlickGGX(NdotV, roughness);
            float ggx1 = GeometrySchlickGGX(NdotL, roughness);

            return ggx1 * ggx2;
        }

        public static float Clamp(float val, float min, float max)
        {
            return Math.Min(Math.Max(min, val), max);
        }
    }
}
