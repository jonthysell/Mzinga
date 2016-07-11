// 
// EloUtils.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016 Jon Thysell <http://jonthysell.com>
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

namespace Mzinga.Trainer
{
    public class EloUtils
    {
        public static void UpdateRatings(int whiteRating, int blackRating, double whiteScore, double blackScore, out int updatedWhiteRating, out int updatedBlackRating)
        {
            if (whiteRating < MinRating)
            {
                throw new ArgumentOutOfRangeException("whiteRating");
            }

            if (blackRating < MinRating)
            {
                throw new ArgumentOutOfRangeException("blackRating");
            }

            if (whiteScore < 0.0 || whiteScore > 1.0)
            {
                throw new ArgumentOutOfRangeException("whiteScore");
            }

            if (blackScore < 0.0 || blackScore > 1.0)
            {
                throw new ArgumentOutOfRangeException("blackScore");
            }

            double qWhite = Math.Pow(10, (double)whiteRating / 400.0);
            double qBlack = Math.Pow(10, (double)blackRating / 400.0);

            double eWhite = qWhite / (qWhite + qBlack);
            double eBlack = qBlack / (qWhite + qBlack);

            updatedWhiteRating = Math.Max(MinRating, whiteRating + (int)Math.Round(K * (whiteScore - eWhite)));
            updatedBlackRating = Math.Max(MinRating, blackRating + (int)Math.Round(K * (blackScore - eBlack)));
        }

        public const int DefaultRating = 1200;
        public const int MinRating = 1;

        private const double K = 32.0;
    }
}
