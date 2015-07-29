using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace EPdevice
{
    /// <summary>
    /// Delegate for the handler used on the NewPacket event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void NewPacketEventHandler(object sender, EventArgs e);


	abstract public partial class GENdevice
	{
		internal protected UsbDevice usbDev = new UsbDevice();
		internal protected WinUsbDevice winUsbDev = new WinUsbDevice();
		internal protected EP_DEVICE_INFO devInfo = new EP_DEVICE_INFO();

		public event PropertyChangedEventHandler PropertyChanged;
        public event NewPacketEventHandler NewPacket; 
        

		//  Informations collected about an electrophysiological device.
		internal protected struct EP_DEVICE_INFO
		{
			internal String epDeviceType;          // Type of this device

			internal Guid deviceClassGuid;         // Device interface class ID.
			internal UInt16 vendorID;              // Device vendor ID.
			internal UInt16 productID;             // Device product ID.
			internal String devicePath;            // Path to the device.

			internal Boolean deviceFound;          // The device was found, path available.
			internal Boolean deviceAttached;       // The device was initialized from WinUsb, it is ready to read/send data.

			internal String deviceName;             // Unique device name.
			internal UInt16 samplesLength;          // Length of one sample.
			internal UInt16 numberOfChannels;       // Number of channels for this device.
			internal UInt16 samplesPerSecond;       // Current number of samples per second.

			internal UInt16 resolutionBits;                         // Input resolution in bit.
			internal float inputDifferantialVoltagePositive;        // Negative input voltage.
			internal float inputDifferantialVoltageNegative;        // Positive input voltage.
		}

        internal Boolean liveData; //true if we want data to be delivered as frames to clients, false otherwise.

		///  <summary>
		///  Helper method for WPF to detect binding-changes.
		///  </summary>
		internal void OnPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
        
        /// <summary>
        /// Event to be used when a new packet is added to packetQueue while sending live data to clients.
        /// </summary>
        protected virtual void OnNewPacket(EventArgs e)
        {
            if (NewPacket != null)
                NewPacket(this, e);
        }


        ///  <summary>
        ///  Get/set method for the liveData flag.
        ///  </summary>
        internal protected Boolean LiveData
        {
            get { return liveData; }
            set
            {
                liveData = value;
                OnPropertyChanged("liveData");
            }
        }


		#region Device attributes
		///  <summary>
		///  Get/set method for device path.
		///  </summary>
		internal protected String EPdeviceType
		{
			get { return devInfo.epDeviceType; }
			set
			{
				devInfo.epDeviceType = value;
				OnPropertyChanged("EPdeviceType");
			}
		}

		///  <summary>
		///  Get/set method for device path.
		///  </summary>
		internal protected String DevicePath
		{
			get { return devInfo.devicePath; }
			set
			{
				devInfo.devicePath = value;
				OnPropertyChanged("DevicePath");
			}
		}

		///  <summary>
		///  Get/set method for device guid.
		///  </summary>
		internal protected Guid DeviceClassGuid
		{
			get { return devInfo.deviceClassGuid; }
			set
			{
				devInfo.deviceClassGuid = value;
				OnPropertyChanged("DeviceClassGuid");
			}
		}

		///  <summary>
		///  Get/set method for device found status.
		///  </summary>
		internal protected Boolean DeviceFound
		{
			get { return devInfo.deviceFound; }
			set
			{
				devInfo.deviceFound = value;
				OnPropertyChanged("DeviceFound");
			}
		}

		///  <summary>
		///  Get/set method for device attached status. This means it is fully working.
		///  </summary>
		internal protected Boolean DeviceAttached
		{
			get { return devInfo.deviceAttached; }
			set
			{
				devInfo.deviceAttached = value;
				OnPropertyChanged("DeviceAttached");
			}
		}

		///  <summary>
		///  Get/set method for the device vendor id.
		///  </summary>
		internal protected UInt16 VendorID
		{
			get { return devInfo.vendorID; }
			set
			{
				devInfo.vendorID = value;
				OnPropertyChanged("VendorID");
			}
		}

		///  <summary>
		///  Get/set method for the device product id.
		///  </summary>
		internal protected UInt16 ProductID
		{
			get { return devInfo.productID; }
			set
			{
				devInfo.productID = value;
				OnPropertyChanged("ProductID");
			}
		}

		///  <summary>
		///  Get/set method for the device name.
		///  </summary>
		internal protected String DeviceName
		{
			get { return devInfo.deviceName; }
			set
			{
				devInfo.deviceName = value;
				OnPropertyChanged("DeviceName");
			}
		}

		///  <summary>
		///  Get/set method for device samples per second (approximation).
		///  </summary>
		internal protected UInt16 SampleLength
		{
			get { return devInfo.samplesLength; }
			set
			{
				devInfo.samplesLength = value;
				OnPropertyChanged("SampleLength");
			}
		}

		///  <summary>
		///  Get/set method for device number of channels.
		///  </summary>
		internal protected UInt16 NumberOfChannels
		{
			get { return devInfo.numberOfChannels; }
			set
			{
				devInfo.numberOfChannels = value;
				OnPropertyChanged("NumberOfChannels");
			}
		}

		///  <summary>
		///  Get/set method for device samples per second (for approximation).
		///  </summary>
		internal protected UInt16 SamplesPerSecond
		{
			get { return devInfo.samplesPerSecond; }
			set
			{
				devInfo.samplesPerSecond = value;
				OnPropertyChanged("SamplesPerSecond");
			}
		}

		///  <summary>
		///  Get/set method for device resolution.
		///  </summary>
		internal protected UInt16 ResolutionBits
		{
			get { return devInfo.resolutionBits; }
			set
			{
				devInfo.resolutionBits = value;
				OnPropertyChanged("ResolutionBits");
			}
		}

		///  <summary>
		///  Get/set method for device positive input voltage.
		///  </summary>
		internal protected float InputDifferantialVoltagePositive
		{
			get { return devInfo.inputDifferantialVoltagePositive; }
			set
			{
				devInfo.inputDifferantialVoltagePositive = value;
				OnPropertyChanged("InputDifferantialVoltagePositive");
			}
		}

		///  <summary>
		///  Get/set method for device negative input voltage.
		///  </summary>
		internal protected float InputDifferantialVoltageNegative
		{
			get { return devInfo.inputDifferantialVoltageNegative; }
			set
			{
				devInfo.inputDifferantialVoltageNegative = value;
				OnPropertyChanged("InputDifferantialVoltageNegative");
			}
		}
		#endregion
	}
}
