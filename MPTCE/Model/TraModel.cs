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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EMGFramework.PatternRecognizers;
using EMGFramework.Utility;
using EMGFramework.ValueObjects;


namespace MPTCE.Model
{
    /// <summary>
    /// Implements the model for the training process. It takes as input a TrainingPackage instance and 
    /// produces a trained pattern recognizer.
    /// </summary>
    public class TraModel : INotifyPropertyChanged
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


        private bool _trained;
        /// <summary>
        /// It is set to true when a training has been completed.
        /// </summary>
        public bool trained
        {
            get
            {
                return _trained;
            }

            set
            {
                if (_trained != value)
                {
                    _trained = value;
                    this.NotifyPropertyChanged("trained");
                }
            }
        }



        private bool _trainingFailed;
        /// <summary>
        /// True after an error occurs during training. False otherwise.
        /// </summary>
        public bool trainingFailed
        {
            get
            {
                return _trainingFailed;
            }

            set
            {
                if (_trainingFailed != value)
                {
                    _trainingFailed = value;
                    this.NotifyPropertyChanged("trainingFailed");
                }
            }
        }



        /// <summary>
        /// Provides access to the interruptTraining property defined in the PatternRecognizer
        /// </summary>
        public bool interruptTraining
        {
            get
            {
                if (_patternRecognizer != null)
                    return _patternRecognizer.interruptTraining;
                else return false;
            }

            set
            {
                if (_patternRecognizer != null)
                    _patternRecognizer.interruptTraining = value;
            }

        }



        private bool _randomizeSets;
        /// <summary>
        /// True if the training and validation sets must be randomized before they are fed to the classifier,
        /// false otherwise. The utility of this property is currently unclear, as for methods such as Linear
        /// Discriminant Analysis the randomization of the sets has no effect on the pattern recognition and
        /// training processes, and for other such as those based on artificial neural networks the randomization
        /// is always mandatory for a correct training.
        /// </summary>
        public bool randomizeSets
        {
            get
            {
                return _randomizeSets;
            }

            set
            {
                if (_randomizeSets != value)
                {
                    _randomizeSets = value;
                    this.NotifyPropertyChanged("randomizeSets");
                }
            }
        }



        private TrainingPackage _trainingPackage;
        /// <summary>
        /// TrainingPackage instance used for training. This instance is generated elsewhere, for example, by a 
        /// treatment stage, and must be provided to this model before training can begin;
        /// </summary>
        public TrainingPackage trainingPackage
        {
            get
            {
                return _trainingPackage;
            }

            set
            {
                if (_trainingPackage != value)
                {
                    _trainingPackage = value;
                    this.NotifyPropertyChanged("trainingPackage");
                }
            }
        }


        private TrainingPackage _selectionTrainingPackage;
        /// <summary>
        /// TrainingPackage containing only the selected training and validation sets that will finally be used
        /// for the training
        /// </summary>
        public TrainingPackage selectionTrainingPackage
        {
            get
            {
                return _selectionTrainingPackage;
            }
            set
            {
                if (_selectionTrainingPackage != value)
                {
                    _selectionTrainingPackage = value;
                    this.NotifyPropertyChanged("selectionTrainingPackage");
                }
            }
        }


        private ObservableCollection<DataSet> _selectedTrainingSets;
        /// <summary>
        /// Model list of selected training sets
        /// </summary>
        public ObservableCollection<DataSet> selectedTrainingSets
        {
            get
            {
                return _selectedTrainingSets;
            }

        }



        /// <summary>
        /// List of the supported PatternRecognizer derived classes in the EMGFramework
        /// </summary>
        public List<string> supportedPatternRecognizers
        {
            get
            {
                return GenericFactory<PatternRecognizer>.Instance.SupportedProducts;
            }
        }



        /// <summary>
        /// List of the activation functions supported by the selected PatternRecognizer
        /// </summary>
        public List<string> supportedActivationFunctions
        {
            get
            {
                if (_patternRecognizer != null)
                    //return (List<string>)_patternRecognizer.GetType().BaseType.GetProperty("supportedActivationFunctions").GetValue(null, null);
                    return _patternRecognizer.supportedActivationFunctions;
                else return null;
            }
        }



        /// <summary>
        /// List of the normalizers supported by the selected PatternRecognizer
        /// </summary>
        public List<string> supportedNormalizers
        {
            get
            {
                if (_patternRecognizer != null)
                    //return (List<string>)_patternRecognizer.GetType().BaseType.GetProperty("supportedNormalizers").GetValue(null, null);
                    return _patternRecognizer.supportedNormalizers;
                else return null;
            }
        }



        /// <summary>
        /// ProgressLog object from the active PatternRecognizer
        /// </summary>
        private ProgressLog _progressLog;
        public ProgressLog progressLog
        {
            get
            {
                return _progressLog;
            }

            set
            {
                if (_progressLog != value)
                {
                    _progressLog = value;
                    this.NotifyPropertyChanged("progressLog");
                }
            }
        }


        private PatternRecognizer _patternRecognizer;
        /// <summary>
        /// Index in the list of supported PatternRecognizer derived classes for the PatternRecognizer currently in use
        /// </summary>
        public int patternRecognizerIdx
        {
            get
            {
                if ((_patternRecognizer != null) && (supportedPatternRecognizers.Count != 0))
                    return supportedPatternRecognizers.IndexOf((string)_patternRecognizer.GetType().GetProperty("ID").GetValue(null, null));
                else return -1;
            }
            set
            {
                if ((value >= 0) && (_patternRecognizer == null || supportedPatternRecognizers.IndexOf((string)_patternRecognizer.GetType().GetProperty("ID").GetValue(null, null)) != value))
                {
                    if (_patternRecognizer != null) _patternRecognizer.PropertyChanged -= _patternRecognizer_PropertyChanged;
                    _patternRecognizer = GenericFactory<PatternRecognizer>.Instance.CreateProduct(supportedPatternRecognizers.ElementAt(value));
                    _patternRecognizer.PropertyChanged += _patternRecognizer_PropertyChanged;
                    _patternRecognizer.progressLog = progressLog;
                    this.NotifyPropertyChanged("patternRecognizerIdx");
                }
            }
        }



        /// <summary>
        /// Activation function to be used in the PatternRecognizer
        /// </summary>
        public int activationFunctionIdx
        {
            get
            {
                if (_patternRecognizer != null)
                    return _patternRecognizer.activationFunctionIdx;
                else return -1;
            }

            set
            {
                if (_patternRecognizer != null && _patternRecognizer.activationFunctionIdx != value)
                {
                    _patternRecognizer.activationFunctionIdx = value;
                    this.NotifyPropertyChanged("activationFunctionIdx");
                }

            }
        }



        /// <summary>
        /// (offline) Normalization function used by the PatternRecognizer  
        /// </summary>
        public int normalizerIdx
        {
            get
            {
                if (_patternRecognizer != null)
                    return _patternRecognizer.normalizerIdx;
                else return -1;
            }

            set
            {
                if (_patternRecognizer != null && _patternRecognizer.normalizerIdx != value)
                {
                    _patternRecognizer.normalizerIdx = value;
                    this.NotifyPropertyChanged("normalizerIdx");
                }

            }

        }



        /// <summary>
        /// Total number of movements, including rest, that are possible as defined by the application configuration.
        /// </summary>
        public int totalMovements;



        /// <summary>
        /// Total number of single movements including rest;
        /// </summary>
        public int totalSingleMovements;



        /// <summary>
        /// String list encoding allowed combinations of single movements. Must be initialised from ViewModel. 
        /// </summary>
        public StringCollection allowedComplexMovements { get; set; }



        private bool _multipleActivationSupport;
        /// <summary>
        /// Acquired from the selected pattern recognizer. True if the pattern recognizer currently selected
        /// supports the simultaneous activation of several movement codes at the output. False otherwise.
        /// </summary>
        public bool multipleActivationSupport
        {
            get
            {
                return _multipleActivationSupport;
            }

            private set
            {
                if (_multipleActivationSupport != value)
                {
                    _multipleActivationSupport = value;
                    this.NotifyPropertyChanged("multipleActivationSupport");
                }
            }
        }


        /// <summary>
        /// True if multiple activation is enabled in the selected pattern recognizer, for those
        /// that support it.
        /// </summary>
        public bool multipleActivationEnabled
        {
            get
            {
                if (_patternRecognizer == null) return false;
                else return _patternRecognizer.multipleActivationEnabled;
            }

            set
            {
                if (_patternRecognizer != null && _patternRecognizer.multipleActivationEnabled != value)
                {
                    _patternRecognizer.multipleActivationEnabled = value;
                }
                else if (_patternRecognizer == null)
                {
                    this.NotifyPropertyChanged("multipleActivationEnabled");
                }
            }
        }



        /// <summary>
        /// True if the index to determine the used activation funciton from the list of
        /// those supported by the selected pattern recognizer is valid (>=0) 
        /// </summary>
        public bool activationFunctionValid
        {
            get
            {
                return (activationFunctionIdx >= 0);
            }
        }


        /// <summary>
        /// The difference between what is considered as activation and an actual output of the classifier
        /// whithin which the output is still considered an activation.
        /// </summary>
        public double activationTolerance
        {
            get
            {
                if (_patternRecognizer != null)
                    return _patternRecognizer.activationTolerance;
                else return 0;
            }

            set 
            {
                if (_patternRecognizer != null)
                _patternRecognizer.activationTolerance = value;
            }
        }



        /// <summary>
        /// The value representing an activation. This is initially equal to the activation value defined
        /// at the selected activation function, but can be changed together with the activation tolerance
        /// to define custom activation intervals.
        /// </summary>
        public double activationLevel
        {
            get
            {
                if (_patternRecognizer != null)
                    return _patternRecognizer.activationLevel;
                else return 0;
            }

            set
            {
                if(_patternRecognizer!=null)
                _patternRecognizer.activationLevel = value;
            }
        }



        public TraModel()
        {
            this.PropertyChanged += TraModel_PropertyChanged;

            trained = false;

            _selectionTrainingPackage = new TrainingPackage();
            _selectedTrainingSets = new ObservableCollection<DataSet>();
            _progressLog = new ProgressLog();

            _selectedTrainingSets.CollectionChanged += _selectedTrainingSets_CollectionChanged;
        }



        void TraModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyInfo propertyInfo;

            switch (e.PropertyName)
            {
                case "patternRecognizerIdx":
                    propertyInfo = _patternRecognizer.GetType().GetProperty("multipleActivationSupport");

                    if (propertyInfo != null)
                        multipleActivationSupport = (bool)propertyInfo.GetValue(null, null);
                    else multipleActivationSupport = false;

                    //Shouldn't also activationFunctionIdx and normalizerIdx have changed as a consequence?
                    this.NotifyPropertyChanged("activationFunctionIdx");
                    this.NotifyPropertyChanged("normalizerIdx");
                    this.NotifyPropertyChanged("activationTolerance");
                    break;

                case "activationFunctionIdx":
                    this.NotifyPropertyChanged("activationFunctionValid");

                    break;

                default:
                    break;
            }
        }



        void _selectedTrainingSets_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //When an element is added or removed from _selectedTrainingSets, we must update the trainingpackage
            //that the patternRecognizer will use.
            UpdateSelectionTrainingPackage();
        }



        /// <summary>
        /// Intended to be called whenever the TrainingPackage received from another stage or the 
        /// selection from training sets whithin that TrainigPackage changes
        /// </summary>
        private void UpdateSelectionTrainingPackage()
        {
            _selectionTrainingPackage.trainingSets.Clear();
            _selectionTrainingPackage.validationSets.Clear();


            if (trainingPackage == null) return;

            _selectionTrainingPackage.Copy(trainingPackage);
            _selectionTrainingPackage.movementCodes.Clear();

            foreach (DataSet trainingSet in selectedTrainingSets)
            {
                _selectionTrainingPackage.trainingSets.Add(CreateLightDataSet(trainingSet,trainingPackage));
                _selectionTrainingPackage.movementCodes.Add(trainingSet.movementCode);

                foreach (DataSet validationSet in trainingPackage.validationSets)
                    if (validationSet.movementCode == trainingSet.movementCode)
                    {
                        _selectionTrainingPackage.validationSets.Add(CreateLightDataSet(validationSet, trainingPackage));
                        break;
                    }
            }
        }



        /// <summary>
        /// Used to remove elements from the feature vectors belonging to channels that are not active.
        /// Frames stored in each DataWindow are also discarded.
        /// </summary>
        /// <param name="inputDataSet"></param>
        /// <returns>A DataSet object containing no frames and only features for the active channels.</returns>
        private DataSet CreateLightDataSet(DataSet inputDataSet, TrainingPackage trainigPackage)
        {
            DataSet outputDataSet = new DataSet(inputDataSet.movementCode, inputDataSet.movementComposition);

            foreach (DataWindow window in inputDataSet.set)
            {
                DataWindow lightWindow = new DataWindow();

                foreach (string featureName in window.features.Keys)
                {
                    double[] channelVector;
                    object placeholder;

                    double[] cleanVector = new double[trainingPackage.recordingConfig.activeChannels]; 

                    window.features.TryGetValue(featureName,out placeholder);
                    channelVector = (double[])placeholder;

                    int pos = 0;

                    for (int i = 0; i < channelVector.Length; i++)
                    {
                        if(trainingPackage.recordingConfig.channelMask[i])
                        {
                            cleanVector[pos] = channelVector[i];
                            pos++;
                        }
                    }

                    lightWindow.features.Add(featureName,cleanVector);
                }

                outputDataSet.set.Add(lightWindow);

            }


            return outputDataSet;
        }



        private void _patternRecognizer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "multipleActivationEnabled":
                case "activationLevel":
                case "activationTolerance": 

                    this.NotifyPropertyChanged(e.PropertyName);
                    break;

                default:
                    break;
            }
        }



        /// <summary>
        /// Performs the training of the selected PatternRecognizer
        /// </summary>
        public void RunTraining()
        {
            trainingFailed = false;

            if (_selectionTrainingPackage.trainingSets.Count > 0 && _patternRecognizer != null)
            {
                progressLog.logItems.Add(new ProgressLogItem(ProgressLogItem.Info, String.Format("Training {0} pattern recognizer", _patternRecognizer.GetType().GetProperty("ID").GetValue(null, null))));

                //We shouldn't need this anymore. inputDim and outputDim are calculated ad the PatternRecognizer class
                //when a new trainingPackage is assigned to the PatternRecognizer.
                /*
                _patternRecognizer.inputDim = _selectionTrainingPackage.trainingSets.ElementAt(0).set.ElementAt(0).features.Count * (int)_selectionTrainingPackage.recordingConfig.nChannels;

                if (_patternRecognizer.multipleActivationEnabled) _patternRecognizer.outputDim = totalSingleMovements;
                else _patternRecognizer.outputDim = totalMovements; //This must be taken from the ViewModel
                */

                _patternRecognizer.trainingPackage = _selectionTrainingPackage;

                _patternRecognizer.RunTraining();

                progressLog.logItems.Add(new ProgressLogItem(ProgressLogItem.Info, String.Format("Training complete.")));

                InstanceManager<PatternRecognizer>.Instance.Register(_patternRecognizer);

                progressLog.logItems.Add(new ProgressLogItem(ProgressLogItem.Info, String.Format("List of trained pattern recognizers updated.")));

                trained = true;

                //We do this because from now on the used training package belongs to the trained PatternRecognizer only!
                //So we initialize a new _selectionTrainingPackage for the next training
                _selectionTrainingPackage = new TrainingPackage();
                UpdateSelectionTrainingPackage();

                // Training packages are no longer needed in this PatterRecognizer, so we clear them
                _patternRecognizer.trainingPackage.trainingSets.Clear();


            }
            else
            {
                progressLog.logItems.Add(new ProgressLogItem(ProgressLogItem.Info, String.Format("Training: nothing to do!")));
                trainingFailed = true;
            }
        }

        /// <summary>
        /// It uses the validation sets in the training package to validate the training of the PatternRecognizer
        /// </summary>
        public void RunValidation()
        {
            //TODO: Implement a validation routine in the base PatternRecognizer class.
            if (trained)
            {
                progressLog.logItems.Add(new ProgressLogItem(ProgressLogItem.Info, String.Format("Validating {0} pattern recognizer", _patternRecognizer.GetType().GetProperty("ID").GetValue(null, null))));
                //_patternRecognizer.RunValidation();

                ValidationWorker();

                //We do not need the validationSets anymore
                _patternRecognizer.trainingPackage.validationSets.Clear();
            }
            else progressLog.logItems.Add(new ProgressLogItem(ProgressLogItem.Info, String.Format("Validation: nothing to do!")));

        }



        /// <summary>
        /// Worker method performing the actual validation process
        /// </summary>
        private void ValidationWorker()
        {
            /*
             * WE HAVE MOVED THE VALIDATION CODE FROM THE PatternRecognizer OBJECT TO HERE.
             * This was done because certain required value objects like movement lists have
             * nothing to do with pattern recognising.
             */
            int okCount = 0;
            int failCount = 0;
            int expected;
            int predicted;

            ClassifToMovCodeConverter c2mConverter = new ClassifToMovCodeConverter();
            MovListToCodeConverter ml2cConverter = new MovListToCodeConverter(totalSingleMovements, allowedComplexMovements);

            c2mConverter.multipleActivation = _patternRecognizer.multipleActivationEnabled;
            c2mConverter.activationLevel = _patternRecognizer.activationLevel;
            c2mConverter.activationTolerance = _patternRecognizer.activationTolerance;

            c2mConverter.movementCodes = _patternRecognizer.trainingPackage.movementCodes;

            Dictionary<int, Dictionary<int, int>> confusionMatrix = new Dictionary<int, Dictionary<int, int>>();
            Dictionary<int, int> predictedDictionary;



            progressLog.Log(ProgressLogItem.Info, "*********** VALIDATION ************");

            foreach (DataSet dataSet in _patternRecognizer.trainingPackage.validationSets)
            {
                expected = dataSet.movementCode;

                okCount = 0;
                failCount = 0;

                foreach (DataWindow dataWindow in dataSet.set)
                {
                    double[] inputVector = new double[_patternRecognizer.inputDim];


                    for (int j = 0; j < dataWindow.features.Values.Count; j++)
                    {
                        double[] channelVector = (double[])dataWindow.features.Values.ElementAt(j);
                        for (int k = 0; k < channelVector.Length; k++)
                            inputVector[(j * channelVector.Length) + k] = channelVector[k];
                    }

                    double[] outputVector = (double[])_patternRecognizer.Classify(inputVector);

                    //Here we must determine which movement code corresponds better 
                    //with the input vector. The method for doing this changes depending on 
                    //whether we are using explicit movement codes or not, so we implement it using a converter class

                    if (_patternRecognizer.multipleActivationEnabled)
                    {
                        List<ushort> movementList = (List<ushort>)c2mConverter.Convert(outputVector, null, null, null);
                        predicted = (int)ml2cConverter.Convert(movementList.ToArray(), null, null, null);
                        
                        //The following happens when several outputs went active producing an impossible composite movement
                        //such as "rest + open" or "open + close", for instance. 
                        if (predicted == -1) predicted = movementList.First();

                        //TODO: in this case, we opt for getting the
                        //first movement of the list, bur we could do other things, like try and remove an element at
                        //a tome and see when we come to a valid combination

                        
                    }
                    else predicted = (int)c2mConverter.Convert(outputVector, null, null, null);


                    //Now we check if the predicted movement matches the expected movement
                    if (predicted == expected) okCount++;
                    else failCount++;

                    //Additionally, we compose the confusion matrix.
                    if (confusionMatrix.TryGetValue(expected, out predictedDictionary))
                    {
                        int predictedCount;

                        if (predictedDictionary.TryGetValue(predicted, out predictedCount))
                        {
                            predictedDictionary[predicted]++;
                        }
                        else predictedDictionary.Add(predicted, 1);
                    }
                    else
                    {
                        predictedDictionary = new Dictionary<int, int>();
                        predictedDictionary.Add(predicted, 1);
                        confusionMatrix.Add(expected, predictedDictionary);
                    }


                }

                string report = "Testing movement <mov " + expected + "/> -> correct: " + Math.Round(100 * okCount / (double)(okCount + failCount), 2) + "%";

                confusionMatrix.TryGetValue(expected, out predictedDictionary);
                if (predictedDictionary.Keys.Count > 1)
                {
                    report = report + "; misclassified as: ";
                    foreach (int prediction in predictedDictionary.Keys)
                    {
                        if (prediction != expected)
                        {
                            report = report + "<mov " + prediction + "/> " + Math.Round(100 * predictedDictionary[prediction] / (double)(okCount + failCount), 2) + "%; ";
                        }

                    }
                }

                progressLog.Log(ProgressLogItem.Info, report);
            }

            progressLog.Log(ProgressLogItem.Info, "********* VALIDATION FINISHED *********");


        }



        /// <summary>
        /// Clears the training information by resetting the PatternRecognizer
        /// </summary>
        public void ClearData()
        {
            _patternRecognizer.ClearTraining();
            trained = false;
        }


    }
}
