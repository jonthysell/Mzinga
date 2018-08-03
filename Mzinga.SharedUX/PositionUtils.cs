// 
// PositionUtils.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2018 Jon Thysell <http://jonthysell.com>
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

using Mzinga.Core;

namespace Mzinga.SharedUX
{
    public class PositionUtils
    {
        public static Position FromCursor(double cursorX, double cursorY, double hexRadius, HexOrientation hexOrientation)
        {
            if (hexRadius < 0)
            {
                throw new ArgumentOutOfRangeException("hexRadius");
            }
            else if (double.IsInfinity(cursorX) || double.IsInfinity(cursorY) || hexRadius == 0) // No hexes on board
            {
                return Position.Origin;
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
                ry = -rx - rz;
            }
            else
            {
                rz = -rx - ry;
            }

            return new Position((int)rx, (int)ry, (int)rz, 0);
        }
    }
}
