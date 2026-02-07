using System;

namespace ECSSwarms
{
    /// <summary>
    /// A tag that a swarm can have. Can be used to make different swarms to interact differently with each other (ex. Predator/Prey).
    /// </summary>
    [Serializable]
    public enum SwarmTag
    {
        /// <summary>
        /// The default tag for swarms. This will not influence the behaviour of the swarms at all.
        /// </summary>
        Default,
        /// <summary>
        /// A predator tag to be used in a predator/prey simulation. This will cause the predators to chase prey.
        /// </summary>
        Predator,
        /// <summary>
        /// A prey tag to be used in a predator/prey simulation. This will cause the prey to flee from predators.
        /// </summary>
        Prey
    }
}
