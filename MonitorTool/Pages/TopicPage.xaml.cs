using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MechDancer.Common;
using MonitorTool.Controls;
using MonitorTool.Source;

namespace MonitorTool.Pages {
	public sealed partial class TopicPage {
		// 数据绑定
		private readonly ViewModel _viewModel = new ViewModel();

		// 监听话题
		private IDisposable _link;

		public TopicPage() => InitializeComponent();

		protected override void OnNavigatedTo(NavigationEventArgs e)
			=> _link = Global.Instance.Receiver.Port.LinkTo
				   (new ActionBlock<(string, string, byte[])>
					    (it => {
						     var (sender, topic, _) = it;
						     var root =
							     TopicView.RootNodes.SingleOrDefault(x => (string) x.Content == sender);
						     if (root == null) {
							     root = new TreeViewNode {
								                             Content = sender,
								                             Children = {
									                                        new TreeViewNode
									                                        {Content = topic}
								                                        }
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

		private class ViewModel : BindableBase {
			private string _dim;
			private string _host;
			private string _topic;

			public string Host {
				get => _host;
				set => SetProperty(ref _host, value);
			}

			public string Topic {
				get => _topic;
				set {
					SetProperty(ref _topic, value);
					if (string.IsNullOrWhiteSpace(value)) return;
					Dim = Global.Instance
					            .Helpers
					            .TryGetValue((Host, Topic), out var helper)
					   && helper.Dimension == TopicGraphicHelper.DimensionEnum.Two
						      ? "二维"
						      : "一维";
				}
			}

			public string Dim {
				get => _dim;
				set {
					SetProperty(ref _dim, value);
					if (string.IsNullOrWhiteSpace(_topic)) return;
					Global.Instance
					      .Helpers
					      .GetOrAdd((_host, _topic), new TopicGraphicHelper(_host, _topic))
					      .Dimension = value == "一维"
						                   ? TopicGraphicHelper.DimensionEnum.One
						                   : TopicGraphicHelper.DimensionEnum.Two;
				}
			}
		}
	}
}