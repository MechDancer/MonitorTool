using MechDancer.Common;
using MonitorTool.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using static Windows.UI.Core.CoreDispatcherPriority;

namespace MonitorTool.Pages {
	public sealed partial class ProbePage {
		private bool _running = true;

		public ProbePage() {
			InitializeComponent();
			View.Dispatcher.RunAsync
				(Low, async () => {
						  while (_running) {
							  View.Items
								 ?.OfType<ProbeView>()
								  .Also(it => {
											foreach (var view in it)
												view.Refresh(TimeSpan.FromSeconds(1));
										});
							  await Task.Delay(500);
						  }
					  });
			Debug.Assert(View.Items != null, "View.Items != null");
			((GroupSelector) View.Items[0]).ButtonClick +=
				group => {
					if (View.Items.OfType<ProbeView>().Select(it => it.Group).Contains(group))
						return false;
					var view = new ProbeView(group);
					View.Dispatcher.RunAsync
						(High, () => View.Items.Insert(View.Items.Count - 1, view));
					view.Dispatcher.RunAsync
						(Low, () => view.CloseButtonClick += () => View.Items.Remove(view));
					return true;
				};
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
			=> _running = false;
	}
}