// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Core;

namespace Mzinga.Test
{
    [TestClass]
    public class PuzzleCandidateTests
    {
        [TestMethod]
        public void PuzzleCandidates_IsOneBestMoveToForceWinPuzzleTest()
        {
            TestUtils.LoadAndExecuteTestCases<PuzzleCandidateIsOneBestMoveToForceWinPuzzleTestCase>("PuzzleCandidate_IsOneBestMoveToForceWinPuzzleTest.csv");
        }

        private class PuzzleCandidateIsOneBestMoveToForceWinPuzzleTestCase : ITestCase
        {
            public object[] TestArgs { get; set; }

            public Board Board;
            public int MaxDepth;

            public Move BestMove;

            public void Execute()
            {
                var pc = new PuzzleCandidate(Board, BestMove, MaxDepth);
                Assert.IsNotNull(pc);
                Assert.IsTrue(pc.IsOneBestMoveToForceWinPuzzle());
            }

            public void Parse(string s)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    throw new ArgumentNullException(nameof(s));
                }

                s = s.Trim();

                string[] vals = s.Split('\t');

                Assert.IsTrue(Board.TryParseGameString(vals[0], false, out Board));
                MaxDepth = int.Parse(vals[1]);
                Assert.IsTrue(Board.TryParseMove(vals[2], out BestMove, out string _));
            }
        }
    }
}
