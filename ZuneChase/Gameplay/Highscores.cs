// ZuneChase - A game designed for your zune.
// Copyright (c) 2009 David McClurg <dpm@efn.org>
// Under the MIT License, details: License.txt.

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace ZuneChase.Gameplay
{
    public class Highscores : GameplayComponent
    {
        public int highScore = 0;

        public Highscores(GameplayScreen screen)
            : base(screen)
        {
        }

        /// <summary>
        /// Loads the high score from a text file.  The StorageDevice was selected during the loading screen.
        /// </summary>
        public override void Load()
        {
            StorageDevice device = (StorageDevice)Game.Services.GetService(typeof(StorageDevice));
            if (device != null)
            {
                StorageContainer container = device.OpenContainer("ZuneChase");
                if (File.Exists(Path.Combine(container.Path, "highscores.txt")))
                {
                    StreamReader reader = null;

                    try
                    {
                        reader = new StreamReader(Path.Combine(container.Path, "highscores.txt"));
                        highScore = Int32.Parse(reader.ReadToEnd(), System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        highScore = 10000;
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Close();
                            reader.Dispose();
                        }
                    }
                }

                container.Dispose();
            }
        }

        /// <summary>
        /// Saves the current highscore to a text file. The StorageDevice was selected during the loading screen.
        /// </summary>
        public void Save()
        {
            StorageDevice device = (StorageDevice)Game.Services.GetService(typeof(StorageDevice));
            if (device != null)
            {
                StorageContainer container = device.OpenContainer("ZuneChase");
                StreamWriter writer = new StreamWriter(Path.Combine(container.Path, "highscores.txt"));
                writer.Write(highScore.ToString(System.Globalization.CultureInfo.InvariantCulture));
                writer.Flush();
                writer.Close();
                container.Dispose();
            }
        }
    }
}