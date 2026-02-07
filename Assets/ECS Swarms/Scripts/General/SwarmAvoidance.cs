using Unity.Physics;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace ECSSwarms
{
    /// <summary>
    /// A static class used to calculate the physics-based avoidance behaviour for swarms.
    /// </summary>
    [BurstCompile]
    public static class SwarmAvoidance
    {
        /// <summary>
        /// Calculates the avoidance behaviour for a single swarm.
        /// </summary>
        /// <param name="world">The CollisionWorld to use for physics sphere casts.</param>
        /// <param name="settings">The settings of the swarm.</param>
        /// <param name="rotation">The rotation of the swarm.</param>
        /// <param name="position">The position of the swarm.</param>
        /// <returns>A (float3, bool) tuple, the float3 being the avoidance vector, and the bool being if this behaviour should be forced.</returns>
        public static (float3, bool) DoAvoidance([ReadOnly] CollisionWorld world, SwarmSettings settings, quaternion rotation, float3 position)
        {
            float3 bestDir = float3.zero;
            bool shouldForce = false;
            float bestFrac = 0.0f;

            for (int i = 0; i < settings.AvoidanceMaxSphereCasts; i++)
            {
                float3 dir = math.rotate(rotation, SwarmVisionHelper.GetLOSVector(i, settings.AvoidanceMaxSphereCasts, settings.VisionAngleRadians));
                ColliderCastHit hit;

                if (world.SphereCast(position + dir * settings.AvoidanceSphereCastRadius, settings.AvoidanceSphereCastRadius, dir, settings.AvoidanceDistance, out hit, settings.AvoidanceColliderFilter))
                {
                    if (i == 0)
                    {
                        shouldForce = (hit.Fraction * settings.AvoidanceDistance) < settings.ForceAvoidanceDistance;
                    }

                    if (hit.Fraction > bestFrac)
                    {
                        bestDir = dir;
                        bestFrac = hit.Fraction;
                    }
                }
                else
                {
                    if (i == 0) // nothing obstructing path, don't correct it
                    {
                        return (Vector3.zero, false);
                    }

                    return (dir, shouldForce);
                }
            }

            return (bestDir, shouldForce);
        }
    }
}
