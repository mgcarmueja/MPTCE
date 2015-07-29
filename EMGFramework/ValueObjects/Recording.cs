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
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMGFramework.DataProvider;

namespace EMGFramework.ValueObjects
{
    /// <summary>
    /// Implements the structure of a complete recording, with the sequence of data objects, recording parameters 
    /// and recording duration in seconds.
    /// </summary>
    public class Recording
    {


        public Recording(ObservableCollection<Frame> data, RecordingConfig parameters)
        {
            _data =  data;
            _parameters = parameters;
        }

        public Recording(RecordingConfig parameters)
        {
            _data = new ObservableCollection<Frame>();
            _parameters = parameters;
        }

        public Recording(ObservableCollection<Frame> data)
        {
            _data = data;
            _parameters = new RecordingConfig();
        }

        public Recording()
        {
            _data = new ObservableCollection<Frame>();
            _parameters = new RecordingConfig();
        }

        //Clears the recording, recursovely calling the Clear method of ist list-based components
        public void Clear()
        {
            this.data.Clear();
            this.parameters.Clear();
        }


        private ObservableCollection<Frame> _data;
        /// <summary>
        /// A list of frames that constitute the content of the recording.
        /// </summary>
        public ObservableCollection<Frame> data
        {
            get
            {
                return _data;
            }

            set
            {
                _data = value;
            }

        }

        private RecordingConfig _parameters;
        /// <summary>
        /// Configuration used to acquire the data
        /// </summary>
        public RecordingConfig parameters
        {
            get
            {
                return _parameters;
            }

            set
            {
                _parameters = value;
            }
        }

        /// <summary>
        /// Total duration of the recording in seconds
        /// </summary>
        public double duration
        {
            get
            {
                if ((_data != null) && (parameters != null))
                {
                    if ((parameters.sampleFreq > 0) && (data.Count > 0))
                        return data.Count / (double)parameters.sampleFreq;
                    else if ((parameters.scheduleItemTime > 0) && ((parameters.scheduleLength-parameters.scheduleWarmupItems) > 0))
                        return parameters.scheduleItemTime * (parameters.scheduleLength-parameters.scheduleWarmupItems);
                    else return 0;
                }
                else return 0;
            }

        }

    }
}
