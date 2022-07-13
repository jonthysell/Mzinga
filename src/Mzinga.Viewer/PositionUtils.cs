// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using Mzinga.Core;

namespace Mzinga.Viewer
{
    public static class PositionUtils
    {
        public static Position FromCursor(double cursorX, double cursorY, double hexRadius, HexOrientation hexOrientation)
        {
            if (hexRadius < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(hexRadius));
            }
            else if (double.IsInfinity(cursorX) || double.IsInfinity(cursorY) || hexRadius == 0) // No hexes on board
            {
                return Position.OriginPosition;
            }

            // Convert cursor to axial
            double q = hexOrientation == HexOrientation.FlatTop ? (cursorX * (2.0 / 3.0)) / hexRadius : ((-cursorY / 3.0) + (Math.Sqrt(3.0) / 3.0) * cursorX) / hexRadius;
            double r = hexOrientation == HexOrientation.FlatTop ? ((-cursorX / 3.0) + (Math.Sqrt(3.0) / 3.0) * cursorY) / hexRadius : (cursorY * (2.0 / 3.0)) / hexRadius;

            // Convert axial to cube
            double x = q;
            double z = r;
            double y = -x - z;

            // Round cube
            double rx = Math.Round(x);
            double ry = Math.Round(y);
            double rz = Math.Round(z);

            double xdiff = Math.Abs(rx - x);
            double ydiff = Math.Abs(ry - y);
            double zdiff = Math.Abs(rz - z);

            if (xdiff > ydiff && xdiff > zdiff)
            {
                rx = -ry - rz;
            }
            else if (ydiff > zdiff)
            {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                ry = -rx - rz;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            }
            else
            {
                rz = -rx - ry;
            }

            return new Position((int)rx, (int)rz, 0);
        }
    }
}
