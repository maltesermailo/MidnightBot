﻿using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Timers;

namespace MidnightBot.Modules.Administration.Commands
{
    class MessageRepeater : DiscordCommand
    {
        private readonly ConcurrentDictionary<Server,Repeater> repeaters = new ConcurrentDictionary<Server,Repeater> ();
        private class Repeater
        {
            [Newtonsoft.Json.JsonIgnore]
            public Timer MessageTimer { get; set; }
            [Newtonsoft.Json.JsonIgnore]
            public Channel RepeatingChannel { get; set; }

            public ulong RepeatingServerId { get; set; }
            public ulong RepeatingChannelId { get; set; }
            public Message lastMessage { get; set; } = null;
            public string RepeatingMessage { get; set; }
            public int Interval { get; set; }

            public Repeater Start ()
            {
                MessageTimer = new Timer { Interval = Interval };
                MessageTimer.Elapsed += async ( s,e ) => await Invoke ();
                return this;
            }

            public async Task Invoke ()
            {
                var ch = RepeatingChannel;
                var msg = RepeatingMessage;
                if (ch != null && !string.IsNullOrWhiteSpace (msg))
                {
                    try
                    {
                        if (lastMessage != null)
                            await lastMessage.Delete ().ConfigureAwait (false);
                    }
                    catch { }
                    try
                    {
                        lastMessage = await ch.SendMessage (msg).ConfigureAwait (false);
                    }
                    catch { }
                }
            }
        }
        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "repeatinvoke")
                .Alias (Module.Prefix + "repinv")
                .Description ($"Zeigt die Repeat Nachricht sofort an und startet den Timer neu. | `{Prefix}repinv`")
                .AddCheck (SimpleCheckers.ManageMessages ())
                .Do (async e =>
                {
                    Repeater rep;
                    if (!repeaters.TryGetValue (e.Server,out rep))
                    {
                        await e.Channel.SendMessage ("`Keine Nachricht die auf diesem Server wiederholt wird.`");
                        return;
                    }

                    await rep.Invoke ();
                });


            cgb.CreateCommand (Module.Prefix + "repeat")
                .Description ("Wiederholt eine Nachricht alle X Minuten. Falls nicht spezifiziert, " +
                             $"Wiederholung ist deaktiviert. Benötigt 'manage messages'. | `{Prefix}repeat 5 Hello there`")
                .Parameter ("minutes",ParameterType.Optional)
                .Parameter ("msg",ParameterType.Unparsed)
                .AddCheck (SimpleCheckers.ManageMessages ())
                .Do (async e =>
                {
                    var minutesStr = e.GetArg ("minutes");
                    var msg = e.GetArg ("msg");

                    // if both null, disable
                    if (string.IsNullOrWhiteSpace (msg) && string.IsNullOrWhiteSpace (minutesStr))
                    {
                        Repeater rep;
                        if (!repeaters.TryRemove(e.Server, out rep))
                            return;
                        rep.MessageTimer.Stop();
                        await e.Channel.SendMessage("Wiederholung deaktiviert").ConfigureAwait(false);
                        return;
                    }
                    int minutes;
                    if (!int.TryParse (minutesStr,out minutes) || minutes < 1 || minutes > 1440)
                    {
                        await e.Channel.SendMessage ("Ungüültige Eingabe").ConfigureAwait (false);
                        return;
                    }

                    var repeater = repeaters.GetOrAdd (
                        e.Server,
                        s => new Repeater
                        {
                            Interval = minutes * 60 * 1000,
                            RepeatingChannel = e.Channel,
                            RepeatingChannelId = e.Channel.Id,
                            RepeatingServerId = e.Server.Id,
                        }.Start ()
                    );

                    if (!string.IsNullOrWhiteSpace (msg))
                        repeater.RepeatingMessage = msg;

                    repeater.MessageTimer.Stop ();
                    repeater.MessageTimer.Start ();

                    await e.Channel.SendMessage (String.Format ("👌 Wiederhole `{0}` alle " +
                                                              "**{1}** Minuten auf Channel {2}.",
                                                              repeater.RepeatingMessage,minutes,repeater.RepeatingChannel))
                                                              .ConfigureAwait (false);
                });
        }

        public MessageRepeater ( DiscordModule module ) : base (module) { }
    }
}
