// ZuneChase - A game designed for your zune.
// Copyright (c) 2009 David McClurg <dpm@efn.org>
// Under the MIT License, details: License.txt.

//#define DEBUG_CAPTURE

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace ZuneChase.Gameplay
{
    /// <summary>
    /// Player is just a boid with input steering and animation
    /// </summary>
    public class Player : Boid
    {
        AnimController anim;

        /// <summary>
        /// Score is number of captures
        /// </summary>
        public int Score = 0;

        /// <summary>
        /// effects
        /// </summary>
        Controller captureGlow = new Controller(0, 1, 0, 1, Controller.Mode.STOPPED);
        SoundEffect captureSound;
        GhostEffect ghostEffect;
        Sonar sonar;
        SoundList flapSounds = new SoundList();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="screen">gameplay screen</param>
        public Player(GameplayScreen screen) : base(screen)
        {
            Radius = 8;
            Position = new Vector3(0,0,0);
            MaxSpeed = 75;
            MaxForce = 3 * MaxSpeed;
            Speed = 0;

            Controller animSpeed = new Controller(2, 1, 2, 3, Controller.Mode.OSCILLATE);
            anim = new AnimController(animSpeed);

            ghostEffect = new GhostEffect(screen, this);
            sonar = new Sonar(screen);
        }

        /// <summary>
        /// load resources
        /// </summary>
        public override void Load()
        {
            captureSound = Game.Content.Load<SoundEffect>(@"Sounds\powerup");
            ghostEffect.Load();
            sonar.Load();
            anim.LoadTextures(Game, "bat_animated");
            flapSounds.Load(Game, "flap");
        }

        Vector2 lastTouchPos = new Vector2(0,0);

        /// <summary>
        /// update time step
        /// </summary>
        /// <param name="dt">delta time</param>
        public override void Update(float dt)
        {
            Vector2 touchPos = Screen.zunePad.GetState().TouchPosition;
#if !ZUNE
            if (touchPos.LengthSquared() < 0.00001f)
            {
                KeyboardState ks = Keyboard.GetState();
                if (ks.IsKeyDown(Keys.Left))
                    touchPos.X = -1;
                if (ks.IsKeyDown(Keys.Right))
                    touchPos.X = 1;
                if (ks.IsKeyDown(Keys.Up))
                    touchPos.Y = 1;
                if (ks.IsKeyDown(Keys.Down))
                    touchPos.Y = -1;
                if (ks.IsKeyDown(Keys.Home))
                {
                    touchPos.X = -1;
                    touchPos.Y = 1;
                }
                if (ks.IsKeyDown(Keys.End))
                {
                    touchPos.X = -1;
                    touchPos.Y = -1;
                }
                if (ks.IsKeyDown(Keys.PageDown))
                {
                    touchPos.X = 1;
                    touchPos.Y = -1;
                }
                if (ks.IsKeyDown(Keys.PageUp))
                {
                    touchPos.X = 1;
                    touchPos.Y = 1;
                }

                //MouseState ms = Mouse.GetState();
                //Vector2 ss = screen.world.ScreenSize;
                //Vector2 pos = new Vector2(ms.X / ss.X, ms.Y / ss.Y);
                //pos = pos * 2 - new Vector2(1, 1);
                //touchPos.X = MathHelper.Clamp(pos.X, -1, 1);
                //touchPos.Y = MathHelper.Clamp(-pos.Y, -1, 1);
            }
#endif

            lastTouchPos = touchPos;

            if (Screen.zunePad.GetState().PadPressed == ButtonState.Pressed)
            {
                sonar.Fire(Position, Forward);
            }

            ghostEffect.Update(dt);
            sonar.Update(dt);

            // advance animation time
            float lastTime = anim;
            anim.Update(dt);
            if (anim < lastTime) // wrapped?
                flapSounds.PlayRandom();
            captureGlow.Update(dt);

            // damp speed
            Speed *= 0.97f;

            // steer the player
            Vector3 steer = new Vector3(touchPos, 0) * MaxForce;
            steer = adjustRawSteeringForce(steer);
            ApplySteering(steer, dt);
        }

        public void DrawBat(float alpha)
        {
            SpriteBatch batch = Screen.ScreenManager.SpriteBatch;

            // compute the sprite angle
            float angle = (float)(Math.PI / 2 - Math.Atan(Forward.Y / Forward.X));
            if (Forward.X < 0)
                angle -= (float)Math.PI;

            const float scale = 0.3f; // match up Radius with size of head
            Vector2 origin = new Vector2(64, 32);   // center of head

            // get the texture
            Texture2D texture = anim.GetTexture();
            //batch.DrawString(screen.scoreFont, string.Format("anim={0}", animFrame), new Vector2(100, 100), Color.White);

            // draw the player
            Vector2 spos = Screen.world.Project(Position);
            Color color = new Color(new Vector4(1, 1, 1, alpha));
            batch.Draw(texture, spos, new Rectangle(0, 0, texture.Width, texture.Height),
                color, angle, origin, scale, SpriteEffects.None, 0);

#if DEBUG_CAPTURE
            // show the collision radius
            screen.drawPrims.AddCircle(spos, Radius, Color.Red);
#endif

#if DEBUG_STEERING
            // show the forward and steering vectors
            Vector3 dir = Forward * 20;
            screen.drawPrims.AddLine(spos.X, spos.Y, spos.X + dir.X, spos.Y - dir.Y, Color.Red);
            Vector3 dir2 = new Vector3(lastTouchPos, 0) * 20;
            screen.drawPrims.AddLine(spos.X, spos.Y, spos.X + dir2.X, spos.Y - dir2.Y, Color.White);

            // show input values
            string text = string.Format("({0},{1})", lastTouchPos.X, lastTouchPos.Y);
            screen.DrawString(screen.scoreFont, text, new Vector2(100, 100), Color.White);
#endif
        }

        public void DrawAdditive()
        {
            if (captureGlow > 0)
            {
                DrawBat(captureGlow);
            }
        }

        /// <summary>
        /// draw
        /// </summary>
        public override void Draw()
        {
            ghostEffect.Draw();
            sonar.Draw();
            DrawBat(1);
        }

        public void Capture(Boid b)
        {
            Score++;

            Screen.particles.CreatePlayerFireSmoke(Screen.world.Project(b.Position));
            //screen.particles.CreateAlienExplosion(screen.world.Project(b.Position));

            captureSound.Play();
            captureGlow.SetMode(Controller.Mode.RING);
        }
    }
}
