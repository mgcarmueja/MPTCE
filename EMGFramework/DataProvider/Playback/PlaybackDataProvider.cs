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
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using EMGFramework.ValueObjects;
using EMGFramework.Timer;
using EMGFramework.File;




namespace EMGFramework.DataProvider
{
    public class PlaybackDataProvider : EMGDataProvider
    {
        public new static string ID
        {
            get
            {
                return "playback";
            }
        }


        public override bool readsFromFile
        {
            get
            {
                return true;
            }
        }

 
        private bool _isRunning = false;
        private uint _frameIdx = 0; //Frame index counter
        private double _timeIdx = 0.0; //Time index counter
        private int _framePeriod; /// Frame period in nanoseconds
        private int _framesPerTick;
        private Random _rnd;
        private FastTimer _timer;


        /// <summary>
        /// List of usable IFileReadWriter objects
        /// </summary>
        private List<IFileReadWriter> _fileReadWriters;


        /// <summary>
        /// Filter string usable at OpenFileDialog windows. Allows filtering to show only the supported file formats
        /// </summary>
        private string _fileFilters;


        /// <summary>
        /// Recording and RecordingConfig instances to store the data that will be loaded from file and played back
        /// </summary>
        private Recording _loadedData;
        private RecordingConfig _loadedDataConfig;


        /// <summary>
        /// This constructor initializes a default configuration for the PlaybackDataProvider
        /// </summary>
        public PlaybackDataProvider()
            :base()
        {
            _framePeriod = 500000; //0.5 msec
            _framesPerTick = 200;
            _rnd = new Random();
            _timer = new FastTimer(3, (_framesPerTick * _framePeriod) / 1000000, GenerateFrame);

            //Iinitialization of anything related to loading playback data from file
            _loadedDataConfig = new RecordingConfig();
            _loadedData = new Recording(_loadedDataConfig);

            _fileReadWriters = new List<IFileReadWriter>();
            
            //Initialization of available IFileReadWriters
            _fileReadWriters.Add(new HDF5RecordingReadWriter());

            //Composing filter string
            _fileFilters = "";
            foreach (IFileReadWriter fileReadWriter in _fileReadWriters)
            {
                _fileFilters = _fileFilters + fileReadWriter.supportedFormats.ToString() + "|";
            }
            _fileFilters = _fileFilters + "All Files (*.*)|*.*";

        }


        private System.Windows.Forms.OpenFileDialog _openFileDialog = new System.Windows.Forms.OpenFileDialog();

        /// <summary>
        /// If the EMGDataProvider needs to connect to some sort of data source (e.g. a local or remote
        /// data server), it should be done whithin this method.
        /// </summary>
        public override void Connect()
        {

            _openFileDialog.Filter = _fileFilters;
            if (_openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadFromFile(_openFileDialog.FileName);
            }

            recordingConfig.maxVoltage = _loadedData.parameters.maxVoltage;
            recordingConfig.minVoltage = _loadedData.parameters.minVoltage;

            status = DataProviderStatus.connected;
        }


        /// <summary>
        /// If the EMGDataProvider needs to disconnect from some sort of data source (e.g. a local or remote
        /// data server), it should happen whithin this method.
        /// </summary>
        public override void Disconnect()
        {
            status = DataProviderStatus.disconnected;
        }

        /// <summary>
        /// Things such as configuring a data acquisition device after having access to it
        /// should be done here.
        /// </summary>
        public override void Init()
        {
            outputQueue = new BlockingCollection<object>();
            status = DataProviderStatus.initialized;
        }

        /// <summary>
        /// Data acquisition starts and the output queue is filled with data objects.
        /// </summary>
        /// <returns></returns>
        public override BlockingCollection<Object> Start()
        {

            if (!_isRunning)
            {
                _isRunning = true;
                if (isPaused == true) isPaused = false;
                else
                {
                    _frameIdx = 0;
                    _timeIdx = 0.0;
                }

                _timer.Start();

            }
            //TODO: Handle exceptions!!
            return outputQueue;
        }


        /// <summary>
        /// Called by a timer, this method either pushes a frame from a file or generates a random frame
        /// </summary>
        private void GenerateFrame()
        {
            double[] samples;
            int position;
            Frame tempFrame;

            if ((_loadedData != null) && (_loadedData.data.Count > 0))
                for (int i = 0; i < _framesPerTick; i++)
                {
                    position = (int)_frameIdx - (int)(recordingConfig.scheduleWarmupItems * recordingConfig.scheduleItemnSamples);

                    if (loop && position > 0) position = position % _loadedData.data.Count;

                    if ((position >= 0)
                        && (position < _loadedData.data.Count))
                        tempFrame = _loadedData.data.ElementAt(position).Clone();
                    else tempFrame = _loadedData.data.ElementAt(0).Clone();

                    tempFrame.sequenceIdx = _frameIdx;
                    tempFrame.timeIdx = (double)_frameIdx / (double)recordingConfig.sampleFreq;

                    if (!outputQueue.IsAddingCompleted)
                        outputQueue.Add(tempFrame);

                    _frameIdx++;

                }
            else
                for (int i = 0; i < _framesPerTick; i++)
                {
                    samples = new double[recordingConfig.nChannels];

                    for (int j = 0; j < recordingConfig.nChannels; j++)
                        samples[j] = (double)(recordingConfig.maxVoltage * 0.90) * (float)((Math.Sin((i + 1) / _rnd.NextDouble() * (10.0)) + Math.Sin((i + 1) / _rnd.NextDouble() * 23.0) + Math.Sin((i + 1) / _rnd.NextDouble() * 31.0)) / 3.0);

                    if (!outputQueue.IsAddingCompleted)
                        outputQueue.Add(new Frame(samples, _frameIdx, _timeIdx, recordingConfig.minVoltage, recordingConfig.maxVoltage));

                    _frameIdx++;
                    _timeIdx += (double)_framePeriod / 1000000000.0;//because the frame period is given in nanoseconds
                }

            Debug.Write("D");
        }


        /// <summary>
        /// Data acquisition stops definitely. The output data queue will not be filled again with data 
        /// </summary>
        public override void Stop()
        {
            //We stop the data thread and mark the output queue as complete for adding.
            if (_isRunning)
            {

                //Tell the data thread to stop and wait for it to finish
                _isRunning = false;
                //_runThread.Join();

                _timer.Stop();

                outputQueue.CompleteAdding();
                _timeIdx = 0.0;
                _frameIdx = 0;
                status = DataProviderStatus.stopped;
            }
            //TODO: Handle exceptions!!
        }

        /// <summary>
        /// Data acquisition is paused. The output data queue is not invalidated for new additions.
        /// </summary>
        public override void Pause()
        {
            //We stop the data thread, but we do not mark the output queue as complete for adding.

            if (_isRunning)
            {
                //Tell the data thread to stop and wait for it to finish
                _isRunning = false;
                //_runThread.Join();
                _timer.Stop();
                isPaused = true; //Moreover, here we activate the paused flag
                status = DataProviderStatus.paused;
            }

            //TODO: Handle exceptions!!
        }


        private void LoadFromFile(string filename)
        {
            string extension = Path.GetExtension(filename);
            IFileReadWriter selectedReadWriter = null;

            foreach (IFileReadWriter fileReadWriter in _fileReadWriters)
            {
                if (fileReadWriter.supportedFormats.ToString().Contains(extension))
                {
                    selectedReadWriter = fileReadWriter;
                    break;
                }
            }

            if (selectedReadWriter != null)
            {

                _loadedData.Clear();

                selectedReadWriter.requestedMovements = BuildRequestedMovements();
                selectedReadWriter.movementSelector = movementSelector;
                selectedReadWriter.knownMovements = knownMovements;
                selectedReadWriter.allowedComplexMovements = allowedComplexMovements;

                selectedReadWriter.ReadFile(filename, _loadedData);
                //recordingValid = true;
            }
            else throw (new DataProviderException("LoadFromFile: Could not select an appropriate reader for this file type!"));
        }


        /// <summary>
        /// Builds the list of requested movements that will be read from a file by a ReadWriter object 
        /// </summary>
        /// <returns>A list of scheduled items</returns>
        private List<ScheduleItem> BuildRequestedMovements()
        {
            List<ScheduleItem> requestedMovements = new List<ScheduleItem>();

            List<int> codeList = new List<int>();

            foreach (ScheduleItem item in recordingConfig.schedule)
            {
                if (!codeList.Contains(item.movementCode))
                {
                    requestedMovements.Add(item);
                    codeList.Add(item.movementCode);
                }
            }

            return requestedMovements;
        }


    }
}
