﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MPTCE.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("8")]
        public uint AcqDeviceNChannels {
            get {
                return ((uint)(this["AcqDeviceNChannels"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2000")]
        public uint AcqDeviceSampleFreq {
            get {
                return ((uint)(this["AcqDeviceSampleFreq"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ADS1298")]
        public string SelectedDevice {
            get {
                return ((string)(this["SelectedDevice"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("12")]
        public uint AcqDeviceGain {
            get {
                return ((uint)(this["AcqDeviceGain"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("-2")]
        public double AcqDeviceMinV {
            get {
                return ((double)(this["AcqDeviceMinV"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public double AcqDeviceMaxV {
            get {
                return ((double)(this["AcqDeviceMaxV"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int AcqSimultMovements {
            get {
                return ((int)(this["AcqSimultMovements"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AcqDetectThresholds {
            get {
                return ((bool)(this["AcqDetectThresholds"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>ADS1298</string>\r\n  <string>playback</string>\r\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection AcqDeviceList {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["AcqDeviceList"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>rest</string>
  <string>open</string>
  <string>close</string>
  <string>flex</string>
  <string>extend</string>
  <string>pronation</string>
  <string>supination</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection AcqMovementsList {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["AcqMovementsList"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool AcqScheduleActive {
            get {
                return ((bool)(this["AcqScheduleActive"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3")]
        public uint AcqScheduleItemTime {
            get {
                return ((uint)(this["AcqScheduleItemTime"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>1,3</string>
  <string>2,3</string>
  <string>1,4</string>
  <string>2,4</string>
  <string>1,5</string>
  <string>2,5</string>
  <string>1,6</string>
  <string>2,6</string>
  <string>3,5</string>
  <string>4,5</string>
  <string>3,6</string>
  <string>4,6</string>
  <string>1,3,5</string>
  <string>2,3,5</string>
  <string>1,3,6</string>
  <string>2,3,6</string>
  <string>1,4,5</string>
  <string>2,4,5</string>
  <string>1,4,6</string>
  <string>2,4,6</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection AcqAllowedMovements {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["AcqAllowedMovements"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3")]
        public uint AcqRepetitionsPerMovement {
            get {
                return ((uint)(this["AcqRepetitionsPerMovement"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>mean</string>
  <string>tmabs</string>
  <string>twl</string>
  <string>tzc</string>
  <string>tslpchs</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection TrtFeaturesList {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["TrtFeaturesList"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("80")]
        public int TrtTrainingSetSize {
            get {
                return ((int)(this["TrtTrainingSetSize"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("40")]
        public int TrtValidationSetSize {
            get {
                return ((int)(this["TrtValidationSetSize"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.7")]
        public float TrtContractionTimePercentage {
            get {
                return ((float)(this["TrtContractionTimePercentage"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("200")]
        public int TrtWindowSizeMsec {
            get {
                return ((int)(this["TrtWindowSizeMsec"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("50")]
        public int TrtWindowOffsetMsec {
            get {
                return ((int)(this["TrtWindowOffsetMsec"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int AcqScheduleWarmupItems {
            get {
                return ((int)(this["AcqScheduleWarmupItems"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>keyboard</string>\r\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection ReaConsumerList {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["ReaConsumerList"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("movDisplay")]
        public string ReaMovDisplayer {
            get {
                return ((string)(this["ReaMovDisplayer"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ADS1298CorrectOffset {
            get {
                return ((bool)(this["ADS1298CorrectOffset"]));
            }
        }
    }
}
