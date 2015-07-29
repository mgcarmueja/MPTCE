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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace EMGFramework.ValueObjects
{
    /// <summary>
    /// A class for storing the preferences for the treatment phase
    /// </summary>
    public class TreatmentConfig : INotifyPropertyChanged
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


        private double _samplesPermsec;
        

        private uint _sampleFreq;
        /// <summary>
        /// Sampling frequency in Hz. Used for internal calculations.
        /// </summary>
        public uint sampleFreq
        {
            get 
            {
                return _sampleFreq;
            }

            set 
            { 
                if(_sampleFreq != value)
                {
                    _sampleFreq = value;
                    _samplesPermsec = _sampleFreq / 1000.0;
                    this.NotifyPropertyChanged("sampleFreq");
                }
            }
        }


        private int _trainingSetSize;
        /// <summary>
        /// Size of the training set of each movement in windows
        /// </summary>
        public int  trainingSetSize
        {
            get 
            {
                return _trainingSetSize;
            }

            set 
            {
                if (_trainingSetSize != value)
                {
                    _trainingSetSize = value;
                    this.NotifyPropertyChanged("trainingSetSize");
                }
            }
        }


        private int _validationSetSize;
        /// <summary>
        /// Size of the validation set of each movement in windows
        /// </summary>
        public int validationSetSize
        {
            get 
            {
                return _validationSetSize;
            }
            
            set 
            {
                if (_validationSetSize != value)
                {
                    _validationSetSize = value;
                    this.NotifyPropertyChanged("validationSetSize");
                }
            }
        }


        private int _windowLength;
        /// <summary>
        /// Length of a sample window in frames
        /// </summary>
        public int windowLength
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
                    _windowLengthmsec = (int)(_windowLength / _samplesPermsec); 

                    this.NotifyPropertyChanged("windowLength");
                }
            }
        }



        private int _windowLengthmsec;
        /// <summary>
        /// Duration of a sample window in milliseconds
        /// </summary>
        public int windowLengthmsec
        {
            get
            {
                return _windowLengthmsec;
            }

            set
            {
                if (_windowLengthmsec != value)
                {
                    _windowLengthmsec = value;
                    _windowLength = (int)(_windowLengthmsec * _samplesPermsec);
                    this.NotifyPropertyChanged("windowLengthmsec");
                }
            }
        }


        private int _windowOffset;
        /// <summary>
        /// Offset between the starting samples of two consecutive windows in number of samples
        /// </summary>
        public int windowOffset
        {
            get 
            {
                return _windowOffset;
            }

            set
            {
                if (_windowOffset != value)
                {
                    _windowOffset = value;
                    _windowOffsetmsec = (int)(_windowOffset / _samplesPermsec);
                    this.NotifyPropertyChanged("windowOffset");
                }
            }
        }


        private int _windowOffsetmsec;
        /// <summary>
        /// Offset between the starting samples of two consecutive windows in milliseconds
        /// </summary>
        public int windowOffsetmsec
        {
            get
            {
                return _windowOffsetmsec;
            }

            set
            {
                if (_windowOffsetmsec != value)
                {
                    _windowOffsetmsec = value;
                    _windowOffset = (int)(_windowOffsetmsec * _samplesPermsec);
                    this.NotifyPropertyChanged("windowOffsetmsec");
                }
            }
        }

        /// <summary>
        /// Detected features;
        /// </summary>
        public List<string> features
        {
            get;
            set;
        }


        private float _contractionTimePercentage;
        /// <summary>
        /// Contraction time percentage expressed as a fraction of 1.  
        /// </summary>
        public float contractionTimePercentage
        {
            get
            {
                return _contractionTimePercentage;
            }
            set 
            {
                if (_contractionTimePercentage != value)
                {
                    _contractionTimePercentage = value;
                    this.NotifyPropertyChanged("contractionTimePercentage");
                }
            }

        }

        public void Copy(TreatmentConfig sourceConfig)
        {
            contractionTimePercentage = sourceConfig.contractionTimePercentage;

            if (features == null)
                features = new List<string>();
            else features.Clear();

            if(sourceConfig.features!=null)
            foreach (string item in sourceConfig.features)
            {
                features.Add(item); 
            }

            sampleFreq = sourceConfig.sampleFreq;
            trainingSetSize = sourceConfig.trainingSetSize;
            validationSetSize = sourceConfig.validationSetSize;
            windowLength = sourceConfig.windowLength;
            windowOffset = sourceConfig.windowOffset;
        }


        public TreatmentConfig()
        { 
        }

    }
}
