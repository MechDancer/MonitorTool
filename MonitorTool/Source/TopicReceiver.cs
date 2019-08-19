using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks.Dataflow;
using MechDancer.Common;
using MechDancer.Framework.Dependency.UniqueComponent;
using MechDancer.Framework.Net.Modules.Multicast;
using MechDancer.Framework.Net.Protocol;
using MechDancer.Framework.Net.Resources;
using Port =
    System.Threading.Tasks.Dataflow.
    BroadcastBlock<System.Collections.Generic.List<System.Numerics.Vector3>>;
using Ports =
    System.Collections.Concurrent.
    ConcurrentDictionary<MonitorTool.Source.TopicConfig,
        System.Threading.Tasks.Dataflow.
        BroadcastBlock<System.Collections.Generic.List<System.Numerics.Vector3>>
    >;

namespace MonitorTool.Source {
    /// <summary>
    /// 话题接收者
    /// </summary>
    public class TopicReceiver : UniqueComponent<TopicReceiver>,
                                 IMulticastListener {
        private static readonly byte[] InterestSet = {(byte) UdpCmd.TopicMessage};
        private static readonly DateTime Origin = DateTime.Now.Date;

        public IReadOnlyCollection<byte> Interest => InterestSet;

        /// <summary>
        /// 画图设置集
        /// </summary>
        public readonly Ports Ports = new Ports();

        public void Process(RemotePacket remotePacket) {
            var (sender, _, payload) = remotePacket;
            var stream = new MemoryStream(payload);
            var topic  = stream.ReadEnd();
            var type   = (GraphType) stream.ReadByte();
            Ports.GetOrAdd(new TopicConfig(sender, topic, type),
                           new Port(null))
                ?.Also(port => {
                      var rest  = stream.Length - stream.Position;
                      var data  = new NetworkDataReader(stream);
                      var frame = new List<Vector3>();
                      switch (type) {
                          case GraphType.OneDimension:
                              switch (rest) {
                                  case sizeof(float):
                                      frame.Add(new Vector3((float) ((DateTime.Now - Origin).Ticks / 1E7),
                                                            data.ReadFloat(),
                                                            float.NaN));
                                      break;
                                  case sizeof(double):
                                      frame.Add(new Vector3((float)((DateTime.Now - Origin).Ticks / 1E7),
                                                            (float) data.ReadDouble(),
                                                            float.NaN));
                                      break;
                              }

                              break;
                          case GraphType.TwoDimension:
                              switch (rest) {
                                  case 2 * sizeof(float):
                                      frame.Add(new Vector3(data.ReadFloat(),
                                                            data.ReadFloat(),
                                                            float.NaN));
                                      break;
                                  case 2 * sizeof(double):
                                      frame.Add(new Vector3((float) data.ReadDouble(),
                                                            (float) data.ReadDouble(),
                                                            float.NaN));
                                      break;
                              }

                              break;
                          case GraphType.Pose:
                              switch (rest) {
                                  case 3 * sizeof(float):
                                      frame.Add(new Vector3(data.ReadFloat(),
                                                            data.ReadFloat(),
                                                            data.ReadFloat()));
                                      break;
                                  case 3 * sizeof(double):
                                      frame.Add(new Vector3((float) data.ReadDouble(),
                                                            (float) data.ReadDouble(),
                                                            (float) data.ReadDouble()));
                                      break;
                              }

                              break;
                          case GraphType.Frame:
                              var length = rest - 1;
                              switch ((FrameType) stream.ReadByte()) {
                                  case FrameType.OneFloat:
                                      length /= sizeof(float);
                                      while (length-- > 0)
                                          frame.Add(new Vector3((float) (DateTime.Now.Ticks / 1E7),
                                                                data.ReadFloat(),
                                                                float.NaN));
                                      break;
                                  case FrameType.OneDouble:
                                      length /= sizeof(double);
                                      while (length-- > 0)
                                          frame.Add(new Vector3((float) (DateTime.Now.Ticks / 1E7),
                                                                (float) data.ReadDouble(),
                                                                float.NaN));
                                      break;
                                  case FrameType.TwoFloat:
                                      length /= 2 * sizeof(float);
                                      while (length-- > 0)
                                          frame.Add(new Vector3(data.ReadFloat(),
                                                                data.ReadFloat(),
                                                                float.NaN));
                                      break;
                                  case FrameType.TwoDouble:
                                      length /= 2 * sizeof(double);
                                      while (length-- > 0)
                                          frame.Add(new Vector3((float) data.ReadDouble(),
                                                                (float) data.ReadDouble(),
                                                                float.NaN));
                                      break;
                                  case FrameType.ThreeFloat:
                                      length /= 3 * sizeof(float);
                                      while (length-- > 0)
                                          frame.Add(new Vector3(data.ReadFloat(),
                                                                data.ReadFloat(),
                                                                data.ReadFloat()));
                                      break;
                                  case FrameType.ThreeDouble:
                                      length /= 3 * sizeof(double);
                                      while (length-- > 0)
                                          frame.Add(new Vector3((float) data.ReadDouble(),
                                                                (float) data.ReadDouble(),
                                                                (float) data.ReadDouble()));
                                      break;
                                  default:
                                      throw new ArgumentOutOfRangeException();
                              }

                              break;
                          default:
                              throw new ArgumentOutOfRangeException();
                      }

                      if (frame.Any()) port.Post(frame);
                  });
        }
    }
}