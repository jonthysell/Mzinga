// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

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
            _id = !string.IsNullOrWhiteSpace(id) ? id.Trim() : throw new ArgumentNullException(nameof(id));
            GameEngineConfig = gameEngineConfig ?? throw new ArgumentNullException(nameof(gameEngineConfig));
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
