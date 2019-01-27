using Windows.Foundation.Collections;

namespace MonitorTool.Source {
	public static class Extensions {
		public static T GetOrDefault<T>(this IPropertySet settings,
										string            id,
										T                 @default
		) => settings.TryGetValue(id, out var data) ? (T) data : @default;
	}
}
