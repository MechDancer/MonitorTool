using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MonitorTool.Source {
	/// <inheritdoc />
	/// <summary>
	///     可绑定对象
	/// </summary>
	public abstract class BindableBase : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		///     设置属性
		/// </summary>
		/// <param name="field">后台字段</param>
		/// <param name="value">新值</param>
		/// <param name="propertyName">属性名称</param>
		/// <typeparam name="T">类型</typeparam>
		/// <returns>是否发生更新</returns>
		protected bool SetProperty<T>(ref T                     field,
									  T                         value,
									  [CallerMemberName] string propertyName = null) {
			if (Equals(field, value)) return false;

			field = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			return true;
		}
	}
}
