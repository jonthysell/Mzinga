// 
// SigIntMonitor.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2021 Jon Thysell <http://jonthysell.com>
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
using System.Threading;
using System.Threading.Tasks;

using Mono.Unix;
using Mono.Unix.Native;

namespace Mzinga.Engine
{
    public class SigIntMonitor
    {
        public event EventHandler SigIntReceived;

        private Task _task;
        private CancellationTokenSource _cts;

        private SigIntMonitor() { }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _task = Task.Factory.StartNew(async () =>
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
            _cts.Cancel();
            _task.Wait();
        }

        private void OnSigIntReceived()
        {
            SigIntReceived?.Invoke(this, null);
        }

        public static SigIntMonitor CreateAndStart(EventHandler sigIntReceivedEventHandler = null)
        {
            var monitor = new SigIntMonitor();
            if (null != sigIntReceivedEventHandler)
            {
                monitor.SigIntReceived += sigIntReceivedEventHandler;
            }
            monitor.Start();
            return monitor;
        }

        private const int SignalTimeoutMS = 50;
    }
}