using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace MonitorTool.Controls {
	public sealed partial class GraphicView {
		public readonly List<Vector2>      List  = new List<Vector2>();
		public          (Vector2, Vector2) Range = (Vector2.Zero, Vector2.Zero);
		public GraphicView() => InitializeComponent();

		private void CanvasControl_OnDraw(CanvasControl sender, CanvasDrawEventArgs args) {
			var (p0, p1) = Range;
			foreach (var p in from it in List
							  where p0.X < it.X && it.X < p1.X
							  where p0.Y < it.Y && it.Y < p1.Y
							  select it)
				args.DrawingSession.DrawCircle(p - p0, 2, Colors.White);
		}

		private void GraphicView_OnUnloaded(object sender, RoutedEventArgs e) {
			Canvas.RemoveFromVisualTree();
			Canvas = null;
		}
	}
}