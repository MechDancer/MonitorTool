using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using MechDancer.Common;
using MechDancer.Framework.Net.Modules.Multicast;
using MechDancer.Framework.Net.Presets;

namespace MonitorTool {
	public class Hub {
		public static Hub Instance = new Hub();

		private readonly ConcurrentDictionary<(string, string), Stream>
			_buffer = new ConcurrentDictionary<(string, string), Stream>();

		private Hub() { }

		public RemoteHub RemoteHub { get; private set; }

		public IReadOnlyDictionary<(string, string), Stream>
			Buffer => _buffer;

		public void SetEndPoint(IPEndPoint endPoint)
			=> RemoteHub = new RemoteHub
				   (name: nameof(MonitorTool),
					group: endPoint,
					additions:
					new MulticastListener
						(pack => {
							 var (sender, _, payload) = pack;
							 var stream = new MemoryStream(payload);
							 _buffer[(sender, stream.ReadEnd())] = stream;
						 },
						 16));
	}
}