using MechDancer.Framework.Net.Presets;
using System.Net;

namespace MonitorTool {
	public static class Hub {
		public static RemoteHub RemoteHub { get; private set; }

		public static void SetEndPoint(IPEndPoint endPoint)
			=> RemoteHub = new RemoteHub(name: nameof(MonitorTool), group: endPoint);
	}
}