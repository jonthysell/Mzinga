// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

namespace Mzinga.Core
{
    public class BoardMetrics
    {
        public BoardState BoardState;

        public int PiecesInPlay = 0;
        public int PiecesInHand = 0;

        public PieceMetrics this[PieceName pieceName]
        {
            get
            {
                return _pieceMetrics[(int)pieceName];
            }
        }

        private readonly PieceMetrics[] _pieceMetrics = new PieceMetrics[(int)PieceName.NumPieceNames];

        public BoardMetrics()
        {
            for (int i = 0; i < _pieceMetrics.Length; i++)
            {
                _pieceMetrics[i] = new PieceMetrics();
            }
        }

        public void Reset()
        {
            BoardState = BoardState.NotStarted;
            PiecesInPlay = 0;
            PiecesInHand = 0;

            for (int i = 0; i < _pieceMetrics.Length; i++)
            {
                _pieceMetrics[i].InPlay = 0;
                _pieceMetrics[i].IsPinned = 0;
                _pieceMetrics[i].IsCovered = 0;
                _pieceMetrics[i].NoisyMoveCount = 0;
                _pieceMetrics[i].QuietMoveCount = 0;
                _pieceMetrics[i].FriendlyNeighborCount = 0;
                _pieceMetrics[i].EnemyNeighborCount = 0;
            }
        }
    }
}
