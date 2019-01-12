using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using Windows.UI.Xaml;
using MechDancer.Common;
using MechDancer.Framework.Net.Presets;

namespace MonitorTool.Controls {
	public sealed partial class ProbeView {
		public delegate void CloseButtonClickHandler();

		private readonly ViewModel _view = new ViewModel();
		private          Probe     _probe;
		private          bool      _running = true;

		public CloseButtonClickHandler CloseButtonClick;

		public ProbeView() => InitializeComponent();

		public ProbeView(IPEndPoint endPoint) : this() => Init(endPoint);

		public IPEndPoint Group => _probe.Group;

		public void Init(IPEndPoint endPoint) {
			_view.Header = endPoint.ToString();
			if (_probe == null) {
				_probe = new Probe(endPoint);
				new Thread(() => {
							   while (_running) _probe.Invoke();
						   })
					{IsBackground = true}.Start();
			} else
				_probe = new Probe(endPoint);

			new Pacemaker(endPoint).Activate();
		}

		public void Refresh(TimeSpan timeout)
			=> _view.Group = _probe[timeout].ToList();

		private void ProbeView_OnUnloaded(object sender, RoutedEventArgs e)
			=> _running = false;

		private void Button_Click(object sender, RoutedEventArgs e)
			=> CloseButtonClick();

		private class ViewModel : INotifyPropertyChanged {
			private readonly ObservableCollection<string> _group
				= new ObservableCollection<string>();

			private string _header = "";

			public string Header {
				get => _header;
				set {
					_header = value;
					Notify(nameof(Header));
				}
			}

			public ICollection<string> Group {
				get => _group;
				set => _group.Sync(value);
			}

			public event PropertyChangedEventHandler PropertyChanged;

			private void Notify(string name)
				=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}