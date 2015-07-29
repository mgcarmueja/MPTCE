using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

//using FileManager;

namespace EPdevice
{
	///  <summary>
	///  This abstract class contains the most important settings & methods 
	///  in order to handle a electrophysiological device.
	///  </summary>
	abstract public partial class GENdevice : IEquatable<GENdevice>, INotifyPropertyChanged
	{

		///  <summary>
		///  Constructor.
		///  </summary>
		public GENdevice(Guid deviceGuid, String devicePath = null)
		{
			EPdeviceType = this.ToString().Substring(this.ToString().LastIndexOf(".") + 1);
			DeviceClassGuid = deviceGuid;
			DevicePath = devicePath;
		}

        /*
		///  <summary>
		///  Destructor.
		///  </summary>
		~GENdevice() { }
        */

		///  <summary>
		///  Implementation for IEquatable.
		///  </summary>
		public Boolean Equals(GENdevice obj)
		{
			return DevicePath.Equals(obj.DevicePath);
		}

		///  <summary>
		///  Validate if the vendor and product id match.
		///  </summary>
		///  		
		///  <returns> True on success. </returns>
		internal protected Boolean ValidateVendorProductID(UInt16 vendor_id, UInt16 product_id)
		{
			return ((vendor_id == VendorID) && (product_id == ProductID));
		}

		///  <summary>
		///  Initializes the EP device.
		///  </summary>
		///  
		///  <returns> True on success. </returns>
		abstract internal Boolean InitializeDevice(String devPath = null);

		///  <summary>
		///  Write configuration to the EP device.
		///  </summary>
		///  
		///  <returns> True on success. </returns>  
		abstract internal Boolean WriteConfiguration();

		///  <summary>
		///  Reads the configuration of the EP device.
		///  </summary>
		///  
		///  <returns> True on success. </returns>
		abstract internal Boolean ReadConfiguration();

		///  <summary>
		///  Process data from EP device.
		///  </summary>
		abstract internal void ProcessData();

        /*
		///  <summary>
		///  Save data from EP device.
		///  </summary>
		abstract internal void SaveData(HDF5group myHDF5device);
         */ 
	}
}
