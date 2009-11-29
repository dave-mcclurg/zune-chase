// ZuneChase - A game designed for your zune.
// Copyright (c) 2009 David McClurg <dpm@efn.org>
// Under the MIT License, details: License.txt.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZuneChase.Gameplay
{
    public class AnimController : Controller
    {
        /// <summary>
        /// animation textures
        /// </summary>
        List<Texture2D> animTextures = new List<Texture2D>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="speed">animation speed</param>
        public AnimController(Controller speed)
            : base(0, speed, 0, 1, Controller.Mode.CYCLE)
        {
        }

        /// <summary>
        /// Load the textures
        /// </summary>
        public void LoadTextures(Game game, string name)
        {
            int num = 0;
            while (true)
            {
                string filename = string.Format(@"Textures\{0}\{0} ({1:00})", name, ++num);
                try
                {
                    Texture2D texture = game.Content.Load<Texture2D>(filename);
                    animTextures.Add(texture);
                }
                catch (Microsoft.Xna.Framework.Content.ContentLoadException)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// get current animation texture
        /// </summary>
        public Texture2D GetTexture()
        {
            int numFrames = animTextures.Count;
            int animFrame = (int)(this * numFrames);
            if (animFrame >= numFrames)
                animFrame = numFrames - 1;
            return animTextures[animFrame];
        }
    }
}