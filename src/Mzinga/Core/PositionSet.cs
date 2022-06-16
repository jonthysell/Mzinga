// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Mzinga.Core
{
    public class PositionSet : HashSet<Position>
    {
        public PositionSet() : base() { }
        public PositionSet(int capacity) : base(capacity) { }
    }
}