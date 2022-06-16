// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

namespace Mzinga.Core
{
    public readonly struct Position
    {
        public readonly int Q;
        public readonly int R;
        public readonly int Stack;

        public Position(int q, int r, int stack)
        {
            Q = q;
            R = r;
            Stack = stack;
        }

        public static readonly Position OriginPosition = new Position(0, 0, 0);

        public static readonly Position NullPosition = new Position(0, 0, -1);

        public static readonly Position[] OriginNeighbors = new Position[]
        {
            new Position(0, -1, 0),
            new Position(1, -1, 0),
            new Position(1, 0, 0),
            new Position(0, 1, 0),
            new Position(-1, 1, 0),
            new Position(-1, 0, 0),
        };

        internal static readonly int[][] NeighborDeltas = new int[][]
        {
            new int[] { 0, -1, 0 },
            new int[] { 1, -1, 0 },
            new int[] { 1, 0, 0 },
            new int[] { 0, 1, 0 },
            new int[] { -1, 1, 0 },
            new int[] { -1, 0, 0 },
            new int[] { 0, 0, 1 },
        };

        public Position GetNeighborAt(Direction direction) => new Position
        (
            Q + NeighborDeltas[(int)direction][0],
            R + NeighborDeltas[(int)direction][1],
            Stack
        );

        public Position GetAbove() => new Position(Q, R, Stack + 1);

        public Position GetBelow() => new Position(Q, R, Stack - 1);

        public Position GetBottom() => Stack == 0 ? this : new Position(Q, R, 0);

        public override bool Equals(object? obj)
        {
            return obj is Position pos && this == pos;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Q, R, Stack);
        }

        public static bool operator ==(in Position lhs, in Position rhs)
        {
            return lhs.Q == rhs.Q && lhs.R == rhs.R && lhs.Stack == rhs.Stack;
        }

        public static bool operator !=(in Position lhs, in Position rhs) => !(lhs == rhs);

        public const int BoardSize = 128;
        public const int BoardStackSize = 8;
    }
}