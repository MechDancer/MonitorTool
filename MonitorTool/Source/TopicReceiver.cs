using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks.Dataflow;
using MechDancer.Framework.Dependency.UniqueComponent;
using MechDancer.Framework.Net.Modules.Multicast;
using MechDancer.Framework.Net.Protocol;
using MechDancer.Framework.Net.Resources;

namespace MonitorTool.Source {
	/// <summary>
	/// 	话题接收者
	/// </summary>
	public class TopicReceiver : UniqueComponent<TopicReceiver>,
	                             IMulticastListener {
		private static readonly byte[] InterestSet = {(byte) UdpCmd.TopicMessage};

		public readonly BroadcastBlock<(string, Stream)> Port
			= new BroadcastBlock<(string, Stream)>(null);

		public IReadOnlyCollection<byte> Interest => InterestSet;

		public void Process(RemotePacket remotePacket) {
			var (sender, _, payload) = remotePacket;
			Port.Post((sender, new MemoryStream(payload)));
		}
	}
}