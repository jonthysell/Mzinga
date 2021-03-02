// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

namespace Mzinga.Trainer
{
    public class EloUtils
    {
        public static void UpdateRatings(int whiteRating, int blackRating, double whiteScore, double blackScore, out int updatedWhiteRating, out int updatedBlackRating)
        {
            UpdateRatings(whiteRating, blackRating, whiteScore, blackScore, DefaultK, DefaultK, out updatedWhiteRating, out updatedBlackRating);
        }

        public static void UpdateRatings(int whiteRating, int blackRating, double whiteScore, double blackScore, double whiteK, double blackK, out int updatedWhiteRating, out int updatedBlackRating)
        {
            if (whiteRating < MinRating)
            {
                throw new ArgumentOutOfRangeException(nameof(whiteRating));
            }

            if (blackRating < MinRating)
            {
                throw new ArgumentOutOfRangeException(nameof(blackRating));
            }

            if (whiteScore < 0.0 || whiteScore > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(whiteScore));
            }

            if (blackScore < 0.0 || blackScore > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(blackScore));
            }

            if (whiteK <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(whiteK));
            }

            if (blackK <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(blackK));
            }

            double qWhite = Math.Pow(10, whiteRating / 400.0);
            double qBlack = Math.Pow(10, blackRating / 400.0);

            double eWhite = qWhite / (qWhite + qBlack);
            double eBlack = qBlack / (qWhite + qBlack);

            updatedWhiteRating = Math.Max(MinRating, whiteRating + (int)Math.Round(whiteK * (whiteScore - eWhite)));
            updatedBlackRating = Math.Max(MinRating, blackRating + (int)Math.Round(blackK * (blackScore - eBlack)));
        }

        public const int DefaultRating = 1200;
        public const int MinRating = 100;

        public const double ProvisionalK = 64.0;
        public const double DefaultK = 32.0;
    }
}
