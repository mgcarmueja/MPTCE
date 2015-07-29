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

namespace EMGFramework.ValueObjects
{
    /// <summary>
    /// A set of thresholds per channel. For a given movement, these thresholds define the maximum, minimum and
    /// average values in each channel as a way of characterizing it
    /// </summary>
    public class ThresholdSet:INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


        private uint _nChannels;
        public uint nChannels
        {
            get 
            {
                return _nChannels;
            }

            set 
            {
                if (_nChannels != value)
                {
                    _nChannels = value;
                    this.NotifyPropertyChanged("nChannels");
                }
            }
        }

        public double[] _variances;
        /// <summary>
        /// Array holding the variance for each channel
        /// </summary>
        public double[] variances
        {
            get
            {
                return _variances;
            }
        }


        /// <summary>
        /// Array holding the standard deviation for each channel
        /// </summary>
        public double[] _stdDevs;
        public double[] stdDevs
        {
            get
            {
                return _stdDevs;
            }
        }


        private double[] _maxValues;
        /// <summary>
        /// An array holding the detected maximum values for each channel 
        /// </summary>
        public double[] maxValues
        {
            get
            {
                return _maxValues;
            }
        }

        private double[] _minValues;
        /// <summary>
        /// An array holding the detected minimum values for each channel 
        /// </summary>
        public double[] minValues
        {
            get
            {
                return _minValues;
            }
        }


        private double[] _avgValues;
        /// <summary>
        /// An array holding the detected average values for each channel 
        /// </summary>
        public double[] avgValues
        {
            get
            {
                return _avgValues;
            }
        }


        private int _movementCode;
        /// <summary>
        /// Code of the movement for which thresholds were calculated.
        /// </summary>
        public int movementCode
        {
            get
            {
                return _movementCode;
            }

            set
            {
                _movementCode = value;
                this.NotifyPropertyChanged("movementCode");
            }
        }

        private int[] _sampleCounter;


        void ThresholdSet_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "nChannels":
                    Init(nChannels);
                    break;

                default:
                    break;
            }
        }


        private void Init(uint nChannels)
        {
            _variances = new double[nChannels];
            _stdDevs = new double[nChannels];
            _maxValues = new double[nChannels];
            _minValues = new double[nChannels];
            _avgValues = new double[nChannels];
            _sampleCounter = new int[nChannels];
        }



        /// <summary>
        /// Initializes a new ThresholdSet instance
        /// </summary>
        /// <param name="nChannels"></param>
        /// <param name="movCode"></param>
        public ThresholdSet(uint nChannels, int movCode)
        {
            Init(nChannels);
            _movementCode = movCode;
            this.PropertyChanged += ThresholdSet_PropertyChanged;
        }



        public ThresholdSet(uint nChannels)
        {
            Init(nChannels);
            _movementCode = -1;
            this.PropertyChanged += ThresholdSet_PropertyChanged;
        }


        /// <summary>
        /// Adds a new value to the average value for a given channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="sample"></param>
        public void AddToAverage(uint channel, double sample)
        {
            _sampleCounter[channel]++;

            _avgValues[channel] = (_avgValues[channel] * ((_sampleCounter[channel] - 1) / (double)(_sampleCounter[channel]))) 
                                    + (sample / (double)(_sampleCounter[channel]));
        }


        /// <summary>
        /// Copies the contents of the source ThresholdSet into the one invoking the method
        /// It only will work well with two ThresholdSet onjects with the same number of channels
        /// </summary>
        /// <param name="source"></param>
        public void Copy(ThresholdSet source)
        {
            if (source == null) return;

            int i;
            int length = source.avgValues.Length;

            for (i=0;i<length;i++)
            {
                this.avgValues[i] = source.avgValues[i];
                this.maxValues[i] = source.maxValues[i];
                this.minValues[i] = source.minValues[i];
                this.stdDevs[i] = source.stdDevs[i];
                this.variances[i] = source.variances[i];
            }

            this.movementCode = source.movementCode;
        }

        /// <summary>
        /// Clears all the statistics stored on the ThresholdSet object without changing the number of channels
        /// </summary>
        public void ClearStats()
        {
            for (uint i = 0; i < nChannels; i++)
            {
                avgValues[i] = 0;
                maxValues[i] = 0;
                minValues[i] = 0;
                stdDevs[i] = 0;
                variances[i] = 0;
            }
        }

    }
}
