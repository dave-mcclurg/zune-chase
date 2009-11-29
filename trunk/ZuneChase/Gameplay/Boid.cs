// ZuneChase - A game designed for your zune.
// Copyright (c) 2009 David McClurg <dpm@efn.org>
// Under the MIT License, details: License.txt.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZuneChase.Gameplay
{
    /// <summary>
    /// Simulated moving object controlled by steering forces
    /// based on http://en.wikipedia.org/wiki/Boids
    /// </summary>
    public class Boid : GameplayComponent
    {
        /// <summary>
        /// mass (defaults to unity so acceleration=force)
        /// </summary>
        public float Mass { get; set; }

        /// <summary>
        /// size of bounding sphere, for obstacle avoidance, etc.
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// current position in world space
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// forward-pointing unit basis vector
        /// </summary>
        public Vector3 Forward { get; set; }
        public Vector3 Right { get; set; }
        public Vector3 Up { get; set; }

        /// <summary>
        /// Speed along Forward direction.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Regenerate the orthonormal basis vectors given a new forward
        /// </summary>
        /// <param name="newUnitForward">unit length forward vector</param>
        public void RegenerateLocalSpace(Vector3 newUnitForward)
        {
            Forward = newUnitForward;

            Right = Vector3.Cross(Forward, Up);
            Right.Normalize();

            Up = Vector3.Cross(Right, Forward);
        }

        /// <summary>
        /// Because local space is velocity-aligned, velocity = Forward * Speed 
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                return Forward * Speed;
            }

            set
            {
                Speed = value.Length();
                if (Speed > float.Epsilon)
                {
                    RegenerateLocalSpace(value * (1 / Speed));
                }
            }
        }

        /// <summary>
        /// the maximum speed this vehicle is allowed to move
        // (velocity is clipped to this magnitude)
        /// </summary>
        public float MaxSpeed { get; set; }

        /// <summary>
        /// the maximum steering force this vehicle can apply
        /// (steering force is clipped to this magnitude)
        /// </summary>
        public float MaxForce { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Boid(GameplayScreen screen) : base(screen)
        {
            Mass = 1;
            Radius = 5;
            Position = new Vector3(0, 0, 0);
            Forward = new Vector3(0, 1, 0);
            Right = new Vector3(1, 0, 0);
            Up = new Vector3(0, 0, 1);
            Speed = 0;
            MaxSpeed = 1.0f;
            MaxForce = 0.1f;
        }

        /// <summary>
        /// clip the steering vector to be inside a cone
        /// </summary>
        /// <param name="steering">steering vector</param>
        /// <param name="cosineOfConeAngle">cosine of cone angle</param>
        /// <param name="forward">cone basis</param>
        /// <returns></returns>
        public Vector3 ClipSteering(Vector3 steering, float cosineOfConeAngle, Vector3 forward)
        {
            float steeringLength = steering.Length();
            if (steeringLength < MyMathHelper.Epsilon)
                return Vector3.Zero;

            // compute the angle between the forward vector and the steering vector
            float dotp = Vector3.Dot(steering, forward);
            float cosineOfSteeringAngle = dotp / steeringLength;
            if (cosineOfSteeringAngle >= cosineOfConeAngle)
                return steering; // steering vector is already inside the cone

            // find the portion of "steering" that is perpendicular to "forward"
            Vector3 perp = steering - forward * dotp;
            float perpLength = perp.Length();
            Vector3 unitPerp;
            if (perpLength < MyMathHelper.Epsilon)
            {
                //handle case where we want to go in exactly the opposite direction
                unitPerp = new Vector3(forward.Y, -forward.X, 0);
            }
            else
                unitPerp = perp * (1/perpLength);

            // construct a new vector whose length equals the steering vector,
            // and lies on the intersection of a steering-forward plane and cone
            Vector3 c0 = forward * cosineOfConeAngle;
            Vector3 c1 = unitPerp * (float)Math.Sqrt(1 - (cosineOfConeAngle * cosineOfConeAngle));
            return (c0 + c1) * steeringLength;
        }

        /// <summary>
        /// Apply steering force for time step
        /// </summary>
        /// <param name="steering">steering force</param>
        /// <param name="dt">delta time</param>
        public void ApplySteering(Vector3 steering, float dt)
        {
            Vector3 clippedForce = MyMathHelper.TruncateLength(steering, MaxForce);

            // compute acceleration and velocity
            Vector3 newAccel = clippedForce * (1 / Mass);
            Vector3 newVelocity = Velocity;

            // Euler integrate (per frame) acceleration into velocity
            newVelocity += newAccel * dt;
            newVelocity = MyMathHelper.TruncateLength(newVelocity, MaxSpeed);

            // Euler integrate (per frame) velocity into position
            Position = Position + (newVelocity * dt);
            Velocity = newVelocity;

            // no leaving the world
            Vector3 pos = Position;
            pos.X = MathHelper.Clamp(pos.X, Screen.world.Limits.Min.X + Radius, Screen.world.Limits.Max.X - Radius);
            pos.Y = MathHelper.Clamp(pos.Y, Screen.world.Limits.Min.Y + Radius, Screen.world.Limits.Max.Y - Radius);
            Position = pos;

#if false
            Vector2 spos = Screen.world.Project(Position);
            Vector3 dir = Forward * 20;
            Screen.drawPrims.AddLine(spos.X, spos.Y, spos.X + dir.X, spos.Y - dir.Y, Color.Red);
            dir = steering * (20 / MaxSpeed);
            Screen.drawPrims.AddLine(spos.X, spos.Y, spos.X + dir.X, spos.Y - dir.Y, Color.White);
#endif
        }

        /// <summary>
        /// Damp steering force when moving slowly
        /// </summary>
        /// <param name="steering"></param>
        /// <returns></returns>
        public Vector3 adjustRawSteeringForce(Vector3 steering)
        {
            float maxAdjustedSpeed = 0.2f * MaxSpeed;

            if ((Speed > maxAdjustedSpeed) || steering.LengthSquared() < float.Epsilon)
            {
                return steering;
            }
            else
            {
                float range = Speed / maxAdjustedSpeed;
                float cosine = MathHelper.Lerp(1.0f, -1.0f, (float)Math.Pow(range, 10));
                return ClipSteering(steering, cosine, Forward);
            }
        }

        /// <summary>
        /// wander controller
        /// </summary>
        public Controller WanderSide = new Controller(0, 1, -1, +1, Controller.Mode.WANDER);

        /// <summary>
        /// steer for wander
        /// </summary>
        /// <param name="dt">delta time</param>
        /// <returns>steering force unit vector</returns>
        public Vector3 steerForWander(float dt)
        {
            WanderSide.Update(dt);

            // return a pure lateral steering vector: (+/-Side)
            return (Right * WanderSide);
        }

        /// <summary>
        /// steer for seek to target
        /// </summary>
        /// <param name="target">seek target</param>
        /// <returns>steering force vector of length 0-maxspeed</returns>
        public Vector3 steerForSeek(Vector3 target)
        {
            Vector3 desiredVelocity = MyMathHelper.TruncateLength((target - Position), MaxSpeed);
            return desiredVelocity - Velocity;
        }

        public Vector3 predictFuturePosition(float predictionTime)
        {
            return Position + Velocity * predictionTime;
        }

        public Vector3 steerForFlee(Vector3 target)
        {
            Vector3 offset = Position - target;
            Vector3 desiredVelocity = MyMathHelper.TruncateLength(offset, MaxSpeed);
            return desiredVelocity - Velocity;
        }

        public Vector3 steerToEvade(Boid e)
        {
            // steering to flee from enemy's future position
            return steerForFlee(e.predictFuturePosition(1));
        }

        public Vector3 steerInsideWorldLimits()
        {
            Vector3 Radius = (Screen.world.Limits.Max - Screen.world.Limits.Min) / 2;
            Vector3 Center = (Screen.world.Limits.Max + Screen.world.Limits.Min) / 2;

            Vector3 relPos = Position - Center;
            relPos.X = Math.Abs(relPos.X / Radius.X);
            relPos.Y = Math.Abs(relPos.Y / Radius.Y);

            if (relPos.X < 0.7f && relPos.Y < 0.7f)
                return Vector3.Zero;

            float scale = Math.Max(relPos.X, relPos.Y);
            scale = (scale - 0.7f) / 0.3f;

            // steer back when outside
            return steerForSeek(Vector3.Zero) * (scale / MaxSpeed);
        }
    }

    /// <summary>
    /// The boid manager
    /// </summary>
    public class BoidManager : GameplayComponent
    {
        public List<Boid> List = new List<Boid>();

        public BoidManager(GameplayScreen screen)
            : base(screen)
        {
        }

        /// <summary>
        /// load resources
        /// </summary>
        public override void Load()
        {
            foreach (Boid b in List)
            {
                b.Load();
            }
        }

        /// <summary>
        /// update time step
        /// </summary>
        /// <param name="dt">delta time</param>
        public override void Update(float dt)
        {
            foreach (Boid b in List)
            {
                b.Update(dt);
            }
        }

        /// <summary>
        /// draw
        /// </summary>
        public override void Draw()
        {
            foreach (Boid b in List)
            {
                b.Draw();
            }
        }
    }
}
