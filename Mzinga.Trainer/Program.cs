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
                    switch (args[0].ToLower())
                    {
                        case "g":
                        case "generate":
                            Trainer.Generate(Int32.Parse(args[1]), Double.Parse(args[2]), Double.Parse(args[3]), args[4]);
                            break;
                        case "b":
                        case "battle":
                            Trainer.Battle(args[1], args[2]);
                            break;
                        case "br":
                        case "battleroyale":
                            Trainer.BattleRoyale(args[1]);
                            break;
                        case "t":
                        case "tournament":
                            Trainer.Tournament(args[1]);
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
            Console.WriteLine("Mzinga.Engine {0}", Assembly.GetEntryAssembly().GetName().Version.ToString());
            Console.WriteLine();

            Console.WriteLine("Commands:");
            Console.WriteLine("generate [count] [minWeight] [maxWeight] [path]");
            Console.WriteLine("battle [whiteprofilepath] [blackprofilepath]");
            Console.WriteLine("battleroyale [path]");
            Console.WriteLine("tournament [path]");

            Console.WriteLine();
        }
    }
}
