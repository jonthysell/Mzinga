﻿// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

using Mono.Unix;
using Mono.Unix.Native;

namespace Mzinga.Engine
{
    public class SigIntMonitor
    {
        public event EventHandler? SigIntReceived;

        private Task? _task;
        private CancellationTokenSource? _cts;

        private SigIntMonitor() { }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _task = Task.Run(async () =>
            {
                UnixSignal signal = new UnixSignal(Signum.SIGINT);
                while (!_cts.IsCancellationRequested)
                {
                    signal.WaitOne(SignalTimeoutMS);
                    if (signal.IsSet)
                    {
                        OnSigIntReceived();
                        signal.Reset();
                    }
                    await Task.Yield();
                }
            });
        }

        public void Stop()
        {
            _cts?.Cancel();
            _task?.Wait();
        }

        private void OnSigIntReceived()
        {
            SigIntReceived?.Invoke(this, EventArgs.Empty);
        }

        public static SigIntMonitor CreateAndStart(EventHandler? sigIntReceivedEventHandler = null)
        {
            var monitor = new SigIntMonitor();
            if (sigIntReceivedEventHandler is not null)
            {
                monitor.SigIntReceived += sigIntReceivedEventHandler;
            }
            monitor.Start();
            return monitor;
        }

        private const int SignalTimeoutMS = 50;
    }
}