// 
// Profile.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017, 2018, 2019 Jon Thysell <http://jonthysell.com>
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
using System.Globalization;
using System.IO;
using System.Xml;

using Mzinga.Core;
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

        public ProfileRecord[] Records { get; private set; }

        public MetricWeights StartMetricWeights { get; private set; }

        public MetricWeights EndMetricWeights { get; private set; }

        public DateTime CreationTimestamp { get; private set; }

        public DateTime LastUpdatedTimestamp { get; private set; }

        public static Random Random
        {
            get
            {
                return _random ??= new Random();
            }
        }
        private static Random _random;

        public Profile(Guid id, string name, MetricWeights startMetricWeights, MetricWeights endMetricWeights)
        {
            Id = id;

            Name = name;

            Records = ProfileRecord.CreateRecords();

            StartMetricWeights = startMetricWeights ?? throw new ArgumentNullException(nameof(startMetricWeights));
            EndMetricWeights = endMetricWeights ?? throw new ArgumentNullException(nameof(endMetricWeights));

            CreationTimestamp = DateTime.Now;
            LastUpdatedTimestamp = DateTime.Now;
        }

        private Profile(Guid id, string name, int generation, Guid? parentA, Guid? parentB, MetricWeights startMetricWeights, MetricWeights endMetricWeights)
        {
            if (generation < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(generation));
            }

            if (!parentA.HasValue)
            {
                throw new ArgumentNullException(nameof(parentA));
            }

            if (!parentB.HasValue)
            {
                throw new ArgumentNullException(nameof(parentB));
            }

            Id = id;

            Name = name;

            Generation = generation;

            ParentA = parentA;
            ParentB = parentB;

            Records = ProfileRecord.CreateRecords();

            StartMetricWeights = startMetricWeights ?? throw new ArgumentNullException(nameof(startMetricWeights));
            EndMetricWeights = endMetricWeights ?? throw new ArgumentNullException(nameof(endMetricWeights));

            CreationTimestamp = DateTime.Now;
            LastUpdatedTimestamp = DateTime.Now;
        }

        private Profile(Guid id, string name, int generation, Guid? parentA, Guid? parentB, ProfileRecord[] records, MetricWeights startMetricWeights, MetricWeights endMetricWeights, DateTime creationTimestamp, DateTime lastUpdatedTimestamp)
        {
            if (generation < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(generation));
            }

            Id = id;
            Name = name;

            Generation = (!parentA.HasValue || !parentB.HasValue) ? 0 : generation;

            ParentA = parentA;
            ParentB = parentB;

            Records = records;

            StartMetricWeights = startMetricWeights ?? throw new ArgumentNullException(nameof(startMetricWeights));
            EndMetricWeights = endMetricWeights ?? throw new ArgumentNullException(nameof(endMetricWeights));

            CreationTimestamp = creationTimestamp;
            LastUpdatedTimestamp = lastUpdatedTimestamp;
        }

        public void UpdateRecord(int rating, GameResult result, ExpansionPieces expansionPieces)
        {
            if (rating < EloUtils.MinRating)
            {
                throw new ArgumentOutOfRangeException(nameof(rating));
            }

            Records[(int)expansionPieces].EloRating = rating;

            switch (result)
            {
                case GameResult.Loss:
                    Records[(int)expansionPieces].Losses++;
                    break;
                case GameResult.Draw:
                    Records[(int)expansionPieces].Draws++;
                    break;
                case GameResult.Win:
                    Records[(int)expansionPieces].Wins++;
                    break;
            }

            Update();
        }

        public void UpdateMetricWeights(MetricWeights startMetricWeights, MetricWeights endMetricWeights, ExpansionPieces expansionPieces)
        {
            StartMetricWeights = startMetricWeights ?? throw new ArgumentNullException(nameof(startMetricWeights));
            EndMetricWeights = endMetricWeights ?? throw new ArgumentNullException(nameof(endMetricWeights));

            Records[(int)expansionPieces].AutoTrains++;

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
                throw new ArgumentNullException(nameof(outputStream));
            }

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true
            };

            using XmlWriter writer = XmlWriter.Create(outputStream, settings);
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

            writer.WriteStartElement("Records");
            for (int i = 0; i < Records.Length; i++)
            {
                writer.WriteStartElement("Record");
                writer.WriteAttributeString("GameType", EnumUtils.GetExpansionPiecesString((ExpansionPieces)i));
                writer.WriteAttributeString("EloRating", Records[i].EloRating.ToString());
                writer.WriteAttributeString("Wins", Records[i].Wins.ToString());
                writer.WriteAttributeString("Losses", Records[i].Losses.ToString());
                writer.WriteAttributeString("Draws", Records[i].Draws.ToString());
                writer.WriteAttributeString("AutoTrains", Records[i].AutoTrains.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Creation");
            writer.WriteValue(CreationTimestamp);
            writer.WriteEndElement();

            writer.WriteStartElement("LastUpdated");
            writer.WriteValue(LastUpdatedTimestamp);
            writer.WriteEndElement();

            StartMetricWeights.WriteMetricWeightsXml(writer, "StartMetricWeights");

            EndMetricWeights.WriteMetricWeightsXml(writer, "EndMetricWeights");

            writer.WriteEndElement(); // </Profile>
        }

        public override string ToString()
        {
            return Name;
        }

        public static Profile ReadXml(Stream inputStream)
        {
            if (null == inputStream)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            Guid id = Guid.Empty;
            string name = null;
            int generation = 0;

            Guid? parentA = null;
            Guid? parentB = null;

            ProfileRecord[] records = ProfileRecord.CreateRecords();

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
                            case "Record":
                                {
                                    if (EnumUtils.TryParseExpansionPieces(reader["GameType"], out ExpansionPieces ep))
                                    {
                                        int.TryParse(reader["EloRating"], out records[(int)ep].EloRating);
                                        int.TryParse(reader["Wins"], out records[(int)ep].Wins);
                                        int.TryParse(reader["Losses"], out records[(int)ep].Losses);
                                        int.TryParse(reader["Draws"], out records[(int)ep].Draws);
                                        int.TryParse(reader["AutoTrains"], out records[(int)ep].AutoTrains);
                                    }
                                }
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

            return new Profile(id, name ?? GenerateName(id), generation, parentA, parentB, records, startMetricWeights, endMetricWeights ?? startMetricWeights, creationTimestamp, lastUpdateTimestamp);
        }

        public static Profile Generate(double minWeight, double maxWeight)
        {
            MetricWeights startMetricWeights = GenerateMetricWeights(minWeight, maxWeight);
            MetricWeights endMetricWeights = GenerateMetricWeights(minWeight, maxWeight);

            Guid id = Guid.NewGuid();

            string name = GenerateName(id);

            return new Profile(id, name, startMetricWeights, endMetricWeights);
        }

        public static Profile Mate(Profile parentA, Profile parentB, double minMix, double maxMix)
        {
            if (null == parentA)
            {
                throw new ArgumentNullException(nameof(parentA));
            }

            if (null == parentB)
            {
                throw new ArgumentNullException(nameof(parentB));
            }

            if (minMix > maxMix)
            {
                throw new ArgumentOutOfRangeException(nameof(minMix));
            }

            Guid id = Guid.NewGuid();

            string name = GenerateName(id);

            int generation = Math.Max(parentA.Generation, parentB.Generation) + 1;

            MetricWeights startMetricWeights = MixMetricWeights(parentA.StartMetricWeights.GetNormalized(), parentB.StartMetricWeights.GetNormalized(), minMix, maxMix);
            MetricWeights endMetricWeights = MixMetricWeights(parentA.EndMetricWeights.GetNormalized(), parentB.EndMetricWeights.GetNormalized(), minMix, maxMix);

            DateTime creationTimestamp = DateTime.Now;

            return new Profile(id, name, generation, parentA.Id, parentB.Id, startMetricWeights, endMetricWeights);
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
                value *= (minMix + (Random.NextDouble() * Math.Abs(maxMix - minMix)));
                mw.Set(bugType, bugTypeWeight, value);
            });

            return mw;
        }

        private static string GenerateName(Guid id)
        {
            string shortId = id.ToString().Substring(0, _syllables.Length);

            string name = "";

            for (int i = 0; i < shortId.Length; i++)
            {
                int j = int.Parse(shortId[i].ToString(), NumberStyles.HexNumber) % _syllables[i].Length;
                name += _syllables[i][j];
            }

            return name;
        }

        private static string[][] _syllables = new string[][]
        {
            new string[] { "Fu", "I", "Je", "Ki", "Ku", "M", "Ma", "Mo", "Na", "Ng", "Sa", "Si", "Ta", "Te", "Ti", "Zu" },
            new string[] { "", "ba", "ha", "hi", "ka", "ki", "ku", "li", "ma", "na", "ni", "si", "ta", "ti", "wa", "ya" },
            new string[] { "", "kwa", "mba", "sha", },
            new string[] { "ba", "go", "ji", "ita", "la", "mi", "ne", "ni", "nyi", "ra", "ri", "si", "tu", "we", "ye", "za" },
        };
    }

    public class ProfileRecord
    {
        public int EloRating = EloUtils.DefaultRating;

        public int Wins = 0;
        public int Losses = 0;
        public int Draws = 0;

        public int AutoTrains = 0;

        public int TotalGames
        {
            get
            {
                return Wins + Losses + Draws;
            }
        }

        public static ProfileRecord[] CreateRecords()
        {
            ProfileRecord[] records = new ProfileRecord[EnumUtils.NumGameTypes];
            for (int i = 0; i < records.Length; i++)
            {
                records[i] = new ProfileRecord();
            }
            return records;
        }
    }
}