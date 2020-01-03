using System;
using System.Collections.Generic;
using System.IO;
using MechDancer.Common;
using TopicManager.Topics;
using V2 = System.ValueTuple<float, float>;
using V3 = System.ValueTuple<float, float, float>;
using V4 = System.ValueTuple<float, float, float, float>;

namespace TopicManager {
    public class TopicManager {
        private const byte
            SizeOfV2 = 2 * sizeof(float),
            SizeOfV3 = 3 * sizeof(float),
            SizeOfV4 = 4 * sizeof(float);

        private readonly Dictionary<string, ITopic> _dictionary
            = new Dictionary<string, ITopic>();

        public bool Receive(string sender, byte[] payload) {
            using var stream = new MemoryStream(payload);
            // parse name
            var name = stream.ReadEnd();
            if (stream.Length - stream.Position < 2) return false;
            // parse config
            var wrapper   = new NetworkDataReader(stream);
            var dimension = wrapper.ReadByte();
            var frameMode = wrapper.ReadByte() != 0;
            var rest      = stream.Length - stream.Position;
            // parse data
            Func<ITopic> newTopic, newData;
            switch (dimension) {
                case 2:
                    newTopic = () => frameMode
                        ? (ITopic) new FrameContainer2D()
                        : (ITopic) new Accumulator2D();
                    newData = () => {
                        var count  = rest / SizeOfV2;
                        var points = new V2[count];
                        for (var i = 0; i < points.Length; ++i)
                            points[i] = (wrapper.ReadFloat(),
                                         wrapper.ReadFloat());
                        return frameMode
                            ? (ITopic) new FrameContainer2D(points)
                            : (ITopic) new Accumulator2D(points);
                    };
                    break;
                case 3:
                    newTopic = () => frameMode
                        ? (ITopic) new FrameContainer3D()
                        : (ITopic) new Accumulator3D();
                    newData = () => {
                        var count  = rest / SizeOfV3;
                        var points = new V3[count];
                        for (var i = 0; i < points.Length; ++i)
                            points[i] = (wrapper.ReadFloat(),
                                         wrapper.ReadFloat(),
                                         wrapper.ReadFloat());
                        return frameMode
                            ? (ITopic) new FrameContainer3D(points)
                            : (ITopic) new Accumulator3D(points);
                    };
                    break;
                case 4:
                    newTopic = () => frameMode
                        ? (ITopic) new FrameContainer4D()
                        : (ITopic) new Accumulator4D();
                    newData = () => {
                        var count  = rest / SizeOfV4;
                        var points = new V4[count];
                        for (var i = 0; i < points.Length; ++i)
                            points[i] = (wrapper.ReadFloat(),
                                         wrapper.ReadFloat(),
                                         wrapper.ReadFloat(),
                                         wrapper.ReadFloat());
                        return frameMode
                            ? (ITopic) new FrameContainer4D(points)
                            : (ITopic) new Accumulator4D(points);
                    };
                    break;
                default:
                    return false;
            }

            // save data
            var title = $"{sender}: {name}";
            lock (_dictionary) {
                if (_dictionary.TryGetValue(title, out var topic))
                    return topic.Add(newData());

                _dictionary.Add(title, newTopic());
                return false;
            }
        }
    }
}
