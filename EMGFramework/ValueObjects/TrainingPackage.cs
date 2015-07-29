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
using EMGFramework.DataProvider;



namespace EMGFramework.ValueObjects
{
    /// <summary>
    /// Contains a training set and a validation set of DataWindows for each one of a number of movements 
    /// </summary>
    public class TrainingPackage : INotifyPropertyChanged
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


        private List<DataSet> _trainingSets;
        public List<DataSet> trainingSets
        {
            get
            {
                return _trainingSets;
            }
        }


        private List<DataSet> _validationSets;
        public List<DataSet> validationSets
        {
            get
            {
                return _validationSets;
            }
        }


        private RecordingConfig _recordingConfig;
        public RecordingConfig recordingConfig
        {
            get
            {
                return _recordingConfig;
            }

            set
            {
                if (value != null)
                {
                    if (_recordingConfig == null)
                        _recordingConfig = new RecordingConfig();

                    _recordingConfig.Copy(value);
                }
                else _recordingConfig = value;

                this.NotifyPropertyChanged("recordingConfig");
            }
        }


        private TreatmentConfig _treatmentConfig;
        public TreatmentConfig treatmentConfig
        {
            get
            {
                return _treatmentConfig;
            }

            set
            {
                if (value != null)
                {
                    if (_treatmentConfig == null)
                        _treatmentConfig = new TreatmentConfig();

                    _treatmentConfig.Copy(value);

                }
                else _treatmentConfig = value;
                this.NotifyPropertyChanged("treatmentConfig");
            }
        }

        private List<int> _movementCodes;
        /// <summary>
        /// List of the movements incuded in the training and validation sets
        /// </summary>
        public List<int> movementCodes
        {
            get 
            {
                return _movementCodes;
            }
        }

        /// <summary>
        /// Performs a copy of the configuration values only, not copying any data.
        /// </summary>
        /// <param name="source"></param>
        public void Copy(TrainingPackage source)
        {
            recordingConfig = source.recordingConfig;
            treatmentConfig = source.treatmentConfig;
            movementCodes.Clear();
            foreach (int item in source.movementCodes)
                movementCodes.Add(item);
            
        }



        public TrainingPackage()
        {
            _trainingSets = new List<DataSet>();
            _validationSets = new List<DataSet>();
            _movementCodes = new List<int>();
        }


        public TrainingPackage(RecordingConfig configuration)
        {
            _trainingSets = new List<DataSet>();
            _validationSets = new List<DataSet>();
            _movementCodes = new List<int>();
            recordingConfig = configuration;
        }


    }
}
