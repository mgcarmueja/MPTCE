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
		#region Device installation structures and constants
		internal const Int32 DIGCF_PRESENT = 2;
		internal const Int32 DIGCF_DEVICEINTERFACE = 0X10;

		[StructLayout(LayoutKind.Sequential)]
		internal struct SP_DEVICE_INTERFACE_DATA
		{
			internal Int32 cbSize;
			internal Guid InterfaceClassGuid;
			internal Int32 Flags;
			internal IntPtr Reserved;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
		{
			internal Int32 cbSize;
			internal String DevicePath;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct SP_DEVINFO_DATA
		{
			internal Int32 cbSize;
			internal Guid ClassGuid;
			internal Int32 DevInst;
			internal Int32 Reserved;
		}
		#endregion

		#region Device management structures and constants
		internal const Int32 DBT_DEVICEARRIVAL = 0x8000;
		internal const Int32 DBT_DEVICEREMOVECOMPLETE = 0x8004;
		internal const Int32 DBT_DEVTYP_DEVICEINTERFACE = 5;
		internal const Int32 DBT_DEVTYP_HANDLE = 6;
		internal const Int32 DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;
		internal const Int32 DEVICE_NOTIFY_SERVICE_HANDLE = 1;
		internal const Int32 DEVICE_NOTIFY_WINDOW_HANDLE = 0;
		internal const Int32 WM_DEVICECHANGE = 0x219;

		// Two declarations for the DEV_BROADCAST_DEVICEINTERFACE structure.
		// Use this one in the call to RegisterDeviceNotification().
		[StructLayout(LayoutKind.Sequential)]
		internal class DEV_BROADCAST_DEVICEINTERFACE
		{
			internal Int32 dbcc_size;
			internal Int32 dbcc_devicetype;
			internal Int32 dbcc_reserved;
			internal Guid dbcc_classguid;
			internal Int16 dbcc_name;
		}

		// Use this to read the device path name and guid.
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		internal class DEV_BROADCAST_DEVICEINTERFACE_1
		{
			internal Int32 dbcc_size;
			internal Int32 dbcc_devicetype;
			internal Int32 dbcc_reserved;
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
			internal Byte[] dbcc_classguid;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
			internal Char[] dbcc_name;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal class DEV_BROADCAST_HDR
		{
			internal Int32 dbch_size;
			internal Int32 dbch_devicetype;
			internal Int32 dbch_reserved;
		}
		#endregion

		#region Device installation DLL-imported routines
		/// <summary>
		/// The SetupDiCreateDeviceInfoList function creates an empty device information set and optionally associates the set with a device setup class and a top-level window.
		/// </summary>
		[DllImport("setupapi.dll", SetLastError = true)]
		internal static extern Int32 SetupDiCreateDeviceInfoList(ref Guid ClassGuid, Int32 hwndParent);

		/// <summary>
		/// The SetupDiDestroyDeviceInfoList function deletes a device information set and frees all associated memory.
		/// </summary>
		[DllImport("setupapi.dll", SetLastError = true)]
		internal static extern Int32 SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

		/// <summary>
		/// The SetupDiEnumDeviceInterfaces function enumerates the device interfaces that are contained in a device information set. 
		/// </summary>
		[DllImport("setupapi.dll", SetLastError = true)]
		internal static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref Guid InterfaceClassGuid, Int32 MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

		/// <summary>
		/// The SetupDiGetClassDevs function returns a handle to a device information set that contains requested device information elements for a local computer. 
		/// </summary>
		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
		internal static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, Int32 Flags);

		/// <summary>
		/// The SetupDiGetDeviceInterfaceDetail function returns details about a device interface.
		/// </summary>
		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
		internal static extern Boolean SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, Int32 DeviceInterfaceDetailDataSize, ref Int32 RequiredSize, IntPtr DeviceInfoData);
		#endregion

		#region Device management DLL-imported routines
		/// <summary>
		/// Registers the device or type of device for which a window will receive notifications.
		/// </summary>
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, Int32 Flags);

		/// <summary>
		/// Closes the specified device notification handle.
		/// </summary>
		[DllImport("user32.dll", SetLastError = true)]
		internal static extern Boolean UnregisterDeviceNotification(IntPtr Handle);
		#endregion
	}
}
