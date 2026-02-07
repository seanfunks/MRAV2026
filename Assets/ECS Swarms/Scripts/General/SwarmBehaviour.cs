using Unity.Collections;
using Unity.Mathematics;

namespace ECSSwarms
{
    /// <summary>
    /// <para>A struct that is created once per update cycle for each swarm, and is used to calculate and set all the behaviours for the swarms.</para>
    /// <para>This is likely the struct you would want to edit if you wanted to add new behaviours.</para>
    /// <para>The lifecycle of a SwarmBehaviour struct is as follows: for each swarm, a SwarmBehaviour is created via the constructor, then 
    /// ForEachSwarmInAdjacentPartitions is called for every other swarm in the adjacent sparse spatial partitions, after all other swarms
    /// have been iterated through Finish is then called. After Finish is called, the position, velocity, and tag properties of the 
    /// SwarmBehaviour are read and used to set the relevant data for the swarm.</para>
    /// </summary>
    public struct SwarmBehaviour
    {
        /// <summary>
        /// Constructor for swarm behaviour.
        /// </summary>
        /// <param name="settings">The settings for this swarm.</param>
        /// <param name="position">The initial position for this swarm.</param>
        /// <param name="velocity">The initial velocity for this swarm.</param>
        /// <param name="tag">The initial tag for this swarm.</param>
        public SwarmBehaviour(SwarmSettings settings, float3 position, float3 velocity, SwarmTag tag)
        {
            this.position = position;
            this.velocity = velocity;
            this.tag = tag;

            this.settings = settings;

            currentSpeed = math.length(velocity);
            seperationVector = float3.zero;
            sumPosition = float3.zero;
            sumVelocity = float3.zero;
            inVisionCount = 0;
        }

        // These variables are used directly by the SwarmSystem.
        // After the call to Finish, these three values are read and the corresponding values for the swarms are set to match these.

        /// <summary>
        /// The position of the swarm. This value is read after Finish is called and used to set the position of the swarm.
        /// </summary>
        public float3 position;
        /// <summary>
        /// The velocity of the swarm. This value is read after Finish is called and used to set the velocity of the swarm.
        /// </summary>
        public float3 velocity;
        /// <summary>
        /// The tag of the swarm. This value is read after Finish is called and used to set the tag of the swarm.
        /// </summary>
        public SwarmTag tag;

        // These are variables local to this struct. They are not read outside of this struct.

        private SwarmSettings settings;
        private float currentSpeed;
        private float3 seperationVector;
        private float3 sumPosition;
        private float3 sumVelocity;
        private int inVisionCount;

        /// <summary>
        /// Called once for every other swarm in adjacent sparse spatial partitions. Used to calculate the relevant behaviours for the swarm.
        /// </summary>
        /// <param name="otherPosition">The position of the other swarm.</param>
        /// <param name="otherVelocity">The velocity of the other swarm.</param>
        /// <param name="otherTag">The tag of the other swarm.</param>
        public void ForEachSwarmInAdjacentPartitions(float3 otherPosition, float3 otherVelocity, SwarmTag otherTag)
        {
            float3 difference = position - otherPosition;
            float distance = math.length(difference);
            float angle = math.acos(math.dot(velocity, -difference) / (currentSpeed * distance));

            if (distance < settings.VisionDistance && angle < settings.VisionAngleRadians)
            {
                if (tag == SwarmTag.Predator && otherTag == SwarmTag.Prey)
                {
                    seperationVector -= math.normalizesafe(difference) * (distance / settings.VisionDistance);
                }
                else if (tag == SwarmTag.Prey && otherTag == SwarmTag.Predator)
                {
                    seperationVector += math.normalizesafe(difference) * 10.0f;
                }
                else
                {
                    seperationVector += math.normalizesafe(difference) * ((settings.VisionDistance - distance) / settings.VisionDistance);
                    sumPosition += otherPosition;
                    sumVelocity += otherVelocity;

                    inVisionCount++;
                }
            }
        }

        /// <summary>
        /// Called after the last ForEachSwarmInAdjacentPartitions call. Used to finalize the behaviours of the swarm. After this function is called,
        /// the position, rotation, and tag of this SwarmBehaviour is read and used to set the corresponding values of the swarm.
        /// </summary>
        /// <param name="deltaTime">The delta time of this update timestep.</param>
        /// <param name="physicsResult">The result of physics based behaviours.</param>
        /// <param name="forcePhysics">If the physics based behaviours should be forced.</param>
        /// <param name="nodePositions">A native array of all the positions for every node.</param>
        /// <param name="nodeTags">A native array of all the tags for every node.</param>
        public void Finish(float deltaTime, float3 physicsResult, bool forcePhysics, NativeArray<float3> nodePositions, NativeArray<SwarmNodeTag> nodeTags)
        {
            deltaTime = math.min(0.05f, deltaTime);

            float3 acceleration = float3.zero;

            if (!forcePhysics)
            {
                if (nodePositions.Length > 0)
                {
                    float3 targetVector = float3.zero;
                    float3 obstacleVector = float3.zero;
                    float closestTarget = 99999999999999.9f;
                    float closestObstacle = 99999999999999.9f;
                    for (int i = 0; i < nodePositions.Length; i++)
                    {
                        if (nodeTags[i] == SwarmNodeTag.Target)
                        {
                            float3 difference = nodePositions[i] - position;
                            float distance = math.lengthsq(difference);

                            if (distance > settings.MinimumTargetDistance * settings.MinimumTargetDistance && distance < closestTarget)
                            {
                                closestTarget = distance;
                                targetVector = difference;
                            }
                        }
                        else if (nodeTags[i] == SwarmNodeTag.Obstacle)
                        {
                            float3 difference = position - nodePositions[i];
                            float distance = math.lengthsq(difference);

                            if (distance < settings.MaximumObstacleDistance * settings.MaximumObstacleDistance && distance < closestObstacle)
                            {
                                obstacleVector = distance;
                                targetVector = difference;
                            }
                        }
                    }

                    if (!targetVector.Equals(float3.zero))
                    {
                        acceleration += (settings.TargetMovementType == MovementType.Force ? math.normalizesafe(targetVector) : SteerTowards(velocity, targetVector, settings)) * settings.TargetWeight;
                    }
                    if (!obstacleVector.Equals(float3.zero))
                    {
                        acceleration += (settings.ObstacleMovementType == MovementType.Force ? math.normalizesafe(obstacleVector) : SteerTowards(velocity, obstacleVector, settings)) * settings.ObstacleWeight;
                    }
                }

                if (inVisionCount > 0)
                {
                    acceleration +=
                        (settings.SeperationMovementType == MovementType.Force ? seperationVector : SteerTowards(velocity, seperationVector, settings)) * settings.SeparationWeight +
                        SteerTowards(velocity, sumVelocity, settings) * settings.AlignmentWeight +
                        (settings.CohesionMovementType == MovementType.Force ? (inVisionCount == 0 ? float3.zero : sumPosition / inVisionCount - position) : SteerTowards(velocity, sumPosition - position * inVisionCount, settings)) * settings.CohesionWeight;
                }
            }

            if (settings.DoAvoidance || settings.DoHover)
            {
                acceleration += physicsResult;
            }

            velocity = ClampMagnitude(velocity + (acceleration + settings.Gravity - math.normalizesafe(velocity) * settings.Drag) * deltaTime, settings.MinSpeed, settings.MaxSpeed);
            position = position + velocity * deltaTime;
        }

        /*######################################################
        #################  Helper Functions  ###################
        ######################################################*/

        /// <summary>
        /// Returns a float3 representing the given velocity steering towards the direction of the vector.
        /// </summary>
        /// <param name="velocity">The current velocity.</param>
        /// <param name="vector">The vector to steer in the direction of.</param>
        /// <param name="settings">The settings for the swarm. Used to access the acceleration property.</param>
        /// <returns>a float3 representing the given velocity steering towards the direction of the vector</returns>
        public static float3 SteerTowards(float3 velocity, float3 vector, SwarmSettings settings)
        {
            return ClampMagnitude(math.normalizesafe(vector) * settings.MaxSpeed - velocity, settings.Acceleration);
        }

        /// <summary>
        /// Clamps a float3 below a maximum magnitude.
        /// </summary>
        /// <param name="vector">The float3 to clamp.</param>
        /// <param name="max">The maximum magnitude.</param>
        /// <returns>the result of clamping the given float3</returns>
        public static float3 ClampMagnitude(float3 vector, float max)
        {
            float magnitude = math.length(vector);
            return magnitude > max ? (max * vector) / magnitude : vector;
        }

        /// <summary>
        /// Clamps a float3 betwenn a minimum and maximum magnitude.
        /// </summary>
        /// <param name="vector">The float3 to clamp.</param>
        /// <param name="min">The minimum magnitude.</param>
        /// <param name="max">The maximum magnitude.</param>
        /// <returns>the result of clamping the given float3</returns>
        public static float3 ClampMagnitude(float3 vector, float min, float max)
        {
            float magnitude = math.length(vector);
            return magnitude > max ? (max * vector) / magnitude :
                (magnitude < min ? (min * vector) / magnitude : vector);
        }
    }
}