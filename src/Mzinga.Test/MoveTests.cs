// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Core;

namespace Mzinga.Test
{
    [TestClass]
    public class MoveTests
    {
        [TestMethod]
        public void Move_BuildMoveString_PassMoveTest()
        {
            Assert.AreEqual(Move.PassString, Move.BuildMoveString(true, PieceName.INVALID, '\0', PieceName.INVALID, '\0'));
            Assert.AreNotEqual(Move.PassString, Move.BuildMoveString(false, PieceName.INVALID, '\0', PieceName.INVALID, '\0'));
        }

        [TestMethod]
        public void Move_BuildMoveString_ValidMoveTest()
        {
            Assert.AreEqual("wA1", Move.BuildMoveString(false, PieceName.wA1, '\0', PieceName.INVALID, '\0'));
            Assert.AreEqual("wB1 wA1", Move.BuildMoveString(false, PieceName.wB1, '\0', PieceName.wA1, '\0'));
            Assert.AreEqual("wB1 -wA1", Move.BuildMoveString(false, PieceName.wB1, '-', PieceName.wA1, '\0'));
            Assert.AreEqual("wB1 wA1-", Move.BuildMoveString(false, PieceName.wB1, '\0', PieceName.wA1, '-'));
        }

        [TestMethod]
        public void Move_TryNormalizeMoveString_InvalidMoveTest()
        {
            foreach (var str in InvalidMoveStrings)
            {
                Assert.IsFalse(Move.TryNormalizeMoveString(str, out string result));
                Assert.AreEqual(default, result);
            }
        }

        private static readonly string[] InvalidMoveStrings = new string[] {
            "",
            " ",
            "test",
            "wA1 test",
            "wA1 ?bA1",
            "wA1 bA1?",
            "bA1 test",
            "bA1 ?wA1",
            "bA1 wA1?",
            "wQ test",
            "wQ ?wA1",
            "wQ wA1?",
        };

        [TestMethod]
        public void Move_TryNormalizeMoveString_ValidNormalizedMoveTest()
        {
            foreach (var str in ValidNormalizedMoveStrings)
            {
                Assert.IsTrue(Move.TryNormalizeMoveString(str, out string result));
                Assert.AreEqual(str, result);
            }
        }

        [TestMethod]
        public void Move_TryNormalizeMoveString_ValidDenormalizedMoveTest()
        {
            foreach (var normalizedMoveStr in ValidNormalizedMoveStrings)
            {
                foreach (var denormalizer in MoveStringDenormalizers)
                {
                    var denormalizedMoveStr = denormalizer(normalizedMoveStr);
                    Assert.IsTrue(Move.TryNormalizeMoveString(denormalizedMoveStr, out string result));
                    Assert.AreEqual(normalizedMoveStr, result);
                }
            }
        }

        [TestMethod]
        public void Move_TryNormalizeMoveStringThenBuildMoveString_ValidNormalizedMoveTest()
        {
            foreach (var str in ValidNormalizedMoveStrings)
            {
                Assert.IsTrue(Move.TryNormalizeMoveString(str, out bool isPass, out PieceName startPiece, out char beforeSeperator, out PieceName endPiece, out char afterSeperator));
                var result = Move.BuildMoveString(isPass, startPiece, beforeSeperator, endPiece, afterSeperator);
                Assert.AreEqual(str, result);
            }
        }

        [TestMethod]
        public void Move_TryNormalizeMoveStringThenBuildMoveString_ValidDenormalizedMoveTest()
        {
            foreach (var normalizedMoveStr in ValidNormalizedMoveStrings)
            {
                foreach (var denormalizer in MoveStringDenormalizers)
                {
                    var denormalizedMoveStr = denormalizer(normalizedMoveStr);
                    Assert.IsTrue(Move.TryNormalizeMoveString(denormalizedMoveStr, out bool isPass, out PieceName startPiece, out char beforeSeperator, out PieceName endPiece, out char afterSeperator));
                    var result = Move.BuildMoveString(isPass, startPiece, beforeSeperator, endPiece, afterSeperator);
                    Assert.AreEqual(normalizedMoveStr, result);
                }
            }
        }

        private static readonly Func<string,string>[] MoveStringDenormalizers = new Func<string, string>[] {
            s => " " + s,
            s => s + " ",
            s => " " + s + " ",
            s => s.ToUpperInvariant(),
            s => s.ToLowerInvariant(),
            s => s.Replace(" ", "  "),
        };

        private static readonly string[] ValidNormalizedMoveStrings = new string[] {
            "pass",
            "wQ",
            "wA1",
            "wB1 wA1",
            "wB1 -wA1",
            "wB1 wA1-",
            "wB1 /wA1",
            "wB1 wA1/",
            "wB1 \\wA1",
            "wB1 wA1\\",
            "bQ",
            "bA1",
            "bB1 bA1",
            "bB1 -bA1",
            "bB1 bA1-",
            "bB1 /bA1",
            "bB1 bA1/",
            "bB1 \\bA1",
            "bB1 bA1\\",
        };
    }
}
