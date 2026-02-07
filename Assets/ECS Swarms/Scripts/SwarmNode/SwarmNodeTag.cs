namespace ECSSwarms
{
    /// <summary>
    /// Tags that a SwarmNode can have. Ex. Target which causes swarms to be attracted to the node's position, Obstacle which 
    /// causes swarms to be repeled by the node's position.
    /// </summary>
    public enum SwarmNodeTag
    {
        /// <summary>
        /// Designates this node as a target. This will cause swarms to be attracted to it.
        /// </summary>
        Target,
        /// <summary>
        /// Designates this node as an obstacle. This will cause swarms to be repelled by it.
        /// </summary>
        Obstacle
    }
}