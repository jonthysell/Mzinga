// 
// Trainer.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017 Jon Thysell <http://jonthysell.com>
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
using System.Threading;
using System.Threading.Tasks;

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

        public Random Random
        {
            get
            {
                return _random ?? (_random = new Random());
            }
        }
        private Random _random;

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
            if (string.IsNullOrWhiteSpace(whiteProfilePath))
            {
                throw new ArgumentNullException("whiteProfilePath");
            }

            if (string.IsNullOrWhiteSpace(blackProfilePath))
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
            BattleRoyale(TrainerSettings.ProfilesPath, TrainerSettings.MaxBattles, TrainerSettings.MaxDraws, TrainerSettings.BulkBattleTimeLimit, TrainerSettings.BattleShuffleProfiles, TrainerSettings.MaxConcurrentBattles);
        }

        private void BattleRoyale(string path, int maxBattles, int maxDraws, TimeSpan timeLimit, bool shuffleProfiles, int maxConcurrentBattles)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            if (maxBattles < 1 && maxBattles != TrainerSettings.MaxMaxBattles)
            {
                throw new ArgumentOutOfRangeException("maxBattles");
            }

            if (maxDraws < 1)
            {
                throw new ArgumentOutOfRangeException("maxDraws");
            }

            if (maxConcurrentBattles < 1 && maxConcurrentBattles != TrainerSettings.MaxMaxConcurrentBattles)
            {
                throw new ArgumentOutOfRangeException("maxConcurrentBattles");
            }

            StartTime = DateTime.Now;

            DateTime brStart = DateTime.Now;
            Log("Battle Royale start.");

            List<Profile> profiles = LoadProfiles(path);

            int combinations = profiles.Count * (profiles.Count - 1);

            if (maxBattles == TrainerSettings.MaxMaxBattles)
            {
                maxBattles = combinations;
            }

            maxBattles = Math.Min(maxBattles, combinations);

            int total = maxBattles;
            int completed = 0;
            int remaining = total;

            List<Profile> whiteProfiles = new List<Profile>(profiles.OrderByDescending(profile => profile.EloRating));
            List<Profile> blackProfiles = new List<Profile>(profiles.OrderBy(profile => profile.EloRating));

            List<Tuple<Profile, Profile>> matches = new List<Tuple<Profile, Profile>>(combinations);

            foreach (Profile whiteProfile in whiteProfiles)
            {
                foreach (Profile blackProfile in blackProfiles)
                {
                    if (whiteProfile != blackProfile)
                    {
                        matches.Add(new Tuple<Profile, Profile>(whiteProfile, blackProfile));
                    }
                }
            }

            if (shuffleProfiles)
            {
                matches = Shuffle(matches);
            }

            matches = new List<Tuple<Profile, Profile>>(matches.Take(remaining));

            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = (maxConcurrentBattles == TrainerSettings.MaxMaxConcurrentBattles) ? Environment.ProcessorCount : maxConcurrentBattles;

            Parallel.ForEach(matches, po, (match, loopState) =>
            {
                Profile whiteProfile = match.Item1;
                Profile blackProfile = match.Item2;

                BoardState roundResult = BoardState.Draw;

                Log("Battle Royale match start {0} vs. {1}.", ToString(whiteProfile), ToString(blackProfile));

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

                Log("Battle Royale match end {0} vs. {1}.", ToString(whiteProfile), ToString(blackProfile));

                Interlocked.Increment(ref completed);
                Interlocked.Decrement(ref remaining);

                // Save Profiles
                lock (whiteProfile)
                {
                    string whiteProfilePath = Path.Combine(path, whiteProfile.Id + ".xml");
                    using (FileStream fs = new FileStream(whiteProfilePath, FileMode.Create))
                    {
                        whiteProfile.WriteXml(fs);
                    }
                }

                lock (blackProfile)
                {
                    string blackProfilePath = Path.Combine(path, blackProfile.Id + ".xml");
                    using (FileStream fs = new FileStream(blackProfilePath, FileMode.Create))
                    {
                        blackProfile.WriteXml(fs);
                    }
                }

                TimeSpan timeRemaining;
                double progress;

                TimeSpan timeoutRemaining = timeLimit - (DateTime.Now - brStart);

                GetProgress(brStart, completed, remaining, out progress, out timeRemaining);
                Log("Battle Royale progress: {0:P2} ETA {1}.", progress, timeoutRemaining < timeRemaining ? ToString(timeoutRemaining) : ToString(timeRemaining));

                if (timeoutRemaining <= TimeSpan.Zero)
                {
                    loopState.Stop();
                }
            });

            if ((timeLimit - (DateTime.Now - brStart)) <= TimeSpan.Zero)
            {
                Log("Battle Royale time-out.");
            }

            Log("Battle Royale end.");

            Profile best = (profiles.OrderByDescending(profile => profile.EloRating)).First();

            Log("Battle Royale Highest Elo: {0}", ToString(best));

        }

        private object _eloLock = new object();

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

            Log("Battle start {0} vs. {1}.", ToString(whiteProfile), ToString(blackProfile));

            DateTime battleStart = DateTime.Now;
            TimeSpan battleElapsed = TimeSpan.Zero;

            List<string> boardKeys = new List<string>();

            // Play Game
            while (gameBoard.GameInProgress)
            {
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

                Move move = gameBoard.CurrentTurnColor == Color.White ? whiteAI.GetBestMove(gameBoard) : blackAI.GetBestMove(gameBoard);
                gameBoard.Play(move);
            }

            BoardState boardState = gameBoard.GameInProgress ? BoardState.Draw : gameBoard.BoardState;

            // Load Results
            double whiteScore = 0.0;
            double blackScore = 0.0;

            GameResult whiteResult = GameResult.Loss;
            GameResult blackResult = GameResult.Loss;

            switch (boardState)
            {
                case BoardState.WhiteWins:
                    whiteScore = 1.0;
                    blackScore = 0.0;
                    whiteResult = GameResult.Win;
                    break;
                case BoardState.BlackWins:
                    whiteScore = 0.0;
                    blackScore = 1.0;
                    blackResult = GameResult.Win;
                    break;
                case BoardState.Draw:
                    whiteScore = 0.5;
                    blackScore = 0.5;
                    whiteResult = GameResult.Draw;
                    blackResult = GameResult.Draw;
                    break;
            }

            int whiteRating;
            int blackRating;

            lock (_eloLock)
            {
                EloUtils.UpdateRatings(whiteProfile.EloRating, blackProfile.EloRating, whiteScore, blackScore, out whiteRating, out blackRating);
            }

            lock (whiteProfile)
            {
                whiteProfile.UpdateRecord(whiteRating, whiteResult);
            }

            lock (blackProfile)
            {
                blackProfile.UpdateRecord(blackRating, blackResult);
            }

            // Output Results
            Log("Battle end {0} {1} vs. {2}", boardState, ToString(whiteProfile), ToString(blackProfile));

            return boardState;
        }

        public void Cull()
        {
            Cull(TrainerSettings.ProfilesPath, TrainerSettings.CullKeepCount, TrainerSettings.ProvisionalRules);
        }

        private void Cull(string path, int keepCount, bool provisionalRules)
        {
            if (string.IsNullOrWhiteSpace(path))
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

            if (provisionalRules)
            {
                profiles = new List<Profile>(profiles.Where(profile => !IsProvisional(profile)));
            }

            profiles = new List<Profile>(profiles.OrderByDescending(profile => profile.EloRating));

            if (keepCount == TrainerSettings.CullKeepMax)
            {
                keepCount = Math.Max(TrainerSettings.CullMinKeepCount, (int)Math.Round(Math.Sqrt(profiles.Count)));
            }

            if (!Directory.Exists(Path.Combine(path, "culled")))
            {
                Directory.CreateDirectory(Path.Combine(path, "culled"));
            }

            int count = 0;
            foreach (Profile p in profiles)
            {
                if (count < keepCount)
                {
                    Log("Kept {0}.", ToString(p));
                    count++;
                }
                else
                {
                    string sourceFile = Path.Combine(path, p.Id + ".xml");
                    string destFile = Path.Combine(path, "culled", p.Id + ".xml");

                    File.Move(sourceFile, destFile);
                    Log("Culled {0}.", ToString(p));
                }
            }

            Log("Cull end.");
        }

        public void Enumerate()
        {
            Enumerate(TrainerSettings.ProfilesPath);
        }

        private void Enumerate(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            StartTime = DateTime.Now;
            Log("Enumerate start.");

            List<Profile> profiles = LoadProfiles(path);
            profiles = new List<Profile>(profiles.OrderByDescending(profile => profile.EloRating));

            foreach (Profile p in profiles)
            {
                Log("{0}", ToString(p));
            }

            Log("Enumerate end.");
        }

        public void Generate()
        {
            Generate(TrainerSettings.ProfilesPath, TrainerSettings.GenerateCount, TrainerSettings.GenerateMinWeight, TrainerSettings.GenerateMaxWeight);
        }

        private void Generate(string path, int count, double minWeight, double maxWeight)
        {
            if (string.IsNullOrWhiteSpace(path))
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

            for (int i = 0; i < count; i++)
            {
                Profile profile = Profile.Generate(minWeight, maxWeight);

                string filename = Path.Combine(path, profile.Id + ".xml");
                using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    profile.WriteXml(fs);
                }

                Log("Generated {0}.", ToString(profile));
            }

            Log("Generate end.");
        }
        
        public void Lifecycle()
        {
            Lifecycle(TrainerSettings.ProfilesPath, TrainerSettings.LifecycleGenerations, TrainerSettings.LifecycleBattles);
        }

        private void Lifecycle(string path, int generations, int battles)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            if (generations == 0)
            {
                throw new ArgumentOutOfRangeException("generations");
            }

            StartTime = DateTime.Now;

            DateTime lifecycleStart = DateTime.Now;
            Log("Lifecycle start.");

            TimeSpan timeRemaining;
            double progress;

            int gen = 1;
            while (generations == TrainerSettings.InfiniteLifeCycleGenerations || gen <= generations)
            {
                if (generations != 1)
                {
                    Log("Lifecycle generation {0} start.", gen);
                }

                // Battle
                if (battles != 0)
                {
                    for (int j = 0; j < Math.Abs(battles); j++)
                    {
                        if (battles < 0)
                        {
                            Tournament(path, TrainerSettings.MaxDraws, TrainerSettings.BulkBattleTimeLimit, TrainerSettings.BattleShuffleProfiles, TrainerSettings.MaxConcurrentBattles);
                        }
                        else if (battles > 0)
                        {
                            BattleRoyale(path, TrainerSettings.MaxBattles, TrainerSettings.MaxDraws, TrainerSettings.BulkBattleTimeLimit, TrainerSettings.BattleShuffleProfiles, TrainerSettings.MaxConcurrentBattles);
                        }
                    }
                }

                // Cull
                Cull(path, TrainerSettings.CullKeepCount, TrainerSettings.ProvisionalRules);

                // Mate
                Mate(path);

                if (generations != 1)
                {
                    Log("Lifecycle generation {0} end.", gen);
                }

                if (generations > 0)
                {
                    GetProgress(lifecycleStart, gen, generations - gen, out progress, out timeRemaining);
                    Log("Lifecycle progress: {0:P2} ETA {1}.", progress, ToString(timeRemaining));
                }

                gen++;
            }

            Log("Lifecycle end.");
        }

        public void Mate()
        {
            Mate(TrainerSettings.ProfilesPath);
        }

        private void Mate(string path)
        {
            Mate(path, TrainerSettings.MateMinMix, TrainerSettings.MateMaxMix, TrainerSettings.MateParentCount, TrainerSettings.MateShuffleParents, TrainerSettings.ProvisionalRules);
        }

        private void Mate(string path, double minMix, double maxMix, int parentCount, bool shuffleParents, bool provisionalRules)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            if (minMix > maxMix)
            {
                throw new ArgumentOutOfRangeException("minMix");
            }

            if (parentCount < TrainerSettings.MateMinParentCount && parentCount != TrainerSettings.MateParentMax)
            {
                throw new ArgumentOutOfRangeException("parentCount");
            }

            StartTime = DateTime.Now;
            Log("Mate start.");

            List<Profile> profiles = LoadProfiles(path);

            if (provisionalRules)
            {
                profiles = new List<Profile>(profiles.Where(profile => !IsProvisional(profile)));
            }

            profiles = shuffleParents ? Shuffle(profiles) : Seed(profiles);

            int maxParents = profiles.Count - (profiles.Count % 2);

            if (parentCount == TrainerSettings.MateParentMax)
            {
                parentCount = maxParents;
            }

            parentCount = Math.Min(parentCount, maxParents); // No more parents that exist

            if (parentCount >= TrainerSettings.MateMinParentCount)
            {
                Queue<Profile> parents = new Queue<Profile>(profiles.Take(parentCount));

                while (parents.Count >= 2)
                {
                    Profile parentA = parents.Dequeue();
                    Profile parentB = parents.Dequeue();

                    Profile child = Profile.Mate(parentA, parentB, minMix, maxMix);

                    Log("Mated {0} and {1} to sire {2}.", ToString(parentA), ToString(parentB), ToString(child));

                    using (FileStream fs = new FileStream(Path.Combine(path, child.Id + ".xml"), FileMode.Create))
                    {
                        child.WriteXml(fs);
                    }
                }
            }

            Log("Mate end.");
        }

        public void Tournament()
        {
            Tournament(TrainerSettings.ProfilesPath, TrainerSettings.MaxDraws, TrainerSettings.BulkBattleTimeLimit, TrainerSettings.BattleShuffleProfiles, TrainerSettings.MaxConcurrentBattles);
        }

        private void Tournament(string path, int maxDraws, TimeSpan timeLimit,  bool shuffleProfiles, int maxConcurrentBattles)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            if (maxDraws < 1)
            {
                throw new ArgumentOutOfRangeException("maxDraws");
            }

            if (maxConcurrentBattles < 1 && maxConcurrentBattles != TrainerSettings.MaxMaxConcurrentBattles)
            {
                throw new ArgumentOutOfRangeException("maxConcurrentBattles");
            }

            StartTime = DateTime.Now;

            DateTime tournamentStart = DateTime.Now;
            Log("Tournament start.");

            List<Profile> profiles = LoadProfiles(path);

            int total = profiles.Count - 1;
            int completed = 0;
            int remaining = total;

            Profile[] currentTier = new Profile[profiles.Count];
            (shuffleProfiles ? Shuffle(profiles) : Seed(profiles)).CopyTo(currentTier);

            int tier = 1;

            while (currentTier.Length > 1)
            {
                Log("Tournament tier {0} start, {1} participants.", tier, currentTier.Length);

                Profile[] winners = new Profile[(int)Math.Round(currentTier.Length / 2.0)];

                ParallelOptions po = new ParallelOptions();
                po.MaxDegreeOfParallelism = (maxConcurrentBattles == TrainerSettings.MaxMaxConcurrentBattles) ? Environment.ProcessorCount : maxConcurrentBattles;

                Parallel.For(0, winners.Length, po, (i, loopState) =>
                {
                    int profileIndex = i * 2;

                    if (profileIndex == currentTier.Length - 1)
                    {
                        // Odd profile out, gimme
                        Log("Tournament auto-advances {0}.", ToString(currentTier[profileIndex]));
                        winners[i] = currentTier[profileIndex];
                    }
                    else
                    {
                        Profile whiteProfile = currentTier[profileIndex];
                        Profile blackProfile = currentTier[profileIndex + 1];

                        BoardState roundResult = BoardState.Draw;

                        Profile drawWinnerProfile = whiteProfile.EloRating < blackProfile.EloRating ? whiteProfile : blackProfile;

                        Log("Tournament match start {0} vs. {1}.", ToString(whiteProfile), ToString(blackProfile));

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
                                    Log("Tournament match draw-out.");
                                    break;
                                }
                            }
                        }

                        if (roundResult == BoardState.Draw)
                        {
                            roundResult = (drawWinnerProfile == whiteProfile) ? BoardState.WhiteWins : BoardState.BlackWins;
                        }

                        Log("Tournament match end {0} vs. {1}.", ToString(whiteProfile), ToString(blackProfile));

                        Interlocked.Increment(ref completed);
                        Interlocked.Decrement(ref remaining);

                        // Add winner back into the participant queue
                        if (roundResult == BoardState.WhiteWins)
                        {
                            winners[i] = whiteProfile;
                        }
                        else if (roundResult == BoardState.BlackWins)
                        {
                            winners[i] = blackProfile;
                        }

                        Log("Tournament advances {0}.", ToString(winners[i]));

                        // Save Profiles
                        lock (whiteProfile)
                        {
                            string whiteProfilePath = Path.Combine(path, whiteProfile.Id + ".xml");
                            using (FileStream fs = new FileStream(whiteProfilePath, FileMode.Create))
                            {
                                whiteProfile.WriteXml(fs);
                            }
                        }

                        lock (blackProfile)
                        {
                            string blackProfilePath = Path.Combine(path, blackProfile.Id + ".xml");
                            using (FileStream fs = new FileStream(blackProfilePath, FileMode.Create))
                            {
                                blackProfile.WriteXml(fs);
                            }
                        }

                        TimeSpan timeRemaining;
                        double progress;

                        TimeSpan timeoutRemaining = timeLimit - (DateTime.Now - tournamentStart);

                        GetProgress(tournamentStart, completed, remaining, out progress, out timeRemaining);
                        Log("Tournament progress: {0:P2} ETA {1}.", progress, timeoutRemaining < timeRemaining ? ToString(timeoutRemaining) : ToString(timeRemaining));

                        if (timeoutRemaining <= TimeSpan.Zero)
                        {
                            loopState.Stop();
                        }
                    }
                });

                Log("Tournament tier {0} end.", tier);
                tier++;

                currentTier = winners;

                if ((timeLimit - (DateTime.Now - tournamentStart)) <= TimeSpan.Zero)
                {
                    Log("Tournament time-out.");
                    break;
                }
            }

            Log("Tournament end.");

            if (currentTier.Length == 1 && null != currentTier[0])
            {
                Profile winner = currentTier[0];
                Log("Tournament Winner: {0}", ToString(winner));
            }

            Profile best = (profiles.OrderByDescending(profile => profile.EloRating)).First();
            Log("Tournament Highest Elo: {0}", ToString(best));
        }

        private List<Profile> LoadProfiles(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            List<Profile> profiles = new List<Profile>();
            
            Parallel.ForEach(Directory.EnumerateFiles(path, "*.xml"), (profilePath) =>
            {
                using (FileStream fs = new FileStream(profilePath, FileMode.Open))
                {
                    Profile profile = Profile.ReadXml(fs);
                    lock (profiles)
                    {
                        profiles.Add(profile);
                    }
                }
            }
            );

            return profiles;
        }

        private List<T> Shuffle<T>(List<T> items)
        {
            if (null == items)
            {
                throw new ArgumentNullException("items");
            }

            List<T> unshuffled = new List<T>(items);

            List<T> shuffled = new List<T>(items.Count);

            while (unshuffled.Count > 0)
            {
                int randIndex = Random.Next(unshuffled.Count);
                T t = unshuffled[randIndex];
                unshuffled.RemoveAt(randIndex);
                shuffled.Add(t);
            }

            return shuffled;
        }

        private List<Profile> Seed(List<Profile> profiles)
        {
            if (null == profiles)
            {
                throw new ArgumentNullException("profiles");
            }

            LinkedList<Profile> sortedProfiles = new LinkedList<Profile>(profiles.OrderByDescending(profile => profile.EloRating));
            List<Profile> seeded = new List<Profile>(sortedProfiles.Count);

            bool first = true;
            while (sortedProfiles.Count > 0)
            {
                if (first)
                {
                    seeded.Add(sortedProfiles.First.Value);
                    sortedProfiles.RemoveFirst();
                }
                else
                {
                    seeded.Add(sortedProfiles.Last.Value);
                    sortedProfiles.RemoveLast();
                }

                first = !first;
            }

            return seeded;
        }

        private void Log(string format, params object[] args)
        {
            TimeSpan elapsedTime = DateTime.Now - StartTime;
            Console.WriteLine(string.Format("{0} > {1}", ToString(elapsedTime), string.Format(format, args)));
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

            double total = completed + remaining;

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
                double avgMs = elapsedMs / completed;

                progress = completed / total;
                timeRemaining = TimeSpan.FromMilliseconds(avgMs * remaining);
            }
        }

        private string ToString(TimeSpan ts)
        {
            return ts.Days.ToString() + "." + ts.ToString(@"hh\:mm\:ss");
        }

        private string ToString(Profile profile)
        {
            if (null == profile)
            {
                throw new ArgumentNullException("profile");
            }

            return string.Format("{0}({1}{2} {3}/{4}/{5})", profile.Id.ToString().Substring(0, 8), profile.EloRating, IsProvisional(profile) ? "?" : " ", profile.Wins, profile.Losses, profile.Draws);
        }

        private bool IsProvisional(Profile profile)
        {
            if (null == profile)
            {
                throw new ArgumentNullException("profile");
            }

            return profile.TotalGames < TrainerSettings.ProvisionalGameCount;
        }
    }

    public enum GameResult
    {
        Loss,
        Draw,
        Win
    }
}
