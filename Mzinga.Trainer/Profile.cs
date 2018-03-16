// 
// Profile.cs
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
using System.IO;
using System.Xml;

using Mzinga.Core.AI;

namespace Mzinga.Trainer
{
    public class Profile
    {
        public Guid Id { get; private set; }

        public string Name
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_name) ? _name : Id.ToString().Substring(0, 8);
            }
            set
            {
                _name = value;
            }
        }
        private string _name = null;

        public int Generation { get; private set; } = 0;

        public Guid? ParentA { get; private set; } = null;

        public Guid? ParentB { get; private set; } = null;

        public int EloRating { get; private set; } = EloUtils.DefaultRating;

        public MetricWeights StartMetricWeights { get; private set; }

        public MetricWeights EndMetricWeights { get; private set; }

        public DateTime CreationTimestamp { get; private set; }

        public DateTime LastUpdatedTimestamp { get; private set; }

        public int TotalGames
        {
            get
            {
                return Wins + Losses + Draws;
            }
        }

        public int Wins { get; private set; } = 0;

        public int Losses { get; private set; } = 0;

        public int Draws { get; private set; } = 0;

        public static Random Random
        {
            get
            {
                return _random ?? (_random = new Random());
            }
        }
        private static Random _random;

        public Profile(Guid id, MetricWeights startMetricWeights, MetricWeights endMetricWeights)
        {
            if (null == startMetricWeights)
            {
                throw new ArgumentNullException("startMetricWeights");
            }

            if (null == endMetricWeights)
            {
                throw new ArgumentNullException("endMetricWeights");
            }

            Id = id;

            StartMetricWeights = startMetricWeights;
            EndMetricWeights = endMetricWeights;

            CreationTimestamp = DateTime.Now;
            LastUpdatedTimestamp = DateTime.Now;
        }

        public Profile(Guid id, int generation, Guid? parentA, Guid? parentB, int eloRating, MetricWeights startMetricWeights, MetricWeights endMetricWeights)
        {
            if (generation < 0)
            {
                throw new ArgumentOutOfRangeException("generation");
            }

            if (!parentA.HasValue)
            {
                throw new ArgumentNullException("parentA");
            }

            if (!parentB.HasValue)
            {
                throw new ArgumentNullException("parentB");
            }

            if (eloRating < EloUtils.MinRating)
            {
                throw new ArgumentOutOfRangeException("eloRating");
            }

            if (null == startMetricWeights)
            {
                throw new ArgumentNullException("startMetricWeights");
            }

            if (null == endMetricWeights)
            {
                throw new ArgumentNullException("endMetricWeights");
            }

            Id = id;

            Generation = generation;

            ParentA = parentA;
            ParentB = parentB;

            EloRating = eloRating;

            StartMetricWeights = startMetricWeights;
            EndMetricWeights = endMetricWeights;

            CreationTimestamp = DateTime.Now;
            LastUpdatedTimestamp = DateTime.Now;
        }

        private Profile(Guid id, string name, int generation, Guid? parentA, Guid? parentB, int eloRating, int wins, int losses, int draws, MetricWeights startMetricWeights, MetricWeights endMetricWeights, DateTime creationTimestamp, DateTime lastUpdatedTimestamp)
        {
            if (generation < 0)
            {
                throw new ArgumentOutOfRangeException("generation");
            }

            if (eloRating < EloUtils.MinRating)
            {
                throw new ArgumentOutOfRangeException("eloRating");
            }

            if (wins < 0)
            {
                throw new ArgumentOutOfRangeException("wins");
            }

            if (losses < 0)
            {
                throw new ArgumentOutOfRangeException("losses");
            }

            if (draws < 0)
            {
                throw new ArgumentOutOfRangeException("draws");
            }

            if (null == startMetricWeights)
            {
                throw new ArgumentNullException("startMetricWeights");
            }

            if (null == endMetricWeights)
            {
                throw new ArgumentNullException("endMetricWeights");
            }

            Id = id;
            Name = name;

            Generation = (!parentA.HasValue || !parentB.HasValue) ? 0 : generation;

            ParentA = parentA;
            ParentB = parentB;

            EloRating = eloRating;

            Wins = wins;
            Losses = losses;
            Draws = draws;

            StartMetricWeights = startMetricWeights;
            EndMetricWeights = endMetricWeights;

            CreationTimestamp = creationTimestamp;
            LastUpdatedTimestamp = lastUpdatedTimestamp;
        }

        public void UpdateRecord(int rating, GameResult result)
        {
            if (rating < EloUtils.MinRating)
            {
                throw new ArgumentOutOfRangeException("rating");
            }

            EloRating = rating;

            switch (result)
            {
                case GameResult.Loss:
                    Losses++;
                    break;
                case GameResult.Draw:
                    Draws++;
                    break;
                case GameResult.Win:
                    Wins++;
                    break;
            }

            Update();
        }

        private void Update()
        {
            LastUpdatedTimestamp = DateTime.Now;
        }

        public void WriteXml(Stream outputStream)
        {
            if (null == outputStream)
            {
                throw new ArgumentNullException("outputStream");
            }

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true
            };

            using (XmlWriter writer = XmlWriter.Create(outputStream, settings))
            {
                writer.WriteStartElement("Profile");

                writer.WriteStartElement("Id");
                writer.WriteValue(Id.ToString());
                writer.WriteEndElement();

                if (!string.IsNullOrWhiteSpace(_name))
                {
                    writer.WriteStartElement("Name");
                    writer.WriteValue(_name.Trim());
                    writer.WriteEndElement();
                }

                writer.WriteStartElement("Generation");
                writer.WriteValue(Generation);
                writer.WriteEndElement();

                if (ParentA.HasValue)
                {
                    writer.WriteStartElement("ParentA");
                    writer.WriteValue(ParentA.ToString());
                    writer.WriteEndElement();
                }

                if (ParentB.HasValue)
                {
                    writer.WriteStartElement("ParentB");
                    writer.WriteValue(ParentB.ToString());
                    writer.WriteEndElement();
                }


                writer.WriteStartElement("EloRating");
                writer.WriteValue(EloRating);
                writer.WriteEndElement();

                writer.WriteStartElement("Wins");
                writer.WriteValue(Wins);
                writer.WriteEndElement();

                writer.WriteStartElement("Losses");
                writer.WriteValue(Losses);
                writer.WriteEndElement();

                writer.WriteStartElement("Draws");
                writer.WriteValue(Draws);
                writer.WriteEndElement();

                writer.WriteStartElement("Creation");
                writer.WriteValue(CreationTimestamp);
                writer.WriteEndElement();

                writer.WriteStartElement("LastUpdated");
                writer.WriteValue(LastUpdatedTimestamp);
                writer.WriteEndElement();

                writer.WriteStartElement("StartMetricWeights");

                MetricWeights.IterateOverWeights((bugType, bugTypeWeight) =>
                {
                    string key = MetricWeights.GetKeyName(bugType, bugTypeWeight);
                    double value = StartMetricWeights.Get(bugType, bugTypeWeight);

                    writer.WriteStartElement(key);
                    writer.WriteValue(value);
                    writer.WriteEndElement();
                });

                writer.WriteEndElement(); // </StartMetricWeights>

                writer.WriteStartElement("EndMetricWeights");

                MetricWeights.IterateOverWeights((bugType, bugTypeWeight) =>
                {
                    string key = MetricWeights.GetKeyName(bugType, bugTypeWeight);
                    double value = EndMetricWeights.Get(bugType, bugTypeWeight);

                    writer.WriteStartElement(key);
                    writer.WriteValue(value);
                    writer.WriteEndElement();
                });

                writer.WriteEndElement(); // </EndMetricWeights>

                writer.WriteEndElement(); // </Profile>
            }
        }

        public static Profile ReadXml(Stream inputStream)
        {
            if (null == inputStream)
            {
                throw new ArgumentNullException("inputStream");
            }

            Guid id = Guid.Empty;
            string name = null;
            int generation = 0;

            Guid? parentA = null;
            Guid? parentB = null;

            int eloRating = EloUtils.DefaultRating;

            int wins = 0;
            int losses = 0;
            int draws = 0;

            MetricWeights startMetricWeights = null;
            MetricWeights endMetricWeights = null;

            DateTime creationTimestamp = DateTime.Now;
            DateTime lastUpdateTimestamp = creationTimestamp;

            using (XmlReader reader = XmlReader.Create(inputStream))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "Id":
                                id = Guid.Parse(reader.ReadElementContentAsString());
                                break;
                            case "Name":
                                name = reader.ReadElementContentAsString();
                                break;
                            case "Generation":
                                generation = reader.ReadElementContentAsInt();
                                break;
                            case "ParentA":
                                parentA = Guid.Parse(reader.ReadElementContentAsString());
                                break;
                            case "ParentB":
                                parentB = Guid.Parse(reader.ReadElementContentAsString());
                                break;
                            case "EloRating":
                                eloRating = reader.ReadElementContentAsInt();
                                break;
                            case "Wins":
                                wins = reader.ReadElementContentAsInt();
                                break;
                            case "Losses":
                                losses = reader.ReadElementContentAsInt();
                                break;
                            case "Draws":
                                draws = reader.ReadElementContentAsInt();
                                break;
                            case "Creation":
                                creationTimestamp = reader.ReadElementContentAsDateTime();
                                break;
                            case "LastUpdated":
                                lastUpdateTimestamp = reader.ReadElementContentAsDateTime();
                                break;
                            case "MetricWeights":
                            case "StartMetricWeights":
                                startMetricWeights = MetricWeights.ReadMetricWeightsXml(reader.ReadSubtree());
                                break;
                            case "EndMetricWeights":
                                endMetricWeights = MetricWeights.ReadMetricWeightsXml(reader.ReadSubtree());
                                break;
                        }
                    }
                }
            }

            return new Profile(id, name, generation, parentA, parentB, eloRating, wins, losses, draws, startMetricWeights, endMetricWeights ?? startMetricWeights, creationTimestamp, lastUpdateTimestamp);
        }

        public static Profile Generate(double minWeight, double maxWeight)
        {
            MetricWeights startMetricWeights = GenerateMetricWeights(minWeight, maxWeight);
            MetricWeights endMetricWeights = GenerateMetricWeights(minWeight, maxWeight);
            return new Profile(Guid.NewGuid(), startMetricWeights, endMetricWeights);
        }

        public static Profile Mate(Profile parentA, Profile parentB, double minMix, double maxMix)
        {
            if (null == parentA)
            {
                throw new ArgumentNullException("parentA");
            }

            if (null == parentB)
            {
                throw new ArgumentNullException("parentB");
            }

            if (minMix > maxMix)
            {
                throw new ArgumentOutOfRangeException("minMix");
            }

            Guid id = Guid.NewGuid();
            int eloRating = EloUtils.DefaultRating;
            int generation = Math.Max(parentA.Generation, parentB.Generation) + 1;

            MetricWeights startMetricWeights = MixMetricWeights(parentA.StartMetricWeights.GetNormalized(), parentB.StartMetricWeights.GetNormalized(), minMix, maxMix);
            MetricWeights endMetricWeights = MixMetricWeights(parentA.EndMetricWeights.GetNormalized(), parentB.EndMetricWeights.GetNormalized(), minMix, maxMix);

            DateTime creationTimestamp = DateTime.Now;

            return new Profile(id, generation, parentA.Id, parentB.Id, eloRating, startMetricWeights, endMetricWeights);
        }

        private static MetricWeights GenerateMetricWeights(double minWeight, double maxWeight)
        {
            MetricWeights mw = new MetricWeights();

            MetricWeights.IterateOverWeights((bugType, bugTypeWeight) =>
            {
                double value = minWeight + (Random.NextDouble() * (maxWeight - minWeight));
                mw.Set(bugType, bugTypeWeight, value);
            });

            return mw;
        }

        private static MetricWeights MixMetricWeights(MetricWeights mwA, MetricWeights mwB, double minMix, double maxMix)
        {
            MetricWeights mw = new MetricWeights();

            MetricWeights.IterateOverWeights((bugType, bugTypeWeight) =>
            {
                double value = 0.5 * (mwA.Get(bugType, bugTypeWeight) + mwB.Get(bugType, bugTypeWeight));
                if (value == 0.0)
                {
                    value = -0.01 + (Random.NextDouble() * 0.02);
                }
                value = value * (minMix + (Random.NextDouble() * Math.Abs(maxMix - minMix)));
                mw.Set(bugType, bugTypeWeight, value);
            });

            return mw;
        }
    }
}