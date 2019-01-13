using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MonitorTool.Source {
	public abstract class BindableBase : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

		protected void SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null) {
			if (Equals(storage, value)) return;

			storage = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}