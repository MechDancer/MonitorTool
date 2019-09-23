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
        private readonly ConcurrentDictionary<string, GraphicConfig> _configs
            = new ConcurrentDictionary<string, GraphicConfig>();

        private readonly ConcurrentDictionary<string, List<Vector3>> _points
            = new ConcurrentDictionary<string, List<Vector3>>();

        public readonly GraphicViewModel ViewModelContext;

        private IEnumerable<string> Topics
            => Global
              .Instance
              .Receiver
              .Ports
              .Keys
              .Select(it => it.ToString())
              .WhereNot(it => _points.ContainsKey(it))
              .ToList();

        #region Private

        private RangeState _state = RangeState.Idle;
        private Point      _origin, _current;
        private DateTime   _pressTime;

        #region Links

        private readonly List<IDisposable> _links = new List<IDisposable>();

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selection = (string) e.AddedItems.Single();
            if (_points.ContainsKey(selection)) return;
            var (info, port) =
                Global.Instance.Receiver.Ports.Single(it => it.Key.ToString() == selection);
            port.LinkTo(new ActionBlock<List<Vector3>>(
                            frame => {
                                var topic = info.ToString();
                                var list  = _points.GetOrAdd(topic, _ => new List<Vector3>());
                                var config = _configs.GetOrAdd(topic,
                                    _ => {
                                        var it = new GraphicConfig(topic);
                                        it.PropertyChanged += (__, ___) => Canvas2D.Invalidate();
                                        MainList.Dispatcher.RunAsync(
                                            Windows.UI.Core.CoreDispatcherPriority.Normal,
                                            () => {
                                                MainList.Items?.Add(it);
                                                MainList.SelectedItems.Add(it);
                                            });
                                        return it;
                                    });
                                lock (list) {
                                    if (info.Type == GraphType.Frame) list.Clear();
                                    list.AddRange(frame);

                                    var verbose = list.Count - _configs[topic].SaveCount;
                                    if (verbose > 0) list.RemoveRange(0, verbose);
                                }

                                Canvas2D.Invalidate();
                            }), new DataflowLinkOptions())
                .Also(_links.Add);
        }

        private void GraphicView_OnUnloaded(object sender, RoutedEventArgs e) {
            foreach (var link in _links) link.Dispose();
            Canvas2D.RemoveFromVisualTree();
            Canvas2D = null;
        }

        #endregion

        public GraphicView(GraphicViewModel context = null) {
            InitializeComponent();
            ViewModelContext = context?.Also(it => it.Canvas = Canvas2D)
                            ?? new GraphicViewModel {Canvas = Canvas2D};
        }

        /// <summary>
        ///     计算范围
        /// </summary>
        /// <param name="current">当前范围</param>
        /// <param name="points">目标点集</param>
        /// <param name="x">确定横轴</param>
        /// <param name="y">确定纵轴</param>
        /// <param name="allowShrink">允许范围缩小</param>
        /// <returns>目标范围</returns>
        private static (Vector2, Vector2) CalculateRange(
            (Vector2, Vector2)   current,
            IEnumerable<Vector3> points,
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
                        } else {
                            x0 = x1 - w;
                        }
                    } else {
                        x1 = x0 + w;
                    }

                if (y1 - y0 < h)
                    if (min.Y <= y0) {
                        if (y1 <= max.Y) {
                            y0 = min.Y;
                            y1 = max.Y;
                        } else {
                            y0 = y1 - h;
                        }
                    } else {
                        y1 = y0 + h;
                    }
            }

            return (new Vector2(x0, y0), new Vector2(x1, y1));
        }

        /// <summary>
        ///     给两个数字排序
        /// </summary>
        /// <param name="a">数字1</param>
        /// <param name="b">数字2</param>
        /// <param name="min">较小的</param>
        /// <param name="max">较大的</param>
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
            // 保存画笔
            var brush = args.DrawingSession;

            // 排除不画的
            var visible = MainList
                         .SelectedItems
                         .OfType<GraphicConfig>()
                         .Select(it => it.Name)
                         .ToHashSet();
            var points = (from entry in _points
                          where visible.Contains(entry.Key)
                          select entry)
                        .SelectNotNull(it => {
                                           var (topic, list) = it;
                                           var count = _configs[topic].ShowCount;
                                           if (count <= 0) return null;
                                           lock (list)
                                               return list.TakeLast(count)
                                                          .ToImmutableList()
                                                          .TakeUnless(x => x.IsEmpty)
                                                         ?.Let(x => Tuple.Create(topic, x));
                                       })
                        .ToImmutableDictionary(it => it.Item1, it => it.Item2);
            if (points.None()) return;

            // 自动范围
            var range =
                ViewModelContext.AutoMove
                    ? CalculateRange
                    (points: points.Values.Select(it => it.Last()),
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

            // 保存参数
            var width  = (float) sender.ActualWidth;
            var height = (float) sender.ActualHeight;
            ViewModelContext.BuildTransform(out var transform, out var reverse);
            // 计算范围指定
            switch (_state) {
                case RangeState.Idle:
                    // 空闲状态
                    break;
                case RangeState.Normal:
                    var x = (float) _current.X;
                    var y = (float) _current.Y;
                    brush.DrawLine(new Vector2(x, 0),     new Vector2(x,     height), Colors.White);
                    brush.DrawLine(new Vector2(0, y),     new Vector2(width, y),      Colors.White);
                    brush.DrawLine(new Vector2(0, y + 1), new Vector2(width, y + 1),  Colors.Black);
                    brush.DrawLine(new Vector2(x + 1, 0), new Vector2(x + 1, height),
                                   Colors.Black);
                    var p = reverse(new Vector3(x, y, float.NaN));
                    brush.DrawText($"{p.X}, {p.Y}", x + 1, y - 23, Colors.Black);
                    brush.DrawText($"{p.X}, {p.Y}", x,     y - 24, Colors.White);
                    break;
                case RangeState.Reset:
                    // 正在重新划定范围
                    brush.DrawRoundedRectangle(new Rect(new Point(_origin.X  - 1, _origin.Y  - 1),
                                                        new Point(_current.X - 1, _current.Y - 1)),
                                               0, 0, Colors.White);
                    brush.DrawRoundedRectangle(new Rect(_origin, _current),
                                               0, 0, Colors.Black);
                    break;
                case RangeState.Done:
                    // 确定范围
                    _state                 = RangeState.Normal;
                    ViewModelContext.AutoX = ViewModelContext.AutoY = false;
                    Order(_origin.X, _current.X, out var x0, out var x1);
                    Order(_origin.Y, _current.Y, out var y0, out var y1);
                    ViewModelContext.Range =
                        (reverse(new Vector3(x0, y1, float.NaN)).Let(it => new Vector2(it.X, it.Y)),
                         reverse(new Vector3(x1, y0, float.NaN)).Let(it => new Vector2(it.X, it.Y)));
                    ViewModelContext.BuildTransform(out transform, out reverse);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            {
                // 计算实际范围显示
                var tp0 = reverse(new Vector3(0, height, float.NaN));
                X0Text.Text = ((long) tp0.X).ToString(CultureInfo.CurrentCulture);
                Y0Text.Text = ((long) tp0.Y).ToString(CultureInfo.CurrentCulture);

                var tp1 = reverse(new Vector3(width, 0, float.NaN));
                X1Text.Text = ((long) tp1.X).ToString(CultureInfo.CurrentCulture);
                Y1Text.Text = ((long) tp1.Y).ToString(CultureInfo.CurrentCulture);
            }

            // 画一个位姿
            void DrawPose(Vector3 point, Color color) {
                var p = new Vector2(point.X, point.Y);
                var d = point.Z;
                brush.FillCircle(p, 2, color);
                if (!float.IsNaN(point.Z))
                    brush.DrawLine(p, p + 20 * new Vector2(MathF.Cos(d), MathF.Sin(d)), color, 1);
            }

            // 画点
            foreach (var (name, list) in points) {
                Vector3  onCanvas;
                Vector3? last   = null;
                var      config = _configs[name];
                var      color  = config.Color;
                foreach (var p in list.SkipLast(1).ToArray()) {
                    onCanvas = transform(p);
                    DrawPose(onCanvas, color);

                    if (last != null && ViewModelContext.Connection)
                        brush.DrawLine(last.Value.Let(it => new Vector2(it.X, it.Y)),
                                       new Vector2(onCanvas.X, onCanvas.Y), color, 1);
                    last = onCanvas;
                }

                onCanvas = transform(list.Last());
                DrawPose(onCanvas, color);
                if (last != null && ViewModelContext.Connection)
                    brush.DrawLine(last.Value.Let(it => new Vector2(it.X, it.Y)),
                                   new Vector2(onCanvas.X, onCanvas.Y), color, 1);
            }
        }

        #region Pointer

        private void MainList_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => Canvas2D.Invalidate();

        private void Canvas2D_OnPointerPressed(object sender, PointerRoutedEventArgs e) {
            _state     = RangeState.Reset;
            _origin    = e.GetCurrentPoint(Canvas2D).Position;
            _pressTime = DateTime.Now;
        }

        private void Canvas2D_OnPointerMoved(object sender, PointerRoutedEventArgs e) {
            _current = e.GetCurrentPoint(Canvas2D).Position;
            if (_state == RangeState.Reset) Canvas2D.Invalidate();
        }

        private void Canvas2D_OnPointerCanceled(object sender, PointerRoutedEventArgs e) => _state = RangeState.Normal;

        private void Canvas2D_OnPointerEntered(object sender, PointerRoutedEventArgs e) => _state = RangeState.Normal;

        private void Canvas2D_OnPointerExited(object sender, PointerRoutedEventArgs e) => _state = RangeState.Idle;

        private void Canvas2D_OnPointerReleased(object sender, PointerRoutedEventArgs e) {
            _state = DateTime.Now - _pressTime < TimeSpan.FromSeconds(0.2)
                         ? RangeState.Normal
                         : RangeState.Done;
            Canvas2D.Invalidate();
        }

        private void Canvas2D_OnPointerWheelChanged(object sender, PointerRoutedEventArgs e) {
            _state                 = RangeState.Normal;
            ViewModelContext.AutoX = ViewModelContext.AutoY = false;

            var pointer = e.GetCurrentPoint(Canvas2D);
            var point   = pointer.Position.Let(it => new Vector2((float) it.X, (float) it.Y));
            var scale   = pointer.Properties.MouseWheelDelta / 480f;

            ViewModelContext.BuildTransform(out _, out var reverse);

            Vector2 Transform(Vector2 p)
                => reverse(new Vector3((p - point) * (1 - scale) + point, float.NaN))
                   .Let(it => new Vector2(it.X, it.Y));

            var w = (float) Canvas2D.ActualWidth;
            var h = (float) Canvas2D.ActualHeight;
            ViewModelContext.Range = (Transform(new Vector2(0, h)),
                                      Transform(new Vector2(w, 0)));

            Canvas2D.Invalidate();
        }

        private enum RangeState : byte {
            Idle,
            Normal,
            Reset,
            Done
        }

        #endregion

        #endregion
    }
}