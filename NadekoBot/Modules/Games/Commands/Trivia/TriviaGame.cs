﻿using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Games.Commands.Trivia
{
    internal class TriviaGame
    {
        private readonly object _guessLock = new object();

        private Server server { get; }
        private Channel channel { get; }

        private int QuestionDurationMiliseconds { get; } = 30000;
        private int HintTimeoutMiliseconds { get; } = 6000;
        public bool ShowHints { get; set; } = true;
        private CancellationTokenSource triviaCancelSource { get; set; }

        public TriviaQuestion CurrentQuestion { get; private set; }
        public HashSet<TriviaQuestion> oldQuestions { get; } = new HashSet<TriviaQuestion>();

        public ConcurrentDictionary<User, int> Users { get; } = new ConcurrentDictionary<User, int>();

        public bool GameActive { get; private set; } = false;
        public bool ShouldStopGame { get; private set; }

        public int WinRequirement { get; } = 10;

        public TriviaGame ( CommandEventArgs e,bool showHints,int winReq = 10 )
        {
            ShowHints = showHints;
            server = e.Server;
            channel = e.Channel;
            WinRequirement = winReq;
            Task.Run(StartGame);
        }

        private async Task StartGame()
        {
            while (!ShouldStopGame)
            {
                // reset the cancellation source
                triviaCancelSource = new CancellationTokenSource();
                var token = triviaCancelSource.Token;
                // load question
                CurrentQuestion = TriviaQuestionPool.Instance.GetRandomQuestion(oldQuestions);
                if (CurrentQuestion == null)
                {
                    await channel.SendMessage($":exclamation: Es konnte keine Frage geladen werden").ConfigureAwait (false);
                    await End().ConfigureAwait (false);
                    return;
                }
                var category = ":grey_question:";
                switch (CurrentQuestion.Category)
                {
                    case "Tiere":
                        {
                            category = ":tiger:";
                            break;
                        }
                    case "Abkürzungen":
                        {
                            category = ":abc:";
                            break;
                        }
                    default:
                            break;
                }
                oldQuestions.Add(CurrentQuestion); //add it to exclusion list so it doesn't show up again
                                                   //sendquestion
                await channel.SendMessage($"{category}__**Kategorie: {CurrentQuestion.Category}**__\n:question:**{CurrentQuestion.Question}**").ConfigureAwait (false);

                //receive messages
                MidnightBot.Client.MessageReceived += PotentialGuess;

                //allow people to guess
                GameActive = true;

                try
                {
                    //hint
                    await Task.Delay(HintTimeoutMiliseconds, token).ConfigureAwait (false);
                    //if (ShowHints)
                    //    await channel.SendMessage ($":exclamation:**Tipp:** {CurrentQuestion.GetHint ()}").ConfigureAwait (false);

                    //timeout
                    await Task.Delay(QuestionDurationMiliseconds - HintTimeoutMiliseconds, token).ConfigureAwait (false);

                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Quiz beendet");
                }
                GameActive = false;
                if (!triviaCancelSource.IsCancellationRequested)
                    await channel.Send($":clock2: :question: **Zeit vorbei!** Die richtige Antwort war **{CurrentQuestion.Answer}**").ConfigureAwait (false);
                MidnightBot.Client.MessageReceived -= PotentialGuess;
                // load next question if game is still running
                await Task.Delay(2000).ConfigureAwait (false);
            }
            await End().ConfigureAwait (false);
        }

        private async Task End()
        {
            ShouldStopGame = true;
            await channel.SendMessage("**Quiz ist zuende**\n" + GetLeaderboard());
            TriviaGame throwAwayValue;
            TriviaCommands.RunningTrivias.TryRemove (server.Id,out throwAwayValue);
        }

        public async Task StopGame()
        {
            if (!ShouldStopGame)
                await channel.SendMessage(":exclamation: Quiz endet nach dieser Frage.").ConfigureAwait (false);
            ShouldStopGame = true;
        }

        private async void PotentialGuess(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.Channel.IsPrivate) return;
                if (e.Server != server) return;
                if (e.User.Id == MidnightBot.Client.CurrentUser.Id) return;

                var guess = false;
                lock (_guessLock)
                {
                    if (GameActive && CurrentQuestion.IsAnswerCorrect(e.Message.Text) && !triviaCancelSource.IsCancellationRequested)
                    {
                        Users.TryAdd(e.User, 0); //add if not exists
                        Users[e.User]++; //add 1 point to the winner
                        guess = true;
                    }
                }
                if (!guess) return;
                triviaCancelSource.Cancel();
                await channel.SendMessage($"☑️ {e.User.Mention} hat es erraten! Die Antwort war: **{CurrentQuestion.Answer}**").ConfigureAwait (false);
                if (Users[e.User] != WinRequirement) return;
                ShouldStopGame = true;
                await channel.Send($":exclamation: Wir haben einen Gewinner! Es ist {e.User.Mention}.").ConfigureAwait (false);
                // add points to the winner
                await FlowersHandler.AddFlowersAsync(e.User, "Hat das Quiz gewonnen", 2).ConfigureAwait (false);
            }
            catch { }
        }

        public string GetLeaderboard()
        {
            if (Users.Count == 0)
                return "";

            var sb = new StringBuilder();
            sb.Append("**Rangliste:**\n-----------\n");

            foreach (var kvp in Users.OrderBy(kvp => kvp.Value))
            {
                sb.AppendLine($"**{kvp.Key.Name}** has {kvp.Value} points".ToString().SnPl(kvp.Value));
            }

            return sb.ToString();
        }
    }
}
