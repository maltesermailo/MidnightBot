﻿using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Classes.JSONModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MidnightBot.Modules.Searches.Commands
{
    class WowJokeCommand : DiscordCommand
    {
        List<WoWJoke> jokes = new List<WoWJoke> ();

        public WowJokeCommand ( DiscordModule module ) : base (module)
        {
        }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "wowjoke")
                .Description ("Get one of Kwoth's penultimate WoW jokes.")
                .Do (async e =>
                {
                    if (!jokes.Any ())
                    {
                        jokes = JsonConvert.DeserializeObject<List<WoWJoke>> (File.ReadAllText ("data/wowjokes.json"));
                    }
                    await e.Channel.SendMessage (jokes[new Random ().Next (0,jokes.Count)].ToString ());
                });
        }
    }
}