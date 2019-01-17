using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks.Dataflow;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using MechDancer.Common;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MonitorTool.Source;

namespace MonitorTool.Controls {
	public sealed partial class GraphicView {
		private readonly ConcurrentDictionary<string, Color> _colors
			= new ConcurrentDictionary<string, Color>();

		private readonly ConcurrentDictionary<string, List<Vector2>> _points
			= new ConcurrentDictionary<string, List<Vector2>>();

		private IEnumerable<string> Topics
			=> Global
			  .Instance
			  .Helpers
			  .Keys
			  .Select(it => $"{it.Item1}: {it.Item2}")
			  .WhereNot(it => _points.ContainsKey(it))
			  .ToList();

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

		public void Operate(string sender, string topic, Action<List<Vector2>> action) {
			lock (_points) {
				var id = $"{sender}: {topic}";
				if (_points.TryGetValue(id, out var list))
					action(list);
				else
					_points[id] = new List<Vector2>().Also(action);
			}

			Canvas2D.Invalidate();
		}

		private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
			var selection = (string) e.AddedItems.Single();
			if (_points.ContainsKey(selection)) return;
			var ((name, topic), helper) =
				Global.Instance.Helpers.Single(it => $"{it.Key.Item1}: {it.Key.Item2}" == selection);
			helper.Port.LinkTo(new ActionBlock<Vector2>(p => Operate(name, topic, list => list.Add(p))),
			                   new DataflowLinkOptions());
		}

		#region Private

		private RangeState _state = RangeState.Idle;
		private Point      _origin, _current;
		private DateTime   _pressTime;

		public GraphicView(ViewModel context = null) {
			InitializeComponent();
			if (context == null) {
				ViewModelContext = new ViewModel(Canvas2D);
			} else {
				ViewModelContext = new ViewModel(Canvas2D) {
					                                           Range        = context.Range,
					                                           AutoX        = context.AutoX,
					                                           AutoY        = context.AutoY,
					                                           AutoMove     = context.AutoMove,
					                                           Proportional = context.Proportional,
					                                           Connection   = context.Connection,
					                                           Background   = context.Background
				                                           };
			}
		}

		/// <summary>
		/// 	计算范围
		/// </summary>
		/// <param name="current">当前范围</param>
		/// <param name="points">目标点集</param>
		/// <param name="x">确定横轴</param>
		/// <param name="y">确定纵轴</param>
		/// <param name="allowShrink">允许范围缩小</param>
		/// <returns>目标范围</returns>
		private static (Vector2, Vector2) CalculateRange(
			(Vector2, Vector2)   current,
			IEnumerable<Vector2> points,
			bool                 x,
			bool                 y,
			bool                 allowShrink
		) {
			var (min, max) = current;
			var x0 = float.MaxValue;
			var x1 = float.MinValue;
			var y0 = float.MaxValue;
			var y1 = float.MinValue;

			var enumerable = points.ToArray();
			if (x) {
				foreach (var vector2 in enumerable) {
					if (vector2.X < x0) x0 = vector2.X;
					if (vector2.X > x1) x1 = vector2.X;
				}
			} else {
				x0 = min.X;
				x1 = max.X;
			}

			if (y) {
				foreach (var vector2 in enumerable) {
					if (vector2.Y < y0) y0 = vector2.Y;
					if (vector2.Y > y1) y1 = vector2.Y;
				}
			} else {
				y0 = min.Y;
				y1 = max.Y;
			}

			if (!allowShrink) {
				var w = max.X - min.X;
				var h = max.Y - min.Y;

				if (x1 - x0 < w)
					if (min.X <= x0) {
						if (x1 <= max.X) {
							x0 = min.X;
							x1 = max.X;
						} else
							x0 = x1 - w;
					} else
						x1 = x0 + w;

				if (y1 - y0 < h)
					if (min.Y <= y0) {
						if (y1 <= max.Y) {
							y0 = min.Y;
							y1 = max.Y;
						} else
							y0 = y1 - h;
					} else
						y1 = y0 + h;
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
			sender.ClearColor = ViewModelContext.Background;

			// 新图像指定随机颜色
			var random = new Random();
			var buffer = new byte[3];
			foreach (var name in _points.Keys)
				if (!_colors.ContainsKey(name)) {
					random.NextBytes(buffer);
					this[name] = Color.FromArgb(255, buffer[0], buffer[1], buffer[2]);
				}

			// 排除不画的
			var visible = MainList.SelectedItems.OfType<ColorItem>().Select(it => it.Name).ToArray();
			var points = _points.Where(it => visible.Contains(it.Key))
			                    .ToImmutableDictionary(it => it.Key, it => it.Value);

			lock (_points) {
				// 自动范围
				var range =
					ViewModelContext.AutoMove
						? CalculateRange
							(points: from list in points.Values where list.Any() select list.Last(),
							 current: ViewModelContext.Range,
							 x: true,
							 y: true,
							 allowShrink: false)
						: ViewModelContext.Range;
				ViewModelContext.Range = CalculateRange
					(points: points.Values.Flatten(),
					 current: range,
					 x: ViewModelContext.AutoX,
					 y: ViewModelContext.AutoY,
					 allowShrink: true);
			}

			// 保存参数
			var width  = (float) sender.ActualWidth;
			var height = (float) sender.ActualHeight;
			ViewModelContext.BuildTransform(out var transform, out var reverse);
			// 计算范围指定
			switch (_state) {
				case RangeState.Idle:
					// 空闲状态
					break;
				case RangeState.Reset:
					// 正在重新划定范围
					args.DrawingSession
					    .DrawRoundedRectangle(new Rect(new Point(_origin.X  - 1, _origin.Y  - 1),
					                                   new Point(_current.X - 1, _current.Y - 1)),
					                          0, 0, Colors.White);
					args.DrawingSession
					    .DrawRoundedRectangle(new Rect(_origin, _current),
					                          0, 0, Colors.Black);
					break;
				case RangeState.Done:
					// 确定范围
					_state                 = RangeState.Idle;
					ViewModelContext.AutoX = ViewModelContext.AutoY = false;
					Order(_origin.X, _current.X, out var x0, out var x1);
					Order(_origin.Y, _current.Y, out var y0, out var y1);
					ViewModelContext.Range = (reverse(new Vector2(x0, y1)),
					                          reverse(new Vector2(x1, y0)));
					ViewModelContext.BuildTransform(out transform, out reverse);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			{ // 计算实际范围显示
				var tp0 = reverse(new Vector2(0, height));
				X0Text.Text = ((int) tp0.X).ToString(CultureInfo.CurrentCulture);
				Y0Text.Text = ((int) tp0.Y).ToString(CultureInfo.CurrentCulture);

				var tp1 = reverse(new Vector2(width, 0));
				X1Text.Text = ((int) tp1.X).ToString(CultureInfo.CurrentCulture);
				Y1Text.Text = ((int) tp1.Y).ToString(CultureInfo.CurrentCulture);
			}

			// 画点
			var r = (int) Math.Min(width, height) / 400 + 2;
			foreach (var (name, list) in points) {
				Vector2? last  = null;
				var      color = _colors[name];
				foreach (var p in list.ToArray()) {
					var onCanvas = transform(p);
					args.DrawingSession.FillCircle(onCanvas, r, color);

					if (last != null && ViewModelContext.Connection)
						args.DrawingSession.DrawLine(last.Value, onCanvas, color, 1);

					last = onCanvas;
				}
			}
		}

		#region Pointer

		private void GraphicView_OnUnloaded(object sender, RoutedEventArgs e) {
			Canvas2D.RemoveFromVisualTree();
			Canvas2D = null;
		}

		private void MainList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
			=> Canvas2D.Invalidate();

		private void Canvas2D_OnPointerPressed(object sender, PointerRoutedEventArgs e) {
			_state     = RangeState.Reset;
			_origin    = e.GetCurrentPoint(Canvas2D).Position;
			_pressTime = DateTime.Now;
		}

		private void Canvas2D_OnPointerMoved(object sender, PointerRoutedEventArgs e) {
			_current = e.GetCurrentPoint(Canvas2D).Position;
			if (_state == RangeState.Reset) Canvas2D.Invalidate();
		}

		private void Canvas2D_OnPointerCanceled(object sender, PointerRoutedEventArgs e)
			=> _state = RangeState.Idle;

		private void Canvas2D_OnPointerReleased(object sender, PointerRoutedEventArgs e) {
			_state = DateTime.Now - _pressTime < TimeSpan.FromSeconds(0.2)
				         ? RangeState.Idle
				         : RangeState.Done;
			Canvas2D.Invalidate();
		}

		private void Canvas2D_OnPointerWheelChanged(object sender, PointerRoutedEventArgs e) {
			_state                 = RangeState.Idle;
			ViewModelContext.AutoX = ViewModelContext.AutoY = false;

			var pointer = e.GetCurrentPoint(Canvas2D);
			var point   = pointer.Position.Let(it => new Vector2((float) it.X, (float) it.Y));
			var scale   = pointer.Properties.MouseWheelDelta / 480f;

			ViewModelContext.BuildTransform(out _, out var reverse);
			Vector2 Transform(Vector2 p) => reverse((p - point) * (1 - scale) + point);
			var w = (float) Canvas2D.ActualWidth;
			var h = (float) Canvas2D.ActualHeight;
			ViewModelContext.Range = (Transform(new Vector2(0, h)),
			                          Transform(new Vector2(w, 0)));

			Canvas2D.Invalidate();
		}

		private enum RangeState : byte {
			Idle,
			Reset,
			Done
		}

		#endregion

		#endregion
	}
}