using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace EPdevice
{
	sealed public partial class ADS1298device
	{
		// Defined ADS1298 commands.
		internal const Byte CMD_START = 0x01;
		internal const Byte CMD_STOP = 0x02;
		internal const Byte CMD_SET_CONFIG = 0x11;
		internal const Byte CMD_GET_CONFIG = 0x12;

		// Pipe-ID for commands & data.
		internal const Byte PIPE_COMMAND_IN = 0x81;
		internal const Byte PIPE_COMMAND_OUT = 0x01;
		internal const Byte PIPE_DATA_IN = 0x82;

		// USB Packet Size
		internal const Byte USB_PACKET_SIZE = 64;

		// Register configuration.
		internal REGISTERMAP regConfig = new REGISTERMAP(0);

		// Threads.
		Thread DataReader;

		// Some device status.
		internal Boolean deviceConfigured { get; set; }			// True if the device configuration was written, re-read and validated.
		internal Boolean processingData { get; set; }			// Set if device is currently processing data over USB.
		internal Boolean processingSaving { get; set; }

		// Received data configurations.
		internal MemoryStream memStream;                              //For saving offline data
        internal ConcurrentQueue<RawDataPacket> _packetQueue;         //Queue of frame packets to be sent to clients in realtime
		internal const Byte SAMPLELENGTH = 3;                         // The sample consists of 3 bytes.
		internal const Byte DATALENGTH = 27;                          // 3 status bytes + 8 channels each 3 bytes.
		internal const Byte DATALENGTHCRC = 28;                       // Data length plus one byte for CRC.

        public ConcurrentQueue<RawDataPacket> packetQueue
        {
            get
            {
                return _packetQueue;
            }
        }

		// ADS specific data.
		internal const Byte RESOLUTION = 24;                                                // 24 Bit resolution
		internal const Int16 INPUT_DIFFERANTIAL_VOLTAGE_POSITIVE = 1200;
		internal const Int16 INPUT_DIFFERANTIAL_VOLTAGE_NEGATIVE = -1200;						  // Differential input voltage.

		// ADS1298 registers.
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct REGISTERMAP
		{
			// Device settings
			internal Byte ID;

			// Global settings accross channels
			internal Byte CONFIG1;
			internal Byte CONFIG2;
			internal Byte CONFIG3;
			internal Byte LOFF;

			// Channel-specific settings
			internal Byte CH1SET;
			internal Byte CH2SET;
			internal Byte CH3SET;
			internal Byte CH4SET;
			internal Byte CH5SET;
			internal Byte CH6SET;
			internal Byte CH7SET;
			internal Byte CH8SET;
			internal Byte RLD_SENSP;
			internal Byte RLD_SENSN;
			internal Byte LOFF_SENSP;
			internal Byte LOFF_SENSN;
			internal Byte LOFF_FLIP;

			// Lead-off status register
			internal Byte LOFF_STATP;
			internal Byte LOFF_STATN;

			// GPIO and OTHER registers
			internal Byte GPIO;
			internal Byte PACE;
			internal Byte RESP;
			internal Byte CONFIG4;
			internal Byte WCT1;
			internal Byte WCT2;

			// Constructor
			internal REGISTERMAP(Byte dummy = 0x00)
			{
				ID = 0x00; CONFIG1 = 0x86; CONFIG2 = 0x13; CONFIG3 = 0xCC; LOFF = 0x00;
				CH1SET = 0x00; CH2SET = 0x00; CH3SET = 0x00; CH4SET = 0x00;
				CH5SET = 0x00; CH6SET = 0x00; CH7SET = 0x00; CH8SET = 0x00;
				RLD_SENSP = 0x00; RLD_SENSN = 0x00; LOFF_SENSP = 0x00; LOFF_SENSN = 0x00; LOFF_FLIP = 0x00;
				LOFF_STATP = 0x00; LOFF_STATN = 0x00;
				GPIO = 0x0F; PACE = 0x00; RESP = 0x20; CONFIG4 = 0x00; WCT1 = 0x00; WCT2 = 0x00;
			}
		}

		// Get-set methods for in-code changes.
		#region Apply device settings
		internal String DEV_ID
		{
			get
			{
				String ID;
				switch ((Byte)(((UInt32)regConfig.ID) & 0x07))
				{
					case 0x00: ID = "ADS1294"; break;
					case 0x01: ID = "ADS1296"; break;
					case 0x02: ID = "ADS1298"; break;
					default: ID = "ADS129x"; break;
				}
				switch ((Byte)((((UInt32)regConfig.ID) >> 5) & 0x07))
				{
					case 0x04: break;
					case 0x06: ID += "R"; break;
					default: ID += "y"; break;
				}
				return ID;
			}
		}
		internal Boolean HR
		{
			get { return Convert.ToBoolean((((UInt32)regConfig.CONFIG1) >> 7) & 0x01); }
			set
			{
				regConfig.CONFIG1 &= 0x7F;    // 0xxx xxxx
				regConfig.CONFIG1 |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("HR");
			}
		}
		internal Byte DR
		{
			get { return (Byte)(((UInt32)regConfig.CONFIG1) & 0x07); }
			set
			{
				regConfig.CONFIG1 &= 0xF8;   // xxx x000
				regConfig.CONFIG1 |= (Byte)((UInt32)value & 0x07);
				OnPropertyChanged("DR");
			}
		}
		internal UInt32 DR_VALUE_HR
		{
			get
			{
				UInt32 Samples;
				switch ((Byte)(((UInt32)regConfig.CONFIG1) & 0x07))
				{
					case 0x00: Samples = 32000; break;
					case 0x01: Samples = 16000; break;
					case 0x02: Samples = 8000; break;
					case 0x03: Samples = 4000; break;
					case 0x04: Samples = 2000; break;
					case 0x05: Samples = 1000; break;
					case 0x06: Samples = 500; break;
					default: Samples = 0; break;
				}
				return Samples;
			}
			set
			{
				Byte RawValue;
				switch (value)
				{
					case 32000: RawValue = 0x00; break;
					case 16000: RawValue = 0x01; break;
					case 8000: RawValue = 0x02; break;
					case 4000: RawValue = 0x03; break;
					case 2000: RawValue = 0x04; break;
					case 1000: RawValue = 0x05; break;
					case 500: RawValue = 0x06; break;
					default: RawValue = 0x06; break;
				}
				regConfig.CONFIG1 &= 0xF8;                           // xxx x000
				regConfig.CONFIG1 |= (Byte)((UInt32)RawValue & 0x07);
				OnPropertyChanged("DR_VALUE_EN");
			}
		}
		internal UInt32 DR_VALUE_LP
		{
			get
			{
				UInt32 Samples;
				switch ((Byte)(((UInt32)regConfig.CONFIG1) & 0x07))
				{
					case 0x00: Samples = 16000; break;
					case 0x01: Samples = 8000; break;
					case 0x02: Samples = 4000; break;
					case 0x03: Samples = 2000; break;
					case 0x04: Samples = 1000; break;
					case 0x05: Samples = 500; break;
					case 0x06: Samples = 250; break;
					default: Samples = 0; break;
				}
				return Samples;
			}
			set
			{
				Byte RawValue;
				switch (value)
				{
					case 16000: RawValue = 0x00; break;
					case 8000: RawValue = 0x01; break;
					case 4000: RawValue = 0x02; break;
					case 2000: RawValue = 0x03; break;
					case 1000: RawValue = 0x04; break;
					case 500: RawValue = 0x05; break;
					case 250: RawValue = 0x06; break;
					default: RawValue = 0x06; break;
				}
				regConfig.CONFIG1 &= 0xF8;                           // xxx x000
				regConfig.CONFIG1 |= (Byte)((UInt32)RawValue & 0x07);
				OnPropertyChanged("DR_VALUE_LP");
			}
		}
		internal Byte GAIN
		{
			set
			{
				CH1_GAIN = value; CH2_GAIN = value; CH3_GAIN = value; CH4_GAIN = value;
				CH5_GAIN = value; CH6_GAIN = value; CH7_GAIN = value; CH8_GAIN = value;
			}
		}
		internal UInt32 GAIN_VALUE
		{
			set
			{
				CH1_GAIN_VALUE = value; CH2_GAIN_VALUE = value; CH3_GAIN_VALUE = value; CH4_GAIN_VALUE = value;
				CH5_GAIN_VALUE = value; CH6_GAIN_VALUE = value; CH7_GAIN_VALUE = value; CH8_GAIN_VALUE = value;
			}
		}
		#endregion
		#region Apply specific channel settings
		private Byte CH1_GAIN
		{
			get { return (Byte)((((UInt32)regConfig.CH1SET) >> 4) & 0x07); }
			set
			{
				regConfig.CH1SET &= 0x8F;   // x000 xxx
				regConfig.CH1SET |= (Byte)(((UInt32)value & 0x07) << 4);
				OnPropertyChanged("CH1_GAIN");
			}
		}
		private UInt32 CH1_GAIN_VALUE
		{
			get
			{
				UInt32 Gain;
				switch ((Byte)((((UInt32)regConfig.CH1SET) >> 4) & 0x07))
				{
					case 0x00: Gain = 6; break;
					case 0x01: Gain = 1; break;
					case 0x02: Gain = 2; break;
					case 0x03: Gain = 3; break;
					case 0x04: Gain = 4; break;
					case 0x05: Gain = 8; break;
					case 0x06: Gain = 12; break;
					default: Gain = 0; break;
				}
				return Gain;
			}
			set
			{
				Byte RawValue;
				switch (value)
				{
					case 6: RawValue = 0x00; break;
					case 1: RawValue = 0x01; break;
					case 2: RawValue = 0x02; break;
					case 3: RawValue = 0x03; break;
					case 4: RawValue = 0x04; break;
					case 8: RawValue = 0x05; break;
					case 12: RawValue = 0x06; break;
					default: RawValue = 0x00; break;
				}
				regConfig.CH1SET &= 0x8F;   // x000 xxx
				regConfig.CH1SET |= (Byte)(((UInt32)RawValue & 0x07) << 4);
				OnPropertyChanged("CH1_GAIN_VALUE");
			}
		}
		private Byte CH2_GAIN
		{
			get { return (Byte)((((UInt32)regConfig.CH2SET) >> 4) & 0x07); }
			set
			{
				regConfig.CH2SET &= 0x8F;   // x000 xxx
				regConfig.CH2SET |= (Byte)(((UInt32)value & 0x07) << 4);
				OnPropertyChanged("CH2_GAIN");
			}
		}
		private UInt32 CH2_GAIN_VALUE
		{
			get
			{
				UInt32 Gain;
				switch ((Byte)((((UInt32)regConfig.CH2SET) >> 4) & 0x07))
				{
					case 0x00: Gain = 6; break;
					case 0x01: Gain = 1; break;
					case 0x02: Gain = 2; break;
					case 0x03: Gain = 3; break;
					case 0x04: Gain = 4; break;
					case 0x05: Gain = 8; break;
					case 0x06: Gain = 12; break;
					default: Gain = 0; break;
				}
				return Gain;
			}
			set
			{
				Byte RawValue;
				switch (value)
				{
					case 6: RawValue = 0x00; break;
					case 1: RawValue = 0x01; break;
					case 2: RawValue = 0x02; break;
					case 3: RawValue = 0x03; break;
					case 4: RawValue = 0x04; break;
					case 8: RawValue = 0x05; break;
					case 12: RawValue = 0x06; break;
					default: RawValue = 0x00; break;
				}
				regConfig.CH2SET &= 0x8F;   // x000 xxx
				regConfig.CH2SET |= (Byte)(((UInt32)RawValue & 0x07) << 4);
				OnPropertyChanged("CH2_GAIN_VALUE");
			}
		}
		private Byte CH3_GAIN
		{
			get { return (Byte)((((UInt32)regConfig.CH3SET) >> 4) & 0x07); }
			set
			{
				regConfig.CH3SET &= 0x8F;   // x000 xxx
				regConfig.CH3SET |= (Byte)(((UInt32)value & 0x07) << 4);
				OnPropertyChanged("CH3_GAIN");
			}
		}
		private UInt32 CH3_GAIN_VALUE
		{
			get
			{
				UInt32 Gain;
				switch ((Byte)((((UInt32)regConfig.CH3SET) >> 4) & 0x07))
				{
					case 0x00: Gain = 6; break;
					case 0x01: Gain = 1; break;
					case 0x02: Gain = 2; break;
					case 0x03: Gain = 3; break;
					case 0x04: Gain = 4; break;
					case 0x05: Gain = 8; break;
					case 0x06: Gain = 12; break;
					default: Gain = 0; break;
				}
				return Gain;
			}
			set
			{
				Byte RawValue;
				switch (value)
				{
					case 6: RawValue = 0x00; break;
					case 1: RawValue = 0x01; break;
					case 2: RawValue = 0x02; break;
					case 3: RawValue = 0x03; break;
					case 4: RawValue = 0x04; break;
					case 8: RawValue = 0x05; break;
					case 12: RawValue = 0x06; break;
					default: RawValue = 0x00; break;
				}
				regConfig.CH3SET &= 0x8F;   // x000 xxx
				regConfig.CH3SET |= (Byte)(((UInt32)RawValue & 0x07) << 4);
				OnPropertyChanged("CH3_GAIN_VALUE");
			}
		}
		private Byte CH4_GAIN
		{
			get { return (Byte)((((UInt32)regConfig.CH4SET) >> 4) & 0x07); }
			set
			{
				regConfig.CH4SET &= 0x8F;   // x000 xxx
				regConfig.CH4SET |= (Byte)(((UInt32)value & 0x07) << 4);
				OnPropertyChanged("CH4_GAIN");
			}
		}
		private UInt32 CH4_GAIN_VALUE
		{
			get
			{
				UInt32 Gain;
				switch ((Byte)((((UInt32)regConfig.CH4SET) >> 4) & 0x07))
				{
					case 0x00: Gain = 6; break;
					case 0x01: Gain = 1; break;
					case 0x02: Gain = 2; break;
					case 0x03: Gain = 3; break;
					case 0x04: Gain = 4; break;
					case 0x05: Gain = 8; break;
					case 0x06: Gain = 12; break;
					default: Gain = 0; break;
				}
				return Gain;
			}
			set
			{
				Byte RawValue;
				switch (value)
				{
					case 6: RawValue = 0x00; break;
					case 1: RawValue = 0x01; break;
					case 2: RawValue = 0x02; break;
					case 3: RawValue = 0x03; break;
					case 4: RawValue = 0x04; break;
					case 8: RawValue = 0x05; break;
					case 12: RawValue = 0x06; break;
					default: RawValue = 0x00; break;
				}
				regConfig.CH4SET &= 0x8F;   // x000 xxx
				regConfig.CH4SET |= (Byte)(((UInt32)RawValue & 0x07) << 4);
				OnPropertyChanged("CH4_GAIN_VALUE");
			}
		}
		private Byte CH5_GAIN
		{
			get { return (Byte)((((UInt32)regConfig.CH5SET) >> 4) & 0x07); }
			set
			{
				regConfig.CH5SET &= 0x8F;   // x000 xxx
				regConfig.CH5SET |= (Byte)(((UInt32)value & 0x07) << 4);
				OnPropertyChanged("CH5_GAIN");
			}
		}
		private UInt32 CH5_GAIN_VALUE
		{
			get
			{
				UInt32 Gain;
				switch ((Byte)((((UInt32)regConfig.CH5SET) >> 4) & 0x07))
				{
					case 0x00: Gain = 6; break;
					case 0x01: Gain = 1; break;
					case 0x02: Gain = 2; break;
					case 0x03: Gain = 3; break;
					case 0x04: Gain = 4; break;
					case 0x05: Gain = 8; break;
					case 0x06: Gain = 12; break;
					default: Gain = 0; break;
				}
				return Gain;
			}
			set
			{
				Byte RawValue;
				switch (value)
				{
					case 6: RawValue = 0x00; break;
					case 1: RawValue = 0x01; break;
					case 2: RawValue = 0x02; break;
					case 3: RawValue = 0x03; break;
					case 4: RawValue = 0x04; break;
					case 8: RawValue = 0x05; break;
					case 12: RawValue = 0x06; break;
					default: RawValue = 0x00; break;
				}
				regConfig.CH5SET &= 0x8F;   // x000 xxx
				regConfig.CH5SET |= (Byte)(((UInt32)RawValue & 0x07) << 4);
				OnPropertyChanged("CH5_GAIN_VALUE");
			}
		}
		private Byte CH6_GAIN
		{
			get { return (Byte)((((UInt32)regConfig.CH6SET) >> 4) & 0x07); }
			set
			{
				regConfig.CH6SET &= 0x8F;   // x000 xxx
				regConfig.CH6SET |= (Byte)(((UInt32)value & 0x07) << 4);
				OnPropertyChanged("CH6_GAIN");
			}
		}
		private UInt32 CH6_GAIN_VALUE
		{
			get
			{
				UInt32 Gain;
				switch ((Byte)((((UInt32)regConfig.CH6SET) >> 4) & 0x07))
				{
					case 0x00: Gain = 6; break;
					case 0x01: Gain = 1; break;
					case 0x02: Gain = 2; break;
					case 0x03: Gain = 3; break;
					case 0x04: Gain = 4; break;
					case 0x05: Gain = 8; break;
					case 0x06: Gain = 12; break;
					default: Gain = 0; break;
				}
				return Gain;
			}
			set
			{
				Byte RawValue;
				switch (value)
				{
					case 6: RawValue = 0x00; break;
					case 1: RawValue = 0x01; break;
					case 2: RawValue = 0x02; break;
					case 3: RawValue = 0x03; break;
					case 4: RawValue = 0x04; break;
					case 8: RawValue = 0x05; break;
					case 12: RawValue = 0x06; break;
					default: RawValue = 0x00; break;
				}
				regConfig.CH6SET &= 0x8F;   // x000 xxx
				regConfig.CH6SET |= (Byte)(((UInt32)RawValue & 0x07) << 4);
				OnPropertyChanged("CH6_GAIN_VALUE");
			}
		}
		private Byte CH7_GAIN
		{
			get { return (Byte)((((UInt32)regConfig.CH7SET) >> 4) & 0x07); }
			set
			{
				regConfig.CH7SET &= 0x8F;   // x000 xxx
				regConfig.CH7SET |= (Byte)(((UInt32)value & 0x07) << 4);
				OnPropertyChanged("CH7_GAIN");
			}
		}
		private UInt32 CH7_GAIN_VALUE
		{
			get
			{
				UInt32 Gain;
				switch ((Byte)((((UInt32)regConfig.CH7SET) >> 4) & 0x07))
				{
					case 0x00: Gain = 6; break;
					case 0x01: Gain = 1; break;
					case 0x02: Gain = 2; break;
					case 0x03: Gain = 3; break;
					case 0x04: Gain = 4; break;
					case 0x05: Gain = 8; break;
					case 0x06: Gain = 12; break;
					default: Gain = 0; break;
				}
				return Gain;
			}
			set
			{
				Byte RawValue;
				switch (value)
				{
					case 6: RawValue = 0x00; break;
					case 1: RawValue = 0x01; break;
					case 2: RawValue = 0x02; break;
					case 3: RawValue = 0x03; break;
					case 4: RawValue = 0x04; break;
					case 8: RawValue = 0x05; break;
					case 12: RawValue = 0x06; break;
					default: RawValue = 0x00; break;
				}
				regConfig.CH7SET &= 0x8F;   // x000 xxx
				regConfig.CH7SET |= (Byte)(((UInt32)RawValue & 0x07) << 4);
				OnPropertyChanged("CH7_GAIN_VALUE");
			}
		}
		private Byte CH8_GAIN
		{
			get { return (Byte)((((UInt32)regConfig.CH8SET) >> 4) & 0x07); }
			set
			{
				regConfig.CH8SET &= 0x8F;   // x000 xxx
				regConfig.CH8SET |= (Byte)(((UInt32)value & 0x07) << 4);
				OnPropertyChanged("CH8_GAIN");
			}
		}
		private UInt32 CH8_GAIN_VALUE
		{
			get
			{
				UInt32 Gain;
				switch ((Byte)((((UInt32)regConfig.CH8SET) >> 4) & 0x07))
				{
					case 0x00: Gain = 6; break;
					case 0x01: Gain = 1; break;
					case 0x02: Gain = 2; break;
					case 0x03: Gain = 3; break;
					case 0x04: Gain = 4; break;
					case 0x05: Gain = 8; break;
					case 0x06: Gain = 12; break;
					default: Gain = 0; break;
				}
				return Gain;
			}
			set
			{
				Byte RawValue;
				switch (value)
				{
					case 6: RawValue = 0x00; break;
					case 1: RawValue = 0x01; break;
					case 2: RawValue = 0x02; break;
					case 3: RawValue = 0x03; break;
					case 4: RawValue = 0x04; break;
					case 8: RawValue = 0x05; break;
					case 12: RawValue = 0x06; break;
					default: RawValue = 0x00; break;
				}
				regConfig.CH8SET &= 0x8F;   // x000 xxx
				regConfig.CH8SET |= (Byte)(((UInt32)RawValue & 0x07) << 4);
				OnPropertyChanged("CH8_GAIN_VALUE");
			}
		}
		#endregion
	}
}
