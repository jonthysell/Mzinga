// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

namespace Mzinga.Viewer
{
    [Serializable]
    public class EngineErrorException : Exception
    {
        public string[] OutputLines { get; private set; }

        public EngineErrorException(string message, string[] outputLines) : base(message)
        {
            OutputLines = outputLines;
        }
    }

    public class EngineInvalidMoveException : EngineErrorException
    {
        public EngineInvalidMoveException(string message, string[] outputLines) : base(message, outputLines) { }
    }
}
