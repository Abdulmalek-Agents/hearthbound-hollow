
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Exoa.Maths
{
    [System.Serializable]
    public class Move
    {
        /// <summary>
        /// Enumeration for different types of movement calculations.
        /// </summary>
        public enum MoveType
        {
            Hyperbolic_Ax3_Bx2_Dx_C,
            Parabolic_Ax2_Bx_C,
            Parabolic2_Ax2_C,
            Linear_Bx_C,
            Logarithmic_Alog_xplusB_C
        }

        public MoveType moveType;
        private Vector3 customForceVector = Vector3.up;
        private Vector3 lastPositionOffset = Vector3.zero;

        /// <summary>
        /// Maximum allowable velocity for the movement.
        /// </summary>
        public float maxVelocity = -1;
        
        /// <summary>
        /// Minimum allowable velocity for the movement.
        /// </summary>
        public float minVelocity = 9999;
        
        private bool hasReachedMaxVelocity = false;
        private float maxVelocityReturn = 0;

        //Used to calculate both the velocity and the area under the curve
        protected float pointOnCurve = 0;
        protected float integralNought = 0;

        /// <summary>
        /// Coefficients for the movement calculations.
        /// </summary>
        public float A = 1;
        public float B = 1;
        public float D = 1;
        public float logBase = 10;

        protected float debugVelocity;
        protected float debugIntegral;
        protected bool active;
        protected BaseVelocityIntegral currentCalc;

        /// <summary>
        /// Initializes a new instance of the Move class with specified parameters.
        /// </summary>
        /// <param name="groundMove">The Move instance to copy parameters from.</param>
        public Move(Move groundMove)
        {
            this.moveType = groundMove.moveType;
            this.maxVelocity = groundMove.maxVelocity;
            this.minVelocity = groundMove.minVelocity;
            this.A = groundMove.A;
            this.B = groundMove.B;
            this.D = groundMove.D;
            this.logBase = groundMove.logBase;
        }

        /// <summary>
        /// Gets or sets the active state of the movement.
        /// </summary>
        public bool Active { get => active; set => active = value; }

        /// <summary>
        /// Initializes the movement with starting speed and direction.
        /// </summary>
        /// <param name="startingSpeed">The initial speed of the movement.</param>
        /// <param name="dir">The direction vector of the movement.</param>
        public void Init(float startingSpeed, Vector3 dir)
        {
            customForceVector = dir;
            lastPositionOffset = Vector3.zero;
            pointOnCurve = 0;

            switch (moveType)
            {
                case MoveType.Linear_Bx_C: currentCalc = new Bx_C(); break;
                case MoveType.Hyperbolic_Ax3_Bx2_Dx_C: currentCalc = new Ax3_Bx2_Dx_C(); break;
                case MoveType.Parabolic_Ax2_Bx_C: currentCalc = new Ax2_Bx_C(); break;
                case MoveType.Parabolic2_Ax2_C: currentCalc = new Ax2_C(); break;
                case MoveType.Logarithmic_Alog_xplusB_C: currentCalc = new AlogXplusB_C(); break;
            }
            currentCalc.Init(A, B, startingSpeed, D, logBase);

            integralNought = CalculateIntegral(pointOnCurve);
            if (float.IsNaN(integralNought))
            {
                pointOnCurve += 0.0001f;
                integralNought = CalculateIntegral(pointOnCurve);
            }
            hasReachedMaxVelocity = false;
            active = true;
            GetVelocity();
        }

        /// <summary>
        /// Gets the current velocity based on the movement calculations.
        /// </summary>
        /// <returns>The current velocity.</returns>
        public float GetVelocity()
        {
            if (hasReachedMaxVelocity) return maxVelocityReturn;
            float result = CalculateVelocity();
            if (maxVelocity > 0 && result > maxVelocity) { return SetMaxVelocityReached(maxVelocity); }
            if (minVelocity < 9999 && result < minVelocity) { return SetMaxVelocityReached(minVelocity); }
            return result;
        }

        /// <summary>
        /// Gets the velocity as a vector based on the force direction.
        /// </summary>
        /// <returns>The velocity vector.</returns>
        public Vector3 GetVelocityOnVector()
        {
            if (hasReachedMaxVelocity) return customForceVector * maxVelocityReturn;
            return GetVelocity() * customForceVector;
        }

        /// <summary>
        /// Calculates the offset position based on the integral value.
        /// </summary>
        /// <returns>The offset position vector.</returns>
        public Vector3 GetOffsetPosition()
        {
            float result = CalculateIntegral(pointOnCurve) - integralNought;
            return result * customForceVector;
        }

        /// <summary>
        /// Calculates the integral of the current point on the curve.
        /// </summary>
        /// <param name="x">The point at which to calculate the integral.</param>
        /// <returns>The integral value at the specified point.</returns>
        protected float CalculateIntegral(float x)
        {
            return currentCalc == null ? 0 : currentCalc.GetIntegral(x);
        }

        /// <summary>
        /// Calculates the velocity based on the current point on the curve.
        /// </summary>
        /// <returns>The velocity value at the current point.</returns>
        protected float CalculateVelocity()
        {
            return currentCalc == null ? 0 : currentCalc.GetVelocity(pointOnCurve);
        }

        /// <summary>
        /// Updates the movement state and returns the resultant vector based on calculations.
        /// </summary>
        /// <returns>The updated movement vector.</returns>
        public Vector3 Update()
        {
            Vector3 result;
            if (!hasReachedMaxVelocity)
                pointOnCurve += Time.deltaTime;
            Vector3 v = GetVelocityOnVector();

            if (hasReachedMaxVelocity)
            {
                result = v * Time.deltaTime;
            }
            else
            {
                Vector3 currentOffset = GetOffsetPosition();
                result = currentOffset - lastPositionOffset;
                lastPositionOffset = GetOffsetPosition();
            }
            debugVelocity = CalculateVelocity();
            debugIntegral = CalculateIntegral(pointOnCurve);

            if (float.IsNaN(result.x))
                return Vector3.zero;
            return result;
        }

        /// <summary>
        /// Sets the maximum velocity state when reached.
        /// </summary>
        /// <param name="val">The value of the velocity that was reached.</param>
        /// <returns>The reached maximum velocity.</returns>
        private float SetMaxVelocityReached(float val)
        {
            hasReachedMaxVelocity = true;
            active = false;
            maxVelocityReturn = val;
            return maxVelocityReturn;
        }
    }
}
