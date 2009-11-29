#region File Description
//-----------------------------------------------------------------------------
// MessageBoxScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace GameState
{
    /// <summary>
    /// A popup message box screen, used to display "are you sure?"
    /// confirmation messages.
    /// </summary>
    class MessageBoxScreen : GameScreen
    {
        #region Fields

        string message;
        Texture2D gradientTexture;

        string usageTextOk = "Yes";
        string usageTextCancel = "No";

        bool includeUsageTextOk = false;
        bool includeUsageTextCancel = false;

        #endregion

        #region Events

        public event EventHandler<EventArgs> Accepted;
        public event EventHandler<EventArgs> Cancelled;

        #endregion

        #region Initialization


        public MessageBoxScreen(string message, string usageTextOk, string usageTextCancel)
        {
            this.message = message;

            this.usageTextOk = usageTextOk;
            this.includeUsageTextOk = (null != usageTextOk);

            this.usageTextCancel = usageTextCancel;
            this.includeUsageTextCancel = (null != usageTextCancel);

            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);
        }


        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent MessageBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            gradientTexture = content.Load<Texture2D>(@"Textures\Menus\gradient");
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input.MenuSelect)
            {
                // Raise the accepted event, then exit the message box.
                if (Accepted != null)
                    Accepted(this, EventArgs.Empty);

                ExitScreen();
            }
            else if (input.MenuCancel)
            {
                // Raise the cancelled event, then exit the message box.
                if (Cancelled != null)
                    Cancelled(this, EventArgs.Empty);

                ExitScreen();
            }
        }


        #endregion

        #region Draw


        /// <summary>
        /// Draws the message box.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            // Darken down any other screens that were drawn beneath the popup.
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            //Usage Text
            Vector2 textPosition = new Vector2(10, 20);

#if false
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);

            // Center the message text in the viewport.
            Vector2 textSize = font.MeasureString(message);

            // The background includes a border somewhat larger than the text itself.
            const int hPad = 32;
            const int vPad = 16;

            Rectangle backgroundRectangle = new Rectangle((int)textPosition.X - hPad,
                                                          (int)textPosition.Y - vPad,
                                                          (int)textSize.X + hPad * 2,
                                                          (int)textSize.Y + vPad * 2);
#endif

            // Fade the popup alpha during transitions.
            Color color = new Color(255, 255, 255, TransitionAlpha);

            spriteBatch.Begin();

            // Draw the message box text.
            spriteBatch.DrawString(font, 
                wordwrap(ScreenManager.GraphicsDevice.Viewport.Width,message,font), 
                textPosition, color);

            // Draw Usage Text
            if (this.includeUsageTextOk || this.includeUsageTextCancel)
            {
                // Display both Usage Lines
                spriteBatch.DrawString(font, usageTextOk, new Vector2(220 - font.MeasureString(usageTextOk).X, 260), color);
                spriteBatch.DrawString(font, usageTextCancel, new Vector2(220 - font.MeasureString(usageTextCancel).X, 280), color);
            }
            else if (this.includeUsageTextOk)
            {
                // Display only the Ok Usage Line
                spriteBatch.DrawString(font, usageTextOk, new Vector2(10, 300), color);
            }

            spriteBatch.End();
        }


        #endregion

        public string wordwrap(int width, String in_string, SpriteFont in_font)
        {
            int x;
            String current_line = "";
            String current_word = "";
            String new_string = "";
            for (x = 0; x < in_string.Length; x++)
            {
                if (in_string[x].CompareTo(' ') == 0)
                {
                    if (in_font.MeasureString(current_word).X + in_font.MeasureString(current_line + " ").X > width)
                    {
                        new_string = new_string + current_line + "\n";
                        current_line = current_word + " ";
                        current_word = "";
                    }
                    else
                    {
                        if (current_line.Length > 0)
                        {
                            current_line = current_line + " " + current_word;
                            current_word = "";
                        }
                        else
                        {
                            current_line = current_word;
                            current_word = "";
                        }
                    }
                }
                else
                {
                    current_word = current_word + in_string[x];
                }
            }
            new_string = new_string + current_line + " " + current_word;
            return new_string;
        }

    }
}
