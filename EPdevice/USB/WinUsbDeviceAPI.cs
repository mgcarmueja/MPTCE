using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace EPdevice
{
	/// <summary>
	///  This class is the API for accessing WinUSB devices.
	/// </summary>

	sealed public partial class WinUsbDevice
	{
		public UInt32 pipeTimeout = 500;		// The host controller cancels transfers that do not complete within the specified time-out interval (in milliseconds).

		#region USB Constants and Enumerations
		internal const Byte USB_DEVICE_DESCRIPTOR_TYPE = 0x01;
		internal const Byte USB_CONFIGURATION_DESCRIPTOR_TYPE = 0x02;
		internal const Byte USB_STRING_DESCRIPTOR_TYPE = 0x03;

		/// <summary>
		/// Use these pipe policies to configure WinUSB for the best match.
		/// </summary>
		internal enum POLICY_TYPE
		{
			SHORT_PACKET_TERMINATE = 1,
			AUTO_CLEAR_STALL,
			PIPE_TRANSFER_TIMEOUT,
			IGNORE_SHORT_PACKETS,
			ALLOW_PARTIAL_READS,
			AUTO_FLUSH,
			RAW_IO,
			MAXIMUM_TRANSFER_SIZE,
			RESET_PIPE_ON_RESUME
		}

		/// <summary>
		/// The USB_CONNECTION_STATUS enumerator indicates the status of the connection to a device on a USB hub port.
		/// </summary>
		internal enum USB_CONNECTION_STATUS
		{
			NoDeviceConnected,
			DeviceConnected,
			DeviceFailedEnumeration,
			DeviceGeneralFailure,
			DeviceCausedOvercurrent,
			DeviceNotEnoughPower,
			DeviceNotEnoughBandwidt,
			DeviceHubNestedTooDeeply,
			DeviceInLegacyHub,
			DeviceEnumerating,
			DeviceReset
		}

		/// <summary>
		/// The USB_DEVICE_SPEED enumeration defines constants for USB device speeds. 
		/// </summary>
		internal enum USB_DEVICE_SPEED
		{
			UsbLowSpeed = 1,
			UsbFullSpeed,
			UsbHighSpeed,
		}

		/// <summary>
		/// The USB_USER_ERROR_CODE enumeration lists the error codes that a USB user-mode request reports when it fails.
		/// </summary>
		internal enum USB_USER_ERROR_CODE
		{
			UsbUserSuccess,
			UsbUserNotSupported,
			UsbUserInvalidRequestCod,
			UsbUserFeatureDisabled,
			UsbUserInvalidHeaderParameter,
			UsbUserInvalidParameter,
			UsbUserMiniportError,
			UsbUserBufferTooSmall,
			UsbUserErrorNotMapped,
			UsbUserDeviceNotStarted,
			UsbUserNoDeviceConnected
		}

		/// <summary>
		/// The USBD_PIPE_TYPE enumerator indicates the type of pipe.
		/// </summary>
		internal enum USBD_PIPE_TYPE
		{
			UsbdPipeTypeControl,
			UsbdPipeTypeIsochronous,
			UsbdPipeTypeBulk,
			UsbdPipeTypeInterrupt
		}
		#endregion

		#region USB structures
		/// <summary>
		/// The USB_CONFIGURATION_DESCRIPTOR structure is used by USB client drivers to hold a USB-defined configuration descriptor.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		internal struct USB_CONFIGURATION_DESCRIPTOR
		{
			internal Byte bLength;
			internal Byte bDescriptorType;
			internal ushort wTotalLength;
			internal Byte bNumInterfaces;
			internal Byte bConfigurationValue;
			internal Byte iConfiguration;
			internal Byte bmAttributes;
			internal Byte MaxPower;
		}

		/// <summary>
		/// The USB_DEVICE_DESCRIPTOR structure is used by USB client drivers to hold a USB-defined configuration descriptor.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		internal struct USB_DEVICE_DESCRIPTOR
		{
			internal Byte bLength;
			internal Byte bDescriptorType;
			internal ushort bcdUSB;
			internal Byte bDeviceClass;
			internal Byte bDeviceSubClass;
			internal Byte bDeviceProtocol;
			internal Byte bMaxPacketSize0;
			internal ushort idVendor;
			internal ushort idProduct;
			internal ushort bcdDevice;
			internal Byte iManufacturer;
			internal Byte iProduct;
			internal Byte iSerialNumber;
			internal Byte bNumConfigurations;
		}

		/// <summary>
		/// The USB_ENDPOINT_DESCRIPTOR structure is used by USB client drivers to retrieve a USB-defined endpoint descriptor.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		internal struct USB_ENDPOINT_DESCRIPTOR
		{
			internal Byte bLength;
			internal Byte bDescriptorType;
			internal Byte bEndpointAddress;
			internal Byte bmAttributes;
			internal ushort wMaxPacketSize;
			internal Byte bInterval;
		}

		/// <summary>
		/// The USB_INTERFACE_DESCRIPTOR structure is used by USB client drivers to retrieve a USB-defined interface descriptor.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		internal struct USB_INTERFACE_DESCRIPTOR
		{
			internal Byte bLength;
			internal Byte bDescriptorType;
			internal Byte bInterfaceNumber;
			internal Byte bAlternateSetting;
			internal Byte bNumEndpoints;
			internal Byte bInterfaceClass;
			internal Byte bInterfaceSubClass;
			internal Byte bInterfaceProtocol;
			internal Byte iInterface;
		}

		/// <summary>
		/// The WINUSB_PIPE_INFORMATION structure contains pipe information that the WinUsb_QueryPipe routine retrieves.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		internal struct WINUSB_PIPE_INFORMATION
		{
			internal USBD_PIPE_TYPE PipeType;
			internal Byte PipeId;
			internal ushort MaximumPacketSize;
			internal Byte Interval;
		}

		/// <summary>
		/// The WINUSB_SETUP_PACKET structure describes a USB setup packet.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct WINUSB_SETUP_PACKET
		{
			internal Byte RequestType;
			internal Byte Request;
			internal ushort Value;
			internal ushort Index;
			internal ushort Length;
		}
		#endregion

		#region USB DLL-import routines
		/// <summary>
		/// The WinUsb_AbortPipe function aborts all of the pending transfers for a pipe. This is a synchronous operation.
		/// </summary>
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_AbortPipe(IntPtr InterfaceHandle, Byte PipeID);

		/// <summary>
		/// The WinUsb_ControlTransfer function transmits control data over a default control endpoint.
		/// </summary>
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_ControlTransfer(IntPtr InterfaceHandle, WINUSB_SETUP_PACKET SetupPacket, Byte[] Buffer, UInt32 BufferLength, ref UInt32 LengthTransferred, IntPtr Overlapped);

		/// <summary>
		/// The WinUsb_FlushPipe function discards any data that is cached in a pipe. This is a synchronous operation.
		/// </summary>
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_FlushPipe(IntPtr InterfaceHandle, Byte PipeID);


		/// <summary>
		/// The WinUsb_Free function releases all of the resources that WinUsb_Initialize allocated. This is a synchronous operation.
		/// </summary>
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_Free(IntPtr InterfaceHandle);

		/// <summary>
		/// The WinUsb_GetAssociatedInterface function retrieves a handle for an associated interface. This is a synchronous operation.
		/// </summary>
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_GetAssociatedInterface(IntPtr InterfaceHandle, Byte AssociatedInterfaceIndex, ref IntPtr AssociatedInterfaceHandle);

		/// <summary>
		/// The WinUsb_GetCurrentAlternateSetting function gets the current alternate interface setting for an interface. This is a synchronous operation.
		/// </summary>
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_GetCurrentAlternateSetting(IntPtr InterfaceHandle, Byte[] AlternateSetting);

		/// <summary>
		/// The WinUsb_GetDescriptor function returns the requested descriptor. This is a synchronous operation.
		/// </summary>
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_GetDescriptor(IntPtr InterfaceHandle, Byte DescriptorType, Byte Index, UInt32 LanguageID, ref USB_DEVICE_DESCRIPTOR Buffer, UInt32 BufferLenght, ref UInt32 LengthTransferred);

		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_GetDescriptor(IntPtr InterfaceHandle, Byte DescriptorType, Byte Index, UInt32 LanguageID, Byte[] Buffer, UInt32 BufferLenght, ref UInt32 LengthTransferred);

		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_GetDescriptor(IntPtr InterfaceHandle, Byte DescriptorType, Byte Index, UInt32 LanguageID, ref USB_CONFIGURATION_DESCRIPTOR Buffer, UInt32 BufferLength, ref UInt32 LengthTransfered);
      
		/// <summary>
		/// The WinUsb_GetOverlappedResult function retrieves the results of an overlapped operation on the specified file.
		/// </summary>
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_GetOverlappedResult(IntPtr InterfaceHandle, IntPtr Overlapped, ref UInt32 lpNumberOfBytesTransferred, Boolean bWait);

		/// <summary>
		/// The WinUsb_GetPipePolicy function retrieves the policy for a specific pipe associated with an endpoint on the device. This is a synchronous operation.
		/// </summary>
		//_Out_ PVOID Value
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_GetPipePolicy(IntPtr InterfaceHandle, UInt32 PolicyType, ref UInt32 ValueLength, ref UInt32 Value);

		/// <summary>
		/// The WinUsb_GetPowerPolicy function retrieves the power policy for a device. This is a synchronous operation.
		/// </summary>
		// _Out_ PVOID Value
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_GetPowerPolicy(IntPtr InterfaceHandle, UInt32 PolicyType, ref UInt32 ValueLength, ref UInt32 Value);

		/// <summary>
		/// The WinUsb_Initialize function creates a WinUSB handle for the device specified by a file handle.
		/// </summary>
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_Initialize(SafeFileHandle DeviceHandle, ref IntPtr InterfaceHandle);

		/// <summary>
		/// The WinUsb_QueryDeviceInformation function gets information about the physical device that is associated with a WinUSB interface handle.
		/// </summary>
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_QueryDeviceInformation(IntPtr InterfaceHandle, UInt32 InformationType, ref UInt32 BufferLength, ref Byte Buffer);

		/// <summary>
		/// The WinUsb_QueryInterfaceSettings function retrieves the interface descriptor for the specified alternate interface settings for a particular interface handle.
		/// </summary>
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_QueryInterfaceSettings(IntPtr InterfaceHandle, Byte AlternateInterfaceNumber, ref USB_INTERFACE_DESCRIPTOR UsbAltInterfaceDescriptor);

		/// <summary>
		/// The WinUsb_QueryPipe function retrieves information about the specified endpoint and the associated pipe for an interface.
		/// </summary>
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_QueryPipe(IntPtr InterfaceHandle, Byte AlternateInterfaceNumber, Byte PipeIndex, ref WINUSB_PIPE_INFORMATION PipeInformation);

		/// <summary>
		/// The WinUsb_ReadPipe function reads data from the specified pipe.
		/// </summary>
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_ReadPipe(IntPtr InterfaceHandle, Byte PipeID, Byte[] Buffer, UInt32 BufferLength, ref UInt32 LengthTransferred, IntPtr Overlapped);

		/// <summary>
		/// The WinUsb_ResetPipe function resets the data toggle and clears the stall condition on a pipe.
		/// </summary>
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_ResetPipe(IntPtr InterfaceHandle, Byte PipeID);

		/// <summary>
		/// The WinUsb_SetCurrentAlternateSetting function sets the alternate setting of an interface.
		/// </summary>
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_SetCurrentAlternateSetting(IntPtr InterfaceHandle, Byte AlternateSetting);

		/// <summary>
		/// The WinUsb_SetPipePolicy function sets the policy for a specific pipe associated with an endpoint on the device. This is a synchronous operation.
		/// Two declarations follow.
		/// </summary>
		// Use this one when the returned Value is a Byte (all except PIPE_TRANSFER_TIMEOUT):
		[DllImport("winusb.dll", SetLastError = true, EntryPoint = "WinUsb_SetPipePolicy")]
		internal static extern Boolean WinUsb_SetPipePolicy8(IntPtr InterfaceHandle, Byte PipeID, UInt32 PolicyType, UInt32 ValueLength, ref Byte Value);

		// Use this alias when the returned Value is a UInt32 (PIPE_TRANSFER_TIMEOUT only):
		[DllImport("winusb.dll", SetLastError = true, EntryPoint = "WinUsb_SetPipePolicy")]
		internal static extern Boolean WinUsb_SetPipePolicy32(IntPtr InterfaceHandle, Byte PipeID, UInt32 PolicyType, UInt32 ValueLength, ref UInt32 Value);

		/// <summary>
		/// The WinUsb_SetPowerPolicy function sets the power policy for a device.
		/// </summary>
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_SetPowerPolicy(IntPtr InterfaceHandle, UInt32 PolicyType, UInt32 ValueLength, UInt32 Value);

		/// <summary>
		/// The WinUsb_WritePipe function writes data to a pipe.
		/// </summary>
		[DllImport("winusb.dll", SetLastError = true)]
		internal static extern Boolean WinUsb_WritePipe(IntPtr InterfaceHandle, Byte PipeID, Byte[] Buffer, UInt32 BufferLength, ref UInt32 LengthTransferred, IntPtr Overlapped);
		#endregion
	}
}
