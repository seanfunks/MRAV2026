using System;
using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECSSwarms
{
    /// <summary>
    /// <para>A shared component for the settings for a particular kind of swarm. Contains attributes related to movement, vision, and behaviours.</para>
    /// <para>You may want to edit this if you want to add customizable settings for custom behaviours.</para>
    /// </summary>
    [Serializable]
    [WriteGroup(typeof(LocalToWorld))]
    public struct SwarmSettings : ISharedComponentData
    {
        /// <summary>
        /// The minimum speed of the swarm. It will not go slower than this.
        /// </summary>
        public float MinSpeed;
        /// <summary>
        /// The maximum speed of the swarm. It will not go faster than this.
        /// </summary>
        public float MaxSpeed;
        /// <summary>
        /// The acceleration of the swarm. The higher this value, the faster the swarm will acclerate and decelerate.
        /// </summary>
        public float Acceleration;

        /// <summary>
        /// The default tag for swarms with these settings.
        /// </summary>
        public SwarmTag DefaultTag;
        /// <summary>
        /// Wether to use the global up or local up for orientation.
        /// </summary>
        public bool GlobalUp;

        /// <summary>
        /// How far this swarm can see.
        /// </summary>
        public float VisionDistance;
        /// <summary>
        /// The angle of this swarms vision cone. A value of 180 degrees means full 360 degree vision.
        /// </summary>
        public float VisionAngleRadians;

        /// <summary>
        /// The type of movement to apply for the seperation behaviour.
        /// </summary>
        public MovementType SeperationMovementType;
        /// <summary>
        /// How strongly to apply the seperation behaviour.
        /// </summary>
        public float SeparationWeight;

        /// <summary>
        /// How strongly to apply the alignment behaviour. This is always applied using steering.
        /// </summary>
        public float AlignmentWeight;

        /// <summary>
        /// The type of movement to apply for the cohesion behaviour.
        /// </summary>
        public MovementType CohesionMovementType;
        /// <summary>
        /// How strongly to apply the cohesion behaviour.
        /// </summary>
        public float CohesionWeight;

        /// <summary>
        /// Wether to do the physics based obstacle avoidance behaviour or not.
        /// </summary>
        public bool DoAvoidance;
        /// <summary>
        /// The type of movement to apply for the avoidance behaviour.
        /// </summary>
        public MovementType AvoidanceMovementType;
        /// <summary>
        /// The distance at which to detect obstacles.
        /// </summary>
        public float AvoidanceDistance;
        /// <summary>
        /// The distance at which to force this behaviour above all other behaviours. For instance in emergency obstacle avoidance scenarios.
        /// </summary>
        public float ForceAvoidanceDistance;
        /// <summary>
        /// The radius of the sphere cast to use for avoidance collision detection.
        /// </summary>
        public float AvoidanceSphereCastRadius;
        /// <summary>
        /// The maximum number of sphere casts to use for avoidance collision detection.
        /// </summary>
        public int AvoidanceMaxSphereCasts;
        /// <summary>
        /// The collider filter to use for avoidance collision detection.
        /// </summary>
        public CollisionFilter AvoidanceColliderFilter;
        /// <summary>
        /// How strongly to apply the avoidance behaviour.
        /// </summary>
        public float AvoidanceWeight;

        /// <summary>
        /// Wether to do the physics based hover behaviour or not.
        /// </summary>
        public bool DoHover;
        /// <summary>
        /// The type of movement to apply for the hover behaviour.
        /// </summary>
        public MovementType HoverMovementType;
        /// <summary>
        /// The downward vector for the hovering direction.
        /// </summary>
        public float3 HoverDirection;
        /// <summary>
        /// The distance to hover above the ground.
        /// </summary>
        public float HoverDistance;
        /// <summary>
        /// The radius of the sphere cast to use for hover collision detection.
        /// </summary>
        public float HoverSphereCastRadius;
        /// <summary>
        /// The collider filter to use for hover collision detection.
        /// </summary>
        public CollisionFilter HoverColliderFilter;
        /// <summary>
        /// How strongly to apply the hover behaviour.
        /// </summary>
        public float HoverWeight;

        /// <summary>
        /// The type of movement to apply for the target behaviour.
        /// </summary>
        public MovementType TargetMovementType;
        /// <summary>
        /// The minimum distance a target must be from a swarm, for it to affect it.
        /// </summary>
        public float MinimumTargetDistance;
        /// <summary>
        /// How strongly to apply the target behaviour.
        /// </summary>
        public float TargetWeight;

        /// <summary>
        /// The type of movement to apply for the obstacle behaviour.
        /// </summary>
        public MovementType ObstacleMovementType;
        /// <summary>
        /// The maximum distance an obstacle must be from a swarm, for it to affect it.
        /// </summary>
        public float MaximumObstacleDistance;
        /// <summary>
        /// How strongly to apply the obstacle behaviour.
        /// </summary>
        public float ObstacleWeight;

        /// <summary>
        /// Gravity to apply to the swarm each update. This will be multiplied by deltaTime before being applied to the swarm.
        /// </summary>
        public float3 Gravity;

        /// <summary>
        /// Drag to apply to the swarm each update. This will be multiplied by deltaTime before being applied to the swarm.
        /// </summary>
        public float Drag;
    }
}