// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Mzinga.Core;

namespace Mzinga.Perft
{
    public class Program
    {
        static GameType GameType = GameType.Base;
        static uint MaxDepth = uint.MaxValue;
        static bool MultiThreaded = false;

        static readonly CancellationTokenSource PerftCTS = new CancellationTokenSource();

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.CancelKeyPress += Console_CancelKeyPress;

            try
            {
                Console.WriteLine($"{AppInfo.Name} v{AppInfo.Version}");
                Console.WriteLine();

                ParseArgs(args);

                RunPerft();
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

            if (ex.InnerException is not null)
            {
                PrintException(ex.InnerException);
            }
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            PerftCTS.Cancel();
            e.Cancel = true;
        }

        static void ParseArgs(string[] args)
        {
            if (args is not null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (uint.TryParse(args[i], out uint maxDepth))
                    {
                        MaxDepth = maxDepth;
                    }
                    else if (Enums.TryParse(args[i], out GameType gameType))
                    {
                        GameType = gameType;
                    }
                    else if (args[i].Equals("-mt", StringComparison.InvariantCultureIgnoreCase))
                    {
                        MultiThreaded = true;
                    }
                }
            }
        }

        static void RunPerft()
        {
            CancellationToken token = PerftCTS.Token;

            var board = new Board(GameType);

            for (int depth = 0; depth <= MaxDepth; depth++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                Task<long?> task = MultiThreaded ? board.ParallelPerftAsync(depth, Environment.ProcessorCount / 2, token) : board.CalculatePerftAsync(depth, token);
                task.Wait();
                sw.Stop();

                if (!task.Result.HasValue)
                {
                    break;
                }

                Console.WriteLine("{0,-9} = {1,16:#,##0} in {2,16:#,##0} ms. {3,8:#,##0.0} KN/s", string.Format("perft({0})", depth), task.Result.Value, sw.ElapsedMilliseconds, Math.Round(task.Result.Value / (double)sw.ElapsedMilliseconds, 1));
            }
        }
    }
}
