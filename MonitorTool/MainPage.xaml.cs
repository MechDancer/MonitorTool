using System;
using Windows.UI.Xaml.Controls;
using MonitorTool.Pages;
using MonitorTool.Source;

namespace MonitorTool {
	public sealed partial class MainPage {
		private readonly ViewModel _view = new ViewModel();
        private Type _currentPage = null;

		public MainPage() {
			InitializeComponent();
			if (Global.Instance.ResetGroup()) return;
			_view.PageHeader = "设置";
            _currentPage = typeof(SettingsPage);
			MainFrame.Navigate(_currentPage);
		}

        private void NavigationView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
            Type newPage;
            if (args.IsSettingsInvoked) {
                _view.PageHeader = "设置";
                newPage = typeof(SettingsPage);
            } else {
                switch (_view.PageHeader = args.InvokedItem.ToString()) {
                    case "网络诊断":
                        newPage = typeof(ProbePage);
                        break;
                    case "绘图":
                        newPage = typeof(GraphicPage);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(args.InvokedItem.ToString());
                }
            }
            if (_currentPage != newPage) MainFrame.Navigate(_currentPage = newPage);
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
