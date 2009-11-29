// ZuneChase - A game designed for your zune.
// Copyright (c) 2009 David McClurg <dpm@efn.org>
// Under the MIT License, details: License.txt.

using System;
using Microsoft.Xna.Framework;

namespace ZuneChase.Gameplay
{
    /// <summary>
    /// A random number generator with floats
    /// </summary>
    public class MyRandom : Random
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MyRandom(int seed)
            : base(seed)
        {
        }

        /// <summary>
        /// Return a random number between 0.0 and 1.0
        /// </summary>
        public float NextFloat()
        {
            return (float)NextDouble();
        }
    }

    public static class MyMathHelper
    {
        public static float Epsilon = 1E-5f;
        
        /// <summary>
        /// Normalize but skip divide if length is zero
        /// </summary>
        public static Vector3 SafeNormalize(Vector3 v)
        {
            float len = v.Length();
            if (len < Epsilon)
                return v;
            return v * (1/len);
        }

        /// <summary>
        /// Truncate vector if length is greater than max length.
        /// </summary>
        public static Vector3 TruncateLength(Vector3 v, float maxLength)
        {
            if (v.LengthSquared() <= maxLength * maxLength)
                return v;
            v.Normalize();
            return v * maxLength;
        }

        /// <summary>
        /// return component of vector parallel to a unit basis vector
        /// </summary>
        /// <param name="unitBasis">unit vector (length==1)</param>
        public static Vector3 parallelComponent(Vector3 v, Vector3 unitBasis)
        {
            float projection = Vector3.Dot(v, unitBasis);
            return unitBasis * projection;
        }

        /// <summary>
        /// return component of vector perpendicular to a unit basis vector
        /// </summary>
        /// <param name="unitBasis">unit vector (length==1)</param>
        public static Vector3 perpendicularComponent(Vector3 v, Vector3 unitBasis)
        {
            return v - parallelComponent(v, unitBasis);
        }
    }
}
