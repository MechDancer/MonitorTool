using Windows.UI;
using Windows.UI.Xaml.Media;
using MonitorTool.Source;

namespace MonitorTool.Controls {
	/// <inheritdoc />
	/// <summary>
	///     图形配置
	/// </summary>
	public class GraphicConfig : BindableBase {
		private Color  _color;
		public  string Name;

		public Color Color {
			get => _color;
			set => SetProperty(ref _color, value);
		}

		public static Brush ToBrush(Color color) => new SolidColorBrush(color);
	}
}
