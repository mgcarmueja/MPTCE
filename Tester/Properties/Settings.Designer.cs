﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tester.Properties {
    
    
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
        public global::System.Collections.Specialized.StringCollection movementsList {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["movementsList"]));
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
        public global::System.Collections.Specialized.StringCollection allowedMovements {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["allowedMovements"]));
            }
        }
    }
}
