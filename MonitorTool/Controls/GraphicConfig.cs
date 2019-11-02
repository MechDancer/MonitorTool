using Windows.UI;
using Windows.UI.Xaml.Media;
using MonitorTool.Source;

namespace MonitorTool.Controls {
    /// <inheritdoc />
    /// <summary>
    /// 图形配置
    /// </summary>
    public class GraphicConfig : BindableBase {
        public readonly string Name;

        private Color _color      = Functions.RandomColor;
        private int   _showCount  = int.MaxValue;
        private int   _saveCount  = 1000;
        private bool  _background = false;
        private bool  _connect    = false;

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="name">名字</param>
        public GraphicConfig(string name) => Name = name;

        /// <summary>
        /// 显示颜色
        /// </summary>
        public Color Color {
            get => _color;
            set {
                if (!SetProperty(ref _color, value)) return;
                Notify(nameof(Brush));
            }
        }

        /// <summary>
        /// 画笔
        /// </summary>
        public Brush Brush => new SolidColorBrush(_color);

        /// <summary>
        /// 显示数量
        /// </summary>
        public int ShowCount {
            get => _showCount;
            set => SetProperty(ref _showCount, value);
        }

        /// <summary>
        /// 保存数量
        /// </summary>
        public int SaveCount {
            get => _saveCount;
            set => SetProperty(ref _saveCount, value);
        }

        /// <summary>
        /// 作为背景
        /// </summary>
        public bool Background {
            get => _background;
            set => SetProperty(ref _background, value);
        }

        /// <summary>
        /// 连线
        /// </summary>
        public bool Connect {
            get => _connect;
            set => SetProperty(ref _connect, value);
        }
    }
}