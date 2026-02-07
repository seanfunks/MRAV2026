using Unity.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using System.Diagnostics;
using Unity.Burst;

namespace ECSSwarms
{
    /// <summary>
    /// The main system for calculating all the behaviours of the swarms, doing the spatial partitioning, and for updating their data (position, velocity, etc.). 
    /// </summary>
    [BurstCompile]
    public partial class SwarmSystem : SystemBase
    {
        private EntityQuery nodeQuery;
        private EntityQuery swarmQuery;

        private List<SwarmSettings> uniqueSettings = new List<SwarmSettings>();

        /// <summary>
        /// A constant collection used for iterating over all adjacent spatial partitions.
        /// </summary>
        readonly static int3[] adjacencies = new int3[] 
        { 
            new int3(-1, -1, -1), new int3(-1, -1, 0), new int3(-1, -1, 1), new int3(-1, 0, -1), new int3(-1, 0, 0), new int3(-1, 0, 1), new int3(-1, 1, -1), new int3(-1, 1, 0), new int3(-1, 1, 1), 
            new int3(0, -1, -1), new int3(0, -1, 0), new int3(0, -1, 1), new int3(0, 0, -1), new int3(0, 0, 0), new int3(0, 0, 1), new int3(0, 1, -1), new int3(0, 1, 0), new int3(0, 1, 1), 
            new int3(1, -1, -1), new int3(1, -1, 0), new int3(1, -1, 1), new int3(1, 0, -1), new int3(1, 0, 0), new int3(1, 0, 1), new int3(1, 1, -1), new int3(1, 1, 0), new int3(1, 1, 1) 
        };

        /// <summary>
        /// Called every frame as long as there is a single swarm in the swarm query. Does the main calculations for each swarm, also updates their data (position, velocity, etc.).
        /// </summary>
        protected override void OnUpdate()
        {
            EntityManager.GetAllUniqueSharedComponentsManaged(uniqueSettings);

            float deltaTime = World.Time.DeltaTime;

            int totalSwarmCount = swarmQuery.CalculateEntityCount();
            int totalNodeCount = nodeQuery.CalculateEntityCount();

            if (totalSwarmCount == 0) // no swarms at all, just stop
                return;

            // Find the highest vision distance in all the settings, this will be used as the cell size for the spatial partitioning
            float cellSize = 0.0f;
            for (int i = 0; i < uniqueSettings.Count; i++)
            {
                if (uniqueSettings[i].VisionDistance > cellSize)
                {
                    cellSize = uniqueSettings[i].VisionDistance;
                }
            }

            NativeParallelMultiHashMap<int3, int> spatialPartitioning = new NativeParallelMultiHashMap<int3, int>(totalSwarmCount, World.Unmanaged.UpdateAllocator.ToAllocator);

            NativeArray<float3> swarmPosition = CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(totalSwarmCount, ref World.Unmanaged.UpdateAllocator, NativeArrayOptions.UninitializedMemory);
            NativeArray<float3> swarmVelocity = CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(totalSwarmCount, ref World.Unmanaged.UpdateAllocator, NativeArrayOptions.UninitializedMemory);
            NativeArray<SwarmTag> swarmTag = CollectionHelper.CreateNativeArray<SwarmTag, RewindableAllocator>(totalSwarmCount, ref World.Unmanaged.UpdateAllocator, NativeArrayOptions.UninitializedMemory);

            NativeArray<float3> nodePositions = CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(totalNodeCount, ref World.Unmanaged.UpdateAllocator, NativeArrayOptions.UninitializedMemory);
            NativeArray<SwarmNodeTag> nodeTags = CollectionHelper.CreateNativeArray<SwarmNodeTag, RewindableAllocator>(totalNodeCount, ref World.Unmanaged.UpdateAllocator, NativeArrayOptions.UninitializedMemory);

            JobHandle mainBarrier = Entities
                .WithName("CopyNodePositionsJob")
                .WithAll<SwarmNode>()
                .ForEach((int entityInQueryIndex, in LocalToWorld localToWorld) =>
                {
                    nodePositions[entityInQueryIndex] = localToWorld.Position;
                })
                .ScheduleParallel(Dependency);

            JobHandle copyNodeTagHandleg = Entities
                .WithName("CopyNodeTagJob")
                .WithAll<SwarmNode>()
                .ForEach((int entityInQueryIndex, in SwarmNode node, in LocalToWorld localToWorld) =>
                {
                    nodeTags[entityInQueryIndex] = node.Tag;
                })
                .ScheduleParallel(Dependency);

            JobHandle setPositionJobHandle = Entities
                .WithName("SetPositionJob")
                .WithAll<SwarmSettings>()
                .ForEach((int entityInQueryIndex, in LocalToWorld localToWorld) =>
                {
                    swarmPosition[entityInQueryIndex] = localToWorld.Position;
                })
                .ScheduleParallel(Dependency);

            JobHandle setVelocityJobHandle = Entities
                .WithName("SetVelocityJob")
                .WithAll<SwarmSettings>()
                .ForEach((int entityInQueryIndex, in SwarmData data) =>
                {
                    swarmVelocity[entityInQueryIndex] = data.Velocity;
                })
                .ScheduleParallel(Dependency);

            JobHandle setTagJobHandle = Entities
                .WithName("SetTagJob")
                .WithAll<SwarmSettings>()
                .ForEach((int entityInQueryIndex, in SwarmData data) =>
                {
                    swarmTag[entityInQueryIndex] = data.Tag;
                })
                .ScheduleParallel(Dependency);

            var parallelPartitioning = spatialPartitioning.AsParallelWriter();
            JobHandle setPartitioningJobHandle = Entities
                .WithName("SetPartitioningJob")
                .WithAll<SwarmSettings>()
                .ForEach((int entityInQueryIndex, in LocalToWorld localToWorld) =>
                {
                    parallelPartitioning.Add(new int3(math.floor(localToWorld.Position / cellSize)), entityInQueryIndex);
                })
                .ScheduleParallel(Dependency);

            mainBarrier = JobHandle.CombineDependencies(mainBarrier, copyNodeTagHandleg);
            mainBarrier = JobHandle.CombineDependencies(mainBarrier, setPositionJobHandle, setVelocityJobHandle);
            mainBarrier = JobHandle.CombineDependencies(mainBarrier, setTagJobHandle, setPartitioningJobHandle);

            for (int settingsIdx = 0; settingsIdx < uniqueSettings.Count; settingsIdx++)
            {
                SwarmSettings settings = uniqueSettings[settingsIdx];
                swarmQuery.AddSharedComponentFilter(settings);

                int swarmCount = swarmQuery.CalculateEntityCount();

                if (swarmCount == 0) // no swarms with this setting, continue
                {
                    swarmQuery.ResetFilter();
                    continue;
                }

                JobHandle main = Entities
                    .WithName("MainSwarmLoop")
                    .WithSharedComponentFilter(settings)
                    .WithReadOnly(swarmPosition)
                    .WithReadOnly(swarmVelocity)
                    .WithReadOnly(swarmTag)
                    .WithReadOnly(nodePositions)
                    .WithReadOnly(nodeTags)
                    .WithReadOnly(spatialPartitioning)
                    .ForEach((int entityInQueryIndex, int nativeThreadIndex, ref SwarmData data, ref LocalToWorld localToWorld) =>
                    {
                        SwarmBehaviour behaviour = new SwarmBehaviour(settings, localToWorld.Position, data.Velocity, data.Tag);

                        int3 thisPartition = new int3(math.floor(localToWorld.Position / cellSize));

                        for (int adj = 0; adj < adjacencies.Length; adj++)
                        {
                            foreach (int i in spatialPartitioning.GetValuesForKey(thisPartition + adjacencies[adj]))
                            {
                                if (i != entityInQueryIndex)
                                {
                                    behaviour.ForEachSwarmInAdjacentPartitions(swarmPosition[i], swarmVelocity[i], swarmTag[i]);
                                }
                            }
                        }

                        behaviour.Finish(deltaTime, data.PhysicsResult, data.ForcePhysics, nodePositions, nodeTags);

                        data.Velocity = behaviour.velocity;
                        data.Tag = behaviour.tag;

                        localToWorld.Value = float4x4.TRS(
                            new float3(behaviour.position),
                            quaternion.LookRotationSafe(data.Velocity, settings.GlobalUp ? math.up() : localToWorld.Up),
                            new float3(1.0f, 1.0f, 1.0f));
                    })
                    .ScheduleParallel(mainBarrier);

                mainBarrier = JobHandle.CombineDependencies(mainBarrier, main);

                swarmQuery.AddDependency(mainBarrier);
                swarmQuery.ResetFilter();
            }

            Dependency = mainBarrier;

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

            nodeQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<SwarmNode>(), ComponentType.ReadOnly<LocalToWorld>() },
            });

            RequireForUpdate(swarmQuery);
        }
    }
}