using System.Net;
using Windows.UI.Xaml;

namespace MonitorTool.Controls {
	public sealed partial class GroupSelector {
		public delegate bool ButtonClickHandler(IPEndPoint group);

		public ButtonClickHandler ButtonClick;

		public GroupSelector() => InitializeComponent();

		private void Done_OnClick(object sender, RoutedEventArgs e) {
			if (!IPAddress.TryParse(Ip.Text, out var ip)) return;
			if (!ushort.TryParse(Port.Text, out var port)) return;
			if (!ButtonClick(new IPEndPoint(ip, port))) return;
			AddButton.Visibility = Visibility.Visible;
			Ip.Text              = Port.Text = "";
		}

		private void Cancel_OnClick(object sender, RoutedEventArgs e) {
			AddButton.Visibility = Visibility.Visible;
			Ip.Text              = Port.Text = "";
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
			=> AddButton.Visibility = Visibility.Collapsed;
	}
}
