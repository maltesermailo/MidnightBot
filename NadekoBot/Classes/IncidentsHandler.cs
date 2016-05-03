﻿using System;
using System.IO;
using Discord;

namespace NadekoBot.Classes {
    internal static class IncidentsHandler {
        public static async void Add(ulong serverId, string text)
        {
            Directory.CreateDirectory ("data/incidents");
            File.AppendAllText ($"data/incidents/{serverId}.txt", text + "\n--------------------------\n");
            var def = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine ($"VORFALL: {text}");
            Console.ForegroundColor = def;
            Channel OwnerPrivateChannel = await NadekoBot.Client.CreatePrivateChannel (NadekoBot.Creds.OwnerIds[0]);
            await OwnerPrivateChannel.SendMessage ($"VORFALL: {text}");
        }
    }
}
