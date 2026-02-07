using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECSSwarms
{
    /// <summary>
    /// An authoring component for SwarmSpawner. A SpawnSpawner is a component used to spawn various quantites of swarms. It contains attributes 
    /// for changing the shape and direction that the swarms are spawned in.
    /// </summary>
    [AddComponentMenu("Swarm Toolkit/Swarm Spawner")]
    public class SwarmSpawnerAuthoring : MonoBehaviour
    {
        [Tooltip("A prefab of the swarm to spawn.")]
        public GameObject Prefab;
        [Tooltip("The shape in which to spawn the swarms.")]
        public SwarmSpawnerShape Shape = SwarmSpawnerShape.Cube;
        [Tooltip("How the directions of the swarms should be initialized.")]
        public SwarmSpawnerDirection Direction = SwarmSpawnerDirection.Random;
        [Tooltip("The bounds of the area to spawn swarms in.")]
        public Bounds SpawnBounds = new Bounds(Vector3.zero, new Vector3(10, 10, 10));
        [Tooltip("The number of swarms to spawn.")]
        public int Count = 1000;

        /// <summary>
        /// Draws the gizmos for this object to the editor scene. This will draw a wire cube around the bounds of the spawner.
        /// </summary>
        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(SpawnBounds.center + transform.position, SpawnBounds.size);
        }
    }

    /// <summary>
    /// Bakes the SwarmSpawnerAuthoring component into it's entity version.
    /// </summary>
    public class SwarmSpawnerBaker : Baker<SwarmSpawnerAuthoring>
    {
        public override void Bake(SwarmSpawnerAuthoring authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.None);
            AddComponent(e, new SwarmSpawner
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                Shape = authoring.Shape,
                Direction = authoring.Direction,
                Bounds = new float3x2(authoring.SpawnBounds.min + authoring.transform.position, authoring.SpawnBounds.max + authoring.transform.position),
                Count = authoring.Count
            });
        }
    }
}
