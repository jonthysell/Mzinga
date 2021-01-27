// 
// PiecePositionBase.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2017, 2019 Jon Thysell <http://jonthysell.com>
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
using System.Text.RegularExpressions;

namespace Mzinga.Core
{
    public abstract class PiecePositionBase
    {
        public PieceName PieceName
        {
            get
            {
                return _pieceName;
            }
            protected set
            {
                _pieceName = value;

                if (value != PieceName.INVALID)
                {
                    Color = EnumUtils.GetColor(value);
                    BugType = EnumUtils.GetBugType(value);
                }
            }
        }
        private PieceName _pieceName = PieceName.INVALID;

        public Position Position { get; protected set; }

        public PlayerColor Color { get; private set; }
        public BugType BugType { get; private set; }

        protected static void Parse(string pieceString, out PieceName pieceName, out Position position)
        {
            if (!TryParse(pieceString, out pieceName, out position))
            {
                throw new ArgumentException(string.Format("Unable to parse \"{0}\".", pieceString));
            }
        }

        protected static bool TryParse(string pieceString, out PieceName pieceName, out Position position)
        {
            if (string.IsNullOrWhiteSpace(pieceString))
            {
                throw new ArgumentNullException(nameof(pieceString));
            }

            pieceString = pieceString.Trim();

            try
            {
                Match match = Regex.Match(pieceString, PieceRegex, RegexOptions.IgnoreCase);

                string nameString = match.Groups[1].Value;
                string positionString = match.Groups[2].Value;

                pieceName = EnumUtils.ParseShortName(nameString);
                position = Position.Parse(positionString);

                return true;
            }
            catch (Exception) { }

            pieceName = PieceName.INVALID;
            position = null;
            return false;
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]", EnumUtils.GetShortName(PieceName), null != Position ? Position.ToString() : "");
        }

        protected const string PieceRegex = @"([a-z0-9]{2,3})\[([0-9\-,]*)\]";
    }
}
