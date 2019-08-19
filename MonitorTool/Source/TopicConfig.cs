using System;

namespace MonitorTool.Source {
    /// <summary>
    /// 图形类型
    /// </summary>
    public enum GraphType : byte {
        Frame        = 0,
        OneDimension = 1,
        TwoDimension = 2,
        Pose         = 3
    }

    /// <summary>
    /// 单帧数据类型
    /// </summary>
    public enum FrameType : byte {
        OneFloat    = 0,
        OneDouble   = 1,
        TwoFloat    = 2,
        TwoDouble   = 3,
        ThreeFloat  = 4,
        ThreeDouble = 5
    }

    /// <summary>
    /// 图形配置
    /// </summary>
    public struct TopicConfig {
        /// <summary>
        /// 发送者
        /// </summary>
        public readonly string Sender;

        /// <summary>
        /// 话题
        /// </summary>
        public readonly string Topic;

        /// <summary>
        /// 类型
        /// </summary>
        public readonly GraphType Type;

        public TopicConfig(string    sender,
                           string    topic,
                           GraphType type) {
            Sender = sender;
            Topic  = topic;
            Type   = type;
        }

        public override string ToString()
            => $"[{TypeName(Type)}] {Sender} : {Topic}";

        private static string TypeName(GraphType type) {
            switch (type) {
                case GraphType.OneDimension:
                    return "1";
                case GraphType.TwoDimension:
                    return "2";
                case GraphType.Pose:
                    return "p";
                case GraphType.Frame:
                    return "f";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}