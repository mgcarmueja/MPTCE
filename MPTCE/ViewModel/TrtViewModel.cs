/*
 * Copyright 2015 Martin Garcia Carmueja 
 * 
 *  This file is part of the Myoelectric Personal Training and Control Environment (MPTCE).
 *
 *  MPTCE is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  MPTCE is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with MPTCE.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows;
using EMGFramework.ValueObjects;
using EMGFramework.Utility;
using EMGFramework.Features;
using MPTCE.Model;

namespace MPTCE.ViewModel
{
    public class TrtViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Event handler that fires whenever a property in the ViewModel has been changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;



        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }



        /// <summary>
        /// An instance to the underlying model
        /// </summary>
        public TrtModel trtModel { get; private set; }



        /// <summary>
        /// A reference to an acqViewModel needed to get its recordedData
        /// </summary>
        public AcqViewModel acqViewModel
        {
            get;
            set;
        }
        


        public Recording acqRecording
        {
            get 
            {
                if (trtModel != null) return trtModel.acqRecording;
                else return null;
            }
            set 
            {
                if ((trtModel != null) && (trtModel.acqRecording != value))
                {
                    trtModel.acqRecording = value;
                }
            }
        }



        private List<string> _features;
        public List<string> features
        {
            get
            {
                return _features;
            }

            private set
            {
                if (_features != value)
                {
                    _features = value;
                    this.NotifyPropertyChanged("features");
                }
            }
        }



        public int trainingSetSize
        {
            get
            {
                return trtModel.treatmentConfig.trainingSetSize;
            }
            set 
            { 
                if(trtModel.treatmentConfig.trainingSetSize != value)
                {
                    trtModel.treatmentConfig.trainingSetSize = value;
                    this.NotifyPropertyChanged("trainingSetSize");
                }
            }
        }



        public int validationSetSize
        {
            get
            {
                return trtModel.treatmentConfig.validationSetSize;
            }
            set
            {
                if (trtModel.treatmentConfig.validationSetSize != value)
                {
                    trtModel.treatmentConfig.validationSetSize = value;
                    this.NotifyPropertyChanged("validationSetSize");
                }
            }
        }



        private int _totalSetSize;
        public int totalSetSize
        {
            get 
            {
                return _totalSetSize;
            }

            set 
            {
                if(_totalSetSize!=value)
                {
                    _totalSetSize = value;
                    this.NotifyPropertyChanged("totalSetSize");
                }
            }
        }



        public ObservableCollection<string> selectedFeatures
        {
            get 
            {
                if (trtModel != null)
                    return trtModel.selectedFeatures;
                else return null;
            }
        }



        public bool treated
        {
            get 
            {
                return trtModel.treated;
            }

        }


        public bool randomWindowSelection
        {
            get 
            {
                return trtModel.randomWindowSelection;
            }

            set 
            {
                trtModel.randomWindowSelection = value;
            }
        }



        public bool includeRests
        {
            get 
            { 
                return trtModel.includeRests; 
            }
            set 
            {
                if (trtModel.includeRests != value)
                {
                    trtModel.includeRests = value;
                    this.NotifyPropertyChanged("includeRests");
                }
            }
        }


        /// <summary>
        /// Visibility for the indicator showing a spinning wheel in the UI
        /// </summary>
        public Visibility busyIndicatorVisible
        {
            get
            {
                if(trtModel.running)
                    return Visibility.Visible;
                else return Visibility.Collapsed;
            }
        }



        /// <summary>
        /// Visibility for the indicator showing a checkmark in the UI
        /// </summary>
        public Visibility okIndicatorVisible
        {
            get
            {
                if (trtModel.treated) return Visibility.Visible;
                else return Visibility.Collapsed;
            }
        }



        /// <summary>
        /// Complementary to the running property on the model. Used to disable controls while 
        /// treatment is runnings
        /// </summary>
        public bool notRunning
        {
            get
            {
                return !trtModel.running;
            }
        }



        /// <summary>
        /// This is currently used only to obtain its running status
        /// </summary>
        public ReaViewModel reaViewModel
        {
            get;
            set;
        }



        /// <summary>
        /// true if the tab corresponding with this ViewModel at the UI should be active, false otherwise
        /// </summary>
        public bool tabActive
        { 
            get
            {
                return (!acqViewModel.detectThresholds && !reaViewModel.isRunning);
            }
        }



        public TrtViewModel()
        {
            this.PropertyChanged += TrtViewModel_PropertyChanged;

            features = GenericFactory<Feature>.Instance.SupportedProducts;

            trtModel = new TrtModel();
            trtModel.PropertyChanged += trtModel_PropertyChanged;
        }



        void TrtViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "trainingSetSize":
                case "validationSetSize":
                    totalSetSize = trainingSetSize + validationSetSize;
                    break;
                default:
                    break;
            }
        }



        void trtModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "randomWindowSelection":
                    this.NotifyPropertyChanged(e.PropertyName);
                    break;
                        
                case "running":
                    this.NotifyPropertyChanged(e.PropertyName);
                    this.NotifyPropertyChanged("notrunning");
                    this.NotifyPropertyChanged("busyIndicatorVisible");
                    break;

                case "treated":
                    this.NotifyPropertyChanged(e.PropertyName);
                    this.NotifyPropertyChanged("okIndicatorVisible");
                    break;
                default:
                    break;
            }
        }

        
        
        void acqViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "recordedData":
                    this.acqRecording = acqViewModel.recordedData;
                    break;

                case "detectThresholds":
                    this.NotifyPropertyChanged("tabActive");
                    break;
                
                default:
                    break;
            }
        }


        void reaViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "isRunning":
                    this.NotifyPropertyChanged("tabActive");
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// This is the final result of the treatment stage: a TrainingPackage object
        /// </summary>
        public TrainingPackage treatmentOutput
        {
            get 
            {
                return trtModel.trainingPackage; 
            }
        }



        /// <summary>
        /// It configures the viewModel with its default values.
        /// </summary>
        public void Configure()
        {
            acqViewModel.PropertyChanged += acqViewModel_PropertyChanged;
            reaViewModel.PropertyChanged += reaViewModel_PropertyChanged;

            acqRecording = acqViewModel.recordedData;

            trainingSetSize = Properties.Settings.Default.TrtTrainingSetSize;
            validationSetSize = Properties.Settings.Default.TrtValidationSetSize;

            trtModel.treatmentConfig.contractionTimePercentage = Properties.Settings.Default.TrtContractionTimePercentage;
            trtModel.treatmentConfig.sampleFreq = Properties.Settings.Default.AcqDeviceSampleFreq;
            trtModel.treatmentConfig.trainingSetSize = Properties.Settings.Default.TrtTrainingSetSize;
            trtModel.treatmentConfig.validationSetSize = Properties.Settings.Default.TrtValidationSetSize;
            trtModel.treatmentConfig.windowLengthmsec = Properties.Settings.Default.TrtWindowSizeMsec;
            trtModel.treatmentConfig.windowOffsetmsec = Properties.Settings.Default.TrtWindowOffsetMsec;

            trtModel.Init();
        }




        private Task _treatTask;

        /// <summary>
        /// This method sould be called when pressing the "Treat" button at the UI
        /// </summary>
        public void Treat()
        {
            _treatTask = Task.Run(new Action(trtModel.ProcessRecording));
           
            //trtModel.ProcessRecording();
        }

        public void LoadFromFile(string fileName)
        {
            //treated = true; // This is done when the loaded data are assigned to the model
        }

        public void SaveToFile(string fileName)
        {
        }

        /// <summary>
        /// This method should clear the treated or loaded data in the model
        /// </summary>
        public void ClearData()
        {
            trtModel.ClearTreatment();
        }


    }
}
