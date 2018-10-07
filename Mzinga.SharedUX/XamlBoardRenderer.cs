// 
// XamlBoardRenderer.cs
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

#if WINDOWS_UWP
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
#elif WINDOWS_WPF
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Resources;
using System.Windows.Shapes;
#endif

using Mzinga.Core;
using Mzinga.SharedUX.ViewModel;

namespace Mzinga.SharedUX
{
    public class XamlBoardRenderer
    {
        public MainViewModel VM { get; private set; }

        public Canvas BoardCanvas { get; private set; }
        public StackPanel WhiteHandStackPanel { get; private set; }
        public StackPanel BlackHandStackPanel { get; private set; }

        public Point CanvasCursorPosition
        {
            get
            {
#if WINDOWS_UWP
                Point point = CoreWindow.GetForCurrentThread().PointerPosition;
                point.X -= (Window.Current.Bounds.X);
                point.Y -= (Window.Current.Bounds.Y);

                Point canvasCoordinates = BoardCanvas.TransformToVisual(Window.Current.Content).TransformPoint(new Point(0, 0));
                point.X -= canvasCoordinates.X;
                point.Y -= canvasCoordinates.Y;

                point.X -= CanvasOffsetX;
                point.Y -= CanvasOffsetY;
#elif WINDOWS_WPF
                Point point = Mzinga.Viewer.MouseUtils.CorrectGetPosition(BoardCanvas);
                point.X -= CanvasOffsetX;
                point.Y -= CanvasOffsetY;
#endif
                return point;
            }
        }

        private double PieceCanvasMargin = 3.0;

        private double CanvasOffsetX = 0.0;
        private double CanvasOffsetY = 0.0;

        public bool RaiseStackedPieces
        {
            get
            {
                return _raiseStackedPieces;
            }
            set
            {
                bool oldValue = _raiseStackedPieces;
                if (oldValue != value)
                {
                    _raiseStackedPieces = value;
                    DrawBoard(LastBoard);
                }
            }
        }
        private bool _raiseStackedPieces;

        private double StackShiftRatio
        {
            get
            {
                return RaiseStackedPieces ? 0.5 : 0.1;
            }
        }

        private Board LastBoard;

        private SolidColorBrush WhiteBrush;
        private SolidColorBrush BlackBrush;

        private SolidColorBrush SelectedMoveEdgeBrush;
        private SolidColorBrush SelectedMoveBodyBrush;

        private SolidColorBrush LastMoveEdgeBrush;

        private SolidColorBrush[] BugBrushes;

        private SolidColorBrush DisabledPieceBrush;

        public XamlBoardRenderer(MainViewModel vm, Canvas boardCanvas, StackPanel whiteHandStackPanel, StackPanel blackHandStackPanel)
        {
            VM = vm ?? throw new ArgumentNullException("vm");
            BoardCanvas = boardCanvas ?? throw new ArgumentNullException("boardCanvas");
            WhiteHandStackPanel = whiteHandStackPanel ?? throw new ArgumentNullException("whiteHandStackPanel");
            BlackHandStackPanel = blackHandStackPanel ?? throw new ArgumentNullException("blackHandStackPanel");

            // Init brushes
            WhiteBrush = new SolidColorBrush(Colors.White);
            BlackBrush = new SolidColorBrush(Colors.Black);

            SelectedMoveEdgeBrush = new SolidColorBrush(Colors.Orange);
            SelectedMoveBodyBrush = new SolidColorBrush(Colors.Aqua)
            {
                Opacity = 0.25
            };

            LastMoveEdgeBrush = new SolidColorBrush(Colors.SeaGreen);

            BugBrushes = new SolidColorBrush[]
            {
                new SolidColorBrush(Color.FromArgb(255, 250, 167, 29)), // Queen
                new SolidColorBrush(Color.FromArgb(255, 139, 63, 27)), // Spider
                new SolidColorBrush(Color.FromArgb(255, 149, 101, 194)), // Beetle
                new SolidColorBrush(Color.FromArgb(255, 65, 157, 70)), // Grasshopper
                new SolidColorBrush(Color.FromArgb(255, 37, 141, 193)), // Ant
                new SolidColorBrush(Color.FromArgb(255, 111, 111, 97)), // Mosquito
                new SolidColorBrush(Color.FromArgb(255, 209, 32, 32)), // Ladybug
                new SolidColorBrush(Color.FromArgb(255, 37, 153, 102)), // Pullbug
            };

            DisabledPieceBrush = new SolidColorBrush(Colors.LightGray);

            // Bind board updates to VM
            if (null != VM)
            {
                VM.PropertyChanged += VM_PropertyChanged;
            }

            // Attach events
#if WINDOWS_UWP
            BoardCanvas.SizeChanged += BoardCanvas_SizeChanged;
            BoardCanvas.Tapped += BoardCanvas_Click;
            BoardCanvas.RightTapped += CancelClick;
            WhiteHandStackPanel.RightTapped += CancelClick;
            BlackHandStackPanel.RightTapped += CancelClick;
#elif WINDOWS_WPF
            BoardCanvas.SizeChanged += BoardCanvas_SizeChanged;
            BoardCanvas.MouseLeftButtonUp += BoardCanvas_Click;
            BoardCanvas.MouseRightButtonUp += CancelClick;
            WhiteHandStackPanel.MouseRightButtonUp += CancelClick;
            BlackHandStackPanel.MouseRightButtonUp += CancelClick;
#endif
        }

        private void VM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Board":
                case "ValidMoves":
                case "BoardHistory":
                case "TargetMove":
                case "ViewerConfig":
                    AppViewModel.Instance.DoOnUIThread(() =>
                    {
                        DrawBoard(VM.Board);
                    });
                    break;
            }
        }

        private void DrawBoard(Board board)
        {
            BoardCanvas.Children.Clear();
            WhiteHandStackPanel.Children.Clear();
            BlackHandStackPanel.Children.Clear();

            CanvasOffsetX = 0.0;
            CanvasOffsetX = 0.0;

            if (null != board)
            {
                Point minPoint = new Point(double.MaxValue, double.MaxValue);
                Point maxPoint = new Point(double.MinValue, double.MinValue);

                double boardCanvasWidth = BoardCanvas.ActualWidth;
                double boardCanvasHeight = BoardCanvas.ActualHeight;

                int maxStack;
                int numPieces;
                Dictionary<int, List<Piece>> piecesInPlay = GetPiecesOnBoard(board, out numPieces, out maxStack);

                int whiteHandCount = board.WhiteHand.Count();
                int blackHandCount = board.BlackHand.Count();

                int verticalPiecesMin = 3 + Math.Max(Math.Max(whiteHandCount, blackHandCount), board.GetWidth());
                int horizontalPiecesMin = 2 + Math.Min(whiteHandCount, 1) + Math.Min(blackHandCount, 1) + board.GetHeight();

                double size = 0.5 * Math.Min(boardCanvasHeight / verticalPiecesMin, boardCanvasWidth / horizontalPiecesMin);

                WhiteHandStackPanel.MinWidth = whiteHandCount > 0 ? (size + PieceCanvasMargin) * 2 : 0;
                BlackHandStackPanel.MinWidth = blackHandCount > 0 ? (size + PieceCanvasMargin) * 2 : 0;

                Position lastMoveStart = VM.AppVM.EngineWrapper.BoardHistory?.LastMove?.OriginalPosition;
                Position lastMoveEnd = VM.AppVM.EngineWrapper.BoardHistory?.LastMove?.Move?.Position;

                PieceName selectedPieceName = VM.AppVM.EngineWrapper.TargetPiece;
                Position targetPosition = VM.AppVM.EngineWrapper.TargetPosition;

                MoveSet validMoves = VM.AppVM.EngineWrapper.ValidMoves;

                HexOrientation hexOrientation = VM.ViewerConfig.HexOrientation;

                // Draw the pieces in play
                for (int stack = 0; stack <= maxStack; stack++)
                {
                    if (piecesInPlay.ContainsKey(stack))
                    {
                        foreach (Piece piece in piecesInPlay[stack])
                        {
                            Position position = piece.Position;

                            if (piece.PieceName == selectedPieceName && null != targetPosition)
                            {
                                position = targetPosition;
                            }

                            Point center = GetPoint(position, size, hexOrientation, true);

                            HexType hexType = (piece.Color == PlayerColor.White) ? HexType.WhitePiece : HexType.BlackPiece;

                            Shape hex = GetHex(center, size, hexType, hexOrientation);
                            BoardCanvas.Children.Add(hex);

                            bool disabled = VM.ViewerConfig.DisablePiecesInPlayWithNoMoves && !(null != validMoves && validMoves.Any(m => m.PieceName == piece.PieceName));

                            UIElement hexText = GetHexText(center, size, piece.PieceName, disabled);

                            BoardCanvas.Children.Add(hexText);

                            minPoint = Min(center, size, minPoint);
                            maxPoint = Max(center, size, maxPoint);
                        }
                    }
                }

                Dictionary<BugType, Stack<Canvas>> pieceCanvasesByBugType = new Dictionary<BugType, Stack<Canvas>>();

                // Draw the pieces in white's hand
                foreach (PieceName pieceName in board.WhiteHand)
                {
                    if (pieceName != selectedPieceName || (pieceName == selectedPieceName && null == targetPosition))
                    {
                        BugType bugType = EnumUtils.GetBugType(pieceName);

                        bool disabled = VM.ViewerConfig.DisablePiecesInHandWithNoMoves && !(null != validMoves && validMoves.Any(m => m.PieceName == pieceName));
                        Canvas pieceCanvas = GetPieceInHandCanvas(new Piece(pieceName, board.GetPiecePosition(pieceName)), size, hexOrientation, disabled);

                        if (!pieceCanvasesByBugType.ContainsKey(bugType))
                        {
                            pieceCanvasesByBugType[bugType] = new Stack<Canvas>();
                        }

                        pieceCanvasesByBugType[bugType].Push(pieceCanvas);
                    }
                }

                DrawHand(WhiteHandStackPanel, pieceCanvasesByBugType);

                pieceCanvasesByBugType.Clear();

                // Draw the pieces in black's hand
                foreach (PieceName pieceName in board.BlackHand)
                {
                    if (pieceName != selectedPieceName || (pieceName == selectedPieceName && null == targetPosition))
                    {
                        BugType bugType = EnumUtils.GetBugType(pieceName);

                        bool disabled = VM.ViewerConfig.DisablePiecesInHandWithNoMoves && !(null != validMoves && validMoves.Any(m => m.PieceName == pieceName));
                        Canvas pieceCanvas = GetPieceInHandCanvas(new Piece(pieceName, board.GetPiecePosition(pieceName)), size, hexOrientation, disabled);

                        if (!pieceCanvasesByBugType.ContainsKey(bugType))
                        {
                            pieceCanvasesByBugType[bugType] = new Stack<Canvas>();
                        }

                        pieceCanvasesByBugType[bugType].Push(pieceCanvas);
                    }
                }

                DrawHand(BlackHandStackPanel, pieceCanvasesByBugType);

                // Highlight last move played
                if (VM.AppVM.ViewerConfig.HighlightLastMovePlayed)
                {
                    // Highlight the lastMove start position
                    if (null != lastMoveStart)
                    {
                        Point center = GetPoint(lastMoveStart, size, hexOrientation, true);

                        Shape hex = GetHex(center, size, HexType.LastMove, hexOrientation);
                        BoardCanvas.Children.Add(hex);

                        minPoint = Min(center, size, minPoint);
                        maxPoint = Max(center, size, maxPoint);
                    }

                    // Highlight the lastMove end position
                    if (null != lastMoveEnd)
                    {
                        Point center = GetPoint(lastMoveEnd, size, hexOrientation, true);

                        Shape hex = GetHex(center, size, HexType.LastMove, hexOrientation);
                        BoardCanvas.Children.Add(hex);

                        minPoint = Min(center, size, minPoint);
                        maxPoint = Max(center, size, maxPoint);
                    }
                }

                // Highlight the selected piece
                if (VM.AppVM.ViewerConfig.HighlightTargetMove)
                {
                    if (selectedPieceName != PieceName.INVALID)
                    {
                        Position selectedPiecePosition = board.GetPiecePosition(selectedPieceName);

                        if (null != selectedPiecePosition)
                        {
                            Point center = GetPoint(selectedPiecePosition, size, hexOrientation, true);

                            Shape hex = GetHex(center, size, HexType.SelectedPiece, hexOrientation);
                            BoardCanvas.Children.Add(hex);

                            minPoint = Min(center, size, minPoint);
                            maxPoint = Max(center, size, maxPoint);
                        }
                    }
                }

                // Draw the valid moves for that piece
                if (VM.AppVM.ViewerConfig.HighlightValidMoves)
                {
                    if (selectedPieceName != PieceName.INVALID && null != validMoves)
                    {
                        foreach (Move validMove in validMoves)
                        {
                            if (validMove.PieceName == selectedPieceName)
                            {
                                Point center = GetPoint(validMove.Position, size, hexOrientation);

                                Shape hex = GetHex(center, size, HexType.ValidMove, hexOrientation);
                                BoardCanvas.Children.Add(hex);

                                minPoint = Min(center, size, minPoint);
                                maxPoint = Max(center, size, maxPoint);
                            }
                        }
                    }
                }

                // Highlight the target position
                if (VM.AppVM.ViewerConfig.HighlightTargetMove)
                {
                    if (null != targetPosition)
                    {
                        Point center = GetPoint(targetPosition, size, hexOrientation, true);

                        Shape hex = GetHex(center, size, HexType.SelectedMove, hexOrientation);
                        BoardCanvas.Children.Add(hex);

                        minPoint = Min(center, size, minPoint);
                        maxPoint = Max(center, size, maxPoint);
                    }
                }

                // Translate all game elements on the board
                double boardWidth = Math.Abs(maxPoint.X - minPoint.X);
                double boardHeight = Math.Abs(maxPoint.Y - minPoint.Y);

                if (!double.IsInfinity(boardWidth) && !double.IsInfinity(boardHeight))
                {
                    double boardCenterX = minPoint.X + (boardWidth / 2);
                    double boardCenterY = minPoint.Y + (boardHeight / 2);

                    double canvasCenterX = boardCanvasWidth / 2;
                    double canvasCenterY = boardCanvasHeight / 2;

                    double offsetX = canvasCenterX - boardCenterX;
                    double offsetY = canvasCenterY - boardCenterY;

                    TranslateTransform translate = new TranslateTransform()
                    {
                        X = offsetX,
                        Y = offsetY
                    };

                    foreach (UIElement child in BoardCanvas.Children)
                    {
                        if (null != (child as Border)) // Hex labels
                        {
                            Canvas.SetLeft(child, Canvas.GetLeft(child) + offsetX);
                            Canvas.SetTop(child, Canvas.GetTop(child) + offsetY);
                        }
                        else if (null != (child as Shape)) // Hexes
                        {
                            child.RenderTransform = translate;
                        }
                    }

                    CanvasOffsetX = offsetX;
                    CanvasOffsetY = offsetY;

                    VM.CanvasHexRadius = size;

                    // Add HUD elements

                    // Add lift text
                    if (maxStack > 0)
                    {
                        Border liftBorder = new Border() { Width = boardCanvasWidth, Height = boardCanvasHeight };

                        TextBlock liftText = new TextBlock();
                        liftText.HorizontalAlignment = HorizontalAlignment.Center;
                        liftText.VerticalAlignment = VerticalAlignment.Bottom;
                        liftText.Text = "Press 'x' to show covered tiles.";

                        liftBorder.Child = liftText;

                        BoardCanvas.Children.Add(liftBorder);
                    }
                }
            }

            LastBoard = board;
        }

        private Point Min(Point center, double size, Point minPoint)
        {
            double minX = Math.Min(minPoint.X, center.X - size);
            double minY = Math.Min(minPoint.Y, center.Y - size);

            return new Point(minX, minY);
        }

        private Point Max(Point center, double size, Point maxPoint)
        {
            double maxX = Math.Max(maxPoint.X, center.X + size);
            double maxY = Math.Max(maxPoint.Y, center.Y + size);

            return new Point(maxX, maxY);
        }

        private Point GetPoint(Position position, double size, HexOrientation hexOrientation, bool stackShift = false)
        {
            if (null == position)
            {
                throw new ArgumentNullException("position");
            }

            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            double x = hexOrientation == HexOrientation.FlatTop ? size * 1.5 * position.Q : size * Math.Sqrt(3.0) * (position.Q + (0.5 * position.R));
            double y = hexOrientation == HexOrientation.FlatTop ? size * Math.Sqrt(3.0) * (position.R + (0.5 * position.Q)) : size * 1.5 * position.R;

            if (stackShift && position.Stack > 0)
            {
                x += hexOrientation == HexOrientation.FlatTop ? size * 1.5 * StackShiftRatio * position.Stack : size * Math.Sqrt(3.0) * StackShiftRatio * position.Stack;
                y -= hexOrientation == HexOrientation.FlatTop ? size * Math.Sqrt(3.0) * StackShiftRatio * position.Stack : size * 1.5 * StackShiftRatio * position.Stack;
            }

            return new Point(x, y);
        }

        private Dictionary<int, List<Piece>> GetPiecesOnBoard(Board board, out int numPieces, out int maxStack)
        {
            if (null == board)
            {
                throw new ArgumentNullException("board");
            }

            numPieces = 0;
            maxStack = -1;

            Dictionary<int, List<Piece>> pieces = new Dictionary<int, List<Piece>>();
            pieces[0] = new List<Piece>();

            PieceName targetPieceName = VM.AppVM.EngineWrapper.TargetPiece;
            Position targetPosition = VM.AppVM.EngineWrapper.TargetPosition;

            bool targetPieceInPlay = false;

            // Add pieces already on the board
            foreach (PieceName pieceName in board.PiecesInPlay)
            {
                Position position = board.GetPiecePosition(pieceName);

                if (pieceName == targetPieceName)
                {
                    if (null != targetPosition)
                    {
                        position = targetPosition;
                    }
                    targetPieceInPlay = true;
                }

                int stack = position.Stack;
                maxStack = Math.Max(maxStack, stack);

                if (!pieces.ContainsKey(stack))
                {
                    pieces[stack] = new List<Piece>();
                }

                pieces[stack].Add(new Piece(pieceName, position));
                numPieces++;
            }

            // Add piece being placed on the board
            if (!targetPieceInPlay && null != targetPosition)
            {
                int stack = targetPosition.Stack;
                maxStack = Math.Max(maxStack, stack);

                if (!pieces.ContainsKey(stack))
                {
                    pieces[stack] = new List<Piece>();
                }

                pieces[stack].Add(new Piece(targetPieceName, targetPosition));
                numPieces++;
            }

            return pieces;
        }

        private Shape GetHex(Point center, double size, HexType hexType, HexOrientation hexOrientation)
        {
            if (null == center)
            {
                throw new ArgumentNullException("center");
            }

            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            double strokeThickness = size / 10;

            Path hex = new Path
            {
                StrokeThickness = strokeThickness
            };

            switch (hexType)
            {
                case HexType.WhitePiece:
                    hex.Fill = WhiteBrush;
                    hex.Stroke = BlackBrush;
                    break;
                case HexType.BlackPiece:
                    hex.Fill = BlackBrush;
                    hex.Stroke = BlackBrush;
                    break;
                case HexType.ValidMove:
                    hex.Fill = SelectedMoveBodyBrush;
                    hex.Stroke = SelectedMoveBodyBrush;
                    break;
                case HexType.SelectedPiece:
                    hex.Stroke = SelectedMoveEdgeBrush;
                    break;
                case HexType.SelectedMove:
                    hex.Fill = SelectedMoveBodyBrush;
                    hex.Stroke = SelectedMoveEdgeBrush;
                    break;
                case HexType.LastMove:
                    hex.Stroke = LastMoveEdgeBrush;
                    break;
            }

            PathGeometry data = new PathGeometry();
            PathFigure figure = new PathFigure();
            figure.IsClosed = true;

            double hexRadius = size - 0.75 * strokeThickness;

            for (int i = 0; i <= 6; i++)
            {
                double angle_deg = 60.0 * i + (hexOrientation == HexOrientation.PointyTop ? 30.0 : 0);

                double angle_rad1 = Math.PI / 180 * (angle_deg - 3);
                double angle_rad2 = Math.PI / 180 * (angle_deg + 3);

                Point p1 = new Point(center.X + hexRadius * Math.Cos(angle_rad1), center.Y + hexRadius * Math.Sin(angle_rad1));
                Point p2 = new Point(center.X + hexRadius * Math.Cos(angle_rad2), center.Y + hexRadius * Math.Sin(angle_rad2));

                if (i == 0)
                {
                    figure.StartPoint = p2;
                }
                else
                {
                    figure.Segments.Add(new LineSegment() { Point = p1 });
                    figure.Segments.Add(new ArcSegment() { Point = p2, SweepDirection = SweepDirection.Counterclockwise });
                }
            }

            data.Figures.Add(figure);
            hex.Data = data;

            return hex;
        }

        private Border GetHexText(Point center, double size, PieceName pieceName, bool disabled)
        {
            if (null == center)
            {
                throw new ArgumentNullException("center");
            }

            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            TextBlock hexText = new TextBlock
            {
                Text = EnumUtils.GetShortName(pieceName).Substring(1),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Arial Black")
            };

            // Add color
            hexText.Foreground = BugBrushes[(int)EnumUtils.GetBugType(pieceName)];

            if (disabled)
            {
                hexText.Foreground = MixSolidColorBrushes((SolidColorBrush)hexText.Foreground, DisabledPieceBrush);
            }

            hexText.FontSize = size * 0.75;

            Canvas.SetLeft(hexText, center.X - (hexText.Text.Length * (hexText.FontSize / 3.0)));
            Canvas.SetTop(hexText, center.Y - (hexText.FontSize / 2.0));

            Border b = new Border() { Height = size * 2.0, Width = size * 2.0 };
            b.Child = hexText;

            Canvas.SetLeft(b, center.X - (b.Width / 2.0));
            Canvas.SetTop(b, center.Y - (b.Height / 2.0));

            return b;
        }

        private static SolidColorBrush MixSolidColorBrushes(SolidColorBrush b1, SolidColorBrush b2)
        {
            SolidColorBrush result = new SolidColorBrush
            {
                Color = Color.FromArgb((byte)((b1.Color.A + b2.Color.A) / 2),
                                       (byte)((b1.Color.R + b2.Color.R) / 2),
                                       (byte)((b1.Color.G + b2.Color.G) / 2),
                                       (byte)((b1.Color.B + b2.Color.B) / 2))
            };
            return result;
        }

        private enum HexType
        {
            WhitePiece,
            BlackPiece,
            ValidMove,
            SelectedPiece,
            SelectedMove,
            LastMove,
        }

        private void DrawHand(StackPanel handPanel, Dictionary<BugType, Stack<Canvas>> pieceCanvases)
        {
            foreach (BugType bugType in EnumUtils.BugTypes)
            {
                if (pieceCanvases.ContainsKey(bugType))
                {
                    if (VM.ViewerConfig.StackPiecesInHand)
                    {
                        int startingCount = pieceCanvases[bugType].Count;

                        Canvas bugStack = new Canvas()
                        {
                            Height = pieceCanvases[bugType].Peek().Height * (1 + startingCount * StackShiftRatio),
                            Width = pieceCanvases[bugType].Peek().Width * (1 + startingCount * StackShiftRatio),
                            Margin = new Thickness(PieceCanvasMargin),
                            Background = new SolidColorBrush(Colors.Transparent),
                        };

                        while (pieceCanvases[bugType].Count > 0)
                        {
                            Canvas pieceCanvas = pieceCanvases[bugType].Pop();
                            Canvas.SetTop(pieceCanvas, pieceCanvas.Height * ((startingCount - pieceCanvases[bugType].Count - 1) * StackShiftRatio));
                            Canvas.SetLeft(pieceCanvas, pieceCanvas.Width * ((startingCount - pieceCanvases[bugType].Count - 1) * StackShiftRatio));
                            bugStack.Children.Add(pieceCanvas);
                        }

                        handPanel.Children.Add(bugStack);
                    }
                    else
                    {
                        foreach (Canvas pieceCanvas in pieceCanvases[bugType].Reverse())
                        {
                            handPanel.Children.Add(pieceCanvas);
                        }
                    }
                }
            }
        }

        private Canvas GetPieceInHandCanvas(Piece piece, double size, HexOrientation hexOrientation, bool disabled)
        {
            Point center = new Point(size, size);

            HexType hexType = (piece.Color == PlayerColor.White) ? HexType.WhitePiece : HexType.BlackPiece;

            Shape hex = GetHex(center, size, hexType, hexOrientation);
            UIElement hexText = GetHexText(center, size, piece.PieceName, disabled);

            Canvas pieceCanvas = new Canvas
            {
                Height = size * 2,
                Width = size * 2,
                Margin = new Thickness(PieceCanvasMargin),
                Background = new SolidColorBrush(Colors.Transparent),
                Name = EnumUtils.GetShortName(piece.PieceName)
            };

            pieceCanvas.Children.Add(hex);
            pieceCanvas.Children.Add(hexText);

            // Add highlight if the piece is selected
            if (VM.AppVM.EngineWrapper.TargetPiece == piece.PieceName)
            {
                Shape highlightHex = GetHex(center, size, HexType.SelectedPiece, hexOrientation);
                pieceCanvas.Children.Add(highlightHex);
            }

#if WINDOWS_UWP
            pieceCanvas.Tapped += PieceCanvas_Click;
            pieceCanvas.RightTapped += CancelClick;
#elif WINDOWS_WPF
            pieceCanvas.MouseLeftButtonUp += PieceCanvas_Click;
            pieceCanvas.MouseRightButtonUp += CancelClick;
#endif

            return pieceCanvas;
        }

#if WINDOWS_UWP
        private void PieceCanvas_Click(object sender, TappedRoutedEventArgs e)
#elif WINDOWS_WPF
        private void PieceCanvas_Click(object sender, MouseButtonEventArgs e)
#endif
        {
            Canvas pieceCanvas = sender as Canvas;

            if (null != pieceCanvas)
            {
                PieceName clickedPiece = EnumUtils.ParseShortName(pieceCanvas.Name);
                VM.PieceClick(clickedPiece);
            }
        }

#if WINDOWS_UWP
        private void BoardCanvas_Click(object sender, TappedRoutedEventArgs e)
#elif WINDOWS_WPF
        private void BoardCanvas_Click(object sender, MouseButtonEventArgs e)
#endif
        {
            Point p = CanvasCursorPosition;
            VM.CanvasClick(p.X, p.Y);
        }

#if WINDOWS_UWP
        private void CancelClick(object sender, RightTappedRoutedEventArgs e)
#elif WINDOWS_WPF
        private void CancelClick(object sender, MouseButtonEventArgs e)
#endif
        {
            VM.CancelClick();
        }

        private DateTime LastRedrawOnSizeChange = DateTime.Now;

#if WINDOWS_UWP || WINDOWS_WPF
        private void BoardCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
#endif
        {
            if (DateTime.Now - LastRedrawOnSizeChange > TimeSpan.FromMilliseconds(20))
            {
                DrawBoard(LastBoard);
                LastRedrawOnSizeChange = DateTime.Now;
            }
        }
    }
}
