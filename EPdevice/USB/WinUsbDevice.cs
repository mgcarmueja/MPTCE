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
		/// <summary>
		/// In this structure we want to hold informations about a device and its endpoints.
		/// </summary>
		internal struct WINUSB_DEVICE_INFO
		{
			internal SafeFileHandle deviceHandle;
			internal IntPtr winUsbHandle;
			internal Byte bulkInPipe;
			internal Byte bulkOutPipe;
			internal Byte interruptInPipe;
			internal Byte interruptOutPipe;
		}

		// Create some structures
		internal WINUSB_DEVICE_INFO devInfo = new WINUSB_DEVICE_INFO();
		internal USB_DEVICE_DESCRIPTOR usbDevDescriptor = new USB_DEVICE_DESCRIPTOR();
		internal USB_INTERFACE_DESCRIPTOR usbInterfaceDescriptor = new USB_INTERFACE_DESCRIPTOR();

		#region Initializing and setting up device
		///  <summary>
		///  Requests a handle with CreateFile.
		///  </summary>
		///  
		///  <param name="devicePathName"> Returned by SetupDiGetDeviceInterfaceDetail in an SP_DEVICE_INTERFACE_DETAIL_DATA structure. </param>
		///  
		///  <returns>
		///  The handle.
		///  </returns>
		internal Boolean GetDeviceHandle(String devicePathName)
		{
			devInfo.deviceHandle = FileIO.CreateFile(devicePathName, (FileIO.GENERIC_WRITE | FileIO.GENERIC_READ), (FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE),
				IntPtr.Zero, FileIO.OPEN_EXISTING, (FileIO.FILE_ATTRIBUTE_NORMAL | FileIO.FILE_FLAG_OVERLAPPED), 0);

			if (!(devInfo.deviceHandle.IsInvalid))
				return true;
			else
				return false;
		}

		///  <summary>
		///  Closes the device handle obtained with CreateFile and frees resources.
		///  </summary>
		internal void CloseDeviceHandle()
		{
			try
			{
				WinUsb_Free(devInfo.winUsbHandle);

				if (!(devInfo.deviceHandle == null))
					if (!(devInfo.deviceHandle.IsInvalid))
						devInfo.deviceHandle.Close();
			}
			catch (Exception)
			{
				throw;
			}
		}

		/// <summary>
		///  Initializes a device interface and obtains information about it.
		/// </summary>
		/// 
		///  <returns> True on success. </returns>
		internal Boolean InitializeDevice()
		{
			WINUSB_PIPE_INFORMATION pipeInfo = new WINUSB_PIPE_INFORMATION();
			Boolean success;

			usbInterfaceDescriptor = default(USB_INTERFACE_DESCRIPTOR);

			try
			{
				// Let's try to initialize the device. If it works we can access the devica via winUsbHandle
				success = WinUsb_Initialize(devInfo.deviceHandle, ref devInfo.winUsbHandle);

				if (success)
				{
					// Lets request an interface descriptor.
					success = WinUsb_QueryInterfaceSettings(devInfo.winUsbHandle, 0, ref usbInterfaceDescriptor);

					if (success)
					{
						// For each endpoint we can query the pipe to learn the endpoint's transfer type & direction and save it to devInfo.
						for (Int32 i = 0; i <= (usbInterfaceDescriptor.bNumEndpoints - 1); i++)
						{
							WinUsb_QueryPipe(devInfo.winUsbHandle, 0, System.Convert.ToByte(i), ref pipeInfo);

							if (((pipeInfo.PipeType == USBD_PIPE_TYPE.UsbdPipeTypeBulk) & UsbEndpointDirectionIn(pipeInfo.PipeId)))
							{
								devInfo.bulkInPipe = pipeInfo.PipeId;
								SetPipePolicy(devInfo.bulkInPipe, Convert.ToUInt32(POLICY_TYPE.SHORT_PACKET_TERMINATE), Convert.ToByte(false));
								SetPipePolicy(devInfo.bulkInPipe, Convert.ToUInt32(POLICY_TYPE.AUTO_CLEAR_STALL), Convert.ToByte(false));
								SetPipePolicy(devInfo.bulkInPipe, Convert.ToUInt32(POLICY_TYPE.PIPE_TRANSFER_TIMEOUT), Convert.ToUInt32(pipeTimeout));
								SetPipePolicy(devInfo.bulkInPipe, Convert.ToUInt32(POLICY_TYPE.IGNORE_SHORT_PACKETS), Convert.ToByte(false));
								SetPipePolicy(devInfo.bulkInPipe, Convert.ToUInt32(POLICY_TYPE.ALLOW_PARTIAL_READS), Convert.ToByte(true));
								SetPipePolicy(devInfo.bulkInPipe, Convert.ToUInt32(POLICY_TYPE.AUTO_FLUSH), Convert.ToByte(false));
								SetPipePolicy(devInfo.bulkInPipe, Convert.ToUInt32(POLICY_TYPE.RAW_IO), Convert.ToByte(false));
								SetPipePolicy(devInfo.bulkInPipe, Convert.ToUInt32(POLICY_TYPE.RESET_PIPE_ON_RESUME), Convert.ToByte(false));
							}
							else if (((pipeInfo.PipeType == USBD_PIPE_TYPE.UsbdPipeTypeBulk) & UsbEndpointDirectionOut(pipeInfo.PipeId)))
							{
								devInfo.bulkOutPipe = pipeInfo.PipeId;
								SetPipePolicy(devInfo.bulkOutPipe, Convert.ToUInt32(POLICY_TYPE.SHORT_PACKET_TERMINATE), Convert.ToByte(false));
								SetPipePolicy(devInfo.bulkOutPipe, Convert.ToUInt32(POLICY_TYPE.AUTO_CLEAR_STALL), Convert.ToByte(false));
								SetPipePolicy(devInfo.bulkOutPipe, Convert.ToUInt32(POLICY_TYPE.PIPE_TRANSFER_TIMEOUT), Convert.ToUInt32(pipeTimeout));
								SetPipePolicy(devInfo.bulkOutPipe, Convert.ToUInt32(POLICY_TYPE.IGNORE_SHORT_PACKETS), Convert.ToByte(false));
								SetPipePolicy(devInfo.bulkOutPipe, Convert.ToUInt32(POLICY_TYPE.ALLOW_PARTIAL_READS), Convert.ToByte(true));
								SetPipePolicy(devInfo.bulkOutPipe, Convert.ToUInt32(POLICY_TYPE.AUTO_FLUSH), Convert.ToByte(false));
								SetPipePolicy(devInfo.bulkOutPipe, Convert.ToUInt32(POLICY_TYPE.RAW_IO), Convert.ToByte(false));
								SetPipePolicy(devInfo.bulkOutPipe, Convert.ToUInt32(POLICY_TYPE.RESET_PIPE_ON_RESUME), Convert.ToByte(false));
							}
							else if ((pipeInfo.PipeType == USBD_PIPE_TYPE.UsbdPipeTypeInterrupt) & UsbEndpointDirectionIn(pipeInfo.PipeId))
							{
								devInfo.interruptInPipe = pipeInfo.PipeId;
								SetPipePolicy(devInfo.interruptInPipe, Convert.ToUInt32(POLICY_TYPE.SHORT_PACKET_TERMINATE), Convert.ToByte(false));
								SetPipePolicy(devInfo.interruptInPipe, Convert.ToUInt32(POLICY_TYPE.AUTO_CLEAR_STALL), Convert.ToByte(false));
								SetPipePolicy(devInfo.interruptInPipe, Convert.ToUInt32(POLICY_TYPE.PIPE_TRANSFER_TIMEOUT), Convert.ToUInt32(pipeTimeout));
								SetPipePolicy(devInfo.interruptInPipe, Convert.ToUInt32(POLICY_TYPE.IGNORE_SHORT_PACKETS), Convert.ToByte(false));
								SetPipePolicy(devInfo.interruptInPipe, Convert.ToUInt32(POLICY_TYPE.ALLOW_PARTIAL_READS), Convert.ToByte(true));
								SetPipePolicy(devInfo.interruptInPipe, Convert.ToUInt32(POLICY_TYPE.AUTO_FLUSH), Convert.ToByte(false));
								SetPipePolicy(devInfo.interruptInPipe, Convert.ToUInt32(POLICY_TYPE.RAW_IO), Convert.ToByte(false));
								SetPipePolicy(devInfo.interruptInPipe, Convert.ToUInt32(POLICY_TYPE.RESET_PIPE_ON_RESUME), Convert.ToByte(false));
							}
							else if ((pipeInfo.PipeType == USBD_PIPE_TYPE.UsbdPipeTypeInterrupt) & UsbEndpointDirectionOut(pipeInfo.PipeId))
							{
								devInfo.interruptOutPipe = pipeInfo.PipeId;
								SetPipePolicy(devInfo.interruptOutPipe, Convert.ToUInt32(POLICY_TYPE.SHORT_PACKET_TERMINATE), Convert.ToByte(false));
								SetPipePolicy(devInfo.interruptOutPipe, Convert.ToUInt32(POLICY_TYPE.AUTO_CLEAR_STALL), Convert.ToByte(false));
								SetPipePolicy(devInfo.interruptOutPipe, Convert.ToUInt32(POLICY_TYPE.PIPE_TRANSFER_TIMEOUT), Convert.ToUInt32(pipeTimeout));
								SetPipePolicy(devInfo.interruptOutPipe, Convert.ToUInt32(POLICY_TYPE.IGNORE_SHORT_PACKETS), Convert.ToByte(false));
								SetPipePolicy(devInfo.interruptOutPipe, Convert.ToUInt32(POLICY_TYPE.PIPE_TRANSFER_TIMEOUT), Convert.ToByte(true));
								SetPipePolicy(devInfo.interruptOutPipe, Convert.ToUInt32(POLICY_TYPE.AUTO_FLUSH), Convert.ToByte(false));
								SetPipePolicy(devInfo.interruptOutPipe, Convert.ToUInt32(POLICY_TYPE.RAW_IO), Convert.ToByte(false));
								SetPipePolicy(devInfo.interruptOutPipe, Convert.ToUInt32(POLICY_TYPE.RESET_PIPE_ON_RESUME), Convert.ToByte(false));
							}
						}
					}
				}
			}
			catch (Exception)
			{
				throw;
			}

			return success;
		}

		///  <summary>
		///  Sets pipe policy. Used when the value parameter is a Byte (all except PIPE_TRANSFER_TIMEOUT).
		///  </summary>
		///  
		///  <param name="pipeId"> Pipe to set a policy for. </param>
		///  <param name="policyType"> POLICY_TYPE member. </param>
		///  <param name="value"> Policy value. </param>
		///  
		///  <returns> True on success. </returns>
		private Boolean SetPipePolicy(Byte pipeId, UInt32 policyType, Byte value)
		{
			try
			{
				return WinUsb_SetPipePolicy8(devInfo.winUsbHandle, pipeId, policyType, 1, ref value);
			}
			catch (Exception)
			{
				throw;
			}
		}

		///  <summary>
		///  Sets pipe policy. Used when the value parameter is a UInt32 (PIPE_TRANSFER_TIMEOUT only).
		///  </summary>
		///  
		///  <param name="pipeId"> Pipe to set a policy for. </param>
		///  <param name="policyType"> POLICY_TYPE member. </param>
		///  <param name="value"> Policy value. </param>
		///  
		///  <returns> True on success. </returns>
		private Boolean SetPipePolicy(Byte pipeId, UInt32 policyType, UInt32 value)
		{
			try
			{
				return WinUsb_SetPipePolicy32(devInfo.winUsbHandle, pipeId, policyType, 4, ref value);
			}
			catch (Exception)
			{
				throw;
			}
		}

		///  <summary>
		///  Is the endpoint's direction IN (device to host)?
		///  </summary>
		///  
		///  <param name="addr"> The endpoint address. </param>
		///  
		///  <returns>
		///  True if IN (device to host), False if OUT (host to device)
		///  </returns> 
		private Boolean UsbEndpointDirectionIn(Int32 addr)
		{
			try
			{
				return ((addr & 0X80) == 0X80);
			}
			catch (Exception)
			{
				throw;
			}
		}

		///  <summary>
		///  Is the endpoint's direction OUT (host to device)?
		///  </summary>
		///  
		///  <param name="addr"> The endpoint address. </param>
		///  
		///  <returns>
		///  True if OUT (host to device, False if IN (device to host)
		///  </returns>
		private Boolean UsbEndpointDirectionOut(Int32 addr)
		{
			try
			{
				return ((addr & 0X80) == 0);
			}
			catch (Exception)
			{
				throw;
			}
		}

		///  <summary>
		///  Returns the vendor and product id of the device.
		///  </summary>
		///  
		///  <returns>
		///  True if device descriptor was returned.
		///  </returns>
		internal Boolean GetUsbDeviceDescriptor()
		{
			UInt32 transfered = 0, size;
			Boolean success;

			usbDevDescriptor = default(USB_DEVICE_DESCRIPTOR);
			size = Convert.ToUInt32(Marshal.SizeOf(usbDevDescriptor));

			success = WinUsb_GetDescriptor(devInfo.winUsbHandle, USB_DEVICE_DESCRIPTOR_TYPE, 0, 0, ref usbDevDescriptor, size, ref transfered);

			return ((transfered == size) && (success));
		}

		public String GetUsbStringDescriptor(Byte index)
		{
			Byte[] buffer = new Byte[256];
			UInt32 transfered = 0;
			Int32 length;
			Boolean success;

			success = WinUsb_GetDescriptor(devInfo.winUsbHandle, USB_STRING_DESCRIPTOR_TYPE,
							index, 0, buffer, (uint)buffer.Length, ref transfered);

			length = buffer[0] - 2;
			if (length <= 0)
				return null;

			char[] chars = System.Text.Encoding.Unicode.GetChars(buffer, 2, length);
			return new String(chars);
		}


		///  <summary>
		///  Aborts all of the pending transfers for a pipe.
		///  </summary>
		///  
		///  <returns>
		///  True if the operation succeeds.
		///  </returns>
		internal Boolean AbortPipe(Byte PipeID)
		{
			return WinUsb_AbortPipe(devInfo.winUsbHandle, PipeID);
		}

		///  <summary>
		///  Discards any data that is cached in a pipe.
		///  </summary>
		///  
		///  <returns>
		///  True if the operation succeeds.
		///  </returns>
		internal Boolean FlushPipe(Byte PipeID)
		{
			return WinUsb_FlushPipe(devInfo.winUsbHandle, PipeID);
		}

		///  <summary>
		///  Resets the data toggle and clears the stall condition on a pipe.
		///  </summary>
		///  
		///  <returns>
		///  True if the operation succeeds.
		///  </returns>
		internal Boolean ResetPipe(Byte PipeID)
		{
			return WinUsb_ResetPipe(devInfo.winUsbHandle, PipeID);
		}

		#endregion

		#region Data transfer
		///  <summary>
		///  Attempts to read data from a bulk IN endpoint.
		///  </summary>
		///  
		///  <param name="InterfaceHandle"> Device interface handle. </param>
		///  <param name="PipeID"> Endpoint address. </param>
		///  <param name="bytesToRead"> Number of bytes to read. </param>
		///  <param name="Buffer"> Buffer for storing the bytes read. </param>
		///  <param name="bytesRead"> Number of bytes read. </param>
		///  <param name="success"> Success or failure status. </param>  
		internal void ReadBulkData(Byte pipeID, UInt32 bytesToRead, ref Byte[] buffer, ref UInt32 bytesRead, ref Boolean success)
		{
			try
			{
				success = WinUsb_ReadPipe(devInfo.winUsbHandle, pipeID, buffer, bytesToRead, ref bytesRead, IntPtr.Zero);

				/*if ((!success) && (bytesRead == 0))
					CloseDeviceHandle();*/
			}
			catch (Exception)
			{
				throw;
			}
		}

		///  <summary>
		///  Attempts to send data via a bulk OUT endpoint.
		///  </summary>
		///  
		///  <param name="buffer"> Buffer containing the bytes to write. </param>
		///  <param name="bytesToWrite"> Number of bytes to write. </param>
		///  
		///  <returns> True on success. </returns>
		internal Boolean SendBulkData(Byte pipeID, ref Byte[] buffer, UInt32 bytesToWrite)
		{
			UInt32 bytesWritten = 0;
			Boolean success;

			try
			{
				success = WinUsb_WritePipe(devInfo.winUsbHandle, pipeID, buffer, bytesToWrite, ref bytesWritten, IntPtr.Zero);

				if (!(success))
					CloseDeviceHandle();
				return success;
			}
			catch (Exception)
			{
				throw;
			}
		}

		///  <summary>
		///  Initiates a Control Read transfer. Data stage is device to host.
		///  </summary>
		/// 
		///  <param name="dataStage"> The received data. </param>
		///  
		///  <returns> True on success. </returns>
		internal Boolean DoControlReadTransfer(ref Byte[] dataStage)
		{
			UInt32 bytesReturned = 0;
			WINUSB_SETUP_PACKET setupPacket;

			try
			{
				//  Vendor-specific request to an interface with device-to-host Data stage.
				setupPacket.RequestType = 0XC1;

				//  The request number that identifies the specific request.
				setupPacket.Request = 2;

				//  Command-specific value to send to the device.
				setupPacket.Index = 0;

				//  Number of bytes in the request's Data stage.
				setupPacket.Length = System.Convert.ToUInt16(dataStage.Length);

				//  Command-specific value to send to the device.
				setupPacket.Value = 0;

				return WinUsb_ControlTransfer(devInfo.winUsbHandle, setupPacket, dataStage, System.Convert.ToUInt16(dataStage.Length), ref bytesReturned, IntPtr.Zero);
			}
			catch (Exception)
			{
				throw;
			}
		}

		///  <summary>
		///  Initiates a Control Write transfer. Data stage is host to device.
		///  </summary>
		///  
		///  <param name="dataStage"> The data to send. </param>
		///  
		///  <returns> True on success. </returns>
		internal Boolean DoControlWriteTransfer(Byte[] dataStage)
		{
			UInt32 bytesReturned = 0;
			ushort index = System.Convert.ToUInt16(0);
			WINUSB_SETUP_PACKET setupPacket;
			ushort value = System.Convert.ToUInt16(0);

			try
			{
				//  Vendor-specific request to an interface with host-to-device Data stage.
				setupPacket.RequestType = 0X41;

				//  The request number that identifies the specific request.
				setupPacket.Request = 1;

				//  Command-specific value to send to the device.
				setupPacket.Index = index;

				//  Number of bytes in the request's Data stage.
				setupPacket.Length = System.Convert.ToUInt16(dataStage.Length);

				//  Command-specific value to send to the device.
				setupPacket.Value = value;

				return WinUsb_ControlTransfer(devInfo.winUsbHandle, setupPacket, dataStage, System.Convert.ToUInt16(dataStage.Length), ref bytesReturned, IntPtr.Zero);
			}
			catch (Exception)
			{
				throw;
			}
		}
		#endregion
	}
}
