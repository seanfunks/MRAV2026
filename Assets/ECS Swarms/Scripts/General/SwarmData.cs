using Unity.Entities;
using Unity.Mathematics;

namespace ECSSwarms
{
    /// <summary>
    /// All the data for a single swarm to hold. Contains the swarms velocity and tag; also contains data for the result of physics based behaviours.
    /// </summary>
    public struct SwarmData : IComponentData
    {
        /// <summary>
        /// The current velocity of the swarm.
        /// </summary>
        public float3 Velocity;
        /// <summary>
        /// The current tag of the swarm.
        /// </summary>
        public SwarmTag Tag;
        /// <summary>
        /// The local to world scale of the swarm.
        /// </summary>
        public float3 Scale;

        /// <summary>
        /// The calculated force from the last physics behaviour calculation.
        /// </summary>
        public float3 PhysicsResult;
        /// <summary>
        /// Wether to force this force vector over other behaviours (ex. in emergency obstacle avoidance situations).
        /// </summary>
        public bool ForcePhysics;
    }
}