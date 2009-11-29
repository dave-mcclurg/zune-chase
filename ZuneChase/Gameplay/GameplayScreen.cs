// ZuneChase - A game designed for your zune.
// Copyright (c) 2009 David McClurg <dpm@efn.org>
// Under the MIT License, details: License.txt.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics;
using GameState;

namespace ZuneChase.Gameplay
{
    /// <summary>
    /// A system owned by the gameplay screen
    /// </summary>
    public abstract class GameplayComponent
    {
        protected GameplayScreen Screen;
        public static Game Game = null; // not available until GameplayScreen.LoadContent()

        public GameplayComponent(GameplayScreen screen)
        {
            this.Screen = screen;
        }

        public virtual void Load() { }
        public virtual void Update(float dt) { }
        public virtual void Draw() { }
    }

    /// <summary>
    /// This component draws the entire background for the game.  It handles
    /// drawing the ground, clouds, sun, and moon.  also handles animating them
    /// and day/night transitions
    /// </summary>
    public sealed class GameplayScreen : GameScreen
    {
        #region singleton
        // http://www.yoda.arachsys.com/csharp/singleton.html
        public static GameplayScreen Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly GameplayScreen instance = new GameplayScreen();
        }
        #endregion // singleton

        bool gameOver = false;
        Song mysong;

        public SpriteFont scoreFont;
        public SpriteFont menuFont;

        int levelSpawned = 0;
        int totalCaptures = 0;
        int currentLevel = 0;
        float timeRemaining = 2*60;

        internal MyRandom random;
        internal ZunePad zunePad;
        internal Highscores highScores;
        internal DrawText drawText;
        internal DrawPrims drawPrims;
        internal BoidManager boidManager;
        internal ParticleSystem particles;
        internal World world;
        internal Player player;

        private GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            random = new MyRandom(Environment.TickCount);
            zunePad = new ZunePad();
            highScores = new Highscores(this);
            drawText = new DrawText(this);
            drawPrims = new DrawPrims(this);
            boidManager = new BoidManager(this);
            particles = new ParticleSystem(this);
            world = new World(this);
            player = new Player(this);
        }

        /// <summary>
        /// Loads all of the content needed by the game.  All of this should have already been cached
        /// into the ContentManager by the LoadingScreen.
        /// </summary>
        public override void LoadContent()
        {
            GameplayComponent.Game = ScreenManager.Game;

            scoreFont = ScreenManager.Game.Content.Load<SpriteFont>(@"Fonts\ScoreFont");
            menuFont = ScreenManager.Game.Content.Load<SpriteFont>(@"Fonts\MenuFont");

            highScores.Load();
            drawText.Load();
            drawPrims.Load();
            particles.Load();
            world.Load();
            boidManager.Load();
            player.Load();

            mysong = ScreenManager.Game.Content.Load<Song>(@"Sounds\ambient");
            MediaPlayer.Play(mysong);

            base.LoadContent();

            // Automatically start the game once all loading is done
            NextLevel();
        }

        /// <summary>
        /// Save the scores and dispose of the particles
        /// </summary>
        public override void UnloadContent()
        {
            highScores.Save();

            particles = null;
            MediaPlayer.Stop();

            base.UnloadContent();
        }

        /// <summary>
        /// Runs one frame of update for the game.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (IsActive)
            {
                //int now = Environment.TickCount;
                //float dt = (now - lastTick) / 1000.0f;
                //dt = MathHelper.Clamp(dt, 0, 1 / 30.0f);

                world.LookAt(player.Position.X, player.Position.Y);

                zunePad.Update(gameTime);
                world.Update(dt);
                boidManager.Update(dt);
                player.Update(dt);
                particles.Update(dt);
                drawText.Update(dt);

                CheckCapture();

                timeRemaining -= dt;
                if (timeRemaining < 0)
                    timeRemaining = 0;
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Input helper method provided by GameScreen.  Packages up the various input
        /// values for ease of use.  Here it checks for pausing and handles controlling
        /// the player's tank.
        /// </summary>
        /// <param name="input">The state of the gamepads</param>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            //// Allows the game to exit
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            //    this.Exit();
            //if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
            //    this.Exit();

            if (input.PauseGame)
            {
                if (gameOver == true)
                {
                    foreach (GameScreen screen in ScreenManager.GetScreens())
                        screen.ExitScreen();

                    ScreenManager.AddScreen(new MainMenuScreen());
                }
                else
                {
                    ScreenManager.AddScreen(new PauseMenuScreen());
                }
            }
            else
            {
            }
        }

        /// <summary>
        /// Draw the game world, effects, and hud
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin();
            world.Draw();
            boidManager.Draw();
            particles.Draw();
            player.Draw();
            ScreenManager.SpriteBatch.End();

            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.Additive, SpriteSortMode.Deferred, SaveStateMode.None);
            player.DrawAdditive();
            ScreenManager.SpriteBatch.End();

            ScreenManager.SpriteBatch.Begin();
            DrawHud();
            drawPrims.Draw();
            drawText.Draw();
            ScreenManager.SpriteBatch.End();
        }

        public void NextLevel()
        {
            currentLevel++;
            player.Score = 0;
            levelSpawned = random.Next(5, 10);

            boidManager.List.Clear();
            for (int i = 0; i < levelSpawned; i++)
            {
                Firefly f = new Firefly(this);
                f.Load();
                boidManager.List.Add(f);
            }
            for (int i = 0; i < 2; i++)
            {
                Butterfly b = new Butterfly(this);
                b.Load();
                boidManager.List.Add(b);
            }
        }

        void CheckCapture()
        {
            foreach (Boid b in boidManager.List)
            {
                Vector3 v = b.Position - player.Position;
                if (v.LengthSquared() < player.Radius * player.Radius)
                {
                    // in front of us?
                    if (Vector3.Dot(v, player.Forward) > 0)
                    {
                        player.Capture(b);
                        timeRemaining += 10;
                        totalCaptures++;

                        boidManager.List.Remove(b);
                        if (boidManager.List.Count == 0)
                        {
                            NextLevel();
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Draw the hud, which consists of the score elements and the GAME OVER tag.
        /// </summary>
        void DrawHud()
        {
            if (gameOver)
            {
                Vector2 size = menuFont.MeasureString("GAME OVER");
                drawText.Add(menuFont, "GAME OVER", new Vector2(ScreenManager.Game.GraphicsDevice.Viewport.Width / 2 - size.X / 2, ScreenManager.Game.GraphicsDevice.Viewport.Height / 2 - size.Y / 2), new Color(255, 64, 64));
            }
            else
            {
                System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.CurrentCulture;

                // Score
                string text = string.Format("SCORE: {0}/{1} ({2})",
                    player.Score.ToString(ci),
                    levelSpawned.ToString(ci),
                    totalCaptures.ToString(ci));
                drawText.Add(scoreFont, text, new Vector2(6, 4), Color.Yellow);

                int mins = ((int)timeRemaining) / 60;
                int secs = ((int)timeRemaining) % 60;
                text = "TIME: " + string.Format("{0}:{1:00}", mins, secs);
                Vector2 size = scoreFont.MeasureString(text);
                drawText.Add(scoreFont, text, new Vector2(234 - (int)size.X, 4), Color.Yellow);

                int highScore = highScores.highScore;
                text = "HIGH SCORE: " + highScore.ToString(ci);
                drawText.Add(scoreFont, text, new Vector2(6, 300), Color.Yellow);

                text = "LEVEL: " + currentLevel.ToString(ci);
                size = scoreFont.MeasureString(text);
                drawText.Add(scoreFont, text, new Vector2(234 - (int)size.X, 300), Color.Yellow);
            }
        }
    }
}