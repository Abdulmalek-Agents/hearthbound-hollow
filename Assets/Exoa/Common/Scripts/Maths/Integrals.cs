
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Maths
{
    /// <summary>
    /// Abstract class representing the base for velocity integral calculations.
    /// </summary>
    public abstract class BaseVelocityIntegral
    {
        protected float a, b, c, d, logbase;

        /// <summary>
        /// Initializes the parameters for the velocity integral.
        /// </summary>
        /// <param name="a">Coefficient a.</param>
        /// <param name="b">Coefficient b.</param>
        /// <param name="c">Coefficient c.</param>
        /// <param name="d">Coefficient d.</param>
        /// <param name="logbase">Base for logarithmic calculations.</param>
        virtual public void Init(float a = 1, float b = 1, float c = 0, float d = 1, float logbase = 10)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.logbase = logbase;
        }

        /// <summary>
        /// Calculates the integral of the velocity function at a given point.
        /// </summary>
        /// <param name="x">The point at which to calculate the integral.</param>
        /// <returns>The value of the integral at the point x.</returns>
        abstract public float GetIntegral(float x);

        /// <summary>
        /// Calculates the velocity at a given point on the curve.
        /// </summary>
        /// <param name="pointOnCurve">The point on the curve for which to calculate the velocity.</param>
        /// <returns>The velocity at the specified point on the curve.</returns>
        abstract public float GetVelocity(float pointOnCurve);
    }

    /// <summary>
    /// Represents a velocity integral with the form Bx + C.
    /// </summary>
    public class Bx_C : BaseVelocityIntegral
    {
        /// <summary>
        /// Calculates the integral of the velocity function at a given point.
        /// </summary>
        /// <param name="x">The point at which to calculate the integral.</param>
        /// <returns>The value of the integral at the point x.</returns>
        public override float GetIntegral(float x)
        {
            float x2 = x * x;
            return ((b * x2) / 2.0f) + c * x;
        }

        /// <summary>
        /// Calculates the velocity at a given point on the curve.
        /// </summary>
        /// <param name="pointOnCurve">The point on the curve for which to calculate the velocity.</param>
        /// <returns>The velocity at the specified point on the curve.</returns>
        override public float GetVelocity(float pointOnCurve)
        {
            return b * pointOnCurve + c;
        }
    }

    /// <summary>
    /// Represents a velocity integral with the form Ax^3 + Bx^2 + Dx + C.
    /// </summary>
    public class Ax3_Bx2_Dx_C : BaseVelocityIntegral
    {
        /// <summary>
        /// Calculates the integral of the velocity function at a given point.
        /// </summary>
        /// <param name="x">The point at which to calculate the integral.</param>
        /// <returns>The value of the integral at the point x.</returns>
        override public float GetIntegral(float x)
        {
            float x2 = x * x;
            float x3 = x2 * x;
            float x4 = x2 * x2;
            return ((a * x4) / 4.0f) + ((b * x3) / 3.0f) + ((d * x2) / 2.0f) + c * x;
        }

        /// <summary>
        /// Calculates the velocity at a given point on the curve.
        /// </summary>
        /// <param name="pointOnCurve">The point on the curve for which to calculate the velocity.</param>
        /// <returns>The velocity at the specified point on the curve.</returns>
        override public float GetVelocity(float pointOnCurve)
        {
            float x2 = pointOnCurve * pointOnCurve;
            float x3 = x2 * pointOnCurve;
            return a * x3 + b * x2 + d * pointOnCurve + c;
        }
    }

    /// <summary>
    /// Represents a velocity integral with the form Ax^2 + Bx + C.
    /// </summary>
    public class Ax2_Bx_C : BaseVelocityIntegral
    {
        /// <summary>
        /// Calculates the integral of the velocity function at a given point.
        /// </summary>
        /// <param name="x">The point at which to calculate the integral.</param>
        /// <returns>The value of the integral at the point x.</returns>
        override public float GetIntegral(float x)
        {
            // Figure out the area under the curve
            float x2 = x * x;
            float x3 = x2 * x;
            return ((a * x3) / 3.0f) + ((b * x2) / 2.0f) + c * x;
        }

        /// <summary>
        /// Calculates the velocity at a given point on the curve.
        /// </summary>
        /// <param name="pointOnCurve">The point on the curve for which to calculate the velocity.</param>
        /// <returns>The velocity at the specified point on the curve.</returns>
        override public float GetVelocity(float pointOnCurve)
        {
            float x2 = pointOnCurve * pointOnCurve;
            return a * x2 + b * pointOnCurve + c;
        }
    }

    /// <summary>
    /// Represents a velocity integral with the form Ax^2 + C.
    /// </summary>
    public class Ax2_C : BaseVelocityIntegral
    {
        /// <summary>
        /// Calculates the integral of the velocity function at a given point.
        /// </summary>
        /// <param name="x">The point at which to calculate the integral.</param>
        /// <returns>The value of the integral at the point x.</returns>
        override public float GetIntegral(float x)
        {
            // Figure out the area under the curve
            return ((a * x * x * x) / 3.0f) + c * x;
        }

        /// <summary>
        /// Calculates the velocity at a given point on the curve.
        /// </summary>
        /// <param name="pointOnCurve">The point on the curve for which to calculate the velocity.</param>
        /// <returns>The velocity at the specified point on the curve.</returns>
        override public float GetVelocity(float pointOnCurve)
        {
            return a * pointOnCurve * pointOnCurve + c;
        }
    }

    /// <summary>
    /// Represents a velocity integral with a logarithmic component: A log(x + B) + C.
    /// </summary>
    public class AlogXplusB_C : BaseVelocityIntegral
    {
        /// <summary>
        /// Calculates the integral of the velocity function at a given point.
        /// </summary>
        /// <param name="x">The point at which to calculate the integral.</param>
        /// <returns>The value of the integral at the point x.</returns>
        override public float GetIntegral(float x)
        {
            return (a * (b + x) * UnityEngine.Mathf.Log(b + x) - a * x + c * x * UnityEngine.Mathf.Log(logbase)) / UnityEngine.Mathf.Log(logbase);
        }

        /// <summary>
        /// Calculates the velocity at a given point on the curve.
        /// </summary>
        /// <param name="pointOnCurve">The point on the curve for which to calculate the velocity.</param>
        /// <returns>The velocity at the specified point on the curve.</returns>
        override public float GetVelocity(float pointOnCurve)
        {
            return (a * UnityEngine.Mathf.Log(pointOnCurve + b)) / UnityEngine.Mathf.Log(logbase) + c;
        }
    }
}
