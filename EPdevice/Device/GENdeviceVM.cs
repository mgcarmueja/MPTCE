using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPdevice
{
	abstract public class GENdeviceVM : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Constructor.
		/// </summary>
		internal GENdeviceVM() { }

		///  <summary>
		///  Helper method for WPF to detect binding-changes.
		///  </summary>
		internal void OnPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
