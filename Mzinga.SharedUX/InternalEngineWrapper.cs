// 
// InternalEngineWrapper.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2018, 2019 Jon Thysell <http://jonthysell.com>
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
using System.Threading.Tasks;

using Mzinga.Engine;

namespace Mzinga.SharedUX
{
    public class InternalEngineWrapper : EngineWrapper
    {
        private readonly string _id;
        private GameEngine _gameEngine;

        public GameEngineConfig GameEngineConfig { get; private set; }

        public InternalEngineWrapper(string id, GameEngineConfig gameEngineConfig) : base()
        {
            _id = !string.IsNullOrWhiteSpace(id) ? id.Trim() : throw new ArgumentNullException("id");
            GameEngineConfig = gameEngineConfig ?? throw new ArgumentNullException("gameEngineConfig");
        }

        public override void StartEngine()
        {
            IsIdle = false;

            _gameEngine = new GameEngine(_id, GameEngineConfig, (format, args) =>
            {
                OnEngineOutput(string.Format(format, args));
            });

            _gameEngine.ParseCommand("info");
        }

        public override void StopEngine()
        {
            _gameEngine.TryCancelAsyncCommand();
            _gameEngine.ParseCommand("exit");
            _gameEngine = null;
        }

        protected override void OnEngineInput(string command)
        {
            Task.Run(() =>
            {
                lock (_gameEngine)
                {
                    _gameEngine.ParseCommand(command);
                }
            });
        }

        protected override void OnCancelCommand()
        {
            _gameEngine.TryCancelAsyncCommand();
        }
    }
}
