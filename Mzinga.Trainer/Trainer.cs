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
        public DateTime StartTime
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
        private DateTime? _startTime = null;

        public TrainerSettings TrainerSettings
        {
            get
            {
                return _settings;
            }
            set
            {
                if (null == value)
                {
                    throw new ArgumentNullException();
                }
                _settings = value;
            }
        }
        private TrainerSettings _settings;

        public Trainer()
        {
            TrainerSettings = new TrainerSettings();
        }

        public void Battle()
        {
            Battle(TrainerSettings.WhiteProfilePath, TrainerSettings.BlackProfilePath);
        }

        private void Battle(string whiteProfilePath, string blackProfilePath)
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

        public void BattleRoyale()
        {
            BattleRoyale(TrainerSettings.ProfilesPath, TrainerSettings.MaxDraws);
        }

        private void BattleRoyale(string path, int maxDraws)
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

            TimeSpan timeRemaining;
            double progress;

            // Run the battle royale
            foreach (Profile whiteProfile in profiles)
            {
                foreach (Profile blackProfile in profiles)
                {
                    if (whiteProfile != blackProfile)
                    {
                        BoardState roundResult = BoardState.Draw;

                        Log("Battle Royale match start.");

                        if (maxDraws == 1)
                        {
                            roundResult = Battle(whiteProfile, blackProfile);
                        }
                        else
                        {
                            int rounds = 0;
                            while (roundResult == BoardState.Draw)
                            {
                                Log("Battle Royale round {0} start.", rounds + 1);

                                roundResult = Battle(whiteProfile, blackProfile);

                                Log("Battle Royale round {0} end.", rounds + 1);

                                rounds++;

                                if (rounds >= maxDraws && roundResult == BoardState.Draw)
                                {
                                    Log("Battle Royale match draw-out.");
                                    break;
                                }
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

                        GetProgress(brStart, completed, remaining, out progress, out timeRemaining);
                        Log("Battle Royale progress: {0:P2} ETA {1}.", progress, timeRemaining);
                    }
                }
            }

            Log("Battle Royale end.");

            Profile best = (profiles.OrderByDescending(profile => profile.EloRating)).First();

            Log("Battle Royale highest ELO: {0} ({1})", best.Id, best.EloRating);

        }
 
        private BoardState Battle(Profile whiteProfile, Profile blackProfile)
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
            whiteAI.MaxDepth = TrainerSettings.MaxDepth;
            whiteAI.MaxTime = TrainerSettings.TurnMaxTime;

            whiteAI.AlphaBetaPruning = TrainerSettings.UseAlphaBetaPruning;
            whiteAI.TranspositionTable = TrainerSettings.UseTranspositionTable;

            GameAI blackAI = new GameAI(blackProfile.MetricWeights);
            blackAI.MaxDepth = TrainerSettings.MaxDepth;
            blackAI.MaxTime = TrainerSettings.TurnMaxTime;

            blackAI.AlphaBetaPruning = TrainerSettings.UseAlphaBetaPruning;
            blackAI.TranspositionTable = TrainerSettings.UseTranspositionTable;

            TimeSpan timeLimit = TrainerSettings.BattleTimeLimit;

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

        public void Cull()
        {
            Cull(TrainerSettings.ProfilesPath, TrainerSettings.CullKeepCount);
        }

        private void Cull(string path, int keepCount)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            if (keepCount < TrainerSettings.CullMinKeepCount && keepCount != TrainerSettings.CullKeepMax)
            {
                throw new ArgumentOutOfRangeException("keepCount");
            }

            StartTime = DateTime.Now;
            Log("Cull start.");

            List<Profile> profiles = LoadProfiles(path);
            profiles = new List<Profile>(profiles.OrderByDescending(profile => profile.EloRating));

            if (keepCount == TrainerSettings.CullKeepMax)
            {
                keepCount = Math.Max(TrainerSettings.CullMinKeepCount, (int)Math.Round(Math.Sqrt(profiles.Count)));
            }

            for (int i = keepCount; i < profiles.Count; i++)
            {                
                File.Delete(Path.Combine(path, profiles[i].Id + ".xml"));
                Log("Culled {0}.", profiles[i].Nickname);
            }

            Log("Cull end.");
        }

        public void Enumerate()
        {
            Enumerate(TrainerSettings.ProfilesPath);
        }

        private void Enumerate(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            StartTime = DateTime.Now;
            Log("Enumerate start.");

            List<Profile> profiles = LoadProfiles(path);
            profiles = new List<Profile>(profiles.OrderByDescending(profile => profile.EloRating));

            foreach (Profile p in profiles)
            {
                Log("{0}, Elo: {1}, CDate: {2}, UDate: {3}", p.Id, p.EloRating, p.CreationTimestamp, p.LastUpdatedTimestamp);
            }

            Log("Enumerate end.");
        }

        public void Generate()
        {
            Generate(TrainerSettings.ProfilesPath, TrainerSettings.GenerateCount, TrainerSettings.GenerateMinWeight, TrainerSettings.GenerateMaxWeight);
        }

        private void Generate(string path, int count, double minWeight, double maxWeight)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            if (count < 1)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            StartTime = DateTime.Now;
            Log("Generate start.");

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

            Log("Generate end.");
        }
        
        public void Lifecycle()
        {
            Lifecycle(TrainerSettings.ProfilesPath, TrainerSettings.LifecycleGenerations, TrainerSettings.LifecycleBattles);
        }

        private void Lifecycle(string path, int generations, int battles)
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
                            Tournament(path, TrainerSettings.MaxDraws);
                        }
                        else if (battles > 0)
                        {
                            BattleRoyale(path, TrainerSettings.MaxDraws);
                        }
                    }
                }

                // Cull
                Cull(path, TrainerSettings.CullKeepCount);

                // Mate
                Mate(path, TrainerSettings.MateMix, TrainerSettings.MateParentCount);

                Log("Lifecycle generation {0} end.", i + 1);
            }

            Log("Lifecycle end.");
        }

        public void Mate()
        {
            Mate(TrainerSettings.ProfilesPath, TrainerSettings.MateMix, TrainerSettings.MateParentCount);
        }

        private void Mate(string path, double mix, int parentCount)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            if (mix < 0.0 || mix > 1.0)
            {
                throw new ArgumentOutOfRangeException("mix");
            }

            if (parentCount < TrainerSettings.MateMinParentCount && parentCount != TrainerSettings.MateParentMax)
            {
                throw new ArgumentOutOfRangeException("parentCount");
            }

            StartTime = DateTime.Now;
            Log("Mate start.");

            List<Profile> profiles = LoadProfiles(path);

            if (parentCount == TrainerSettings.MateParentMax)
            {
                parentCount = profiles.Count;
            }

            if (profiles.Count > parentCount)
            {
                profiles = new List<Profile>(profiles.OrderByDescending(profile => profile.EloRating).Take(parentCount));
            }

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

        public void Tournament()
        {
            Tournament(TrainerSettings.ProfilesPath, TrainerSettings.MaxDraws);
        }

        private void Tournament(string path, int maxDraws)
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

            TimeSpan timeRemaining;
            double progress;

            while (remaining.Count > 1)
            {
                Profile whiteProfile = remaining.Dequeue();
                Profile blackProfile = remaining.Dequeue();

                BoardState roundResult = BoardState.Draw;

                Log("Tournament match start.");


                if (maxDraws == 1)
                {
                    roundResult = Battle(whiteProfile, blackProfile);
                }
                else
                {
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

                GetProgress(tournamentStart, profiles.Count - remaining.Count, remaining.Count, out progress, out timeRemaining);
                Log("Tournament progress: {0:P2} ETA {1}.", progress, timeRemaining);
            }

            Log("Tournament end.");

            Profile best = remaining.Dequeue();

            Log("Tournament Winner: {0} ({1})", best.Id, best.EloRating);
        }

        private List<Profile> LoadProfiles(string path)
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

        private void Log(string format, params object[] args)
        {
            TimeSpan elapsedTime = DateTime.Now - StartTime;
            Console.WriteLine(String.Format("{0} > {1}", elapsedTime, String.Format(format, args)));
        }

        private void GetProgress(DateTime startTime, int completed, int remaining, out double progress, out TimeSpan timeRemaining)
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
