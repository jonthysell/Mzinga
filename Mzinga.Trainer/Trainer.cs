// 
// Trainer.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016 Jon Thysell <http://jonthysell.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mzinga.Core;
using Mzinga.Core.AI;

namespace Mzinga.Trainer
{
    public class Trainer
    {

        public static DateTime StartTime
        {
            get
            {
                return _startTime.Value;
            }
            private set
            {
                if (!_startTime.HasValue)
                {
                    _startTime = value;
                }
            }
        }
        private static DateTime? _startTime = null;

        public static void Generate(int count, double minWeight, double maxWeight, string path)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            Directory.CreateDirectory(path);

            List<Profile> profiles = Profile.Generate(count, minWeight, maxWeight);

            foreach (Profile profile in profiles)
            {
                string filename = Path.Combine(path, profile.Id + ".xml");
                using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    profile.WriteXml(fs);
                }
                Log("Generated {0}.", profile.Nickname);
            }
        }

        public static void Tournament(string path, int maxDraws)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            if (maxDraws < 1)
            {
                throw new ArgumentOutOfRangeException("maxDraws");
            }

            StartTime = DateTime.Now;

            DateTime tournamentStart = DateTime.Now;
            Log("Tournament start.");

            List<Profile> profiles = LoadProfiles(path);
            Queue<Profile> remaining = new Queue<Profile>(profiles);

            while (remaining.Count > 1)
            {
                Profile whiteProfile = remaining.Dequeue();
                Profile blackProfile = remaining.Dequeue();

                BoardState roundResult = BoardState.Draw;

                Log("Tournament match start.");

                int rounds = 0;
                while (roundResult == BoardState.Draw)
                {
                    Log("Tournament round {0} start.", rounds + 1);

                    roundResult = Battle(whiteProfile, blackProfile);

                    Log("Tournament round {0} end.", rounds + 1);

                    rounds++;

                    if (rounds >= maxDraws && roundResult == BoardState.Draw)
                    {
                        roundResult = whiteProfile.EloRating >= blackProfile.EloRating ? BoardState.WhiteWins : BoardState.BlackWins;
                        Log("Tournament match draw-out.");
                    }
                }

                Log("Tournament match end, {0}.", roundResult);

                if (roundResult == BoardState.WhiteWins)
                {
                    remaining.Enqueue(whiteProfile);
                }
                else if (roundResult == BoardState.BlackWins)
                {
                    remaining.Enqueue(blackProfile);
                }

                // Save Profiles
                string whiteProfilePath = Path.Combine(path, whiteProfile.Id + ".xml");
                using (FileStream fs = new FileStream(whiteProfilePath, FileMode.Create))
                {
                    whiteProfile.WriteXml(fs);
                }

                string blackProfilePath = Path.Combine(path, blackProfile.Id + ".xml");
                using (FileStream fs = new FileStream(blackProfilePath, FileMode.Create))
                {
                    blackProfile.WriteXml(fs);
                }

                TimeSpan timeRemaining;
                double progress;
                GetProgress(tournamentStart, profiles.Count - remaining.Count, remaining.Count, out progress, out timeRemaining);
                Log("Tournament progress: {0:P2} ETA {1}.", progress, timeRemaining);
            }

            Log("Tournament end.");

            Profile best = remaining.Dequeue();

            Log("Tournament Winner: {0} ({1})", best.Id, best.EloRating);
        }

        public static void BattleRoyale(string path, int maxDraws)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            if (maxDraws < 1)
            {
                throw new ArgumentOutOfRangeException("maxDraws");
            }

            StartTime = DateTime.Now;

            DateTime brStart = DateTime.Now;
            Log("Battle Royale start.");

            List<Profile> profiles = LoadProfiles(path);

            int total = profiles.Count * (profiles.Count - 1);
            int completed = 0;
            int remaining = total;

            // Run the battle royale
            foreach (Profile whiteProfile in profiles)
            {
                foreach (Profile blackProfile in profiles)
                {
                    if (whiteProfile != blackProfile)
                    {
                        BoardState roundResult = BoardState.Draw;

                        Log("Battle Royale match start.");

                        int rounds = 0;
                        while (roundResult == BoardState.Draw)
                        {
                            Log("Battle Royale round {0} start.", rounds + 1);

                            roundResult = Battle(whiteProfile, blackProfile);

                            Log("Battle Royale round {0} end.", rounds + 1);

                            rounds++;

                            if (rounds >= maxDraws && roundResult == BoardState.Draw)
                            {
                                roundResult = whiteProfile.EloRating >= blackProfile.EloRating ? BoardState.WhiteWins : BoardState.BlackWins;
                                Log("Battle Royale match draw-out.");
                            }
                        }

                        Log("Battle Royale match end, {0}.", roundResult);
                        completed++;
                        remaining--;

                        // Save Profiles
                        string whiteProfilePath = Path.Combine(path, whiteProfile.Id + ".xml");
                        using (FileStream fs = new FileStream(whiteProfilePath, FileMode.Create))
                        {
                            whiteProfile.WriteXml(fs);
                        }

                        string blackProfilePath = Path.Combine(path, blackProfile.Id + ".xml");
                        using (FileStream fs = new FileStream(blackProfilePath, FileMode.Create))
                        {
                            blackProfile.WriteXml(fs);
                        }

                        TimeSpan timeRemaining;
                        double progress;
                        GetProgress(brStart, completed, remaining, out progress, out timeRemaining);
                        Log("Battle Royale progress: {0:P2} ETA {1}.", progress, timeRemaining);
                    }
                }
            }

            Log("Battle Royale end.");

            Profile best = (profiles.OrderByDescending(profile => profile.EloRating)).First();

            Log("Battle Royale highest ELO: {0} ({1})", best.Id, best.EloRating);

        }

        public static void Battle(string whiteProfilePath, string blackProfilePath)
        {
            if (String.IsNullOrWhiteSpace(whiteProfilePath))
            {
                throw new ArgumentNullException("whiteProfilePath");
            }

            if (String.IsNullOrWhiteSpace(blackProfilePath))
            {
                throw new ArgumentNullException("blackProfilePath");
            }

            StartTime = DateTime.Now;

            // Load Profiles
            Profile whiteProfile;
            using (FileStream fs = new FileStream(whiteProfilePath, FileMode.Open))
            {
                whiteProfile = Profile.ReadXml(fs);
            }

            Profile blackProfile;
            using (FileStream fs = new FileStream(blackProfilePath, FileMode.Open))
            {
                blackProfile = Profile.ReadXml(fs);
            }

            Battle(whiteProfile, blackProfile);

            // Save Profiles
            using (FileStream fs = new FileStream(whiteProfilePath, FileMode.Create))
            {
                whiteProfile.WriteXml(fs);
            }

            using (FileStream fs = new FileStream(blackProfilePath, FileMode.Create))
            {
                blackProfile.WriteXml(fs);
            }
        }

        public static BoardState Battle(Profile whiteProfile, Profile blackProfile)
        {
            if (null == whiteProfile)
            {
                throw new ArgumentNullException("whiteProfile");
            }

            if (null == blackProfile)
            {
                throw new ArgumentNullException("blackProfile");
            }

            if (whiteProfile.Id == blackProfile.Id)
            {
                throw new Exception("Profile cannot battle itself.");
            }

            // Create Game
            GameBoard gameBoard = new GameBoard();

            // Create AIs
            GameAI whiteAI = new GameAI(whiteProfile.MetricWeights);
            whiteAI.MaxDepth = TrainerSettings.DefaultMaxDepth;
            whiteAI.MaxTime = TrainerSettings.DefaultMaxTime;

            whiteAI.AlphaBetaPruning = TrainerSettings.DefaultUseAlphaBetaPruning;
            whiteAI.TranspositionTable = TrainerSettings.DefaultUseTranspositionTable;

            GameAI blackAI = new GameAI(blackProfile.MetricWeights);
            blackAI.MaxDepth = TrainerSettings.DefaultMaxDepth;
            blackAI.MaxTime = TrainerSettings.DefaultMaxTime;

            blackAI.AlphaBetaPruning = TrainerSettings.DefaultUseAlphaBetaPruning;
            blackAI.TranspositionTable = TrainerSettings.DefaultUseTranspositionTable;

            TimeSpan timeLimit = TrainerSettings.DefaultBattleTimeLimit;

            Log("Battle start, {0} vs. {1}.", whiteProfile.Nickname, blackProfile.Nickname);

            DateTime battleStart = DateTime.Now;
            TimeSpan battleElapsed = TimeSpan.Zero;

            List<string> boardKeys = new List<string>();

            // Play Game
            while (gameBoard.GameInProgress)
            {
                Move move = gameBoard.CurrentTurnColor == Color.White ? whiteAI.GetBestMove(gameBoard) : blackAI.GetBestMove(gameBoard);
                gameBoard.Play(move);

                boardKeys.Add(gameBoard.GetTranspositionKey());

                if (boardKeys.Count >= 6)
                {
                    int lastIndex = boardKeys.Count - 1;
                    if (boardKeys[lastIndex] == boardKeys[lastIndex - 4] && boardKeys[lastIndex - 1] == boardKeys[lastIndex - 5])
                    {
                        Log("Battle loop-out.");
                        break;
                    }
                }

                battleElapsed = DateTime.Now - battleStart;

                if (battleElapsed > timeLimit)
                {
                    Log("Battle time-out.");
                    break;
                }
            }

            // Load Results
            double whiteScore = 0.0;
            double blackScore = 0.0;

            switch (gameBoard.BoardState)
            {
                case BoardState.WhiteWins:
                    whiteScore = 1.0;
                    blackScore = 0.0;
                    break;
                case BoardState.BlackWins:
                    whiteScore = 0.0;
                    blackScore = 1.0;
                    break;
                case BoardState.Draw:
                case BoardState.InProgress:
                    whiteScore = 0.5;
                    blackScore = 0.5;
                    break;
            }

            int whiteRating;
            int blackRating;
            EloUtils.UpdateRatings(whiteProfile.EloRating, blackProfile.EloRating, whiteScore, blackScore, out whiteRating, out blackRating);

            whiteProfile.UpdateRating(whiteRating);
            blackProfile.UpdateRating(blackRating);

            BoardState boardState = gameBoard.BoardState == BoardState.InProgress ? BoardState.Draw : gameBoard.BoardState;

            // Output Results
            Log("Battle end, {0}", boardState);
            Log("Battle end, {0} vs. {1}", whiteProfile.Nickname, blackProfile.Nickname);

            return boardState;
        }

        public static void Cull(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            StartTime = DateTime.Now;
            Log("Cull start.");

            List<Profile> profiles = LoadProfiles(path);
            profiles = new List<Profile>(profiles.OrderByDescending(profile => profile.EloRating));

            int keepCount = Math.Max(TrainerSettings.CullMinKeepCount, (int)Math.Round(Math.Sqrt(profiles.Count)));

            for (int i = keepCount; i < profiles.Count; i++)
            {                
                File.Delete(Path.Combine(path, profiles[i].Id + ".xml"));
                Log("Culled {0}.", profiles[i].Nickname);
            }

            Log("Cull end.");
        }

        public static void Mate(string path, double mix)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            if (mix < 0.0 || mix > 1.0)
            {
                throw new ArgumentOutOfRangeException("mix");
            }

            StartTime = DateTime.Now;
            Log("Mate start.");

            List<Profile> profiles = LoadProfiles(path);

            foreach (Profile parentA in profiles)
            {
                foreach (Profile parentB in profiles)
                {
                    if (parentA != parentB)
                    {
                        Profile child = Profile.Mate(parentA, parentB, mix);

                        Log("Mated {0} and {1} to sire {2}.", parentA.Nickname, parentB.Nickname, child.Nickname);

                        using (FileStream fs = new FileStream(Path.Combine(path, child.Id + ".xml"), FileMode.Create))
                        {
                            child.WriteXml(fs);
                        }
                    }
                }
            }

            Log("Mate end.");
        }

        public static void Lifecycle(string path, int generations, int battles)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            if (generations < 1)
            {
                throw new ArgumentOutOfRangeException("generations");
            }

            StartTime = DateTime.Now;
            Log("Lifecycle start.");

            for (int i = 0; i < generations; i++)
            {
                Log("Lifecycle generation {0} start.", i + 1);

                // Battle
                if (battles != 0)
                {
                    for (int j = 0; j < Math.Abs(battles); j++)
                    {
                        if (battles < 0)
                        {
                            Tournament(path, TrainerSettings.DefaultMaxDraws);
                        }
                        else if (battles > 0)
                        {
                            BattleRoyale(path, TrainerSettings.DefaultMaxDraws);
                        }
                    }
                }

                // Cull
                Cull(path);

                // Mate
                Mate(path, TrainerSettings.DefaultMix);

                Log("Lifecycle generation {0} end.", i + 1);
            }

            Log("Lifecycle end.");
        }

        private static List<Profile> LoadProfiles(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            List<Profile> profiles = new List<Profile>();

            foreach (string profilePath in Directory.EnumerateFiles(path, "*.xml"))
            {
                using (FileStream fs = new FileStream(profilePath, FileMode.Open))
                {
                    Profile profile = Profile.ReadXml(fs);
                    profiles.Add(profile);
                }
            }

            return profiles;
        }

        private static void Log(string format, params object[] args)
        {
            TimeSpan elapsedTime = DateTime.Now - StartTime;
            Console.WriteLine(String.Format("{0} > {1}", elapsedTime, String.Format(format, args)));
        }

        private static void GetProgress(DateTime startTime, int completed, int remaining, out double progress, out TimeSpan timeRemaining)
        {
            if (completed < 0)
            {
                throw new ArgumentOutOfRangeException("completed");
            }

            if (remaining < 0)
            {
                throw new ArgumentOutOfRangeException("remaining");
            }

            double total = (double)(completed + remaining);

            if (completed == 0)
            {
                progress = 0.0;
                timeRemaining = TimeSpan.MaxValue;
            }
            else if (remaining == 0)
            {
                progress = 1.0;
                timeRemaining = TimeSpan.Zero;
            }
            else
            {
                TimeSpan elapsedTime = DateTime.Now - startTime;

                double elapsedMs = elapsedTime.TotalMilliseconds;
                double avgMs = elapsedMs / (double)completed;

                progress = (double)completed / total;
                timeRemaining = TimeSpan.FromMilliseconds(avgMs * (double)remaining);
            }
        }
    }
}
