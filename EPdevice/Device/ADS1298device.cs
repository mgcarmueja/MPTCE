using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;

using System.Diagnostics;
//using FileManager;


namespace EPdevice
{
	///  <summary>
	///  This class represents an ADS129x device.
	///  </summary>
	sealed public partial class ADS1298device : GENdevice, INotifyPropertyChanged
	{
		///  <summary>
		///  Constructor.
		///  </summary>
		internal ADS1298device(Guid deviceGuid, String devicePath = null)
			: base(deviceGuid, devicePath)
		{
			;
		}

		///  <summary>
		///  Destructor.
		///  </summary>
		/*
        ~ADS1298device()
		{
			;
		}
        */

		///  <summary>
		///  In order to initialize the device we must have a valid class guid and device path name.
		///  Use FindDevice from UsbDevice to look up connected devices.
		///  </summary>
		///
		///  <param name="devicePath"> Unique device path. </param>
		///  
		///  <returns>  True if successful. </returns>
		internal override Boolean InitializeDevice(String devPath = null)
		{
			Byte[] configBuffer = new Byte[Marshal.SizeOf(regConfig)];

			deviceConfigured = false;
			processingData = true;

			// Check for a valid device path.
			if (devPath != null)
				DevicePath = devPath;
			else if ((DevicePath == null) && (devPath == null))
				return false;

			// Check if this devices exists.
			DeviceFound = usbDev.CheckDevice(DeviceClassGuid, ref devInfo.devicePath);

			if (DeviceFound)
			{
				// Pass the devices path name & get the device handle.
				DeviceAttached = winUsbDev.GetDeviceHandle(this.DevicePath);

				if (DeviceAttached)
				{
					// Initialize this device
					DeviceAttached = winUsbDev.InitializeDevice();

					if (DeviceAttached)
					{
						// Lets get the descriptor so we can read the product & vendor id.
						DeviceAttached = winUsbDev.GetUsbDeviceDescriptor();
						VendorID = winUsbDev.usbDevDescriptor.idVendor;
						ProductID = winUsbDev.usbDevDescriptor.idProduct;

						// Flush the pipes.
						winUsbDev.FlushPipe(PIPE_COMMAND_IN);
						winUsbDev.FlushPipe(PIPE_COMMAND_OUT);
						winUsbDev.FlushPipe(PIPE_DATA_IN);

						// Read configuration
						ReadConfiguration();

						while (!deviceConfigured)
						{
							// Let's do a double check here, save the current config & re-read it from the device
							configBuffer = StructureToByteArray(ref regConfig);
							ReadConfiguration();
							if (configBuffer.SequenceEqual(StructureToByteArray(ref regConfig)))
								deviceConfigured = true;
						}

						DeviceName = winUsbDev.GetUsbStringDescriptor(winUsbDev.usbDevDescriptor.iProduct);
						if (String.IsNullOrEmpty(winUsbDev.GetUsbStringDescriptor(winUsbDev.usbDevDescriptor.iProduct)))
						{
							DeviceName = String.Concat(DeviceName,
								String.Concat(Guid.NewGuid().ToString().Replace("-", "").Substring(1, 4)));     // Append random numbers
						}
						else
							DeviceName = String.Concat(DeviceName, " ", winUsbDev.GetUsbStringDescriptor(winUsbDev.usbDevDescriptor.iSerialNumber));

						SampleLength = SAMPLELENGTH;
						ResolutionBits = RESOLUTION;
						InputDifferantialVoltagePositive = INPUT_DIFFERANTIAL_VOLTAGE_POSITIVE;
						InputDifferantialVoltageNegative = INPUT_DIFFERANTIAL_VOLTAGE_NEGATIVE;

						EPdeviceType = this.ToString().Substring(this.ToString().LastIndexOf(".") + 1);
					}
				}
				else
					winUsbDev.CloseDeviceHandle();
			}

			return (DeviceFound && DeviceAttached && deviceConfigured);
		}

		///  <summary>
		///  Writes the current configuration to the ADS1298 device.
		///  </summary>
		///  
		///  <returns> True if successful. </returns>		
		internal override Boolean WriteConfiguration()
		{
			Boolean success;
			Byte[] configBuffer;
			Byte[] writeBuffer = new Byte[USB_PACKET_SIZE];

			configBuffer = StructureToByteArray(ref regConfig);                  // Get the structure into a byte Array

			writeBuffer[0] = CMD_SET_CONFIG;                                     // The first byte must be the CMD_SET_CONFIG byte
			Array.Copy(configBuffer, 0, writeBuffer, 1, configBuffer.Length);    // Followed by the RAW configuration

			success = winUsbDev.SendBulkData(PIPE_COMMAND_OUT, ref writeBuffer, (UInt32)writeBuffer.Length);

			// Write device specific data
			if (DEV_ID.Contains("ADS1294"))
				NumberOfChannels = 4;
			else if (DEV_ID.Contains("ADS1296"))
				NumberOfChannels = 4;
			else if (DEV_ID.Contains("ADS1298"))
				NumberOfChannels = 8;
			else
				NumberOfChannels = 8;
			SamplesPerSecond = (UInt16)((HR) ? DR_VALUE_HR : DR_VALUE_LP);

			return success;
		}

		///  <summary>
		///  Reads the configuration of the ADS1298 device and saves it to the REGISTERMAP structure.
		///  </summary>
		///   
		///  <returns> True if successful. </returns>
		internal override Boolean ReadConfiguration()
		{
			Byte[] readBuffer = new Byte[USB_PACKET_SIZE];
			Byte[] writeBuffer = new Byte[USB_PACKET_SIZE];
			Byte[] configBuffer = new Byte[Marshal.SizeOf(regConfig)];

			UInt32 bytesRead = 0;
			Boolean success;

			writeBuffer[0] = CMD_GET_CONFIG;
			success = winUsbDev.SendBulkData(PIPE_COMMAND_OUT, ref writeBuffer, (UInt32)writeBuffer.Length);

			if (success)
			{
				winUsbDev.ReadBulkData(PIPE_COMMAND_IN, (UInt32)readBuffer.Length, ref readBuffer, ref bytesRead, ref success);
				Array.Copy(readBuffer, 0, configBuffer, 0, Marshal.SizeOf(regConfig));			    // The first byte is CMD_GET_CONFIG.
				regConfig = ByteArrayToStructure(ref configBuffer);									// Save the config to the register structure.
			}

			// Write device specific data
			if (DEV_ID.Contains("ADS1294"))
				NumberOfChannels = 4;
			else if (DEV_ID.Contains("ADS1296"))
				NumberOfChannels = 4;
			else if (DEV_ID.Contains("ADS1298"))
				NumberOfChannels = 8;
			else
				NumberOfChannels = 8;
			SamplesPerSecond = (UInt16)((HR) ? DR_VALUE_HR : DR_VALUE_LP);

			return success;
		}

		///  <summary>
		///  Takes care of reading data.
		///  </summary>
		internal override void ProcessData()
		{
            ThreadStart thSt;

			if (processingData && DataReader != null)    // We are currently reading data from the EPdevice. Finish it.
			{
				processingData = false;
                if (DataReader.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                {
                    while (DataReader.IsAlive)						// Wait for the device to transmit all accumulated data.
                        Thread.Sleep(50);                           // But only if we are not waiting on ourselves!
                }
			}
			else                                         // The device is currently in idle state. Start reading data.
			{
				processingData = true;

                if (liveData == false)
                {
                    thSt = new ThreadStart(ReadData);
                }
                else
                {
                    thSt = new ThreadStart(ReadDataLive);
                }

				DataReader = new Thread(thSt);
				DataReader.Start();
			}
		}



        /*
		///  <summary>
		///  Save data from EP device.
		///  </summary>
		internal override void SaveData(HDF5group hdf5device)
		{
			Int32 rec, data;
			float[] fWriteBuffer;
			byte[] readBuffer = new byte[DATALENGTH];

			Int32[] iWriteBuffer;


			UInt64 counter = 0;
			UInt64[] counterBuffer = new UInt64[1];

			// Write raw data to file.
            if (hdf5device != null) rec = hdf5device.addRecord("EMG RAW", NumberOfChannels, SamplesPerSecond, HDF5group.DataType.INT32, HDF5group.CompressionType.GZIP, 9);
            else rec = 0;

            if (memStream != null)
			{
				memStream.Position = 0;
				while (memStream.Read(readBuffer, 0, DATALENGTH) == DATALENGTH)
				{
					iWriteBuffer = new Int32[8];
					for (UInt32 i = SampleLength, j = 0; i < DATALENGTH; i += SampleLength, j++)
					{
						iWriteBuffer[j] = readBuffer[i] << 24 | readBuffer[i + 1] << 16 | readBuffer[i + 2] << 8;
						iWriteBuffer[j] /= 256;
					}
                    if (hdf5device != null) hdf5device.getRecord(rec).writeData<Int32>(iWriteBuffer);
				}
			}

			// Write attributes
            if (hdf5device != null)
            {
                hdf5device.getRecord(rec).writeAttribute("Manufacturer", winUsbDev.GetUsbStringDescriptor(winUsbDev.usbDevDescriptor.iManufacturer));
                hdf5device.getRecord(rec).writeAttribute("Device name", winUsbDev.GetUsbStringDescriptor(winUsbDev.usbDevDescriptor.iProduct));
                hdf5device.getRecord(rec).writeAttribute("Serial number", winUsbDev.GetUsbStringDescriptor(winUsbDev.usbDevDescriptor.iSerialNumber));
                hdf5device.getRecord(rec).writeAttribute("Number of channels", NumberOfChannels);
                hdf5device.getRecord(rec).writeAttribute("Samples / second", SamplesPerSecond);
                hdf5device.getRecord(rec).writeAttribute("Resolution [Bit]", ResolutionBits);
                hdf5device.getRecord(rec).writeAttribute("+Vref [mV]", (InputDifferantialVoltagePositive).ToString("R"));
                hdf5device.getRecord(rec).writeAttribute("-Vref [mV]", (InputDifferantialVoltageNegative).ToString("R"));
            }
			// Write data + time to file
			counter = 0;
            if (hdf5device != null)  rec = hdf5device.addRecord("EMG + timing", NumberOfChannels + 1, SamplesPerSecond, HDF5group.DataType.FLOAT, HDF5group.CompressionType.GZIP, 9);
			
            if (memStream != null)
			{
				memStream.Position = 0;
				while (memStream.Read(readBuffer, 0, DATALENGTH) == DATALENGTH)
				{
					fWriteBuffer = new float[9];
					fWriteBuffer[0] = counter / (float)SamplesPerSecond;
					for (UInt32 i = SampleLength, j = 1; i < DATALENGTH; i += SampleLength, j++)
					{
						data = readBuffer[i] << 24 | readBuffer[i + 1] << 16 | readBuffer[i + 2] << 8;
						data /= 256;
						// Analog inputs are full differential
						if (data > 0)
							fWriteBuffer[j] = (data * (InputDifferantialVoltagePositive - InputDifferantialVoltageNegative)) 
													/ (float)Math.Pow(2, ResolutionBits-1);
						else if (data < 0)
							fWriteBuffer[j] = (data * (InputDifferantialVoltagePositive - InputDifferantialVoltageNegative)) 
													/ (float)Math.Pow(2, ResolutionBits-1);
						else
							fWriteBuffer[j] = 0;
						switch (j)
						{
							case 0: fWriteBuffer[j] /= CH1_GAIN_VALUE; break;
							case 1: fWriteBuffer[j] /= CH2_GAIN_VALUE; break;
							case 2: fWriteBuffer[j] /= CH3_GAIN_VALUE; break;
							case 3: fWriteBuffer[j] /= CH4_GAIN_VALUE; break;
							case 4: fWriteBuffer[j] /= CH5_GAIN_VALUE; break;
							case 5: fWriteBuffer[j] /= CH6_GAIN_VALUE; break;
							case 6: fWriteBuffer[j] /= CH7_GAIN_VALUE; break;
							case 7: fWriteBuffer[j] /= CH8_GAIN_VALUE; break;
							default: break;
						}
					}
                    if (hdf5device != null) hdf5device.getRecord(rec).writeData<float>(fWriteBuffer);
					counter++;
				}
			}
        

			// Write attributes
            if (hdf5device != null)
            {
                hdf5device.getRecord(rec).writeAttribute("Manufacturer", winUsbDev.GetUsbStringDescriptor(winUsbDev.usbDevDescriptor.iManufacturer));
                hdf5device.getRecord(rec).writeAttribute("Device name", winUsbDev.GetUsbStringDescriptor(winUsbDev.usbDevDescriptor.iProduct));
                hdf5device.getRecord(rec).writeAttribute("Serial number", winUsbDev.GetUsbStringDescriptor(winUsbDev.usbDevDescriptor.iSerialNumber));
                hdf5device.getRecord(rec).writeAttribute("Number of channels", NumberOfChannels);
                hdf5device.getRecord(rec).writeAttribute("Samples / second", SamplesPerSecond);
                hdf5device.getRecord(rec).writeAttribute("Resolution [Bit]", ResolutionBits);
                hdf5device.getRecord(rec).writeAttribute("+Vref [mV]", (InputDifferantialVoltagePositive).ToString("R"));
                hdf5device.getRecord(rec).writeAttribute("-Vref [mV]", (InputDifferantialVoltageNegative).ToString("R"));
            }

			// Write sample counter to extra hdf5 record
			counter = 0;
            if (hdf5device != null) rec = hdf5device.addRecord("Sample counter", 1, SamplesPerSecond, HDF5group.DataType.UINT64, HDF5group.CompressionType.GZIP, 9);

			if (memStream != null)
			{
				memStream.Position = 0;
				while (memStream.Read(readBuffer, 0, DATALENGTH) == DATALENGTH)
				{
					counterBuffer[0] = (UInt64)(readBuffer[1] << 8 | readBuffer[2]);

					for (UInt32 i=0; i<(UInt32)(counter/(256*256)); i++)	// overflows
						counterBuffer[0] += (256*256);

                    if (hdf5device != null) hdf5device.getRecord(rec).writeData<UInt64>(counterBuffer);

					counter++;
				}
			}
		}
        */

		///  <summary>
		///  Reads data from the ADS1298 device.
		///  </summary>
		internal void ReadData()
		{
			const UInt32 maxBufferSize = 62208;				// Optimal buffer value

			UInt32 bufferSize;
			UInt32 bytesRead = 0;

			Byte[] readBuffer;
			Byte[] writeBuffer = new Byte[USB_PACKET_SIZE];
			Boolean success = false, sendSuccess = true;

			// Select a optimal buffer size (not extending the consts above)
			WinUsbDevice myWinUsb = new WinUsbDevice();

			bufferSize = (UInt32)((HR ? DR_VALUE_HR : DR_VALUE_LP) * DATALENGTH * (UInt32)(myWinUsb.pipeTimeout * 0.75) / 1000);
			bufferSize = bufferSize > maxBufferSize ? maxBufferSize : bufferSize;

			readBuffer = new Byte[bufferSize];
			memStream = new MemoryStream();

			try
			{
				winUsbDev.FlushPipe(PIPE_DATA_IN);

				// Send start-command to ADS device.
				writeBuffer[0] = CMD_START;
				success = winUsbDev.SendBulkData(PIPE_COMMAND_OUT, ref writeBuffer, (UInt32)writeBuffer.Length);       // Send start-command to ADS device.

				if (success)
				{
					do
					{
						// Read some data.
						winUsbDev.ReadBulkData(PIPE_DATA_IN, bufferSize, ref readBuffer, ref bytesRead, ref success);
						memStream.Write(readBuffer, 0, (Int32)bytesRead);
						sendSuccess &= success;
					}
					while (processingData && bytesRead != 0);
				}
				if (sendSuccess)
				{
					writeBuffer[0] = CMD_STOP;
					success = winUsbDev.SendBulkData(PIPE_COMMAND_OUT, ref writeBuffer, (UInt32)writeBuffer.Length);   // Send stop-command to ADS device.
				}
			}
			catch (ThreadAbortException)
			{
				throw;
			}

			finally
			{
				writeBuffer[0] = CMD_STOP;
				winUsbDev.SendBulkData(PIPE_COMMAND_OUT, ref writeBuffer, (UInt32)writeBuffer.Length);					// Send stop-command to ADS device.
				winUsbDev.ReadBulkData(PIPE_DATA_IN, bufferSize, ref readBuffer, ref bytesRead, ref success);		// Read the samples after we stopped the reading process.
			}
		}


        ///  <summary>
        ///  Reads data from the ADS1298 device and produces data frames to be sent live to clients.
        ///  </summary>
        internal void ReadDataLive()
        {
            const UInt32 maxBufferSize = 62208;				// Optimal buffer value
            const UInt32 maxBufferDelay = 15;               // Maximum buffer delay in milliseconds

            UInt32 bufferSize;
            UInt32 bytesRead = 0;
            UInt32 sampleFreq;

            UInt32 frameCounter = 0;                      // frame sequence number relative to the beginning of the data acquisition
            UInt32 frameDiff;                             // Difference to calculate the frames in the current package
            UInt32 nPackets = 0;                          //Debug variable to show the numbers of frames generated

            Byte[] readBuffer;
            Byte[] writeBuffer = new Byte[USB_PACKET_SIZE];
            Boolean success = false, sendSuccess = true;

            // Select a optimal buffer size (not extending the consts above)
            WinUsbDevice myWinUsb = new WinUsbDevice();
            Stopwatch myStopwatch = new Stopwatch();
            long ticksBegin, ticksEnd;
            double timeStamp, timeDelta;

            sampleFreq = (UInt32)(HR ? DR_VALUE_HR : DR_VALUE_LP);

            //bufferSize = (UInt32)((sampleFreq * DATALENGTH * (UInt32)((myWinUsb.pipeTimeout<maxBufferDelay) ? myWinUsb.pipeTimeout : maxBufferDelay )* 0.75) / 1000);
            
            //The buffer size is solely determined by the maximum buffer delay that we choose to allow. 
            bufferSize = (UInt32) (DATALENGTH * sampleFreq * maxBufferDelay / 1000);
            bufferSize = bufferSize > maxBufferSize ? maxBufferSize : bufferSize;

            readBuffer = new Byte[bufferSize];
            _packetQueue = new System.Collections.Concurrent.ConcurrentQueue<RawDataPacket>();
            RawDataPacket finalPacket;

            try
            {
                winUsbDev.FlushPipe(PIPE_DATA_IN);

                // Send start-command to ADS device.
                writeBuffer[0] = CMD_START;
                success = winUsbDev.SendBulkData(PIPE_COMMAND_OUT, ref writeBuffer, (UInt32)writeBuffer.Length);       // Send start-command to ADS device.

                if (success)
                {
                    myStopwatch.Reset();
                    myStopwatch.Start();
                    do
                    {
                        // Read some data.
                        ticksBegin = myStopwatch.ElapsedTicks;
                        winUsbDev.ReadBulkData(PIPE_DATA_IN, bufferSize, ref readBuffer, ref bytesRead, ref success);
                        ticksEnd = myStopwatch.ElapsedTicks;

                        timeStamp = (double)ticksBegin / (double)Stopwatch.Frequency;
                        timeDelta = (double)(ticksEnd - ticksBegin) / (double)Stopwatch.Frequency;

                        // Get a proper frame index for our newly read frames
                        // This frame index is 24-bit wide, so at 2000 samples/sec it will
                        // overflow and get back to 0 every 8388 seconds

                        frameDiff=0;

                        for (UInt32 i = 0; i < bytesRead; i += DATALENGTH)
                        {
                            frameCounter = frameCounter & 0x00ffffff;
                            readBuffer[i] = (Byte) ((frameCounter & 0x00ff0000) >> 16);
                            readBuffer[i+1] = (Byte) ((frameCounter & 0x0000ff00) >> 8);
                            readBuffer[i+2] = (Byte) (frameCounter & 0x000000ff);
                            frameCounter++;
                            frameDiff++;
                        }


                            //Here we can create a packet and add it to the frame collection
                            _packetQueue.Enqueue(new RawDataPacket(readBuffer, (Int32)bytesRead, sampleFreq, NumberOfChannels, DATALENGTH, frameDiff, timeStamp, timeDelta));

                        nPackets++;
                        
                        //We have read a new packet, so we make our listeners aware of it
                        OnNewPacket(EventArgs.Empty);
                        
                        sendSuccess &= success;
                    }
                    while (processingData && bytesRead != 0);

                    //Now we send a special duplicate packet indicating there will be no more packets coming
                    finalPacket=new RawDataPacket(readBuffer, (Int32)bytesRead, sampleFreq, NumberOfChannels, DATALENGTH, frameDiff,timeStamp,timeDelta);
                    finalPacket.lastPacket = true;
                    _packetQueue.Enqueue(finalPacket);

                    OnNewPacket(EventArgs.Empty);

                    //DEBUG
                    Console.Out.WriteLine("{0} packets processed.", nPackets);
                    //DEBUG/
                }
                if (sendSuccess)
                {
                    writeBuffer[0] = CMD_STOP;
                    success = winUsbDev.SendBulkData(PIPE_COMMAND_OUT, ref writeBuffer, (UInt32)writeBuffer.Length);   // Send stop-command to ADS device.
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }

            finally
            {
                writeBuffer[0] = CMD_STOP;
                winUsbDev.SendBulkData(PIPE_COMMAND_OUT, ref writeBuffer, (UInt32)writeBuffer.Length);					// Send stop-command to ADS device.
                winUsbDev.ReadBulkData(PIPE_DATA_IN, bufferSize, ref readBuffer, ref bytesRead, ref success);		// Read the samples after we stopped the reading process.
            }
        }



		///  <summary>
		///  Find the starting point of the received data
		///  </summary>
		///  
		internal Boolean FindStartingPoint(ref Int32 sampleStartPoint)
		{
			Int32 readBytes;
			Boolean foundStart = false;
			Byte[] checkBuffer = new Byte[1000 * DATALENGTH];

			Byte[] sample = new Byte[DATALENGTH];
			Byte checkByte;

			memStream.Seek(0, SeekOrigin.Begin);
			readBytes = memStream.Read(checkBuffer, 0, checkBuffer.Length);

			// Look up the start. Every sample starts with 1100
			for (sampleStartPoint = 0, foundStart = false; ((sampleStartPoint + DATALENGTH) < checkBuffer.Length) && !foundStart; sampleStartPoint++)
			{
				checkByte = (Byte)(((UInt32)checkBuffer[sampleStartPoint]) & 0xF0); // xxxx 0000
				if (checkByte == 0xC0) // 1100 0000
				{
					foundStart = true;
					for (Int32 j = sampleStartPoint; j < checkBuffer.Length; j += DATALENGTH)
					{
						checkByte = (Byte)(((UInt32)checkBuffer[j]) & 0xF0); // xxxx 0000;
						if (checkByte != 0xC0)
							foundStart = false;
					}
				}
			}
			sampleStartPoint--;

			return foundStart;
		}

		///  <summary>
		///  The PIC-Software sends after each 24 bytes (8 channels x 3 bytes) a 8-Bit CRC.
		///  Check if the CRC is OK and delete the CRC byte out of the byte array.
		///  </summary>
		///
		///  <returns> True if CRC check was OK. </returns>
		internal Boolean CheckCRC(ref Int32 sampleStartPoint, ref Int32 CRCerrors)
		{
			Int32 readBytes, validStatus;
			Boolean foundStart = false;
			Byte[] checkBuffer = new Byte[100 * DATALENGTH];

			Boolean CRC_Check = false;

			Byte[] data = new Byte[DATALENGTH - 1];
			Byte[] sample = new Byte[DATALENGTH];
			Byte checkByte;

			CRC8 crc = new CRC8(CRC8_POLY.CRC8_CCITT);
			crc.init_val = 0xFF;

			memStream.Seek(0, SeekOrigin.Begin);
			readBytes = memStream.Read(checkBuffer, 0, checkBuffer.Length);

			ReadConfiguration();

			// Look up the start. Every sample starts with 1100
			for (sampleStartPoint = 0, foundStart = false, validStatus = 10; ((sampleStartPoint + validStatus * DATALENGTH) < checkBuffer.Length) && !foundStart; sampleStartPoint++)
			{
				checkByte = (Byte)(((UInt32)checkBuffer[sampleStartPoint]) & 0xF0); // xxxx 0000
				if (checkByte == 0xC0) // 1100 0000
				{
					foundStart = true;
					for (Int32 j = sampleStartPoint; j < checkBuffer.Length; j += DATALENGTH)
					{
						checkByte = (Byte)(((UInt32)checkBuffer[j]) & 0xF0); // xxxx 0000;
						if (checkByte != 0xC0)
							foundStart = false;
					}
				}
			}
			sampleStartPoint--;

			if (foundStart)
			{
				CRC_Check = true;

				memStream.Seek(0, SeekOrigin.Begin);
				readBytes = memStream.Read(checkBuffer, 0, (Int32)sampleStartPoint);		// Skip bytes till starting point.


				while (memStream.Read(data, 0, (Int32)DATALENGTH) == 28)
				{
					Array.Copy(data, sample, DATALENGTH);
					if (crc.CheckSum(sample) != data[DATALENGTH])
					{
						CRC_Check = false;
						CRCerrors++;
					}
				}
			}
			return foundStart; // && CRC_Check;
		}

		///  <summary>
		///  Imports settings from file.
		///  </summary>
		internal void ImportSettings(FileInfo file)
		{
			XmlReader filereader;
			Byte[] config;
			Boolean success = false;

			config = new Byte[Marshal.SizeOf(new REGISTERMAP(0x00))];

			using (filereader = XmlReader.Create(file.FullName))
			{
				while (filereader.Read())
				{
					if (filereader.IsStartElement())
					{
						switch (filereader.Name)
						{
							case "Data":
								filereader.ReadElementContentAsBase64(config, 0, config.Length);
								success = true;
								break;
							default: break;
						}
					}
				}
			}
			if (success)
			{
				regConfig = ByteArrayToStructure(ref config);
				WriteConfiguration();
				ReadConfiguration();
			}
		}

		///  <summary>
		///  Export settings to file.
		///  </summary>
		internal void ExportSettings(FileInfo file)
		{
			XmlWriter filewriter;
			Byte[] config;

			config = StructureToByteArray(ref regConfig);

			using (filewriter = XmlWriter.Create(file.FullName))
			{
				filewriter.WriteStartDocument();
				filewriter.WriteStartElement("Device-Configuration");

				// Device info
				filewriter.WriteStartElement("Device");
				filewriter.WriteElementString("Type", "ADS129x");
				filewriter.WriteElementString("ID", DEV_ID);
				filewriter.WriteElementString("Serial-Number", "");
				filewriter.WriteEndElement();

				// Data
				filewriter.WriteStartElement("Configuration");
				filewriter.WriteElementString("Type", "RAW-Data");
				filewriter.WriteStartElement("Data");
				filewriter.WriteBase64(config, 0, config.Length);
				filewriter.WriteEndElement();
				filewriter.WriteEndElement();

				filewriter.WriteEndElement();
				filewriter.WriteEndDocument();
			}
		}

		///  <summary>
		///  Reset settings to a defined default.
		///  </summary>
		internal void ResetSettings()
		{
			regConfig = new REGISTERMAP(0x00);
			WriteConfiguration();
			ReadConfiguration();
		}

		///  <summary>
		///  Reset settings to a defined default.
		///  </summary>
		internal void RefreshSettings()
		{
			ReadConfiguration();
		}

		///  <summary>
		///  Save settings to device.
		///  </summary>
		internal void SaveSettings()
		{
			WriteConfiguration();
		}

		///  <summary>
		///  Convert a REGISTERMAP structure to a byte array.
		///  </summary>
		///  
		///  <param name="strRegister"> REGISTERMAP struct to convert. </param>
		///  
		///  <returns> Byte-Array. </returns>
		internal Byte[] StructureToByteArray(ref REGISTERMAP strRegister)
		{
			Int32 size;
			Byte[] byteArray;

			size = Marshal.SizeOf(strRegister);
			byteArray = new Byte[size];

			IntPtr ptr = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(strRegister, ptr, true);
			Marshal.Copy(ptr, byteArray, 0, size);
			Marshal.FreeHGlobal(ptr);

			return byteArray;
		}

		///  <summary>
		///  Convert a byte array to a REGISTERMAP structure.
		///  </summary>
		///  
		///  <param name="byteArray"> Buffer to convert. </param>
		///  
		///  <returns> REGISTERMAP structure. </returns>
		internal REGISTERMAP ByteArrayToStructure(ref Byte[] byteArray)
		{
			Int32 size;
			REGISTERMAP strRegister = new REGISTERMAP(0);
			IntPtr ptr;

			size = Marshal.SizeOf(strRegister);
			ptr = Marshal.AllocHGlobal(size);

			Marshal.Copy(byteArray, 0, ptr, size);
			strRegister = (REGISTERMAP)Marshal.PtrToStructure(ptr, strRegister.GetType());
			Marshal.FreeHGlobal(ptr);

			return strRegister;
		}
	}
}
