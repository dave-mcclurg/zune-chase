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
    /// The firefly class
    /// </summary>
    public class Firefly : Boid
    {
        Controller alpha;
        Controller radiusOffset;
        Texture2D texture = null;

        public Firefly(GameplayScreen screen)
            : base(screen)
        {
            Radius = 4;

            Vector3 initPos = new Vector3();
            initPos.X = MathHelper.Lerp(screen.world.Limits.Min.X, screen.world.Limits.Max.X, screen.random.NextFloat());
            initPos.Y = MathHelper.Lerp(screen.world.Limits.Min.Y, screen.world.Limits.Max.Y, screen.random.NextFloat());
            initPos.Z = 0;

            Position = initPos;

            MaxForce = 5;
            MaxSpeed = MathHelper.Lerp(8, 12, screen.random.NextFloat());

            MaxForce *= 2; // 1.65f;
            MaxSpeed *= 2; // 1.65f;

            Speed = MathHelper.Lerp(MaxSpeed / 2, MaxSpeed, screen.random.NextFloat());

            float angle = (float)(screen.random.NextFloat() * MathHelper.TwoPi);
            Velocity = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0) * Speed;

            alpha = new Controller(screen.random.NextFloat(),
                MathHelper.Lerp(0.4f, 0.8f, screen.random.NextFloat()), 0, 1, Controller.Mode.OSCILLATE);
            radiusOffset = new Controller(
                screen.random.NextFloat() * 4 - 2, 50, -2, 2, Controller.Mode.OSCILLATE);
        }

        public override void Load()
        {
            texture = Game.Content.Load<Texture2D>(@"Textures\boid");
        }

        public override void Draw()
        {
            SpriteBatch batch = Screen.ScreenManager.SpriteBatch;
            Vector2 spos = Screen.world.Project(Position);

            float radius = Radius;

            // animate the radius to simulate flicker
            if (Screen.random.NextFloat() < 0.5f)
            {
                radius += radiusOffset/2;
                //radius = radius + MathHelper.Lerp(-1, 2, screen.random.NextFloat());
            }

            float x = spos.X - radius;
            float y = spos.Y - radius;
            float w = radius * 2;
            float h = radius * 2;

            // compute color
            //if (alpha > 0.2f)
            {
                Color color = new Color(255, 255, 0, 255 * alpha);
                batch.Draw(texture,
                    new Rectangle((int)x, (int)y, (int)w, (int)h),
                    new Rectangle(0, 0, texture.Width, texture.Height),
                    color);
            }

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

            //if (limit2d.LengthSquared() > 0)
            //{
                //Vector2 spos = Screen.world.Project(Position);
                //Vector3 dir = limit2d * 20;
                //Screen.drawPrims.AddLine(spos.X, spos.Y, spos.X + dir.X, spos.Y - dir.Y, Color.Yellow);
            //}

            //Vector2 spos = Screen.world.Project(Position);
            //Screen.drawText.Add(string.Format("{0:0.00}", (float)WanderSide),
            //    //string.Format("({0:0.00},{1:0.00}", wander2d.X, wander2d.Y),
            //    new Vector2(spos.X + 5, spos.Y + 5));

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
        }
    }
}