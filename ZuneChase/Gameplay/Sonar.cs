// ZuneChase - A game designed for your zune.
// Copyright (c) 2009 David McClurg <dpm@efn.org>
// Under the MIT License, details: License.txt.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace ZuneChase.Gameplay
{
    class Sonar : GameplayComponent
    {
        /// <summary>
        /// Represents a sonar ping
        /// </summary>
        public class Ping
        {
            public Vector3 Position;
            public Vector3 Velocity;
            public bool IsAlive;
        }

        List<Ping> pings;
        Texture2D texture;
        SoundEffect fired;
        float fireTimer = 0;

        public Sonar(GameplayScreen screen)
            : base(screen)
        {
            pings = new List<Ping>();
        }

        public override void Load()
        {
            texture = Game.Content.Load<Texture2D>(@"Textures\boid");
            fired = Game.Content.Load<SoundEffect>(@"Sounds\sonar");
        }

        public override void Update(float dt)
        {
            BoundingBox limits = Screen.world.Limits;
            float radius = 1;

            fireTimer = MathHelper.Clamp(fireTimer - dt, 0, 1);

            foreach (Ping p in pings)
            {
                if (p.IsAlive)
                {
                    p.Position += p.Velocity * dt;

                    if (p.Position.X < limits.Min.X + radius ||
                        p.Position.X > limits.Max.X - radius ||
                        p.Position.Y < limits.Min.Y + radius ||
                        p.Position.Y > limits.Max.Y - radius)
                    {
                        p.IsAlive = false;
                    }
                }
            }
        }

        public override void Draw()
        {
            SpriteBatch batch = Screen.ScreenManager.SpriteBatch;
            foreach (Ping p in pings)
            {
                if (p.IsAlive)
                {
                    batch.Draw(texture, Screen.world.Project(p.Position), Color.White);
                }
            }
        }

        public void CheckHit()
        {
            //w.IsAlive = false;
            //particles.CreatePlayerExplosion(new Vector2(player.Position.X + player.Width / 2, player.Position.Y + player.Height / 2));
        }

        /// <summary>
        /// Returns an instance of a usable wave.  Prefers reusing an existing (dead)
        /// wave over creating a new instance.
        /// </summary>
        /// <returns>A wave ready to place into the world.</returns>
        Ping CreateWave()
        {
            foreach (Ping p in pings)
            {
                if (!p.IsAlive)
                {
                    p.IsAlive = true;
                    return p;
                }
            }

            Ping newWave = new Ping();
            pings.Add(newWave);

            newWave.IsAlive = true;
            return newWave;
        }

        public void Fire(Vector3 pos, Vector3 dir)
        {
            if (fireTimer <= 0)
            {
                float speed = 100;
                Ping p = CreateWave();
                p.Position = pos;
                p.Velocity = dir * speed;
                fired.Play();
                fireTimer = 2;
            }
        }
    }
}