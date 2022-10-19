// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Converters;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Mzinga.Core;

namespace Mzinga.Viewer
{
    public class TileControl : TemplatedControl
    {
        public static readonly StyledProperty<PieceName> PieceNameProperty = AvaloniaProperty.Register<TileControl, PieceName>(nameof(PieceName));

        public PieceName PieceName
        {
            get
            {
                return GetValue(PieceNameProperty);
            }
            set
            {
                SetValue(PieceNameProperty, value);
            }
        }

        public static readonly StyledProperty<double> HexSizeProperty = AvaloniaProperty.Register<TileControl, double>(nameof(HexSize));

        public double HexSize
        {
            get
            {
                return GetValue(HexSizeProperty);
            }
            set
            {
                SetValue(HexSizeProperty, value);
            }
        }

        public static readonly StyledProperty<HexOrientation> HexOrientationProperty = AvaloniaProperty.Register<TileControl, HexOrientation>(nameof(HexOrientation));

        public HexOrientation HexOrientation
        {
            get
            {
                return GetValue(HexOrientationProperty);
            }
            set
            {
                SetValue(HexOrientationProperty, value);
            }
        }

        public static readonly StyledProperty<PieceStyle> PieceStyleProperty = AvaloniaProperty.Register<TileControl, PieceStyle>(nameof(PieceStyle));

        public PieceStyle PieceStyle
        {
            get
            {
                return GetValue(PieceStyleProperty);
            }
            set
            {
                SetValue(PieceStyleProperty, value);
            }
        }

        public static readonly StyledProperty<bool> UseColoredPiecesProperty = AvaloniaProperty.Register<TileControl, bool>(nameof(UseColoredPieces));

        public bool UseColoredPieces
        {
            get
            {
                return GetValue(UseColoredPiecesProperty);
            }
            set
            {
                SetValue(UseColoredPiecesProperty, value);
            }
        }

        public static readonly StyledProperty<bool> AddPieceNumbersProperty = AvaloniaProperty.Register<TileControl, bool>(nameof(AddPieceNumbers));

        public bool AddPieceNumbers
        {
            get
            {
                return GetValue(AddPieceNumbersProperty);
            }
            set
            {
                SetValue(AddPieceNumbersProperty, value);
            }
        }

        static TileControl()
        {
            PieceNameProperty.Changed.AddClassHandler<TileControl>(PieceNameChanged);
            HexSizeProperty.Changed.AddClassHandler<TileControl>(HexSizeChanged);
            UseColoredPiecesProperty.Changed.AddClassHandler<TileControl>(UseColoredPiecesChanged);
        }

        private static void PieceNameChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (sender is TileControl tc)
            {
                tc.Background = tc.GetBackgroundBrush();
                tc.Foreground = tc.GetForegroundBrush();
            }
        }

        private static void HexSizeChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (sender is TileControl tc)
            {
                tc.Width = tc.HexSize * 2.0;
                tc.Height = tc.HexSize * 2.0;
            }
        }

        private static void UseColoredPiecesChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (sender is TileControl tc)
            {
                tc.Foreground = tc.GetForegroundBrush();
            }
        }

        public TileControl() : base()
        {
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            var baseHexShape = e.NameScope.Find("PART_BaseHexShape") as HexShape;
            baseHexShape.Bind(Shape.StrokeThicknessProperty, this.GetObservable(HexSizeProperty).Select(hexSize => hexSize / 10));

            var bugGraphicOuterGrid = e.NameScope.Find("PART_BugGraphicOuterGrid") as Grid;
            bugGraphicOuterGrid.Bind(IsVisibleProperty, this.GetObservable(PieceStyleProperty).Select(pieceStyle => pieceStyle == PieceStyle.Graphical));

            var bugGraphicInnerGrid = e.NameScope.Find("PART_BugGraphicInnerGrid") as Visual;
            bugGraphicInnerGrid.Bind(WidthProperty, this.GetObservable(HexSizeProperty).Select(hexSize => hexSize * 2.0 * Math.Sin(Math.PI / 6) * GraphicalBugSizeRatio));
            bugGraphicInnerGrid.Bind(HeightProperty, this.GetObservable(HexSizeProperty).Select(hexSize => hexSize * 2.0 * Math.Sin(Math.PI / 6) * GraphicalBugSizeRatio));
            bugGraphicInnerGrid.Bind(RenderTransformProperty, this.GetObservable(PieceNameProperty).Select(_ => GetBugGraphicGridRenderTransform()));
            bugGraphicInnerGrid.Bind(RenderTransformProperty, this.GetObservable(HexOrientationProperty).Select(_ => GetBugGraphicGridRenderTransform()));
            bugGraphicInnerGrid.RenderTransformOrigin = RelativePoint.Center;

            var bugGraphicBugShape = e.NameScope.Find("PART_BugGraphicBugShape") as BugShape;
            bugGraphicBugShape.Bind(BugShape.BugTypeProperty, this.GetObservable(PieceNameProperty).Select(pieceName => Enums.GetBugType(pieceName)));

            var bugGraphicBugNumberTextBlock = e.NameScope.Find("PART_BugGraphicBugNumberTextBlock") as TextBlock;
            bugGraphicBugNumberTextBlock.Bind(TextBlock.FontSizeProperty, this.GetObservable(HexSizeProperty).Select(hexSize => hexSize / 2));
            bugGraphicBugNumberTextBlock.Bind(TextBlock.TextProperty, this.GetObservable(PieceNameProperty).Select(pieceName => Enums.TryGetBugNum(pieceName, out int bugNum) ? bugNum.ToString() : ""));
            bugGraphicBugNumberTextBlock.Bind(IsVisibleProperty, this.GetObservable(PieceNameProperty).Select(_ => IsBugNumVisible()));
            bugGraphicBugNumberTextBlock.Bind(IsVisibleProperty, this.GetObservable(AddPieceNumbersProperty).Select(_ => IsBugNumVisible()));

            var bugGraphicBugNumberEllipse = e.NameScope.Find("PART_BugGraphicBugNumberEllipse") as Ellipse;
            bugGraphicBugNumberEllipse.Bind(IsVisibleProperty, this.GetObservable(PieceNameProperty).Select(_ => IsBugNumVisible()));
            bugGraphicBugNumberEllipse.Bind(IsVisibleProperty, this.GetObservable(AddPieceNumbersProperty).Select(_ => IsBugNumVisible()));

            base.OnApplyTemplate(e);
        }

        private bool IsBugNumVisible()
        {
            return AddPieceNumbers && Enums.TryGetBugNum(PieceName, out int _);
        }

        private IBrush GetBackgroundBrush()
        {
            return Enums.GetColor(PieceName) == PlayerColor.White ? Brushes.White : Brushes.Black;
        }

        private IBrush GetForegroundBrush()
        {
            return UseColoredPieces ? ColorUtils.BugColorBrushes[(int)Enums.GetBugType(PieceName)] : (Enums.GetColor(PieceName) == PlayerColor.White ? Brushes.Black : Brushes.White);
        }

        private ITransform GetBugGraphicGridRenderTransform()
        {
            double rotateAngle = HexOrientation == HexOrientation.PointyTop ? -90.0 : 0.0;
            if (Enums.TryGetBugNum(PieceName, out int bugNum))
            {
                rotateAngle += (bugNum - 1) * 60.0;
            }
            return new RotateTransform(rotateAngle);
        }

        public const double GraphicalBugSizeRatio = 1.25;
    }
}