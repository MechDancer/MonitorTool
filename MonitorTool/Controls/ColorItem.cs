using Windows.UI;
using Windows.UI.Xaml.Media;
using MonitorTool.Source;

namespace MonitorTool.Controls {
	public class ColorItem : BindableBase {
		public delegate void UpdateHandler(ColorItem sender);

		private Color  _color;
		public  string Name;

		public UpdateHandler Update;

		public Color Color {
			get => _color;
			set {
				if (SetProperty(ref _color, value)) Update?.Invoke(this);
			}
		}

		public static Brush ToBrush(Color color) => new SolidColorBrush(color);
	}
}
