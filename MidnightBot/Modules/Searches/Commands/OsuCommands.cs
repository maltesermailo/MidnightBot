﻿using Discord.Commands;
using MidnightBot.Classes;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace MidnightBot.Modules.Searches.Commands
{
    internal class OsuCommands : DiscordCommand
    {
        public OsuCommands ( DiscordModule module ) : base (module)
        {
        }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "osu")
                  .Description ($"Zeigt Osu-Stats für einen Spieler. | `{Prefix}osu Name` oder `{Prefix}osu Name taiko`")
                  .Parameter ("usr",ParameterType.Required)
                  .Parameter ("mode",ParameterType.Unparsed)
                  .Do (async e =>
                  {
                      if (string.IsNullOrWhiteSpace (e.GetArg ("usr")))
                          return;

                      using (WebClient cl = new WebClient ())
                      {
                          try
                          {
                              var m = 0;
                              if (!string.IsNullOrWhiteSpace (e.GetArg ("mode")))
                              {
                                  m = ResolveGameMode (e.GetArg ("mode"));
                              }

                              cl.CachePolicy = new System.Net.Cache.RequestCachePolicy (System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                              cl.Headers.Add (HttpRequestHeader.UserAgent,"Mozilla/5.0 (Windows NT 6.2; Win64; x64)");
                              cl.DownloadDataAsync (new Uri ($"http://lemmmy.pw/osusig/sig.php?uname={ e.GetArg ("usr") }&flagshadow&xpbar&xpbarhex&pp=2&mode={m}"));
                              cl.DownloadDataCompleted += async ( s,cle ) =>
                              {
                                  try
                                  {
                                      await e.Channel.SendFile ($"{e.GetArg ("usr")}.png",new MemoryStream (cle.Result)).ConfigureAwait (false);
                                      await e.Channel.SendMessage ($"`Profil Link:`https://osu.ppy.sh/u/{Uri.EscapeDataString (e.GetArg ("usr"))}\n`Bild bereitgestellt von https://lemmmy.pw/osusig`").ConfigureAwait (false);
                                  }
                                  catch { }
                              };
                          }
                          catch
                          {
                              await e.Channel.SendMessage ("💢 Osu Signatur konnte nicht gefunden werden :\\").ConfigureAwait (false);
                          }
                      }
                  });

            cgb.CreateCommand (Module.Prefix + "osu b")
                .Description ($"Zeigt Informationen über eine Beatmap. | `{Prefix}osu b https://osu.ppy.sh/s/127712`")
                .Parameter ("map",ParameterType.Unparsed)
                .Do (async e =>
                {
                    if (string.IsNullOrWhiteSpace (MidnightBot.Creds.OsuAPIKey))
                    {
                        await e.Channel.SendMessage ("💢 Ein osu! API Key ist benötigt.").ConfigureAwait (false);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace (e.GetArg ("map")))
                        return;

                    try
                    {
                        var mapId = ResolveMap (e.GetArg ("map"));
                        var reqString = $"https://osu.ppy.sh/api/get_beatmaps?k={MidnightBot.Creds.OsuAPIKey}&{mapId}";
                        var obj = JArray.Parse (await SearchHelper.GetResponseStringAsync (reqString).ConfigureAwait (false))[0];
                        var sb = new System.Text.StringBuilder ();
                        var starRating = $"{obj["difficultyrating"]}";
                        starRating = starRating.Substring (0,(starRating.IndexOf ('.') + 3));
                        var time = TimeSpan.FromSeconds (Double.Parse ($"{obj["total_length"]}")).ToString (@"mm\:ss");
                        sb.AppendLine ($"{obj["artist"]} - {obj["title"]}, gemapped von {obj["creator"]}. https://osu.ppy.sh/s/{obj["beatmapset_id"]}");
                        sb.AppendLine ($"{starRating} Sterne, {obj["bpm"]} BPM | AR{obj["diff_approach"]}, CS{obj["diff_size"]}, OD{obj["diff_overall"]} | Länge: {time}");
                        await e.Channel.SendMessage (sb.ToString ()).ConfigureAwait (false);
                    }
                    catch
                    {
                        await e.Channel.SendMessage ("Etwas ist schief gelaufen");
                    }
                });

            cgb.CreateCommand (Module.Prefix + "osu top5")
                .Description ($"Zeigt die Top 5 Spiele eines Benutzers.  | `{Prefix}osu top5 Name`")
                .Parameter ("usr",ParameterType.Required)
                .Parameter ("mode",ParameterType.Unparsed)
                .Do (async e =>
                {
                    if (string.IsNullOrWhiteSpace (MidnightBot.Creds.OsuAPIKey))
                    {
                        await e.Channel.SendMessage ("💢 Ein osu! API Key ist benötigt.").ConfigureAwait (false);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace (e.GetArg ("usr")))
                    {
                        await e.Channel.SendMessage ("💢 Bitte gib einen Benutzernamen ein.").ConfigureAwait (false);
                        return;
                    }

                    try
                    {
                        var m = 0;
                        if (!string.IsNullOrWhiteSpace (e.GetArg ("mode")))
                        {
                            m = ResolveGameMode (e.GetArg ("mode"));
                        }

                        var reqString = $"https://osu.ppy.sh/api/get_user_best?k={MidnightBot.Creds.OsuAPIKey}&u={Uri.EscapeDataString (e.GetArg ("usr"))}&type=string&limit=5&m={m}";
                        var obj = JArray.Parse (await SearchHelper.GetResponseStringAsync (reqString).ConfigureAwait (false));
                        var sb = new System.Text.StringBuilder ($"`Top 5 Plays für {e.GetArg ("usr")}:`\n```xl" + Environment.NewLine);
                        foreach (var item in obj)
                        {
                            var mapReqString = $"https://osu.ppy.sh/api/get_beatmaps?k={MidnightBot.Creds.OsuAPIKey}&b={item["beatmap_id"]}";
                            var map = JArray.Parse (await SearchHelper.GetResponseStringAsync (mapReqString).ConfigureAwait (false))[0];
                            //var pp = Math.Round (Double.Parse ($"{item["pp"]}"),2);
                            var pp = $"{item["pp"]}";
                            pp = pp.Substring (0,(pp.IndexOf ('.') + 3));
                            var acc = CalculateAcc (item,m);
                            var mods = ResolveMods (Int32.Parse ($"{item["enabled_mods"]}"));
                            if (mods != "+")
                                sb.AppendLine ($"{pp + "pp",-7} | {acc + "%",-7} | {map["artist"] + "-" + map["title"] + " ("+ map["version"],-40}) | **{mods,-10}** | /b/{item["beatmap_id"]}");
                            else
                                sb.AppendLine ($"{pp + "pp",-7} | {acc + "%",-7} | {map["artist"] + "-" + map["title"] + " ("+ map["version"],-40}) | /b/{item["beatmap_id"]}");
                        }
                        sb.Append ("```");
                        await e.Channel.SendMessage (sb.ToString ()).ConfigureAwait (false);
                    }
                    catch
                    {
                        await e.Channel.SendMessage ("Etwas ist schief gelaufen.");
                    }
                });
        }

        //https://osu.ppy.sh/wiki/Accuracy
        private static Double CalculateAcc ( JToken play,int mode )
        {
            if (mode == 0)
            {
                var hitPoints = Double.Parse ($"{play["count50"]}") * 50 + Double.Parse ($"{play["count100"]}") * 100 + Double.Parse ($"{play["count300"]}") * 300;
                var totalHits = Double.Parse ($"{play["count50"]}") + Double.Parse ($"{play["count100"]}") + Double.Parse ($"{play["count300"]}") + Double.Parse ($"{play["countmiss"]}");
                totalHits *= 300;
                return Math.Round (hitPoints / totalHits * 100,2);
            }
            else if (mode == 1)
            {
                var hitPoints = Double.Parse ($"{play["countmiss"]}") * 0 + Double.Parse ($"{play["count100"]}") * 0.5 + Double.Parse ($"{play["count300"]}") * 1;
                var totalHits = Double.Parse ($"{play["countmiss"]}") + Double.Parse ($"{play["count100"]}") + Double.Parse ($"{play["count300"]}");
                hitPoints *= 300;
                totalHits *= 300;
                return Math.Round (hitPoints / totalHits * 100,2);
            }
            else if (mode == 2)
            {
                var fruitsCaught = Double.Parse ($"{play["count50"]}") + Double.Parse ($"{play["count100"]}") + Double.Parse ($"{play["count300"]}");
                var totalFruits = Double.Parse ($"{play["countmiss"]}") + Double.Parse ($"{play["count50"]}") + Double.Parse ($"{play["count100"]}") + Double.Parse ($"{play["count300"]}") + Double.Parse ($"{play["countkatu"]}");
                return Math.Round (fruitsCaught / totalFruits * 100,2);
            }
            else
            {
                var hitPoints = Double.Parse ($"{play["count50"]}") * 50 + Double.Parse ($"{play["count100"]}") * 100 + Double.Parse ($"{play["countkatu"]}") * 200 + (Double.Parse ($"{play["count300"]}") + Double.Parse ($"{play["countgeki"]}")) * 300;
                var totalHits = Double.Parse ($"{play["countmiss"]}") + Double.Parse ($"{play["count50"]}") + Double.Parse ($"{play["count100"]}") + Double.Parse ($"{play["countkatu"]}") + Double.Parse ($"{play["count300"]}") + Double.Parse ($"{play["countgeki"]}");
                totalHits *= 300;
                return Math.Round (hitPoints / totalHits * 100,2);
            }
        }

        private static string ResolveMap ( string mapLink )
        {
            Match s = new Regex (@"osu.ppy.sh\/s\/",RegexOptions.IgnoreCase).Match (mapLink);
            Match b = new Regex (@"osu.ppy.sh\/b\/",RegexOptions.IgnoreCase).Match (mapLink);
            Match p = new Regex (@"osu.ppy.sh\/p\/",RegexOptions.IgnoreCase).Match (mapLink);
            Match m = new Regex (@"&m=",RegexOptions.IgnoreCase).Match (mapLink);
            if (s.Success)
            {
                var mapId = mapLink.Substring (mapLink.IndexOf ("/s/") + 3);
                return $"s={mapId}";
            }
            else if (b.Success)
            {
                if (m.Success)
                    return $"b={mapLink.Substring (mapLink.IndexOf ("/b/") + 3,mapLink.IndexOf ("&m") - (mapLink.IndexOf ("/b/") + 3))}";
                else
                    return $"b={mapLink.Substring (mapLink.IndexOf ("/b/") + 3)}";
            }
            else if (p.Success)
            {
                if (m.Success)
                    return $"b={mapLink.Substring (mapLink.IndexOf ("?b=") + 3,mapLink.IndexOf ("&m") - (mapLink.IndexOf ("?b=") + 3))}";
                else
                    return $"b={mapLink.Substring (mapLink.IndexOf ("?b=") + 3)}";
            }
            else
            {
                return $"s={mapLink}"; //just a default incase an ID number was provided by itself (non-url)?
            }
        }

        private static int ResolveGameMode ( string mode )
        {
            switch (mode.ToLower ())
            {
                case "std":
                case "standard":
                case " std":
                case " standard":
                    return 0;
                case "taiko":
                case " taiko":
                    return 1;
                case "ctb":
                case "catchthebeat":
                case " ctb":
                case " catchthebeat":
                    return 2;
                case "mania":
                case "osu!mania":
                case " mania":
                case " osu!mania":
                    return 3;
                default:
                    return 0;
            }
        }

        //https://github.com/ppy/osu-api/wiki#mods
        private static string ResolveMods ( int mods )
        {
            var modString = $"+";

            if (IsBitSet (mods,0))
                modString += "NF";
            if (IsBitSet (mods,1))
                modString += "EZ";
            if (IsBitSet (mods,8))
                modString += "HT";

            if (IsBitSet (mods,3))
                modString += "HD";
            if (IsBitSet (mods,4))
                modString += "HR";
            if (IsBitSet (mods,6) && !IsBitSet (mods,9))
                modString += "DT";
            if (IsBitSet (mods,9))
                modString += "NC";
            if (IsBitSet (mods,10))
                modString += "FL";

            if (IsBitSet (mods,5))
                modString += "SD";
            if (IsBitSet (mods,14))
                modString += "PF";

            if (IsBitSet (mods,7))
                modString += "RX";
            if (IsBitSet (mods,11))
                modString += "AT";
            if (IsBitSet (mods,12))
                modString += "SO";
            return modString;
        }

        private static bool IsBitSet ( int mods,int pos )
        {
            return (mods & (1 << pos)) != 0;
        }
    }
}