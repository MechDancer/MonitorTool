using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks.Dataflow;
using MechDancer.Common;
using MonitorTool.Source;

namespace MonitorTool.Controls {
    public class TopicGraphicHelper {
        public enum GraphType : byte {
            OneDemension = 1,
            TwoDemension = 2,
            Frame
        }

        public static string TypeName(GraphType type) {
            switch (type) {
                case GraphType.OneDemension:
                    return "Ò»Î¬";
                case GraphType.TwoDemension:
                    return "¶þÎ¬";
                case GraphType.Frame:
                    return "µ¥Ö¡";
            }
            throw new ArgumentOutOfRangeException();
        }

        private readonly long
            _time = DateTime.Now.Ticks;

        public readonly BroadcastBlock<(GraphType, List<Vector2>)>
            Port = new BroadcastBlock<(GraphType, List<Vector2>)>(null);

        public GraphType
            Type = GraphType.OneDemension;

        public TopicGraphicHelper(string sender,
                                  string topic)
            => Global.Instance
                     .Receiver
                     .Port
                     .LinkTo(new ActionBlock<(string sender, string topic, byte[] payload)>
                                 (it => Process(it.payload)),
                             it => it.sender == sender && it.topic == topic);

        private void Process(byte[] payload) {
            var stream = new NetworkDataReader(new MemoryStream(payload));
            var frame = new List<Vector2>();

            switch (Type) {
                case GraphType.OneDemension:
                    switch (payload.Length) {
                        case sizeof(float):
                            frame.Add(new Vector2((float)((DateTime.Now.Ticks - _time) / 1E7),
                                            stream.ReadFloat()));
                            break;
                        case sizeof(double):
                            frame.Add(new Vector2((float)((DateTime.Now.Ticks - _time) / 1E7),
                                            (float)stream.ReadDouble()));
                            break;
                    }

                    break;
                case GraphType.TwoDemension:
                    switch (payload.Length) {
                        case 2 * sizeof(float):
                            frame.Add(new Vector2(stream.ReadFloat(),
                                            stream.ReadFloat()));
                            break;
                        case 2 * sizeof(double):
                            frame.Add(new Vector2((float)stream.ReadDouble(),
                                            (float)stream.ReadDouble()));
                            break;
                    }
                    break;
                case GraphType.Frame:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Port.Post((Type, frame));
        }
    }
}
