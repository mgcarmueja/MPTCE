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
using EMGFramework.Pipelines;
using EMGFramework.DataProvider;
using EMGFramework.ValueObjects;
using EMGFramework.Features;




namespace EMGFramework.Utility
{
    /// <summary>
    /// Pipeline stage for feature extraction. By default it acts as an intermediate stage for online pattern
    /// recognition, but can also be configured as a final stage for offline feature extraction. When running, it
    /// takes a DataWindow object as input, extracts a given set of features from it and adds them to the input DataWindow,
    /// which is either forwarded to the next pipeline stage (online) queue or stored into an output list (offline). 
    /// </summary>
    public class FeatureExtractor : Stage
    {
        /// <summary>
        /// True by default, configures the stage for either online or offline operation
        /// </summary>
        public bool isOnline
        {
            get 
            {
                return enableOutput;
            }

            set 
            {
                if (enableOutput != value)
                    enableOutput = value;
            }
        }

        private bool _isMonitored=false;
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


        /// <summary>
       /// List of features that must be extracted
       /// </summary>
        public List<string> selectedFeatures{ get; set; }

        private Dictionary<string,Feature> _featureObjects;


        private List<DataWindow> _outputData;
        /// <summary>
        /// Output list used for offline feature detection
        /// </summary>
        public List<DataWindow> outputData
        {
            get
            {
                return _outputData;
            }
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

        /*
        private RecordingConfig _recordingConfig;
        /// <summary>
        /// Recording configuration used for retrieving running parameters 
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
        */

        public FeatureExtractor()
            : base(2, true, true)
        {
        }

        public override void Init()
        {
            base.Init();
            _outputData = new List<DataWindow>();
            _dataMonitor = new BlockingCollection<object>();
            _featureObjects = new Dictionary<string,Feature>();

            foreach (string featureName in selectedFeatures)
                _featureObjects.Add(featureName,GenericFactory<Feature>.Instance.CreateProduct(featureName));
            
        }

        /// <summary>
        /// Called when operating in offline mode
        /// </summary>
        /// <param name="inputItem"></param>
        protected override void TaskFinal(object inputItem)
        {
            DataWindow window = (DataWindow)inputItem;

            Process(window);

            _outputData.Add(window);
            if (_isMonitored) _dataMonitor.Add(window);

        }


        /// <summary>
        /// Called when operating in online mode
        /// </summary>
        /// <param name="inputItem"></param>
        /// <param name="outputItem"></param>
        protected override void TaskIntermediate(object inputItem, out object outputItem)
        {
            DataWindow window = (DataWindow)inputItem;


            Process(window);

            outputItem=window;
            if(_isMonitored) _dataMonitor.Add(window);
        }


        /// <summary>
        /// Performs the common tasks of TaskInitial and TaskIntermediate
        /// </summary>
        /// <param name="window"></param>
        private void Process(DataWindow window)
        {
            foreach (Feature feature in _featureObjects.Values)
                feature.frameBlockSize = window.frames.Count;

            foreach (Frame frame in window.frames)
            {
                foreach (Feature feature in _featureObjects.Values)
                    feature.Process(frame);
            }

            foreach (string key in _featureObjects.Keys)
            {
                Feature feature;
                if (_featureObjects.TryGetValue(key, out feature))
                {
                    window.features.Add(key, feature.output);
                    feature.Clear();
                }
            }
        }



        public override void Stop()
        {
            Debug.WriteLine("Stopping feature extraction stage...");
            _dataMonitor.CompleteAdding();
            base.Stop();

        }



    }
}
