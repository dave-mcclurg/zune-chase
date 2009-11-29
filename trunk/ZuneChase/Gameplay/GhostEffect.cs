// ZuneChase - A game designed for your zune.
// Copyright (c) 2009 David McClurg <dpm@efn.org>
// Under the MIT License, details: License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace ZuneChase.Gameplay
{
    class GhostEffect : GameplayComponent
    {
        class Ghost
        {
            public Vector3 pos;
            public float alpha;
            public float scale;

            public Ghost(Vector3 pos)
            {
                this.pos = pos;
                this.alpha = 0.3f;
                this.scale = 0.6f;
            }
        }

        Boid boid;

        public GhostEffect(GameplayScreen screen, Boid boid)
            : base(screen)
        {
            this.boid = boid;
        }

        /// <summary>
        /// Ghost trail
        /// </summary>
        Queue<Ghost> ghostQ = new Queue<Ghost>();
        Texture2D ghostTexture;
        int ghostCounter = 0;

        /// <summary>
        /// load resources
        /// </summary>
        public override void Load()
        {
            ghostTexture = Game.Content.Load<Texture2D>(@"Textures\boid");
        }

        /// <summary>
        /// update time step
        /// </summary>
        /// <param name="dt">delta time</param>
        public override void Update(float dt)
        {
            // age the ghosts
            foreach (Ghost ghost in ghostQ)
            {
                ghost.scale = Math.Max(ghost.scale - dt * 0.2f, 0);
                ghost.alpha = Math.Max(ghost.alpha - dt * 0.1f, 0);
            }

            // remove the ones you cannot see any longer
            while (ghostQ.Count > 0)
            {
                Ghost ghead = ghostQ.Peek();
                if (ghead.scale > 0 && ghead.alpha > 0)
                    break;
                ghostQ.Dequeue();
            }
        }

        public override void Draw()
        {
            SpriteBatch batch = Screen.ScreenManager.SpriteBatch;

            // draw the ghosts
            foreach (Ghost ghost in ghostQ)
            {
                Color ghostColor = new Color(new Vector4(1, 1, 1, ghost.alpha));
                Vector2 ghostPos = Screen.world.Project(ghost.pos);
                batch.Draw(ghostTexture, ghostPos, new Rectangle(0, 0, ghostTexture.Width, ghostTexture.Height),
                    ghostColor, 0, new Vector2(ghostTexture.Width / 2, ghostTexture.Height / 2), ghost.scale, SpriteEffects.None, 0);
            }

            // add a ghost
            if (++ghostCounter >= 4)
            {
                ghostCounter = 0;
                Ghost ghost = new Ghost(boid.Position);
                float fScale = MathHelper.Lerp(0, 1, boid.Speed / boid.MaxSpeed);
                ghost.alpha *= fScale;
                ghost.scale *= fScale;
                ghostQ.Enqueue(ghost);
            }
        }
    }
}