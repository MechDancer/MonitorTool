using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using MechDancer.Common;
using MechDancer.Framework.Net.Presets;
using MonitorTool.Source;

namespace MonitorTool.Controls {
	/// <inheritdoc cref="Windows.UI.Xaml.Controls.UserControl" />
	/// <summary>
	/// 	探针界面
	/// </summary>
	public sealed partial class ProbeView {
		public delegate void CloseButtonClickHandler();

		private readonly ViewModel _view = new ViewModel(); // 数据绑定
		private          Pacemaker _pacemaker;              // 起搏器
		private          Probe     _probe;                  // 探针
		private          bool      _running = true;         // 线程标记

		public CloseButtonClickHandler CloseButtonClick; // 关闭事件

		public ProbeView() => InitializeComponent();

		public ProbeView(IPEndPoint group) : this()
			=> Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Init(group));

		public IPEndPoint Group => _probe.Group;

		public void Init(IPEndPoint endPoint) {
			_view.Header = endPoint.ToString();
			if (_probe == null) {
				_probe = new Probe(endPoint);
				new Thread(() => {
					           while (_running) _probe.Invoke();
				           }) {IsBackground = true}.Start();
			} else
				_probe = new Probe(endPoint);

			(_pacemaker = new Pacemaker(endPoint)).Activate();
		}

		/// <summary>
		/// 	探针刷新到界面显示
		/// </summary>
		/// <param name="timeout">超时时间</param>
		public void Refresh(TimeSpan timeout) => _view.Group = _probe[timeout].ToList();

		private void ProbeView_OnUnloaded(object sender, RoutedEventArgs e) => _running = false;

		private void Refresh_Click(object sender, RoutedEventArgs e) => _pacemaker?.Activate();

		private void Close_Click(object sender, RoutedEventArgs e) => CloseButtonClick();

		private class ViewModel : BindableBase {
			private readonly ObservableCollection<string> _group
				= new ObservableCollection<string>();

			private string _header = "";

			public string Header {
				get => _header;
				set => SetProperty(ref _header, value);
			}

			public ICollection<string> Group {
				get => _group;
				set => _group.Sync(value);
			}
		}
	}
}
