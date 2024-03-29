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
        public static readonly StyledProperty<PieceName> PieceNameProperty = AvaloniaProperty.Register<TileControl, PieceName>(nameof(PieceName), PieceName.INVALID);

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

        public static readonly StyledProperty<bool> UseSplitBackgroundProperty = AvaloniaProperty.Register<TileControl, bool>(nameof(UseSplitBackground));

        public bool UseSplitBackground
        {
            get
            {
                return GetValue(UseSplitBackgroundProperty);
            }
            set
            {
                SetValue(UseSplitBackgroundProperty, value);
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

        public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<TileControl, string>(nameof(Text));

        public string Text
        {
            get
            {
                return GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public static readonly ISolidColorBrush TileControlStrokeBrush;

        public static readonly ISolidColorBrush TileControlDisabledBrush;

        public static readonly IBrush SplitBackgroundPointyTopBrush;

        public static readonly IBrush SplitBackgroundFlatTopBrush;

        static TileControl()
        {
            PieceNameProperty.Changed.AddClassHandler<TileControl>(PieceNameChanged);
            HexSizeProperty.Changed.AddClassHandler<TileControl>(HexSizeChanged);
            HexOrientationProperty.Changed.AddClassHandler<TileControl>(HexOrientationChanged);
            UseColoredPiecesProperty.Changed.AddClassHandler<TileControl>(UseColoredPiecesChanged);
            UseSplitBackgroundProperty.Changed.AddClassHandler<TileControl>(UseSplitBackgroundChanged);
            IsEnabledProperty.Changed.AddClassHandler<TileControl>(IsEnabledChanged);

            TileControlStrokeBrush = App.Current.FindResource("TileControlStrokeBrush") as ISolidColorBrush;
            TileControlDisabledBrush = App.Current.FindResource("TileControlDisabledBrush") as ISolidColorBrush;
            SplitBackgroundPointyTopBrush = App.Current.FindResource("SplitBackgroundPointyTopBrush") as IBrush;
            SplitBackgroundFlatTopBrush = App.Current.FindResource("SplitBackgroundFlatTopBrush") as IBrush;
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

        private static void HexOrientationChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (sender is TileControl tc)
            {
                tc.Background = tc.GetBackgroundBrush();
            }
        }

        private static void UseColoredPiecesChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (sender is TileControl tc)
            {
                tc.Foreground = tc.GetForegroundBrush();
            }
        }

        private static void UseSplitBackgroundChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (sender is TileControl tc)
            {
                tc.Background = tc.GetBackgroundBrush();
            }
        }

        private static void IsEnabledChanged(object sender, AvaloniaPropertyChangedEventArgs e)
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
            // Bind the base hex
            var baseHexShape = e.NameScope.Find("PART_BaseHexShape") as HexShape;
            baseHexShape.Bind(Shape.StrokeThicknessProperty, this.GetObservable(HexSizeProperty).Select(hexSize => hexSize / 10));
            baseHexShape.Bind(Shape.StrokeProperty, this.GetObservable(IsEnabledProperty).Select(isEnabled => isEnabled ? TileControlStrokeBrush : ColorUtils.MixSolidColorBrushes(TileControlStrokeBrush, TileControlDisabledBrush)));

            // Bind the graphical piece body
            var bugGraphicOuterGrid = e.NameScope.Find("PART_BugGraphicOuterGrid") as Grid;
            bugGraphicOuterGrid.Bind(IsVisibleProperty, this.GetObservable(PieceStyleProperty).Select(pieceStyle => pieceStyle == PieceStyle.Graphical));

            var bugGraphicInnerGrid = e.NameScope.Find("PART_BugGraphicInnerGrid") as Grid;
            bugGraphicInnerGrid.Bind(WidthProperty, this.GetObservable(HexSizeProperty).Select(hexSize => hexSize * 2.0 * Math.Sin(Math.PI / 6) * GraphicalBugSizeRatio));
            bugGraphicInnerGrid.Bind(HeightProperty, this.GetObservable(HexSizeProperty).Select(hexSize => hexSize * 2.0 * Math.Sin(Math.PI / 6) * GraphicalBugSizeRatio));
            bugGraphicInnerGrid.Bind(RenderTransformProperty, this.GetObservable(PieceNameProperty).Select(_ => GetBugGraphicGridRenderTransform()));
            bugGraphicInnerGrid.Bind(RenderTransformProperty, this.GetObservable(HexOrientationProperty).Select(_ => GetBugGraphicGridRenderTransform()));
            bugGraphicInnerGrid.RenderTransformOrigin = RelativePoint.Center;

            var bugGraphicBugShape = e.NameScope.Find("PART_BugGraphicBugShape") as BugShape;
            bugGraphicBugShape.Bind(BugShape.BugTypeProperty, this.GetObservable(PieceNameProperty).Select(pieceName => Enums.GetBugType(pieceName)));

            var bugGraphicBugNumberTextBlock = e.NameScope.Find("PART_BugGraphicBugNumberTextBlock") as TextBlock;
            bugGraphicBugNumberTextBlock.Bind(TextBlock.FontSizeProperty, this.GetObservable(HexSizeProperty).Select(hexSize => hexSize * 0.5));
            bugGraphicBugNumberTextBlock.Bind(TextBlock.TextProperty, this.GetObservable(PieceNameProperty).Select(pieceName => Enums.TryGetBugNum(pieceName, out int bugNum) ? bugNum.ToString() : ""));
            bugGraphicBugNumberTextBlock.Bind(PaddingProperty, this.GetObservable(HexSizeProperty).Select(hexSize => new Thickness(0, 0, 0, hexSize * 0.075)));
            bugGraphicBugNumberTextBlock.Bind(IsVisibleProperty, this.GetObservable(PieceNameProperty).Select(_ => IsBugNumVisible()));
            bugGraphicBugNumberTextBlock.Bind(IsVisibleProperty, this.GetObservable(AddPieceNumbersProperty).Select(_ => IsBugNumVisible()));

            var bugGraphicBugNumberEllipse = e.NameScope.Find("PART_BugGraphicBugNumberEllipse") as Ellipse;
            bugGraphicBugNumberEllipse.Bind(IsVisibleProperty, this.GetObservable(PieceNameProperty).Select(_ => IsBugNumVisible()));
            bugGraphicBugNumberEllipse.Bind(IsVisibleProperty, this.GetObservable(AddPieceNumbersProperty).Select(_ => IsBugNumVisible()));

            // Bind the text piece body
            var bugTextGrid = e.NameScope.Find("PART_BugTextGrid") as Grid;
            bugTextGrid.Bind(IsVisibleProperty, this.GetObservable(PieceStyleProperty).Select(pieceStyle => pieceStyle == PieceStyle.Text || !string.IsNullOrEmpty(Text)));

            var bugTextTextBlock = e.NameScope.Find("PART_BugTextTextBlock") as TextBlock;
            bugTextTextBlock.Bind(TextBlock.FontSizeProperty, this.GetObservable(HexSizeProperty).Select(hexSize => hexSize * 0.75));
            bugTextTextBlock.Bind(TextBlock.TextProperty, this.GetObservable(PieceNameProperty).Select(_ => GetBugText()));
            bugTextTextBlock.Bind(TextBlock.TextProperty, this.GetObservable(AddPieceNumbersProperty).Select(_ => GetBugText()));
            bugTextTextBlock.Bind(PaddingProperty, this.GetObservable(HexSizeProperty).Select(hexSize => new Thickness(0, 0, 0, hexSize * 0.1)));

            base.OnApplyTemplate(e);
        }

        private bool IsBugNumVisible()
        {
            return AddPieceNumbers && Enums.TryGetBugNum(PieceName, out int _);
        }

        private IBrush GetBackgroundBrush()
        {
            if (UseSplitBackground)
            {
                return HexOrientation == HexOrientation.PointyTop ? SplitBackgroundPointyTopBrush : SplitBackgroundFlatTopBrush;
            }

            return Enums.GetColor(PieceName) == PlayerColor.White ? Brushes.White : Brushes.Black;
        }

        private IBrush GetForegroundBrush()
        {
            var bugBrush = UseColoredPieces ? ColorUtils.BugColorBrushes[(int)Enums.GetBugType(PieceName)] : (Enums.GetColor(PieceName) == PlayerColor.White ? Brushes.Black : Brushes.White);
            return IsEnabled ? bugBrush : ColorUtils.MixSolidColorBrushes(bugBrush, TileControlDisabledBrush);
        }

        private ITransform GetBugGraphicGridRenderTransform()
        {
            double rotateAngle = HexOrientation == HexOrientation.PointyTop ? -90.0 : -60.0;
            if (Enums.TryGetBugNum(PieceName, out int bugNum))
            {
                rotateAngle += (bugNum - 1) * 60.0;
            }
            return new RotateTransform(rotateAngle);
        }

        private string GetBugText()
        {
            if (!string.IsNullOrEmpty(Text))
            {
                return Text;
            }

            string text = PieceName.ToString().Substring(1);
            return AddPieceNumbers ? text : text.TrimEnd('1', '2', '3');
        }

        public const double GraphicalBugSizeRatio = 1.25;
    }
}