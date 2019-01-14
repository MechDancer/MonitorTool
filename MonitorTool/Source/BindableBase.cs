using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MonitorTool.Source {
	public abstract class BindableBase : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

		protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null) {
			if (Equals(storage, value)) return false;

			storage = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			return true;
		}
	}
}