using System;
using System.Collections.Generic;
using System.Linq;

namespace TopicManager.Topics {
    /// <summary>
    /// 2 维累积器
    /// </summary>
    public class Accumulator2D : Accumulator<(float, float)> {
        public Accumulator2D(params (float, float)[] points)
            : base(2, points) { }
    }

    /// <summary>
    /// 3 维累积器
    /// </summary>
    public class Accumulator3D : Accumulator<(float, float, float)> {
        public Accumulator3D(params (float, float, float)[] points)
            : base(3, points) { }
    }

    /// <summary>
    /// 4 维累积器
    /// </summary>
    public class Accumulator4D : Accumulator<(float, float, float, float)> {
        public Accumulator4D(params (float, float, float, float)[] points)
            : base(4, points) { }
    }

    /// <summary>
    /// 累积器
    /// </summary>
    /// <typeparam name="T">向量类型</typeparam>
    public class Accumulator<T> : ITopic {
        private readonly List<T>    _list = new List<T>();
        public           byte       Dimension { get; }
        public           bool       FrameMode => false;
        public           TopicState State     { get; set; } = TopicState.None;
        public           uint       Capacity;

        public Accumulator(byte dim, params T[] points) {
            Dimension = dim;
            switch (points.Length) {
                case 0:
                    Capacity = 100;
                    break;
                case 1:
                    Capacity = 100;
                    _list.AddRange(points);
                    break;
                default:
                    Capacity = (uint) Math.Max(100, points.Length);
                    _list.AddRange(points);
                    break;
            }
        }

        public bool Add(ITopic others) {
            if (State == TopicState.None || !(others is Accumulator<T> accumulator))
                return false;

            var actual = accumulator._list;
            lock (_list) {
                if (actual.Count >= Capacity) {
                    _list.Clear();
                    _list.AddRange(actual.TakeLast((int) Capacity));
                } else if (_list.Count + actual.Count > Capacity) {
                    _list.RemoveRange(0, _list.Count + actual.Count - (int) Capacity);
                    _list.AddRange(actual);
                } else
                    _list.AddRange(actual);
            }

            return others.State == TopicState.Activate;
        }

        public List<T> Take(int count) {
            lock (_list) {
                return _list.Take(count).ToList();
            }
        }

        public List<T> Get() {
            lock (_list) {
                return new List<T>(_list);
            }
        }
    }
}
