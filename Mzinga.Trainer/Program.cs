// 
// Program.cs
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
using System.Reflection;

namespace Mzinga.Trainer
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
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
                        default:
                            ShowHelp();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: {0}", ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("Mzinga.Trainer {0}", Assembly.GetEntryAssembly().GetName().Version.ToString());
            Console.WriteLine();

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
            Console.WriteLine("generate               Create new random profiles");
            Console.WriteLine("lifecycle              Battle, cull, mate cycle for profiles");
            Console.WriteLine("mate                   Mate every profile with each other");
            Console.WriteLine("tournament             Fight an elimination-style tournament");
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
            Console.WriteLine("-MaxDraws              The max number of times to retry battles that end in a draw");
            Console.WriteLine("-MateMinMix            The min multiplier to mix up weights in children profiles");
            Console.WriteLine("-MateMaxMix            The max multiplier to mix up weights in children profiles");
            Console.WriteLine("-MateParentCount       The number of profiles to mate");
            Console.WriteLine("-MateShuffleParents    Whether or not to have random parents mate");
            Console.WriteLine("-MaxDepth              The maximum ply depth of the AI search");
            Console.WriteLine("-UseAlphaBetaPruning   Whether or not to use alpha-beta pruning");
            Console.WriteLine("-UseTranspositionTable Whether or not to use a transposition table");
            Console.WriteLine("-TurnMaxTime           The maximum time to let the AI think on its turn");
            Console.WriteLine("-BattleTimeLimit       The maximum time to let a battle run before declaring a draw");
            Console.WriteLine();
        }

        static Command ParseArguments(string[] args, TrainerSettings trainerSettings)
        {
            if (null == args || args.Length == 0)
            {
                throw new ArgumentNullException("args");
            }

            if (null == trainerSettings)
            {
                throw new ArgumentNullException("trainerSettings");
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
            }

            if (cmd == Command.Unknown)
            {
                throw new Exception(String.Format("Unknown command: {0}", args[0]));
            }

            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-pp":
                    case "-profilespath":
                        trainerSettings.ProfilesPath = args[i + 1];
                        i++;
                        break;
                    case "-wpp":
                    case "-whiteprofilepath":
                        trainerSettings.WhiteProfilePath = args[i + 1];
                        i++;
                        break;
                    case "-bpp":
                    case "-blackprofilepath":
                        trainerSettings.BlackProfilePath = args[i + 1];
                        i++;
                        break;
                    case "-ckc":
                    case "-cullkeepcount":
                        trainerSettings.CullKeepCount = Int32.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-gc":
                    case "-generatecount":
                        trainerSettings.GenerateCount = Int32.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-gminw":
                    case "-generateminweight":
                        trainerSettings.GenerateMinWeight = Double.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-gmaxw":
                    case "-generatemaxweight":
                        trainerSettings.GenerateMaxWeight = Double.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-lg":
                    case "-lifecyclegenerations":
                        trainerSettings.LifecycleGenerations = Int32.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-lc":
                    case "-lifecyclebattles":
                        trainerSettings.LifecycleBattles = Int32.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-mb":
                    case "-maxbattles":
                        trainerSettings.MaxBattles = Int32.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-mdraws":
                    case "-maxdraws":
                        trainerSettings.MaxDraws = Int32.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-mminm":
                    case "-mateminmix":
                        trainerSettings.MateMinMix = Double.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-mmaxm":
                    case "-matemaxmix":
                        trainerSettings.MateMaxMix = Double.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-mpc":
                    case "-mateparentcount":
                        trainerSettings.MateParentCount = Int32.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-msp":
                    case "-mateshuffleparents":
                        trainerSettings.MateShuffleParents = Boolean.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-mdepth":
                    case "-maxdepth":
                        trainerSettings.MaxDepth = Int32.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-uabp":
                    case "-usealphabetapruning":
                        trainerSettings.UseAlphaBetaPruning = Boolean.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-utt":
                    case "-usetranspositiontable":
                        trainerSettings.UseTranspositionTable = Boolean.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-tmt":
                    case "-turnmaxtime":
                        trainerSettings.TurnMaxTime = TimeSpan.Parse(args[i + 1]);
                        i++;
                        break;
                    case "-btl":
                    case "-battletimelimit":
                        trainerSettings.BattleTimeLimit = TimeSpan.Parse(args[i + 1]);
                        i++;
                        break;
                    default:
                        throw new Exception(String.Format("Unknown parameter: {0}", args[i]));
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
        Generate,
        Lifecycle,
        Mate,
        Tournament
    }
}
