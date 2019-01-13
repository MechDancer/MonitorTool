﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MechDancer.Common;
using Hub = MonitorTool.Source.Hub;

namespace MonitorTool.Pages {
	public sealed partial class TopicPage {
		private IDisposable _link;
		public TopicPage() => InitializeComponent();

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			_link = Hub.Instance.Receiver.Port.LinkTo
				(new ActionBlock<(string, Stream)>
					 (it => {
						  var (sender, payload) = it;
						  var root = TopicView.RootNodes.SingleOrDefault(x => (string) x.Content == sender);
						  if (root == null) {
							  root = new TreeViewNode {
														  Content = sender,
														  Children = {
																		 new TreeViewNode
																		 {Content = payload.ReadEnd()}
																	 }
													  };
							  TopicView.RootNodes.Add(root);
						  } else {
							  var topic = payload.ReadEnd();
							  if (root.Children.None(x => (string) x.Content == topic))
								  root.Children.Add(new TreeViewNode {Content = topic});
						  }
					  },
					  new ExecutionDataflowBlockOptions
					  {TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext()}
					 ));
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
			=> _link?.Dispose();
	}
}