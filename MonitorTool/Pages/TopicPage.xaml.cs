using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MechDancer.Common;
using MonitorTool.Controls;
using MonitorTool.Source;
using Hub = MonitorTool.Source.Hub;

namespace MonitorTool.Pages {
	public sealed partial class TopicPage {
		private IDisposable _link;
		private ViewModel   _viewModel = new ViewModel();
		public TopicPage() => InitializeComponent();

		protected override void OnNavigatedTo(NavigationEventArgs e)
			=> _link = Hub.Instance.Receiver.Port.LinkTo
				   (new ActionBlock<(string, string, byte[])>
					    (it => {
						     var (sender, topic, _) = it;
						     var root =
							     TopicView.RootNodes.SingleOrDefault(x => (string) x.Content == sender);
						     if (root == null) {
							     root = new TreeViewNode {
								                             Content  = sender,
								                             Children = {new TreeViewNode {Content = topic}}
							                             };
							     TopicView.RootNodes.Add(root);
						     } else if (root.Children.None(x => (string) x.Content == topic))
							     root.Children.Add(new TreeViewNode {Content = topic});
					     },
					     new ExecutionDataflowBlockOptions {
						                                       TaskScheduler = TaskScheduler
							                                      .FromCurrentSynchronizationContext()
					                                       }
					    ));

		protected override void OnNavigatedFrom(NavigationEventArgs e)
			=> _link?.Dispose();

		private void TopicView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args) {
			if (!(args.InvokedItem is TreeViewNode node)) return;
			if (!(node.Parent is TreeViewNode root)) return;

			var host  = (string) root.Content;
			var topic = (string) node.Content;

			if (string.IsNullOrWhiteSpace(host)) {
				_viewModel.Host  = topic;
				_viewModel.Topic = "";
			} else {
				_viewModel.Host  = host;
				_viewModel.Topic = topic;
			}
		}

		private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (_viewModel.Topic == "") return;
			var dim = (string) e.AddedItems.Single() == "一维"
				          ? TopicGraphicHelper.DimensionEnum.One
				          : TopicGraphicHelper.DimensionEnum.Two;
			Hub.Instance.Helpers.GetOrAdd
				((_viewModel.Host, _viewModel.Topic),
				 new TopicGraphicHelper(_viewModel.Host, _viewModel.Topic)
				).Dimension = dim;
		}

		private class ViewModel : BindableBase {
			private string _host;
			private string _topic;

			public string Host {
				get => _host;
				set => SetProperty(ref _host, value);
			}

			public string Topic {
				get => _topic;
				set => SetProperty(ref _topic, value);
			}
		}
	}
}