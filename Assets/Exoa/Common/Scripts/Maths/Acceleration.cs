
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Maths
{
    [System.Serializable]
    public class Acceleration
    /// <summary>
    /// A class that computes and stores the acceleration of an object based on its positional changes over time.
    /// It maintains a register of positions and associated timestamps to calculate acceleration.
    /// </summary>
    {

        private Vector3[] positionRegister;
        private float[] posTimeRegister;
        private int positionSamplesTaken = 0;
        public int samples = 22;
        public float maxMagnitude;
        private Vector3 outputAcceleration;

        public Vector3 OutputAcceleration
        {
            get
            /// <summary>
            /// Gets the output acceleration while ensuring it does not exceed the maximum magnitude.
            /// </summary>
            /// <returns>The calculated output acceleration, clamped to maxMagnitude if necessary.</returns>
            {
                if (maxMagnitude > 0 && outputAcceleration.magnitude > maxMagnitude)
                    return outputAcceleration.normalized * maxMagnitude;
                return outputAcceleration;
            }
        }

        public void Clear()
        /// <summary>
        /// Clears the position and time registers, resetting the sample count.
        /// </summary>
        {
            posTimeRegister = null;
            positionRegister = null;
            positionSamplesTaken = 0;
        }

        public bool AddPosition(Vector3 position, bool debug = false, bool mouseUp = false)
        /// <summary>
        /// Adds a new position sample and calculates the acceleration based on stored samples.
        /// Updates the registers and computes acceleration if enough samples have been taken.
        /// </summary>
        /// <param name="position">The new position to be added.</param>
        /// <param name="debug">Flag to enable debug logging.</param>
        /// <param name="mouseUp">Flag indicating if the mouse button is released.</param>
        /// <returns>Returns true if acceleration could be calculated, false otherwise.</returns>
        {
            Vector3 averageSpeedChange = Vector3.zero;
            outputAcceleration = Vector3.zero;
            Vector3 deltaDistance;
            float deltaTime;
            Vector3 speedA;
            Vector3 speedB;

            //Clamp sample amount. In order to calculate acceleration we need at least 2 changes
            //in speed, so we need at least 3 position samples.
            if (samples < 3)
            {
                samples = 3;
            }

            //Initialize
            if (positionRegister == null)
            {

                positionRegister = new Vector3[samples];
                posTimeRegister = new float[samples];
            }
            if (mouseUp && positionRegister.Length > 0)
            {
                position = positionRegister[positionRegister.Length - 1];
            }
            else if (!mouseUp && positionRegister.Length > 0 && positionRegister[positionRegister.Length - 1] == position)
            {
                return false;
            }
            //Fill the position and time sample array and shift the location in the array to the left
            //each time a new sample is taken. This way index 0 will always hold the oldest sample and the
            //highest index will always hold the newest sample. 
            for (int i = 0; i < positionRegister.Length - 1; i++)
            {

                positionRegister[i] = positionRegister[i + 1];
                posTimeRegister[i] = posTimeRegister[i + 1];
            }
            positionRegister[positionRegister.Length - 1] = position;
            posTimeRegister[posTimeRegister.Length - 1] = Time.time;

            positionSamplesTaken++;
            if (debug) Debug.Log("Linear positionSamplesTaken:" + positionSamplesTaken + " positionRegister.Length:" + positionRegister.Length);

            //The output acceleration can only be calculated if enough samples are taken.
            if (positionSamplesTaken >= 3)
            {
                List<float> timeList = new List<float>();
                //Calculate average speed change.
                for (int i = 0; i < positionRegister.Length - 2; i++)
                {
                    if (debug)
                    {
                        Debug.Log("Linear " + i + " t:" + posTimeRegister[i] + " p:" + positionRegister[i].Dump());
                    }

                    deltaDistance = positionRegister[i + 1] - positionRegister[i];
                    deltaTime = posTimeRegister[i + 1] - posTimeRegister[i];

                    //If deltaTime is 0, the output is invalid.
                    if (deltaTime == 0 || posTimeRegister[i] == 0 || deltaDistance == Vector3.zero)
                    {
                        continue;
                    }
                    if (debug)
                    {
                        Debug.Log("Linear " + i + " deltaDistance:" + deltaDistance.magnitude + " deltaTime:" + deltaTime);
                    }
                    speedA = deltaDistance / deltaTime;
                    deltaDistance = positionRegister[i + 2] - positionRegister[i + 1];
                    deltaTime = posTimeRegister[i + 2] - posTimeRegister[i + 1];

                    if (deltaTime == 0 || deltaDistance == Vector3.zero)
                    {
                        continue;
                    }

                    speedB = deltaDistance / deltaTime;

                    //This is the accumulated speed change at this stage, not the average yet.
                    averageSpeedChange += speedB - speedA;
                    timeList.Add(posTimeRegister[i]);
                }
                if (timeList.Count == 0)
                    return false;
                if (debug) Debug.Log("Linear averageSpeedChange:" + averageSpeedChange);
                //Now this is the average speed change.
                averageSpeedChange /= timeList.Count - 2;

                //Get the total time difference.
                if (debug)
                {
                    Debug.Log("A " + timeList[timeList.Count - 1]);
                    Debug.Log("B " + timeList[0]);
                }
                float deltaTimeTotal = timeList[timeList.Count - 1] - timeList[0];
                if (deltaTimeTotal == 0)
                    return false;

                //Now calculate the acceleration, which is an average over the amount of samples taken.
                outputAcceleration = averageSpeedChange / deltaTimeTotal;

                if (debug)
                {
                    Debug.Log("Linear count:" + timeList.Count);
                    Debug.Log("Linear deltaTimeTotal:" + deltaTimeTotal);
                    Debug.Log("Linear averageSpeedChange:" + averageSpeedChange.Dump());
                    Debug.Log("Linear vector:" + outputAcceleration.Dump());
                }
                return true;
            }

            else
            {

                return false;
            }
        }
    }
}
