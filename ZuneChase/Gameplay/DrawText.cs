// ZuneChase - A game designed for your zune.
// Copyright (c) 2009 David McClurg <dpm@efn.org>
// Under the MIT License, details: License.txt.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZuneChase.Gameplay
{
    public class DrawText : GameplayComponent
    {
        class Text
        {
            SpriteFont font;
            string str;
            Vector2 pos;
            Color color;

            public Text(SpriteFont f, string s, Vector2 p, Color c)
            {
                font = f;
                str = s;
                pos = p;
                color = c;
            }

            public void Draw(SpriteBatch batch)
            {
                batch.DrawString(font, str, pos, color);
            }
        }

        List<Text> drawList = new List<Text>();
        SpriteFont defaultFont;

        public DrawText(GameplayScreen screen) : base(screen)
        {
        }

        /// <summary>
        /// load resources
        /// </summary>
        public override void Load()
        {
            defaultFont = Game.Content.Load<SpriteFont>(@"Fonts\Pescadero");
        }

        /// <summary>
        /// update time step
        /// </summary>
        /// <param name="dt">delta time</param>
        public override void Update(float dt)
        {
            //if (drawList.Count > 0)
            //{
            //    Text msg = drawList[0];
            //    msg.time -= dt;
            //    if (msg.time <= 0)
            //    {
            //        drawList.Remove(msg);
            //    }
            //}
        }

        public override void Draw()
        {
            SpriteBatch batch = Screen.ScreenManager.SpriteBatch;

            foreach (Text text in drawList)
            {
                text.Draw(batch);
            }
            drawList.Clear();

            //if (drawList.Count > 0)
            //{
            //    Text msg = drawList[0];
            //    msg.Draw(batch);
            //}
        }

        public void Add(string s, Vector2 p)
        {
            //GraphicsDevice gd = Screen.ScreenManager.GraphicsDevice;
            //Vector2 centerPos = new Vector2(gd.Viewport.Width / 2, gd.Viewport.Height / 2);
            //Vector2 p = centerPos - defaultFont.MeasureString(s) / 2;

            drawList.Add(new Text(defaultFont, s, p, Color.Yellow));
        }

        public void Add(SpriteFont f, string s, Vector2 p, Color c)
        {
            drawList.Add(new Text(f, s, new Vector2(p.X + 1, p.Y + 1), Color.Black));
            drawList.Add(new Text(f, s, p, c));
        }
    }
}
