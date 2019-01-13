using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using MonitorTool.Controls;

namespace MonitorTool.Pages {
	public sealed partial class GraphicPage {
		public GraphicPage() => InitializeComponent();

		private bool TopicSelector_OnButtonClick(string topic) {
			var graphic = new GraphicView {Range = (Vector2.Zero, new Vector2(400, 400)), Connection = true};

			var random = new Random();
			var list0  = new List<Vector2>();
			var list1  = new List<Vector2>();

			for (var i = 0; i < 100; i++) {
				list0.Add(new Vector2(random.Next(400), random.Next(400)));
				list1.Add(new Vector2(random.Next(400), random.Next(400)));
			}

			graphic.Points["sample0"] = (Colors.Aqua, list0);
			graphic.Points["sample1"] = (Colors.Orange, list1);

			var item = new PivotItem {Header = topic, Content = graphic};
			Pivot.Items?.Add(item);
			GridView.Items?.Insert(GridView.Items.Count - 1,
								   new GridViewItem {
														Height  = 200,
														Width   = 200,
														Content = new TextBlock {Text = topic, FontSize = 36}
													});
			return true;
		}
	}
}