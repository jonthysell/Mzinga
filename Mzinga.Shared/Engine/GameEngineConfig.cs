// 
// GameEngineConfig.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2018 Jon Thysell <http://jonthysell.com>
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

        public int? TranspositionTableSizeMB { get; private set; } = null;

        public MetricWeights MetricWeights { get; private set; } = null;

        public PonderDuringIdleType PonderDuringIdle { get; private set; } = PonderDuringIdleType.Disabled;

        public int MaxHelperThreads
        {
            get
            {
                // Hard min is 0, hard max is Environment.ProcessorCount - 1
#if DEBUG
                return 0;
#else
                return Math.Max(0, _maxHelperThreads.HasValue ? Math.Min(_maxHelperThreads.Value, Environment.ProcessorCount - 1) : Environment.ProcessorCount - 1);
#endif
            }
        }
        private int? _maxHelperThreads = null;

        #endregion

        public GameEngineConfig(Stream inputStream)
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
                        case "TranspositionTableSizeMB":
                            ParseTranspositionTableSizeMBValue(reader.ReadElementContentAsString());
                            break;
                        case "MetricWeights":
                            MetricWeights = MetricWeights.ReadMetricWeightsXml(reader.ReadSubtree());
                            break;
                        case "MaxHelperThreads":
                            ParseMaxHelperThreadsValue(reader.ReadElementContentAsString());
                            break;
                        case "PonderDuringIdle":
                            ParsePonderDuringIdleValue(reader.ReadElementContentAsString());
                            break;
                    }
                }
            }
        }

        private void ParseTranspositionTableSizeMBValue(string rawValue)
        {
            int intValue;
            if (int.TryParse(rawValue, out intValue))
            {
                TranspositionTableSizeMB = intValue;
            }
        }

        private void ParseMaxHelperThreadsValue(string rawValue)
        {
            int intValue;
            MaxHelperThreadsType enumValue;

            if (int.TryParse(rawValue, out intValue))
            {
                _maxHelperThreads = intValue;
            }
            else if (Enum.TryParse(rawValue, out enumValue))
            {
                switch (enumValue)
                {
                    case MaxHelperThreadsType.None:
                        _maxHelperThreads = 0;
                        break;
                    case MaxHelperThreadsType.Auto:
                        _maxHelperThreads = null;
                        break;

                }
            }
        }

        private void ParsePonderDuringIdleValue(string rawValue)
        {
            PonderDuringIdleType enumValue;
            if (Enum.TryParse(rawValue, out enumValue))
            {
                PonderDuringIdle = enumValue;
            }
        }

        public GameAI GetGameAI()
        {
            if (TranspositionTableSizeMB.HasValue)
            {
                return null != MetricWeights? new GameAI(MetricWeights, TranspositionTableSizeMB.Value) : new GameAI(TranspositionTableSizeMB.Value);
            }
            else if (null != MetricWeights)
            {
                return new GameAI(MetricWeights);
            }

            return new GameAI();
        }
    }

    public enum MaxHelperThreadsType
    {
        None,
        Auto,
    }

    public enum PonderDuringIdleType
    {
        Disabled,
        SingleThreaded,
        MultiThreaded
    }
}
