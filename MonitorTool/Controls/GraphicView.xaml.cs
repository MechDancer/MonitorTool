using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MonitorTool.Source;

namespace MonitorTool.Controls {
	public sealed partial class GraphicView {
		private readonly Dictionary<string, Color> _colors
			= new Dictionary<string, Color>();

		public readonly Dictionary<string, List<Vector2>> Points
			= new Dictionary<string, List<Vector2>>();

		public readonly ViewModel ViewModelContext;

		public GraphicView() {
			InitializeComponent();
			ViewModelContext = new ViewModel(Canvas2D);
		}

		/// <summary>
		/// 	设置颜色
		/// </summary>
		/// <param name="topic">话题名字</param>
		public Color this[string topic] {
			set {
				var item = (MainList.Items ?? throw new MemberAccessException())
				          .OfType<ColorItem>()
				          .SingleOrDefault(it => it.Name == topic);
				if (item == null) {
					item = new ColorItem {
						                     Name  = topic,
						                     Color = value,
						                     Update = it => {
							                              _colors[it.Name] = it.Color;
							                              Canvas2D.Invalidate();
						                              }
					                     };
					_colors[topic] = value;
					MainList.Items.Add(item);
					MainList.SelectedItems.Add(item);
				} else
					item.Color = value;
			}
		}

		public (Vector2, Vector2) Range {
			get => (new Vector2(ViewModelContext.X0, ViewModelContext.Y0),
			        new Vector2(ViewModelContext.X1, ViewModelContext.Y1));
			set {
				ViewModelContext.X0 = value.Item1.X;
				ViewModelContext.Y0 = value.Item1.Y;
				ViewModelContext.X1 = value.Item2.X;
				ViewModelContext.Y1 = value.Item2.Y;
			}
		}

		public void Update() => Canvas2D.Invalidate();

		private void CanvasControl_OnDraw(CanvasControl sender, CanvasDrawEventArgs args) {
			sender.ClearColor = ViewModelContext.Background;

			var (p0, p1) = Range;
			var width  = (float) sender.ActualWidth;
			var height = (float) sender.ActualHeight;
			var size   = p1 - p0;
			var kX     = width  / size.X;
			var kY     = height / size.Y;

			var c0 = (p0 + p1)                  / 2;
			var c1 = new Vector2(width, height) / 2;

			if (ViewModelContext.Proportional) kX = kY = Math.Min(kX, kY);

			{
				Vector2 Reverse(Vector2 origin) {
					var move = origin - c1;
					return new Vector2(move.X / kX, move.Y / -kY) + c0;
				}

				var tp0 = Reverse(new Vector2(0,     height));
				var tp1 = Reverse(new Vector2(width, 0));
				X0Text.Text = ((int) tp0.X).ToString(CultureInfo.CurrentCulture);
				X1Text.Text = ((int) tp1.X).ToString(CultureInfo.CurrentCulture);
				Y0Text.Text = ((int) tp0.Y).ToString(CultureInfo.CurrentCulture);
				Y1Text.Text = ((int) tp1.Y).ToString(CultureInfo.CurrentCulture);
			}

			Vector2 Transform(Vector2 origin) {
				var move = origin - c0;
				return new Vector2(kX * move.X, -kY * move.Y) + c1;
			}

			var random = new Random();
			var buffer = new byte[3];
			foreach (var name in Points.Keys)
				if (!_colors.ContainsKey(name)) {
					random.NextBytes(buffer);
					this[name] = Color.FromArgb(255, buffer[0], buffer[1], buffer[2]);
				}

			var visible = MainList.SelectedItems.OfType<ColorItem>().Select(it => it.Name);
			foreach (var (name, list) in Points.Where(it => visible.Contains(it.Key))) {
				Vector2? last  = null;
				var      color = _colors[name];
				foreach (var p in list) {
					var onCanvas = Transform(p);
					args.DrawingSession.FillCircle(onCanvas, 4, color);

					if (last != null && ViewModelContext.Connection)
						args.DrawingSession.DrawLine(last.Value, onCanvas, color, 1);

					last = onCanvas;
				}
			}
		}

		private void GraphicView_OnUnloaded(object sender, RoutedEventArgs e) {
			Canvas2D.RemoveFromVisualTree();
			Canvas2D = null;
		}

		private void MainList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
			=> Canvas2D.Invalidate();

		public class ViewModel : BindableBase {
			private readonly CanvasControl _canvas;
			private          Color         _background = Colors.Transparent;
			private          bool          _connection;
			private          bool          _proportional = true;

			private float _x0, _x1, _y0, _y1;

			public ViewModel(CanvasControl canvas)
				=> _canvas = canvas;

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
		}
	}
}