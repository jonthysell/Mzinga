// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using Mzinga.Core;
using Mzinga.Core.AI;
using Mzinga.Engine;

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
                _settings = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        private TrainerSettings _settings;

        public Random Random
        {
            get
            {
                return _random ??= new Random();
            }
        }
        private Random _random;

        public Trainer()
        {
            TrainerSettings = new TrainerSettings();
        }

        public void Battle()
        {
            RunCommandInAllGameTypes(() => { Battle(TrainerSettings.WhiteProfilePath, TrainerSettings.BlackProfilePath); });
        }

        private void Battle(string whiteProfilePath, string blackProfilePath)
        {
            if (string.IsNullOrWhiteSpace(whiteProfilePath))
            {
                throw new ArgumentNullException(nameof(whiteProfilePath));
            }

            if (string.IsNullOrWhiteSpace(blackProfilePath))
            {
                throw new ArgumentNullException(nameof(blackProfilePath));
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
            RunCommandInAllGameTypes(() => { BattleRoyale(TrainerSettings.ProfilesPath, TrainerSettings.MaxBattles, TrainerSettings.MaxDraws, TrainerSettings.BulkBattleTimeLimit, TrainerSettings.BattleShuffleProfiles, TrainerSettings.ProvisionalFirst, TrainerSettings.MaxConcurrentBattles); });
        }

        private void BattleRoyale(string path, int maxBattles, int maxDraws, TimeSpan timeLimit, bool shuffleProfiles, bool provisionalFirst, int maxConcurrentBattles)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (maxBattles < 1 && maxBattles != TrainerSettings.MaxMaxBattles)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBattles));
            }

            if (maxDraws < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDraws));
            }

            if (maxConcurrentBattles < 1 && maxConcurrentBattles != TrainerSettings.MaxMaxConcurrentBattles)
            {
                throw new ArgumentOutOfRangeException(nameof(maxConcurrentBattles));
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

            List<Profile> whiteProfiles = profiles.OrderByDescending(profile => profile.Records[(int)TrainerSettings.GameType].EloRating).ToList();
            List<Profile> blackProfiles = profiles.OrderBy(profile => profile.Records[(int)TrainerSettings.GameType].EloRating).ToList();

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

            if (provisionalFirst)
            {
                matches = matches.OrderByDescending(match => IsProvisional(match.Item1) != IsProvisional(match.Item2) ? 1 : 0).ToList();
            }

            matches = matches.Take(remaining).ToList();

            ParallelOptions po = new ParallelOptions
            {
                MaxDegreeOfParallelism = (maxConcurrentBattles == TrainerSettings.MaxMaxConcurrentBattles) ? Environment.ProcessorCount : maxConcurrentBattles
            };

            Parallel.ForEach(matches.AsParallel().AsOrdered(), po, (match, loopState) =>
            {
                Profile whiteProfile = match.Item1;
                Profile blackProfile = match.Item2;

                BoardState roundResult = BoardState.Draw;

                if (maxDraws == 1)
                {
                    roundResult = Battle(whiteProfile, blackProfile);
                }
                else
                {
                    int rounds = 0;
                    while (roundResult == BoardState.Draw)
                    {
                        roundResult = Battle(whiteProfile, blackProfile);

                        rounds++;

                        if (roundResult == BoardState.Draw)
                        {
                            if (rounds >= maxDraws && roundResult == BoardState.Draw)
                            {
                                Log("Battle {0} vs. {1} draw-out.", ToString(whiteProfile), ToString(blackProfile));
                                break;
                            }
                            Log("Battle {0} vs. {1} draws, re-match.", ToString(whiteProfile), ToString(blackProfile));
                        }
                    }
                }

                Interlocked.Increment(ref completed);
                Interlocked.Decrement(ref remaining);

                // Save Profiles
                lock (whiteProfile)
                {
                    string whiteProfilePath = Path.Combine(path, whiteProfile.Id + ".xml");
                    using FileStream fs = new FileStream(whiteProfilePath, FileMode.Create);
                    whiteProfile.WriteXml(fs);
                }

                lock (blackProfile)
                {
                    string blackProfilePath = Path.Combine(path, blackProfile.Id + ".xml");
                    using FileStream fs = new FileStream(blackProfilePath, FileMode.Create);
                    blackProfile.WriteXml(fs);
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

            Profile best = (profiles.OrderByDescending(profile => profile.Records[(int)TrainerSettings.GameType].EloRating)).First();

            Log("Battle Royale Highest Elo: {0}", ToString(best));

        }

        private readonly object _eloLock = new object();

        private BoardState Battle(Profile whiteProfile, Profile blackProfile)
        {
            if (null == whiteProfile)
            {
                throw new ArgumentNullException(nameof(whiteProfile));
            }

            if (null == blackProfile)
            {
                throw new ArgumentNullException(nameof(blackProfile));
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

            Log("Battle start {0} {1} vs. {2}.", EnumUtils.GetExpansionPiecesString(gameBoard.ExpansionPieces), ToString(whiteProfile, gameBoard.ExpansionPieces), ToString(blackProfile, gameBoard.ExpansionPieces));

            DateTime battleStart = DateTime.Now;
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

                    TimeSpan battleElapsed = DateTime.Now - battleStart;
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
                whiteRating = whiteProfile.Records[(int)gameBoard.ExpansionPieces].EloRating;
                whiteK = IsProvisional(whiteProfile) ? EloUtils.ProvisionalK : EloUtils.DefaultK;
            }

            int blackRating;
            double blackK;

            lock (blackProfile)
            {
                blackRating = blackProfile.Records[(int)gameBoard.ExpansionPieces].EloRating;
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
                whiteProfile.UpdateRecord(whiteEndRating, whiteResult, gameBoard.ExpansionPieces);
            }

            lock (blackProfile)
            {
                blackProfile.UpdateRecord(blackEndRating, blackResult, gameBoard.ExpansionPieces);
            }

            // Output Results
            Log("Battle end {0} {1} {2} vs. {3}.", EnumUtils.GetExpansionPiecesString(gameBoard.ExpansionPieces), boardState, ToString(whiteProfile, gameBoard.ExpansionPieces), ToString(blackProfile, gameBoard.ExpansionPieces));

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
                throw new ArgumentNullException(nameof(path));
            }

            if (keepCount < TrainerSettings.CullMinKeepCount && keepCount != TrainerSettings.CullKeepMax)
            {
                throw new ArgumentOutOfRangeException(nameof(keepCount));
            }

            StartTime = DateTime.Now;
            Log("Cull start.");

            List<Profile> profiles = LoadProfiles(path);

            if (provisionalRules)
            {
                profiles = profiles.Where(profile => !IsProvisional(profile)).ToList();
            }

            profiles = profiles.OrderByDescending(profile => profile.Records[(int)TrainerSettings.GameType].EloRating).ToList();

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
                throw new ArgumentNullException(nameof(path));
            }

            StartTime = DateTime.Now;
            Log("Enumerate start.");

            List<Profile> profiles = LoadProfiles(path);
            profiles = profiles.OrderByDescending(profile => profile.Records[(int)TrainerSettings.GameType].EloRating).ToList();

            foreach (Profile p in profiles)
            {
                Log("{0}", ToString(p));

                string profilePath = Path.Combine(path, p.Id + ".xml");
                using FileStream fs = new FileStream(profilePath, FileMode.Create);
                p.WriteXml(fs);
            }

            Log("Enumerate end.");
        }

        public void Analyze()
        {
            RunCommandInAllGameTypes(() => { Analyze(TrainerSettings.ProfilesPath); });
        }

        public void Analyze(string path)
        {
            StartTime = DateTime.Now;
            Log("Analyze start.");

            List<Profile> profiles = LoadProfiles(path);
            profiles = profiles.OrderByDescending(profile => profile.Records[(int)TrainerSettings.GameType].EloRating).ToList();

            string resultFile = Path.Combine(path, string.Format("analyze{0}.csv", EnumUtils.GetExpansionPiecesString(TrainerSettings.GameType)));

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

                    profileSB.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8}", p.Id, p.Name, p.Records[(int)TrainerSettings.GameType].EloRating, p.Generation, p.ParentA.HasValue ? p.ParentA.ToString() : "", p.ParentB.HasValue ? p.ParentB.ToString() : "", p.Records[(int)TrainerSettings.GameType].Wins, p.Records[(int)TrainerSettings.GameType].Losses, p.Records[(int)TrainerSettings.GameType].Draws);

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
                throw new ArgumentNullException(nameof(path));
            }

            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
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
                throw new ArgumentNullException(nameof(path));
            }

            if (generations == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(generations));
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
                            BattleRoyale(path, TrainerSettings.MaxBattles, TrainerSettings.MaxDraws, TrainerSettings.BulkBattleTimeLimit, TrainerSettings.BattleShuffleProfiles, TrainerSettings.ProvisionalFirst, TrainerSettings.MaxConcurrentBattles);
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
                throw new ArgumentNullException(nameof(path));
            }

            if (minMix > maxMix)
            {
                throw new ArgumentOutOfRangeException(nameof(minMix));
            }

            if (parentCount < TrainerSettings.MateMinParentCount && parentCount != TrainerSettings.MateParentMax)
            {
                throw new ArgumentOutOfRangeException(nameof(parentCount));
            }

            StartTime = DateTime.Now;
            Log("Mate start.");

            List<Profile> profiles = LoadProfiles(path);

            if (provisionalRules)
            {
                profiles = profiles.Where(profile => !IsProvisional(profile)).ToList();
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

                    using FileStream fs = new FileStream(Path.Combine(path, child.Id + ".xml"), FileMode.Create);
                    child.WriteXml(fs);
                }
            }

            Log("Mate end.");
        }

        public void Tournament()
        {
            RunCommandInAllGameTypes(() => { Tournament(TrainerSettings.ProfilesPath, TrainerSettings.MaxDraws, TrainerSettings.BulkBattleTimeLimit, TrainerSettings.BattleShuffleProfiles, TrainerSettings.MaxConcurrentBattles); });
        }

        private void Tournament(string path, int maxDraws, TimeSpan timeLimit,  bool shuffleProfiles, int maxConcurrentBattles)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (maxDraws < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDraws));
            }

            if (maxConcurrentBattles < 1 && maxConcurrentBattles != TrainerSettings.MaxMaxConcurrentBattles)
            {
                throw new ArgumentOutOfRangeException(nameof(maxConcurrentBattles));
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

                        Profile drawWinnerProfile = whiteProfile.Records[(int)TrainerSettings.GameType].EloRating < blackProfile.Records[(int)TrainerSettings.GameType].EloRating ? whiteProfile : blackProfile;

                        if (maxDraws == 1)
                        {
                            roundResult = Battle(whiteProfile, blackProfile);
                        }
                        else
                        {
                            int rounds = 0;
                            while (roundResult == BoardState.Draw)
                            {
                                roundResult = Battle(whiteProfile, blackProfile);

                                rounds++;

                                if (roundResult == BoardState.Draw)
                                {
                                    if (rounds >= maxDraws && roundResult == BoardState.Draw)
                                    {
                                        Log("Battle {0} vs. {1} draw-out.", ToString(whiteProfile), ToString(blackProfile));
                                        break;
                                    }
                                    Log("Battle {0} vs. {1} draws, re-match.", ToString(whiteProfile), ToString(blackProfile));
                                }
                            }
                        }

                        if (roundResult == BoardState.Draw)
                        {
                            // Need someone to advance
                            roundResult = (drawWinnerProfile == whiteProfile) ? BoardState.WhiteWins : BoardState.BlackWins;
                        }

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
                            using FileStream fs = new FileStream(whiteProfilePath, FileMode.Create);
                            whiteProfile.WriteXml(fs);
                        }

                        lock (blackProfile)
                        {
                            string blackProfilePath = Path.Combine(path, blackProfile.Id + ".xml");
                            using FileStream fs = new FileStream(blackProfilePath, FileMode.Create);
                            blackProfile.WriteXml(fs);
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

            Profile best = (profiles.OrderByDescending(profile => profile.Records[(int)TrainerSettings.GameType].EloRating)).First();
            Log("Tournament Highest Elo: {0}", ToString(best));
        }

        public void AutoTrain()
        {
            RunCommandInAllGameTypes(() => { AutoTrain(TrainerSettings.TargetProfilePath); });
        }

        private void AutoTrain(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
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
                // Create Game
                GameBoard gameBoard = new GameBoard(TrainerSettings.GameType);

                EventHandler<BestMoveFoundEventArgs> puzzleCandidateHandler = null;

                if (TrainerSettings.FindPuzzleCandidates)
                {
                    puzzleCandidateHandler = GetPuzzleCandidateHandler(gameBoard);
                    gameAI.BestMoveFound += puzzleCandidateHandler;
                }

                Log("AutoTrain battle {0} {1} start.", EnumUtils.GetExpansionPiecesString(gameBoard.ExpansionPieces), battleCount + 1);

                try
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    cts.CancelAfter(TrainerSettings.BattleTimeLimit);

                    Task treeStrapTask = TrainerSettings.MaxDepth >= 0 ? gameAI.TreeStrapAsync(gameBoard, TrainerSettings.MaxDepth, TrainerSettings.MaxHelperThreads, cts.Token) : gameAI.TreeStrapAsync(gameBoard, TrainerSettings.TurnMaxTime, TrainerSettings.MaxHelperThreads, cts.Token);
                    treeStrapTask.Wait();

                    // Update profile with final MetricWeights
                    profile.UpdateMetricWeights(gameAI.StartMetricWeights, gameAI.EndMetricWeights, gameBoard.ExpansionPieces);

                    // Write profile
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        profile.WriteXml(fs);
                    }

                    Log("AutoTrain battle {0} {1} end {2};{3}[{4}].", EnumUtils.GetExpansionPiecesString(gameBoard.ExpansionPieces), battleCount + 1, gameBoard.BoardState.ToString(), gameBoard.CurrentTurnColor.ToString(), gameBoard.CurrentPlayerTurn);
                }
                catch (Exception ex)
                {
                    Log("AutoTrain battle {0} {1} interrupted with exception {2}.", EnumUtils.GetExpansionPiecesString(gameBoard.ExpansionPieces), battleCount + 1, ex.Message);
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

        public void Top()
        {
            RunCommandInAllGameTypes(() => { Top(TrainerSettings.ProfilesPath); });
        }

        private void Top(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            StartTime = DateTime.Now;
            Log("Top start.");

            List<Profile> profiles = LoadProfiles(path);

            ExpansionPieces gameType = TrainerSettings.GameType;

            foreach (Profile profile in profiles.OrderByDescending(profile => profile.Records[(int)gameType].EloRating).Take(TrainerSettings.TopCount))
            {
                Log("Top {0}: {1}", EnumUtils.GetExpansionPiecesString(gameType), ToString(profile, gameType));
            }

            Log("Top end.");
        }

        public void MergeTop()
        {
            MergeTop(TrainerSettings.ProfilesPath);
        }

        private void MergeTop(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            StartTime = DateTime.Now;
            Log("MergeTop start.");

            List<Profile> profiles = LoadProfiles(path);

            string resultFile = Path.Combine(path, "mergetop.txt");

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                ConformanceLevel = ConformanceLevel.Fragment,
            };

            using (XmlWriter xw = XmlWriter.Create(resultFile, settings))
            {
                for (int i = 0; i < EnumUtils.NumGameTypes; i++)
                {
                    ExpansionPieces gameType = (ExpansionPieces)i;

                    Profile topProfile = profiles.OrderByDescending(profile => profile.Records[i].EloRating).First();

                    Log("Adding {0} for {1}", ToString(topProfile, gameType), EnumUtils.GetExpansionPiecesString(gameType));

                    topProfile.StartMetricWeights.WriteMetricWeightsXml(xw, "StartMetricWeights", gameType);
                    topProfile.EndMetricWeights.WriteMetricWeightsXml(xw, "EndMetricWeights", gameType);
                }
            }

            Log("MergeTop end.");
        }

        public void ExportAI()
        {
            ExportAI(TrainerSettings.ProfilesPath);
        }

        private void ExportAI(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            StartTime = DateTime.Now;
            Log("ExportAI start.");

            GameEngineConfig config = Engine.GameEngineConfig.GetDefaultEngineConfig();

            string version = Assembly.GetEntryAssembly().GetName().Version.ToString();

            for (int i = 0; i < EnumUtils.NumGameTypes; i++)
            {
                ExpansionPieces gameType = (ExpansionPieces)i;

                Guid id = Guid.Parse(string.Format("00000000-0000-0000-0000-{0}{1}", version.Replace(".", ""), i));
                string name = string.Format("Mzinga v{0} ({1})", version, EnumUtils.GetExpansionPiecesString(gameType));

                Profile p = new Profile(id, name, config.MetricWeightSet[gameType][0], config.MetricWeightSet[gameType][1]);

                string profilePath = Path.Combine(path, p.Id + ".xml");
                using FileStream fs = new FileStream(profilePath, FileMode.Create);
                Log("{0}", ToString(p, gameType));
                p.WriteXml(fs);
            }

            Log("ExportAI end.");
        }

        public void BuildInitialTables()
        {
            BuildInitialTables(TrainerSettings.ProfilesPath);
        }

        public void BuildInitialTables(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            StartTime = DateTime.Now;
            Log("BuildInitialTables start.");

            GameEngineConfig config = Engine.GameEngineConfig.GetDefaultEngineConfig();

            string resultFile = Path.Combine(path, "initialtables.txt");

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                ConformanceLevel = ConformanceLevel.Fragment,
            };

            using (XmlWriter xw = XmlWriter.Create(resultFile, settings))
            {
                for (int i = 0; i < EnumUtils.NumGameTypes; i++)
                {
                    ExpansionPieces gameType = (ExpansionPieces)i;

                    Log("Building {0} initial table.", EnumUtils.GetExpansionPiecesString(gameType));

                    // Creating initial table to save results

                    TranspositionTable initialTable = new TranspositionTable(TrainerSettings.TransTableSize * 1024 * 1024);

                    // Creating board and AI

                    GameBoard gameBoard = new GameBoard(gameType);

                    MetricWeights[] mw = config.GetMetricWeights(gameType);

                    GameAI gameAI = new GameAI(new GameAIConfig()
                    {
                        StartMetricWeights = mw[0],
                        EndMetricWeights = mw[0] ?? mw[1],
                        MaxBranchingFactor = config.MaxBranchingFactor,
                        TranspositionTableSizeMB = TrainerSettings.TransTableSize,
                        InitialTranspositionTable =  null,
                    });

                    ulong currentKey = gameBoard.ZobristKey;

                    gameAI.BestMoveFound += (sender, args) =>
                    {
                        // When best moves are found that meet the criteria, save their entries into the initial table
                        if (args.Depth >= TrainerSettings.MaxDepth && gameAI.TranspositionTable.TryLookup(currentKey, out TranspositionTableEntry entry))
                        {
                            lock (initialTable)
                            {
                                initialTable.Store(currentKey, new TranspositionTableEntry()
                                {
                                    Type = entry.Type,
                                    Value = entry.Value,
                                    Depth = entry.Depth,
                                    BestMove = entry.BestMove,
                                });
                            }
                        }
                    };

                    // Run search from every possible board position
                    CancellationTokenSource cts = new CancellationTokenSource();
                    Task searchTask = Task.Factory.StartNew(async () =>
                    {
                        await gameBoard.ForEachBoardPosition(TrainerSettings.InitialTableDepth == TrainerSettings.MaxInitialTableDepth ? int.MaxValue : TrainerSettings.InitialTableDepth, () =>
                        {
                            currentKey = gameBoard.ZobristKey;
                            GetBestMove(gameBoard, gameAI);
                        }, cts.Token);
                    });

                    Task progressTask = Task.Factory.StartNew(async () =>
                    {
                        while (!cts.Token.IsCancellationRequested)
                        {
                            lock (initialTable)
                            {
                                Log("Initial {0} table has {1} entries.", EnumUtils.GetExpansionPiecesString(gameType), initialTable.Count);
                            }
                            await Task.Delay(TimeSpan.FromMinutes(1.0));
                        }
                    });

                    searchTask.Wait(cts.Token);
                    cts.Cancel();
                    progressTask.Wait();

                    Log("Saving {0} initial table ({1} entries).", EnumUtils.GetExpansionPiecesString(gameType), initialTable.Count);

                    initialTable.WriteTranspositionTableXml(xw, "InitialTranspositionTable", gameType);
                }
            }

            Log("BuildInitialTables end.");
        }

        private static List<Profile> LoadProfiles(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            List<Profile> profiles = new List<Profile>();
            
            Parallel.ForEach(Directory.EnumerateFiles(path, "*.xml"), (profilePath) =>
            {
                using FileStream fs = new FileStream(profilePath, FileMode.Open);
                Profile profile = Profile.ReadXml(fs);
                lock (profiles)
                {
                    profiles.Add(profile);
                }
            }
            );

            return profiles;
        }

        private List<T> Shuffle<T>(List<T> items)
        {
            if (null == items)
            {
                throw new ArgumentNullException(nameof(items));
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
                throw new ArgumentNullException(nameof(profiles));
            }

            LinkedList<Profile> sortedProfiles = new LinkedList<Profile>(profiles.OrderByDescending(profile => profile.Records[(int)TrainerSettings.GameType].EloRating));
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

        private void RunCommandInAllGameTypes(Action action)
        {
            if (!TrainerSettings.AllGameTypes)
            {
                action();
            }
            else
            {
                for (int i = 0; i < EnumUtils.NumGameTypes; i++)
                {
                    TrainerSettings.GameType = (ExpansionPieces)i;
                    action();
                }
            }
        }

        private void Log(string format, params object[] args)
        {
            TimeSpan elapsedTime = DateTime.Now - StartTime;
            Console.WriteLine(string.Format("{0} > {1}", ToString(elapsedTime), string.Format(format, args)));
        }

        private static void GetProgress(DateTime startTime, int completed, int remaining, out double progress, out TimeSpan timeRemaining)
        {
            if (completed < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(completed));
            }

            if (remaining < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(remaining));
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
                    Log("Puzzle Candidate: {0} {1} {2}", gameBoard.ToGameString(), args.Depth, NotationUtils.ToBoardSpaceMoveString(gameBoard, args.Move));
                }
            };
        }

        private static bool IsPuzzleCandidate(BestMoveFoundEventArgs args)
        {
            return args.Depth % 2 == 1 && double.IsPositiveInfinity(args.Score);
        }

        private static string ToString(TimeSpan ts)
        {
            return ts.Days.ToString() + "." + ts.ToString(@"hh\:mm\:ss");
        }

        private string ToString(Profile profile)
        {
            return ToString(profile, TrainerSettings.GameType);
        }

        private string ToString(Profile profile, ExpansionPieces gameType)
        {
            if (null == profile)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            return string.Format("{0}({1}{2} {3}/{4}/{5})", profile.Name, profile.Records[(int)gameType].EloRating, IsProvisional(profile) ? "?" : " ", profile.Records[(int)gameType].Wins, profile.Records[(int)gameType].Losses, profile.Records[(int)gameType].Draws);
        }

        private bool IsProvisional(Profile profile)
        {
            if (null == profile)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            return profile.Records[(int)TrainerSettings.GameType].TotalGames < TrainerSettings.ProvisionalGameCount;
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
