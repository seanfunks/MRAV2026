using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ECSSwarms
{
    /// <summary>
    /// <para>The authoring component for SwarmSettings. SwarmSettings is a shared component for the settings for a particular kind of swarm. It contains attributes 
    /// related to movement, vision, and behaviours.</para>
    /// <para>You may want to edit this if you want to add customizable settings for custom behaviours.</para>
    /// </summary>
    [AddComponentMenu("Swarm Toolkit/Swarm Settings")]
    public class SwarmSettingsAuthoring : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("The minimum speed of the swarm. It will not go slower than this.")]
        public float MinSpeed = 8.0f;
        [Tooltip("The maximum speed of the swarm. It will not go faster than this.")] 
        public float MaxSpeed = 12.0f;
        [Tooltip("The acceleration of the swarm. The higher this value, the faster the swarm will acclerate and decelerate.")] 
        public float Acceleration = 10.0f;

        [Header("Behaviour")]
        [Tooltip("The default tag for swarms with these settings.")] 
        public SwarmTag DefaultTag = SwarmTag.Default;
        [Tooltip("Wether to use the global up or local up for orientation.")] 
        public bool GlobalUp = true;

        [Header("Vision")]
        [Tooltip("How far this swarm can see.")]
        public float VisionDistance = 10.0f;
        [Tooltip("The angle of this swarms vision cone. A value of 180 degrees means full 360 degree vision.")]
        public float VisionAngleDegrees = 120.0f;

        [Header("Seperation")]
        [Tooltip("The type of movement to apply for the seperation behaviour.")]
        public MovementType SeperationMovementType = MovementType.Force;
        [Tooltip("How strongly to apply the seperation behaviour.")]
        public float SeparationWeight = 25.0f;

        [Header("Alignment")]
        [Tooltip("How strongly to apply the alignment behaviour. This is always applied using steering.")]
        public float AlignmentWeight = 1.0f;

        [Header("Cohesion")]
        [Tooltip("The type of movement to apply for the cohesion behaviour.")]
        public MovementType CohesionMovementType = MovementType.Steering;
        [Tooltip("How strongly to apply the cohesion behaviour.")]
        public float CohesionWeight = 1.0f;

        [Header("Avoidance")]
        [Tooltip("Wether to do the physics based obstacle avoidance behaviour or not.")]
        public bool DoAvoidance = false;
        [Tooltip("The type of movement to apply for the avoidance behaviour.")]
        public MovementType AvoidanceMovementType = MovementType.Steering;
        [Tooltip("The distance at which to detect obstacles.")]
        public float AvoidanceDistance = 10.0f;
        [Tooltip("The distance at which to force this behaviour above all other behaviours. For instance in emergency obstacle avoidance scenarios.")]
        public float ForceAvoidanceDistance = 3.0f;
        [Tooltip("The radius of the sphere cast to use for avoidance collision detection.")]
        public float AvoidanceSphereCastRadius = 1.0f;
        [Tooltip("The maximum number of sphere casts to use for avoidance collision detection.")]
        public int AvoidanceMaxSphereCasts = 90;
        [Tooltip("The collider filter to use for avoidance collision detection.")]
        public CollisionFilter AvoidanceColliderFilter = CollisionFilter.Default;
        [Tooltip("How strongly to apply the avoidance behaviour.")]
        public float AvoidanceWeight = 10.0f;

        [Header("Hover")]
        [Tooltip("Wether to do the physics based hover behaviour or not.")]
        public bool DoHover = false;
        [Tooltip("The type of movement to apply for the hover behaviour.")]
        public MovementType HoverMovementType = MovementType.Force;
        [Tooltip("The downward vector for the hovering direction.")]
        public float3 HoverDirection = math.down();
        [Tooltip("The distance to hover above the ground.")]
        public float HoverDistance = 3.0f;
        [Tooltip("The radius of the sphere cast to use for hover collision detection.")]
        public float HoverSphereCastRadius = 1.0f;
        [Tooltip("The collider filter to use for hover collision detection.")]
        public CollisionFilter HoverColliderFilter = CollisionFilter.Default;
        [Tooltip("How strongly to apply the hover behaviour.")]
        public float HoverWeight = 20.0f;

        [Header("Target")]
        [Tooltip("The type of movement to apply for the target behaviour.")]
        public MovementType TargetMovementType = MovementType.Steering;
        [Tooltip("The minimum distance a target must be from a swarm, for it to affect it.")]
        public float MinimumTargetDistance = 15.0f;
        [Tooltip("How strongly to apply the target behaviour.")]
        public float TargetWeight = 0.3f;

        [Header("Obstacle")]
        [Tooltip("The type of movement to apply for the obstacle behaviour.")]
        public MovementType ObstacleMovementType = MovementType.Steering;
        [Tooltip("The maximum distance an obstacle must be from a swarm, for it to affect it.")]
        public float MaximumObstacleDistance = 15.0f;
        [Tooltip("How strongly to apply the obstacle behaviour.")]
        public float ObstacleWeight = 1.0f;

        [Header("Gravity")]
        [Tooltip("Gravity to apply to the swarm each update. This will be multiplied by deltaTime before being applied to the swarm.")]
        public float3 Gravity = float3.zero;

        [Header("Drag")]
        [Tooltip("Drag to apply to the swarm each update. This will be multiplied by deltaTime before being applied to the swarm.")]
        public float Drag = 0.0f;
    }

    /// <summary>
    /// Bakes the SwarmSettingsAuthoring component into it's entity version.
    /// </summary>
    public class SwarmSettingsBaker : Baker<SwarmSettingsAuthoring>
    {
        public override void Bake(SwarmSettingsAuthoring authoring)
        {
            // You may want to edit this function call in order to add your own custom settings.

            Entity e = GetEntity(TransformUsageFlags.None);

            // Add shared settings component
            AddSharedComponent(e, new SwarmSettings
            {
                MinSpeed = authoring.MinSpeed,
                MaxSpeed = authoring.MaxSpeed,
                Acceleration = authoring.Acceleration,

                DefaultTag = authoring.DefaultTag,
                GlobalUp = authoring.GlobalUp,

                VisionDistance = authoring.VisionDistance,
                VisionAngleRadians = math.radians(authoring.VisionAngleDegrees),

                SeperationMovementType = authoring.SeperationMovementType,
                SeparationWeight = authoring.SeparationWeight,

                AlignmentWeight = authoring.AlignmentWeight,

                CohesionMovementType = authoring.CohesionMovementType,
                CohesionWeight = authoring.CohesionWeight,

                DoAvoidance = authoring.DoAvoidance,
                AvoidanceMovementType = authoring.AvoidanceMovementType,
                AvoidanceDistance = authoring.AvoidanceDistance,
                ForceAvoidanceDistance = authoring.ForceAvoidanceDistance,
                AvoidanceSphereCastRadius = authoring.AvoidanceSphereCastRadius,
                AvoidanceMaxSphereCasts = authoring.AvoidanceMaxSphereCasts,
                AvoidanceColliderFilter = authoring.AvoidanceColliderFilter,
                AvoidanceWeight = authoring.AvoidanceWeight,

                DoHover = authoring.DoHover,
                HoverMovementType = authoring.HoverMovementType,
                HoverDirection = authoring.HoverDirection,
                HoverDistance = authoring.HoverDistance,
                HoverSphereCastRadius = authoring.HoverSphereCastRadius,
                HoverColliderFilter = authoring.HoverColliderFilter,
                HoverWeight = authoring.HoverWeight,

                TargetMovementType = authoring.TargetMovementType,
                MinimumTargetDistance = authoring.MinimumTargetDistance,
                TargetWeight = authoring.TargetWeight,

                ObstacleMovementType = authoring.ObstacleMovementType,
                MaximumObstacleDistance = authoring.MaximumObstacleDistance,
                ObstacleWeight = authoring.ObstacleWeight,

                Gravity = authoring.Gravity,

                Drag = authoring.Drag,
            });

            // Add individual swarm data component (values are set in the SwarmSpawnerSystem)
            AddComponent(e, new SwarmData());
        }
    }
}
