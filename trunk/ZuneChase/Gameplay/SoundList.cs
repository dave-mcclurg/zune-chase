// ZuneChase - A game designed for your zune.
// Copyright (c) 2009 David McClurg <dpm@efn.org>
// Under the MIT License, details: License.txt.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace ZuneChase.Gameplay
{
    public class SoundList
    {
        List<SoundEffect> sounds = new List<SoundEffect>();
        Random random = new MyRandom(Environment.TickCount);

        public SoundList()
        {
        }

        public void Load(Game game, string name)
        {
            int num = 0;
            while (true)
            {
                string filename = string.Format(@"Sounds\{0}\{0} ({1:00})", name, ++num);
                try
                {
                    SoundEffect sound = game.Content.Load<SoundEffect>(filename);
                    sounds.Add(sound);
                }
                catch (Microsoft.Xna.Framework.Content.ContentLoadException)
                {
                    break;
                }
            }
        }

        public void PlayRandom()
        {
            sounds[random.Next(sounds.Count)].Play();
        }
    }
}