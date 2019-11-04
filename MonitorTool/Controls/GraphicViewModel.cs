using System;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml.Media;
using MechDancer.Common;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MonitorTool.Source;

namespace MonitorTool.Controls {
    public class GraphicViewModel : BindableBase {
        private bool _autoMove,
                     _autoX = true,
                     _autoY = true;

        private Color _background = Colors.Transparent;
        private bool  _proportional;
        private bool  _command;
        private bool  _frozen;

        private float _x0, _x1, _y0, _y1;

        public byte          BorderWidth = 10;
        public CanvasControl Canvas;
        public GraphicConfig DefaultConfig = new GraphicConfig("default");

        /// <summary>
        ///     设置最小绘图范围
        /// </summary>
        public (Vector2, Vector2) Range {
            get => (new Vector2(_x0, _y0), new Vector2(_x1, _y1));
            set {
                _x0 = value.Item1.X;
                _y0 = value.Item1.Y;
                _x1 = value.Item2.X;
                _y1 = value.Item2.Y;
                Canvas.Invalidate();
            }
        }

        /// <summary>
        ///     设置自动范围
        /// </summary>
        public bool AutoX {
            get => _autoX;
            set {
                if (SetProperty(ref _autoX, value)) Canvas.Invalidate();
            }
        }

        public bool AutoY {
            get => _autoY;
            set {
                if (SetProperty(ref _autoY, value)) Canvas.Invalidate();
            }
        }

        public bool AutoMove {
            get => _autoMove;
            set {
                if (SetProperty(ref _autoMove, value)) Canvas.Invalidate();
            }
        }

        public bool Proportional {
            get => _proportional;
            set {
                if (SetProperty(ref _proportional, value)) Canvas.Invalidate();
            }
        }

        public bool Command {
            get => _command;
            set {
                if (SetProperty(ref _command, value)) Canvas.Invalidate();
            }
        }

        public bool Frozen {
            get => _frozen;
            set => SetProperty(ref _frozen, value);
        }

        public Color Background {
            get => _background;
            set {
                if (!SetProperty(ref _background, value)) return;
                Notify(nameof(BackgroundBrush));
                Canvas.ClearColor = value;
            }
        }

        public Brush BackgroundBrush => new SolidColorBrush(_background);

        public void BuildTransform(out Func<Vector3, Vector3> transform,
                                   out Func<Vector3, Vector3> reverse) {
            var width  = (float) Canvas.ActualWidth;
            var height = (float) Canvas.ActualHeight;

            var (p0, p1) = Range;
            var size = p1 - p0;
            var c0   = (p0 + p1)                  / 2;
            var c1   = new Vector2(width, height) / 2;

            var kX = (width  - 2 * BorderWidth) / size.X;
            var kY = (height - 2 * BorderWidth) / size.Y;

            if (_proportional) kX = kY = Math.Min(kX, kY);

            transform = p => p.Let(it => new Vector2(it.X, it.Y) - c0)
                              .Let(it => new Vector3(new Vector2(it.X * kX, it.Y * -kY) + c1, -p.Z));
            reverse = p => p.Let(it => new Vector2(it.X, it.Y) - c1)
                            .Let(it => new Vector3(new Vector2(it.X / kX, it.Y / -kY) + c0, -p.Z));
        }
    }
}