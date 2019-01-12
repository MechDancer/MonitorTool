using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using MonitorTool.Pages;

namespace MonitorTool {
	public sealed partial class MainPage {
		private readonly IPropertySet _settings
			= ApplicationData.Current.LocalSettings.Values;

		private readonly ViewModel _view = new ViewModel();

		private string get(string key, string @default)
			=> _settings.TryGetValue(key, out var data) ? data.ToString() : @default;

		public MainPage() {
			InitializeComponent();

			var net = byte.Parse(get("Ip0", "0"));
			if (net < 224 || net >= 239) {
				_view.PageHeader = "设置";
				MainFrame.Navigate(typeof(SettingsPage));
			} else {
				var ip   = IPAddress.Parse($"{get("Ip0", "0")}.{get("Ip1", "0")}.{get("Ip2", "0")}.{get("Ip3", "0")}");
				var port = ushort.Parse(get("Port", "0"));
				Hub.SetEndPoint(new IPEndPoint(ip, port));
				Hub.RemoteHub.Monitor.OpenAll();
				new Thread(() => {
							   while (true) {
								   Hub.RemoteHub.Yell();
								   Thread.Sleep(500);
							   }
						   }).Start();
			}
		}

		private void NavigationView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
			if (args.IsSettingsInvoked) {
				_view.PageHeader = "设置";
				MainFrame.Navigate(typeof(SettingsPage));
			} else
				switch (args.InvokedItem) {
					case "网络诊断":
						_view.PageHeader = "网络诊断";
						MainFrame.Navigate(typeof(ProbePage));
						break;
					default:
						throw new ArgumentOutOfRangeException(args.InvokedItem.ToString());
				}
		}

		private class ViewModel : INotifyPropertyChanged {
			public event PropertyChangedEventHandler PropertyChanged;

			private void Notify(string name)
				=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

			private string _pageHeader = "";

			public string PageHeader {
				get => _pageHeader;
				set {
					_pageHeader = value;
					Notify(nameof(PageHeader));
				}
			}
		}
	}
}