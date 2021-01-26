// 
// Program.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017, 2018, 2019 Jon Thysell <http://jonthysell.com>
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
using System.Reflection;
using System.Text;

namespace Mzinga.Engine
{
    public class Program
    {
        static string ID
        {
            get
            {
                return string.Format("Mzinga.Engine v{0}", Assembly.GetEntryAssembly().GetName().Version.ToString());
            }
        }

        private static GameEngine _engine;

        private static volatile bool _interceptCancel = false;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            GameEngineConfig config = null != args && args.Length > 0 ? LoadConfig(args[0]) : GameEngineConfig.GetDefaultEngineConfig();

            _engine = new GameEngine(ID, config, PrintLine);
            _engine.ParseCommand("info");

            Console.CancelKeyPress += Console_CancelKeyPress;

            _engine.StartAsyncCommand += (s, e) =>
            {
                _interceptCancel = true;
            };

            _engine.EndAsyncCommand += (s, e) =>
            {
                _interceptCancel = false;
            };

            while (!_engine.ExitRequested)
            {
                string command = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(command))
                {
                    _engine.ParseCommand(command);
                }
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (_interceptCancel)
            {
                _engine.TryCancelAsyncCommand();
                e.Cancel = true;
            }
        }

        static void PrintLine(string format, params object[] arg)
        {
            Console.Out.WriteLine(format, arg);
        }

        static GameEngineConfig LoadConfig(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                return new GameEngineConfig(fs);
            }
        }
    }
}
