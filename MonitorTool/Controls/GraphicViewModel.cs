using System;
using System.Numerics;
using Windows.UI;
using MechDancer.Common;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MonitorTool.Source;

namespace MonitorTool.Controls {
	public sealed partial class GraphicView {
		private readonly ViewModel _viewModelContext;

		private class ViewModel : BindableBase {
			private readonly CanvasControl _canvas;
			private          bool          _autoRange;
			private          Color         _background = Colors.Transparent;
			private          bool          _connection;
			private          bool          _proportional = true;

			private float _x0, _x1, _y0, _y1;

			public ViewModel(CanvasControl canvas) => _canvas = canvas;

			/// <summary>
			/// 	设置最小绘图范围
			/// </summary>
			public (Vector2, Vector2) Range {
				get => (new Vector2(_x0, _y0), new Vector2(_x1, _y1));
				set {
					X0 = value.Item1.X;
					Y0 = value.Item1.Y;
					X1 = value.Item2.X;
					Y1 = value.Item2.Y;
				}
			}

			/// <summary>
			/// 	设置自动范围
			/// </summary>
			public bool AutoRange {
				get => _autoRange;
				set {
					if (SetProperty(ref _autoRange, value))
						_canvas.Invalidate();
				}
			}

			public bool Connection {
				get => _connection;
				set {
					if (SetProperty(ref _connection, value))
						_canvas.Invalidate();
				}
			}

			public bool Proportional {
				get => _proportional;
				set {
					if (SetProperty(ref _proportional, value))
						_canvas.Invalidate();
				}
			}

			public Color Background {
				get => _background;
				set {
					if (SetProperty(ref _background, value))
						_canvas.ClearColor = value;
				}
			}

			public float X0 {
				get => _x0;
				set => SetProperty(ref _x0, value);
			}

			public float X1 {
				get => _x1;
				set => SetProperty(ref _x1, value);
			}

			public float Y0 {
				get => _y0;
				set => SetProperty(ref _y0, value);
			}

			public float Y1 {
				get => _y1;
				set => SetProperty(ref _y1, value);
			}

			public void BuildTransform(out Func<Vector2, Vector2> transform,
			                           out Func<Vector2, Vector2> reverse) {
				var (p0, p1) = Range;
				var width  = (float) _canvas.ActualWidth;
				var height = (float) _canvas.ActualHeight;
				var size   = p1 - p0;
				var c0     = (p0 + p1)                  / 2;
				var c1     = new Vector2(width, height) / 2;

				var kX = width  / size.X;
				var kY = height / size.Y;
				if (_proportional)
					kX = kY = Math.Min(kX, kY);

				transform = p => (p - c0).Let(move => new Vector2(move.X * kX, move.Y * -kY) + c1);
				reverse   = p => (p - c1).Let(move => new Vector2(move.X / kX, move.Y / -kY) + c0);
			}
		}
	}
}