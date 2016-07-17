// 
// GameEngineConfig.cs
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
using System.IO;
using System.Xml;

using Mzinga.Core.AI;

namespace Mzinga.Engine
{
    public class GameEngineConfig
    {

        #region GameAI

        public bool AlphaBetaPruning { get; private set; }

        public bool TranspositionTable { get; private set; }

        public int MaxDepth { get; private set; }

        public TimeSpan? MaxTime { get; private set; }

        public MetricWeights MetricWeights { get; private set; }

        #endregion

        public GameEngineConfig()
        {
            AlphaBetaPruning = false;
            TranspositionTable = false;
            MaxDepth = 0;
            MaxTime = null;
            MetricWeights = new MetricWeights();
        }

        public GameEngineConfig(Stream inputStream) : this()
        {
            LoadConfig(inputStream);
        }

        private void LoadConfig(Stream inputStream)
        {
            if (null == inputStream)
            {
                throw new ArgumentNullException("inputStream");
            }

            using (XmlReader reader = XmlReader.Create(inputStream))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "GameAI":
                                LoadGameAIConfig(reader.ReadSubtree());
                                break;
                        }
                    }
                }
            }
        }

        private void LoadGameAIConfig(XmlReader reader)
        {
            if (null == reader)
            {
                throw new ArgumentNullException("reader");
            }

            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "MaxDepth":
                            MaxDepth = reader.ReadElementContentAsInt();
                            break;
                        case "MaxTime":
                            MaxTime = TimeSpan.Parse(reader.ReadElementContentAsString());
                            break;
                        case "AlphaBetaPruning":
                            AlphaBetaPruning = reader.ReadElementContentAsBoolean();
                            break;
                        case "TranspositionTable":
                            TranspositionTable = reader.ReadElementContentAsBoolean();
                            break;
                        case "MetricWeights":
                            MetricWeights = MetricWeights.ReadMetricWeightsXml(reader.ReadSubtree());
                            break;
                    }
                }
            }
        }

        public GameAI GetGameAI()
        {
            GameAI ai = new GameAI();

            ai.MaxDepth = MaxDepth;
            ai.MaxTime = MaxTime;

            ai.AlphaBetaPruning = AlphaBetaPruning;
            ai.TranspositionTable = TranspositionTable;

            ai.MetricWeights.CopyFrom(MetricWeights);

            return ai;
        }
    }
}
