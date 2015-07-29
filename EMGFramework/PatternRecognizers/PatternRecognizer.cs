/*
 * Copyright 2015 Martin Garcia Carmueja 
 * 
 *  This file is part of EMGFramework.
 *
 *  EMGFramework is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  EMGFramework is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with EMGFramework.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EMGFramework.Utility;
using EMGFramework.ValueObjects;

namespace EMGFramework.PatternRecognizers
{
    /// <summary>
    /// Delegate used to define the activation function that will be used for the PatternRecognizer
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    //public delegate void ActivationFunction(double[] input);

    /// <summary>
    /// Delegate defining a normalizer method that will be used in offline scenarios.
    /// </summary>
    /// <param name="featureVectors">List containing arrays of feature vectors. 
    /// Usually each array groups feature vectors belonging to the same movement</param>
    //public delegate void OfflineNormalizer(List<double[,]> featureVectors);

    /// <summary>
    /// Delegate defining a normalizer method that will be used in online scenarios.
    /// </summary>
    /// <param name="featureVector">Single realtime feature vector to normalize</param>
    //public delegate void OnlineNormalizer(double[] featureVector);


    public abstract class PatternRecognizer : INotifyPropertyChanged
    {

        private ProgressLog _progressLog;
        /// <summary>
        /// Progress log object
        /// </summary>
        public ProgressLog progressLog
        {
            get 
            {
                return _progressLog;
            }
            
            set 
            {
                if(_progressLog!=value)
                {
                    _progressLog = value;
                    this.NotifyPropertyChanged("progressLog");
                }
            }
        }

        
        private bool _interruptTraining = false;
        /// <summary>
        /// For pattern recognizers with a potentially long training process, this property can be used
        /// to exit the training loop at any time.
        /// </summary>
        public bool interruptTraining
        {
            get 
            {
                return _interruptTraining;
            }

            set
            {
                if (_interruptTraining != value)
                {
                    _interruptTraining = value;
                    this.NotifyPropertyChanged("interruptTraining");
                }
            }
        }


        private bool _trained = false;
        /// <summary>
        /// True if the PatternRecognizer has been trained, false otherwise or after calling
        /// ClearTraining()
        /// </summary>
        public bool trained
        {
            get 
            { 
                return _trained; 
            }

            protected set
            {
                if (_trained != value)
                {
                    _trained = value;
                    this.NotifyPropertyChanged("trained");
                }
            }
            
        }

        /// <summary>
        /// Event handler that fires whenever a property has been changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }



        private TrainingPackage _trainingPackage;
        /// <summary>
        /// The training package that this algorithm will use for training and verification purposes
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


        private int _inputDim;
        /// <summary>
        /// Dimension of the input vector
        /// </summary>
        public int inputDim
        {
            get
            {
                return _inputDim;
            }
            protected set 
            {
                if (_inputDim != value)
                {
                    _inputDim = value;
                    this.NotifyPropertyChanged("inputDim");
                }
            }
        }


        private int _outputDim;
        /// <summary>
        /// Dimension of the output vector
        /// </summary>
        public int outputDim
        {
            get
            {
                return _outputDim;
            }

            protected set
            {
                if (_outputDim != value)
                {
                    _outputDim = value;
                    this.NotifyPropertyChanged("outputDim");
                }
            }
        }
        

        /// <summary>
        /// Actual activation function instance that will be used
        /// </summary>
        protected Activation activationFunction;
        /// <summary>
        /// Index of the selected activation function
        /// </summary>
        public int activationFunctionIdx
        {
            get 
            {
                if ((activationFunction != null) && (supportedActivationFunctions != null) && (supportedActivationFunctions.Count > 0))
                    return supportedActivationFunctions.IndexOf((string)activationFunction.GetType().GetProperty("ID").GetValue(null, null));
                else return -1;
            }
            
            set
            {
                if((value>=0) && (value< supportedActivationFunctions.Count))
                {
                    activationFunction = GenericFactory<Activation>.Instance.CreateProduct(supportedActivationFunctions.ElementAt(value));
                    this.NotifyPropertyChanged("activationFunctionIdx");
                }
                //And if not, we do nothing, but we could also throw an exception here.
            }
        }



        protected Normalization normalizer;
        /// <summary>
        /// Offline normalizer method to be used
        /// </summary>
        public int normalizerIdx
        {
            get
            {
                if ((normalizer != null) && (supportedNormalizers != null) && (supportedNormalizers.Count > 0))
                    return supportedNormalizers.IndexOf((string)normalizer.GetType().GetProperty("ID").GetValue(null, null));
                else return -1;
            }

            set
            {
                if ((value >= 0) && (value < supportedNormalizers.Count))
                {
                    normalizer = GenericFactory<Normalization>.Instance.CreateProduct(supportedNormalizers.ElementAt(value));
                    this.NotifyPropertyChanged("normalizerIdx");
                }
                //And if not, we do nothing, but we could also throw an exception here.
            }
        }


        protected List<string> _supportedActivationFunctions; 
        /// <summary>
        /// List of supported activation functions by a given PatternRecognizer derived class
        /// </summary>
        public List<string> supportedActivationFunctions
        {
            get { return _supportedActivationFunctions;}
        }



        protected List<string> _supportedNormalizers;
        /// <summary>
        /// List of supported offline normalizers by a given PatternRecognizer derived class
        /// </summary>
        public List<string> supportedNormalizers
        {
            get { return _supportedNormalizers;}
        }


        /// <summary>
        /// This ID will be used for the PatternRecognizerFactory to register and instantiate classes descending
        /// from this one. Don't forget to override this property with your class' own ID!
        /// </summary>
        public static string ID
        {
            get
            {
                return "Undefined";
            }
        }

        /// <summary>
        /// This displayName should be more user-friendly than the ID and therefore usable in an UI.
        /// </summary>
        public static string displayName
        {
            get { return "Undefined pattern recognizer"; }
        }


        /// <summary>
        /// This static property should be redefined in any children class to indicate whether the pattern recognizer
        /// supports the simultaneous activation of several classes at its output or not.
        /// </summary>
        public static bool multipleActivationSupport
        {
            get 
            {
                return false;
            }
        }


        private bool _multipleActivationEnabled;
        /// <summary>
        /// True if the pattern recognizer is performing pattern detection with multiple activation at the output.
        /// False otherwise.
        /// </summary>
        public bool multipleActivationEnabled
        {
            get
            {
                return _multipleActivationEnabled;
            }

            set 
            {
                if (_multipleActivationEnabled != value)
                {
                    _multipleActivationEnabled = value;
                    this.NotifyPropertyChanged("multipleActivationEnabled");
                }
            }
        }


        private double _activationLevel;
        /// <summary>
        /// Specifies the value which represents an activation on a given pattern recognizer.
        /// This should therefore be initialised by the constructor on the derived classes that use it. 
        /// </summary>
        public double activationLevel
        {
            get
            {
                return _activationLevel;
            }
            set
            {
                if (_activationLevel != value)
                {
                    _activationLevel = value;
                    this.NotifyPropertyChanged("activationLevel");
                }

            }
        }



        private double _activationTolerance;
        /// <summary>
        /// Specifies the maximum difference allowed between the ideal and the actual activation value so that
        /// it is still regarded as an activation.
        /// </summary>
        public double activationTolerance
        {
            get
            {
                return _activationTolerance;
            }
            set
            {
                if (_activationTolerance != value)
                {
                    _activationTolerance = value;
                    this.NotifyPropertyChanged("activationTolerance");
                }

            }
        }




        private ClassifToMovCodeConverter _converter;



        /// <summary>
        /// Performs initialization functions common to all constructors
        /// </summary>
        private void Init() 
        {
            this.PropertyChanged += PatternRecognizer_PropertyChanged;

            _progressLog = new ProgressLog();
            activationFunctionIdx = -1;
            normalizerIdx = -1;
            _converter = new ClassifToMovCodeConverter();
            activationTolerance = 0.1;
        }


        /// <summary>
        ///  
        /// </summary>
        /// <param name="newTrainingPackage">Reference to the TrainingPackage instance that the PatternRecognizer will use for training and verification.</param>
        public PatternRecognizer(TrainingPackage newTrainingPackage)
        {
            Init();
            _trainingPackage = newTrainingPackage;

        }


        public PatternRecognizer()
        {
            Init();

            _trainingPackage = null;
            _inputDim = 0;
            _outputDim = 0;

            
        }


        void PatternRecognizer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "multipleActivationEnabled":
                    if (_converter != null)
                        _converter.multipleActivation = multipleActivationEnabled;
                    break;

                case "activationFunctionIdx":
                    activationLevel = activationFunction.activationLevel;
                    break;

                case "activationLevel":
                    _converter.activationLevel = activationLevel;
                    break;
                    
                case "activationTolerance":
                    _converter.activationTolerance = activationTolerance;
                    break;

                case "trainingPackage":

                    if (trainingPackage != null)
                    {
                        outputDim = trainingPackage.movementCodes.Count;
                        int nfeatures = trainingPackage.trainingSets[0].set[0].features.Count;
                        inputDim = nfeatures * (int)trainingPackage.recordingConfig.activeChannels;
                        _converter.movementCodes = trainingPackage.movementCodes;
                    }
                    break;

                default:
                    break;
            }
        }



        /// <summary>
        /// Performs a training using the training sets. 
        /// For supervised trainings, remember that the movement Codes at the trainingPackage must be consulted 
        /// to determine what each classification output component means.
        /// </summary>
        public abstract void RunTraining();

        /// <summary>
        /// Validates a training using the validation sets
        /// </summary>
        public void RunValidation()
        {
            int okCount = 0;
            int failCount = 0;
            int expected;
            int predicted;


            Dictionary<int, Dictionary<int, int>> confusionMatrix = new Dictionary<int, Dictionary<int, int>>();
            Dictionary<int, int> predictedDictionary;



            progressLog.Log(ProgressLogItem.Info, "*********** VALIDATION ************");

            foreach (DataSet dataSet in trainingPackage.validationSets)
            {
                expected = dataSet.movementCode;

                okCount = 0;
                failCount = 0;

                foreach (DataWindow dataWindow in dataSet.set)
                {
                    double[] inputVector = new double[inputDim];
                    int inputPos = 0;

                    for (int j = 0; j < dataWindow.features.Values.Count; j++)
                    {
                        double[] channelVector = (double[])dataWindow.features.Values.ElementAt(j);

                        for (int k = 0; k < channelVector.Length; k++)
                        {
                            inputVector[inputPos] = channelVector[k];
                            inputPos++;
                        }
                    }


                    double[] outputVector = (double[])Classify(inputVector);

                    //Here we must determine which movement code corresponds better 
                    //with the input vector. The method for doing this changes depending on 
                    //whether we are using explicit movement codes or not, so we implement it using a converter class

                    predicted = (int) _converter.Convert(outputVector, null, null, null);
                    

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
                        predictedDictionary = new Dictionary<int,int>();
                        predictedDictionary.Add(predicted,1);
                        confusionMatrix.Add(expected, predictedDictionary);
                    }

                
                }

                string report = "Testing movement <mov " + expected + "/> -> correct: " + Math.Round(100 * okCount / (double)(okCount + failCount),2)+"%";

                confusionMatrix.TryGetValue(expected, out predictedDictionary);
                if (predictedDictionary.Keys.Count > 1)
                {
                    report = report +"; misclassified as: ";
                    foreach (int prediction in predictedDictionary.Keys)
                    {
                        if (prediction != expected)
                        {
                            report = report + "<mov " + prediction + "/> " + Math.Round(100 * predictedDictionary[prediction] / (double)(okCount + failCount),2) + "%; ";
                        }

                    }
                }

                progressLog.Log(ProgressLogItem.Info, report);
            }

            progressLog.Log(ProgressLogItem.Info, "********* VALIDATION FINISHED *********");

        }


        /// <summary>
        /// Performs classification of an input feature vector and returns the category the vector 
        /// has been classified into. 
        /// </summary>
        /// <param name="inputVector"></param>
        /// <returns></returns>
        public abstract object Classify(object inputVector);

        /// <summary>
        /// Resets the PatternRecognizer to an untrained status
        /// </summary>
        public abstract void ClearTraining();


       
    }
}
