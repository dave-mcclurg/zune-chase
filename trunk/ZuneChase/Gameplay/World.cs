// ZuneChase - A game designed for your zune.
// Copyright (c) 2009 David McClurg <dpm@efn.org>
// Under the MIT License, details: License.txt.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ZuneChase.Gameplay
{
    public class World : GameplayComponent
    {
        Vector2 screenSize;
        Vector2 screenOrigin;
        Texture2D texture;
        Vector2 textureSize;
        BoundingBox limits; // limits of world coordinates
        Vector2 lookPos;

        public Vector2 ScreenSize { get { return screenSize; } }
        public BoundingBox Limits { get { return limits; } }

        public World(GameplayScreen screen) : base(screen)
        {
        }

        /// <summary>
        /// Project a world position onto the screen
        /// </summary>
        /// <param name="pos">world position</param>
        /// <returns>screen position</returns>
        public Vector2 Project(Vector3 pos)
        {
            Vector2 texturePos = new Vector2(pos.X + textureSize.X * 0.5f, -pos.Y + textureSize.Y * 0.5f);
            Vector2 screenPos = texturePos - screenOrigin;

            // clip it to the screen
            screenPos.X = MathHelper.Clamp(screenPos.X, 0, screenSize.X);
            screenPos.Y = MathHelper.Clamp(screenPos.Y, 0, screenSize.Y);

            return screenPos;
        }

        /// <summary>
        /// load resources
        /// </summary>
        public override void Load()
        {
            screenSize = new Vector2(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
            screenOrigin = new Vector2(0, 0);

            texture = Game.Content.Load<Texture2D>(@"Textures\night_sky\night_sky (01)");
            textureSize = new Vector2(texture.Width, texture.Height);

            limits = new BoundingBox(new Vector3(-textureSize * 0.5f, 0), new Vector3(textureSize * 0.5f, 0));
            lookPos = new Vector2(0, 0);
        }

        public void LookAt(float x, float y)
        {
            lookPos = new Vector2(x, y);
        }

        /// <summary>
        /// update time step
        /// </summary>
        /// <param name="dt">delta time</param>
        public override void Update(float dt)
        {
            Vector2 texturePos = new Vector2(lookPos.X + textureSize.X * 0.5f, -lookPos.Y + textureSize.Y * 0.5f);

            screenOrigin.X = texturePos.X - (screenSize.X * 0.5f);
            screenOrigin.Y = texturePos.Y - (screenSize.Y * 0.5f);

            screenOrigin.X = MathHelper.Clamp(screenOrigin.X, 0, textureSize.X - screenSize.X);
            screenOrigin.Y = MathHelper.Clamp(screenOrigin.Y, 0, textureSize.Y - screenSize.Y);
        }

        public override void Draw()
        {
            SpriteBatch batch = Screen.ScreenManager.SpriteBatch;
            batch.Draw(texture, new Vector2(0, 0), null,
                Color.White, 0, screenOrigin, 1, SpriteEffects.None, 0f);
        }
    }
}
