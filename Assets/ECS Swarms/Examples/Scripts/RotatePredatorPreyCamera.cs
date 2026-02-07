using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSwarms
{
    /// <summary>
    /// This class is only used to rotate the camera for the PredatorPrey example scene.
    /// </summary>
    public class RotatePredatorPreyCamera : MonoBehaviour
    {
        /// <summary>
        /// The speed at which the camera should rotate. Measured in degrees per second.
        /// </summary>
        public float speed = 5.0f;

        /// <summary>
        /// Called after all Update functions have been called.
        /// </summary>
        void LateUpdate()
        {
            transform.Rotate(new Vector3(0, speed * Time.deltaTime, 0));
        }
    }
}