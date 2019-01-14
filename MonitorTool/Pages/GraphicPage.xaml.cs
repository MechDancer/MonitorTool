using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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

			var graphic = new GraphicView {Range = (Vector2.Zero, new Vector2(400, 400))};

			var random = new Random();
			var list0  = new List<Vector2>();
			var list1  = new List<Vector2>();

			for (var i = 0; i < 100; i++) {
				list0.Add(new Vector2(random.Next(-200, 600), random.Next(400)));
				list1.Add(new Vector2(random.Next(-200, 600), random.Next(400)));
			}

			graphic.Points["sample0"] = list0;
			graphic.Points["sample1"] = list1;

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