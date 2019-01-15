using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks.Dataflow;
using MechDancer.Common;
using MonitorTool.Source;

namespace MonitorTool.Controls {
	public class TopicGraphicHelper {
		public enum Dimension : byte { One = 1, Two = 2 }

		private readonly long _time = DateTime.Now.Ticks;

		public readonly List<GraphicView> Views
			= new List<GraphicView>();

		public TopicGraphicHelper(
			string    sender,
			string    topic,
			Dimension dimension
		) {
			Hub.Instance
			   .Receiver
			   .Port
			   .LinkTo(new ActionBlock<(string sender, string topic, byte[] payload)>
						   (it => Process(sender, topic, it.payload, dimension)),
					   it => it.sender == sender && it.topic == topic);
		}

		private void Process(
			string    sender,
			string    topic,
			byte[]    payload,
			Dimension dimension
		) {
			var stream = new MemoryStream(payload);
			var id     = $"{sender}: {topic}";

			Vector2 v;
			switch (dimension) {
				case Dimension.One:
					v = new Vector2((float) (DateTime.Now.Ticks - _time) / 10000,
									BitConverter.ToSingle(stream.WaitReversed(4)));
					break;
				case Dimension.Two:
					v = new Vector2(BitConverter.ToSingle(stream.WaitReversed(4)),
									BitConverter.ToSingle(stream.WaitReversed(4)));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			foreach (var graphicView in Views) {
				if (graphicView.Points.TryGetValue(id, out var list))
					list.Add(v);
				else
					graphicView.Points[id] = new List<Vector2> {v};
				graphicView.Update();
			}
		}
	}
}