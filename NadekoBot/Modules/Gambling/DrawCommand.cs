﻿using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Extensions;
using MidnightBot.Modules.Gambling.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Gambling
{
    internal class DrawCommand : DiscordCommand
    {
        public DrawCommand(DiscordModule module) : base (module)
    {
    }

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Module.Prefix + "draw")
                .Description("Zieht eine Karte vom Stapel.Wenn du eine Nummer angibst [x], werden bis zu 5 Karten vom Stapel gezogen.\n**Benutzung**: $draw [x]")
                .Parameter("count", ParameterType.Optional)
                .Do(DrawCardFunc());

            cgb.CreateCommand(Module.Prefix + "shuffle")
                .Alias(Module.Prefix + "sh")
                .Description("Mischt alle Karten zurück in den Stapel.")
                .Do(ReshuffleTask());
        }

        private static readonly ConcurrentDictionary<Discord.Server, Cards> AllDecks = new ConcurrentDictionary<Discord.Server, Cards>();

        private static Func<CommandEventArgs, Task> ReshuffleTask()
        {
            return async e =>
            {
                AllDecks.AddOrUpdate(e.Server,
                    (s) => new Cards(),
                    (s, c) =>
                    {
                        c.Restart();
                        return c;
                    });

                await e.Channel.SendMessage("Deck reshuffled.").ConfigureAwait (false);
            };
        }

        private Func<CommandEventArgs, Task> DrawCardFunc() => async (e) =>
        {
            var cards = AllDecks.GetOrAdd(e.Server, (s) => new Cards());

            try
            {
                var num = 1;
                var isParsed = int.TryParse(e.GetArg("count"), out num);
                if (!isParsed || num < 2)
                {
                    var c = cards.DrawACard();
                    await e.Channel.SendFile(c.Name + ".jpg", (Properties.Resources.ResourceManager.GetObject(c.Name) as Image).ToStream()).ConfigureAwait (false);
                    return;
                }
                if (num > 5)
                    num = 5;

                var images = new List<Image>();
                var cardObjects = new List<Cards.Card>();
                for (var i = 0; i < num; i++)
                {
                    if (cards.CardPool.Count == 0 && i != 0)
                    {
                        await e.Channel.SendMessage("Keine Karten mehr im Stapel.").ConfigureAwait (false);
                        break;
                    }
                    var currentCard = cards.DrawACard();
                    cardObjects.Add(currentCard);
                    images.Add(Properties.Resources.ResourceManager.GetObject(currentCard.Name) as Image);
                }
                var bitmap = images.Merge();
                await e.Channel.SendFile(images.Count + " cards.jpg", bitmap.ToStream());
                if (cardObjects.Count == 5)
                {
                    await e.Channel.SendMessage(Cards.GetHandValue(cardObjects)).ConfigureAwait (false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Ziehen von Karte(n) " + ex.ToString());
            }
        };
    }
}
