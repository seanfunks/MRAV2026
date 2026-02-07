using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECSSwarms
{
    /// <summary>
    /// The system for spawning all the swarms for their corresponding SwarmSpawner components. Once a SwarmSpawner is used to spawn swarms, it is destroyed.
    /// </summary>
    //[RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct SwarmSpawnerSystem : ISystem
    {
        /// <summary>
        /// Called every frame. Gets every SwarmSpawner, spawns its swarms, then deletes the SwarmSpawner entity.
        /// </summary>
        public void OnUpdate(ref SystemState state)
        {
            ComponentLookup<LocalToWorld> localToWorldDict = SystemAPI.GetComponentLookup<LocalToWorld>();
            ComponentLookup<SwarmData> dataDict = SystemAPI.GetComponentLookup<SwarmData>();

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var world = state.World.Unmanaged;

            foreach (var (spawner, entity) in SystemAPI.Query<RefRO<SwarmSpawner>>().WithEntityAccess())
            {
                NativeArray<Entity> newSwarms = CollectionHelper.CreateNativeArray<Entity, RewindableAllocator>(spawner.ValueRO.Count, ref world.UpdateAllocator);

                state.EntityManager.Instantiate(spawner.ValueRO.Prefab, newSwarms);
                SwarmSettings settings = state.EntityManager.GetSharedComponentManaged<SwarmSettings>(spawner.ValueRO.Prefab);

                SetSwarmLocalToWorld setLocalToWorldJob = new SetSwarmLocalToWorld
                {
                    LocalToWorldFromEntity = localToWorldDict,
                    DataFromEntity = dataDict,
                    NewSwarms = newSwarms,

                    Bounds = spawner.ValueRO.Bounds,
                    Shape = spawner.ValueRO.Shape,
                    Direction = spawner.ValueRO.Direction,
                    Settings = settings
                };
                state.Dependency = setLocalToWorldJob.Schedule(spawner.ValueRO.Count, 64, state.Dependency);
                state.Dependency.Complete();

                ecb.DestroyEntity(entity);
            }

            ecb.Playback(state.EntityManager);
        }
    }

    /// <summary>
    /// A parallel for loop job used to set each swarms data and local to world properties. Spawns the swarms randomly according to the 
    /// relevant settings in the SwarmSpawner.
    /// </summary>
    [BurstCompile()]
    struct SetSwarmLocalToWorld : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction]
        [NativeDisableParallelForRestriction]
        public ComponentLookup<LocalToWorld> LocalToWorldFromEntity;

        [NativeDisableContainerSafetyRestriction]
        [NativeDisableParallelForRestriction]
        public ComponentLookup<SwarmData> DataFromEntity;

        public NativeArray<Entity> NewSwarms;
        public float3x2 Bounds;
        public SwarmSpawnerShape Shape;
        public SwarmSpawnerDirection Direction;
        public SwarmSettings Settings;

        /// <summary>
        /// Called in parallel for each index of every swarm to spawn. Each call to this function will set the releveant data for the swarm with the corresponding index.
        /// </summary>
        /// <param name="i">The index to use.</param>
        public void Execute(int i)
        {
            Entity entity = NewSwarms[i];

            Random random = new Random((uint)(entity.Index + i) * 0x45d9f3b);

            float3 randomPos;
            if (Shape == SwarmSpawnerShape.Cube)
            {
                randomPos = random.NextFloat3() * (Bounds.c1 - Bounds.c0) + Bounds.c0;
            }
            else // ellipsoid
            {
                float3 point = random.NextFloat3();
                while (math.length(point - new float3(0.5f, 0.5f, 0.5f)) > 0.5f)
                {
                    point = random.NextFloat3();
                }
                randomPos = math.lerp(Bounds.c0, Bounds.c1, point);
            }

            float3 randomDir;
            if (Direction == SwarmSpawnerDirection.Random)
            {
                randomDir = math.normalizesafe(random.NextFloat3() - new float3(0.5f, 0.5f, 0.5f));
            }
            else if (Direction == SwarmSpawnerDirection.Outward)
            {
                randomDir = math.normalizesafe(randomPos - (Bounds.c0 + Bounds.c1) * 0.5f);
            }
            else if (Direction == SwarmSpawnerDirection.Inward)
            {
                randomDir = math.normalizesafe((Bounds.c0 + Bounds.c1) * 0.5f - randomPos);
            }
            else // default
            {
                randomDir = LocalToWorldFromEntity[entity].Forward;
            }

            quaternion rot = quaternion.LookRotationSafe(randomDir, math.up());

            DataFromEntity[entity] = new SwarmData
            {
                Velocity = randomDir * Settings.MinSpeed,
                Tag = Settings.DefaultTag,

                PhysicsResult = float3.zero,
                ForcePhysics = false
            };

            LocalToWorldFromEntity[entity] = new LocalToWorld
            {
                Value = float4x4.TRS(randomPos, rot, new float3(1.0f, 1.0f, 1.0f))
            };
        }
    }
}