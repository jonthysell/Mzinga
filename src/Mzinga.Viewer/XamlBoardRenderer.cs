// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

using Mzinga.Core;
using Mzinga.Viewer.ViewModels;

namespace Mzinga.Viewer
{
    public class XamlBoardRenderer
    {
        public MainViewModel VM { get; private set; }

        public Canvas BoardCanvas { get; private set; }
        public StackPanel WhiteHandStackPanel { get; private set; }
        public StackPanel BlackHandStackPanel { get; private set; }

        private readonly double PieceCanvasMargin = 3.0;

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
                return RaiseStackedPieces ? RaisedStackShiftLevel : BaseStackShiftLevel;
            }
        }

        private const double BaseStackShiftLevel = 0.1;
        private const double RaisedStackShiftLevel = 0.5;

        private const double GraphicalBugSizeRatio = 1.25;

        private Board LastBoard;

        private readonly SolidColorBrush WhiteBrush;
        private readonly SolidColorBrush BlackBrush;

        private readonly SolidColorBrush PieceOutlineBrush;

        private readonly SolidColorBrush SelectedMoveEdgeBrush;
        private readonly SolidColorBrush SelectedMoveBodyBrush;

        private readonly SolidColorBrush LastMoveEdgeBrush;

        private readonly SolidColorBrush DisabledPieceBrush;

        private readonly SolidColorBrush[] BugBrushes;

        private readonly string[] BugPathGeometries;

        public XamlBoardRenderer(MainViewModel vm, Canvas boardCanvas, StackPanel whiteHandStackPanel, StackPanel blackHandStackPanel)
        {
            VM = vm ?? throw new ArgumentNullException(nameof(vm));
            BoardCanvas = boardCanvas ?? throw new ArgumentNullException(nameof(boardCanvas));
            WhiteHandStackPanel = whiteHandStackPanel ?? throw new ArgumentNullException(nameof(whiteHandStackPanel));
            BlackHandStackPanel = blackHandStackPanel ?? throw new ArgumentNullException(nameof(blackHandStackPanel));

            // Init brushes
            WhiteBrush = new SolidColorBrush(Colors.White);
            BlackBrush = new SolidColorBrush(Colors.Black);

            PieceOutlineBrush = new SolidColorBrush(Color.Parse("#333333"));

            SelectedMoveEdgeBrush = new SolidColorBrush(Colors.Orange);
            SelectedMoveBodyBrush = new SolidColorBrush(Colors.Aqua)
            {
                Opacity = 0.25
            };

            LastMoveEdgeBrush = new SolidColorBrush(Colors.SeaGreen);

            DisabledPieceBrush = new SolidColorBrush(Colors.LightGray);

            BugBrushes = new SolidColorBrush[]
            {
                new SolidColorBrush(Color.FromArgb(255, 250, 167, 29)), // Bee
                new SolidColorBrush(Color.FromArgb(255, 139, 63, 27)), // Spider
                new SolidColorBrush(Color.FromArgb(255, 149, 101, 194)), // Beetle
                new SolidColorBrush(Color.FromArgb(255, 65, 157, 70)), // Grasshopper
                new SolidColorBrush(Color.FromArgb(255, 37, 141, 193)), // Ant
                new SolidColorBrush(Color.FromArgb(255, 111, 111, 97)), // Mosquito
                new SolidColorBrush(Color.FromArgb(255, 209, 32, 32)), // Ladybug
                new SolidColorBrush(Color.FromArgb(255, 37, 153, 102)), // Pillbug
            };

            BugPathGeometries = new string[]
            {
                "m -441.32388 -178.41533 -0.61467 0.16468 1.80227 6.72491 a 3.180386 3.180386 0 0 0 -0.13773 0.18112 6.3607721 1.9082317 30 0 0 -4.10778 -3.31496 6.3607721 1.9082317 30 0 0 -6.46261 -1.52781 6.3607721 1.9082317 30 0 0 2.52771 3.49443 4.7705791 1.590193 15 0 0 -0.87237 0.6127 4.7705791 1.590193 15 0 0 4.19621 2.77063 4.7705791 1.590193 15 0 0 1.57087 0.32312 0.63607719 1.9082317 75 0 0 -0.26165 0.45919 0.63607719 1.9082317 75 0 0 2.00804 0.12064 0.63607719 1.9082317 75 0 0 0.89275 -0.32673 3.180386 3.180386 0 0 0 0.0579 0.20544 l -4.88715 4.88747 0.44967 0.44966 4.7126 -4.7126 a 3.180386 3.180386 0 0 0 0.0772 0.1351 3.180386 4.7705791 0 0 0 -0.0302 0.0739 l -0.0273 -0.0158 -3.49837 6.05962 0.5509 0.31785 2.51884 -4.36253 a 3.180386 4.7705791 0 0 0 -0.0181 0.4947 3.180386 4.7705791 0 0 0 3.18053 4.77045 3.180386 4.7705791 0 0 0 3.18019 -4.77045 3.180386 4.7705791 0 0 0 -0.021 -0.49996 l 2.5218 4.36779 0.55091 -0.31818 -3.49838 -6.05929 -0.0256 0.0148 a 3.180386 4.7705791 0 0 0 -0.0309 -0.0805 3.180386 3.180386 0 0 0 0.0776 -0.12655 l 4.71128 4.71161 0.45 -0.44966 -4.88517 -4.88517 a 3.180386 3.180386 0 0 0 0.0552 -0.20774 1.9082317 0.63607721 15 0 0 0.89341 0.32673 1.9082317 0.63607721 15 0 0 2.00771 -0.12064 1.9082317 0.63607721 15 0 0 -0.26131 -0.45919 1.590193 4.7705791 75 0 0 1.57053 -0.32312 1.590193 4.7705791 75 0 0 4.19654 -2.77063 1.590193 4.7705791 75 0 0 -0.8727 -0.6127 1.9082317 6.3607721 60 0 0 2.52805 -3.49443 1.9082317 6.3607721 60 0 0 -6.46261 1.52781 1.9082317 6.3607721 60 0 0 -4.10845 3.31594 3.180386 3.180386 0 0 0 -0.13773 -0.17914 l 1.8026 -6.72787 -0.61434 -0.16468 -1.71319 6.39391 a 3.180386 3.180386 0 0 0 -0.43356 -0.27742 1.9082317 1.2721544 0 0 0 0.42731 -0.80072 1.9082317 1.2721544 0 0 0 -0.63012 -0.94272 l 0.9631 -3.59501 -0.30701 -0.0825 -0.94666 3.53322 a 1.9082317 1.2721544 0 0 0 -0.98742 -0.18539 1.9082317 1.2721544 0 0 0 -0.98743 0.18506 l -0.94666 -3.53289 -0.30733 0.0825 0.96309 3.59501 a 1.9082317 1.2721544 0 0 0 -0.63012 0.94272 1.9082317 1.2721544 0 0 0 0.42699 0.80072 3.180386 3.180386 0 0 0 -0.43323 0.27611 z", // Bee
                "M 31.927734 0 L 31.927734 38.048828 L 40.796875 47.052734 A 10.146194 22.828936 0 0 0 40.226562 48.71875 L 28.605469 48.974609 L 1.7441406 23.519531 L 0 25.361328 L 27.617188 51.533203 L 39.535156 51.271484 A 10.146194 22.828936 0 0 0 39.013672 53.935547 L 11.308594 53.935547 L 11.308594 56.470703 L 38.667969 56.470703 A 10.146194 22.828936 0 0 0 38.515625 58.166016 L 30.169922 62.417969 L 21.246094 99.404297 L 23.712891 100 L 32.361328 64.148438 L 38.386719 61.080078 A 10.146194 22.828936 0 0 0 38.369141 61.859375 A 10.146194 22.828936 0 0 0 48.515625 84.6875 A 10.146194 22.828936 0 0 0 58.662109 61.859375 A 10.146194 22.828936 0 0 0 58.650391 61.082031 L 64.669922 64.148438 L 73.320312 100 L 75.785156 99.404297 L 66.863281 62.417969 L 58.523438 58.171875 A 10.146194 22.828936 0 0 0 58.363281 56.470703 L 85.722656 56.470703 L 85.722656 53.935547 L 58.023438 53.935547 A 10.146194 22.828936 0 0 0 57.5 51.271484 L 69.414062 51.533203 L 69.416016 51.533203 L 97.03125 25.361328 L 95.287109 23.519531 L 68.425781 48.974609 L 56.796875 48.71875 A 10.146194 22.828936 0 0 0 56.226562 47.060547 L 65.103516 38.048828 L 65.103516 0 L 62.566406 0 L 62.566406 37.009766 L 55.128906 44.558594 A 10.146194 22.828936 0 0 0 54.050781 42.751953 L 54.074219 42.755859 L 55.652344 35.310547 L 53.169922 34.785156 L 51.974609 40.427734 A 10.146194 22.828936 0 0 0 48.515625 39.029297 A 10.146194 22.828936 0 0 0 45.056641 40.419922 L 43.861328 34.785156 L 41.378906 35.310547 L 42.957031 42.755859 L 42.96875 42.753906 A 10.146194 22.828936 0 0 0 41.910156 44.566406 L 34.464844 37.007812 L 34.464844 0 L 31.927734 0 z", // Spider
                "M 32.810547 0 L 23.402344 3.7480469 L 23.412109 3.7734375 L 18.898438 5.5800781 L 27.277344 26.505859 A 8.674272 8.674272 0 0 0 27.402344 27.556641 A 9.8403609 14.095652 0 0 0 26.099609 34.541016 A 9.8403609 14.095652 0 0 0 28.199219 43.236328 A 21.520721 27.850343 0 0 0 24.898438 45.347656 L 22.246094 36.921875 L 22.933594 21.146484 L 19.138672 20.982422 L 18.421875 37.423828 L 21.826172 48.238281 A 21.520721 27.850343 0 0 0 20.087891 50.431641 L 12.660156 49.960938 L 0 71.886719 L 3.2890625 73.785156 L 14.769531 53.900391 L 17.890625 54.097656 A 21.520721 27.850343 0 0 0 14.419922 69.21875 A 21.520721 27.850343 0 0 0 14.613281 72.675781 L 3.7695312 75.068359 L 8.1777344 100 L 11.917969 99.337891 L 8.1445312 77.992188 L 15.164062 76.443359 A 21.520721 27.850343 0 0 0 35.939453 97.068359 A 21.520721 27.850343 0 0 0 56.699219 76.439453 L 63.734375 77.992188 L 59.960938 99.339844 L 63.701172 100 L 68.109375 75.068359 L 57.273438 72.677734 A 21.520721 27.850343 0 0 0 57.460938 69.21875 A 21.520721 27.850343 0 0 0 54.007812 54.097656 L 57.109375 53.900391 L 68.589844 73.785156 L 71.878906 71.886719 L 59.220703 49.960938 L 51.794922 50.431641 A 21.520721 27.850343 0 0 0 50.056641 48.222656 L 53.457031 37.423828 L 52.740234 20.982422 L 48.947266 21.146484 L 49.632812 36.921875 L 46.978516 45.349609 A 21.520721 27.850343 0 0 0 43.660156 43.255859 A 9.8403609 14.095652 0 0 0 45.779297 34.541016 A 9.8403609 14.095652 0 0 0 44.476562 27.568359 A 8.674272 8.674272 0 0 0 44.601562 26.503906 L 52.980469 5.5800781 L 48.466797 3.7734375 L 48.476562 3.7480469 L 39.068359 0 L 35.939453 7.8554688 L 32.810547 0 z M 35.939453 8.2929688 L 41.466797 10.494141 L 38.539062 17.806641 A 8.674272 8.674272 0 0 0 33.339844 17.808594 L 30.412109 10.494141 L 35.939453 8.2929688 z", // Beetle
                "M 22.822266 0 L 10.861328 5.3535156 L 11.441406 6.6503906 L 22.232422 1.8203125 L 24.878906 6.4042969 A 7.5512447 7.5512447 0 0 0 24.703125 6.4941406 A 7.5512447 7.5512447 0 0 0 21.476562 15.833984 A 11.250689 45.002759 0 0 0 21.146484 16.992188 L 9.0976562 21.134766 L 4.34375 7.3027344 L 3.28125 7.6679688 L 8.0351562 21.5 L 9.1308594 24.691406 L 20.144531 20.908203 A 11.250689 45.002759 0 0 0 18.726562 28.689453 L 6.8398438 25.636719 L 1.59375 42.857422 L 2.6699219 43.185547 L 6.7910156 29.660156 L 18.230469 32.597656 A 11.250689 45.002759 0 0 0 17.228516 51.054688 A 11.250689 45.002759 0 0 0 17.236328 51.832031 A 9.6164947 3.4799555 57.271764 0 0 15.068359 47.728516 A 9.6164947 3.4799555 57.271764 0 0 6.9824219 41.693359 A 9.6164947 3.4799555 57.271764 0 0 6.2695312 43.580078 L 6.2617188 43.580078 L 0 99.876953 L 1.0390625 100 L 6.9453125 46.919922 A 9.6164947 3.4799555 57.271764 0 0 9.3945312 51.769531 A 9.6164947 3.4799555 57.271764 0 0 15.835938 57.914062 A 2.7801814 14.466934 3.1476777 0 0 15.203125 64.253906 A 2.7801814 14.466934 3.1476777 0 0 17.191406 78.822266 A 2.7801814 14.466934 3.1476777 0 0 19.085938 75.78125 A 11.250689 45.002759 0 0 0 28.480469 96.058594 A 11.250689 45.002759 0 0 0 37.871094 75.775391 A 14.466934 2.7801814 86.852322 0 0 39.767578 78.822266 A 14.466934 2.7801814 86.852322 0 0 41.755859 64.253906 A 14.466934 2.7801814 86.852322 0 0 41.123047 57.914062 A 3.4799555 9.6164947 32.728236 0 0 47.564453 51.769531 A 3.4799555 9.6164947 32.728236 0 0 50.013672 46.919922 L 55.919922 100 L 56.958984 99.876953 L 50.697266 43.580078 L 50.689453 43.580078 A 3.4799555 9.6164947 32.728236 0 0 49.976562 41.693359 A 3.4799555 9.6164947 32.728236 0 0 41.890625 47.728516 A 3.4799555 9.6164947 32.728236 0 0 39.720703 51.837891 A 11.250689 45.002759 0 0 0 39.730469 51.054688 A 11.250689 45.002759 0 0 0 38.724609 32.597656 L 50.167969 29.660156 L 54.289062 43.185547 L 55.365234 42.857422 L 50.119141 25.636719 L 38.226562 28.691406 A 11.250689 45.002759 0 0 0 36.816406 20.908203 L 47.828125 24.691406 L 48.923828 21.5 L 53.677734 7.6679688 L 52.615234 7.3027344 L 47.861328 21.134766 L 35.824219 16.996094 A 11.250689 45.002759 0 0 0 35.474609 15.869141 A 7.5512447 7.5512447 0 0 0 35.019531 9.2578125 A 7.5512447 7.5512447 0 0 0 32.080078 6.4023438 L 34.726562 1.8203125 L 45.517578 6.6484375 L 46.097656 5.3535156 L 34.136719 0 L 30.761719 5.84375 A 7.5512447 7.5512447 0 0 0 26.195312 5.8417969 L 22.822266 0 z", // Grasshopper
                "m 45.974609 0 -3.298828 7.9003906 a 11.409362 10.775509 0 0 0 -4.085937 8.2480474 11.409362 10.775509 0 0 0 0.0625 1.013671 l -5.441406 -2.207031 -10.117188 0.689453 0.08594 1.265625 9.826171 -0.669922 5.90625 2.396485 a 11.409362 10.775509 0 0 0 8.982422 8.095703 7.6062418 17.747898 0 0 0 -5.101562 11.410156 l -14.179688 -1.875 -16.109375 -24.705078 -1.699218 1.107422 16.61914 25.484375 15.13086 2.003906 a 7.6062418 17.747898 0 0 0 -0.160157 3.621094 7.6062418 17.747898 0 0 0 0.03906 1.683594 l -17 -1.480469 h -0.002 L 0 60.681641 1.1132812 62.376953 25.955078 46.064453 42.572266 47.511719 A 7.6062418 17.747898 0 0 0 46.34375 59.339844 L 27.587891 61.384766 6.5078125 89.933594 8.0683594 91.082031 28.568359 63.320312 46.050781 61.410156 A 11.409362 19.015604 0 0 0 38.589844 79.236328 11.409362 19.015604 0 0 0 50 98.251953 11.409362 19.015604 0 0 0 61.410156 79.236328 11.409362 19.015604 0 0 0 53.929688 61.408203 l 17.501953 1.912109 20.5 27.761719 1.560547 -1.148437 L 72.412109 61.384766 53.642578 59.337891 a 7.6062418 17.747898 0 0 0 3.789063 -11.826172 l 16.611328 -1.447266 24.84375 16.3125 L 100 60.681641 74.566406 43.982422 57.570312 45.462891 A 7.6062418 17.747898 0 0 0 57.605469 43.779297 7.6062418 17.747898 0 0 0 57.4375 40.160156 L 72.576172 38.154297 89.195312 12.669922 87.496094 11.5625 71.386719 36.267578 57.203125 38.144531 a 7.6062418 17.747898 0 0 0 -5.105469 -11.410156 11.409362 10.775509 0 0 0 8.990235 -8.097656 l 5.90625 -2.396485 9.826171 0.669922 0.08594 -1.265625 -10.119141 -0.689453 -5.43164 2.205078 a 11.409362 10.775509 0 0 0 0.05469 -1.011718 11.409362 10.775509 0 0 0 -4.09375 -8.2656255 L 54.025391 0 51.685547 0.9765625 53.78125 5.9941406 A 11.409362 10.775509 0 0 0 50 5.3730469 11.409362 10.775509 0 0 0 46.21875 5.9941406 l 2.095703 -5.0175781 z", // Ant
                "M 43.318359 0 A 0.94229363 20.730459 0 0 0 42.375 20.730469 A 0.94229363 20.730459 0 0 0 42.511719 31.392578 A 5.1826148 4.2403211 0 0 0 38.134766 35.580078 A 5.1826148 4.2403211 0 0 0 39.785156 38.675781 A 8.4806423 8.4806423 0 0 0 35.671875 42.728516 L 35.710938 42.611328 L 29.623047 40.5625 L 26.494141 23.109375 L 20.25 13.681641 L 18.679688 14.722656 L 24.707031 23.826172 L 27.964844 41.992188 L 35.082031 44.388672 A 8.4806423 8.4806423 0 0 0 34.859375 45.921875 L 27.697266 45.205078 L 10.722656 51.724609 L 0.56835938 48.511719 L 0 50.308594 L 10.779297 53.720703 L 27.953125 47.125 L 34.962891 47.826172 A 8.4806423 8.4806423 0 0 0 35.5 49.638672 L 28.914062 53.189453 L 23.705078 70.769531 L 15.814453 78.130859 L 17.099609 79.507812 L 25.369141 71.794922 L 30.498047 54.476562 L 36.417969 51.285156 A 8.4806423 8.4806423 0 0 0 37.642578 52.662109 A 5.1826148 20.259312 7.6708066 0 0 31.742188 71.039062 A 5.1826148 20.259312 7.6708066 0 0 34.173828 91.808594 A 5.1826148 20.259312 7.6708066 0 0 39.767578 83.044922 A 3.7691745 25.441926 0 0 0 43.318359 100 A 3.7691745 25.441926 0 0 0 46.865234 83.042969 A 20.259312 5.1826148 82.329193 0 0 52.460938 91.808594 A 20.259312 5.1826148 82.329193 0 0 54.892578 71.039062 A 20.259312 5.1826148 82.329193 0 0 48.992188 52.662109 A 8.4806423 8.4806423 0 0 0 50.220703 51.287109 L 56.136719 54.476562 L 61.267578 71.794922 L 69.535156 79.507812 L 70.820312 78.130859 L 62.929688 70.769531 L 57.722656 53.189453 L 51.140625 49.642578 A 8.4806423 8.4806423 0 0 0 51.664062 47.826172 L 58.681641 47.125 L 75.855469 53.720703 L 86.634766 50.308594 L 86.066406 48.511719 L 75.912109 51.724609 L 58.9375 45.203125 L 58.9375 45.205078 L 51.783203 45.919922 A 8.4806423 8.4806423 0 0 0 51.552734 44.388672 L 58.669922 41.992188 L 61.927734 23.826172 L 67.955078 14.722656 L 66.384766 13.681641 L 60.140625 23.109375 L 57.013672 40.5625 L 50.925781 42.611328 L 50.964844 42.726562 A 8.4806423 8.4806423 0 0 0 46.851562 38.675781 A 5.1826148 4.2403211 0 0 0 48.5 35.580078 A 5.1826148 4.2403211 0 0 0 44.125 31.396484 A 0.94229363 20.730459 0 0 0 44.259766 20.730469 A 0.94229363 20.730459 0 0 0 43.318359 0 z", // Mosquito
                "M 27.429688 0 L 26.011719 1.1816406 L 33.066406 9.6425781 A 9.2202667 5.5321599 0 0 0 32.958984 10.439453 A 9.2202667 5.5321599 0 0 0 33.501953 12.292969 A 20.284586 23.972693 0 0 0 22.972656 26.294922 L 15.03125 11.398438 L 3.9667969 11.398438 L 3.9667969 15.085938 L 12.818359 15.085938 L 21.966797 32.246094 A 20.284586 23.972693 0 0 0 21.894531 33.935547 A 20.284586 23.972693 0 0 0 22.189453 38.011719 A 31.348907 35.037012 0 0 0 20.880859 39.294922 L 20.873047 39.257812 L 11.941406 41.216797 L 2.4960938 32.529297 L 0 35.242188 L 10.857422 45.230469 L 17.220703 43.833984 A 31.348907 35.037012 0 0 0 10.830078 64.962891 A 31.348907 35.037012 0 0 0 13.890625 79.980469 L 13.376953 87.976562 L 5.2480469 90.130859 L 6.1914062 93.695312 L 16.886719 90.863281 L 17.195312 86.072266 A 31.348907 35.037012 0 0 0 42.179688 100 A 31.348907 35.037012 0 0 0 67.162109 86.054688 L 67.470703 90.863281 L 78.166016 93.695312 L 79.111328 90.130859 L 70.982422 87.976562 L 70.46875 79.964844 A 31.348907 35.037012 0 0 0 73.529297 64.962891 A 31.348907 35.037012 0 0 0 67.154297 43.837891 L 73.501953 45.230469 L 84.359375 35.242188 L 81.861328 32.529297 L 72.417969 41.216797 L 63.486328 39.257812 L 63.474609 39.310547 A 31.348907 35.037012 0 0 0 62.152344 37.974609 A 20.284586 23.972693 0 0 0 62.464844 33.935547 A 20.284586 23.972693 0 0 0 62.410156 32.210938 L 71.541016 15.085938 L 80.392578 15.085938 L 80.392578 11.398438 L 69.328125 11.398438 L 61.382812 26.302734 A 20.284586 23.972693 0 0 0 50.851562 12.296875 A 9.2202667 5.5321599 0 0 0 51.400391 10.439453 A 9.2202667 5.5321599 0 0 0 51.294922 9.640625 L 58.345703 1.1816406 L 56.929688 0 L 50.353516 7.8847656 A 9.2202667 5.5321599 0 0 0 42.179688 4.9082031 A 9.2202667 5.5321599 0 0 0 34.005859 7.8886719 L 27.429688 0 z", // Ladybug
                "M 0.39648438 0 L 0 5.4277344 L 25.066406 7.2558594 L 30.722656 13.107422 A 24.186731 45.35012 0 0 0 23.923828 21.423828 A 12.093366 2.267506 54.232346 0 0 19.263672 18.173828 A 12.093366 2.267506 54.232346 0 0 21.880859 25.378906 A 24.186731 45.35012 0 0 0 18.949219 33.597656 A 12.093366 2.267506 23.737921 0 0 12.042969 32.771484 A 12.093366 2.267506 23.737921 0 0 17.947266 37.648438 A 24.186731 45.35012 0 0 0 16.294922 50.021484 A 12.093366 2.267506 0 0 0 11.019531 51.890625 A 12.093366 2.267506 0 0 0 16.189453 53.75 A 24.186731 45.35012 0 0 0 16.166016 54.650391 A 24.186731 45.35012 0 0 0 16.662109 63.548828 A 12.093366 2.267506 12.462954 0 0 9.0058594 63.992188 A 12.093366 2.267506 12.462954 0 0 17.275391 68.060547 A 24.186731 45.35012 0 0 0 18.949219 75.714844 A 2.267506 12.093366 78.795931 0 0 8.9511719 79.986328 A 2.267506 12.093366 78.795931 0 0 20.337891 80.033203 A 24.186731 45.35012 0 0 0 25.318359 90.115234 A 2.267506 12.093366 63.399933 0 0 16.898438 96.84375 A 2.267506 12.093366 63.399933 0 0 28.142578 93.740234 A 24.186731 45.35012 0 0 0 40.353516 100 A 24.186731 45.35012 0 0 0 52.558594 93.738281 A 12.093366 2.267506 26.600067 0 0 63.808594 96.84375 A 12.093366 2.267506 26.600067 0 0 55.416016 90.128906 A 24.186731 45.35012 0 0 0 60.388672 80.037109 A 12.093366 2.267506 11.204069 0 0 71.753906 79.986328 A 12.093366 2.267506 11.204069 0 0 61.753906 75.712891 A 24.186731 45.35012 0 0 0 63.460938 68.052734 A 2.267506 12.093366 77.537046 0 0 71.701172 63.992188 A 2.267506 12.093366 77.537046 0 0 64.068359 63.546875 A 24.186731 45.35012 0 0 0 64.541016 54.650391 A 24.186731 45.35012 0 0 0 64.523438 53.748047 A 12.093366 2.267506 0 0 0 69.6875 51.890625 A 12.093366 2.267506 0 0 0 64.388672 50.017578 A 24.186731 45.35012 0 0 0 62.759766 37.646484 A 2.267506 12.093366 66.262079 0 0 68.664062 32.771484 A 2.267506 12.093366 66.262079 0 0 61.759766 33.595703 A 24.186731 45.35012 0 0 0 58.806641 25.410156 A 2.267506 12.093366 35.767654 0 0 61.443359 18.173828 A 2.267506 12.093366 35.767654 0 0 56.775391 21.431641 A 24.186731 45.35012 0 0 0 50.001953 13.087891 L 55.640625 7.2558594 L 80.707031 5.4277344 L 80.310547 0 L 53.173828 1.9785156 L 45.203125 10.224609 A 24.186731 45.35012 0 0 0 40.353516 9.2988281 A 24.186731 45.35012 0 0 0 35.535156 10.255859 L 27.533203 1.9785156 L 0.39648438 0 z", // Pillbug
            };

            // Bind board updates to VM
            if (VM is not null)
            {
                VM.PropertyChanged += VM_PropertyChanged;
            }

            // Attach events
            BoardCanvas.PropertyChanged += BoardCanvas_SizeChanged;
            BoardCanvas.PointerReleased += BoardCanvas_Click;
            WhiteHandStackPanel.PointerReleased += CancelClick;
            BlackHandStackPanel.PointerReleased += CancelClick;
        }

        private void VM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainViewModel.Board):
                case nameof(MainViewModel.ValidMoves):
                case nameof(MainViewModel.TargetMove):
                case nameof(MainViewModel.ViewerConfig):
                    AppViewModel.Instance.DoOnUIThread(() =>
                    {
                        DrawBoard(MainViewModel.Board);
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

            int z = BoardCanvas.ZIndex;

            if (board is not null)
            {
                Point minPoint = new Point(double.MaxValue, double.MaxValue);
                Point maxPoint = new Point(double.MinValue, double.MinValue);

                double boardCanvasWidth = BoardCanvas.Bounds.Width;
                double boardCanvasHeight = BoardCanvas.Bounds.Height;

                var piecesInPlay = GetPiecesOnBoard(board, out int numPieces, out int maxStack);

                int whiteHandCount = board.GetWhiteHand().Count();
                int blackHandCount = board.GetBlackHand().Count();

                int verticalPiecesMin = 3 + Math.Max(Math.Max(whiteHandCount, blackHandCount), board.GetWidth());
                int horizontalPiecesMin = 2 + Math.Min(whiteHandCount, 1) + Math.Min(blackHandCount, 1) + board.GetHeight();

                double size = 0.5 * Math.Min(boardCanvasHeight / verticalPiecesMin, boardCanvasWidth / horizontalPiecesMin);

                WhiteHandStackPanel.MinWidth = whiteHandCount > 0 ? (size + PieceCanvasMargin) * 2 : 0;
                BlackHandStackPanel.MinWidth = blackHandCount > 0 ? (size + PieceCanvasMargin) * 2 : 0;

                Position? lastMoveStart = MainViewModel.AppVM.EngineWrapper.Board?.BoardHistory.LastMove?.Source;
                Position? lastMoveEnd = MainViewModel.AppVM.EngineWrapper.Board?.BoardHistory.LastMove?.Destination;

                PieceName selectedPieceName = MainViewModel.AppVM.EngineWrapper.TargetPiece;
                Position? targetPosition = MainViewModel.AppVM.EngineWrapper.TargetPosition;

                MoveSet validMoves = MainViewModel.AppVM.EngineWrapper.ValidMoves;

                HexOrientation hexOrientation = MainViewModel.ViewerConfig.HexOrientation;

                Dictionary<BugType, Stack<Canvas>> pieceCanvasesByBugType = new Dictionary<BugType, Stack<Canvas>>();

                // Draw the pieces in white's hand
                foreach (PieceName pieceName in board.GetWhiteHand())
                {
                    if (pieceName != selectedPieceName || (pieceName == selectedPieceName && targetPosition is null))
                    {
                        BugType bugType = Enums.GetBugType(pieceName);

                        bool disabled = MainViewModel.ViewerConfig.DisablePiecesInHandWithNoMoves && !(validMoves is not null && validMoves.Any(m => m.PieceName == pieceName));
                        Canvas pieceCanvas = GetPieceInHandCanvas(pieceName, size, hexOrientation, disabled);

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
                foreach (PieceName pieceName in board.GetBlackHand())
                {
                    if (pieceName != selectedPieceName || (pieceName == selectedPieceName && targetPosition is null))
                    {
                        BugType bugType = Enums.GetBugType(pieceName);

                        bool disabled = MainViewModel.ViewerConfig.DisablePiecesInHandWithNoMoves && !(validMoves is not null && validMoves.Any(m => m.PieceName == pieceName));
                        Canvas pieceCanvas = GetPieceInHandCanvas(pieceName, size, hexOrientation, disabled);

                        if (!pieceCanvasesByBugType.ContainsKey(bugType))
                        {
                            pieceCanvasesByBugType[bugType] = new Stack<Canvas>();
                        }

                        pieceCanvasesByBugType[bugType].Push(pieceCanvas);
                    }
                }

                DrawHand(BlackHandStackPanel, pieceCanvasesByBugType);

                // Draw the pieces in play
                z++;
                for (int stack = 0; stack <= maxStack; stack++)
                {
                    if (piecesInPlay.ContainsKey(stack))
                    {
                        foreach (var tuple in piecesInPlay[stack])
                        {
                            var pieceName = tuple.Item1;
                            var position = tuple.Item2;

                            if (pieceName == selectedPieceName && targetPosition.HasValue)
                            {
                                position = targetPosition.Value;
                            }

                            Point center = GetPoint(position, size, hexOrientation, true);

                            HexType hexType = (Enums.GetColor(pieceName) == PlayerColor.White) ? HexType.WhitePiece : HexType.BlackPiece;

                            Shape hex = GetHex(center, size, hexType, hexOrientation);
                            hex.ZIndex = z;
                            BoardCanvas.Children.Add(hex);

                            bool disabled = MainViewModel.ViewerConfig.DisablePiecesInPlayWithNoMoves && !(validMoves is not null && validMoves.Any(m => m.PieceName == pieceName));

                            var hexText = MainViewModel.ViewerConfig.PieceStyle == PieceStyle.Text ? GetPieceText(center, size, pieceName, disabled) : GetPieceGraphics(center, size, pieceName, disabled);
                            hexText.ZIndex = z + 1;
                            BoardCanvas.Children.Add(hexText);

                            minPoint = Min(center, size, minPoint);
                            maxPoint = Max(center, size, maxPoint);
                        }
                        z += 2;
                    }
                }

                // Highlight last move played
                if (MainViewModel.AppVM.ViewerConfig.HighlightLastMovePlayed)
                {
                    z++;
                    // Highlight the lastMove start position
                    if (lastMoveStart.HasValue && lastMoveStart.Value.Stack >= 0)
                    {
                        Point center = GetPoint(lastMoveStart.Value, size, hexOrientation, true);

                        Shape hex = GetHex(center, size, HexType.LastMove, hexOrientation);
                        hex.ZIndex = z;
                        BoardCanvas.Children.Add(hex);

                        minPoint = Min(center, size, minPoint);
                        maxPoint = Max(center, size, maxPoint);
                    }

                    // Highlight the lastMove end position
                    if (lastMoveEnd.HasValue)
                    {
                        Point center = GetPoint(lastMoveEnd.Value, size, hexOrientation, true);

                        Shape hex = GetHex(center, size, HexType.LastMove, hexOrientation);
                        hex.ZIndex = z;
                        BoardCanvas.Children.Add(hex);

                        minPoint = Min(center, size, minPoint);
                        maxPoint = Max(center, size, maxPoint);
                    }
                }

                // Highlight the selected piece
                if (MainViewModel.AppVM.ViewerConfig.HighlightTargetMove)
                {
                    z++;
                    if (selectedPieceName != PieceName.INVALID)
                    {
                        Position selectedPiecePosition = board.GetPosition(selectedPieceName);

                        if (selectedPiecePosition != Position.NullPosition)
                        {
                            Point center = GetPoint(selectedPiecePosition, size, hexOrientation, true);
                            
                            Shape hex = GetHex(center, size, HexType.SelectedPiece, hexOrientation);
                            hex.ZIndex = z;
                            BoardCanvas.Children.Add(hex);

                            minPoint = Min(center, size, minPoint);
                            maxPoint = Max(center, size, maxPoint);
                        }
                    }
                }

                // Draw the valid moves for that piece
                if (MainViewModel.AppVM.ViewerConfig.HighlightValidMoves)
                {
                    z++;
                    if (selectedPieceName != PieceName.INVALID && validMoves is not null)
                    {
                        foreach (Move validMove in validMoves)
                        {
                            if (validMove.PieceName == selectedPieceName)
                            {
                                Point center = GetPoint(validMove.Destination, size, hexOrientation);

                                Shape hex = GetHex(center, size, HexType.ValidMove, hexOrientation);
                                hex.ZIndex = z;
                                BoardCanvas.Children.Add(hex);

                                minPoint = Min(center, size, minPoint);
                                maxPoint = Max(center, size, maxPoint);
                            }
                        }
                    }
                }

                // Highlight the target position
                if (MainViewModel.AppVM.ViewerConfig.HighlightTargetMove)
                {
                    z++;
                    if (targetPosition.HasValue)
                    {
                        Point center = GetPoint(targetPosition.Value, size, hexOrientation, true);

                        Shape hex = GetHex(center, size, HexType.SelectedMove, hexOrientation);
                        hex.ZIndex = z;
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

                    foreach (var child in BoardCanvas.Children)
                    {
                        child.RenderTransform = translate;
                    }

                    CanvasOffsetX = offsetX;
                    CanvasOffsetY = offsetY;

                    VM.CanvasHexRadius = size;
                }

                VM.CanRaiseStackedPieces = maxStack > 0;
            }

            LastBoard = board;
        }

        private static Point Min(Point center, double size, Point minPoint)
        {
            double minX = Math.Min(minPoint.X, center.X - size);
            double minY = Math.Min(minPoint.Y, center.Y - size);

            return new Point(minX, minY);
        }

        private static Point Max(Point center, double size, Point maxPoint)
        {
            double maxX = Math.Max(maxPoint.X, center.X + size);
            double maxY = Math.Max(maxPoint.Y, center.Y + size);

            return new Point(maxX, maxY);
        }

        private Point GetPoint(Position position, double size, HexOrientation hexOrientation, bool stackShift = false)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
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

        private static Dictionary<int, List<Tuple<PieceName, Position>>> GetPiecesOnBoard(Board board, out int numPieces, out int maxStack)
        {
            if (board is null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            numPieces = 0;
            maxStack = -1;

            Dictionary<int, List<Tuple<PieceName, Position>>> pieces = new Dictionary<int, List<Tuple<PieceName, Position>>>
            {
                [0] = new List<Tuple<PieceName, Position>>()
            };

            PieceName targetPieceName = MainViewModel.AppVM.EngineWrapper.TargetPiece;
            Position? targetPosition = MainViewModel.AppVM.EngineWrapper.TargetPosition;

            bool targetPieceInPlay = false;

            // Add pieces already on the board
            foreach (PieceName pieceName in board.GetPiecesInPlay())
            {
                Position position = board.GetPosition(pieceName);

                if (pieceName == targetPieceName)
                {
                    if (targetPosition.HasValue)
                    {
                        position = targetPosition.Value;
                    }
                    targetPieceInPlay = true;
                }

                int stack = position.Stack;
                maxStack = Math.Max(maxStack, stack);

                if (!pieces.ContainsKey(stack))
                {
                    pieces[stack] = new List<Tuple<PieceName, Position>>();
                }

                pieces[stack].Add(new Tuple<PieceName, Position>(pieceName, position));
                numPieces++;
            }

            // Add piece being placed on the board
            if (!targetPieceInPlay && targetPosition.HasValue)
            {
                int stack = targetPosition.Value.Stack;
                maxStack = Math.Max(maxStack, stack);

                if (!pieces.ContainsKey(stack))
                {
                    pieces[stack] = new List<Tuple<PieceName, Position>>();
                }

                pieces[stack].Add(new Tuple<PieceName, Position>(targetPieceName, targetPosition.Value));
                numPieces++;
            }

            return pieces;
        }

        private Shape GetHex(Point center, double size, HexType hexType, HexOrientation hexOrientation)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
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
                    hex.Stroke = PieceOutlineBrush;
                    break;
                case HexType.BlackPiece:
                    hex.Fill = BlackBrush;
                    hex.Stroke = PieceOutlineBrush;
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
            PathFigure figure = new PathFigure
            {
                IsClosed = true
            };

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
                    figure.Segments.Add(new ArcSegment() {
                        Point = p2,
                        SweepDirection = SweepDirection.CounterClockwise,
                    });
                }
            }

            data.Figures.Add(figure);
            hex.Data = data;

            return hex;
        }

        private Border GetPieceText(Point center, double size, PieceName pieceName, bool disabled)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            SolidColorBrush bugBrush = MainViewModel.ViewerConfig.PieceColors ? BugBrushes[(int)Enums.GetBugType(pieceName)] : (Enums.GetColor(pieceName) == PlayerColor.White ? BlackBrush : WhiteBrush);

            // Create text
            string text = pieceName.ToString().Substring(1);
            TextBlock bugText = new TextBlock
            {
                Text = MainViewModel.ViewerConfig.AddPieceNumbers ? text : text.TrimEnd('1', '2', '3'),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Arial Black"),
                FontSize = size * 0.75,
                Foreground = disabled ? MixSolidColorBrushes(bugBrush, DisabledPieceBrush) : bugBrush,
            };

            Canvas.SetLeft(bugText, center.X - (bugText.Text.Length * (bugText.FontSize / 3.0)));
            Canvas.SetTop(bugText, center.Y - (bugText.FontSize / 2.0));

            Border b = new Border() { Height = size * 2.0, Width = size * 2.0 };
            b.Child = bugText;

            Canvas.SetLeft(b, center.X - (b.Width / 2.0));
            Canvas.SetTop(b, center.Y - (b.Height / 2.0));

            return b;
        }

        private Border GetPieceGraphics(Point center, double size, PieceName pieceName, bool disabled)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            SolidColorBrush bugBrush = MainViewModel.ViewerConfig.PieceColors ? BugBrushes[(int)Enums.GetBugType(pieceName)] : (Enums.GetColor(pieceName) == PlayerColor.White ? BlackBrush : WhiteBrush);

            // Create bug
            Path bugPath = new Path()
            {
                Data = Geometry.Parse(BugPathGeometries[(int)Enums.GetBugType(pieceName)]),
                Stretch = Stretch.Uniform,
                Fill = disabled ? MixSolidColorBrushes(bugBrush, DisabledPieceBrush) : bugBrush,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            Grid safeGrid = new Grid() { Height = size * 2.0, Width = size * 2.0 };

            Grid bugGrid = new Grid() { Height = size * 2.0 * Math.Sin(Math.PI / 6) * GraphicalBugSizeRatio, Width = size * 2.0 * Math.Sin(Math.PI / 6) * GraphicalBugSizeRatio };
            bugGrid.Children.Add(bugPath);

            safeGrid.Children.Add(bugGrid);

            // Bug rotation
            double rotateAngle = MainViewModel.ViewerConfig.HexOrientation == HexOrientation.PointyTop ? -90.0 : 0.0;

            if (int.TryParse(pieceName.ToString().Last().ToString(), out int bugNum))
            {
                rotateAngle += (bugNum - 1) * 60.0;

                if (MainViewModel.ViewerConfig.AddPieceNumbers)
                {
                    // Add bug number
                    TextBlock bugText = new TextBlock
                    {
                        Text = bugNum.ToString(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontFamily = new FontFamily("Arial Black"),
                        FontSize = size * 0.5,
                        Foreground = Enums.GetColor(pieceName) == PlayerColor.White ? WhiteBrush : BlackBrush,
                    };

                    Ellipse bugTextEllipse = new Ellipse()
                    {
                        HorizontalAlignment = bugText.HorizontalAlignment,
                        VerticalAlignment = bugText.VerticalAlignment,
                        Width = bugText.FontSize,
                        Height = bugText.FontSize,
                        Fill = bugPath.Fill,
                    };

                    safeGrid.Children.Add(bugTextEllipse);
                    safeGrid.Children.Add(bugText);
                }
            }

            bugGrid.RenderTransform = new RotateTransform(rotateAngle);
            bugGrid.RenderTransformOrigin = RelativePoint.Center;

            Border b = new Border() { Height = size * 2.0, Width = size * 2.0 };
            b.Child = safeGrid;

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
            for (int bt = 0; bt < (int)BugType.NumBugTypes; bt++)
            {
                var bugType = (BugType)bt;
                if (pieceCanvases.ContainsKey(bugType))
                {
                    if (MainViewModel.ViewerConfig.StackPiecesInHand)
                    {
                        int startingCount = pieceCanvases[bugType].Count;

                        Canvas bugStack = new Canvas()
                        {
                            Height = pieceCanvases[bugType].Peek().Height * (1 + startingCount * BaseStackShiftLevel),
                            Width = pieceCanvases[bugType].Peek().Width * (1 + startingCount * BaseStackShiftLevel),
                            Margin = new Thickness(PieceCanvasMargin),
                            Background = new SolidColorBrush(Colors.Transparent),
                        };

                        while (pieceCanvases[bugType].Count > 0)
                        {
                            Canvas pieceCanvas = pieceCanvases[bugType].Pop();
                            Canvas.SetTop(pieceCanvas, pieceCanvas.Height * ((startingCount - pieceCanvases[bugType].Count - 1) * BaseStackShiftLevel));
                            Canvas.SetLeft(pieceCanvas, pieceCanvas.Width * ((startingCount - pieceCanvases[bugType].Count - 1) * BaseStackShiftLevel));
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

        private Canvas GetPieceInHandCanvas(PieceName pieceName, double size, HexOrientation hexOrientation, bool disabled)
        {
            Point center = new Point(size, size);

            HexType hexType = (Enums.GetColor(pieceName) == PlayerColor.White) ? HexType.WhitePiece : HexType.BlackPiece;

            Shape hex = GetHex(center, size, hexType, hexOrientation);
            var hexText = MainViewModel.ViewerConfig.PieceStyle == PieceStyle.Text ? GetPieceText(center, size, pieceName, disabled) : GetPieceGraphics(center, size, pieceName, disabled);

            Canvas pieceCanvas = new Canvas
            {
                Height = size * 2,
                Width = size * 2,
                Margin = new Thickness(PieceCanvasMargin),
                Background = new SolidColorBrush(Colors.Transparent),
                Name = pieceName.ToString()
            };

            pieceCanvas.Children.Add(hex);
            pieceCanvas.Children.Add(hexText);

            // Add highlight if the piece is selected
            if (MainViewModel.AppVM.EngineWrapper.TargetPiece == pieceName)
            {
                Shape highlightHex = GetHex(center, size, HexType.SelectedPiece, hexOrientation);
                pieceCanvas.Children.Add(highlightHex);
            }

            pieceCanvas.PointerReleased += PieceCanvas_Click;

            return pieceCanvas;
        }

        private void PieceCanvas_Click(object sender, PointerReleasedEventArgs e)
        {
            if (sender is Canvas pieceCanvas)
            {
                if (e.InitialPressMouseButton == MouseButton.Left)
                {
                    PieceName clickedPiece = Enum.Parse<PieceName>(pieceCanvas.Name);
                    MainViewModel.PieceClick(clickedPiece);
                    e.Handled = true;
                }
                else if (e.InitialPressMouseButton == MouseButton.Right)
                {
                    MainViewModel.CancelClick();
                    e.Handled = true;
                }
            }
        }

        private void BoardCanvas_Click(object sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                Point point = e.GetPosition(BoardCanvas);
                VM.CanvasClick(point.X - CanvasOffsetX, point.Y - CanvasOffsetY);
                e.Handled = true;
            }
            else if (e.InitialPressMouseButton == MouseButton.Right)
            {
                MainViewModel.CancelClick();
                e.Handled = true;
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            MainViewModel.CancelClick();
        }

        private DateTime LastRedrawOnSizeChange = DateTime.Now;

        private void TryRedraw()
        {
            if (DateTime.Now - LastRedrawOnSizeChange > TimeSpan.FromMilliseconds(20))
            {
                DrawBoard(LastBoard);
                LastRedrawOnSizeChange = DateTime.Now;
            }
        }

        private void BoardCanvas_SizeChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == Canvas.BoundsProperty)
            {
                TryRedraw();
            }
        }
    }
}
