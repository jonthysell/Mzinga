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

            List<Profile> profiles = LoadProfiles(path);

            DateTime startTime = DateTime.Now;
            TimeSpan timeElapsed = TimeSpan.Zero;

            Console.WriteLine("{0} > Tournament Start.", timeElapsed);

            Queue<Profile> remaining = new Queue<Profile>(profiles);

            while (remaining.Count > 1)
            {
                Profile whiteProfile = remaining.Dequeue();

                Profile blackProfile = remaining.Dequeue();

                BoardState roundResult = BoardState.Draw;

                timeElapsed = DateTime.Now - startTime;
                Console.WriteLine("{0} > Tournament Match Start.", timeElapsed);

                int rounds = 0;
                while (roundResult == BoardState.Draw)
                {
                    timeElapsed = DateTime.Now - startTime;
                    Console.WriteLine("{0} > Tournament Round {1} Start.", timeElapsed, rounds + 1);

                    roundResult = Battle(whiteProfile, blackProfile);

                    timeElapsed = DateTime.Now - startTime;
                    Console.WriteLine("{0} > Tournament Round {1} End.", timeElapsed, rounds + 1);

                    rounds++;

                    if (rounds >= 10 && roundResult == BoardState.Draw)
                    {
                        roundResult = whiteProfile.EloRating >= blackProfile.EloRating ? BoardState.WhiteWins : BoardState.BlackWins;

                        timeElapsed = DateTime.Now - startTime;
                        Console.WriteLine("{0} > Tournament Match Draw-out.", timeElapsed);
                    }
                }

                timeElapsed = DateTime.Now - startTime;
                Console.WriteLine("{0} > Tournament Match End, {1}.", timeElapsed, roundResult);

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

            timeElapsed = DateTime.Now - startTime;

            Console.WriteLine("{0} > Tournament End.", timeElapsed);

            Profile best = remaining.Dequeue();

            Console.WriteLine("Tournament Winner: {0} ({1})", best.Id, best.EloRating);
        }

        public static void BattleRoyale(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            List<Profile> profiles = LoadProfiles(path);

            DateTime startTime = DateTime.Now;
            TimeSpan timeElapsed = TimeSpan.Zero;

            Console.WriteLine("{0} > Battle Royale Start.", timeElapsed);

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

            timeElapsed = DateTime.Now - startTime;

            Console.WriteLine("{0} > Battle Royale End.", timeElapsed);

            Profile best = (profiles.OrderBy(profile => profile.EloRating)).Last();

            Console.WriteLine("Battle Royale Highest ELO: {0} ({1})", best.Id, best.EloRating);

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

            DateTime startTime = DateTime.Now;
            TimeSpan timeElapsed = TimeSpan.Zero;

            TimeSpan timeLimit = TimeSpan.FromMinutes(1.0);

            Console.WriteLine("{0} > Battle start, {1} vs. {2}.", timeElapsed, whiteProfile.Nickname, blackProfile.Nickname);

            List<string> boardKeys = new List<string>();

            // Play Game
            while (gameBoard.GameInProgress)
            {
                Move move = gameBoard.CurrentTurnColor == Color.White ? whiteAI.GetBestMove(gameBoard) : blackAI.GetBestMove(gameBoard);
                gameBoard.Play(move);

                boardKeys.Add(gameBoard.GetTranspositionKey());

                timeElapsed = DateTime.Now - startTime;

                if (boardKeys.Count >= 6)
                {
                    int lastIndex = boardKeys.Count - 1;
                    if (boardKeys[lastIndex] == boardKeys[lastIndex - 4] && boardKeys[lastIndex - 1] == boardKeys[lastIndex - 5])
                    {
                        Console.WriteLine("{0} > Battle loop-out.", timeElapsed);
                        break;
                    }
                }

                if (timeElapsed > timeLimit)
                {
                    Console.WriteLine("{0} > Battle time-out.", timeElapsed);
                    break;
                }
                else if ((int)timeElapsed.TotalSeconds % 10 == 0)
                {
                    Console.WriteLine("{0} > Battle in-progress.", timeElapsed);
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
            Console.WriteLine("{0} > Battle end, {1}", timeElapsed, boardState);
            Console.WriteLine("{0} > Battle end, {1} / {2}", timeElapsed, whiteProfile.Nickname, blackProfile.Nickname);

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
    }
}
