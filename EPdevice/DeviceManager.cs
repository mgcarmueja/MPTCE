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
		///  <summary>
		///  ///  Constructor. Sets the class GUID, product & vendor id.
		///  Predefined values are in DeviceManagerProp.cs.
		///  </summary>
		public EPdeviceManager(UInt16 vendor_id, UInt16 product_id)
		{
			UsbNotifications = new UsbDevice();
			WinUsbNotifications = new WinUsbDevice();
			EPdevicesVM = new ObservableCollection<GENdeviceVM>();
			_EPdevices = new Collection<GENdevice>();

			if (vendor_id != 0)
				VENDOR_ID = vendor_id;

			if (product_id != 0)
				PRODUCT_ID = product_id;
		}

		public EPdeviceManager(String class_guid = null, UInt16 vendor_id = 0, UInt16 product_id = 0)
			: this(vendor_id, product_id)
		{
			if (class_guid != null)
				CLASS_GUID = new Guid(class_guid);
		}
		public EPdeviceManager(Guid class_guid, UInt16 vendor_id = 0, UInt16 product_id = 0)
			: this(vendor_id, product_id)
		{
			if (class_guid != Guid.Empty)
				CLASS_GUID = class_guid;
		}

        /*
		///  <summary>
		///  Destructor.
		///  </summary>
		~EPdeviceManager()
		{
			;
		}
        */
 
		///  <summary>
		///  Check if any devices are already connected and create them.
		///  </summary>
		///  
		///  <param name="WPFhandler"> a pointer to the main handler. </param>
		public void Initialize(IntPtr WPFhandler)
		{
			ADS1298device myADSdevice;
			String[] devicePathNames = { };

			// Check if there are any devices for this class connected.
			if (UsbNotifications.FindDevice(CLASS_GUID, ref devicePathNames))
			{
				foreach (String devicePathName in devicePathNames)
				{
					// Create device and initialize it.
					myADSdevice = new ADS1298device(this.CLASS_GUID, devicePathName);

					if (myADSdevice.InitializeDevice())
					{
						if (myADSdevice.ValidateVendorProductID(VENDOR_ID, PRODUCT_ID))
						{
							_EPdevices.Add(myADSdevice);
							Thread.Sleep(500);
							EPdevicesVM.Add(new ADS1298deviceVM(myADSdevice));
						}
					}
				}
			}

			// Register for notifications on this device class.
			UsbNotifications.RegisterForDeviceNotifications(WPFhandler, CLASS_GUID, ref UsbNotificationHandle);
		}

		///  <summary>
		///  Check if any devices are already connected and create them.
		///  </summary>
		///  
		///  <param name="WPFhandler"> a pointer to the main handler. </param>
		public void Close()
		{
			// Unregister from notifications.
			UsbNotifications.UnregisterForDeviceNotifications(UsbNotificationHandle);
		}

		///  <summary>
		///  A WM_DEVICECHANGE message has arrived indicating that a device has been attached or removed.
		///  </summary>
		///  
		///  <param name="Msg"> the ID of the message which arrived. </param>
		///  <param name="wParam"> the wParam value. </param>
		///  <param name="lParam"> the lParam value. </param>
		public void OnDeviceChange(IntPtr WPFhandler, Int32 Msg, IntPtr wParam, IntPtr lParam)
		{
			String devicePathName = null;
			try
			{
				//  If WParam contains DBT_DEVICEARRIVAL, a device has been attached.
				if ((wParam.ToInt32() == UsbDevice.DBT_DEVICEARRIVAL))
				{
					if (RetrieveDevicePathFromMessage(lParam, ref devicePathName))
					{
						AddEPdevice(devicePathName);
					}
				}
				// If WParam contains DBT_DEVICEREMOVAL, a device has been removed.
				else if ((wParam.ToInt32() == UsbDevice.DBT_DEVICEREMOVECOMPLETE))
				{
					if (RetrieveDevicePathFromMessage(lParam, ref devicePathName))
					{
						RemoveEPdevice(devicePathName);
					}
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		///  <summary>
		///  Adds a new attached device.
		///  </summary>
		///  <param name="devicePathName"> the device path name to add to the manager. </param>
		private void AddEPdevice(String devicePathName)
		{
			ADS1298device adsDevice = new ADS1298device(this.CLASS_GUID, devicePathName);

			if (adsDevice.InitializeDevice())
			{
				if (adsDevice.ValidateVendorProductID(VENDOR_ID, PRODUCT_ID))
				{
					_EPdevices.Add(adsDevice);
					Thread.Sleep(100);
					EPdevicesVM.Add(new ADS1298deviceVM(adsDevice));
				}
			}
		}

		///  <summary>
		///  Deletes a removed device.
		///  </summary>
		///  <param name="devicePathName"> the device path name to remove from the manager. </param>
		private void RemoveEPdevice(String devicePathName)
		{
			for (Int32 i = 0; i < _EPdevices.Count; i++)
			{
				if (String.Compare(_EPdevices.ElementAt(i).DevicePath, devicePathName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					EPdevicesVM.RemoveAt(i);
					Thread.Sleep(100);
					_EPdevices.RemoveAt(i);
				}
			}
		}

		///  <summary>
		///  OnDeviceChange retrieves both the wParam and lParam value. Use the lParam value to retriev the 
		///  device path name which was changed.
		///  </summary>
		///  <param name="lParam"> the lParam value. </param>
		///  <param name="devicePath"> returns the device path from the original message. </param>
		private Boolean RetrieveDevicePathFromMessage(IntPtr lParam, ref String devicePath)
		{
			Int32 stringSize;

			try
			{
				UsbDevice.DEV_BROADCAST_DEVICEINTERFACE_1 devBroadcastDeviceInterface = new UsbDevice.DEV_BROADCAST_DEVICEINTERFACE_1();
				UsbDevice.DEV_BROADCAST_HDR devBroadcastHeader = new UsbDevice.DEV_BROADCAST_HDR();

				// The lParam parameter of Message is a pointer to a DEV_BROADCAST_HDR structure.
				Marshal.PtrToStructure(lParam, devBroadcastHeader);

				if (devBroadcastHeader.dbch_devicetype == UsbDevice.DBT_DEVTYP_DEVICEINTERFACE)
				{
					// The dbch_devicetype parameter indicates that the event applies to a device interface.
					// So the structure in lParam is actually a DEV_BROADCAST_INTERFACE structure, which begins with a DEV_BROADCAST_HDR.

					// Obtain the number of characters in dbch_name by subtracting the 32 bytes in the strucutre 
					// that are not part of dbch_name and dividing by 2 because there are 2 bytes per character.
					stringSize = System.Convert.ToInt32((devBroadcastHeader.dbch_size - 32) / 2);

					// The dbcc_name parameter of devBroadcastDeviceInterface contains the device name. 
					// Trim dbcc_name to match the size of the String.         
					devBroadcastDeviceInterface.dbcc_name = new Char[stringSize + 1];

					// Marshal data from the unmanaged block pointed to by m.LParam 
					// to the managed object devBroadcastDeviceInterface.
					Marshal.PtrToStructure(lParam, devBroadcastDeviceInterface);

					// Store the device name in the referenced String.
					devicePath = new String(devBroadcastDeviceInterface.dbcc_name, 0, stringSize);
				}
				return !String.IsNullOrEmpty(devicePath);
			}
			catch (Exception)
			{
				throw;
			}
		}

		///  <summary>
		///  Read data from connected devices.
		///  </summary>
		public void ProcessData()
		{
			for (Int32 i = 0; i < _EPdevices.Count; i++)
			{
				_EPdevices.ElementAt(i).ProcessData();
			}
		}

        /*
		///  <summary>
		///  Save the recorded data.
		///  </summary>
		public void SaveData(String fileName)
		{
			List<String> devTypes;

			// Create HDF5 file. The rey catch block allows for continued execution even if some DLLs are not found.
            // That will of course produce no output file, but it helps for debugging the logic :D
            try
            {
                //HDF5saver = new HDF5file(fileName);
            }
            catch (FileNotFoundException)
            {
                //HDF5saver = null;
            }

			// Iterate trough the connected devices and look up device types
			devTypes = new List<String>();
			for (Int32 i = 0; i < _EPdevices.Count; i++)
			{
				devTypes.Add(_EPdevices.ElementAt(i).EPdeviceType);
			}
			devTypes = devTypes.Distinct().ToList();
			devTypes.Sort();

			// Save process
			Int32 deviceTypeID, deviceID;
			HDF5group H5group;

			for (Int32 i = 0; i < devTypes.Count; i++) // Iterate trough device types
			{
                deviceTypeID = 0;

				if (HDF5saver!=null) deviceTypeID = HDF5saver.addDeviceType(devTypes.ElementAt(i));

				for (Int32 j = 0; j < _EPdevices.Count; j++)  // Collect devices from this type
				{
					if (String.Compare(_EPdevices.ElementAt(j).EPdeviceType, devTypes.ElementAt(i), true) == 0) // Device type found
					{
                        if (HDF5saver != null)
                        {
                            H5group = HDF5saver.getDeviceType(deviceTypeID);
                            deviceID = H5group.addGroup(_EPdevices.ElementAt(j).DeviceName);
                            H5group = H5group.getGroup(deviceID);

                        }
                        else H5group = null;

                        _EPdevices.ElementAt(j).SaveData(H5group);

					}
				}
			}
            if (HDF5saver != null)  HDF5saver.Dispose();
		}

         */
 
		///  <summary>
		///  Return the WM_DEVICECHANGE constant.
		///  </summary>
		public Int32 Get_WM_DEVICECHANGE()
		{
			return UsbDevice.WM_DEVICECHANGE;
		}
	}
}
