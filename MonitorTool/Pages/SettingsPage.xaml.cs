using System.Net;
using Windows.UI.Xaml.Navigation;
using MonitorTool.Source;

namespace MonitorTool.Pages {
	public sealed partial class SettingsPage {
		private readonly ViewModel
			_viewModel = new ViewModel();

		public SettingsPage() => InitializeComponent();

		protected override void OnNavigatedTo(NavigationEventArgs e)
			=> _viewModel.Group = Global.Instance.Group ?? new IPEndPoint(IPAddress.Any, 0);

		protected override void OnNavigatedFrom(NavigationEventArgs e) => Global.Instance.Group = _viewModel.Group;

		private class ViewModel : BindableBase {
			private byte   _ip0, _ip1, _ip2, _ip3;
			private ushort _port;

			public IPEndPoint Group {
				get => new IPEndPoint(new IPAddress(new[] {_ip0, _ip1, _ip2, _ip3}), _port);
				set {
					var address = value.Address.GetAddressBytes();
					SetProperty(ref _ip0,  address[0]);
					SetProperty(ref _ip1,  address[1]);
					SetProperty(ref _ip2,  address[2]);
					SetProperty(ref _ip3,  address[3]);
					SetProperty(ref _port, (ushort) value.Port);
				}
			}

			public string Ip0 {
				get => _ip0.ToString();
				set => SetProperty(ref _ip0, byte.TryParse(value, out var data) ? data : (byte) 0);
			}

			public string Ip1 {
				get => _ip1.ToString();
				set => SetProperty(ref _ip1, byte.TryParse(value, out var data) ? data : (byte) 0);
			}

			public string Ip2 {
				get => _ip2.ToString();
				set => SetProperty(ref _ip2, byte.TryParse(value, out var data) ? data : (byte) 0);
			}

			public string Ip3 {
				get => _ip3.ToString();
				set => SetProperty(ref _ip3, byte.TryParse(value, out var data) ? data : (byte) 0);
			}

			public string Port {
				get => _port.ToString();
				set => SetProperty(ref _port, ushort.TryParse(value, out var data) ? data : (byte) 0);
			}
		}
	}
}
