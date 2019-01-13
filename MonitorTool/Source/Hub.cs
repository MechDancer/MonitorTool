using System.Net;
using System.Threading;
using MechDancer.Framework.Net.Presets;

namespace MonitorTool.Source {
	public class Hub {
		public static readonly Hub Instance = new Hub();

		private Hub() {
			new Thread
				(() => {
					 while (true) {
						 if (RemoteHub == null)
							 Thread.Sleep(500);
						 RemoteHub?.Invoke();
					 }
				 }) {IsBackground = true}.Start();
		}

		public RemoteHub     RemoteHub { get; private set; }
		public TopicReceiver Receiver  { get; } = new TopicReceiver();

		public void SetEndPoint(IPEndPoint endPoint) {
			if (RemoteHub != null) return;
			RemoteHub = new RemoteHub
				(name: nameof(MonitorTool),
				 group: endPoint,
				 additions: Receiver);
			new Pacemaker(endPoint).Activate();
		}
	}
}