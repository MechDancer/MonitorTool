using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;

namespace MonitorTool.Controls {
	public sealed partial class ColorDialog {
		public delegate void ButtonClickHandler(ICollection<ColorItem> colors);

		private readonly ICollection<ColorItem> _colorItems;

		public ColorDialog(ICollection<ColorItem> colorItems) {
			InitializeComponent();
			_colorItems = colorItems;
			Debug.Assert(MainList.Items != null, "MainList.Items != null");
			foreach (var item in _colorItems) MainList.Items.Add(item);
		}

		public event ButtonClickHandler ButtonClick;

		private void ContentDialog_PrimaryButtonClick(
			ContentDialog                     sender,
			ContentDialogButtonClickEventArgs args
		) => ButtonClick?.Invoke(_colorItems);
	}
}