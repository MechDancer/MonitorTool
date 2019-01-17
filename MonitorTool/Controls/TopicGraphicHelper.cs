using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks.Dataflow;
using MechDancer.Common;
using MonitorTool.Source;

namespace MonitorTool.Controls {
	public class TopicGraphicHelper {
		public enum DimensionEnum : byte {
			One = 1,
			Two = 2
		}

		private readonly long _time = DateTime.Now.Ticks;

		public readonly BroadcastBlock<Vector2> Port
			= new BroadcastBlock<Vector2>(null);

		public DimensionEnum Dimension = DimensionEnum.One;

		public TopicGraphicHelper(
			string sender,
			string topic
		) => Global.Instance
		           .Receiver
		           .Port
		           .LinkTo(new ActionBlock<(string sender, string topic, byte[] payload)>
			                   (it => Process(it.payload)),
		                   it => it.sender == sender && it.topic == topic);

		private void Process(byte[] payload) {
			Vector2? v      = null;
			var      stream = new MemoryStream(payload);

			switch (Dimension) {
				case DimensionEnum.One:
					switch (payload.Length) {
						case sizeof(float):
							v = new Vector2((float) ((DateTime.Now.Ticks - _time) / 1E7),
							                stream.ReadFloat());
							break;
						case sizeof(double):
							v = new Vector2((float) ((DateTime.Now.Ticks - _time) / 1E7),
							                (float) stream.ReadDouble());
							break;
					}

					break;
				case DimensionEnum.Two:
					switch (payload.Length) {
						case 2 * sizeof(float):
							v = new Vector2(stream.ReadFloat(),
							                stream.ReadFloat());
							break;
						case 2 * sizeof(double):
							v = new Vector2((float) stream.ReadDouble(),
							                (float) stream.ReadDouble());
							break;
					}

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			v?.Let(Port.Post);
		}
	}
}