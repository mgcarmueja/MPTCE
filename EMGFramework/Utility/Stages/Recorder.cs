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
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using EMGFramework.Pipelines;
using EMGFramework.DataProvider;
using EMGFramework.ValueObjects;




namespace EMGFramework.Utility
{  
    /// <summary>
    /// Final pipeline stage for storing the incoming frames 
    /// </summary>
    public class Recorder : Stage
    {
        private UInt64 _processedItems = 0;
        private RecordingConfig _recordingConfig = null;
        private Recording _recording;
        private ObservableCollection<Frame> _recordingData;
        private BlockingCollection<Object> _dataMonitor;
        private double _duration;

        private uint _indexCompensation;
        private double _timeCompensation;


        private double _min;
        private double _max;
        private bool _firstFrame;

        /// <summary>
        /// Event handler that fires whenever a property has been changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }



        public UInt64 processedItems
        {
            get { return _processedItems; }
        }



        /// <summary>
        /// Recording configuration typically copied from the DataProvider
        /// </summary>
        public RecordingConfig recordingConfig
        {
            get
            {
                return _recordingConfig;
            }

            set
            {
                _recordingConfig = value;
            }
        }

        public ObservableCollection<Frame> recordingData
        {
            get 
            {
                return _recordingData;
            }
        }

        public BlockingCollection<object> dataMonitor
        {
            get
            {
                return _dataMonitor;
            }
        }


        public Recording recording
        {
            get
            {
                return _recording;
            }
            set
            {
                if (_recording != value)
                {
                    _recording = value;
                    this.NotifyPropertyChanged("recording");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="recordingObject"></param>
        public Recorder(Recording recordingObject)
            :base(0,true,false)
        {
            _recording = recordingObject;
            _recordingConfig = _recording.parameters;
            _recordingData = _recording.data;
        }

        public override void Init()
        {
            base.Init();
            _dataMonitor = new BlockingCollection<object>();
            _indexCompensation = (uint) (_recordingConfig.scheduleWarmupItems * _recordingConfig.scheduleItemTime * _recordingConfig.sampleFreq);
            _timeCompensation = (double)(_indexCompensation / (double)_recordingConfig.sampleFreq);
        }


        protected override void TaskFinal(object inputItem)
        {

            Frame myItem = (Frame)inputItem;
            //Debug.Write("R");


            if (!dataMonitor.IsAddingCompleted)
            {
                //We compensate time and sequence indexes for the warmup schedule items
                myItem.sequenceIdx -= _indexCompensation;
                myItem.timeIdx -= _timeCompensation;

                //Getting maxvalue and minvalue ffrom the frame
                //to calculate the maxvoltage and minvoltage of the entire recording

                if (_firstFrame)
                {
                    _firstFrame = false;

                    _min = myItem.minVal;
                    _max = myItem.maxVal;
                }
                else
                {
                    if (myItem.minVal < _min) _min = myItem.minVal;
                    if (myItem.maxVal > _max) _max = myItem.maxVal;
                }

                _recordingData.Add(myItem);
                _dataMonitor.Add(inputItem);
                _duration = myItem.timeIdx - ((Frame)_recordingData.ElementAt(0)).timeIdx;

                _processedItems++;
            }
        }


        public override void Start()
        {
            _firstFrame = true;

            base.Start();
        }


        public override void Stop()
        {
            if (_min <= 0)
                _recording.parameters.minVoltage = _min;
            else _recording.parameters.minVoltage = 0;

            if (_max >= 0)
                _recording.parameters.maxVoltage = _max;
            else _recording.parameters.maxVoltage = 0;

            Debug.WriteLine("Stopping recording stage...");
            _dataMonitor.CompleteAdding();
            base.Stop();
            
        }







    }
}
