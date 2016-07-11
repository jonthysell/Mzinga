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
                return _startTime;
            }
            private set
            {
                _startTime = value;
            }
        }
        private static DateTime _startTime;

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
            }
        }

        public static void Tournament(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            StartTime = DateTime.Now;
            Log("Tournament start.");

            List<Profile> profiles = LoadProfiles(path);
            Queue<Profile> remaining = new Queue<Profile>(profiles);

            while (remaining.Count > 1)
            {
                Log("Tournament remaining: {0}.", remaining.Count);

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

                    if (rounds >= 10 && roundResult == BoardState.Draw)
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
            }

            Log("Tournament end.");

            Profile best = remaining.Dequeue();

            Log("Tournament Winner: {0} ({1})", best.Id, best.EloRating);
        }

        public static void BattleRoyale(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            StartTime = DateTime.Now;
            Log("Battle Royale start.");

            List<Profile> profiles = LoadProfiles(path);

            // Run the battle royale
            foreach (Profile whiteProfile in profiles)
            {
                foreach (Profile blackProfile in profiles)
                {
                    if (whiteProfile != blackProfile)
                    {
                        Battle(whiteProfile, blackProfile);

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
                    }
                }
            }

            Log("Battle Royale end.");

            Profile best = (profiles.OrderBy(profile => profile.EloRating)).Last();

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
            whiteAI.MaxDepth = GameAI.IterativeDepth;
            whiteAI.MaxTime = TimeSpan.FromSeconds(1.0);

            whiteAI.AlphaBetaPruning = true;
            whiteAI.TranspositionTable = true;

            GameAI blackAI = new GameAI(blackProfile.MetricWeights);
            blackAI.MaxDepth = GameAI.IterativeDepth;
            blackAI.MaxTime = TimeSpan.FromSeconds(1.0);

            blackAI.AlphaBetaPruning = true;
            blackAI.TranspositionTable = true;

            TimeSpan timeLimit = TimeSpan.FromMinutes(1.0);

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
                else if ((int)battleElapsed.TotalSeconds % 10 == 0)
                {
                    Log("Battle in-progress.");
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
    }
}
