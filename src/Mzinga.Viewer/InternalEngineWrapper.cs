// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

using Mzinga.Engine;

namespace Mzinga.Viewer
{
    public class InternalEngineWrapper : EngineWrapper
    {
        private readonly string _id;
        private Engine.Engine _engine;

        public EngineConfig EngineConfig { get; private set; }

        public InternalEngineWrapper(string id, EngineConfig gameEngineConfig) : base()
        {
            _id = !string.IsNullOrWhiteSpace(id) ? id.Trim() : throw new ArgumentNullException(nameof(id));
            EngineConfig = gameEngineConfig ?? throw new ArgumentNullException(nameof(gameEngineConfig));
        }

        public override void StartEngine()
        {
            IsIdle = false;

            _engine = new Engine.Engine(_id, EngineConfig, (format, args) =>
            {
                OnEngineOutput(string.Format(format, args));
            }, AssemblyUtils.GetEmbeddedMarkdownText<InternalEngineWrapper>("Licenses.txt").TrimEnd());

            _engine.ParseCommand("info");
        }

        public override void StopEngine()
        {
            _engine.TryCancelAsyncCommand();
            _engine.ParseCommand("exit");
            _engine = null;
        }

        protected override void OnEngineInput(string command)
        {
            Task.Run(() =>
            {
                lock (_engine)
                {
                    _engine.ParseCommand(command);
                }
            });
        }

        protected override void OnCancelCommand()
        {
            _engine.TryCancelAsyncCommand();
        }
    }
}
