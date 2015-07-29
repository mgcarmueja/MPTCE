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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EMGFramework.Pipelines;
using EMGFramework.DataProvider;
using EMGFramework.ValueObjects;


namespace EMGFramework.Utility
{
    /// <summary>
    /// Event argument class for passing the current status of the RecordingController to the listener.
    /// </summary>
    public class RecordingStatusEventArgs : EventArgs
    {
        public int status { get; private set; }

        public RecordingStatusEventArgs(int statusValue)
            : base()
        {
            status = statusValue;
        }

    }


    /// <summary>
    /// Implementation of a mechanism for controlling the recording process from the first stage of a processing pipeline.
    /// It is designed as an intermediate stage which uses the output queue of a EMGDataProvider object as datasource. 
    /// </summary>
    public class AcquisitionController : Stage
    {

        private UInt64 _totalFrameCounter = 0; //Counts frames from start to stop
        private UInt64 _partialFrameCounter = 0; //Counts frames whithin the same schedule item (movement)
        private bool _ignoreData = false; //Set to true makes the RecordingController not to treat nor forward incoming data.
        private UInt16[] _scheduleItem;
        private ushort _movCode;


        /// <summary>
        /// An event clients can use to be notified whenever the status of the RecordingController changes
        /// due to transitions between schedule items. Implemented following the .NET Framework guidelines.
        /// Be careful though, because responses to events will run, as usual, in the same thread as the RecordingController
        /// object generating the event.
        /// </summary>
        public event EventHandler StatusChanged;


        /// <summary>
        /// Invoke the StatusChanged event; called whenever the status of the RecordingController changes
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnStatusChanged(EventArgs e)
        {
            if (StatusChanged != null)
                StatusChanged(this, e);
        }


        private bool _isOnline=false;
        /// <summary>
        /// True if this stage is being used online, false otherwise
        /// </summary>
        public bool isOnline
        {
            get
            {
                return _isOnline;
            }

            set
            {
                if (_isOnline != value)
                    _isOnline = value;
            }
        }


        private bool _isMonitored = false;
        /// <summary>
        /// Controls whether or not data should be sent to the data monitor
        /// </summary>
        public bool isMonitored
        {
            get
            {
                return _isMonitored;
            }

            set
            {
                if (_isMonitored != value)
                    _isMonitored = value;
            }
        }


        private int _currentScheduleItem = -1;
        /// <summary>
        /// It returns the index of the schedule item currently being used -1 indicates that the schedule is not being run
        /// </summary>
        public int currentScheduleItem
        {
            get
            {
                return _currentScheduleItem;
            }
        }


        private EMGDataProvider _dataProvider = null;
        public EMGDataProvider dataProvider
        {
            get
            {
                return _dataProvider;
            }

            set
            {
                _dataProvider = value;
            }
        }


        private Pipeline _homePipeline = null;
        /// <summary>
        /// A reference to the pipeline this RecordingController lives in.
        /// </summary>
        public Pipeline homePipeline
        {
            get
            {
                return _homePipeline;
            }

            set
            {
                _homePipeline = value;
            }
        }


        private RecordingConfig _recordingConfig = null;
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

        /// <summary>
        /// List of allowed complex movements encoded as strings
        /// </summary>
        public StringCollection allowedComplexMovements { get; set; }

        /// <summary>
        /// Number of existing single movements including rest
        /// </summary>
        public int numSingleMovements { get; set; }


        private BlockingCollection<int> _recordingEventQueue = null; //Allows ONE consumer to receive recording events
        /// <summary>
        /// Allows ONE consumer to receive recording events. This is the way we can communicate the occurrence of recording events 
        /// between the thread running the RecordingController and ONE consumer thread.
        /// </summary>
        public BlockingCollection<int> recordingEventQueue
        {
            get { return _recordingEventQueue; }
        }


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
        /// True if someone will monitor the data coming through this stage with the dataMonitor collection,
        /// false otherwise.
        /// </summary>
        public bool dataMonitoring;


        private BlockingCollection<int> _progressMonitor;
        /// <summary>
        /// Can be used by another thread to monitor the progress percentage of the current recording schedule item.
        /// </summary>
        public BlockingCollection<int> progressMonitor
        {
            get 
            {
                return _progressMonitor;
            }

        }

        //temporal variables for the progress percentage of the current recording item
        private int _progressPercentage = 0;
        private int _oldProgressPercentage = 0;

        //True if the noise detection thread is running. False otherwise,
        private bool _noiseDetectionActive;

        /// <summary>
        /// Task for the noise detection thread.
        /// </summary>
        private Task _noiseDetectionTask;

        /// <summary>
        /// Queue for the noise detection task to receive frames
        /// </summary>
        private BlockingCollection<object> _noiseDetectionQueue;


        public AcquisitionController(Pipeline homePipeline, EMGDataProvider sourceDataProvider)
            : base(2, true, true)
        {
            _homePipeline = homePipeline;
            _dataProvider = sourceDataProvider;
            CommonConstructorInit();
        }


        public AcquisitionController(Pipeline homePipeline)
            : base(2, true, true)
        {
            _homePipeline = homePipeline;
            CommonConstructorInit();
        }

        public AcquisitionController()
            : base(2, true, true)
        {
            CommonConstructorInit();
        }

        private void CommonConstructorInit()
        {
            _recordingEventQueue = new BlockingCollection<int>();
            _progressMonitor = new BlockingCollection<int>(); 
        }


        public override void Init()
        {
            base.Init();

            //Emptying the event queue in case nobody consumed it the last time
            for (int i = 0; i < _recordingEventQueue.Count; i++) _recordingEventQueue.Take();

            _dataProvider.recordingConfig = _recordingConfig;

            try
            {
                _dataProvider.Connect();
            }
            catch (DataProviderException ex)
            {
                throw new AcquisitionControllerException("EMGDataProvider.Connect:" + ex.Message);
            }

            _dataProvider.Init(); //So that we have an outputQueue to give away
            inputQueue = _dataProvider.outputQueue;

            _dataMonitor = new BlockingCollection<object>();

            _noiseDetectionQueue = new BlockingCollection<object>();
        }



        public override void Start()
        {
            if (!isOnline)
            {
                _totalFrameCounter = 0;
                _partialFrameCounter = 0;
                _ignoreData = false;
                _currentScheduleItem = 0;

                _scheduleItem = _recordingConfig.schedule.ElementAt(_currentScheduleItem).movementComposition;
                _movCode = GetMovementCode(_scheduleItem);
                _recordingEventQueue.Add(_currentScheduleItem);
                OnStatusChanged(new RecordingStatusEventArgs(_currentScheduleItem));
                _progressMonitor.Add(0);

                //Noise detection will remain disabled because it produces a too high 
                //lower limit for threshold detection when recording very weak signals
                //_noiseDetectionActive = true;
                
                _noiseDetectionTask = Task.Run(new Action(NoiseDetection));

            }

            base.Start();

            //Here we start the EMGDataProvider
            _dataProvider.Start();
        }


        public override void Stop()
        {
            //First we stop the EMGDataProvider to shut down the flow of data

            //DEBUG
            Debug.WriteLine("Stopping AcquisitionController...");
            //DEBUG/

            _dataProvider.Stop();
            _dataProvider.Disconnect();

            if (!isOnline)
            {
                _recordingEventQueue.Add(-2);
                _progressMonitor.Add(0);
            }

            _dataMonitor.CompleteAdding();
            _noiseDetectionQueue.CompleteAdding();
            base.Stop();
        }

        protected override void TaskIntermediate(object inputItem, out object outputItem)
        {


            if (_isOnline)
            {
                outputItem = inputItem;
                if(!_dataMonitor.IsAddingCompleted && dataMonitoring)
                _dataMonitor.Add(outputItem);
                return;
            }

            if (_noiseDetectionActive)
                _noiseDetectionQueue.Add(inputItem);


            if (!_ignoreData) //We are not yet done with processing frames 
            {
                Frame currentFrame = (Frame)inputItem;


                _totalFrameCounter++;
                _partialFrameCounter++;

                if (!isOnline)
                {
                    _progressPercentage = (int)((_partialFrameCounter / (double) recordingConfig.scheduleItemnSamples) * 100);

                    if (_progressPercentage != _oldProgressPercentage)
                    {
                        _progressMonitor.Add(_progressPercentage);
                        _oldProgressPercentage = _progressPercentage;
                    }
                }


                if (_partialFrameCounter > _recordingConfig.scheduleItemnSamples)
                {

                    _currentScheduleItem++;
                    _partialFrameCounter = 1;

                    if (_currentScheduleItem >= _recordingConfig.schedule.Count())
                    {
                        _ignoreData = true;
                        _currentScheduleItem = -1;

                        //Mark the Pipeline to which this stage belongs to as requiring to be stopped
                        //and stop the pipeline stages after this one 
                        //We cannot just call Stop() here. 
                        //That would make this thread to try to join itself, which would cause a deadlock.
                        //see: http://msdn.microsoft.com/en-us/library/95hbf2ta(v=vs.110).aspx

                        outputItem = null;

                        if (_homePipeline != null) _homePipeline.stopPending = true;

                        //Generate an event here indicating a transition to the stopped status
                        if (!isOnline)
                        {
                            _recordingEventQueue.Add(_currentScheduleItem);
                            //Sending a -1 to the progress monitor also indicates that no more progress information will be sent
                            _progressMonitor.Add(-1);
                        }
                        OnStatusChanged(new RecordingStatusEventArgs(_currentScheduleItem));
                        return;
                    }

                    //Generate an event here indicating a transition to a new item (movement) in the schedule
                    //For consumers listening on the event queue (mostly applies to other threads), we add
                    //the index of the current schedule item to that queue.
                    if (!isOnline)
                    {
                        _recordingEventQueue.Add(_currentScheduleItem);
                        _progressMonitor.Add(0);
                    }
                    _scheduleItem = _recordingConfig.schedule.ElementAt(_currentScheduleItem).movementComposition;
                    _movCode = GetMovementCode(_scheduleItem);
                    OnStatusChanged(new RecordingStatusEventArgs(_currentScheduleItem));
                }



                foreach (ushort item in _scheduleItem) currentFrame.tagList.Add(item);
                //We can also add the numeric movement ID here
                currentFrame.movementCode = _movCode;


                if (_currentScheduleItem >= recordingConfig.scheduleWarmupItems)
                    outputItem = inputItem;
                else outputItem = null;
                //Debug.Write("C");
            }
            else outputItem = null;
        }



        /// <summary>
        /// Looks up in the configuration the corresponding integer code for the tagList and returns it.
        /// </summary>
        /// <param name="tagList"></param>
        /// <returns></returns>
        private ushort GetMovementCode(ushort[] tagList)
        {

            if (tagList.Length == 1)
            {
                return (ushort)(tagList[0]);
            }

            int count = 0;

            string tagString = "";
            foreach (ushort singlemov in tagList)
            {
                tagString = tagString + singlemov;
                count++;
                if (count < tagList.Length) tagString = tagString + ",";
            }

            return (ushort)(allowedComplexMovements.IndexOf(tagString) + numSingleMovements);

        }


        /// <summary>s
        /// Auxiliary function for detecting the maximum noise amplitude in every channel.
        /// This function is executed on a separate thread. It will detect the noise amplitude
        /// during the second second of the initial rest. If no initial rest is defined at the
        /// recording schedule, its results will not be usable! This method assumes a movement
        /// duration of at least two seconds.
        /// </summary>
        private void NoiseDetection()
        {
            Debug.WriteLine("AcquisitionController: running noise detection");

            int frameCounter = 0;

            uint startIndex = recordingConfig.sampleFreq;
            uint stopIndex = 2*recordingConfig.sampleFreq;

            Frame frame = new Frame(null,0,0,0,0);

            double[] maxAmplitudes = new double[recordingConfig.nChannels]; 

            foreach (object item in _noiseDetectionQueue.GetConsumingEnumerable())
            {
                frame = (Frame)item;
           
                frameCounter = (int)frame.sequenceIdx;
           
                if (frameCounter >= stopIndex)
                {
                    _noiseDetectionActive = false;
                    break;
                }

                if (frameCounter > startIndex)
                {
                    for (int i = 0; i < recordingConfig.nChannels; i++)
                    {
                        double AbsAmplitude = Math.Abs(frame.samples[i]);
                        if (AbsAmplitude > maxAmplitudes[i])
                            maxAmplitudes[i] = AbsAmplitude;
                    }
                }
            }

            //We copy the detected maximum amplitudes to 
            //the ThresholdSet structure in the RecordingConfig object.
            if(recordingConfig.thresholdSet!=null)
            for (int i = 0; i < recordingConfig.nChannels; i++)
                recordingConfig.thresholdSet.minValues[i] = maxAmplitudes[i];

            Debug.WriteLine("AcquisitionController: Noise detection finished at frame index "+frame.sequenceIdx+".");
        }


    }

}
