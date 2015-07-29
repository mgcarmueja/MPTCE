using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

//using FileManager;

namespace EPdevice
{
	public sealed partial class EPdeviceManager
	{
		private UInt16 VENDOR_ID = 0x04D8;																// Device vendor id.
		private UInt16 PRODUCT_ID = 0xF9D8;																// Device product id.
		private Guid CLASS_GUID = new Guid("{2BA9DC1B-FA5B-4DDC-BC18-FD171C8DE946}");		// Device class guid.

		private UsbDevice UsbNotifications;
		private IntPtr UsbNotificationHandle;
		private WinUsbDevice WinUsbNotifications;

		private ObservableCollection<GENdeviceVM> EPdevicesVM;		// List of EP devices accessible from WPF
		private Collection<GENdevice> _EPdevices;							// List of EP devices

		//private HDF5file HDF5saver;

		///  <summary>
		///  Get-set methods
		///  </summary>
		public ObservableCollection<GENdeviceVM> EPdevicesViewModel
		{
			get { return EPdevicesVM; }
		}
        
        public Collection<GENdevice> EPdevices
                {
            get { return _EPdevices; }
        }

    
	}
}
