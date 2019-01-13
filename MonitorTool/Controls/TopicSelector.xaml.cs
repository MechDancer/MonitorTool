using Windows.UI.Xaml;

namespace MonitorTool.Controls {
	public sealed partial class TopicSelector {
		public delegate bool ButtonClickHandler(string topic);

		public TopicSelector() => InitializeComponent();

		public event ButtonClickHandler ButtonClick;

		private void Done_OnClick(object sender, RoutedEventArgs e) {
			var topic = Topic.Text;
			if (string.IsNullOrWhiteSpace(topic)) return;
			if (!ButtonClick?.Invoke(Topic.Text) ?? true) return;
			AddButton.Visibility = Visibility.Visible;
			Topic.Text           = "";
		}

		private void Cancel_OnClick(object sender, RoutedEventArgs e) {
			AddButton.Visibility = Visibility.Visible;
			Topic.Text           = "";
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
			=> AddButton.Visibility = Visibility.Collapsed;
	}
}