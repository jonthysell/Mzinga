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
        public void PuzzleCandidates_IsPuzzleTest()
        {
            TestUtils.LoadAndExecuteTestCases<PuzzleCandidateIsPuzzleTestCase>("PuzzleCandidate_IsPuzzleTest.csv");
        }

        private class PuzzleCandidateIsPuzzleTestCase : ITestCase
        {
            public Board Board;
            public int MaxDepth;

            public Move BestMove;

            public void Execute()
            {
                var pc = new PuzzleCandidate(Board, BestMove, MaxDepth);
                Assert.IsNotNull(pc);
                Assert.IsTrue(pc.IsPuzzle());
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
