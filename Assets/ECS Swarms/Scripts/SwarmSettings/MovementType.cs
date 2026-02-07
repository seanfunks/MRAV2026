namespace ECSSwarms
{
    /// <summary>
    /// The mode in which to apply a movement vector.
    /// </summary>
    public enum MovementType
    {
        /// <summary>
        /// Calculate movement by steering the current velocity towards the vector.
        /// </summary>
        Steering,
        /// <summary>
        /// Calculate movement by applying the movement vector as a force.
        /// </summary>
        Force
    }
}
