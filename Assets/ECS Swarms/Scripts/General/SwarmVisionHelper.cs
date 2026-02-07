using Unity.Mathematics;
using UnityEngine;

namespace ECSSwarms
{
    /// <summary>
    /// A static class used to assist in vision/avoidance based calculations for swarm behaviours.
    /// </summary>
    public static class SwarmVisionHelper
    {
        /// <summary>
        /// A constant value for phi, used to calculate the line of sight vectors.
        /// </summary>
        static readonly float phi = Mathf.PI * (3.0f - Mathf.Sqrt(5.0f));

        /// <summary>
        /// Gets a line-of-sight vector. These vectors follow a pattern of starting pointing forward and then emanating outwards spherically, these can be used for
        /// obstacle avoidance, as you can follow the first vector that doesnt have any collision.
        /// </summary>
        /// <param name="index">The index of the LOS vector to calculate.</param>
        /// <param name="maxCasts">The total amount of LOS vectors in this collection.</param>
        /// <param name="visionAngleRadians">The angle of vision in radians. </param>
        /// <returns>A normalized float3 pointing in the direction of the corresponding LOS vector</returns>
        public static float3 GetLOSVector(int index, int maxCasts, float visionAngleRadians)
        {
            float finalZ = math.cos(visionAngleRadians);
            float theta = phi * index;

            float z = math.lerp(1.0f, finalZ, index / (float)(maxCasts - 1));
            float radius = math.sqrt(1.0f - z * z);

            return new float3(math.cos(theta) * radius, math.sin(theta) * radius, z);
        }
    }
}
