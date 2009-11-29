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
    /// The butterfly class
    /// </summary>
    public class Butterfly : Boid
    {
        Controller alpha;
        Controller radiusOffset;
        AnimController anim;

        public Butterfly(GameplayScreen screen)
            : base(screen)
        {
            Radius = 4;

            Vector3 initPos = new Vector3();
            initPos.X = MathHelper.Lerp(screen.world.Limits.Min.X, screen.world.Limits.Max.X, screen.random.NextFloat());
            initPos.Y = MathHelper.Lerp(screen.world.Limits.Min.Y, screen.world.Limits.Max.Y, screen.random.NextFloat());
            initPos.Z = 0;

            Position = initPos;

            MaxForce = 5 * 1.65f;
            MaxSpeed = MathHelper.Lerp(8, 12, screen.random.NextFloat()) * 1.65f;

            MaxSpeed = 75;
            MaxForce = 3 * MaxSpeed;

            Speed = MathHelper.Lerp(MaxSpeed / 2, MaxSpeed, screen.random.NextFloat());

            float angle = (float)(screen.random.NextFloat() * MathHelper.TwoPi);
            Velocity = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0) * Speed;

            alpha = new Controller(screen.random.NextFloat(),
                MathHelper.Lerp(0.4f, 0.8f, screen.random.NextFloat()), 0, 1, Controller.Mode.OSCILLATE);
            radiusOffset = new Controller(
                screen.random.NextFloat() * 4 - 2, 50, -2, 2, Controller.Mode.OSCILLATE);

            Controller animSpeed = new Controller(2, 1, 3, 7, Controller.Mode.OSCILLATE);
            anim = new AnimController(animSpeed);
        }

        public override void Load()
        {
            anim.LoadTextures(Game, "butterfly");
        }

        public override void Draw()
        {
            SpriteBatch batch = Screen.ScreenManager.SpriteBatch;
            Vector2 spos = Screen.world.Project(Position);

            // get the texture
            Texture2D texture = anim.GetTexture();

            // compute the sprite angle
            float angle = (float)(Math.PI / 2 - Math.Atan(Forward.Y / Forward.X));
            if (Forward.X < 0)
                angle -= (float)Math.PI;

            float scale = 0.1f;

            Vector2 origin = new Vector2(texture.Width/2, texture.Height/2);   // center of head

            batch.Draw(texture, spos, new Rectangle(0, 0, texture.Width, texture.Height),
                Color.White, angle, origin, scale, SpriteEffects.None, 0);

            //if (Simulation.Instance.showPos)
            //{
            //    float off = Radius;
            //    g.DrawString(string.Format("({0:0.0},{1:0.0})", spos.X, spos.Y),
            //        font, fontBrush, spos.X + off, spos.Y + off, new StringFormat());
            //}
        }

        /// <summary>
        /// update time step
        /// </summary>
        /// <param name="dt">delta time</param>
        public override void Update(float dt)
        {
            Vector3 wander2d = steerForWander(dt);
            Vector3 limit2d = steerInsideWorldLimits();
            Vector3 steer = Forward + wander2d + limit2d * 3;

            steer = steer * MaxSpeed;

            //Vector3 eOffset = Position - screen.player.Position;
            //float eDistance = eOffset.Length();
            //float radSum = 75; // Radius + screen.player.Radius;
            //if (eDistance <= radSum)
            //{
            //    steer = steerToEvade(screen.player)*100;
            //}

            //Vector3 steer = new Vector3(1, 0, 0) * 100;
            steer = adjustRawSteeringForce(steer);

            // Smoothed steering used to damp out rapid steering changes
            // damp out abrupt changes and oscillations in steering acceleration
            // (rate is proportional to time step, then clipped into useful range)
            //float smoothRate = MathHelper.Clamp(9 * dt, 0.15f, 0.4f);
            //smoothSteer = Vector3.Lerp(smoothSteer, steer, smoothRate);
            //ApplySteering(adjustRawSteeringForce(smoothSteer), dt);

            ApplySteering(adjustRawSteeringForce(steer), dt);

            alpha.Update(dt);
            radiusOffset.Update(dt);
            anim.Update(dt);
        }
    }
}