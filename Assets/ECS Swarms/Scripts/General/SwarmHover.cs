using Unity.Physics;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace ECSSwarms
{
    /// <summary>
    /// A static class used to calculate the physics-based hover behaviour for swarms.
    /// </summary>
    [BurstCompile]
    public static class SwarmHover
    {
        /// <summary>
        /// Calculates the hover behaviour for a single swarm.
        /// </summary>
        /// <param name="world">The CollisionWorld to use for physics spherecasts.</param>
        /// <param name="settings">The settings of the swarm.</param>
        /// <param name="rotation">The rotation of the swarm.</param>
        /// <param name="position">The position of the swarm.</param>
        /// <returns>A (float3, bool) tuple, the float3 being the hover vector, and the bool being if this behaviour should be forced.</returns>
        public static (float3, bool) DoHover([ReadOnly] CollisionWorld world, SwarmSettings settings, quaternion rotation, float3 position)
        {
            ColliderCastHit hit;

            if (world.SphereCast(position, settings.HoverSphereCastRadius, settings.HoverDirection, settings.HoverDistance, out hit, settings.HoverColliderFilter))
            {
                return (math.lerp(-math.normalizesafe(settings.HoverDirection), float3.zero, hit.Fraction), false);
            }

            return (float3.zero, false);
        }
    }
}
