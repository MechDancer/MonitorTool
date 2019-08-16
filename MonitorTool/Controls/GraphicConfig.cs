using Windows.UI;
using Windows.UI.Xaml.Media;
using MonitorTool.Source;

namespace MonitorTool.Controls {
	/// <inheritdoc />
	/// <summary>
	/// 图形配置
	/// </summary>
	public class GraphicConfig : BindableBase {
		public readonly string Name;
		private         Color  _color;
		private         int    _count = int.MaxValue;

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="name">名字</param>
		public GraphicConfig(string name) {
			Name   = name;
			_color = Functions.RandomColor;
		}

		/// <summary>
		/// 显示颜色
		/// </summary>
		public Color Color {
			get => _color;
			set {
				if (!SetProperty(ref _color, value)) return;
				Notify(nameof(Brush));
			}
		}

		/// <summary>
		/// 画笔
		/// </summary>
		public Brush Brush => new SolidColorBrush(_color);

        /// <summary>
        /// 显示数量
        /// </summary>
		public int Count {
			get => _count;
			set {
				if (!SetProperty(ref _count, value)) return;
				Notify(nameof(CountText));
			}
		}

		public string CountText {
			get => _count.ToString();
			set {
				if (int.TryParse(value, out var data)) Count = data;
			}
		}
	}
}
