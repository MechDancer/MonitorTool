using System;
using System.ComponentModel;
using System.Net;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using MechDancer.Framework.Net.Presets;
using MonitorTool.Pages;
using Hub = MonitorTool.Source.Hub;

namespace MonitorTool {
	public sealed partial class MainPage {
		private readonly IPropertySet _settings
			= ApplicationData.Current.LocalSettings.Values;

		private readonly ViewModel _view = new ViewModel();

		public MainPage() {
			InitializeComponent();

			var net = byte.Parse(Get("Ip0", "0"));
			if (net < 224 || net >= 239) {
				_view.PageHeader = "设置";
				MainFrame.Navigate(typeof(SettingsPage));
			} else {
				var group = new IPEndPoint
					(IPAddress.Parse($"{Get("Ip0", "0")}.{Get("Ip1", "0")}.{Get("Ip2", "0")}.{Get("Ip3", "0")}"),
					 ushort.Parse(Get("Port", "0")));

				new Pacemaker(group).Activate();
				Hub.Instance.SetEndPoint(group);
			}
		}

		private string Get(string key, string @default)
			=> _settings.TryGetValue(key, out var data) ? data.ToString() : @default;

		private void NavigationView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
			if (args.IsSettingsInvoked) {
				_view.PageHeader = "设置";
				MainFrame.Navigate(typeof(SettingsPage));
			} else
				switch (_view.PageHeader = args.InvokedItem.ToString()) {
					case "网络诊断":
						MainFrame.Navigate(typeof(ProbePage));
						break;
					case "话题监视":
						MainFrame.Navigate(typeof(TopicPage));
						break;
					default:
						throw new ArgumentOutOfRangeException(args.InvokedItem.ToString());
				}
		}

		private class ViewModel : INotifyPropertyChanged {
			private string _pageHeader = "";

			public string PageHeader {
				get => _pageHeader;
				set {
					_pageHeader = value;
					Notify(nameof(PageHeader));
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

			private void Notify(string name)
				=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}