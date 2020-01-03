using System.Collections.Generic;

namespace TopicManager.Topics {
    /// <summary>
    /// 2 维帧容器
    /// </summary>
    public class FrameContainer2D : FrameContainer<(float, float)> {
        public FrameContainer2D(params (float, float)[] points)
            : base(2, points) { }
    }

    /// <summary>
    /// 3 维帧容器
    /// </summary>
    public class FrameContainer3D : FrameContainer<(float, float, float)> {
        public FrameContainer3D(params (float, float, float)[] points)
            : base(3, points) { }
    }

    /// <summary>
    /// 4 维帧容器
    /// </summary>
    public class FrameContainer4D : FrameContainer<(float, float, float, float)> {
        public FrameContainer4D(params (float, float, float, float)[] points)
            : base(4, points) { }
    }

    /// <summary>
    /// 帧容器
    /// </summary>
    /// <typeparam name="T">向量类型</typeparam>
    public class FrameContainer<T> : ITopic {
        private readonly List<T>    _list = new List<T>();
        public           byte       Dimension { get; }
        public           bool       FrameMode => true;
        public           TopicState State     { get; set; } = TopicState.None;

        public FrameContainer(byte dim, params T[] points) {
            Dimension = dim;
            _list.AddRange(points);
        }

        public bool Add(ITopic others) {
            if (State == TopicState.None || !(others is FrameContainer<T> container))
                return false;

            lock (_list) {
                _list.Clear();
                _list.AddRange(container._list);
            }

            return State == TopicState.Activate;
        }

        public List<T> Get() {
            lock (_list) {
                return new List<T>(_list);
            }
        }
    }
}
