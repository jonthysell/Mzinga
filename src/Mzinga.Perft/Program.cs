// 
// Program.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2019, 2021 Jon Thysell <http://jonthysell.com>
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
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Mzinga.Core;

namespace Mzinga.Perft
{
    public class Program
    {
        static ExpansionPieces GameType = ExpansionPieces.None;
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

            if (null != ex.InnerException)
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
            if (null != args)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (EnumUtils.TryParseExpansionPieces(args[i], out ExpansionPieces gameType))
                    {
                        GameType = gameType;
                    }
                    else if (uint.TryParse(args[i], out uint maxDepth))
                    {
                        MaxDepth = maxDepth;
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

            GameBoard gameBoard = new GameBoard(GameType);

            for (int depth = 0; depth <= MaxDepth; depth++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                Task<long?> task = MultiThreaded ? gameBoard.ParallelPerftAsync(depth, Environment.ProcessorCount / 2, token) : gameBoard.CalculatePerftAsync(depth, token);
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
