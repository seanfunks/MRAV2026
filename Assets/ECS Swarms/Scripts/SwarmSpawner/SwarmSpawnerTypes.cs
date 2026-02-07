using System;

namespace ECSSwarms
{
    /// <summary>
    /// The different shapes that a SwarmSpawner can use to spawn.
    /// </summary>
    [Serializable]
    public enum SwarmSpawnerShape
    {
        /// <summary>
        /// Spawn the swarms in a cube shape. I.e., filling up the entire bounds of the spawner.
        /// </summary>
        Cube,
        /// <summary>
        /// Spawns the swarms in a ellipsoid shape (stretched sphere). This sphere will stretch to fit the bounds of the spawner.
        /// </summary>
        Ellipsoid
    }

    /// <summary>
    /// The different ways that a swarms direction can be set by a SwarmSpawner.
    /// </summary>
    [Serializable]
    public enum SwarmSpawnerDirection
    {
        /// <summary>
        /// Every swarms direction will be completely randomly initialized.
        /// </summary>
        Random,
        /// <summary>
        /// All the swarms direction will be initilized so that they face towards the center of the bounds of the spawner.
        /// </summary>
        Inward,
        /// <summary>
        /// All the swarms direction will be initilized so that they face away from the center of the bounds of the spawner.
        /// </summary>
        Outward,
        /// <summary>
        /// All the swarms direction will be left as the default. This means they will stay facing the same direction as their prefab.
        /// </summary>
        Default
    }
}
