using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks.Dataflow;
using Windows.UI.Xaml.Controls;
using MonitorTool.Controls;

namespace MonitorTool.Pages {
	public sealed partial class GraphicPage {
		public GraphicPage() => InitializeComponent();

		private bool TopicSelector_OnButtonClick(string topic) {
			Debug.Assert(GridView.Items != null, "GridView.Items != null");
			Debug.Assert(Pivot.Items    != null, "Pivot.Items != null");

			if (Pivot.Items.Any(it => it is PivotItem x && (string) x.Header == topic))
				return false;

			var graphic = new GraphicView {Range = (Vector2.Zero, new Vector2(10, 1))};

			new TopicGraphicHelper("kotlin topic server", "sample0")
			   .Port
			   .LinkTo(new ActionBlock<Vector2>
				           (p => graphic.Operate("kotlin topic server", "sample0", list => list.Add(p))),
			           new DataflowLinkOptions());

			new TopicGraphicHelper("kotlin topic server", "sample1")
			   .Port
			   .LinkTo(new ActionBlock<Vector2>
				           (p => graphic.Operate("kotlin topic server", "sample1", list => list.Add(p))),
			           new DataflowLinkOptions());

			var item = new PivotItem {Header = topic, Content = graphic};
			Pivot.Items.Add(item);
			GridView.Items.Insert(GridView.Items.Count - 1,
			                      new GridViewItem {
				                                       Height  = 200,
				                                       Width   = 200,
				                                       Tag     = topic,
				                                       Content = new TextBlock {Text = topic, FontSize = 36},
			                                       });
			return true;
		}

		private void GridView_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (!(e.AddedItems.SingleOrDefault() is Control control)) return;
			if (!(control.Tag is string name)) return;
			Pivot.SelectedItem =
				(Pivot.Items ?? throw new InvalidOperationException())
			   .Single(it => (string) ((PivotItem) it).Header == name);
			((GridView) sender).SelectedIndex = -1;
		}
	}
}