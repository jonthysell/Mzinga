// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Mzinga.Viewer
{
    public class HexShape : Shape
    {
        public static readonly StyledProperty<double> HexSizeProperty = AvaloniaProperty.Register<HexShape, double>(nameof(HexSize));

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

        public static readonly StyledProperty<HexOrientation> HexOrientationProperty = AvaloniaProperty.Register<HexShape, HexOrientation>(nameof(HexOrientation));

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

        static HexShape()
        {
            AffectsGeometry<HexShape>(HexSizeProperty, HexOrientationProperty, StrokeThicknessProperty);
        }

        protected override Geometry CreateDefiningGeometry()
        {
            PathGeometry data = new PathGeometry();
            PathFigure figure = new PathFigure
            {
                IsClosed = true
            };

            double hexRadius = HexSize - StrokeThickness;

            for (int i = 0; i <= 6; i++)
            {
                double angle_deg = 60.0 * i + (HexOrientation == HexOrientation.PointyTop ? 30.0 : 0);

                double angle_rad1 = Math.PI / 180 * (angle_deg - 3);
                double angle_rad2 = Math.PI / 180 * (angle_deg + 3);

                Point p1 = new Point(HexSize + hexRadius * Math.Cos(angle_rad1), HexSize + hexRadius * Math.Sin(angle_rad1));
                Point p2 = new Point(HexSize + hexRadius * Math.Cos(angle_rad2), HexSize + hexRadius * Math.Sin(angle_rad2));

                if (i == 0)
                {
                    figure.StartPoint = p2;
                }
                else
                {
                    figure.Segments.Add(new LineSegment() { Point = p1 });
                    figure.Segments.Add(new ArcSegment() {
                        Point = p2,
                        SweepDirection = SweepDirection.CounterClockwise,
                    });
                }
            }

            data.Figures.Add(figure);

            return data;
        }

    }
}