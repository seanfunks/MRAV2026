using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

namespace ECSSwarms
{
    /// <summary>
    /// The authoring component for SwarmNode. A SwarmNode is a 'node' with a tag and position that can influence the behaviour of swarms.
    /// </summary>
    [AddComponentMenu("Swarm Toolkit/Swarm Node")]
    public class SwarmNodeAuthoring : MonoBehaviour
    {
        [Tooltip("The tag of the node. This will affect how swarms interact with this node. For example, a tag of Target will attract swarms, but a tag of Obstacle will repel swarms.")]
        public SwarmNodeTag Tag;
    }

    /// <summary>
    /// Bakes the SwarmNodeAuthoring component into it's entity version.
    /// </summary>
    public class SwarmNodeBaker : Baker<SwarmNodeAuthoring>
    {
        public override void Bake(SwarmNodeAuthoring authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new SwarmNode
            {
                Tag = authoring.Tag
            });
        }
    }
}