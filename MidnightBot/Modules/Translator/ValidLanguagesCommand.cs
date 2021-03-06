﻿using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Modules.Translator.Helpers;
using System;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Translator
{
    class ValidLanguagesCommand : DiscordCommand
    {
        public ValidLanguagesCommand ( DiscordModule module ) : base (module) { }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand(Module.Prefix + "translangs")
                .Description ($"Listet die verfügbaren Sprachen zur Übersetzung. | `{Prefix}translangs` oder `{Prefix}translangs language`")
                .Parameter ("search",ParameterType.Optional)
                .Do ( ListLanguagesFunc());
        }
        private Func<CommandEventArgs,Task> ListLanguagesFunc () => async e =>
        {
            try
            {
                GoogleTranslator.EnsureInitialized();
                string s = e.GetArg ("search");
                string ret = "";
                foreach (string key in GoogleTranslator._languageModeMap.Keys)
                {
                    if (!s.Equals(""))
                    {
                        if (key.ToLower().Contains ( s))
                        {
                            ret += " " + key + ";";
                        }
                    }
                    else
                    {
                        ret += " " + key + ";";
                    }
                }
                await e.Channel.SendMessage ( ret).ConfigureAwait (false);
            }
            catch
            {
                await e.Channel.SendMessage ("Falsches Eingabeformat, oder etwas ist falsch gelaufen...").ConfigureAwait (false);
            }

        };
    }
}