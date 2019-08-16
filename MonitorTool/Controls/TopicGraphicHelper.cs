using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks.Dataflow;
using MechDancer.Common;
using MonitorTool.Source;

namespace MonitorTool.Controls {
    public class TopicGraphicHelper {
        public enum GraphType : byte {
            OneDimension = 1,
            TwoDimension = 2,
            Pose,
            Frame
        }

        public static string TypeName(GraphType type) {
            switch (type) {
                case GraphType.OneDimension:
                    return "一维";
                case GraphType.TwoDimension:
                    return "二维";
                case GraphType.Pose:
                    return "位姿";
                case GraphType.Frame:
                    return "单帧";
            }

            throw new ArgumentOutOfRangeException();
        }

        private readonly long
            _time = DateTime.Now.Ticks;

        public readonly BroadcastBlock<(GraphType, List<Vector3>)>
            Port = new BroadcastBlock<(GraphType, List<Vector3>)>(null);

        public GraphType
            Type = GraphType.OneDimension;

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
            var frame  = new List<Vector3>();

            switch (Type) {
                case GraphType.OneDimension:
                    switch (payload.Length) {
                        case sizeof(float):
                            frame.Add(new Vector3((float) ((DateTime.Now.Ticks - _time) / 1E7),
                                                  stream.ReadFloat(),
                                                  float.NaN));
                            break;
                        case sizeof(double):
                            frame.Add(new Vector3((float) ((DateTime.Now.Ticks - _time) / 1E7),
                                                  (float) stream.ReadDouble(),
                                                  float.NaN));
                            break;
                    }

                    break;
                case GraphType.TwoDimension:
                    switch (payload.Length) {
                        case sizeof(float):
                            frame.Add(new Vector3(stream.ReadFloat(),
                                                  stream.ReadFloat(),
                                                  float.NaN));
                            break;
                        case sizeof(double):
                            frame.Add(new Vector3((float) stream.ReadDouble(),
                                                  (float) stream.ReadDouble(),
                                                  float.NaN));
                            break;
                    }

                    break;
                case GraphType.Pose:
                    switch (payload.Length) {
                        case 3 * sizeof(float):
                            frame.Add(new Vector3(stream.ReadFloat(),
                                                  stream.ReadFloat(),
                                                  stream.ReadFloat()));
                            break;
                        case 3 * sizeof(double):
                            frame.Add(new Vector3((float) stream.ReadDouble(),
                                                  (float) stream.ReadDouble(),
                                                  (float) stream.ReadDouble()));
                            break;
                    }

                    break;
                case GraphType.Frame:
                    var length = stream.ReadInt();
                    switch (stream.ReadByte()) {
                        case 0:
                            while (length-- > 0)
                                frame.Add(new Vector3(stream.ReadFloat(),
                                                      stream.ReadFloat(),
                                                      float.NaN));
                            break;
                        case 1:
                            while (length-- > 0)
                                frame.Add(new Vector3((float) stream.ReadDouble(),
                                                      (float) stream.ReadDouble(),
                                                      float.NaN));
                            break;
                        case 2:
                            while (length-- > 0)
                                frame.Add(new Vector3(stream.ReadFloat(),
                                                      stream.ReadFloat(),
                                                      stream.ReadFloat()));
                            break;
                        case 3:
                            while (length-- > 0)
                                frame.Add(new Vector3((float) stream.ReadDouble(),
                                                      (float) stream.ReadDouble(),
                                                      (float) stream.ReadDouble()));
                            break;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (frame.Any()) Port.Post((Type, frame));
        }
    }
}