using System;
using Windows.UI;

namespace MonitorTool.Source {
	public static class Functions {
		private static readonly Random Random = new Random();

		public static Color RandomColor {
			get {
				var buffer = new byte[3];
				Random.NextBytes(buffer);
				return Color.FromArgb(255, buffer[0], buffer[1], buffer[2]);
			}
		}
	}
}
