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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMGFramework.Pipelines;
using EMGFramework.ValueObjects;


namespace EMGFramework.Utility
{
    /// <summary>
    /// An intermediate pipeline stage implementing a value shifter. This stage will also detect the overall
    /// maximum and minimum values for each channel The maximum values will be used together with the maximum
    /// noise amplitude detected in other stage to establish the limits for threshold definition.
    /// </summary>
    public class ThresholdEngine : Stage
    {
        public RecordingConfig recordingConfig { set; private get; }



        private double _lowerPercentage;
        /// <summary>
        /// Percentage of the signal amplitude that will be excluded from average. Used to skip rests.
        /// </summary>
        public double lowerPercentage
        {
            get
            {
                return _lowerPercentage;
            }

            set
            {
                if (_lowerPercentage != value)
                {
                    _lowerPercentage = value;
                }
            }
        }


        //private bool _firstRun = true;

        /// <summary>
        /// If true, the incoming signal will be rectified (converted into absolute values). 
        /// If false, the 0 level of the signal will be shifted, so that all negative values are also positive, but the 
        /// waveform will be preserved.
        /// </summary>
        public bool rectify = true;



        /// <summary>
        /// It determines whether the ThresholdEngine is active or not. If set to false, the stage will just
        /// forward the incoming data.
        /// </summary>
        public bool enabled = true;



        /// <summary>
        /// True if this stage is being used for online signal processing. False if it is being used during
        /// recording.
        /// </summary>
        public bool isOnline = false;


        private ThresholdSet _thresholdSet;


        private double[,] _movingWindow;
        


        private int _movingWindowStart;



        private uint _windowLength;
        /// <summary>
        /// Size of the window holding the last frames processed by this stage. This window can be used to 
        /// determine the running maximum, running average, etc. of the signal, which are used in the process 
        /// of determining upper and lower values for threshold setting.
        /// </summary>
        public uint windowLength
        {
            get
            {
                return _windowLength;
            }

            set
            {
                if (_windowLength != value)
                {
                    _windowLength = value;
                }
            }
        }



        /// <summary>
        /// Delegate for defining processing functions
        /// </summary>
        /// <param name="movingWindow"></param>
        /// <returns></returns>
        private delegate void ProcessingFunction(double[,] movingWindow, double[] output);



        /// <summary>
        /// processing function to be used
        /// </summary>
        private ProcessingFunction _processingFunction;



        /// <summary>
        /// Stores the processed frames for a post-processing task that will be run when 
        /// this pipeline stage is stopped.
        /// </summary>
        private List<Frame> _postProcessingBuffer;


        private BlockingCollection<object> _dataMonitor;
        /// <summary>
        /// BlockingCollection that can be used by another process process to monitor the output data produced
        /// </summary>
        public BlockingCollection<object> dataMonitor
        {
            get
            {
                return _dataMonitor;
            }
        }



        /// <summary>
        /// True if someone will monitor the data coming out of this stage with the dataMonitor collection,
        /// false otherwise.
        /// </summary>
        public bool monitoring;



        public ThresholdEngine()
            : base(2, true, true)
        {
            _postProcessingBuffer = new List<Frame>();

            //By default, we'll be ignoring the lower 5% of the EMG signal amplitude
            lowerPercentage = 0.05;

            //Using a default processing window of 100 samples
            windowLength = 100;
        }



        public override void Init()
        {
            base.Init();
            //_firstRun = true;

            _postProcessingBuffer.Clear();

            _thresholdSet = recordingConfig.thresholdSet;
            _movingWindow = new double[windowLength, recordingConfig.nChannels];
            _movingWindowStart = 0;
            _processingFunction = new ProcessingFunction(GetRMS);

            _dataMonitor = new BlockingCollection<object>();
        }



        public override void Stop()
        {
            //Here we do things like calculating activation thresholds for each channel 
            //Disabled due to lack of a valid theoretical basis
            //if (enabled && !isOnline) Postprocess();

            _dataMonitor.CompleteAdding();

            base.Stop();
        }



        /// <summary>
        /// Brings signal values into positive realm, processes the signal to eliminate anything but local 
        /// peak values and prepares the data to statistically determine which interval of values is a valid
        /// range for defining an activation threshold.
        /// </summary>
        /// <param name="inputItem">an input Frame object</param>
        /// <param name="outputItem">an output Frame object</param>
        protected override void TaskIntermediate(object inputItem, out object outputItem)
        {
            double value, absMin, absMax;


            Frame myItem = (Frame)inputItem;

            if (enabled)
            {
                if (rectify)
                {
                    for (uint i = 0; i < myItem.nsamples; i++)
                    {
                        value = Math.Abs(myItem.samples[i]);

                        myItem.samples[i] = value;

                        absMin = Math.Abs(myItem.minVal);
                        absMax = Math.Abs(myItem.maxVal);
                        if (absMin > absMax) myItem.maxVal = absMin;
                        else myItem.maxVal = absMax;
                    }
                }
                else
                {
                    myItem.minVal = -1 * myItem.minVal;

                    for (uint i = 0; i < myItem.nsamples; i++)
                    {
                        myItem.samples[i] += myItem.minVal;
                    }

                    myItem.maxVal += myItem.minVal;
                    myItem.minVal = 0;
                }
                
                AddToMovingWindow(myItem.samples);
                _processingFunction(_movingWindow, myItem.samples);

                //Detecting the maximum value of the processed frame for each channel
                //and defining a new maximum and minimum for each frame
                myItem.minVal = myItem.samples[0];
                myItem.maxVal = myItem.samples[0];

                if (!isOnline)
                for (uint i = 0; i < myItem.nsamples; i++)
                {
                        if (myItem.samples[i] > _thresholdSet.maxValues[i])
                            _thresholdSet.maxValues[i] = myItem.samples[i];

                        if ((myItem.samples[i]) > myItem.maxVal) myItem.maxVal = myItem.samples[i];
                        else if ((myItem.samples[i]) < myItem.minVal) myItem.minVal = myItem.samples[i];
                }

            }

            outputItem = myItem;

           if(isOnline && !_dataMonitor.IsAddingCompleted && monitoring)
                _dataMonitor.Add(outputItem);
        }


        
        /// <summary>
        /// It performs calculations that can only be done after the recording has ended, such as calculating
        /// the standard deviation and variance to select upper and lower limits for the threshold. The use of this
        /// function was disabled on the final version because the theoretical basis of its workings is not correct.
        /// </summary>
        private void Postprocess()
        {
            int length = _postProcessingBuffer.Count;
            double[] lowerLimit;

            lowerLimit = new double[_postProcessingBuffer.First().nsamples];

            //Calculation of the average value excluding a defined lower percentage of the signal amplitude. 
            // This is a way to exclude eventual rest periods.

            for (uint i = 0; i < _postProcessingBuffer.First().nsamples; i++)
            {
                lowerLimit[i] = _thresholdSet.minValues[i] +
                                (_thresholdSet.maxValues[i] - _thresholdSet.minValues[i]) * lowerPercentage;

                foreach (Frame item in _postProcessingBuffer)
                {
                    if (item.samples[i] > lowerLimit[i])
                        _thresholdSet.AddToAverage(i, item.samples[i]);
                }
            }


            //Variace and standard deviation 
            foreach (Frame item in _postProcessingBuffer)
            {
                for (uint i = 0; i < item.samples.Length; i++)
                {
                    if (item.samples[i] > lowerLimit[i])
                    {
                        double temp = item.samples[i] - _thresholdSet.avgValues[i];
                        double sqTemp = temp * temp;

                        _thresholdSet.variances[i] += (sqTemp) / (double)length;
                    }
                }
            }


            for (int i = 0; i < _thresholdSet.variances.Length; i++)
                _thresholdSet.stdDevs[i] += Math.Sqrt(_thresholdSet.variances[i]);


            //Now, we define extreme values as those being distanced from the mean more than three times the 
            //standard deviation. If the data follow a normal distribution, 99.7% of all samples should fall 
            //within this interval (http://en.wikipedia.org/wiki/Standard_deviation)

            for (int i = 0; i < _thresholdSet.avgValues.Length; i++)
                _thresholdSet.maxValues[i] = _thresholdSet.avgValues[i] + 3 * _thresholdSet.stdDevs[i];

            for (int i = 0; i < _thresholdSet.avgValues.Length; i++)
            {
				_thresholdSet.minValues [i] = lowerLimit [i];

			}

        }


        /// <summary>
        /// Puts the data array of a Frame object into the moving window
        /// and changes the start position of the latter
        /// </summary>
        /// <param name="element"></param>
        private void AddToMovingWindow(double[] element)
        {
            for (int j = 0; j < recordingConfig.nChannels; j++)
                _movingWindow[_movingWindowStart, j] = element[j];

            _movingWindowStart = (int)((_movingWindowStart + 1) % windowLength);

        }


        /*
         * Some testing processing functions to be used for processing the signal before a threshold detection is attempted
         * 
         */

        /// <summary>
        /// Calculates the maximum value for each channel of the values inside the moving window
        /// </summary>
        /// <param name="movingWindow"></param>
        /// <param name="output"></param>
        private void GetMaximum(double[,] movingWindow, double[] output)
        {
            double max;
            int i, j;


            for (j = 0; j < recordingConfig.nChannels; j++)
            {
                max = movingWindow[0, j];

                for (i = 0; i < windowLength; i++)
                    if (max < movingWindow[i, j]) max = movingWindow[i, j];
                output[j] = max;

            }
        }


        /// <summary>
        /// Calculates the Root Mean Square for each channel of the values inside the moving window
        /// </summary>
        /// <param name="movingWindow"></param>
        /// <param name="output"></param>
        private void GetRMS(double[,] movingWindow, double[] output)
        {
            double rms;
            int i, j;

            for (j = 0; j < recordingConfig.nChannels; j++)
            {
                rms = 0;

                for (i = 0; i < windowLength; i++)
                    rms += ((movingWindow[i, j] * movingWindow[i, j]) / (double)windowLength);

                output[j] = Math.Sqrt(rms);

            }
        }


        /// <summary>
        /// Calculates the average value for each channel of the values inside the moving window
        /// </summary>
        /// <param name="movingWindow"></param>
        /// <param name="output"></param>
        private void GetAverage(double[,] movingWindow, double[] output)
        {
            double avg;
            int i, j;

            for (j = 0; j < recordingConfig.nChannels; j++)
            {
                avg = 0;

                for (i = 0; i < windowLength; i++)
                    avg += (movingWindow[i, j] / (double)windowLength);

                output[j] = avg;

            }

        }


        /// <summary>
        /// Calculates the integral value for each channel of the values inside the moving window
        /// </summary>
        /// <param name="movingWindow"></param>
        /// <param name="output"></param>
        private void GetIntegral(double[,] movingWindow, double[] output)
        {
            double sum;
            int i, j;

            for (j = 0; j < recordingConfig.nChannels; j++)
            {
                sum = 0;

                for (i = 0; i < windowLength; i++)
                    sum += movingWindow[i, j];

                output[j] = sum;

            }

        }


        /// <summary>
        /// Placeholder doing just nothing.
        /// </summary>
        /// <param name="movingWindow"></param>
        /// <param name="output"></param>
        private void DoNothing(double[,] movingWindow, double[] output)
        {

        }


    }




}
