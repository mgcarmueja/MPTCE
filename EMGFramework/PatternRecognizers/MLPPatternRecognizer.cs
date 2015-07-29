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
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMGFramework.ValueObjects;
using EMGFramework.Utility;
using Encog.Engine.Network.Activation;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.Train;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training.Propagation.Back;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Neural.Networks.Training.Lma;



namespace EMGFramework.PatternRecognizers
{
    /// <summary>
    /// Class for pattern recognition using Linear Discriminant Analysis
    /// </summary>
    public class MLPPatternRecognizer : PatternRecognizer
    {
        private List<double[,]> _dataArrayList;

        private double[][] trainingInput;
        private double[][] expectedOutput;




        new public static string ID
        {
            get
            {
                return "MLP";
            }
        }


        new public static string displayName
        {
            get { return "Multilayer perceptron"; }
        }


        /// <summary>
        /// This static property should be redefined in any children class to indicate whether the pattern recognizer
        /// supports the simultaneous activation of several classes at its output or not.
        /// </summary>
        new public static bool multipleActivationSupport
        {
            get
            {
                return true;
            }
        }




        private int _maxEpochs;
        /// <summary>
        /// Absolute maximum number of epochs that the training algorithm should run.
        /// a value of 0 or lower means no maximum.
        /// </summary>
        public int maxEpochs
        {
            get
            {
                return _maxEpochs;
            }

            set
            {
                if (_maxEpochs != value)
                {
                    _maxEpochs = value;
                    NotifyPropertyChanged("maxEpochs");
                }
            }
        }



        private double _targetError;
        /// <summary>
        /// Error the classifier should reach before it is considered trained.
        /// </summary>
        public double targetError
        {
            get
            {
                return _targetError;
            }

            set
            {
                if (_targetError != value)
                {
                    _targetError = value;
                    NotifyPropertyChanged("targetError");
                }
            }
        }



        private int _hiddenLayerSize;
        /// <summary>
        /// Defines the sie of the hidden layer in the neural network
        /// </summary>
        public int hiddenLayerSize
        {
            get
            {
                return _hiddenLayerSize;
            }

            set
            {
                if (_hiddenLayerSize != value)
                {
                    _hiddenLayerSize = value;

                    NotifyPropertyChanged("hiddenLayerSize");
                }
            }
        }



        private bool _autoHiddenLayerSize;
        /// <summary>
        /// True if the size of the hidden layer in the neural network should be calculated 
        /// automatically (as a function of the input dimension), false otherwise
        /// </summary>
        public bool autoHiddenLayerSize
        {
            get
            {
                return _autoHiddenLayerSize;
            }
            set 
            {
                if (_autoHiddenLayerSize != value)
                {
                    _autoHiddenLayerSize = value;
                    this.NotifyPropertyChanged("autoHiddenLayerSize");
                }
            }
        }



        private int _hiddenLayerSizeFactor;
        /// <summary>
        /// Factor by which the input dimension should be multiplied to determine the size of the hidden neuron layer
        /// when autoHiddenLayerSize is set to true
        /// </summary>
        public int hiddenLayerSizeFactor
        {
            get
            {
                return _hiddenLayerSizeFactor;
            }

            set
            {
                if (_hiddenLayerSizeFactor != value)
                {
                    _hiddenLayerSizeFactor = value;
                    this.NotifyPropertyChanged("hiddenLayerSizeFactor");
                }
            }
        }
       


        private int _trainingProgressStride;
        /// <summary>
        /// Determines how often in number of generations the training progress will be reported
        /// </summary>
        public int trainingProgressStride
        {
            get
            {
                return _trainingProgressStride;
            }

            set 
            { 
                if(_trainingProgressStride!=value)
                {
                    _trainingProgressStride = value;
                    NotifyPropertyChanged("trainingProgressStride");

                }
            }
        }


        /// <summary>
        /// This is thehidden layer size that actually will be used to configure the neural network.
        /// The origin of its value will vary depending on whether it is automatically calculated
        /// or predefined.
        /// </summary>
        private int _actualHiddenLayerSize;



        /// <summary>
        /// Neural network used by the MLP pattern recognizer
        /// </summary>
        private BasicNetwork _network;


        /// <summary>
        /// Minimum activation level adatped from that presented by the selected activation function
        /// </summary>
        private double _activationMin;


        /// <summary>
        /// Maximum activation level adatped from that presented by the selected activation function
        /// </summary>
        private double _activationMax;



        /// <summary>
        /// Common initialization for all constructors
        /// </summary>
        private void Init()
        {
            _supportedActivationFunctions = new List<string> { "EncogLog", "EncogTANH", "EncogSoftMax", "EncogSigmoid" };
            _supportedNormalizers = new List<string> { "0 to 1", "-1 to 1" };
            maxEpochs = 10000;
            targetError = 0.0001;
            hiddenLayerSizeFactor = 3;
            hiddenLayerSize = inputDim * hiddenLayerSizeFactor;
            autoHiddenLayerSize = true;
            trainingProgressStride = 10;    
        }


        public MLPPatternRecognizer(TrainingPackage trainingPackage)
            : base(trainingPackage)
        {
            Init();
        }


        public MLPPatternRecognizer()
            : base()
        {

            Init();
        }


        /// <summary>
        /// Takes the trainigPackage and generates a list of bidimensional arrays. Each array containing the feature vectors
        /// for one trainingSet;
        /// </summary>
        private void DataAsArrayList()
        {
            DataWindow currentWindow;

            _dataArrayList = new List<double[,]>();

            foreach (DataSet trainingSet in trainingPackage.trainingSets)
            {
                double[,] currentArray = new double[trainingSet.set.Count, base.inputDim];

                _dataArrayList.Add(currentArray);


                //And now we fill it up with data!

                for (int i = 0; i < trainingSet.set.Count; i++)
                {
                    currentWindow = trainingSet.set.ElementAt(i);

                    for (int j = 0; j < currentWindow.features.Values.Count; j++)
                    {
                        double[] channelVector = (double[])currentWindow.features.Values.ElementAt(j);
                        for (int k = 0; k < channelVector.Length; k++)
                            currentArray[i, (j * channelVector.Length) + k] = channelVector[k];
                    }
                }
            }
        }



        /// <summary>
        /// Generates the arrays with input values and expected output values for the Encog training process,
        /// using as inputs the training sets stored in the training package
        /// </summary>
        private void FillTrainingArrays()
        {
            int nSamples = 0;

            //Turning the training sets of the training package into a list of two-dimensional arrays
            //and normalizing it using the offline method of the current normalizer
            DataAsArrayList();
            normalizer.RunOffline(_dataArrayList);


            //First, we count how many samples trainingset and expted output arrays will have
            foreach (double[,] dataArray in _dataArrayList)
                nSamples += dataArray.GetLength(0);


            trainingInput = new double[nSamples][];
            expectedOutput = new double[nSamples][];

            int pos = 0;
            int setNumber = 0;

            foreach (double[,] dataArray in _dataArrayList)
            {

                //Creating the expected output -- This should be done only once for each trainingSet!
                double[] tempExpected = new double[outputDim];

                for (int i = 0; i < outputDim; i++) tempExpected[i] = _activationMin;

                //DataSet trainingSet = trainingPackage.trainingSets[setNumber];

                tempExpected[setNumber] = activationLevel;

                setNumber++;

                int dataArrayLength = dataArray.GetLength(0);

                for (int i = 0; i < dataArrayLength; i++)
                {
                    trainingInput[pos] = new double[inputDim];
                    expectedOutput[pos] = tempExpected;

                    for (int j = 0; j < inputDim; j++)
                    {
                        trainingInput[pos][j] = dataArray[i, j];
                    }

                    pos++;
                }
            }
        }


        /// <summary>
        /// Initializes the neural network prior to the training. The actual activation output 
        /// for the output layer and its maximum and minimum levels are also defined here.
        /// </summary>
        /// <returns></returns>
        private BasicNetwork PrepareNetwork()
        {

            IActivationFunction outputLayerActivation;

            EncogActivation encogActivation = (EncogActivation)base.activationFunction;

            _activationMin = base.activationFunction.activationMin;
            _activationMax = base.activationFunction.activationMax;

            // Checking if the selected activation function is adequate for the
            // output layer and changing it if needed. The activation function in the
            // last neuron layer should produce outputs between [0,1] or [-1,1].

            if (_activationMin < -1 && _activationMax > 1)
            {
                //We will use a TANH activation function for the output layer
                outputLayerActivation = new ActivationTANH();
                _activationMin = -1;
                _activationMax = 1;
            }

            else if (_activationMin == 0 && _activationMax > 1)
            {
                //We will use a Sigmoid activation function for the output layer
                outputLayerActivation = new ActivationSigmoid();
                _activationMin = 0;
                _activationMax = 1;
            }

            else
            {
                outputLayerActivation = ((EncogActivation)base.activationFunction).CreateInstance();      
            }



            BasicNetwork network = new BasicNetwork();

            
            if (autoHiddenLayerSize)
                _actualHiddenLayerSize = inputDim * hiddenLayerSizeFactor;
            else _actualHiddenLayerSize = hiddenLayerSize;


            network.AddLayer(new BasicLayer(null, true, inputDim));
            network.AddLayer(new BasicLayer(encogActivation.CreateInstance(), true, _actualHiddenLayerSize));
            network.AddLayer(new BasicLayer(outputLayerActivation, false, outputDim));
            network.Structure.FinalizeStructure();
            network.Reset();

            return network;

        }



        /// <summary>
        /// Trains the Multilayer Perceptron. We won't be using backpropagation though, but Resilient Propagation.
        /// According to Jeff Heaton in https://s3.amazonaws.com/heatonresearch-books/free/Encog3CS-User.pdf, 
        /// it is the best training algorithm for feedforward neural networks provided by Encog. The original
        /// backpropagation algorithm is, according to the author, added only for historical reasons.
        /// </summary>
        public override void RunTraining()
        {

            interruptTraining = false;

            /*
             * Create an initialise our feedforward neural network. We will be using one hidden layer and
             * bias for all except the output layer. The hidden layer will have twice the number of PEs as
             * the input layer.
             */

            _network = PrepareNetwork();

            // Now we set up the training set and training algorithm

            FillTrainingArrays();
            IMLDataSet trainingSet = new BasicMLDataSet(trainingInput, expectedOutput);

            IMLTrain train = new ResilientPropagation(_network, trainingSet);

            //This training method is apparently what Matlab uses, but doesn't work right now.
            //LevenbergMarquardtTraining train = new LevenbergMarquardtTraining(_network, trainingSet);

            // And finally, we perform the training

            progressLog.Log(ProgressLogItem.Info,"Training up to a maximum of "+ maxEpochs +" epochs");
            progressLog.Log(ProgressLogItem.Info,"MLP target error: " + targetError);
            progressLog.Log(ProgressLogItem.Info, "Hidden layer size: " + _actualHiddenLayerSize);


            progressLog.Log(ProgressLogItem.Info, "*********** TRAINING ************");

            int epoch = 1;

            do
            {
                train.Iteration();
                if(epoch%trainingProgressStride==0)
                progressLog.Log(ProgressLogItem.Info, @"Epoch #" + epoch + @" Error: " + Math.Round(train.Error, 8));
                epoch++;
            } while ((double.IsNaN(train.Error) || (train.Error > targetError && !interruptTraining)) && (maxEpochs <= 0 || epoch <= maxEpochs));

            progressLog.Log(ProgressLogItem.Info, "Training finished after " + (epoch - 1) + " epochs. Last error: " + Math.Round(train.Error, 8));

            if (!interruptTraining)
            {
                if (train.Error <= targetError)
                    progressLog.Log(ProgressLogItem.Info, "================== TARGET ERROR REACHED ===================");
                else
                    progressLog.Log(ProgressLogItem.Info, "================= MAXIMUM EPOCHS REACHED ==================");

            }
            else
                progressLog.Log(ProgressLogItem.Info, "============== TRAINING INTERRUPTED BY USER =============");

            interruptTraining = false;

            trained = true;


        }


        /// <summary>
        /// Performs classification of an input feature vector and returns the category the vector 
        /// has been classified into. 
        /// </summary>
        /// <param name="inputVector"></param>
        /// <returns></returns>
        public override object Classify(object inputVector)
        {
            //base.Classify();


            //TODO: Use the matrices initialized during training to calculate the discriminant function 
            //and DON'T FORGET to use the activation function if available!

            BasicMLData input = new BasicMLData((double[])inputVector);

            //First, we normalize the input vector using the online normalization function 
            normalizer.RunOnline(input.Data);

            IMLData temp = _network.Compute(input);

            double[] output = new double[temp.Count];
            temp.CopyTo(output, 0, temp.Count);

            return output;
        }


        /// <summary>
        /// We don't do very much in this implementation, as any training information will be
        /// overwritten by a subsequent training. The PatternRecognizer is flagged as untrained though.
        /// </summary>
        public override void ClearTraining()
        {
            trained = false;
        }


    }
}
