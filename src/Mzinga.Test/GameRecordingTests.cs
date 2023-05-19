// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Core;

namespace Mzinga.Test
{
    [TestClass]
    public class GameRecordingTests
    {
        [TestMethod]
        public void GameRecordingTests_NewTest()
        {
            GameRecording gr = new GameRecording(new Board(), GameRecordingSource.Game);
            Assert.IsNotNull(gr);
        }

        [TestMethod]
        public void GameRecordingTests_LoadTest()
        {
            TestUtils.ProcessEmbeddedResources("\\.(pgn|sgf)$", (name, inputStream) =>
            {
                var gr = GameRecording.Load(inputStream, name);
                Assert.IsNotNull(gr);
            });
        }

        [TestMethod]
        public void GameRecordingTests_LoadSGFTest()
        {
            TestUtils.ProcessEmbeddedResources("\\.sgf$", (name, inputStream) =>
            {
                var gr = GameRecording.LoadSGF(inputStream, name);
                Assert.IsNotNull(gr);
            });
        }

        [TestMethod]
        public void GameRecordingTests_LoadPGNTest()
        {
            TestUtils.ProcessEmbeddedResources("\\.pgn$", (name, inputStream) =>
            {
                var gr = GameRecording.LoadPGN(inputStream, name);
                Assert.IsNotNull(gr);
            });
        }

        [TestMethod]
        public void GameRecordingTests_SavePGNTest()
        {
            TestUtils.ProcessEmbeddedResources("\\.(pgn|sgf)$", (name, inputStream) =>
            {
                var gr = GameRecording.Load(inputStream, name);
                Assert.IsNotNull(gr);

                using var ms = new MemoryStream();
                gr.SavePGN(ms);

                Assert.IsNotNull(ms);
                string actualPGNText = Encoding.ASCII.GetString(ms.ToArray()).ReplaceLineEndings();

                var expectedPGNStream = TestUtils.GetEmbeddedResource(Path.ChangeExtension(name, ".pgn"));
                Assert.IsNotNull(expectedPGNStream);

                using var sr = new StreamReader(expectedPGNStream);
                string expectedPGNText = sr.ReadToEnd().ReplaceLineEndings();

                Assert.AreEqual(expectedPGNText, actualPGNText);
            });
        }
    }
}
