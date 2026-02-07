using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;

namespace ECSSwarms
{
    /// <summary>
    /// <para>The system for calculating all the physics based behaviours of the swarms. The reason these must be split from the regular behaviours is because
    /// the physics system can only be queried in certain update groups. </para>
    /// <para>You may want to edit this if you choose to implement your own custom physics based behaviours.</para>
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [BurstCompile]
    public partial class SwarmPhysicsSystem : SystemBase
    {
        private EntityQuery swarmQuery;

        private List<SwarmSettings> uniqueSettings = new List<SwarmSettings>();

        /// <summary>
        /// Called every frame as long as there is a single swarm in the swarm query. Does the physics based calculations for the swarms.
        /// </summary>
        protected override void OnUpdate()
        {
            EntityManager.GetAllUniqueSharedComponentsManaged(uniqueSettings);

            for (int settingsIdx = 0; settingsIdx < uniqueSettings.Count; settingsIdx++)
            {
                SwarmSettings settings = uniqueSettings[settingsIdx];
                swarmQuery.AddSharedComponentFilter(settings);

                int swarmCount = swarmQuery.CalculateEntityCount();

                if (swarmCount == 0 || (!settings.DoAvoidance && !settings.DoHover)) // no swarms with this setting, continue
                {
                    swarmQuery.ResetFilter();
                    continue;
                }

                CollisionWorld world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld.CollisionWorld;

                JobHandle physicsLoopJob = Entities
                    .WithName("SwarmPhysicsLoop")
                    .WithSharedComponentFilter(settings)
                    .WithReadOnly(world)
                    .ForEach((int entityInQueryIndex, ref SwarmData data, in LocalToWorld localToWorld) =>
                    {
                        data.PhysicsResult = float3.zero;
                        data.ForcePhysics = false;

                        if (settings.DoAvoidance)
                        {
                            (float3 result, bool force) = SwarmAvoidance.DoAvoidance(world, settings, localToWorld.Rotation, localToWorld.Position);
                            data.PhysicsResult += (settings.AvoidanceMovementType == MovementType.Force ? result : SwarmBehaviour.SteerTowards(data.Velocity, result, settings)) * settings.AvoidanceWeight;
                            data.ForcePhysics = data.ForcePhysics | force;
                        }
                        if (settings.DoHover)
                        {
                            (float3 result, bool force) = SwarmHover.DoHover(world, settings, localToWorld.Rotation, localToWorld.Position);
                            data.PhysicsResult += (settings.HoverMovementType == MovementType.Force ? result : SwarmBehaviour.SteerTowards(data.Velocity, result, settings)) * settings.HoverWeight;
                            data.ForcePhysics = data.ForcePhysics | force;
                        }
                    })
                    .ScheduleParallel(Dependency);

                physicsLoopJob.Complete(); // Must be called because it interacts with physics
                swarmQuery.ResetFilter();
            }

            uniqueSettings.Clear();
        }

        /// <summary>
        /// Called when this system is created.
        /// </summary>
        protected override void OnCreate()
        {
            swarmQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<SwarmSettings>(), ComponentType.ReadWrite<SwarmData>(), ComponentType.ReadWrite<LocalToWorld>() },
            });
        }
    }
}