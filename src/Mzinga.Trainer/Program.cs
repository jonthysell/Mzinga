﻿// 
// Program.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017, 2018, 2019, 2021 Jon Thysell <http://jonthysell.com>
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
using System.Text;

using Mzinga.Core;

namespace Mzinga.Trainer
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            try
            {
                Console.WriteLine($"{AppInfo.Name} v{AppInfo.Version}");
                Console.WriteLine();

                if (args.Length == 0)
                {
                    ShowHelp();
                }
                else
                {
                    Trainer t = new Trainer();

                    Command cmd = ParseArguments(args, t.TrainerSettings);

                    switch (cmd)
                    {
                        case Command.Battle:
                            t.Battle();
                            break;
                        case Command.BattleRoyale:
                            t.BattleRoyale();
                            break;
                        case Command.Cull:
                            t.Cull();
                            break;
                        case Command.Enumerate:
                            t.Enumerate();
                            break;
                        case Command.Analyze:
                            t.Analyze();
                            break;
                        case Command.Generate:
                            t.Generate();
                            break;
                        case Command.Lifecycle:
                            t.Lifecycle();
                            break;
                        case Command.Mate:
                            t.Mate();
                            break;
                        case Command.Tournament:
                            t.Tournament();
                            break;
                        case Command.AutoTrain:
                            t.AutoTrain();
                            break;
                        case Command.Top:
                            t.Top();
                            break;
                        case Command.MergeTop:
                            t.MergeTop();
                            break;
                        case Command.ExportAI:
                            t.ExportAI();
                            break;
                        case Command.BuildInitialTables:
                            t.BuildInitialTables();
                            break;
                        default:
                            ShowHelp();
                            break;
                    }
                }
            }
            catch (AggregateException ex)
            {
                PrintException(ex);
                foreach (Exception innerEx in ex.InnerExceptions)
                {
                    PrintException(innerEx);
                }
            }
            catch (Exception ex)
            {
                PrintException(ex);
            }
        }

        static void PrintException(Exception ex)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine();
            Console.Error.WriteLine("Error: {0}", ex.Message);
            Console.Error.WriteLine(ex.StackTrace);

            Console.ForegroundColor = oldColor;

            if (null != ex.InnerException)
            {
                PrintException(ex.InnerException);
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("Mzinga.Trainer.exe [command] ([parametername] [parametervalue]...)");
            Console.WriteLine();

            Console.WriteLine("Example:");
            Console.WriteLine("Mzinga.Trainer.exe enumerate -ProfilesPath c:\\profiles\\");
            Console.WriteLine();

            Console.WriteLine("Commands:");

            Console.WriteLine("battle                 Fight a single battle between two profiles");
            Console.WriteLine("battleroyale           Fight every profile against each other");
            Console.WriteLine("cull                   Delete the lowest ranking profiles");
            Console.WriteLine("enumerate              List all of the profiles");
            Console.WriteLine("analyze                Analyze all of the profiles");
            Console.WriteLine("generate               Create new random profiles");
            Console.WriteLine("lifecycle              Battle, cull, mate cycle for profiles");
            Console.WriteLine("mate                   Mate every profile with each other");
            Console.WriteLine("tournament             Fight an single elimination tournament");
            Console.WriteLine("autotrain              Train a single profile against itself");
            Console.WriteLine("top                    List the top profiles for each GameType");
            Console.WriteLine("mergetop               Merge the top profiles from each GameType");
            Console.WriteLine("exportai               Export the current engine AI as profiles");
            Console.WriteLine("buildinitialtables     Build and export InitialTranspositionTable");

            Console.WriteLine();

            Console.WriteLine("Parameters:");

            Console.WriteLine("-ProfilesPath          Where the profiles are stored");
            Console.WriteLine("-WhiteProfilePath      The white profile in a single battle");
            Console.WriteLine("-BlackProfilePath      The black profile in a single battle");
            Console.WriteLine("-CullKeepCount         How many to profiles to keep when culling");
            Console.WriteLine("-GenerateCount         How many profiles to generate");
            Console.WriteLine("-GenerateMinWeight     The minimum weight value for random profiles");
            Console.WriteLine("-GenerateMaxWeight     The maximum weight value for random profiles");
            Console.WriteLine("-LifecycleGenerations  The number of generations to run");
            Console.WriteLine("-LifecycleBattles      The number/type of battles in each generation");
            Console.WriteLine("-MaxBattles            The max number of battles in a battle royale");
            Console.WriteLine("-MaxConcurrentBattles  The max number of battles at the same time");
            Console.WriteLine("-BattleShuffleProfiles Whether or not to have profiles fight in random order");
            Console.WriteLine("-BulkBattleTimeLimit   The max time for tournaments / battle royales");
            Console.WriteLine("-ProvisionalRules      Whether or not to use provisional rules");
            Console.WriteLine("-ProvisionalGameCount  The number of games a profile stays provisional");
            Console.WriteLine("-MaxDraws              The max number of times to retry battles that end in a draw");
            Console.WriteLine("-MateMinMix            The min multiplier to mix up weights in children profiles");
            Console.WriteLine("-MateMaxMix            The max multiplier to mix up weights in children profiles");
            Console.WriteLine("-MateParentCount       The number of profiles to mate");
            Console.WriteLine("-MateShuffleParents    Whether or not to have random parents mate");
            Console.WriteLine("-TransTableSize        The maximum size of each AI's transposition table in MB");
            Console.WriteLine("-MaxDepth              The maximum ply depth of the AI search");
            Console.WriteLine("-TurnMaxTime           The maximum time to let the AI think on its turn");
            Console.WriteLine("-BattleTimeLimit       The maximum time to let a battle run before declaring a draw");
            Console.WriteLine("-GameType              Base,Base+M,Base+L,Base+P,Base+ML,Base+MP,Base+LP,Base+MLP");
            Console.WriteLine("-TargetProfilePath     The target profile");
            Console.WriteLine("-FindPuzzleCandidates  Output puzzle candidates during battles");
            Console.WriteLine("-MaxHelperThreads      The maximum helper threads for each AI to use");
            Console.WriteLine("-TopCount              The number of profiles to return when calling top");
            Console.WriteLine("-AllGameTypes          Run the specifcied command through every game type");
            Console.WriteLine("-ProvisionalFirst      Prioritize battles with at least one provisional profile");
            Console.WriteLine("-InitialTableDepth     The ply depth to play to build initial tables");
            Console.WriteLine();
        }

        static Command ParseArguments(string[] args, TrainerSettings trainerSettings)
        {
            if (null == args || args.Length == 0)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (null == trainerSettings)
            {
                throw new ArgumentNullException(nameof(trainerSettings));
            }

            Command cmd = Command.Unknown;
            switch (args[0].ToLower())
            {
                case "b":
                case "battle":
                    cmd = Command.Battle;
                    break;
                case "br":
                case "battleroyale":
                    cmd = Command.BattleRoyale;
                    break;
                case "c":
                case "cull":
                    cmd = Command.Cull;
                    break;
                case "e":
                case "enumerate":
                    cmd = Command.Enumerate;
                    break;
                case "a":
                case "analyze":
                    cmd = Command.Analyze;
                    break;
                case "g":
                case "generate":
                    cmd = Command.Generate;
                    break;
                case "l":
                case "lifecycle":
                    cmd = Command.Lifecycle;
                    break;
                case "m":
                case "mate":
                    cmd = Command.Mate;
                    break;
                case "t":
                case "tournament":
                    cmd = Command.Tournament;
                    break;
                case "at":
                case "autotrain":
                    cmd = Command.AutoTrain;
                    break;
                case "top":
                    cmd = Command.Top;
                    break;
                case "mt":
                case "mergetop":
                    cmd = Command.MergeTop;
                    break;
                case "ea":
                case "exportai":
                    cmd = Command.ExportAI;
                    break;
                case "bit":
                case "buildinitialtables":
                    cmd = Command.BuildInitialTables;
                    break;
            }

            if (cmd == Command.Unknown)
            {
                throw new Exception(string.Format("Unknown command: {0}", args[0]));
            }

            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i].Substring(1).ToLower())
                {
                    case "pp":
                    case "profilespath":
                        trainerSettings.ProfilesPath = args[++i];
                        break;
                    case "wpp":
                    case "whiteprofilepath":
                        trainerSettings.WhiteProfilePath = args[++i];
                        break;
                    case "bpp":
                    case "blackprofilepath":
                        trainerSettings.BlackProfilePath = args[++i];
                        break;
                    case "ckc":
                    case "cullkeepcount":
                        trainerSettings.CullKeepCount = int.Parse(args[++i]);
                        break;
                    case "gc":
                    case "generatecount":
                        trainerSettings.GenerateCount = int.Parse(args[++i]);
                        break;
                    case "gminw":
                    case "generateminweight":
                        trainerSettings.GenerateMinWeight = double.Parse(args[++i]);
                        break;
                    case "gmaxw":
                    case "generatemaxweight":
                        trainerSettings.GenerateMaxWeight = double.Parse(args[++i]);
                        break;
                    case "lg":
                    case "lifecyclegenerations":
                        trainerSettings.LifecycleGenerations = int.Parse(args[++i]);
                        break;
                    case "lb":
                    case "lifecyclebattles":
                        trainerSettings.LifecycleBattles = int.Parse(args[++i]);
                        break;
                    case "mb":
                    case "maxbattles":
                        trainerSettings.MaxBattles = int.Parse(args[++i]);
                        break;
                    case "mcb":
                    case "maxconcurrentbattles":
                        trainerSettings.MaxConcurrentBattles = int.Parse(args[++i]);
                        break;
                    case "bsp":
                    case "battleshuffleprofiles":
                        trainerSettings.BattleShuffleProfiles = bool.Parse(args[++i]);
                        break;
                    case "mdraws":
                    case "maxdraws":
                        trainerSettings.MaxDraws = int.Parse(args[++i]);
                        break;
                    case "bbtl":
                    case "bulkbattletimelimit":
                        trainerSettings.BulkBattleTimeLimit = TimeSpan.Parse(args[++i]);
                        break;
                    case "pr":
                    case "provisionalrules":
                        trainerSettings.ProvisionalRules = bool.Parse(args[++i]);
                        break;
                    case "pgc":
                    case "provisionalgamecount":
                        trainerSettings.ProvisionalGameCount = int.Parse(args[++i]);
                        break;
                    case "mminm":
                    case "mateminmix":
                        trainerSettings.MateMinMix = double.Parse(args[++i]);
                        break;
                    case "mmaxm":
                    case "matemaxmix":
                        trainerSettings.MateMaxMix = double.Parse(args[++i]);
                        break;
                    case "mpc":
                    case "mateparentcount":
                        trainerSettings.MateParentCount = int.Parse(args[++i]);
                        break;
                    case "msp":
                    case "mateshuffleparents":
                        trainerSettings.MateShuffleParents = bool.Parse(args[++i]);
                        break;
                    case "tts":
                    case "TransTableSize":
                        trainerSettings.TransTableSize = int.Parse(args[++i]);
                        break;
                    case "mdepth":
                    case "maxdepth":
                        trainerSettings.MaxDepth = int.Parse(args[++i]);
                        break;
                    case "tmt":
                    case "turnmaxtime":
                        trainerSettings.TurnMaxTime = TimeSpan.Parse(args[++i]);
                        break;
                    case "btl":
                    case "battletimelimit":
                        trainerSettings.BattleTimeLimit = TimeSpan.Parse(args[++i]);
                        break;
                    case "gt":
                    case "gametype":
                        trainerSettings.GameType = EnumUtils.ParseExpansionPieces(args[++i]);
                        break;
                    case "tpp":
                    case "targetprofilepath":
                        trainerSettings.TargetProfilePath = args[++i];
                        break;
                    case "findpuzzlecandidates":
                    case "fpc":
                        trainerSettings.FindPuzzleCandidates = bool.Parse(args[++i]);
                        break;
                    case "maxhelperthreads":
                    case "mht":
                        trainerSettings.MaxHelperThreads = int.Parse(args[++i]);
                        break;
                    case "topcount":
                    case "tc":
                        trainerSettings.TopCount = int.Parse(args[++i]);
                        break;
                    case "agt":
                    case "allgametypes":
                        trainerSettings.AllGameTypes = bool.Parse(args[++i]);
                        break;
                    case "provisionalfirst":
                    case "pf":
                        trainerSettings.ProvisionalFirst = bool.Parse(args[++i]);
                        break;
                    case "itd":
                    case "initialtabledepth":
                        trainerSettings.InitialTableDepth = int.Parse(args[++i]);
                        break;
                    default:
                        throw new Exception(string.Format("Unknown parameter: {0}", args[i]));
                }
            }

            return cmd;
        }
    }

    public enum Command
    {
        Unknown = -1,
        Battle,
        BattleRoyale,
        Cull,
        Enumerate,
        Analyze,
        Generate,
        Lifecycle,
        Mate,
        Tournament,
        AutoTrain,
        Top,
        MergeTop,
        ExportAI,
        BuildInitialTables,
    }
}
