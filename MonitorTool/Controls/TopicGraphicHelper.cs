using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks.Dataflow;
using MechDancer.Common;
using MonitorTool.Source;
using static System.BitConverter;

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
			Vector2? v = null;
			switch (Dimension) {
				case DimensionEnum.One:
					switch (payload.Length) {
						case sizeof(float):
							v = new Vector2((float) (DateTime.Now.Ticks - _time) / 10000,
							                ToSingle(payload.Also(Array.Reverse)));
							break;
						case sizeof(double):
							v = new Vector2((float) (DateTime.Now.Ticks - _time) / 10000,
							                (float) ToDouble(payload.Also(Array.Reverse)));
							break;
						default:
							break;
					}

					break;
				case DimensionEnum.Two:
					var stream = new MemoryStream(payload);
					switch (payload.Length) {
						case 2 * sizeof(float):
							v = new Vector2(ToSingle(stream.WaitReversed(sizeof(float))),
							                ToSingle(stream.WaitReversed(sizeof(float))));
							break;
						case 2 * sizeof(double):
							v = new Vector2((float) ToDouble(stream.WaitReversed(sizeof(double))),
							                (float) ToDouble(stream.WaitReversed(sizeof(double))));
							break;
						default:
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