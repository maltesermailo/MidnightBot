﻿using Discord.Commands;
using NadekoBot.Classes;
using NadekoBot.Modules.Games.Commands.Trivia;
using NadekoBot.Modules.Permissions.Classes;
using System.Collections.Concurrent;
using System.Linq;

namespace NadekoBot.Modules.Games.Commands
{
    internal class TriviaCommands : DiscordCommand
    {
        public static ConcurrentDictionary<ulong,TriviaGame> RunningTrivias = new ConcurrentDictionary<ulong,TriviaGame> ();

        public TriviaCommands ( DiscordModule module ) : base (module)
        {
        }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "t")
                .Description ($"Startet ein Quiz. Du kannst nohint hinzufügen um Tipps zu verhindern." +
                               "Erster Spieler mit 10 Punkten gewinnt. 30 Sekunden je Frage." +
                              $"\n**Benutzung**:`{Module.Prefix}t nohint`")
                 .Parameter ("args",ParameterType.Multiple)
                 .AddCheck(SimpleCheckers.ManageMessages())
                 .Do (async e =>
                  {
                      TriviaGame trivia;
                      if (!RunningTrivias.TryGetValue (e.Server.Id,out trivia))
                      {
                          var showHints = !e.Args.Contains ("nohint");
                          var triviaGame = new TriviaGame (e,showHints);
                          if (RunningTrivias.TryAdd (e.Server.Id,triviaGame))
                              await e.Channel.SendMessage ("**Quiz gestartet!**").ConfigureAwait (false);
                          else
                              await triviaGame.StopGame ().ConfigureAwait (false);
                      }
                      else
                          await e.Channel.SendMessage ("Auf diesem Server läuft bereits ein Quiz.\n" + trivia.CurrentQuestion).ConfigureAwait (false);
                  });

            cgb.CreateCommand (Module.Prefix + "tl")
                .Description ("Zeigt eine Rangliste des derzeitigen Quiz.")
                .Do (async e =>
                {
                    TriviaGame trivia;
                    if (RunningTrivias.TryGetValue (e.Server.Id,out trivia))
                        await e.Channel.SendMessage (trivia.GetLeaderboard ()).ConfigureAwait (false);
                    else
                        await e.Channel.SendMessage ("Es läuft kein Quiz auf diesem Server.").ConfigureAwait (false);
                });

            cgb.CreateCommand (Module.Prefix + "tq")
                .Description ("Beendet Quiz nach der derzeitgen Frage.")
                .AddCheck(SimpleCheckers.ManageMessages())
                .Do (async e =>
                {
                    TriviaGame trivia;
                    if (RunningTrivias.TryGetValue (e.Server.Id,out trivia))
                    {
                        await trivia.StopGame ().ConfigureAwait (false);
                    }
                    else
                        await e.Channel.SendMessage ("Es läuft kein Quiz auf diesem Server.").ConfigureAwait (false);
                });
        }
    }
}