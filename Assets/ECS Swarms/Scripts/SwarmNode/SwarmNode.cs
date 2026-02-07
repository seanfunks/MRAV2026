using System;
using Unity.Entities;
using Unity.Transforms;

namespace ECSSwarms
{
    /// <summary>
    /// A 'node' with a tag and position that can influence the behaviour of swarms.
    /// </summary>
    [Serializable]
    [WriteGroup(typeof(LocalToWorld))]
    public struct SwarmNode : IComponentData 
    {
        /// <summary>
        /// The tag for this node. This will affect how swarms interact with this node. For example, a tag of Target will attract swarms, but a tag of Obstacle will repel swarms.
        /// </summary>
        public SwarmNodeTag Tag;
    }
}