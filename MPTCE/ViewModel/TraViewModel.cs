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
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using EMGFramework.ValueObjects;
using EMGFramework.Utility;
using MPTCE.Model;

namespace MPTCE.ViewModel
{
    public class TraViewModel : INotifyPropertyChanged
    {

        /// <summary>
        /// Event handler that fires whenever a property has been changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


        /// <summary>
        /// An instance of the underlying model
        /// </summary>
        public TraModel traModel
        {
            get;
            private set;
        }


        public List<string> supportedPatternRecognizers
        {
            get
            {
                return traModel.supportedPatternRecognizers;
            }
        }

        public List<string> supportedActivationFunctions
        {
            get
            {
                return traModel.supportedActivationFunctions;
            }
        }

        public List<string> supportedNormalizers
        {
            get
            {
                return traModel.supportedNormalizers;
            }

        }

        public int patternRecognizerIdx
        {
            get
            {
                return traModel.patternRecognizerIdx;
            }
            set
            {
                traModel.patternRecognizerIdx = value;
            }
        }


        public int activationFunctionIdx
        {
            get
            {
                return traModel.activationFunctionIdx;
            }
            set
            {
                traModel.activationFunctionIdx = value;
            }
        }



        private string _selectedPatternRecognizer;
        public string selectedPatternRecognizer
        {
            get
            {
                return _selectedPatternRecognizer;
            }
            set
            {
                if (_selectedPatternRecognizer != value)
                {
                    _selectedPatternRecognizer = value;
                    this.NotifyPropertyChanged("selectedPatternRecognizer");
                }
            }
        }



        private string _selectedActivationFunction;
        public string selectedActivationFunction
        {
            get
            {
                return _selectedActivationFunction;
            }
            set
            {
                if (_selectedActivationFunction != value)
                {
                    _selectedActivationFunction = value;
                    this.NotifyPropertyChanged("selectedActivationFunction");
                }
            }

        }



        private string _selectedNormalizer;
        public string selectedNormalizer
        {
            get
            {
                return _selectedNormalizer;
            }
            set
            {
                if (_selectedNormalizer != value)
                {
                    _selectedNormalizer = value;
                    this.NotifyPropertyChanged("selectedNormalizer");
                }
            }

        }


        public TrainingPackage trainingPackage
        {
            get
            {
                return traModel.trainingPackage;
            }

            set
            {
                traModel.trainingPackage = value;
            }

        }



        public TrainingPackage selectionTrainingPackage
        {
            get
            {
                return traModel.selectionTrainingPackage;
            }

            set
            {
                traModel.selectionTrainingPackage = value;
            }
        }



        /// <summary>
        /// Property giving acces to the collection of selected training sets in the underlying model
        /// </summary>
        public ObservableCollection<DataSet> selectedTrainingSets
        {
            get
            {
                if (traModel != null)
                    return traModel.selectedTrainingSets;
                else return null;
            }
        }



        /// <summary>
        /// This is only used as placeholder when we don't have a trainingPackage yet
        /// </summary>
        private ObservableCollection<DataSet> _trainingSets;
        /// <summary>
        /// Reference to the list of available training sets. Mainly used to display it at the interface
        /// </summary>
        public ObservableCollection<DataSet> trainingSets
        {
            get
            {
                return _trainingSets;
            }
        }



        /// <summary>
        /// A reference to an acqViewModel we need to get its recordedData
        /// </summary>
        public TrtViewModel trtViewModel
        {
            get;
            set;
        }



        /// <summary>
        /// True if the model contains a trained PatternRecognizer, false otherwise.
        /// </summary>
        public bool trained
        {
            get
            {
                if (traModel != null)
                    return traModel.trained;
                else return false;
            }
        }



        private bool _training;
        /// <summary>
        /// True if training or validation are running. False otherwise.
        /// </summary>
        public bool training
        {
            get
            {
                return _training;
            }

            private set
            {
                if (_training != value)
                {
                    _training = value;
                    this.NotifyPropertyChanged("training");
                    this.NotifyPropertyChanged("notTraining");
                }
            }
        }

        /// <summary>
        /// Complementary of the training property. This is used for disabling some UI controls while training is 
        /// running.
        /// </summary>
        public bool notTraining
        {
            get
            {
                return (!training);
            }
        }


        public bool randomizeSets
        {
            get
            {
                if (traModel != null)
                    return traModel.randomizeSets;
                else return false;
            }

            set
            {
                if (traModel != null)
                    traModel.randomizeSets = value;
            }
        }

        
        /// <summary>
        /// Observable collection consumed by the view to display logging messages generated 
        /// during training and validation.
        /// </summary>
        public ObservableCollection<ProgressLogItem> logItems{get; private set;}   

        /// <summary>
        /// Accessed at the view when an PropertyChanged on "concurrentLogList" arrives to 
        /// update the logItems ObservableCollection via a Dispatcher.Invoke() call
        /// </summary>
        public BlockingCollection<ProgressLogItem> concurrentLogList { get; private set; }



        public bool multipleActivationSupport
        {
            get
            {
                return traModel.multipleActivationSupport;
            }
        }

        /// <summary>
        /// Level interpreted as as activation for a given pattern recognizer
        /// </summary>
        public double activationLevel
        {
            get
            {
                return traModel.activationLevel;
            }

            set
            {
                traModel.activationLevel = value;
            }
        }

        /// <summary>
        /// Distance from the activation level within which classification outputs are still considered activations
        /// </summary>
        public double activationTolerance
        {
            get 
            {
                return traModel.activationTolerance;
            }

            set 
            {
                traModel.activationTolerance = value;
            }
        }


        /// <summary>
        /// True if the activation funciton used in the pattern recognizer is valid, false otherwise.
        /// </summary>
        public bool activationFunctionValid
        {
            get
            {
                return traModel.activationFunctionValid;
            }
        }



        public bool multipleActivationEnabled
        {
            get
            {
                return traModel.multipleActivationEnabled;
            }

            set
            {
                if (traModel.multipleActivationEnabled != value)
                    traModel.multipleActivationEnabled = value;
            }
        }



        public bool explicitMovements
        {
            get
            {
                return !traModel.multipleActivationEnabled;
            }

            set
            {
                if (traModel.multipleActivationEnabled != !value)
                {
                    traModel.multipleActivationEnabled = !value;
                    this.NotifyPropertyChanged("explicitMovements");
                }

            }
        }



        /// <summary>
        /// Set to true, it interrupts the current training, but does not invalidate the data. 
        /// </summary>
        public bool interruptTraining
        {
            get
            {
                return traModel.interruptTraining;
            }

            set
            {
                traModel.interruptTraining = value;
            }
        }



        /// <summary>
        /// Dispatcher coming from the main window. Used to create tasks that alter collections only to
        /// be changed from the GUI.
        /// </summary>
        public Dispatcher dispatcher
        {
            get;
            set;
        }



        /// <summary>
        /// This is currently only used to be able to set the enabled status of the training 
        /// TabItem in the GUI depending on whether or not recording with threshold detection is enabled
        /// in the acquisition stage.
        /// </summary>
        public AcqViewModel acqViewModel
        {
            get;
            set;
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



        public TraViewModel()
        {
            this.PropertyChanged += TraViewModel_PropertyChanged;
            traModel = new TraModel();
            traModel.totalMovements = Properties.Settings.Default.AcqMovementsList.Count
                + Properties.Settings.Default.AcqAllowedMovements.Count;
            traModel.totalSingleMovements = Properties.Settings.Default.AcqMovementsList.Count;
            traModel.allowedComplexMovements = Properties.Settings.Default.AcqAllowedMovements;
            traModel.PropertyChanged += traModel_PropertyChanged;
            _trainingSets = new ObservableCollection<DataSet>();
            logItems = new ObservableCollection<ProgressLogItem>();
            concurrentLogList = new BlockingCollection<ProgressLogItem>();
        }


        /// <summary>
        /// It configures the viewModel with its default values.
        /// </summary>
        public void Configure()
        {
            trtViewModel.PropertyChanged += trtViewModel_PropertyChanged;
            reaViewModel.PropertyChanged += reaViewModel_PropertyChanged;
            acqViewModel.PropertyChanged += acqViewModel_PropertyChanged;

            traModel.patternRecognizerIdx = 0;
            traModel.activationFunctionIdx = 0;
            traModel.normalizerIdx = 0;
            traModel.totalMovements = Properties.Settings.Default.AcqMovementsList.Count +
                                        Properties.Settings.Default.AcqAllowedMovements.Count;

        }

        void acqViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "detectThresholds":
                    this.NotifyPropertyChanged("tabActive");
                    break;

                default:
                    break;
            }
        }


        void trtViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "treated":
                    if (trtViewModel.treated)
                    {
                        trainingPackage = trtViewModel.treatmentOutput;
                    }
                    else
                    {
                        trainingPackage = null;
                    }

                    //Here, update trainingSets
                    dispatcher.BeginInvoke(new Action(UpdateTrainingSets));
                    break;

                default:
                    break;
            }
        }



        void TraViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "selectedPatternRecognizer":
                    traModel.patternRecognizerIdx = traModel.supportedPatternRecognizers.IndexOf(selectedPatternRecognizer);
                    break;

                case "selectedActivationFunction":
                    traModel.activationFunctionIdx = traModel.supportedActivationFunctions.IndexOf(selectedActivationFunction);
                    break;

                case "selectedNormalizer":
                    traModel.normalizerIdx = traModel.supportedNormalizers.IndexOf(selectedNormalizer);
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



        void traModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //Forward incoming events
            this.NotifyPropertyChanged(e.PropertyName);

            switch (e.PropertyName)
            {
                case "patternRecognizerIdx":
                    selectedPatternRecognizer = traModel.supportedPatternRecognizers.ElementAt(traModel.patternRecognizerIdx);
                    this.NotifyPropertyChanged("supportedActivationFunctions");
                    this.NotifyPropertyChanged("supportedNormalizers");
                    break;

                case "activationFunctionIdx":
                    if (traModel.activationFunctionIdx >= 0)
                        selectedActivationFunction = traModel.supportedActivationFunctions.ElementAt(traModel.activationFunctionIdx);
                    else selectedActivationFunction = null;
                    break;

                case "normalizerIdx":
                    if (traModel.normalizerIdx >= 0)
                        selectedNormalizer = traModel.supportedNormalizers.ElementAt(traModel.normalizerIdx);
                    else selectedNormalizer = null;
                    break;

                case "trained":
                case "trainingFailed":
                    this.NotifyPropertyChanged(e.PropertyName);
                    if (traModel.trained)
                    {
                        //The status of the model has changes from not trained to trained -> a training has just ended!
                        traModel.RunValidation();

                        //The following should end the thread running FillLog() by taking it out of the 
                        //wait on an empty BlockingCollection<LogItem>
                        traModel.progressLog.RefreshQueue();

                        training = false;
                    }
                    else if (traModel.trainingFailed)
                    {
                        traModel.progressLog.RefreshQueue();
                        training = false;
                    }

                    break;

                case "explicitMovements":
                case "ramdomizeSets":
                case "multipleActivationEnabled":
                case "multipleActivationSupport":
                case "activationLevel":
                case "activationTolerance":
                case "activationFunctionValid":
                    

                    this.NotifyPropertyChanged(e.PropertyName);
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// Used to update the ObservableCollection of training sets when necessary
        /// </summary>
        private void UpdateTrainingSets()
        {
            _trainingSets.Clear();

            if (trainingPackage != null && trainingPackage.trainingSets != null)
            {
                foreach (DataSet dataset in trainingPackage.trainingSets)
                    _trainingSets.Add(dataset);

            }
        }


        private Task _fillLogTask;
        private Task _runTrainingTask;

        public void RunTraining()
        {
            training = true;

            _fillLogTask = Task.Run(new Action(FillLog));

            _runTrainingTask = Task.Run(new Action(traModel.RunTraining));
            
            //traModel.RunTraining();
            //traModel.RunValidation is run through the event fired when traModel.trained becomes true.
            //and that should happen within the _runTrainingTask.
        }


        public void SaveToFile(string filename)
        {

        }

        public void LoadFromFile(string filename)
        {
        }


        public void ClearData()
        {
            traModel.ClearData();
            logItems.Clear();
        }



        /// <summary>
        /// Called on a separate thread, it listens for incoming LogItem objects from the model. Each time 
        /// a LogItem object arrives, it is placed in a BlockingCollection and an event that will trigger 
        /// the Dispatcher in the view is called.
        /// </summary>
        private void FillLog()
        {
            /*
             *This assignation avoids a possible problem when refreshing the BlockingCollection at the ProgressLog object
             *while Fillog() is into the event handler. If the collection is meanwhile marked as complete and then reinitialized,
             *Fillog() will find a new empty collection when it returns to the loop and will stop again there. This is bad
             *because the refresh of the BlockingCollection was intended for FillLog() to end.
             */

            BlockingCollection<ProgressLogItem> logItems = traModel.progressLog.logItems;

            foreach (ProgressLogItem logItem in logItems.GetConsumingEnumerable())
            {
                concurrentLogList.Add(logItem);
                this.NotifyPropertyChanged("concurrentLogList");
            }
        }


    }
}
