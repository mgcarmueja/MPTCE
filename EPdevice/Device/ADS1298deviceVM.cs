using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;

namespace EPdevice
{
	sealed public class ADS1298deviceVM : GENdeviceVM, INotifyPropertyChanged
	{
		private ADS1298device model;
		private ICommand importSettingsCommand;
		private ICommand exportSettingsCommand;
		private ICommand resetSettingsCommand;
		private ICommand refreshSettingsCommand;
		private ICommand saveSettingsCommand;

		///  <summary>
		///  Constructor.
		///  </summary>
		public ADS1298deviceVM(ADS1298device model)
			: base()
		{
			this.model = model;
			importSettingsCommand = new RelayCommand(ImportSettings) { IsEnabled = true };
			exportSettingsCommand = new RelayCommand(ExportSettings) { IsEnabled = true };
			resetSettingsCommand = new RelayCommand(ResetSettings) { IsEnabled = true };
			refreshSettingsCommand = new RelayCommand(RefreshSettings) { IsEnabled = true };
			saveSettingsCommand = new RelayCommand(SaveSettings) { IsEnabled = true };

			OnPropertyChanged(String.Empty);

			// Catch settings from the ADS1298 class.
			this.model.PropertyChanged += (sender, e) =>
			{
				OnPropertyChanged(e.PropertyName);
			};
		}

		#region Commands
		///  <summary>
		///  Prepare the settings import.
		///  </summary>
		private void ImportSettings()
		{
			FileInfo file;
			Microsoft.Win32.OpenFileDialog dlg;

			dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.DefaultExt = ".xml";
			dlg.Filter = "XML documents (*.xml)|*.xml";

			if (dlg.ShowDialog() == true)
			{
				file = new FileInfo(dlg.FileName);
				model.ImportSettings(file);
			}
		}

		///  <summary>
		///  Prepare the settings export.
		///  </summary>
		private void ExportSettings()
		{
			FileInfo file;
			Microsoft.Win32.SaveFileDialog dlg;

			dlg = new Microsoft.Win32.SaveFileDialog();
			dlg.DefaultExt = ".xml";
			dlg.Filter = "XML documents (*.xml)|*.xml";

			if (dlg.ShowDialog() == true)
			{
				file = new FileInfo(dlg.FileName);
				model.ExportSettings(file);
			}
		}

		///  <summary>
		///  Prepare the reset settings.
		///  </summary>
		private void ResetSettings()
		{
			model.ResetSettings();
			OnPropertyChanged(String.Empty);
		}
		///  <summary>
		///  Prepare the refresh option.
		///  </summary>
		private void RefreshSettings()
		{
			model.RefreshSettings();
			OnPropertyChanged(String.Empty);
		}
		///  <summary>
		///  Prepare the save settings.
		///  </summary>
		private void SaveSettings()
		{
			model.SaveSettings();
		}
		#endregion

		#region ICommands
		///  <summary>
		///  Import settings from file command.
		///  </summary>
		public ICommand ImportSettingsCommand
		{
			get { return importSettingsCommand; }
			set { importSettingsCommand = value; }
		}

		///  <summary>
		///  Export settings to file command.
		///  </summary>
		public ICommand ExportSettingsCommand
		{
			get { return exportSettingsCommand; }
			set { exportSettingsCommand = value; }
		}

		///  <summary>
		///  Reset settings to a defined default command.
		///  </summary>
		public ICommand ResetSettingsCommand
		{
			get { return resetSettingsCommand; }
			set { resetSettingsCommand = value; }
		}

		///  <summary>
		///  Refresh settings from the device command.
		///  </summary>
		public ICommand RefreshSettingsCommand
		{
			get { return refreshSettingsCommand; }
			set { refreshSettingsCommand = value; }
		}

		///  <summary>
		///  Save settings to the device command.
		///  </summary>
		public ICommand SaveSettingsCommand
		{
			get { return saveSettingsCommand; }
			set { saveSettingsCommand = value; }
		}
		#endregion


		#region Device attributes
		///  <summary>
		///  Get/set method for device path.
		///  </summary>
		public String EPdeviceType
		{
			get { return model.EPdeviceType; }
			set { model.EPdeviceType = value; }
		}

		///  <summary>
		///  Get/set method for device guid.
		///  </summary>
		public Guid DeviceClassGuid
		{
			get { return model.DeviceClassGuid; }
			set { model.DeviceClassGuid = value; }
		}

		///  <summary>
		///  Get/set method for the device vendor id.
		///  </summary>
		public UInt16 VendorID
		{
			get { return model.VendorID; }
			set { model.VendorID = value; }
		}

		///  <summary>
		///  Get/set method for the device product id.
		///  </summary>
		public UInt16 ProductID
		{
			get { return model.ProductID; }
			set { model.ProductID = value; }
		}

		///  <summary>
		///  Get/set method for device path.
		///  </summary>
		public String DevicePath
		{
			get { return model.DevicePath; }
			set { model.DevicePath = value; }
		}

		///  <summary>
		///  Get/set method for device found status.
		///  </summary>
		public Boolean DeviceFound
		{
			get { return model.DeviceFound; }
			set { model.DeviceFound = value; }
		}

		///  <summary>
		///  Get/set method for device attached status. This means it is fully working.
		///  </summary>
		public Boolean DeviceAttached
		{
			get { return model.DeviceAttached; }
			set { model.DeviceAttached = value; }
		}

        ///  <summary>
        ///  Get/set method for the flagh indicating whether the device will serve live data frames when recording or not.
        ///  </summary>
        public Boolean LiveData 
        {
            get { return model.liveData; }
            set { model.liveData= value; }
        }


		///  <summary>
		///  Get/set method for the device name.
		///  </summary>
		public String DeviceName
		{
			get { return model.DeviceName; }
			set { model.DeviceName = value; }
		}

		///  <summary>
		///  Get/set method for device samples per second (approximation).
		///  </summary>
		public UInt16 SampleLength
		{
			get { return model.SampleLength; }
			set { model.SampleLength = value; }
		}

		///  <summary>
		///  Get/set method for device number of channels.
		///  </summary>
		public UInt16 NumberOfChannels
		{
			get { return model.NumberOfChannels; }
			set { model.NumberOfChannels = value; }
		}

		///  <summary>
		///  Get/set method for device samples per second (approximation).
		///  </summary>
		public UInt16 SamplesPerSecond
		{
			get { return model.SamplesPerSecond; }
			set { model.SamplesPerSecond = value; }
		}

		///  <summary>
		///  Get/set method for device resolution.
		///  </summary>
		public UInt16 ResolutionBits
		{
			get { return model.ResolutionBits; }
			set { model.ResolutionBits = value; }
		}

		///  <summary>
		///  Get/set method for device positive input voltage.
		///  </summary>
		public float InputDifferantialVoltagePositive
		{
			get { return model.InputDifferantialVoltagePositive; }
			set { model.InputDifferantialVoltagePositive = value; }
		}

		///  <summary>
		///  Get/set method for device negative input voltage.
		///  </summary>
		public float InputDifferantialVoltageNegative
		{
			get { return model.InputDifferantialVoltageNegative; }
			set { model.InputDifferantialVoltageNegative = value; }
		}
		#endregion

		// Methods for the Registers
		#region Helpers
		private UInt32[] gain_list = { 6, 1, 2, 3, 4, 8, 12 };
		public UInt32[] GAIN_LIST { get { return gain_list; } }

		private UInt32[] dr_list = { 32000, 16000, 8000, 4000, 2000, 1000, 500, 250 };
		public UInt32[] DR_LIST { get { return dr_list; } }

		private String[] test_freq_list = { "Pulsed at fCLK/2^21", "Pulsed at fCLK/2^20", "Not used", "At dc" };
		public String[] TEST_FREQ_LIST { get { return test_freq_list; } }

		private String[] mux_list = { "Normal electrode input", "Input shorted", "Used in confunction with RLD",
												 "MVDD for supply measurement", "Temperature sensor", "Test signal", "RLD_DRP", "RLD_DRN" };
		public String[] MUX_LIST { get { return mux_list; } }

		private String[] comp_th_list = { "95%", "92,5%", "90%", "87,5%", "85%", "80%", "75%", "70%" };
		public String[] COMP_TH_LIST { get { return comp_th_list; } }

		private String[] ilead_off_list = { "6nA", "12nA", "18nA", "24nA" };
		public String[] ILEAD_OFF_LIST { get { return ilead_off_list; } }

		private String[] flead_off_list = { "Default?", "AC lead-off detection at fDR/4", "Do not use", "DC lead-off detection turned on" };
		public String[] FLEAD_OFF_LIST { get { return flead_off_list; } }

		private String[] pacee_list = { "Channel 2", "Channel 4", "Channel 6", "Channel 8" };
		public String[] PACEE_LIST { get { return pacee_list; } }

		private String[] paceo_list = { "Channel 1", "Channel 3", "Channel 5", "Channel 7" };
		public String[] PACEO_LIST { get { return paceo_list; } }

		private String[] resp_ph_list = { "22,5°", "45°", "67,5°", "90°", "112,2,5°", "135°", "157,5°" };
		public String[] RESP_PH_LIST { get { return resp_ph_list; } }

		private String[] resp_ctrl_list = { "No respiration", "External respiration", "Internal resp. with internal signals", "Internal resp. with user-generated signals" };
		public String[] RESP_CTRL_LIST { get { return resp_ctrl_list; } }

		private String[] resp_freq_list = { "64kHZ modulation clock", "32kHz modulation clock", "8kHz square wave", "8kHz square wave", "4kHz square wave", "2kHz square wave", "1kHz square wave", "500Hz square wave", };
		public String[] RESP_FREQ_LIST { get { return resp_freq_list; } }

		private String[] wct_list = { "Channel 1 positive input connected", "Channel 1 negative input connected", "Channel 2 positive input connected", "Channel 2 negative input connected", "Channel 3 positive input connected", "Channel 3 negative input connected", "Channel 4 positive input connected", "Channel 4 negative input connected", };
		public String[] WCT_LIST { get { return wct_list; } }
		#endregion

		#region Device settings
		// ID Control Register (Factory-Programmed, Read-Only)
		public String DEV_ID
		{
			get
			{
				String ID;
				switch ((Byte)(((UInt32)model.regConfig.ID) & 0x07))
				{
					case 0x00: ID = "ADS1294"; break;
					case 0x01: ID = "ADS1296"; break;
					case 0x02: ID = "ADS1298"; break;
					default: ID = "ADS129x"; break;
				}
				switch ((Byte)((((UInt32)model.regConfig.ID) >> 5) & 0x07))
				{
					case 0x04: break;
					case 0x06: ID += "R"; break;
					default: ID += "y"; break;
				}
				return ID;
			}
		}
		#endregion

		#region Global settings accross channels
		// CONFIG1: Configuration register 1.
		public Boolean HR
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CONFIG1) >> 7) & 0x01); }
			set
			{
				model.regConfig.CONFIG1 &= 0x7F;    // 0xxx xxxx
				model.regConfig.CONFIG1 |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("HR");
				OnPropertyChanged("HR_TXT");
				OnPropertyChanged("DR");
				OnPropertyChanged("DR_VALUE");
			}
		}
		public String HR_TXT
		{
			get
			{
				return (HR) ? "High-resolution mode" : "Low-power mode";
			}
		}
		public Boolean DAISY_EN_N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CONFIG1) >> 6) & 0x01); }
			set
			{
				model.regConfig.CONFIG1 &= 0xBF;   // x0xx xxxx
				model.regConfig.CONFIG1 |= (Byte)(((value ? 1 : 0) & 0x01) << 6);
				OnPropertyChanged("DAISY_EN_N");
				OnPropertyChanged("DAISY_EN_N_TXT");
			}
		}
		public String DAISY_EN_N_TXT
		{
			get
			{
				return (DAISY_EN_N) ? "Multiple readback mode" : "Daisy-chain mode";
			}
		}
		public Boolean CLK_EN
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CONFIG1) >> 5) & 0x01); }
			set
			{
				model.regConfig.CONFIG1 &= 0xDF;   // xx0x xxxx
				model.regConfig.CONFIG1 |= (Byte)(((value ? 1 : 0) & 0x01) << 5);
				OnPropertyChanged("CLK_EN");
				OnPropertyChanged("CLK_EN_TXT");
			}
		}
		public String CLK_EN_TXT
		{
			get
			{
				return (CLK_EN) ? "Oscillator clock output enabled" : "Oscillator clock output disabled";
			}
		}
		public Byte DR
		{
			get { return (Byte)(((UInt32)model.regConfig.CONFIG1) & 0x07); }
			set
			{
				model.regConfig.CONFIG1 &= 0xF8;   // xxx x000
				model.regConfig.CONFIG1 |= (Byte)((UInt32)value & 0x07);
				OnPropertyChanged("DR");
				OnPropertyChanged("DR_VALUE");
			}
		}
		public UInt32 DR_VALUE
		{
			get
			{
				UInt32 Samples;
				switch ((Byte)(((UInt32)model.regConfig.CONFIG1) & 0x07))
				{
					case 0x00: Samples = (UInt32)((HR) ? 32000 : 16000); break;
					case 0x01: Samples = (UInt32)((HR) ? 16000 : 8000); break;
					case 0x02: Samples = (UInt32)((HR) ? 8000 : 4000); break;
					case 0x03: Samples = (UInt32)((HR) ? 4000 : 2000); break;
					case 0x04: Samples = (UInt32)((HR) ? 2000 : 1000); break;
					case 0x05: Samples = (UInt32)((HR) ? 1000 : 500); break;
					case 0x06: Samples = (UInt32)((HR) ? 500 : 250); break;
					default: Samples = 0; break;
				}
				return Samples;
			}
			set
			{
				Byte RawValue;
				switch (value)
				{
					case 32000: RawValue = (Byte)(((HR) ? 0x00 : 0x06)); break;
					case 16000: RawValue = (Byte)(((HR) ? 0x01 : 0x00)); break;
					case 8000: RawValue = (Byte)(((HR) ? 0x02 : 0x01)); break;
					case 4000: RawValue = (Byte)(((HR) ? 0x03 : 0x02)); break;
					case 2000: RawValue = (Byte)(((HR) ? 0x04 : 0x03)); break;
					case 1000: RawValue = (Byte)(((HR) ? 0x05 : 0x04)); break;
					case 500: RawValue = (Byte)(((HR) ? 0x06 : 0x05)); break;
					case 250: RawValue = (Byte)(((HR) ? 0x06 : 0x06)); break;
					default: RawValue = 0x06; break;
				}
				model.regConfig.CONFIG1 &= 0xF8;                           // xxx x000
				model.regConfig.CONFIG1 |= (Byte)((UInt32)RawValue & 0x07);
				OnPropertyChanged("DR");
				OnPropertyChanged("DR_VALUE");
			}
		}

		// CONFIG2: Configuration register 2.
		public Boolean WCT_CHOP
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CONFIG2) >> 5) & 0x01); }
			set
			{
				model.regConfig.CONFIG2 &= 0xDF;   // xx0x xxxx
				model.regConfig.CONFIG2 |= (Byte)(((value ? 1 : 0) & 0x01) << 5);
				OnPropertyChanged("WCT_CHOP");
				OnPropertyChanged("WCT_CHOP_TXT");
			}
		}
		public String WCT_CHOP_TXT
		{
			get
			{
				return (WCT_CHOP) ? "Chopping frequency constant at fMOD/16" : "Chopping frequency varies";
			}
		}
		public Boolean INT_TEST
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CONFIG2) >> 4) & 0x01); }
			set
			{
				model.regConfig.CONFIG2 &= 0xEF;   // xxx0 xxxx
				model.regConfig.CONFIG2 |= (Byte)(((value ? 1 : 0) & 0x01) << 4);
				OnPropertyChanged("INT_TEST");
				OnPropertyChanged("INT_TEST_TXT");
			}
		}
		public String INT_TEST_TXT
		{
			get
			{
				return (INT_TEST) ? "Test signals are generated internally" : "Test signals are driven externally";
			}
		}
		public Boolean TEST_AMP
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CONFIG2) >> 2) & 0x01); }
			set
			{
				model.regConfig.CONFIG2 &= 0xFB;   // xxxx x0xx
				model.regConfig.CONFIG2 |= (Byte)(((value ? 1 : 0) & 0x01) << 2);
				OnPropertyChanged("TEST_AMP");
				OnPropertyChanged("TEST_AMP_TXT");
			}
		}
		public String TEST_AMP_TXT
		{
			get
			{
				return (TEST_AMP) ? "2x -(VREFP - VREFN) / 2.4mV" : "1x -(VREFP - VREFN) / 2.4mV";
			}
		}
		public Byte TEST_FREQ
		{
			get { return (Byte)(((UInt32)model.regConfig.CONFIG2) & 0x03); }
			set
			{
				model.regConfig.CONFIG2 &= 0xFC;   // xxxx xx00
				model.regConfig.CONFIG2 |= (Byte)(((UInt32)value) & 0x03);
				OnPropertyChanged("TEST_FREQ");
			}
		}

		// CONFIG3: Configuration register 3.
		public Boolean PD_REFBUF_N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CONFIG3) >> 7) & 0x01); }
			set
			{
				model.regConfig.CONFIG3 &= 0x7F;   // 0xxx xxxx
				model.regConfig.CONFIG3 |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("PD_REFBUF_N");
				OnPropertyChanged("PD_REFBUF_N_TXT");
			}
		}
		public String PD_REFBUF_N_TXT
		{
			get
			{
				return (PD_REFBUF_N) ? "Internal reference buffer is enabled" : "Internal reference buffer is powered-down";
			}
		}
		public Boolean VREF_4V
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CONFIG3) >> 5) & 0x01); }
			set
			{
				model.regConfig.CONFIG3 &= 0xDF;   // xx0x xxxx
				model.regConfig.CONFIG3 |= (Byte)(((value ? 1 : 0) & 0x01) << 5);
				OnPropertyChanged("VREF_4V");
				OnPropertyChanged("VREF_4V_TXT");
			}
		}
		public String VREF_4V_TXT
		{
			get
			{
				return (VREF_4V) ? "VREFP is set to 4V" : "VREFP is set to 2.4V";
			}
		}
		public Boolean RLD_MEAS
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CONFIG3) >> 4) & 0x01); }
			set
			{
				model.regConfig.CONFIG3 &= 0xEF;   // xxx0 xxxx
				model.regConfig.CONFIG3 |= (Byte)(((value ? 1 : 0) & 0x01) << 4);
				OnPropertyChanged("RLD_MEAS");
				OnPropertyChanged("RLD_MEAS_TXT");
			}
		}
		public String RLD_MEAS_TXT
		{
			get
			{
				return (RLD_MEAS) ? "Open" : "RLD_IN signal is routed to spec. channel";
			}
		}
		public Boolean RLDREF_INT
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CONFIG3) >> 3) & 0x01); }
			set
			{
				model.regConfig.CONFIG3 &= 0xF7;   // xxxx 0xxx
				model.regConfig.CONFIG3 |= (Byte)(((value ? 1 : 0) & 0x01) << 3);
				OnPropertyChanged("RLDREF_INT");
				OnPropertyChanged("RLDREF_INT_TXT");
			}
		}
		public String RLDREF_INT_TXT
		{
			get
			{
				return (RLDREF_INT) ? "RLDREF signal (AVDD – AVSS)/2 generated internally" : "RLDREF signal fed externally";
			}
		}
		public Boolean PD_RLD_N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CONFIG3) >> 2) & 0x01); }
			set
			{
				model.regConfig.CONFIG3 &= 0xFB;   // xxxx x0xx
				model.regConfig.CONFIG3 |= (Byte)(((value ? 1 : 0) & 0x01) << 2);
				OnPropertyChanged("PD_RLD_N");
				OnPropertyChanged("PD_RLD_N_TXT");
			}
		}
		public String PD_RLD_N_TXT
		{
			get
			{
				return (PD_RLD_N) ? "RLD buffer is powered down" : "RLD buffer is enabled";
			}
		}
		public Boolean RLD_LOFF_SENS
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CONFIG3) >> 1) & 0x01); }
			set
			{
				model.regConfig.CONFIG3 &= 0xFD;   // xxxx xx0x
				model.regConfig.CONFIG3 |= (Byte)(((value ? 1 : 0) & 0x01) << 1);
				OnPropertyChanged("RLD_LOFF_SENS");
				OnPropertyChanged("RLD_LOFF_SENS_TXT");
			}
		}
		public String RLD_LOFF_SENS_TXT
		{
			get
			{
				return (RLD_LOFF_SENS) ? "RLD sense is enabled" : "RLD sense is disabled";
			}
		}
		public Boolean RLD_STAT
		{
			get { return Convert.ToBoolean(((UInt32)model.regConfig.CONFIG3) & 0x01); }
			set
			{
				model.regConfig.CONFIG3 &= 0xFE;   // xxxx xxx0
				model.regConfig.CONFIG3 |= (Byte)((value ? 1 : 0) & 0x01);
				OnPropertyChanged("RLD_STAT");
				OnPropertyChanged("RLD_STAT_TXT");
			}
		}
		public String RLD_STAT_TXT
		{
			get
			{
				return (RLD_STAT) ? "RLD is not connected" : "RLD is connected";
			}
		}

		// LOFF: Lead-Off control register.
		public Byte COMP_TH
		{
			get { return (Byte)((((UInt32)model.regConfig.LOFF) >> 5) & 0x07); }
			set
			{
				model.regConfig.LOFF &= 0x1F;   // 000x xxxx
				model.regConfig.LOFF |= (Byte)(((UInt32)value & 0x07) << 5);
				OnPropertyChanged("COMP_TH");
			}
		}
		public Boolean VLEAD_OFF_EN
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF) >> 4) & 0x01); }
			set
			{
				model.regConfig.LOFF &= 0xEF;   // xxx0 xxxx
				model.regConfig.LOFF |= (Byte)(((value ? 1 : 0) & 0x01) << 4);
				OnPropertyChanged("VLEAD_OFF_EN");
				OnPropertyChanged("VLEAD_OFF_EN_TXT");
			}
		}
		public String VLEAD_OFF_EN_TXT
		{
			get
			{
				return (VLEAD_OFF_EN) ? "Pull-up/Pull-down resister mode lead-off" : "Current source mode lead-off";
			}
		}
		public Byte ILEAD_OFF
		{
			get { return (Byte)((((UInt32)model.regConfig.LOFF) >> 2) & 0x03); }
			set
			{
				model.regConfig.LOFF &= 0xF3;   // xxxx 00xx
				model.regConfig.LOFF |= (Byte)(((UInt32)value & 0x03) << 2);
				OnPropertyChanged("ILEAD_OFF");
			}
		}
		public Byte FLEAD_OFF
		{
			get { return (Byte)(((UInt32)model.regConfig.LOFF) & 0x03); }
			set
			{
				model.regConfig.LOFF &= 0xFC;   // xxxx xx00
				model.regConfig.LOFF |= (Byte)((UInt32)value & 0x03);
				OnPropertyChanged("FLEAD_OFF");
			}
		}
		#endregion

		#region Channel settings
		// CHnSET: Global channel settings.
		public Boolean PD
		{
			set
			{
				CH1_PD = value; CH2_PD = value; CH3_PD = value; CH4_PD = value;
				CH5_PD = value; CH6_PD = value; CH7_PD = value; CH8_PD = value;
			}
		}
		public Byte GAIN
		{
			set
			{
				CH1_GAIN = value; CH2_GAIN = value; CH3_GAIN = value; CH4_GAIN = value;
				CH5_GAIN = value; CH6_GAIN = value; CH7_GAIN = value; CH8_GAIN = value;
			}
		}
		public UInt32 GAIN_VALUE
		{
			set
			{
				CH1_GAIN_VALUE = value; CH2_GAIN_VALUE = value; CH3_GAIN_VALUE = value; CH4_GAIN_VALUE = value;
				CH5_GAIN_VALUE = value; CH6_GAIN_VALUE = value; CH7_GAIN_VALUE = value; CH8_GAIN_VALUE = value;
			}
		}
		public Byte MUX
		{
			set
			{
				CH1_MUX = value; CH2_MUX = value; CH3_MUX = value; CH4_MUX = value;
				CH5_MUX = value; CH6_MUX = value; CH7_MUX = value; CH8_MUX = value;
			}
		}

		// CH1SET: Individual channel 1 settings.
		public Boolean CH1_PD
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CH1SET) >> 7) & 0x01); }
			set
			{
				model.regConfig.CH1SET &= 0x7F;   // 0xxx xxxx
				model.regConfig.CH1SET |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("CH1_PD");
				OnPropertyChanged("CH1_PD_TXT");
			}
		}
		public String CH1_PD_TXT
		{
			get
			{
				return (CH1_PD) ? "Channel is powered-down" : "Normal operation";
			}
		}
		public Byte CH1_GAIN
		{
			get { return (Byte)((((UInt32)model.regConfig.CH1SET) >> 4) & 0x07); }
			set
			{
				model.regConfig.CH1SET &= 0x8F;   // x000 xxx
				model.regConfig.CH1SET |= (Byte)(((UInt32)value & 0x07) << 4);
				OnPropertyChanged("CH1_GAIN");
			}
		}
		public UInt32 CH1_GAIN_VALUE
		{
			get
			{
				UInt32 Gain;
				switch ((Byte)((((UInt32)model.regConfig.CH1SET) >> 4) & 0x07))
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
				model.regConfig.CH1SET &= 0x8F;   // x000 xxx
				model.regConfig.CH1SET |= (Byte)(((UInt32)RawValue & 0x07) << 4);
				OnPropertyChanged("CH1_GAIN_VALUE");
			}
		}
		public Byte CH1_MUX
		{
			get { return (Byte)(((UInt32)model.regConfig.CH1SET) & 0x03); }
			set
			{
				model.regConfig.CH1SET &= 0xFC;   // xxx xx00
				model.regConfig.CH1SET |= (Byte)((UInt32)value & 0x03);
				OnPropertyChanged("CH1_MUX");
			}
		}

		// CH2SET: Individual channel 2 settings.
		public Boolean CH2_PD
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CH2SET) >> 7) & 0x01); }
			set
			{
				model.regConfig.CH2SET &= 0x7F;   // 0xxx xxxx
				model.regConfig.CH2SET |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("CH2_PD");
				OnPropertyChanged("CH2_PD_TXT");
			}
		}
		public String CH2_PD_TXT
		{
			get
			{
				return (CH2_PD) ? "Channel is powered-down" : "Normal operation";
			}
		}
		public Byte CH2_GAIN
		{
			get { return (Byte)((((UInt32)model.regConfig.CH2SET) >> 4) & 0x07); }
			set
			{
				model.regConfig.CH2SET &= 0x8F;   // x000 xxx
				model.regConfig.CH2SET |= (Byte)(((UInt32)value & 0x07) << 4);
				OnPropertyChanged("CH2_GAIN");
			}
		}
		public UInt32 CH2_GAIN_VALUE
		{
			get
			{
				UInt32 Gain;
				switch ((Byte)((((UInt32)model.regConfig.CH2SET) >> 4) & 0x07))
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
				model.regConfig.CH2SET &= 0x8F;   // x000 xxx
				model.regConfig.CH2SET |= (Byte)(((UInt32)RawValue & 0x07) << 4);
				OnPropertyChanged("CH2_GAIN_VALUE");
			}
		}
		public Byte CH2_MUX
		{
			get { return (Byte)(((UInt32)model.regConfig.CH2SET) & 0x03); }
			set
			{
				model.regConfig.CH2SET &= 0xFC;   // xxx xx00
				model.regConfig.CH2SET |= (Byte)((UInt32)value & 0x03);
				OnPropertyChanged("CH2_MUX");
			}
		}

		// CH3SET: Individual channel 3 settings.
		public Boolean CH3_PD
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CH3SET) >> 7) & 0x01); }
			set
			{
				model.regConfig.CH3SET &= 0x7F;   // 0xxx xxxx
				model.regConfig.CH3SET |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("CH3_PD");
				OnPropertyChanged("CH3_PD_TXT");
			}
		}
		public String CH3_PD_TXT
		{
			get
			{
				return (CH3_PD) ? "Channel is powered-down" : "Normal operation";
			}
		}
		public Byte CH3_GAIN
		{
			get { return (Byte)((((UInt32)model.regConfig.CH3SET) >> 4) & 0x07); }
			set
			{
				model.regConfig.CH3SET &= 0x8F;   // x000 xxx
				model.regConfig.CH3SET |= (Byte)(((UInt32)value & 0x07) << 4);
				OnPropertyChanged("CH3_GAIN");
			}
		}
		public UInt32 CH3_GAIN_VALUE
		{
			get
			{
				UInt32 Gain;
				switch ((Byte)((((UInt32)model.regConfig.CH3SET) >> 4) & 0x07))
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
				model.regConfig.CH3SET &= 0x8F;   // x000 xxx
				model.regConfig.CH3SET |= (Byte)(((UInt32)RawValue & 0x07) << 4);
				OnPropertyChanged("CH3_GAIN_VALUE");
			}
		}
		public Byte CH3_MUX
		{
			get { return (Byte)(((UInt32)model.regConfig.CH3SET) & 0x03); }
			set
			{
				model.regConfig.CH3SET &= 0xFC;   // xxx xx00
				model.regConfig.CH3SET |= (Byte)((UInt32)value & 0x03);
				OnPropertyChanged("CH3_MUX");
			}
		}

		// CH4SET: Individual channel 4 settings.
		public Boolean CH4_PD
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CH4SET) >> 7) & 0x01); }
			set
			{
				model.regConfig.CH4SET &= 0x7F;   // 0xxx xxxx
				model.regConfig.CH4SET |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("CH4_PD");
				OnPropertyChanged("CH4_PD_TXT");
			}
		}
		public String CH4_PD_TXT
		{
			get
			{
				return (CH4_PD) ? "Channel is powered-down" : "Normal operation";
			}
		}
		public Byte CH4_GAIN
		{
			get { return (Byte)((((UInt32)model.regConfig.CH4SET) >> 4) & 0x07); }
			set
			{
				model.regConfig.CH4SET &= 0x8F;   // x000 xxx
				model.regConfig.CH4SET |= (Byte)(((UInt32)value & 0x07) << 4);
				OnPropertyChanged("CH4_GAIN");
			}
		}
		public UInt32 CH4_GAIN_VALUE
		{
			get
			{
				UInt32 Gain;
				switch ((Byte)((((UInt32)model.regConfig.CH4SET) >> 4) & 0x07))
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
				model.regConfig.CH4SET &= 0x8F;   // x000 xxx
				model.regConfig.CH4SET |= (Byte)(((UInt32)RawValue & 0x07) << 4);
				OnPropertyChanged("CH4_GAIN_VALUE");
			}
		}
		public Byte CH4_MUX
		{
			get { return (Byte)(((UInt32)model.regConfig.CH4SET) & 0x03); }
			set
			{
				model.regConfig.CH4SET &= 0xFC;   // xxx xx00
				model.regConfig.CH4SET |= (Byte)((UInt32)value & 0x03);
				OnPropertyChanged("CH4_MUX");
			}
		}

		// CH5SET: Individual channel 5 settings.
		public Boolean CH5_PD
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CH5SET) >> 7) & 0x01); }
			set
			{
				model.regConfig.CH5SET &= 0x7F;   // 0xxx xxxx
				model.regConfig.CH5SET |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("CH5_PD");
				OnPropertyChanged("CH5_PD_TXT");
			}
		}
		public String CH5_PD_TXT
		{
			get
			{
				return (CH5_PD) ? "Channel is powered-down" : "Normal operation";
			}
		}
		public Byte CH5_GAIN
		{
			get { return (Byte)((((UInt32)model.regConfig.CH5SET) >> 4) & 0x07); }
			set
			{
				model.regConfig.CH5SET &= 0x8F;   // x000 xxx
				model.regConfig.CH5SET |= (Byte)(((UInt32)value & 0x07) << 4);
				OnPropertyChanged("CH5_GAIN");
			}
		}
		public UInt32 CH5_GAIN_VALUE
		{
			get
			{
				UInt32 Gain;
				switch ((Byte)((((UInt32)model.regConfig.CH5SET) >> 4) & 0x07))
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
				model.regConfig.CH5SET &= 0x8F;   // x000 xxx
				model.regConfig.CH5SET |= (Byte)(((UInt32)RawValue & 0x07) << 4);
				OnPropertyChanged("CH5_GAIN_VALUE");
			}
		}
		public Byte CH5_MUX
		{
			get { return (Byte)(((UInt32)model.regConfig.CH5SET) & 0x03); }
			set
			{
				model.regConfig.CH5SET &= 0xFC;   // xxx xx00
				model.regConfig.CH5SET |= (Byte)((UInt32)value & 0x03);
				OnPropertyChanged("CH5_MUX");
			}
		}

		// CH6SET: Individual channel 6 settings.
		public Boolean CH6_PD
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CH6SET) >> 7) & 0x01); }
			set
			{
				model.regConfig.CH6SET &= 0x7F;   // 0xxx xxxx
				model.regConfig.CH6SET |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("CH6_PD");
				OnPropertyChanged("CH6_PD_TXT");
			}
		}
		public String CH6_PD_TXT
		{
			get
			{
				return (CH6_PD) ? "Channel is powered-down" : "Normal operation";
			}
		}
		public Byte CH6_GAIN
		{
			get { return (Byte)((((UInt32)model.regConfig.CH6SET) >> 4) & 0x07); }
			set
			{
				model.regConfig.CH6SET &= 0x8F;   // x000 xxx
				model.regConfig.CH6SET |= (Byte)(((UInt32)value & 0x07) << 4);
				OnPropertyChanged("CH6_GAIN");
			}
		}
		public UInt32 CH6_GAIN_VALUE
		{
			get
			{
				UInt32 Gain;
				switch ((Byte)((((UInt32)model.regConfig.CH6SET) >> 4) & 0x07))
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
				model.regConfig.CH6SET &= 0x8F;   // x000 xxx
				model.regConfig.CH6SET |= (Byte)(((UInt32)RawValue & 0x07) << 4);
				OnPropertyChanged("CH6_GAIN_VALUE");
			}
		}
		public Byte CH6_MUX
		{
			get { return (Byte)(((UInt32)model.regConfig.CH6SET) & 0x03); }
			set
			{
				model.regConfig.CH6SET &= 0xFC;   // xxx xx00
				model.regConfig.CH6SET |= (Byte)((UInt32)value & 0x03);
				OnPropertyChanged("CH6_MUX");
			}
		}

		// CH7SET: Individuall channel 7 settings.
		public Boolean CH7_PD
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CH7SET) >> 7) & 0x01); }
			set
			{
				model.regConfig.CH7SET &= 0x7F;   // 0xxx xxxx
				model.regConfig.CH7SET |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("CH7_PD");
				OnPropertyChanged("CH7_PD_TXT");
			}
		}
		public String CH7_PD_TXT
		{
			get
			{
				return (CH7_PD) ? "Channel is powered-down" : "Normal operation";
			}
		}
		public Byte CH7_GAIN
		{
			get { return (Byte)((((UInt32)model.regConfig.CH7SET) >> 4) & 0x07); }
			set
			{
				model.regConfig.CH7SET &= 0x8F;   // x000 xxx
				model.regConfig.CH7SET |= (Byte)(((UInt32)value & 0x07) << 4);
				OnPropertyChanged("CH7_GAIN");
			}
		}
		public UInt32 CH7_GAIN_VALUE
		{
			get
			{
				UInt32 Gain;
				switch ((Byte)((((UInt32)model.regConfig.CH7SET) >> 4) & 0x07))
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
				model.regConfig.CH7SET &= 0x8F;   // x000 xxx
				model.regConfig.CH7SET |= (Byte)(((UInt32)RawValue & 0x07) << 4);
				OnPropertyChanged("CH7_GAIN_VALUE");
			}
		}
		public Byte CH7_MUX
		{
			get { return (Byte)(((UInt32)model.regConfig.CH7SET) & 0x03); }
			set
			{
				model.regConfig.CH7SET &= 0xFC;   // xxx xx00
				model.regConfig.CH7SET |= (Byte)((UInt32)value & 0x03);
				OnPropertyChanged("CH7_MUX");
			}
		}

		// CH8SET: Individual channel 8 settings.
		public Boolean CH8_PD
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CH8SET) >> 7) & 0x01); }
			set
			{
				model.regConfig.CH8SET &= 0x7F;   // 0xxx xxxx
				model.regConfig.CH8SET |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("CH8_PD");
				OnPropertyChanged("CH8_PD_TXT");
			}
		}
		public String CH8_PD_TXT
		{
			get
			{
				return (CH8_PD) ? "Channel is powered-down" : "Normal operation";
			}
		}
		public Byte CH8_GAIN
		{
			get { return (Byte)((((UInt32)model.regConfig.CH8SET) >> 4) & 0x07); }
			set
			{
				model.regConfig.CH8SET &= 0x8F;   // x000 xxx
				model.regConfig.CH8SET |= (Byte)(((UInt32)value & 0x07) << 4);
				OnPropertyChanged("CH8_GAIN");
			}
		}
		public UInt32 CH8_GAIN_VALUE
		{
			get
			{
				UInt32 Gain;
				switch ((Byte)((((UInt32)model.regConfig.CH8SET) >> 4) & 0x07))
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
				model.regConfig.CH8SET &= 0x8F;   // x000 xxx
				model.regConfig.CH8SET |= (Byte)(((UInt32)RawValue & 0x07) << 4);
				OnPropertyChanged("CH8_GAIN_VALUE");
			}
		}
		public Byte CH8_MUX
		{
			get { return (Byte)(((UInt32)model.regConfig.CH8SET) & 0x03); }
			set
			{
				model.regConfig.CH8SET &= 0xFC;   // xxx xx00
				model.regConfig.CH8SET |= (Byte)((UInt32)value & 0x03);
				OnPropertyChanged("CH8_MUX");
			}
		}

		// RLD_SENSP
		public Boolean RLD8P
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.RLD_SENSP) >> 7) & 0x01); }
			set
			{
				model.regConfig.RLD_SENSP &= 0x7F;   // 0xxx xxxx
				model.regConfig.RLD_SENSP |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("RLD8P");
			}
		}
		public Boolean RLD7P
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.RLD_SENSP) >> 6) & 0x01); }
			set
			{
				model.regConfig.RLD_SENSP &= 0xBF;   // x0xx xxxx
				model.regConfig.RLD_SENSP |= (Byte)(((value ? 1 : 0) & 0x01) << 6);
				OnPropertyChanged("RLD7P");
			}
		}
		public Boolean RLD6P
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.RLD_SENSP) >> 5) & 0x01); }
			set
			{
				model.regConfig.RLD_SENSP &= 0xDF;   // xx0x xxxx
				model.regConfig.RLD_SENSP |= (Byte)(((value ? 1 : 0) & 0x01) << 5);
				OnPropertyChanged("RLD6P");
			}
		}
		public Boolean RLD5P
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.RLD_SENSP) >> 4) & 0x01); }
			set
			{
				model.regConfig.RLD_SENSP &= 0xEF;   // xxx0 xxxx
				model.regConfig.RLD_SENSP |= (Byte)(((value ? 1 : 0) & 0x01) << 4);
				OnPropertyChanged("RLD5P");
			}
		}
		public Boolean RLD4P
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.RLD_SENSP) >> 3) & 0x01); }
			set
			{
				model.regConfig.RLD_SENSP &= 0xF7;   // xxxx 0xxx
				model.regConfig.RLD_SENSP |= (Byte)(((value ? 1 : 0) & 0x01) << 3);
				OnPropertyChanged("RLD4P");
			}
		}
		public Boolean RLD3P
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.RLD_SENSP) >> 2) & 0x01); }
			set
			{
				model.regConfig.RLD_SENSP &= 0xFB;   // xxxx x0xx
				model.regConfig.RLD_SENSP |= (Byte)(((value ? 1 : 0) & 0x01) << 2);
				OnPropertyChanged("RLD3P");
			}
		}
		public Boolean RLD2P
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.RLD_SENSP) >> 1) & 0x01); }
			set
			{
				model.regConfig.RLD_SENSP &= 0xFD;   // xxxx xx0x
				model.regConfig.RLD_SENSP |= (Byte)(((value ? 1 : 0) & 0x01) << 1);
				OnPropertyChanged("RLD2P");
			}
		}
		public Boolean RLD1P
		{
			get { return Convert.ToBoolean(((UInt32)model.regConfig.RLD_SENSP) & 0x01); }
			set
			{
				model.regConfig.RLD_SENSP &= 0xFE;   // xxxx xxx0
				model.regConfig.RLD_SENSP |= (Byte)((value ? 1 : 0) & 0x01);
				OnPropertyChanged("RLD1P");
			}
		}

		// RLD_SENSN
		public Boolean RLD8N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.RLD_SENSN) >> 7) & 0x01); }
			set
			{
				model.regConfig.RLD_SENSN &= 0x7F;   // 0xxx xxxx
				model.regConfig.RLD_SENSN |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("RLD8N");
			}
		}
		public Boolean RLD7N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.RLD_SENSN) >> 6) & 0x01); }
			set
			{
				model.regConfig.RLD_SENSN &= 0xBF;   // x0xx xxxx
				model.regConfig.RLD_SENSN |= (Byte)(((value ? 1 : 0) & 0x01) << 6);
				OnPropertyChanged("RLD7N");
			}
		}
		public Boolean RLD6N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.RLD_SENSN) >> 5) & 0x01); }
			set
			{
				model.regConfig.RLD_SENSN &= 0xDF;   // xx0x xxxx
				model.regConfig.RLD_SENSN |= (Byte)(((value ? 1 : 0) & 0x01) << 5);
				OnPropertyChanged("RLD6N");
			}
		}
		public Boolean RLD5N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.RLD_SENSN) >> 4) & 0x01); }
			set
			{
				model.regConfig.RLD_SENSN &= 0xEF;   // xxx0 xxxx
				model.regConfig.RLD_SENSN |= (Byte)(((value ? 1 : 0) & 0x01) << 4);
				OnPropertyChanged("RLD5N");
			}
		}
		public Boolean RLD4N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.RLD_SENSN) >> 3) & 0x01); }
			set
			{
				model.regConfig.RLD_SENSN &= 0xF7;   // xxxx 0xxx
				model.regConfig.RLD_SENSN |= (Byte)(((value ? 1 : 0) & 0x01) << 3);
				OnPropertyChanged("RLD4N");
			}
		}
		public Boolean RLD3N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.RLD_SENSN) >> 2) & 0x01); }
			set
			{
				model.regConfig.RLD_SENSN &= 0xFB;   // xxxx x0xx
				model.regConfig.RLD_SENSN |= (Byte)(((value ? 1 : 0) & 0x01) << 2);
				OnPropertyChanged("RLD3N");
			}
		}
		public Boolean RLD2N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.RLD_SENSN) >> 1) & 0x01); }
			set
			{
				model.regConfig.RLD_SENSN &= 0xFD;   // xxxx xx0x
				model.regConfig.RLD_SENSN |= (Byte)(((value ? 1 : 0) & 0x01) << 1);
				OnPropertyChanged("RLD2N");
			}
		}
		public Boolean RLD1N
		{
			get { return Convert.ToBoolean(((UInt32)model.regConfig.RLD_SENSN) & 0x01); }
			set
			{
				model.regConfig.RLD_SENSN &= 0xFE;   // xxxx xxx0
				model.regConfig.RLD_SENSN |= (Byte)((value ? 1 : 0) & 0x01);
				OnPropertyChanged("RLD1N");
			}
		}

		// LOFF_SENSP
		public Boolean LOFF8P
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_SENSP) >> 7) & 0x01); }
			set
			{
				model.regConfig.LOFF_SENSP &= 0x7F;   // 0xxx xxxx
				model.regConfig.LOFF_SENSP |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("LOFF8P");
			}
		}
		public Boolean LOFF7P
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_SENSP) >> 6) & 0x01); }
			set
			{
				model.regConfig.LOFF_SENSP &= 0xBF;   // x0xx xxxx
				model.regConfig.LOFF_SENSP |= (Byte)(((value ? 1 : 0) & 0x01) << 6);
				OnPropertyChanged("LOFF7P");
			}
		}
		public Boolean LOFF6P
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_SENSP) >> 5) & 0x01); }
			set
			{
				model.regConfig.LOFF_SENSP &= 0xDF;   // xx0x xxxx
				model.regConfig.LOFF_SENSP |= (Byte)(((value ? 1 : 0) & 0x01) << 5);
				OnPropertyChanged("LOFF6P");
			}
		}
		public Boolean LOFF5P
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_SENSP) >> 4) & 0x01); }
			set
			{
				model.regConfig.LOFF_SENSP &= 0xEF;   // xxx0 xxxx
				model.regConfig.LOFF_SENSP |= (Byte)(((value ? 1 : 0) & 0x01) << 4);
				OnPropertyChanged("LOFF5P");
			}
		}
		public Boolean LOFF4P
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_SENSP) >> 3) & 0x01); }
			set
			{
				model.regConfig.LOFF_SENSP &= 0xF7;   // xxxx 0xxx
				model.regConfig.LOFF_SENSP |= (Byte)(((value ? 1 : 0) & 0x01) << 3);
				OnPropertyChanged("LOFF4P");
			}
		}
		public Boolean LOFF3P
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_SENSP) >> 2) & 0x01); }
			set
			{
				model.regConfig.LOFF_SENSP &= 0xFB;   // xxxx x0xx
				model.regConfig.LOFF_SENSP |= (Byte)(((value ? 1 : 0) & 0x01) << 2);
				OnPropertyChanged("LOFF3P");
			}
		}
		public Boolean LOFF2P
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_SENSP) >> 1) & 0x01); }
			set
			{
				model.regConfig.LOFF_SENSP &= 0xFD;   // xxxx xx0x
				model.regConfig.LOFF_SENSP |= (Byte)(((value ? 1 : 0) & 0x01) << 1);
				OnPropertyChanged("LOFF2P");
			}
		}
		public Boolean LOFF1P
		{
			get { return Convert.ToBoolean(((UInt32)model.regConfig.LOFF_SENSP) & 0x01); }
			set
			{
				model.regConfig.LOFF_SENSP &= 0xFE;   // xxxx xxx0
				model.regConfig.LOFF_SENSP |= (Byte)((value ? 1 : 0) & 0x01);
				OnPropertyChanged("LOFF1P");
			}
		}

		// LOFF_SENSN
		public Boolean LOFF8N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_SENSN) >> 7) & 0x01); }
			set
			{
				model.regConfig.LOFF_SENSN &= 0x7F;   // 0xxx xxxx
				model.regConfig.LOFF_SENSN |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("LOFF8N");
			}
		}
		public Boolean LOFF7N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_SENSN) >> 6) & 0x01); }
			set
			{
				model.regConfig.LOFF_SENSN &= 0xBF;   // x0xx xxxx
				model.regConfig.LOFF_SENSN |= (Byte)(((value ? 1 : 0) & 0x01) << 6);
				OnPropertyChanged("LOFF7N");
			}
		}
		public Boolean LOFF6N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_SENSN) >> 5) & 0x01); }
			set
			{
				model.regConfig.LOFF_SENSN &= 0xDF;   // xx0x xxxx
				model.regConfig.LOFF_SENSN |= (Byte)(((value ? 1 : 0) & 0x01) << 5);
				OnPropertyChanged("LOFF6N");
			}
		}
		public Boolean LOFF5N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_SENSN) >> 4) & 0x01); }
			set
			{
				model.regConfig.LOFF_SENSN &= 0xEF;   // xxx0 xxxx
				model.regConfig.LOFF_SENSN |= (Byte)(((value ? 1 : 0) & 0x01) << 4);
				OnPropertyChanged("LOFF5N");
			}
		}
		public Boolean LOFF4N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_SENSN) >> 3) & 0x01); }
			set
			{
				model.regConfig.LOFF_SENSN &= 0xF7;   // xxxx 0xxx
				model.regConfig.LOFF_SENSN |= (Byte)(((value ? 1 : 0) & 0x01) << 3);
				OnPropertyChanged("LOFF4N");
			}
		}
		public Boolean LOFF3N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_SENSN) >> 2) & 0x01); }
			set
			{
				model.regConfig.LOFF_SENSN &= 0xFB;   // xxxx x0xx
				model.regConfig.LOFF_SENSN |= (Byte)(((value ? 1 : 0) & 0x01) << 2);
				OnPropertyChanged("LOFF3N");
			}
		}
		public Boolean LOFF2N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_SENSN) >> 1) & 0x01); }
			set
			{
				model.regConfig.LOFF_SENSN &= 0xFD;   // xxxx xx0x
				model.regConfig.LOFF_SENSN |= (Byte)(((value ? 1 : 0) & 0x01) << 1);
				OnPropertyChanged("LOFF2N");
			}
		}
		public Boolean LOFF1N
		{
			get { return Convert.ToBoolean(((UInt32)model.regConfig.LOFF_SENSN) & 0x01); }
			set
			{
				model.regConfig.LOFF_SENSN &= 0xFE;   // xxxx xxx0
				model.regConfig.LOFF_SENSN |= (Byte)((value ? 1 : 0) & 0x01);
				OnPropertyChanged("LOFF1N");
			}
		}

		// LOFF_FLIP
		public Boolean LOFF_FLIP8
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_FLIP) >> 7) & 0x01); }
			set
			{
				model.regConfig.LOFF_FLIP &= 0x7F;   // 0xxx xxxx
				model.regConfig.LOFF_FLIP |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("LOFF_FLIP8");
			}
		}
		public Boolean LOFF_FLIP7
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_FLIP) >> 6) & 0x01); }
			set
			{
				model.regConfig.LOFF_FLIP &= 0xBF;   // x0xx xxxx
				model.regConfig.LOFF_FLIP |= (Byte)(((value ? 1 : 0) & 0x01) << 6);
				OnPropertyChanged("LOFF_FLIP7");
			}
		}
		public Boolean LOFF_FLIP6
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_FLIP) >> 5) & 0x01); }
			set
			{
				model.regConfig.LOFF_FLIP &= 0xDF;   // xx0x xxxx
				model.regConfig.LOFF_FLIP |= (Byte)(((value ? 1 : 0) & 0x01) << 5);
				OnPropertyChanged("LOFF_FLIP6");
			}
		}
		public Boolean LOFF_FLIP5
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_FLIP) >> 4) & 0x01); }
			set
			{
				model.regConfig.LOFF_FLIP &= 0xEF;   // xxx0 xxxx
				model.regConfig.LOFF_FLIP |= (Byte)(((value ? 1 : 0) & 0x01) << 4);
				OnPropertyChanged("LOFF_FLIP5");
			}
		}
		public Boolean LOFF_FLIP4
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_FLIP) >> 3) & 0x01); }
			set
			{
				model.regConfig.LOFF_FLIP &= 0xF7;   // xxxx 0xxx
				model.regConfig.LOFF_FLIP |= (Byte)(((value ? 1 : 0) & 0x01) << 3);
				OnPropertyChanged("LOFF_FLIP4");
			}
		}
		public Boolean LOFF_FLIP3
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_FLIP) >> 2) & 0x01); }
			set
			{
				model.regConfig.LOFF_FLIP &= 0xFB;   // xxxx x0xx
				model.regConfig.LOFF_FLIP |= (Byte)(((value ? 1 : 0) & 0x01) << 2);
				OnPropertyChanged("LOFF_FLIP3");
			}
		}
		public Boolean LOFF_FLIP2
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_FLIP) >> 1) & 0x01); }
			set
			{
				model.regConfig.LOFF_FLIP &= 0xFD;   // xxxx xx0x
				model.regConfig.LOFF_FLIP |= (Byte)(((value ? 1 : 0) & 0x01) << 1);
				OnPropertyChanged("LOFF_FLIP2");
			}
		}
		public Boolean LOFF_FLIP1
		{
			get { return Convert.ToBoolean(((UInt32)model.regConfig.LOFF_FLIP) & 0x01); }
			set
			{
				model.regConfig.LOFF_FLIP &= 0xFE;   // xxxx xxx0
				model.regConfig.LOFF_FLIP |= (Byte)((value ? 1 : 0) & 0x01);
				OnPropertyChanged("LOFF_FLIP1");
			}
		}
		#endregion

		#region Lead-off status register
		// LOFF_STATP (Read-Only register)
		public Boolean IN8P_OFF
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_STATP) >> 7) & 0x01); }
		}
		public Boolean IN7P_OFF
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_STATP) >> 6) & 0x01); }
		}
		public Boolean IN6P_OFF
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_STATP) >> 5) & 0x01); }
		}
		public Boolean IN5P_OFF
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_STATP) >> 4) & 0x01); }
		}
		public Boolean IN4P_OFF
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_STATP) >> 3) & 0x01); }
		}
		public Boolean IN3P_OFF
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_STATP) >> 2) & 0x01); }
		}
		public Boolean IN2P_OFF
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_STATP) >> 1) & 0x01); }
		}
		public Boolean IN1P_OFF
		{
			get { return Convert.ToBoolean(((UInt32)model.regConfig.LOFF_STATP) & 0x01); }
		}

		// LOFF_STATN (Read-Only register)
		public Boolean IN8N_OFF
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_STATN) >> 7) & 0x01); }
		}
		public Boolean IN7N_OFF
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_STATN) >> 6) & 0x01); }
		}
		public Boolean IN6N_OFF
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_STATN) >> 5) & 0x01); }
		}
		public Boolean IN5N_OFF
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_STATN) >> 4) & 0x01); }
		}
		public Boolean IN4N_OFF
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_STATN) >> 3) & 0x01); }
		}
		public Boolean IN3N_OFF
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_STATN) >> 2) & 0x01); }
		}
		public Boolean IN2N_OFF
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.LOFF_STATN) >> 1) & 0x01); }
		}
		public Boolean IN1N_OFF
		{
			get { return Convert.ToBoolean(((UInt32)model.regConfig.LOFF_STATN) & 0x01); }
		}
		#endregion

		#region GPIO and OTHER registers
		// GPIO: General-Purpose I/O register.
		public Byte GPIOD
		{
			get { return (Byte)((((UInt32)model.regConfig.GPIO) >> 4) & 0x0F); }
			set
			{
				model.regConfig.GPIO &= 0x0F;   // 0000 xxxx
				model.regConfig.GPIO |= (Byte)(((UInt32)value & 0x0F) << 4);
				OnPropertyChanged("GPIOD");
			}
		}
		public Boolean GPIOD4
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.GPIO) >> 7) & 0x01); }
			set
			{
				model.regConfig.GPIO &= 0x7F;   // 0xxx xxxx
				model.regConfig.GPIO |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("GPIOD4");
				OnPropertyChanged("GPIOD4_TXT");
			}
		}
		public String GPIOD4_TXT
		{
			get
			{
				return (GPIOD4) ? "Is set" : "Is unset";
			}
		}
		public Boolean GPIOD3
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.GPIO) >> 6) & 0x01); }
			set
			{
				model.regConfig.GPIO &= 0xBF;   // x0xx xxxx
				model.regConfig.GPIO |= (Byte)(((value ? 1 : 0) & 0x01) << 6);
				OnPropertyChanged("GPIOD3");
				OnPropertyChanged("GPIOD3_TXT");
			}
		}
		public String GPIOD3_TXT
		{
			get
			{
				return (GPIOD3) ? "Is set" : "Is unset";
			}
		}
		public Boolean GPIOD2
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.GPIO) >> 5) & 0x01); }
			set
			{
				model.regConfig.GPIO &= 0xDF;   // xx0x xxxx
				model.regConfig.GPIO |= (Byte)(((value ? 1 : 0) & 0x01) << 5);
				OnPropertyChanged("GPIOD2");
				OnPropertyChanged("GPIOD2_TXT");
			}
		}
		public String GPIOD2_TXT
		{
			get
			{
				return (GPIOD2) ? "Is set" : "Is unset";
			}
		}
		public Boolean GPIOD1
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.GPIO) >> 4) & 0x01); }
			set
			{
				model.regConfig.GPIO &= 0xEF;   // xxx0 xxxx
				model.regConfig.GPIO |= (Byte)(((value ? 1 : 0) & 0x01) << 4);
				OnPropertyChanged("GPIOD1");
				OnPropertyChanged("GPIOD1_TXT");
			}
		}
		public String GPIOD1_TXT
		{
			get
			{
				return (GPIOD1) ? "Is set" : "Is unset";
			}
		}
		public Byte GPIOC
		{
			get { return (Byte)(((UInt32)model.regConfig.GPIO) & 0x0F); }
			set
			{
				model.regConfig.GPIO &= 0xF0;   // xxxx 0000
				model.regConfig.GPIO |= (Byte)((UInt32)value);
				OnPropertyChanged("GPIOC");
			}
		}
		public Boolean GPIOC4
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.GPIO) >> 3) & 0x01); }
			set
			{
				model.regConfig.GPIO &= 0xF7;   // xxxx 0xxx
				model.regConfig.GPIO |= (Byte)(((value ? 1 : 0) & 0x01) << 3);
				OnPropertyChanged("GPIOC4");
				OnPropertyChanged("GPIOC4_TXT");
			}
		}
		public String GPIOC4_TXT
		{
			get
			{
				return (GPIOC4) ? "Is set" : "Is unset";
			}
		}
		public Boolean GPIOC3
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.GPIO) >> 2) & 0x01); }
			set
			{
				model.regConfig.GPIO &= 0xFB;   // xxxx x0xx
				model.regConfig.GPIO |= (Byte)(((value ? 1 : 0) & 0x01) << 2);
				OnPropertyChanged("GPIOC3");
				OnPropertyChanged("GPIOC3_TXT");
			}
		}
		public String GPIOC3_TXT
		{
			get
			{
				return (GPIOC3) ? "Is set" : "Is unset";
			}
		}
		public Boolean GPIOC2
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.GPIO) >> 1) & 0x01); }
			set
			{
				model.regConfig.GPIO &= 0xFD;   // xxxx xx0x
				model.regConfig.GPIO |= (Byte)(((value ? 1 : 0) & 0x01) << 1);
				OnPropertyChanged("GPIOC2");
				OnPropertyChanged("GPIOC2_TXT");
			}
		}
		public String GPIOC2_TXT
		{
			get
			{
				return (GPIOC2) ? "Is set" : "Is unset";
			}
		}
		public Boolean GPIOC1
		{
			get { return Convert.ToBoolean(((UInt32)model.regConfig.GPIO) & 0x01); }
			set
			{
				model.regConfig.GPIO &= 0xFE;   // xxxx xxx0
				model.regConfig.GPIO |= (Byte)((value ? 1 : 0) & 0x01);
				OnPropertyChanged("GPIOC1");
				OnPropertyChanged("GPIOC1_TXT");
			}
		}
		public String GPIOC1_TXT
		{
			get
			{
				return (GPIOC1) ? "Is set" : "Is unset";
			}
		}

		// PACE: PACE detect register.
		public Byte PACEE
		{
			get { return (Byte)((((UInt32)model.regConfig.PACE) >> 3) & 0x07); }
			set
			{
				model.regConfig.PACE &= 0xE7;   // xxx0 0xxx
				model.regConfig.PACE |= (Byte)(((UInt32)value & 0x07) << 3);
				OnPropertyChanged("PACEE");
			}
		}
		public Byte PACEO
		{
			get { return (Byte)((((UInt32)model.regConfig.PACE) >> 1) & 0x03); }
			set
			{
				model.regConfig.PACE &= 0xF9;   // xxxx x00x
				model.regConfig.PACE |= (Byte)(((UInt32)value & 0x03) << 1);
				OnPropertyChanged("PACEO");
			}
		}
		public Boolean PD_PACE_N
		{
			get { return Convert.ToBoolean(((UInt32)model.regConfig.PACE) & 0x01); }
			set
			{
				model.regConfig.PACE &= 0xFE;   // xxxx xxx0
				model.regConfig.PACE |= (Byte)((value ? 1 : 0) & 0x01);
				OnPropertyChanged("PD_PACE_N");
				OnPropertyChanged("PD_PACE_N_TXT");
			}
		}
		public String PD_PACE_N_TXT
		{
			get
			{
				return (PD_PACE_N) ? "PACE detect buffer turned on" : "PACE detect buffer turned off";
			}
		}

		// RESP: Respiration control register.
		public Boolean RESP_DEMOD_EN1
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.RESP) >> 7) & 0x01); }
			set
			{
				model.regConfig.RESP &= 0x7F;    // 0xxx xxxx
				model.regConfig.RESP |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("RESP_DEMOD_EN1");
				OnPropertyChanged("RESP_DEMOD_EN1_TXT");
			}
		}
		public String RESP_DEMOD_EN1_TXT
		{
			get
			{
				return (RESP_DEMOD_EN1) ? "RESP demodulation circuitry turned on" : "RESP demodulation circuitry turned off";
			}
		}
		public Boolean RESP_MOD_EN1
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.RESP) >> 6) & 0x01); }
			set
			{
				model.regConfig.RESP &= 0xBF;   // x0xx xxxx
				model.regConfig.RESP |= (Byte)(((value ? 1 : 0) & 0x01) << 6);
				OnPropertyChanged("RESP_MOD_EN1");
				OnPropertyChanged("RESP_MOD_EN1_TXT");
			}
		}
		public String RESP_MOD_EN1_TXT
		{
			get
			{
				return (RESP_DEMOD_EN1) ? "RESP modulation circuitry turned on" : "RESP modulation circuitry turned off";
			}
		}
		public Byte RESP_PH
		{
			get { return (Byte)((((UInt32)model.regConfig.PACE) >> 2) & 0x07); }
			set
			{
				model.regConfig.PACE &= 0xE3;   // xxx0 00xx
				model.regConfig.PACE |= (Byte)(((UInt32)value & 0x07) << 2);
				OnPropertyChanged("RESP_PH");
			}
		}
		public Byte RESP_CTRL
		{
			get { return (Byte)(((UInt32)model.regConfig.PACE) & 0x03); }
			set
			{
				model.regConfig.PACE &= 0xFC;   // xxxx xx00
				model.regConfig.PACE |= (Byte)((UInt32)value & 0x03);
				OnPropertyChanged("RESP_CTRL");
			}
		}

		// CONFIG4: Configuration register 4.
		public Byte RESP_FREQ
		{
			get { return (Byte)((((UInt32)model.regConfig.CONFIG4) >> 5) & 0x07); }
			set
			{
				model.regConfig.CONFIG4 &= 0x1F;   // 000x xxxx
				model.regConfig.CONFIG4 |= (Byte)(((UInt32)value & 0x07) << 5);
				OnPropertyChanged("RESP_FREQ");
			}
		}
		public Boolean SINGLE_SHOT
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CONFIG4) >> 3) & 0x01); }
			set
			{
				model.regConfig.CONFIG4 &= 0xF7;   // xxxx 0xxx
				model.regConfig.CONFIG4 |= (Byte)(((value ? 1 : 0) & 0x01) << 3);
				OnPropertyChanged("SINGLE_SHOT");
				OnPropertyChanged("SINGLE_SHOT_TXT");
			}
		}
		public String SINGLE_SHOT_TXT
		{
			get
			{
				return (SINGLE_SHOT) ? "Single-shot mode" : "Continuous conversion mode";
			}
		}
		public Boolean WCT_TO_RLD
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CONFIG4) >> 2) & 0x01); }
			set
			{
				model.regConfig.CONFIG4 &= 0xFB;   // xxxx x0xx
				model.regConfig.CONFIG4 |= (Byte)(((value ? 1 : 0) & 0x01) << 2);
				OnPropertyChanged("WCT_TO_RLD");
				OnPropertyChanged("WCT_TO_RLD_TXT");
			}
		}
		public String WCT_TO_RLD_TXT
		{
			get
			{
				return (WCT_TO_RLD) ? "WCT to RLD connection on" : "WCT to RLD connection off";
			}
		}
		public Boolean PD_LOFF_COMP_N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.CONFIG4) >> 1) & 0x01); }
			set
			{
				model.regConfig.CONFIG4 &= 0xFD;   // xxxx xx0x
				model.regConfig.CONFIG4 |= (Byte)(((value ? 1 : 0) & 0x01) << 1);
				OnPropertyChanged("PD_LOFF_COMP_N");
				OnPropertyChanged("PD_LOFF_COMP_N_TXT");
			}
		}
		public String PD_LOFF_COMP_N_TXT
		{
			get
			{
				return (PD_LOFF_COMP_N) ? "Lead-off comperators enabled" : "Lead-off comperators disabled";
			}
		}

		// WCT1: Wilson Central Terminal and Augmented Lead control register.
		public Boolean aVF_CH6
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.WCT1) >> 7) & 0x01); }
			set
			{
				model.regConfig.WCT1 &= 0x7F;   // 0xxx xxxx
				model.regConfig.WCT1 |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("aVF_CH6");
				OnPropertyChanged("aVF_CH6_TXT");
			}
		}
		public String aVF_CH6_TXT
		{
			get
			{
				return (aVF_CH6) ? "Enabled" : "Disabled";
			}
		}
		public Boolean aVL_CH5
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.WCT1) >> 6) & 0x01); }
			set
			{
				model.regConfig.WCT1 &= 0xBF;  // x0xx xxxx
				model.regConfig.WCT1 |= (Byte)(((value ? 1 : 0) & 0x01) << 6);
				OnPropertyChanged("aVL_CH5");
				OnPropertyChanged("aVL_CH5_TXT");
			}
		}
		public String aVL_CH5_TXT
		{
			get
			{
				return (aVL_CH5) ? "Enabled" : "Disabled";
			}
		}
		public Boolean aVR_CH7
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.WCT1) >> 5) & 0x01); }
			set
			{
				model.regConfig.WCT1 &= 0xDF;   // xx0x xxxx
				model.regConfig.WCT1 |= (Byte)(((value ? 1 : 0) & 0x01) << 5);
				OnPropertyChanged("aVR_CH7");
				OnPropertyChanged("aVR_CH7_TXT");
			}
		}
		public String aVR_CH7_TXT
		{
			get
			{
				return (aVR_CH7) ? "Enabled" : "Disabled";
			}
		}
		public Boolean aVR_CH4
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.WCT1) >> 4) & 0x01); }
			set
			{
				model.regConfig.WCT1 &= 0xEF;  // xxx0 xxxx
				model.regConfig.WCT1 |= (Byte)(((value ? 1 : 0) & 0x01) << 4);
				OnPropertyChanged("aVR_CH4");
				OnPropertyChanged("aVR_CH4_TXT");
			}
		}
		public String aVR_CH4_TXT
		{
			get
			{
				return (aVR_CH4) ? "Enabled" : "Disabled";
			}
		}
		public Boolean PD_WCTA_N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.WCT1) >> 3) & 0x01); }
			set
			{
				model.regConfig.WCT1 &= 0xF7;  // xxxx 0xxx
				model.regConfig.WCT1 |= (Byte)(((value ? 1 : 0) & 0x01) << 3);
				OnPropertyChanged("PD_WCTA_N");
				OnPropertyChanged("PD_WCTA_N_TXT");
			}
		}
		public String PD_WCTA_N_TXT
		{
			get
			{
				return (PD_WCTA_N) ? "Powered on" : "Powered off";
			}
		}
		public Byte WCTA
		{
			get { return (Byte)(((UInt32)model.regConfig.WCT1) & 0x07); }
			set
			{
				model.regConfig.WCT1 &= 0xF8;   // xxxx x000
				model.regConfig.WCT1 |= (Byte)((UInt32)value & 0x07);
				OnPropertyChanged("WCTA");
			}
		}

		// WCT2: Wilson Central Terminal control register.
		public Boolean PD_WCTC_N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.WCT2) >> 7) & 0x01); }
			set
			{
				model.regConfig.WCT2 &= 0x7F;   // 0xxx xxxx
				model.regConfig.WCT2 |= (Byte)(((value ? 1 : 0) & 0x01) << 7);
				OnPropertyChanged("PD_WCTC_N");
				OnPropertyChanged("PD_WCTC_N_TXT");
			}
		}
		public String PD_WCTC_N_TXT
		{
			get
			{
				return (PD_WCTC_N) ? "Powered on" : "Powered off";
			}
		}
		public Boolean PD_WCTB_N
		{
			get { return Convert.ToBoolean((((UInt32)model.regConfig.WCT2) >> 6) & 0x01); }
			set
			{
				model.regConfig.WCT2 &= 0xBF;   // x0xx xxxx
				model.regConfig.WCT2 |= (Byte)(((value ? 1 : 0) & 0x01) << 6);
				OnPropertyChanged("PD_WCTB_N");
				OnPropertyChanged("PD_WCTB_N_TXT");
			}
		}
		public String PD_WCTB_N_TXT
		{
			get
			{
				return (PD_WCTB_N) ? "Powered on" : "Powered off";
			}
		}
		public Byte WCTB
		{
			get { return (Byte)((((UInt32)model.regConfig.WCT2) >> 3) & 0x07); }
			set
			{
				model.regConfig.WCT2 &= 0xC7;   // xx00 0xxx
				model.regConfig.WCT2 |= (Byte)(((UInt32)value & 0x07) << 3);
				OnPropertyChanged("WCTB");
			}
		}
		public Byte WCTC
		{
			get { return (Byte)(((UInt32)model.regConfig.WCT2) & 0x07); }
			set
			{
				model.regConfig.WCT2 &= 0xF8;   // xxxx x000
				model.regConfig.WCT2 |= (Byte)((UInt32)value & 0x07);
				OnPropertyChanged("WCTC");
			}
		}
		#endregion
	}

	public class ValueMatcher
	{
		public Byte ByteValue { get; set; }
		public UInt32 DezValue { get; set; }
		public String StrValue { get; set; }

		public ValueMatcher(Byte byt, UInt32 dez)
		{
			ByteValue = byt;
			DezValue = dez;
		}
		public ValueMatcher(Byte byt, String str)
		{
			ByteValue = byt;
			StrValue = str;
		}
	}
}
