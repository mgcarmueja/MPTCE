using System;
using System.Runtime.InteropServices;

namespace EPdevice
{
	///  <summary>
	///  This class provides routines for detecting devices and receiving device notifications.
	///  </summary>
	///  
	sealed public partial class UsbDevice
	{
		///  <summary>
		///  Use SetupDi API functions to retrieve the device path name of an attached device that belongs to a device interface class.
		///  </summary>
		///  
		///  <param name="deviceGuid"> an interface class GUID. </param>
		///  <param name="devicePathNames"> a pointer to an array of device path names of attached deviced. </param>
		///  
		///  <returns>
		///   True if a devices are found, False if not. 
		///  </returns>
		internal Boolean FindDevice(Guid deviceGuid, ref String[] devicePathNames)
		{
			IntPtr deviceInfoSet = IntPtr.Zero;
			Int32 memberIndex = 0;
			SP_DEVICE_INTERFACE_DATA MyDeviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
			Int32 bufferSize = 0;
			IntPtr detailDataBuffer = IntPtr.Zero;
			IntPtr pDevicePathName = IntPtr.Zero;
			Boolean devicesFound = false, moreDevices, success;

			try
			{
				// SetupDiGetClassDevs returns a pointer to an array of structures containing informations about all devices 
				// in the device interface class specified by the Guid. 
				deviceInfoSet = SetupDiGetClassDevs(ref deviceGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

				// Save the size of the structure.
				MyDeviceInterfaceData.cbSize = Marshal.SizeOf(MyDeviceInterfaceData);

				do
				{
					// SetupDiEnumDeviceInterfaces takes the previous return SP_DEVINFO_DATA and looks up their members.
					// We receive a SP_DEVICE_INTERFACE_DATA here.
					moreDevices = SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref deviceGuid, memberIndex, ref MyDeviceInterfaceData);

					if (moreDevices)
					{
						// SetupDiGetDeviceInterfaceDetail returns a structure with the device path name and its size as a SP_DEVICE_INTERFACE_DETAIL_DATA.
						// Calling this function we don't know the size of the structure and the function won't return the structure unless we call it with the correct size.
						// That's why we need to call the function twice.

						// The first call  won't return the struture but an error. Still we can retrieve the size of the structure.
						success = SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref MyDeviceInterfaceData, IntPtr.Zero, 0, ref bufferSize, IntPtr.Zero);

						// Allocate the memory for SP_DEVICE_INTERFACE_DETAIL_DATA of the size of bufferSize.
						detailDataBuffer = Marshal.AllocHGlobal(bufferSize);

						// Copy the bufferSize into the start of the structure, size depends on 32 or 64-Bit OS.
						Marshal.WriteInt32(detailDataBuffer, (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);

						// The second call is now correct and a pointer to the structure is returned within detailDataBuffer.
						success = SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref MyDeviceInterfaceData, detailDataBuffer, bufferSize, ref bufferSize, IntPtr.Zero);

						// The first four bytes are the buffersize.
						pDevicePathName = new IntPtr(detailDataBuffer.ToInt32() + 4);

						// Now make space for the next device and get the string with the device path.
						Array.Resize(ref devicePathNames, devicePathNames.Length + 1);
						devicePathNames[devicePathNames.Length - 1] = Marshal.PtrToStringAuto(pDevicePathName);

						devicesFound = true;
					}

					memberIndex++;
				} while (moreDevices == true);

				return devicesFound;

			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				if (detailDataBuffer != IntPtr.Zero)
					Marshal.FreeHGlobal(detailDataBuffer);

				if (deviceInfoSet != IntPtr.Zero)
					SetupDiDestroyDeviceInfoList(deviceInfoSet);
			}
		}
		internal Boolean FindDevice(Guid deviceGuid, ref String devicePathName)
		{
			String[] devicePathNames = { };

			if ((this.FindDevice(deviceGuid, ref devicePathNames) == true) && (devicePathNames.Length == 1))
			{
				devicePathName = devicePathNames[devicePathNames.Length - 1];
				return true;
			}


			devicePathName = "";
			return false;
		}

		///  <summary>
		///  Requests to receive a notification when a device is attached or removed.
		///  </summary>
		///  
		///  <param name="formHandle"> handle to the window that will receive device events. </param>
		///  <param name="classGuid"> device interface GUID. </param>
		///  <param name="deviceNotificationHandle"> returned device notification handle. </param>
		///  
		///  <returns>
		///  True on success.
		///  </returns>
		internal Boolean RegisterForDeviceNotifications(IntPtr formHandle, Guid classGuid, ref IntPtr deviceNotificationHandle)
		{
			// A DEV_BROADCAST_DEVICEINTERFACE header holds information about the request.
			DEV_BROADCAST_DEVICEINTERFACE devBroadcastDeviceInterface = new DEV_BROADCAST_DEVICEINTERFACE();

			IntPtr devBroadcastDeviceInterfaceBuffer = IntPtr.Zero;
			Int32 size = 0;

			try
			{
				// Set the parameters in the DEV_BROADCAST_DEVICEINTERFACE structure.
				// Set the size of this structure
				size = Marshal.SizeOf(devBroadcastDeviceInterface);
				devBroadcastDeviceInterface.dbcc_size = size;

				// Request to receive notifications about a class of devices.
				devBroadcastDeviceInterface.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
				// Reserved; do not use.
				devBroadcastDeviceInterface.dbcc_reserved = 0;
				// Specify the interface class to receive notifications about.
				devBroadcastDeviceInterface.dbcc_classguid = classGuid;

				// Allocate memory for the buffer that holds the DEV_BROADCAST_DEVICEINTERFACE structure.
				devBroadcastDeviceInterfaceBuffer = Marshal.AllocHGlobal(size);

				// Copy the DEV_BROADCAST_DEVICEINTERFACE structure to the buffer. We are then ready to call the API function. Set fDeleteOld True to prevent memory leaks.
				Marshal.StructureToPtr(devBroadcastDeviceInterface, devBroadcastDeviceInterfaceBuffer, true);

				// Call deviceNotificationHandle to request to receive notification messages when a device in an interface class is attached or removed.
				deviceNotificationHandle = RegisterDeviceNotification(formHandle, devBroadcastDeviceInterfaceBuffer, DEVICE_NOTIFY_WINDOW_HANDLE);

				// Marshal data from the unmanaged block devBroadcastDeviceInterfaceBuffer to the managed object devBroadcastDeviceInterface
				Marshal.PtrToStructure(devBroadcastDeviceInterfaceBuffer, devBroadcastDeviceInterface);

				if ((deviceNotificationHandle.ToInt32() == IntPtr.Zero.ToInt32()))
					return false;
				else
					return true;
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				if (devBroadcastDeviceInterfaceBuffer != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(devBroadcastDeviceInterfaceBuffer);
				}
			}
		}

		///  <summary>
		///  Requests to stop receiving notification messages when a device in an interface class is attached or removed.
		///  </summary>
		///  
		///  <param name="deviceNotificationHandle"> handle returned previously by RegisterDeviceNotification. </param>
		///  
		///  <returns>
		///  True on success.
		///  </returns>
		internal Boolean UnregisterForDeviceNotifications(IntPtr deviceNotificationHandle)
		{
			Boolean success;
			try
			{
				success = UnregisterDeviceNotification(deviceNotificationHandle);
				return success;
			}
			catch (Exception)
			{
				throw;
			}
		}

		///  <summary>
		///  Checks if a device with the given path exists.
		///  </summary>
		///  
		///  <param name="devicePathName"> path to the device to check on. </param>
		///  
		///  <returns>
		///  True if device exists.
		///  </returns>
		internal Boolean CheckDevice(Guid deviceGuid, ref String devicePathName)
		{
			String[] devicePathNames = { };

			if (this.FindDevice(deviceGuid, ref devicePathNames))
				foreach (String tDevicePath in devicePathNames)
					if (String.Compare(tDevicePath, devicePathName, StringComparison.OrdinalIgnoreCase) == 0)
						return true;

			return false;
		}
	}
}
