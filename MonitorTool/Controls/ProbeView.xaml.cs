using MechDancer.Common;
using MechDancer.Framework.Net.Presets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using Windows.UI.Xaml;

namespace MonitorTool.Controls {
	public sealed partial class ProbeView {
		private          Probe     _probe;
		private          bool      _running = true;
		private readonly ViewModel _view    = new ViewModel();

		public delegate void CloseButtonClickHandler();

		public CloseButtonClickHandler CloseButtonClick;

		public IPEndPoint Group => _probe.Group;

		public ProbeView() => InitializeComponent();

		public ProbeView(IPEndPoint endPoint) : this() => Init(endPoint);

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

		private class ViewModel : INotifyPropertyChanged {
			public event PropertyChangedEventHandler PropertyChanged;

			private void Notify(string name)
				=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

			private string _header = "";

			private readonly ObservableCollection<string> _group
				= new ObservableCollection<string>();

			public string Header {
				get => _header;
				set {
					_header = value;
					Notify(nameof(Header));
				}
			}

			public ICollection<string> Group {
				get => _group;
				set {
					var temp = _group.Retain(value).ToList();
					foreach (var name in _group.ToList())
						if (temp.NotContains(name))
							_group.Remove(name);
					foreach (var name in value)
						if (temp.NotContains(name))
							_group.Add(name);
				}
			}
		}

		private void ProbeView_OnUnloaded(object sender, RoutedEventArgs e)
			=> _running = false;

		private void Button_Click(object sender, RoutedEventArgs e)
			=> CloseButtonClick();
	}
}