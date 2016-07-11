// 
// Profile.cs
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
using System.Xml;

using Mzinga.Core;
using Mzinga.Core.AI;

namespace Mzinga.Trainer
{
    public class Profile
    {
        public string Nickname
        {
            get
            {
                return String.Format("{0}({1})", Id.ToString().Substring(0, 8), EloRating);
            }
        }

        public Guid Id { get; private set; }

        public int Generation { get; private set; }

        public int EloRating { get; private set; }

        public MetricWeights MetricWeights { get; private set; }

        public DateTime CreationTimestamp { get; private set; }

        public DateTime LastUpdatedTimestamp { get; private set; }

        public static Random Random
        {
            get
            {
                if (null == _random)
                {
                    _random = new Random();
                }
                return _random;
            }
        }
        private static Random _random;

        public Profile(Guid id, int generation, int eloRating, MetricWeights metricWeights, DateTime creationTimestamp, DateTime lastUpdatedTimestamp)
        {
            if (generation < 0)
            {
                throw new ArgumentOutOfRangeException("generation");
            }

            if (eloRating < EloUtils.MinRating)
            {
                throw new ArgumentOutOfRangeException("eloRating");
            }

            if (null == metricWeights)
            {
                throw new ArgumentNullException("metricWeights");
            }

            Id = id;
            Generation = generation;

            EloRating = eloRating;

            MetricWeights = metricWeights;

            CreationTimestamp = creationTimestamp;
            LastUpdatedTimestamp = lastUpdatedTimestamp;
        }

        public void UpdateRating(int rating)
        {
            if (rating < EloUtils.MinRating)
            {
                throw new ArgumentOutOfRangeException("rating");
            }

            EloRating = rating;

            Update();
        }

        public void Update()
        {
            LastUpdatedTimestamp = DateTime.Now;
        }

        public void WriteXml(Stream outputStream)
        {
            if (null == outputStream)
            {
                throw new ArgumentNullException("outputStream");
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create(outputStream, settings))
            {
                writer.WriteStartElement("Profile");

                writer.WriteStartElement("Id");
                writer.WriteValue(Id.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("Generation");
                writer.WriteValue(Generation);
                writer.WriteEndElement();

                writer.WriteStartElement("EloRating");
                writer.WriteValue(EloRating);
                writer.WriteEndElement();

                writer.WriteStartElement("Creation");
                writer.WriteValue(CreationTimestamp);
                writer.WriteEndElement();

                writer.WriteStartElement("LastUpdated");
                writer.WriteValue(LastUpdatedTimestamp);
                writer.WriteEndElement();

                writer.WriteStartElement("MetricWeights");

                for (int playerInt = 0; playerInt < MetricWeights.NumPlayers; playerInt++ )
                {
                    Player player = (Player)playerInt;
                    for (int playerWeightInt = 0; playerWeightInt < MetricWeights.NumPlayerWeights; playerWeightInt++)
                    {
                        PlayerWeight playerWeight = (PlayerWeight)playerWeightInt;
                        string key = MetricWeights.GetKeyName(player, playerWeight);
                        double value = MetricWeights.Get(player, playerWeight);

                        writer.WriteStartElement(key);
                        writer.WriteValue(value);
                        writer.WriteEndElement();
                    }

                    for (int bugTypeInt = 0; bugTypeInt < EnumUtils.NumBugTypes; bugTypeInt++)
                    {
                        BugType bugType = (BugType)bugTypeInt;
                        for (int bugTypeWeightInt = 0; bugTypeWeightInt < MetricWeights.NumBugTypeWeights; bugTypeWeightInt++)
                        {
                            BugTypeWeight bugTypeWeight = (BugTypeWeight)bugTypeWeightInt;
                            string key = MetricWeights.GetKeyName(player, bugType, bugTypeWeight);
                            double value = MetricWeights.Get(player, bugType, bugTypeWeight);

                            writer.WriteStartElement(key);
                            writer.WriteValue(value);
                            writer.WriteEndElement();
                        }
                    }
                }

                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }

        public static Profile ReadXml(Stream inputStream)
        {
            if (null == inputStream)
            {
                throw new ArgumentNullException("inputStream");
            }

            Guid id = Guid.Empty;
            int eloRating = EloUtils.DefaultRating;
            int generation = 0;

            MetricWeights metricWeights = null;

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
                            case "Generation":
                                generation = reader.ReadElementContentAsInt();
                                break;
                            case "EloRating":
                                eloRating = reader.ReadElementContentAsInt();
                                break;
                            case "Creation":
                                creationTimestamp = reader.ReadElementContentAsDateTime();
                                break;
                            case "LastUpdated":
                                lastUpdateTimestamp = reader.ReadElementContentAsDateTime();
                                break;
                            case "MetricWeights":
                                metricWeights = ReadMetricWeightsXml(reader.ReadSubtree());
                                break;
                        }
                    }
                }
            }

            return new Profile(id, generation, eloRating, metricWeights, creationTimestamp, lastUpdateTimestamp);
        }

        private static MetricWeights ReadMetricWeightsXml(XmlReader xmlReader)
        {
            if (null == xmlReader)
            {
                throw new ArgumentNullException("xmlReader");
            }

            MetricWeights mw = new MetricWeights();

            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement() && xmlReader.Name != "MetricWeights")
                {
                    string key = xmlReader.Name;
                    double value = xmlReader.ReadElementContentAsDouble();

                    if (key == "DrawScore")
                    {
                        mw.DrawScore = value;
                    }
                    else
                    {
                        Player player;
                        PlayerWeight playerWeight;
                        BugType bugType;
                        BugTypeWeight bugTypeWeight;

                        if (MetricWeights.TryParseKeyName(key, out player, out playerWeight))
                        {
                            mw.Set(player, playerWeight, value);
                        }
                        else if (MetricWeights.TryParseKeyName(key, out player, out bugType, out bugTypeWeight))
                        {
                            mw.Set(player, bugType, bugTypeWeight, value);
                        }
                    }
                }
            }

            return mw;
        }

        public static List<Profile> Generate(int count, double minWeight, double maxWeight)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            List<Profile> profiles = new List<Profile>();

            for (int i = 0; i < count; i++)
            {
                Profile p = Generate(minWeight, maxWeight);
                profiles.Add(p);
            }

            return profiles;
        }

        public static Profile Generate(double minWeight, double maxWeight)
        {
            Guid id = Guid.NewGuid();
            int eloRating = EloUtils.DefaultRating;
            int generation = 0;

            MetricWeights metricWeights = GenerateMetricWeights(minWeight, maxWeight);

            DateTime creationTimestamp = DateTime.Now;

            return new Profile(id, generation, eloRating, metricWeights, creationTimestamp, DateTime.Now);
        }

        private static MetricWeights GenerateMetricWeights(double minWeight, double maxWeight)
        {
            MetricWeights mw = new MetricWeights();

            for (int playerInt = 0; playerInt < MetricWeights.NumPlayers; playerInt++)
            {
                Player player = (Player)playerInt;
                for (int playerWeightInt = 0; playerWeightInt < MetricWeights.NumPlayerWeights; playerWeightInt++)
                {
                    PlayerWeight playerWeight = (PlayerWeight)playerWeightInt;

                    double value = minWeight + (Random.NextDouble() * (maxWeight - minWeight));

                    mw.Set(player, playerWeight, value);
                }

                for (int bugTypeInt = 0; bugTypeInt < EnumUtils.NumBugTypes; bugTypeInt++)
                {
                    BugType bugType = (BugType)bugTypeInt;
                    for (int bugTypeWeightInt = 0; bugTypeWeightInt < MetricWeights.NumBugTypeWeights; bugTypeWeightInt++)
                    {
                        BugTypeWeight bugTypeWeight = (BugTypeWeight)bugTypeWeightInt;

                        double value = minWeight + (Random.NextDouble() * (maxWeight - minWeight));

                        mw.Set(player, bugType, bugTypeWeight, value);
                    }
                }
            }

            return mw;
        }
    }
}