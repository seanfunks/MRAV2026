using Unity.Entities;
using Unity.Mathematics;

namespace ECSSwarms
{
    /// <summary>
    /// A component used to spawn various quantites of swarms. Contains attributes for changing the shape and direction that the swarms are spawned in.
    /// </summary>
    public struct SwarmSpawner : IComponentData
    {
        /// <summary>
        /// The swarm entity to spawn.
        /// </summary>
        public Entity Prefab;
        /// <summary>
        /// The shape in which to spawn the swarms.
        /// </summary>
        public SwarmSpawnerShape Shape;
        /// <summary>
        /// How the directions of the swarms should be initialized.
        /// </summary>
        public SwarmSpawnerDirection Direction;
        /// <summary>
        /// The bounds of the area to spawn swarms in.
        /// </summary>
        public float3x2 Bounds;
        /// <summary>
        /// The number of swarms to spawn.
        /// </summary>
        public int Count;
    }
}