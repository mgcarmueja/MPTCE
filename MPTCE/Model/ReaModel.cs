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
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using EMGFramework.DataProvider;
using EMGFramework.PatternRecognizers;
using EMGFramework.Utility;
using EMGFramework.ValueObjects;
using EMGFramework.Pipelines;
using MPTCE.Dialogs;



namespace MPTCE.Model
{
    /// <summary>
    /// Model for realtime movement recognition 
    /// </summary>
    public class ReaModel : INotifyPropertyChanged
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


        private PatternRecognizer _patternRecognizer;
        /// <summary>
        /// String describing the pattern recognizer object in use. This corresponds to the ID class 
        /// property defined in the class of the selected pattern recognizer. PatternRecognizer objects
        /// are never created in this model, but simply retrieved from an InstanceManager.
        /// </summary>
        public string patternRecognizer
        {
            get
            {
                if (_patternRecognizer != null)
                    return (string)_patternRecognizer.GetType().GetProperty("ID").GetValue(null, null);
                else return null;
            }

            set
            {
                if (
                    InstanceManager<PatternRecognizer>.Instance.RegisteredInstances.Contains(value)
                    && (
                        _patternRecognizer == null
                        || (string)_patternRecognizer.GetType().GetProperty("ID").GetValue(null, null) != value
                      )
                  )
                    _patternRecognizer = InstanceManager<PatternRecognizer>.Instance.Retrieve(value);
                this.NotifyPropertyChanged("patternRecognizer");
            }
        }


        private EMGDataProvider _dataProvider;
        /// <summary>
        /// String describig the EMGDataProvider used
        /// </summary>
        public string dataProvider
        {
            get
            {
                if (_dataProvider != null)
                    return (string)_dataProvider.GetType().GetProperty("ID").GetValue(null, null);
                else return null;
            }

            set
            {
                if (
                    GenericFactory<EMGDataProvider>.Instance.SupportedProducts.Contains(value)
                    && (
                        _dataProvider == null
                        || (string)_dataProvider.GetType().GetProperty("ID").GetValue(null, null) != value
                       )
                    )
                {
                    //We use the InstanceManager<EMGDataProvider> as a cache 
                    //for generating EMGDataProvider objects only once

                    _dataProvider = InstanceManager<EMGDataProvider>.Instance.Retrieve(value);
                    if (_dataProvider == null)
                    {
                        _dataProvider = GenericFactory<EMGDataProvider>.Instance.CreateProduct(value);
                        InstanceManager<EMGDataProvider>.Instance.Register(_dataProvider);

                        if (_dataProvider.readsFromFile)
                        {
                            _dataProvider.knownMovements = Properties.Settings.Default.AcqMovementsList;
                            _dataProvider.allowedComplexMovements = Properties.Settings.Default.AcqAllowedMovements;
                            _dataProvider.movementSelector = new DialogMovementSelector();
                        }

                    }
                    _dataProvider.loop = true;

                    this.NotifyPropertyChanged("dataProvider");
                }
            }
        }


        private bool _levelControlled;
        /// <summary>
        /// True if level-based and not pattern-based movement recognition is being used
        /// </summary>
        public bool levelControlled
        {
            get
            {
                return _levelControlled;
            }
            set
            {
                if (_levelControlled != value)
                {
                    _levelControlled = value;
                    this.NotifyPropertyChanged("levelControlled");
                }
            }
        }


        private TreatmentConfig _treatmentConfig;
        /// <summary>
        /// Treatment configuration used by some pipeline stages.
        /// </summary>
        public TreatmentConfig treatmentConfig
        {
            get
            {
                if (!levelControlled)
                    if (_patternRecognizer != null)
                        return _patternRecognizer.trainingPackage.treatmentConfig;
                    else return null;
                else return _treatmentConfig;
            }

            private set
            {
                if (_treatmentConfig != value)
                {
                    _treatmentConfig = value;
                    this.NotifyPropertyChanged("treatmentConfig");
                }
            }
        }


        private RecordingConfig _recordingConfig;
        /// <summary>
        /// Recording configuration used by some pipeline stages.
        /// </summary>
        public RecordingConfig recordingConfig
        {
            get
            {
                return _recordingConfig;
            }

            private set
            {
                if (_recordingConfig != value)
                {
                    bool nullChanged = false;

                    if ((_recordingConfig != null && value == null) || (_recordingConfig == null && value != null))
                        nullChanged = true;


                    _recordingConfig = value;
                    this.NotifyPropertyChanged("recordingConfig");
                    if (nullChanged) this.NotifyPropertyChanged("readyToRun");
                }
            }
        }



        /// <summary>
        /// Pipeline object for movement detection
        /// </summary>
        private Pipeline _pipeline;


        private ObjectServer<Movement> _objectServer;
        /// <summary>
        /// Object server used to allow more than one client to listen to movement codes
        /// </summary>
        public ObjectServer<Movement> objectServer
        {
            get
            {
                return _objectServer;
            }

        }


        /// <summary>
        /// Pipeline stages needed for pattern-based movement recognition
        /// </summary>
        private AcquisitionController _acquisitionController;
        private WindowMaker _windowMaker;
        private FeatureExtractor _featureExtractor;
        private PatternRecognition _patternRecognition;
        private MovementGenerator _movementGenerator;

        /// <summary>
        /// Pipeline stages needed for level-based movement recognition
        /// </summary>
        private ThresholdEngine _thresholdEngine;


        private bool _isRunning;
        /// <summary>
        /// True if the model is running a movement recognition session. False otherwise.
        /// </summary>
        public bool isRunning
        {
            get
            {
                return _isRunning;
            }
            private set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    this.NotifyPropertyChanged("isRunning");
                }
            }
        }



        /// <summary>
        /// Returns the data monitoring BlockingCollection  defined in the acquisition controller
        /// </summary>
        public BlockingCollection<object> dataMonitor
        {
            get
            {
                if (_acquisitionController != null && !levelControlled) return _acquisitionController.dataMonitor;
                else if (_thresholdEngine != null && levelControlled) return _thresholdEngine.dataMonitor;

                else return null;
            }
        }



        private BindingList<ThresholdControl> _thresholdControls;
        /// <summary>
        /// This is the list containing the controls that are used to define the threshold level for each active channel
        /// when performing threshold-based detection.
        /// </summary>
        public BindingList<ThresholdControl> thresholdControls
        {
            get
            {
                return _thresholdControls;
            }
        }



        private int _nMovements;
        /// <summary>
        /// Total number of movements (simple and composite) that will be recognised.
        /// </summary>
        public int nMovements
        {
            get
            {
                return _nMovements;
            }

            set
            {
                if (_nMovements != value)
                {
                    _nMovements = value;
                    this.NotifyPropertyChanged("nMovements");
                }
            }
        }



        private int _numSingleMovements;
        /// <summary>
        /// Number of known single movements
        /// </summary>
        public int numSingleMovements
        {
            get
            {
                return _numSingleMovements;
            }

            set
            {
                if (_numSingleMovements != value)
                {
                    _numSingleMovements = value;
                    this.NotifyPropertyChanged("numSingleMovements");
                }
            }

        }



        private StringCollection _allowedComplexMovements;
        /// <summary>
        /// String collection containing the list of allowed combinations of single movements
        /// </summary>
        public StringCollection allowedComplexMovements
        {
            get
            {
                return _allowedComplexMovements;
            }

            set
            {
                if (_allowedComplexMovements != value)
                {
                    _allowedComplexMovements = value;
                    this.NotifyPropertyChanged("allowedComplexMovements");
                }
            }
        }



        private RecordingConfig _thresholdRecordingConfig;
        /// <summary>
        /// RecordingConfig instance containing the configuration used when performing threshond-based
        /// movement detection.
        /// </summary>
        public RecordingConfig thresholdRecordingConfig
        {
            get
            {
                return _thresholdRecordingConfig;
            }

            set
            {
                if (_thresholdRecordingConfig != value)
                {
                    _thresholdRecordingConfig = value;
                    this.NotifyPropertyChanged("thresholdRecordingConfig");
                }
            }
        }



        public bool readyToRun
        {
            get
            {
                return (recordingConfig != null);
            }
        }


        private MovementMetadata _movementMetadata;
        /// <summary>
        /// Metadata for the movements generated through the MovementGenerator pipeline stage.
        /// A copy of this object will be added to every movement generated by that stage.
        /// </summary>
        public MovementMetadata movementMetadata
        {
            get
            {
                return _movementMetadata;
            }
        }


        private bool _multipleActivation;
        /// <summary>
        /// True if the movement generator should expect multiple simultaneous activations from the classifier.
        /// False otherwise
        /// </summary>
        public bool multipleActivation
        {
            get
            {
                return _multipleActivation;
            }

            set
            {
                if (_multipleActivation != value)
                {
                    _multipleActivation = value;
                    this.NotifyPropertyChanged("multipleActivation");
                }
            }
        }

        /// <summary>
        /// True if a pattern recognizer is defined in the model and 
        /// it supports multiple activation. False otherwise.
        /// </summary>
        public bool multipleActivationSupported
        {
            get
            {
                if (_patternRecognizer != null)
                {
                    bool returnValue = (bool)_patternRecognizer.GetType().GetProperty("multipleActivationSupport").GetValue(null, null);
                    if (returnValue != null) return returnValue;
                    else return false;
                }
                else return false;
            }
        }

        /// <summary>
        /// This property builds dynamically a list of supported movements by either the active PatternRecognizer
        /// or the thresholdControls, depending on whether levelControlled is false or true, respectively.
        /// </summary>
        public List<Movement> supportedMovements
        {

            get
            {
                List<Movement> movList = new List<Movement>();
                List<int> addedMovements = new List<int>();
                MovCodeToStringConverter converter = new MovCodeToStringConverter(Properties.Settings.Default.AcqMovementsList, Properties.Settings.Default.AcqAllowedMovements);

                if (levelControlled)
                {
                    foreach (ThresholdControl control in thresholdControls)
                    {
                        int movCode = control.selectedMovement;

                        if (!addedMovements.Contains(movCode))
                        {
                            string movementName = (string)converter.Convert(movCode, null, null, null);
                            Movement newMovement = new Movement(movCode, movementName);
                            movList.Add(newMovement);
                        }
                        addedMovements.Add(movCode);
                    }

                }
                else if (_patternRecognizer != null)
                {

                    foreach (int movCode in _patternRecognizer.trainingPackage.movementCodes)
                    {
                        if (movCode != 0) //Rests should not count as a supported movement for RealtimeConsumer objects.
                        {
                            string movementName = (string)converter.Convert(movCode, null, null, null);
                            Movement newMovement = new Movement(movCode, movementName);
                            movList.Add(newMovement);
                        }
                    }
                }

                return movList;
            }
        }




        public ReaModel()
        {
            this.PropertyChanged += ReaModel_PropertyChanged;

            _objectServer = new ObjectServer<Movement>();


            _thresholdControls = new BindingList<ThresholdControl>();

            _movementMetadata = new MovementMetadata();

        }



        void ReaModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {

                case "thresholdRecordingConfig":
                    if (thresholdRecordingConfig != null)
                    {
                        LoadThesholdControls();
                    }
                    break;

                case "patternRecognizer":
                    if (!levelControlled)
                    {
                        if (_patternRecognizer != null)
                            recordingConfig = _patternRecognizer.trainingPackage.recordingConfig;
                        else _patternRecognizer = null;
                        this.NotifyPropertyChanged("multipleActivationSupported");
                    }
                    break;

                case "levelControlled":
                    if (levelControlled)
                    {
                        recordingConfig = thresholdRecordingConfig;
                    }
                    else if (_patternRecognizer != null)
                    {
                        recordingConfig = _patternRecognizer.trainingPackage.recordingConfig;
                    }
                    else recordingConfig = null;

                    if (_movementGenerator != null) _movementGenerator.levelControlled = levelControlled;
                    break;

                case "multipleActivation":

                    if (_movementGenerator != null)
                        _movementGenerator.multipleActivation = multipleActivation;

                    break;
                default:
                    break;
            }
        }






        /// <summary>
        /// Initialization of the pipeline used for realtime movement recognition based on the settings
        /// </summary>
        public void Init()
        {

            //Initializing the parameters that will not change between executions for all stages

            _acquisitionController = new AcquisitionController();
            _windowMaker = new WindowMaker();
            _featureExtractor = new FeatureExtractor();
            _patternRecognition = new PatternRecognition();
            _movementGenerator = new MovementGenerator();
            _thresholdEngine = new ThresholdEngine();

            _acquisitionController.isOnline = true;
            _acquisitionController.isMonitored = true;
            _windowMaker.isOnline = true;

            _featureExtractor.isOnline = true;
            _featureExtractor.isMonitored = false;

            _movementGenerator.nMovements = nMovements;
            _movementGenerator.numSingleMovements = numSingleMovements;
            _movementGenerator.allowedComplexMovements = allowedComplexMovements;


            _movementGenerator.objectServer = _objectServer;
            _movementGenerator.movementMetadata = movementMetadata;

            _thresholdEngine.enabled = true;
            _thresholdEngine.isOnline = true;

        }


        private void LoadThesholdControls()
        {
            ThresholdSet thresholdSet = thresholdRecordingConfig.thresholdSet;
            uint i;

            thresholdControls.Clear();

            for (i = 0; i < thresholdSet.nChannels; i++)
            {

                if (thresholdRecordingConfig.channelMask[i])
                {
                    //Channel i is enabled
                    ThresholdControl control = new ThresholdControl(i+1, thresholdSet.minValues[i], thresholdSet.maxValues[i]);
                    control.PropertyChanged += control_PropertyChanged;
                    thresholdControls.Add(control);
                }
            }
        }


        /// <summary>
        /// When something changes in a ThresholdControl object, perhaps we will need to propagate those changes. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void control_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ThresholdControl control = (ThresholdControl)sender;

            switch (e.PropertyName)
            {
                case "threshold":
                case "selectedMovement":
                    this.NotifyPropertyChanged(e.PropertyName);
                    break;
                
                default:
                    break;
            }
        }


        /// <summary>
        /// Called before starting the movement detection process. It builds the proper pipeline depending on whether
        /// a threshold-based or a pattern-recognition-based movement detection is bein performed
        /// </summary>
        private void PreparePipeline()
        {
            _pipeline = new Pipeline();

            _acquisitionController.dataProvider = _dataProvider;
            _acquisitionController.recordingConfig = recordingConfig;

            if (!levelControlled)
            {
                _acquisitionController.dataMonitoring = true;
                _thresholdEngine.monitoring = false;

                _windowMaker.config = _patternRecognizer.trainingPackage.treatmentConfig;
                _featureExtractor.selectedFeatures = _patternRecognizer.trainingPackage.treatmentConfig.features;
                _patternRecognition.patternRecognizer = _patternRecognizer;

                _movementGenerator.multipleActivation = multipleActivation;
                _movementGenerator.activationLevel = _patternRecognizer.activationLevel;
                _movementGenerator.activationTolerance = _patternRecognizer.activationTolerance;
                _movementGenerator.movementCodes = _patternRecognizer.trainingPackage.movementCodes;

                _pipeline.AddStage(_acquisitionController);
                _pipeline.AddStage(_windowMaker);
                _pipeline.AddStage(_featureExtractor);
                _pipeline.AddStage(_patternRecognition);
                _pipeline.AddStage(_movementGenerator);
            }
            else
            {
                _acquisitionController.dataMonitoring = false;
                _thresholdEngine.monitoring = true;

                _movementGenerator.multipleActivation = false;
                _movementGenerator.activationLevel = 1;
                _movementGenerator.thresholdControls.Clear();

                foreach (ThresholdControl item in thresholdControls)
                    _movementGenerator.thresholdControls.Add(item);

                _thresholdEngine.recordingConfig = recordingConfig;

                //TODO: This does not work as it is
                //Where sould the RecordingConfig for the EMGDataProvider come from?


                _pipeline.AddStage(_acquisitionController);

                _pipeline.AddStage(_thresholdEngine);

                //TODO: We have a processed signal that can be compared against thresholds stored in a
                //ThresholdSet object. Now we need a stage that will generate activations on each channel.
                //Perhaps this can be incorporated directly into the MovementGenerator class.

                _pipeline.AddStage(_movementGenerator);
            }
        }


        /// <summary>
        /// Starts the movement recognition by first initializing the pipeline and dataSource that will be used and then
        /// starting everything
        /// </summary>
        public void RunRecognition()
        {
            //if (_patternRecognizer == null) return;

            PreparePipeline();
            try
            {
                _pipeline.Init();
            }
            catch (AcquisitionControllerException ex)
            {
                throw new ReaModelException("Pipeline.Init:" + ex.Message);
            }

            _pipeline.Start();

            isRunning = true;

        }



        /// <summary>
        /// Stops the movement recognition if it is running
        /// </summary>
        public void StopRecognition()
        {
            _pipeline.Stop();
            isRunning = false;
        }




    }
}
