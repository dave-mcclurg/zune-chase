// ZuneChase - A game designed for your zune.
// Copyright (c) 2009 David McClurg <dpm@efn.org>
// Under the MIT License, details: License.txt.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace ZuneChase.Gameplay
{
    /// <summary>
    /// Supposedly useful controller for gameplay, animation, and effects.
    /// </summary>
    /// <example>
    /// Controller WanderSide = new Controller(0, 1, -1, +1, Controller.Mode.WANDER);
    /// Vector3 steering = (Right * WanderSide);
    /// </example>
    public class Controller
    {
        /// <summary>
        /// Controller mode
        /// </summary>
        public enum Mode
        {
            STOPPED,        /// Stop at current position
            ONCE,           /// Stop at start or end depending on the direction (speed).
            CYCLE,          /// When at end, jump back to start or visa versa.
            OSCILLATE,      /// Go from start to end and back to start.
            RING,           /// Oscillate once.
            WANDER,         /// Wander randomly back and forth
        }

        /// <summary>
        /// Output curve
        /// </summary>
        public enum Curve
        {
            LINEAR,     // linear
            SQUARED,    // slow down near the end
            CUBIC,      // ease in/out
        }

        Curve curve = Curve.LINEAR;
        Mode mode;
        int ringCounter;

        float pos;
        Controller speed;
        float min;
        float max;

        /// implicit conversion operator for Controller -> float
        public static implicit operator float(Controller p)
        {
            switch (p.curve)
            {
                case Curve.SQUARED:
                    return (float)Math.Pow(p.pos, 2);
                case Curve.CUBIC:
                    return (float)MathHelper.SmoothStep(0, 1, p.pos);
            }
            return p.pos;
        }

        /// implicit conversion operator for float -> Controller
        public static implicit operator Controller(float x)
        {
            return new Controller(x);
        }

        /// <summary>
        /// Constructor for constant position
        /// </summary>
        /// <param name="initial">constant position</param>
        public Controller(float initial)
        {
            this.pos = initial;
            this.speed = null;
            this.min = initial;
            this.max = initial;

            SetMode(Mode.STOPPED);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initial">initial position</param>
        /// <param name="speed">time multiplier</param>
        /// <param name="min">minimum position</param>
        /// <param name="max">maxiumum position</param>
        /// <param name="mode">initial mode</param>
        public Controller(float initial, Controller speed, float min, float max, Mode mode)
        {
            Debug.Assert(min <= max);

            this.pos = MathHelper.Clamp(initial, min, max);
            this.speed = speed;
            this.min = min;
            this.max = max;

            SetMode(mode);
        }

        /// <summary>
        /// Set the mode for the controller
        /// </summary>
        public void SetMode(Mode mode)
        {
            this.mode = mode;

            if (null != speed && speed < 0)
                this.ringCounter = -1;
            else
                this.ringCounter = 0;
        }

        /// <summary>
        /// Set the curve for the controller
        /// </summary>
        public void SetCurve(Curve curve)
        {
            switch (curve)
            {
                case Curve.SQUARED:
                case Curve.CUBIC:
                    // doesn't really make sense otherwise
                    Debug.Assert(min == 0 && max == 1);
                    break;
            }
            this.curve = curve;
        }

        /// <summary>
        /// update time step
        /// </summary>
        /// <param name="dt">delta time</param>
        public void Update(float dt)
        {
            if (null != speed)
            {
                speed.Update(dt);
            }
            switch (mode)
            {
                case Mode.ONCE:  // Stop at start or end depending on the direction (speed).
                    {
                        pos = pos + speed * dt;
                        if (pos < min)
                        {
                            pos = min;
                            mode = Mode.STOPPED;
                        }
                        else if (pos > max)
                        {
                            pos = max;
                            mode = Mode.STOPPED;
                        }
                    }
                    break;

                case Mode.CYCLE: // When at end, jump back to start or visa versa.
                    {
                        pos = pos + speed * dt;
                        if (pos < min)
                        {
                            pos = pos + (max - min);    //wrap
                        }
                        else if (pos > max)
                        {
                            pos = pos + (min - max);    //wrap
                        }
                    }
                    break;
                
                case Mode.OSCILLATE: // Go from start to end back to start.
                    {
                        pos = pos + speed * dt;
                        if (pos < min)
                        {
                            pos = min;
                            speed = -speed;
                        }
                        else if (pos > max)
                        {
                            pos = max;
                            speed = -speed;
                        }
                    }
                    break;
                
                case Mode.RING:   // Oscillate once.
                    {
                        pos = pos + speed * dt;
                        if (pos < min)
                        {
                            pos = min;
                            speed = -speed;
                            ringCounter++;
                        }
                        else if (pos > max)
                        {
                            pos = max;
                            speed = -speed;
                            ringCounter++;
                        }
                        if (ringCounter >= 2)
                        {
                            mode = Mode.STOPPED;
                        }
                    }
                    break;
                
                case Mode.WANDER: // Wander randomly back and forth
                    {
                        // create a random number generator if needed
                        float rand01 = (float)GameplayScreen.Instance.random.NextDouble(); // 0 to 1
                        pos = pos + (((rand01 * 2) - 1) * dt);
                        pos = MathHelper.Clamp(pos, min, max);
                    }
                    break;
            }
        }
    }
}