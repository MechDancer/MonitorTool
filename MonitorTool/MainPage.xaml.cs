using System;
using Windows.UI.Xaml.Controls;
using MonitorTool.Pages;
using MonitorTool.Source;

namespace MonitorTool {
	public sealed partial class MainPage {
		private readonly ViewModel _view = new ViewModel();

		public MainPage() {
			InitializeComponent();
			if (Global.Instance.ResetGroup()) return;
			_view.PageHeader = "设置";
			MainFrame.Navigate(typeof(SettingsPage));
		}

		private void NavigationView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
			if (args.IsSettingsInvoked) {
				_view.PageHeader = "设置";
				MainFrame.Navigate(typeof(SettingsPage));
			} else {
				switch (_view.PageHeader = args.InvokedItem.ToString()) {
					case "网络诊断":
						MainFrame.Navigate(typeof(ProbePage));
						break;
					case "话题监视":
						MainFrame.Navigate(typeof(TopicPage));
						break;
					case "绘图":
						MainFrame.Navigate(typeof(GraphicPage));
						break;
					default:
						throw new ArgumentOutOfRangeException(args.InvokedItem.ToString());
				}
			}
		}

		private class ViewModel : BindableBase {
			private string _pageHeader = "";

			public string PageHeader {
				get => _pageHeader;
				set => SetProperty(ref _pageHeader, value);
			}
		}
	}
}
