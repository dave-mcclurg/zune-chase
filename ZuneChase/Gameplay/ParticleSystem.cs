//-----------------------------------------------------------------------------
// ParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ZuneChase.Gameplay
{
    /// <summary>
    /// A relatively simple particle system.  We recycle particles instead of creating
    /// and destroying them as we need more.  "Effects" are created via factory methods
    /// on ParticleSystem, rather than a data driven model due to the relatively low
    /// number of effects.
    /// </summary>
    public class ParticleSystem : GameplayComponent
    {
        Random          random;

        //Texture2D       tank_tire;
        //Texture2D       tank_top;
        Texture2D       fire;
        Texture2D       smoke;

        List<Particle>  particles;

        public ParticleSystem(GameplayScreen screen) : base(screen)
        {
        }

        /// <summary>
        /// load resources
        /// </summary>
        public override void Load()
        {
            random = new Random();
            particles = new List<Particle>();

            //tank_tire = Game.Content.Load<Texture2D>("tank_tire");
            //tank_top = Game.Content.Load<Texture2D>("tank_top");
            fire = Game.Content.Load<Texture2D>(@"Textures\fire");
            smoke = Game.Content.Load<Texture2D>(@"Textures\smoke");
        }

        /// <summary>
        /// Update all active particles.
        /// </summary>
        /// <param name="elapsed">The amount of time elapsed since last Update.</param>
        public override void Update(float elapsed)
        {
            for (int i = 0; i < particles.Count; ++i)
            {
                particles[i].Life -= elapsed;
                if (particles[i].Life <= 0.0f)
                {
                    continue;
                }
                particles[i].Position += particles[i].Velocity * elapsed;
                particles[i].Rotation += particles[i].RotationRate * elapsed;
                particles[i].Alpha += particles[i].AlphaRate * elapsed;
                particles[i].Scale += particles[i].ScaleRate * elapsed;

                if (particles[i].Alpha <= 0.0f)
                    particles[i].Alpha = 0.0f;                                    
            }
        }

        /// <summary>
        /// Draws the particles.
        /// </summary>
        public override void Draw()
        {
            SpriteBatch spriteBatch = Screen.ScreenManager.SpriteBatch;
            for (int i = 0; i < particles.Count; ++i)
            {
                Particle p = particles[i];
                if (p.Life <= 0.0f)
                    continue;

                float alphaF = 255.0f * p.Alpha;
                if (alphaF < 0.0f)
                    alphaF = 0.0f;
                if (alphaF > 255.0f)
                    alphaF = 255.0f;

                spriteBatch.Draw(p.Texture, p.Position, null, new Color(p.Color.R, p.Color.G, p.Color.B, (byte)alphaF), p.Rotation, new Vector2(p.Texture.Width / 2, p.Texture.Height / 2), p.Scale, SpriteEffects.None, 0.0f);
            }
        }

        /// <summary>
        /// Creats a particle, preferring to reuse a dead one in the particles list 
        /// before creating a new one.
        /// </summary>
        /// <returns></returns>
        Particle CreateParticle()
        {
            Particle p = null;

            for (int i = 0; i < particles.Count; ++i)
            {
                if (particles[i].Life <= 0.0f)
                {
                    p = particles[i];
                    break;
                }
            }

            if (p == null)
            {
                p = new Particle();
                particles.Add(p);
            }

            p.Color = Color.White;

            return p;
        }

        /// <summary>
        /// Creats the effect for when an alien dies.
        /// </summary>
        /// <param name="position">Where on the screen to create the effect.</param>
        public void CreateAlienExplosion(Vector2 position)
        {
            Particle p = null;

            for (int i = 0; i < 8; ++i)
            {
                p = CreateParticle();
                p.Position = position;
                p.RotationRate = -6.0f + 12.0f * (float)random.NextDouble();
                p.Scale = 0.5f;
                p.ScaleRate = 0.25f;// *(float)random.NextDouble();
                p.Alpha = 2.0f;
                p.AlphaRate = -1.0f;
                p.Velocity.X = -32.0f + 64.0f * (float)random.NextDouble();
                p.Velocity.Y = -32.0f + 64.0f * (float)random.NextDouble();
                p.Texture = smoke;
                p.Life = 2.0f;
            }

            //for (int i = 0; i < 3; ++i)
            //{
            //    p = CreateParticle();
            //    p.Position = position;
            //    p.Position.X += -8.0f + 16.0f * (float)random.NextDouble();
            //    p.Position.Y += -8.0f + 16.0f * (float)random.NextDouble();
            //    p.RotationRate = -2.0f + 4.0f * (float)random.NextDouble();
            //    p.Scale = 0.25f;
            //    p.ScaleRate = 1.0f;// *(float)random.NextDouble();
            //    p.Alpha = 2.0f;
            //    p.AlphaRate = -1.0f;
            //    p.Velocity = Vector2.Zero;
            //    p.Texture = fire;
            //    p.Life = 2.0f;
            //}
        }

        /// <summary>
        /// Creats the effect for when the player fires a bullet.
        /// </summary>
        /// <param name="position">Where on the screen to create the effect.</param>        
        public void CreatePlayerFireSmoke(Vector2 position)
        {
            for (int i = 0; i < 8; ++i)
            {
                Particle p = CreateParticle();
                p.Texture = smoke;
                p.Color = Color.White;
                p.Position.X = position.X;
                p.Position.Y = position.Y;
                p.Alpha = 1.0f;
                p.AlphaRate = -1.0f;
                p.Life = 1.0f;
                p.Rotation = 0.0f;
                p.RotationRate = -2.0f + 4.0f * (float)random.NextDouble();
                p.Scale = 0.25f;
                p.ScaleRate = 0.25f;
                p.Velocity.X = -4 + 8.0f * (float)random.NextDouble();
                p.Velocity.Y = -16.0f + -32.0f * (float)random.NextDouble();
            }
        }
    }

    /// <summary>
    /// A basic particle.  Since this is strictly a data class, I decided to not go
    /// the full property route and used public fields instead.
    /// </summary>
    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Texture2D Texture;
        public float RotationRate;
        public float Rotation;
        public float Life;
        public float AlphaRate;
        public float Alpha;
        public float ScaleRate;
        public float Scale;
        public Color Color = Color.White;
    }    
}