using System.Net;
using System.Net.Sockets;
using System.Threading;
using Windows.Foundation.Collections;
using Windows.Storage;
using MechDancer.Framework.Net.Presets;

namespace MonitorTool.Source {
    /// <summary>
    /// 全局资源
    /// </summary>
    public class Global {
        /// <summary>
        /// 单例
        /// </summary>
        public static readonly Global
            Instance = new Global();

        // 持久化设置
        private readonly IPropertySet
            _settings = ApplicationData.Current.LocalSettings.Values;

        private uint      _version;
        private RemoteHub _remoteHub;

        private Global() {
        }

        public IPEndPoint Group {
            get => _remoteHub?.Group;
            set {
                var current = _remoteHub;
                if (!Check(value)) return;
                if (Equals(current?.Group, value)) return;

                var hub = _remoteHub = new RemoteHub(nameof(MonitorTool),
                                                     group: value,
                                                     additions: Receiver);
                new Thread
                (() => {
                    var last = ++_version;
                    _settings["Ip"]   = value.Address.GetAddressBytes();
                    _settings["Port"] = value.Port;
                    if (current == null)
                        new Pacemaker(value).Activate();
                    else
                        current.Yell();
                    while (last == _version) hub.Invoke();
                }) {IsBackground = true}.Start();
            }
        }

        public TopicReceiver Receiver { get; } = new TopicReceiver();

        public bool ResetGroup() {
            if (!_settings.TryGetValue("Ip",   out var ip)) return false;
            if (!_settings.TryGetValue("Port", out var port)) return false;
            var group = new IPEndPoint(new IPAddress((byte[]) ip), (int) port);
            Group = group;
            return Equals(_remoteHub?.Group, group);
        }

        private static bool Check(IPEndPoint group) {
            var ip = group.Address.GetAddressBytes();
            return group.Address.AddressFamily == AddressFamily.InterNetwork
                && 224                         <= ip[0]
                && ip[0]                       < 240
                && group.Port                  != 0;
        }
    }
}