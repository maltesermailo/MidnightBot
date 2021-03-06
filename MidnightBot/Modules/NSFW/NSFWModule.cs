﻿using Discord.Commands;
using Discord.Modules;
using MidnightBot.Classes;
using MidnightBot.Modules.Permissions.Classes;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightBot.Modules.NSFW
{
    internal class NSFWModule : DiscordModule
    {

        private readonly Random rng = new Random ();

        public override string Prefix { get; } = MidnightBot.Config.CommandPrefixes.NSFW;

        public override void Install ( ModuleManager manager )
        {
            manager.CreateCommands ("",cgb =>
            {

                cgb.AddCheck (PermissionChecker.Instance);

                cgb.CreateCommand (Prefix + "hentai")
                    .Description ($"Zeigt ein zufälliges NSFW Hentai Bild von gelbooru und danbooru mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. (mehrere Tags mit + zwischen den Tags) | `{Prefix}hentai yuri+kissing`")
                    .Parameter ("tag",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var tag = e.GetArg ("tag")?.Trim () ?? "";
                        var links = await Task.WhenAll(SearchHelper.GetGelbooruImageLink("rating%3Aexplicit+" + tag), SearchHelper.GetDanbooruImageLink("rating%3Aexplicit+" + tag), SearchHelper.GetATFBooruImageLink("rating%3Aexplicit+" + tag)).ConfigureAwait(false);

                        if (links.All(l => l == null))
                        {
                            await e.Channel.SendMessage ("`Keine Ergebnisse.`");
                            return;
                        }

                        await e.Channel.SendMessage(String.Join("\n\n", links)).ConfigureAwait(false);
                    });

                cgb.CreateCommand(Prefix + "atfbooru")
                    .Alias (Prefix + "atf")
                    .Description($"Shows a random hentai image from atfbooru with a given tag. Tag is optional but preffered. (multiple tags are appended with +) | `{Prefix}atf yuri+kissing`")
                    .Parameter("tag", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        var tag = e.GetArg("tag")?.Trim() ?? "";
                        var link = await SearchHelper.GetATFBooruImageLink (tag).ConfigureAwait (false);
                        if (string.IsNullOrWhiteSpace(link))
                            await e.Channel.SendMessage("Suche ergab keine Ergebnisse ;(");
                        else
                            await e.Channel.SendMessage(link).ConfigureAwait(false);
                    });

                cgb.CreateCommand (Prefix + "danbooru")
                    .Description ($"Zeigt ein zufälliges Hentai Bild von danbooru mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. (mehrere Tags mit + zwischen den Tags) | `{Prefix}danbooru yuri+kissing`")
                    .Parameter ("tag",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var tag = e.GetArg ("tag")?.Trim () ?? "";
                        var link = await SearchHelper.GetDanbooruImageLink (tag).ConfigureAwait (false);
                        if (string.IsNullOrWhiteSpace (link))
                            await e.Channel.SendMessage ("Suche ergab keine Ergebnisse ;(");
                        else
                            await e.Channel.SendMessage (link).ConfigureAwait (false);
                    });

                cgb.CreateCommand(Prefix + "r34")
                .Description($"Zeigt ein zufälliges Hentai Bild von rule34.paheal.net mit einem gegebenen Tag. | `{Prefix}r34 bacon`")
                .Parameter("tag", ParameterType.Unparsed)
                .Do(async e =>
                {
                    var tag = e.GetArg("tag")?.Trim() ?? "";
                    var link = await SearchHelper.GetR34ImageLink(tag).ConfigureAwait(false);
                    if (string.IsNullOrWhiteSpace(link))
                        await e.Channel.SendMessage("Search yielded no results ;(");
                    else
                        await e.Channel.SendMessage(link).ConfigureAwait(false);
                });

                cgb.CreateCommand (Prefix + "gelbooru")
                    .Description ($"Zeigt ein zufälliges Hentai Bild von gelbooru mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. (mehrere Tags mit + zwischen den Tags) | `{Prefix}gelbooru yuri+kissing`")
                    .Parameter ("tag",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var tag = e.GetArg ("tag")?.Trim () ?? "";
                        var link = await SearchHelper.GetGelbooruImageLink (tag).ConfigureAwait (false);
                        if (string.IsNullOrWhiteSpace (link))
                            await e.Channel.SendMessage ("Suche ergab keine Ergebnisse ;(");
                        else
                            await e.Channel.SendMessage (link).ConfigureAwait (false);
                    });
                cgb.CreateCommand (Prefix + "rule34")
                    .Description ($"Zeigt ein zufälliges Hentai Bild von rule34.xx  mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. Benutze + für mehrere Tags. | `{Prefix}rule34 yuri+kissing`")
                    .Parameter ("tag",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var tag = e.GetArg ("tag")?.Trim () ?? "";
                        var link = await SearchHelper.GetRule34ImageLink (tag).ConfigureAwait (false);
                        if (string.IsNullOrWhiteSpace (link))
                            await e.Channel.SendMessage ("Suche ergab keine Ergebnisse ;(");
                        else
                            await e.Channel.SendMessage (link).ConfigureAwait (false);
                    });
                cgb.CreateCommand (Prefix + "e621")
                    .Description ($"Zeigt ein zufälliges Hentai Bild von e621.net mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. Benutze Leerzeichen für mehrere Tags. | `{Prefix}e621 yuri+kissing`")
                    .Parameter ("tag",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var tag = e.GetArg ("tag")?.Trim () ?? "";
                        await e.Channel.SendMessage (await SearchHelper.GetE621ImageLink (tag).ConfigureAwait (false)).ConfigureAwait (false);
                    });
                cgb.CreateCommand (Prefix + "derpi")
                    .Description ($"Zeigt ein zufälliges Hentai Bild von derpiboo.ru mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. Benutze + für mehrere Tags. | `{Prefix}derpi yuri+kissing`")
                    .Parameter ("tag",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var tag = e.GetArg ("tag")?.Trim () ?? "";
                        await e.Channel.SendIsTyping ().ConfigureAwait (false);
                        await e.Channel.SendMessage (await SearchHelper.GetDerpibooruImageLink (tag).ConfigureAwait (false)).ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "boobs")
                    .Description ($"Erwachsenen Inhalt. | `{Prefix}boobs`")
                    .Do (async e =>
                    {
                        try
                        {
                            var obj = JArray.Parse (await SearchHelper.GetResponseStringAsync ($"http://api.oboobs.ru/boobs/{rng.Next (0,9380)}").ConfigureAwait (false))[0];
                            await e.Channel.SendMessage ($"http://media.oboobs.ru/{ obj["preview"].ToString () }").ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ($"💢 {ex.Message}").ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "butts")
                    .Alias (Prefix + "ass",Prefix + "butt")
                    .Description ($"Erwachsenen Inhalt. | `{Prefix}butts` oder `{Prefix}ass`")
                    .Do (async e =>
                    {
                        try
                        {
                            var obj = JArray.Parse (await SearchHelper.GetResponseStringAsync ($"http://api.obutts.ru/butts/{rng.Next (0,3373)}").ConfigureAwait (false))[0];
                            await e.Channel.SendMessage ($"http://media.obutts.ru/{ obj["preview"].ToString () }").ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ($"💢 {ex.Message}").ConfigureAwait (false);
                        }
                    });
            });
        }
    }
}