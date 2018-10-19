// 
// Trainer.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017, 2018 Jon Thysell <http://jonthysell.com>
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
using System.Text;
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
                _settings = value ?? throw new ArgumentNullException();
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

            TimeSpan timeRemaining = TimeSpan.FromSeconds(TrainerSettings.BattleTimeLimit.TotalSeconds * total);
            double progress = 0.0;

            TimeSpan timeoutRemaining = timeLimit - (DateTime.Now - brStart);

            Log("Battle Royale start, ETA: {0}.", timeoutRemaining < timeRemaining ? ToString(timeoutRemaining) : ToString(timeRemaining));

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

            ParallelOptions po = new ParallelOptions
            {
                MaxDegreeOfParallelism = (maxConcurrentBattles == TrainerSettings.MaxMaxConcurrentBattles) ? Environment.ProcessorCount : maxConcurrentBattles
            };

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

                lock (_progressLock)
                {
                    timeoutRemaining = timeLimit - (DateTime.Now - brStart);

                    GetProgress(brStart, completed, remaining, out progress, out timeRemaining);
                    Log("Battle Royale progress: {0:P2}, ETA: {1}.", progress, timeoutRemaining < timeRemaining ? ToString(timeoutRemaining) : ToString(timeRemaining));
                }

                if (timeoutRemaining <= TimeSpan.Zero)
                {
                    loopState.Stop();
                }
            });

            if ((timeLimit - (DateTime.Now - brStart)) <= TimeSpan.Zero)
            {
                Log("Battle Royale time-out.");
            }

            Log("Battle Royale end, elapsed time: {0}.", ToString(DateTime.Now - brStart));

            Profile best = (profiles.OrderByDescending(profile => profile.EloRating)).First();

            Log("Battle Royale Highest Elo: {0}", ToString(best));

        }

        private readonly object _eloLock = new object();

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
            GameBoard gameBoard = new GameBoard(TrainerSettings.GameType);

            // Create AIs
            GameAI whiteAI = new GameAI(new GameAIConfig()
            {
                StartMetricWeights = whiteProfile.StartMetricWeights,
                EndMetricWeights = whiteProfile.EndMetricWeights,
                TranspositionTableSizeMB = TrainerSettings.TransTableSize,
            });

            if (TrainerSettings.FindPuzzleCandidates)
            {
                whiteAI.BestMoveFound += GetPuzzleCandidateHandler(gameBoard);
            }

            GameAI blackAI = new GameAI(new GameAIConfig()
            {
                StartMetricWeights = blackProfile.StartMetricWeights,
                EndMetricWeights = blackProfile.EndMetricWeights,
                TranspositionTableSizeMB = TrainerSettings.TransTableSize,
            });

            if (TrainerSettings.FindPuzzleCandidates)
            {
                blackAI.BestMoveFound += GetPuzzleCandidateHandler(gameBoard);
            }

            TimeSpan timeLimit = TrainerSettings.BattleTimeLimit;

            Log("Battle start {0} vs. {1}.", ToString(whiteProfile), ToString(blackProfile));

            DateTime battleStart = DateTime.Now;
            TimeSpan battleElapsed = TimeSpan.Zero;

            List<ulong> boardKeys = new List<ulong>();

            try
            {
                // Play Game
                while (gameBoard.GameInProgress)
                {
                    boardKeys.Add(gameBoard.ZobristKey);

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

                    Move move = GetBestMove(gameBoard, gameBoard.CurrentTurnColor == PlayerColor.White ? whiteAI : blackAI);
                    gameBoard.Play(move);
                }
            }
            catch (Exception ex)
            {
                Log("Battle interrupted with exception: {0}", ex.Message);
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
            double whiteK;

            lock (whiteProfile)
            {
                whiteRating = whiteProfile.EloRating;
                whiteK = IsProvisional(whiteProfile) ? EloUtils.ProvisionalK : EloUtils.DefaultK;
            }

            int blackRating;
            double blackK;

            lock (blackProfile)
            {
                blackRating = blackProfile.EloRating;
                blackK = IsProvisional(blackProfile) ? EloUtils.ProvisionalK : EloUtils.DefaultK;
            }

            int whiteEndRating;
            int blackEndRating;

            lock (_eloLock)
            {
                EloUtils.UpdateRatings(whiteRating, blackRating, whiteScore, blackScore, whiteK, blackK, out whiteEndRating, out blackEndRating);
            }

            lock (whiteProfile)
            {
                whiteProfile.UpdateRecord(whiteEndRating, whiteResult);
            }

            lock (blackProfile)
            {
                blackProfile.UpdateRecord(blackEndRating, blackResult);
            }

            // Output Results
            Log("Battle end {0} {1} vs. {2}", boardState, ToString(whiteProfile), ToString(blackProfile));

            return boardState;
        }

        private Move GetBestMove(GameBoard gameBoard, GameAI ai)
        {
            if (TrainerSettings.MaxDepth >= 0)
            {
                return ai.GetBestMove(gameBoard, TrainerSettings.MaxDepth, TrainerSettings.MaxHelperThreads);
            }

            return ai.GetBestMove(gameBoard, TrainerSettings.TurnMaxTime, TrainerSettings.MaxHelperThreads);
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

                string profilePath = Path.Combine(path, p.Id + ".xml");
                using (FileStream fs = new FileStream(profilePath, FileMode.Create))
                {
                    p.WriteXml(fs);
                }
            }

            Log("Enumerate end.");
        }

        public void Analyze()
        {
            Analyze(TrainerSettings.ProfilesPath);
        }

        public void Analyze(string path)
        {
            StartTime = DateTime.Now;
            Log("Analyze start.");

            List<Profile> profiles = LoadProfiles(path);
            profiles = new List<Profile>(profiles.OrderByDescending(profile => profile.EloRating));

            string resultFile = Path.Combine(path, "analyze.csv");

            using (StreamWriter sw = new StreamWriter(resultFile))
            {
                // Header
                StringBuilder headerSB = new StringBuilder();

                headerSB.Append("Id,Name,EloRating,Generation,ParentA,ParentB,Wins,Losses,Draws");

                MetricWeights.IterateOverWeights((bugType, bugTypeWeight) =>
                {
                    headerSB.AppendFormat(",Start{0}.{1}", bugType, bugTypeWeight);
                    headerSB.AppendFormat(",End{0}.{1}", bugType, bugTypeWeight);
                });

                sw.WriteLine(headerSB.ToString());

                foreach (Profile p in profiles)
                {
                    StringBuilder profileSB = new StringBuilder();

                    profileSB.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8}", p.Id, p.Name, p.EloRating, p.Generation, p.ParentA.HasValue ? p.ParentA.ToString() : "", p.ParentB.HasValue ? p.ParentB.ToString() : "", p.Wins, p.Losses, p.Draws);

                    MetricWeights startNormalized = p.StartMetricWeights.GetNormalized();
                    MetricWeights endNormalized = p.EndMetricWeights.GetNormalized();

                    MetricWeights.IterateOverWeights((bugType, bugTypeWeight) =>
                    {
                        profileSB.AppendFormat(",{0:0.00}", startNormalized.Get(bugType, bugTypeWeight));
                        profileSB.AppendFormat(",{0:0.00}", endNormalized.Get(bugType, bugTypeWeight));
                    });

                    sw.WriteLine(profileSB.ToString());
                }
            }

            Log("Analyze end.");
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
                    GetProgress(lifecycleStart, gen, generations - gen, out double progress, out TimeSpan timeRemaining);
                    Log("Lifecycle progress: {0:P2} ETA {1}.", progress, ToString(timeRemaining));
                }

                // Output analysis
                Analyze(path);

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

            List<Profile> profiles = LoadProfiles(path);

            int total = profiles.Count - 1;
            int completed = 0;
            int remaining = total;

            TimeSpan timeRemaining = TimeSpan.FromSeconds(TrainerSettings.BattleTimeLimit.TotalSeconds * total);
            double progress = 0.0;

            TimeSpan timeoutRemaining = timeLimit - (DateTime.Now - tournamentStart);

            Log("Tournament start, ETA: {0}.", timeoutRemaining < timeRemaining ? ToString(timeoutRemaining) : ToString(timeRemaining));

            Profile[] currentTier = new Profile[profiles.Count];
            (shuffleProfiles ? Shuffle(profiles) : Seed(profiles)).CopyTo(currentTier);

            int tier = 1;

            while (currentTier.Length > 1)
            {
                Log("Tournament tier {0} start, {1} participants.", tier, currentTier.Length);

                Profile[] winners = new Profile[(int)Math.Round(currentTier.Length / 2.0)];

                ParallelOptions po = new ParallelOptions
                {
                    MaxDegreeOfParallelism = (maxConcurrentBattles == TrainerSettings.MaxMaxConcurrentBattles) ? Environment.ProcessorCount : maxConcurrentBattles
                };

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
                        int whiteIndex = Random.Next(0, 2); // Help mitigate top players always playing white
                        Profile whiteProfile = currentTier[profileIndex + whiteIndex];
                        Profile blackProfile = currentTier[profileIndex + 1 - whiteIndex];

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

                        lock (_progressLock)
                        {
                            timeoutRemaining = timeLimit - (DateTime.Now - tournamentStart);

                            GetProgress(tournamentStart, completed, remaining, out progress, out timeRemaining);
                            Log("Tournament progress: {0:P2}, ETA: {1}.", progress, timeoutRemaining < timeRemaining ? ToString(timeoutRemaining) : ToString(timeRemaining));
                        }

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

            Log("Tournament end, elapsed time: {0}.", ToString(DateTime.Now - tournamentStart));

            if (currentTier.Length == 1 && null != currentTier[0])
            {
                Profile winner = currentTier[0];
                Log("Tournament Winner: {0}", ToString(winner));
            }

            Profile best = (profiles.OrderByDescending(profile => profile.EloRating)).First();
            Log("Tournament Highest Elo: {0}", ToString(best));
        }

        public void AutoTrain()
        {
            AutoTrain(TrainerSettings.TargetProfilePath);
        }

        private void AutoTrain(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            StartTime = DateTime.Now;

            Log("AutoTrain start.");

            // Read profile
            Profile profile;
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                profile = Profile.ReadXml(fs);
            }

            int battleCount = 0;

            // Create AI
            GameAI gameAI = new GameAI(new GameAIConfig()
            {
                StartMetricWeights = profile.StartMetricWeights,
                EndMetricWeights = profile.EndMetricWeights,
                TranspositionTableSizeMB = TrainerSettings.TransTableSize,
            });

            while (TrainerSettings.MaxBattles == TrainerSettings.MaxMaxBattles || battleCount < TrainerSettings.MaxBattles)
            {
                Log("AutoTrain battle {0} start.", battleCount + 1);

                // Create Game
                GameBoard gameBoard = new GameBoard(TrainerSettings.GameType);

                EventHandler<BestMoveFoundEventArgs> puzzleCandidateHandler = null;

                if (TrainerSettings.FindPuzzleCandidates)
                {
                    puzzleCandidateHandler = GetPuzzleCandidateHandler(gameBoard);
                    gameAI.BestMoveFound += puzzleCandidateHandler;
                }

                try
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    cts.CancelAfter(TrainerSettings.BattleTimeLimit);

                    Task treeStrapTask = TrainerSettings.MaxDepth >= 0 ? gameAI.TreeStrapAsync(gameBoard, TrainerSettings.MaxDepth, TrainerSettings.MaxHelperThreads, cts.Token) : gameAI.TreeStrapAsync(gameBoard, TrainerSettings.TurnMaxTime, TrainerSettings.MaxHelperThreads, cts.Token);
                    treeStrapTask.Wait();

                    // Update profile with final MetricWeights
                    profile.UpdateMetricWeights(gameAI.StartMetricWeights, gameAI.EndMetricWeights);

                    // Write profile
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        profile.WriteXml(fs);
                    }

                    Log("AutoTrain battle {0} end {1};{2}[{3}].", battleCount + 1, gameBoard.BoardState.ToString(), gameBoard.CurrentTurnColor.ToString(), gameBoard.CurrentPlayerTurn);
                }
                catch (Exception ex)
                {
                    Log("AutoTrain battle {0} interrupted with exception {1}.", battleCount + 1, ex.Message);
                }
                finally
                {
                    if (null != puzzleCandidateHandler)
                    {
                        gameAI.BestMoveFound -= puzzleCandidateHandler;
                    }
                }

                battleCount++;
            }

            Log("AutoTrain end.");
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

        private EventHandler<BestMoveFoundEventArgs> GetPuzzleCandidateHandler(GameBoard gameBoard)
        {
            return (sender, args) =>
            {
                if (IsPuzzleCandidate(args))
                {
                    Log("Puzzle Candidate: {0} {1} {2}", gameBoard.BoardString, args.Depth, args.Move.ToString());
                }
            };
        }

        private bool IsPuzzleCandidate(BestMoveFoundEventArgs args)
        {
            return args.Depth % 2 == 1 && double.IsPositiveInfinity(args.Score);
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

            return string.Format("{0}({1}{2} {3}/{4}/{5})", profile.Name, profile.EloRating, IsProvisional(profile) ? "?" : " ", profile.Wins, profile.Losses, profile.Draws);
        }

        private bool IsProvisional(Profile profile)
        {
            if (null == profile)
            {
                throw new ArgumentNullException("profile");
            }

            return profile.TotalGames < TrainerSettings.ProvisionalGameCount;
        }

        private readonly object _progressLock = new object();
    }

    public enum GameResult
    {
        Loss,
        Draw,
        Win
    }
}
