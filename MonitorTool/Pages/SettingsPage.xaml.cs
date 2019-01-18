using System;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace MonitorTool.Pages {
	public sealed partial class SettingsPage {
		private readonly IPropertySet _settings
			= ApplicationData.Current.LocalSettings.Values;

		public SettingsPage() => InitializeComponent();

		private string Ip0 {
			get => Get(nameof(Ip0), "0");
			set => Set(nameof(Ip0), value, data => (byte.TryParse(data, out var num), num));
		}

		private string Ip1 {
			get => Get(nameof(Ip1), "0");
			set => Set(nameof(Ip1), value, data => (byte.TryParse(data, out var num), num));
		}

		private string Ip2 {
			get => Get(nameof(Ip2), "0");
			set => Set(nameof(Ip2), value, data => (byte.TryParse(data, out var num), num));
		}

		private string Ip3 {
			get => Get(nameof(Ip3), "0");
			set => Set(nameof(Ip3), value, data => (byte.TryParse(data, out var num), num));
		}

		private string Port {
			get => Get(nameof(Port), "0");
			set => Set(nameof(Port), value, data => (ushort.TryParse(data, out var num), num));
		}

		private string Get(string key, string @default)
			=> _settings.TryGetValue(key, out var data) ? data.ToString() : @default;

		private void Set<T>(string key, string value, Func<string, (bool, T)> func) {
			var (success, data) = func(value);
			if (success) _settings[key] = data;
		}
	}
}
