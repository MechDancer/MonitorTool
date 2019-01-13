using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace MonitorTool.Controls {
	public sealed partial class GraphicView {
		public readonly Dictionary<string, (Color, List<Vector2>)> Content
			= new Dictionary<string, (Color, List<Vector2>)>();

		public bool Connection   = false;
		public bool Proportional = true;

		public (Vector2, Vector2) Range = (Vector2.Zero, Vector2.Zero);

		public GraphicView() => InitializeComponent();

		private void CanvasControl_OnDraw(CanvasControl sender, CanvasDrawEventArgs args) {
			var (p0, p1) = Range;
			var width  = (float) sender.ActualWidth;
			var height = (float) sender.ActualHeight;
			var size   = p1 - p0;
			var kX     = width  / size.X;
			var kY     = height / size.Y;

			var c0 = (p0 + p1)                  / 2;
			var c1 = new Vector2(width, height) / 2;

			if (Proportional) kX = kY = Math.Min(kX, kY);

			Vector2 Transform(Vector2 origin) {
				var move = origin - c0;
				return new Vector2(kX * move.X, -kY * move.Y) + c1;
			}

			foreach (var (a, b) in new[] {
											 (p0, new Vector2(p0.X, p1.Y)), (new Vector2(p0.X, p1.Y), p1),
											 (p1, new Vector2(p1.X, p0.Y)), (new Vector2(p1.X, p0.Y), p0)
										 })
				args.DrawingSession.DrawLine(Transform(a), Transform(b), Colors.Azure, 3);

			foreach (var (_, (color, list)) in Content) {
				Vector2? last = null;
				foreach (var p in list) {
					var onCanvas = Transform(p);
					args.DrawingSession.FillCircle(onCanvas, 4, color);

					if (last != null && Connection)
						args.DrawingSession.DrawLine(last.Value, onCanvas, color, 1);

					last = onCanvas;
				}
			}
		}

		private void GraphicView_OnUnloaded(object sender, RoutedEventArgs e) {
			Canvas.RemoveFromVisualTree();
			Canvas = null;
		}
	}
}