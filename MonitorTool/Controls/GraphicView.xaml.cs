using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace MonitorTool.Controls {
	public sealed partial class GraphicView {
		public readonly Dictionary<string, List<Vector2>> Points
			= new Dictionary<string, List<Vector2>>();

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

		/// <summary>
		/// 	自动调整范围
		/// </summary>
		public bool AutoRange {
			get => _viewModelContext.AutoRange;
			set => _viewModelContext.AutoRange = value;
		}

		/// <summary>
		/// 	设置最小绘图范围
		/// </summary>
		public (Vector2, Vector2) Range {
			get => _viewModelContext.Range;
			set => _viewModelContext.Range = value;
		}

		/// <summary>
		/// 	连线
		/// </summary>
		public bool Connection {
			get => _viewModelContext.Connection;
			set => _viewModelContext.Connection = value;
		}

		/// <summary>
		/// 	保持比例
		/// </summary>
		public bool Proportional {
			get => _viewModelContext.Proportional;
			set => _viewModelContext.Proportional = value;
		}

		/// <summary>
		/// 	强制重新画图
		/// </summary>
		public void Update() => Canvas2D.Invalidate();

		#region Never Mind

		private readonly ViewModel                 _viewModelContext;
		private readonly Dictionary<string, Color> _colors = new Dictionary<string, Color>();

		private RerangeState _state = RerangeState.Idle;
		private Point        _origin, _current;

		public GraphicView() {
			InitializeComponent();
			_viewModelContext = new ViewModel(Canvas2D);
		}

		private (Vector2, Vector2) CalculateRange() {
			var x0 = float.MaxValue;
			var y0 = float.MaxValue;
			var x1 = float.MinValue;
			var y1 = float.MinValue;
			foreach (var list in Points.Values)
			foreach (var vector2 in list) {
				if (vector2.X      < x0) x0 = vector2.X;
				else if (vector2.X > x1) x1 = vector2.X;
				if (vector2.Y      < y0) y0 = vector2.Y;
				else if (vector2.Y > y1) y1 = vector2.Y;
			}

			return (new Vector2(x0, y0), new Vector2(x1, y1));
		}

		private static void Order(double    a,
		                          double    b,
		                          out float min,
		                          out float max) {
			if (a < b) {
				min = (float) a;
				max = (float) b;
			} else {
				min = (float) b;
				max = (float) a;
			}
		}

		private void CanvasControl_OnDraw(CanvasControl sender, CanvasDrawEventArgs args) {
			// 修改背景色
			sender.ClearColor = _viewModelContext.Background;

			// 自动范围
			if (AutoRange) Range = CalculateRange();

			// 保存参数
			var width  = (float) sender.ActualWidth;
			var height = (float) sender.ActualHeight;
			_viewModelContext.BuildTransform(out var transform, out var reverse);

			// 计算范围
			switch (_state) {
				case RerangeState.Idle:
					break;
				case RerangeState.Reset:
					args.DrawingSession
					    .DrawRoundedRectangle(new Rect(_origin, _current), 0, 0, Colors.White);
					break;
				case RerangeState.Done:
					var rect = new Rect(_origin, _current);
					if (rect.Height < 100 || rect.Width < 100)
						break;

					_state    = RerangeState.Idle;
					AutoRange = false;
					Order(_origin.X, _current.X, out var x0, out var x1);
					Order(_origin.Y, _current.Y, out var y0, out var y1);
					_viewModelContext.Range = (reverse(new Vector2(x0, y1)),
					                           reverse(new Vector2(x1, y0)));
					_viewModelContext.BuildTransform(out transform, out reverse);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			{
				var tp0 = reverse(new Vector2(0, height));
				X0Text.Text = ((int) tp0.X).ToString(CultureInfo.CurrentCulture);
				Y0Text.Text = ((int) tp0.Y).ToString(CultureInfo.CurrentCulture);

				var tp1 = reverse(new Vector2(width, 0));
				X1Text.Text = ((int) tp1.X).ToString(CultureInfo.CurrentCulture);
				Y1Text.Text = ((int) tp1.Y).ToString(CultureInfo.CurrentCulture);
			}

			var random = new Random();
			var buffer = new byte[3];
			foreach (var name in Points.Keys)
				if (!_colors.ContainsKey(name)) {
					random.NextBytes(buffer);
					this[name] = Color.FromArgb(255, buffer[0], buffer[1], buffer[2]);
				}

			var r       = (int) Math.Min(width, height) / 400 + 2;
			var visible = MainList.SelectedItems.OfType<ColorItem>().Select(it => it.Name);
			foreach (var (name, list) in Points.Where(it => visible.Contains(it.Key))) {
				Vector2? last  = null;
				var      color = _colors[name];
				foreach (var p in list) {
					var onCanvas = transform(p);
					args.DrawingSession.FillCircle(onCanvas, r, color);

					if (last != null && _viewModelContext.Connection)
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

		private void Canvas2D_OnPointerPressed(object sender, PointerRoutedEventArgs e) {
			_state  = RerangeState.Reset;
			_origin = e.GetCurrentPoint(Canvas2D).Position;
		}

		private void Canvas2D_OnPointerMoved(object sender, PointerRoutedEventArgs e) {
			_current = e.GetCurrentPoint(Canvas2D).Position;
			if (_state == RerangeState.Reset) Canvas2D.Invalidate();
		}

		private void Canvas2D_OnPointerCanceled(object sender, PointerRoutedEventArgs e)
			=> _state = RerangeState.Idle;

		private void Canvas2D_OnPointerReleased(object sender, PointerRoutedEventArgs e) {
			_state = RerangeState.Done;
			Canvas2D.Invalidate();
		}

		private enum RerangeState : byte {
			Idle,
			Reset,
			Done
		}

		#endregion
	}
}