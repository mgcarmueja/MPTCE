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
using System.Linq;
using System.Text;
using EMGFramework.Pipelines;
using EMGFramework.ValueObjects;



namespace EMGFramework.Utility
{

    /// <summary>
    /// Implementation of the process that transforms a recording into a sequence of overlapping windows.
    /// This pipeline stage supports both offline and online working modes, acting either as an initial
    /// stage or as an intermediate stage respectively. For this to work, the isOnline property must be 
    /// set at best BEFORE adding an instance of this class to a pipeline.
    /// </summary>
    public class WindowMaker : Stage
    {

        private int _offsetCounter = 0;
        private int _startFrame;
        private int _stride;
        private Pipeline _myPipeline;
        private List<Frame> _outputBuffer;



        /// <summary>
        /// true if the class is being used for online pattern recognition, false otherwise.
        /// By default, new WindowMaker instances are initialized for online use.
        /// </summary>
        public bool isOnline
        {
            get
            {
                return enableInput;
            }

            set
            {
                if (enableInput != value)
                    enableInput = value;
            }
        }


        public BlockingCollection<object> lockingCollection
        {
            get;
            private set;
        }

        public TreatmentConfig config
        {
            get;
            set;
        }


        public Recording recording
        {
            get;
            set;
        }



        public Pipeline homePipeline
        {
            get
            {
                return _myPipeline;
            }

            set
            {
                _myPipeline = value;
            }
        }


        private DataWindow _dataWindow;


        public WindowMaker(TreatmentConfig treatmentConfig, Recording usedRecording)
            : base(20, true, true)
        {
            this.config = treatmentConfig;
            this.recording = usedRecording;
        }


        public WindowMaker(TreatmentConfig treatmentConfig)
            : base(20, true, true)
        {
            this.config = treatmentConfig;
        }


        public WindowMaker(Recording usedRecording)
            : base(20, true, true)
        {
            this.recording = usedRecording;
        }


        public WindowMaker()
            : base(20, true, true)
        {

        }


        public override void Init()
        {
            base.Init();

            _offsetCounter = 0;
            _startFrame = 0;
            _stride = config.windowOffset;
            lockingCollection = new BlockingCollection<object>();
            _outputBuffer = new List<Frame>();
        }


        /// <summary>
        /// Called when isOnline is set to false, which is NOT the default. 
        /// </summary>
        /// <param name="outputItem"></param>
        protected override void TaskInitial(out object outputItem)
        {

            int frameLimit = _startFrame + config.windowLength;

            if (_startFrame < recording.data.Count())
            {
                DataWindow dataWindow = new DataWindow();

                for (int i = _startFrame; ((i < frameLimit) && (i < recording.data.Count())); i++)
                    dataWindow.frames.Add((Frame)recording.data.ElementAt(i));

                _startFrame += _stride;
                outputItem = dataWindow;
            }
            else //We have to stop the acquisition, but we cannot do it from the windowmaker thread
            {
                outputItem = null;
                _myPipeline.stopPending = true;
                if (!lockingCollection.IsAddingCompleted) lockingCollection.CompleteAdding();
            }

        }


        /// <summary>
        /// Called when isOnline is set to true, which is its default value.
        /// </summary>
        /// <param name="inputItem"></param>
        /// <param name="outputItem"></param>
        protected override void TaskIntermediate(object inputItem, out object outputItem)
        {
            outputItem=null;

            Frame inputFrame = (Frame)inputItem;

            _outputBuffer.Add(inputFrame);

            _offsetCounter = (_offsetCounter + 1) % _stride;

            if (_outputBuffer.Count == config.windowLength)
            {

                if (_offsetCounter == 0)
                {
                    _dataWindow = new DataWindow();
                    foreach (Frame item in _outputBuffer) _dataWindow.frames.Add(item);
                    outputItem = _dataWindow;
                }

                _outputBuffer.RemoveAt(0);

            }
        }
    
    
    
    }
}
