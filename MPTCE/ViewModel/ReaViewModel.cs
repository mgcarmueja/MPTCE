/*
 * Copyright 2015 Martin Garcia Carmueja 
 * 
 *  This file is part of the Myoelectric Personal Training and Control Environment (MPTCE).
 *
 *  MPTCE is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  MPTCE is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with MPTCE.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using EMGFramework.Utility;
using EMGFramework.DataProvider;
using EMGFramework.PatternRecognizers;
using EMGFramework.ValueObjects;
using QDGraph;
using MPTCE.Model;
using MPTCE.RealtimeConsumers;



namespace MPTCE.ViewModel
{
    public class ReaViewModel : INotifyPropertyChanged
    {

        /// <summary>
        /// Event handler that fires whenever a property in the ViewModel has been changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }



        /// <summary>
        /// Array of plot models user to display the data gathered through each channel
        /// </summary>
        public GraphModel[] plotModels { get; private set; }


        /// <summary>
        /// Array of point BlockingCollections used to communicate between the point reader thread and the GUI thread
        /// </summary>
        public BlockingCollection<System.Windows.Point>[] concurrentPointLists { get; private set; }


        /// <summary>
        /// An instance to the underlying model
        /// </summary>
        public ReaModel reaModel { get; private set; }


        private bool _plotGraphs;
        /// <summary>
        /// True if realtime graphs should be displayed at the UI, false otherwise
        /// </summary>
        public bool plotGraphs
        {
            get
            {
                return _plotGraphs;
            }

            set
            {
                if (_plotGraphs != value)
                {
                    _plotGraphs = value;
                    this.NotifyPropertyChanged("plotGraphs");
                }
            }
        }


        /// <summary>
        /// True if movement recognition is threshold-based, false if pattern recognition is used.
        /// </summary>
        public bool levelControlled
        {
            get
            {
                return reaModel.levelControlled;
            }

            set
            {
                if (reaModel.levelControlled != value)
                {
                    reaModel.levelControlled = value;
                    this.NotifyPropertyChanged("levelControlled");
                    this.NotifyPropertyChanged("prControlled");
                }
            }
        }

        /// <summary>
        /// Complementary attribute of levelControlled
        /// </summary>
        public bool prControlled
        {
            get
            {
                return !reaModel.levelControlled;
            }
        }


        /// <summary>
        /// List of the available data acquisition devices
        /// </summary>
        public List<string> devices
        {
            get
            {
                //return _devices;
                return GenericFactory<EMGDataProvider>.Instance.SupportedProducts;
            }
        }



        private string _selectedDevice;
        /// <summary>
        /// Contains the identificator of the selected device
        /// </summary>
        public string selectedDevice
        {
            get
            {
                return _selectedDevice;
            }

            set
            {
                if (_selectedDevice != value)
                {
                    _selectedDevice = value;
                    reaModel.dataProvider = _selectedDevice;
                    this.NotifyPropertyChanged("selectedDevice");
                }
            }
        }


        /// <summary>
        /// List of registered (i.e. trained) PatternRecognizer instances.
        /// </summary>
        public List<string> patternRecognizers
        {
            get
            {
                return InstanceManager<PatternRecognizer>.Instance.RegisteredInstances;
            }
        }



        private string _selectedPatternRecognizer;
        /// <summary>
        /// Name of the currently selected PatternRecognizer instance for realtime pattern recognition.
        /// </summary>
        public string selectedPatternRecognizer
        {
            get
            {
                return _selectedPatternRecognizer;
            }

            set
            {
                if (_selectedPatternRecognizer != value)
                {
                    _selectedPatternRecognizer = value;
                    reaModel.patternRecognizer = _selectedPatternRecognizer;
                    this.NotifyPropertyChanged("selectedPatternRecognizer");

                }
            }
        }



        /// <summary>
        /// True if a movement recognition session is taking place. False otherwise.
        /// </summary>
        public bool isRunning
        {
            get
            {
                return reaModel.isRunning;
            }
        }


        public bool isNotRunning
        {
            get 
            {
                if (reaModel != null)
                    return !reaModel.isRunning;
                else return true;
            }
        }


        private int _graphPointsToCommit;

        /// <summary>
        /// Stores the number of points in the graph point queue of each graph that are still pending to be 
        /// committed. This is accessed by the view when refreshing the graphs
        /// </summary>
        public int graphPointsToCommit
        {
            get
            {
                return _graphPointsToCommit;
            }
            private set
            {
                if (_graphPointsToCommit != value)
                {
                    _graphPointsToCommit = value;
                    this.NotifyPropertyChanged("graphPointsToCommit");
                }
            }
        }


        /// <summary>
        /// Object server used by the RealtimeConsumers initialized in the view.
        /// </summary>
        public ObjectServer<Movement> objectServer
        {
            get
            {
                return reaModel.objectServer;
            }
        }


        private RealtimeConsumer _movDisplayConsumer;
        /// <summary>
        /// RealtimeConsumer-derived object implementing the big movement recognition display used in the
        /// realtime pane.
        /// </summary>
        public RealtimeConsumer movDisplayConsumer
        {
            get
            {
                return _movDisplayConsumer;
            }
        }


        private List<RealtimeConsumer> _consumerList;
        /// <summary>
        /// List containing the RealtimeConsumer objects to appear at the toolbox
        /// in the realtime pane.
        /// </summary>
        public List<RealtimeConsumer> consumerList
        {
            get
            {
                return _consumerList;
            }
        }


        /// <summary>
        /// A RecordingConfig object that contains only information related to a thresold-based "training"
        /// </summary>
        public RecordingConfig thresholdRecordingConfig
        {
            get
            {
                return reaModel.thresholdRecordingConfig;
            }

            set
            {
                reaModel.thresholdRecordingConfig = value;
                this.NotifyPropertyChanged("thresholdSelectable");
            }
        }


        public bool thresholdSelectable
        {
            get
            {
                return ((thresholdRecordingConfig != null) && !isRunning);
            }
        }


        public BindingList<ThresholdControl> thresholdControls
        {
            get
            {
                return reaModel.thresholdControls;
            }
        }


        private List<Movement> _movements;
        /// <summary>
        /// List of selectable movements
        /// </summary>
        public List<Movement> movements
        {
            get
            {
                return _movements;
            }

        }


        public bool readyToRun
        {
            get
            {
                return reaModel.readyToRun;
            }
        }


        private bool _applicationActive;
        /// <summary>
        /// True if this application is the active one and therefore will receive mouse
        /// and keyboard events. False otherwise.
        /// </summary>
        public bool applicationActive
        {
            get
            {
                return _applicationActive;
            }

            set 
            {
                if (_applicationActive != value)
                {
                    _applicationActive = value;
                    this.NotifyPropertyChanged("applicationActive");
                }
            }
        }


        public bool multipleActivation
        {
            get
            {
                return reaModel.multipleActivation;
            }

            set
            {
                reaModel.multipleActivation = value;
            }
        }


        public bool multipleActivationSupported
        {
            get
            {
                return reaModel.multipleActivationSupported;
            }
        }


        /// <summary>
        /// Object containing configuration that will be passed to the used consumers and will be updated
        /// to them via events as it changes.
        /// </summary>
        public ConsumerConfig consumerConfig
        {
            get;
            private set;
        }


        public ReaViewModel()
        {
            _movements = new List<Movement>();

            consumerConfig = new ConsumerConfig();


            for (int i = 1; i < Properties.Settings.Default.AcqMovementsList.Count; i++)
                _movements.Add(new Movement(i, Properties.Settings.Default.AcqMovementsList[i]));
            
            reaModel = new ReaModel();
            reaModel.PropertyChanged += reaModel_PropertyChanged;
            InstanceManager<PatternRecognizer>.Instance.ContentChanged += InstanceManagerPatternRecognizer_ContentChanged;

            _movDisplayConsumer = GenericFactory<RealtimeConsumer>.Instance.CreateProduct(Properties.Settings.Default.ReaMovDisplayer);
            if (_movDisplayConsumer != null) _movDisplayConsumer.objectServer = reaModel.objectServer;

            _consumerList = new List<RealtimeConsumer>();

            //Populate the consumer list with realtimeConsumer instances from those subclasses enabled 
            //through program configuration.
            foreach (string item in Properties.Settings.Default.ReaConsumerList)
            {

                RealtimeConsumer realtimeConsumer = GenericFactory<RealtimeConsumer>.Instance.CreateProduct(item);
                if (realtimeConsumer != null)
                {
                    _consumerList.Add(realtimeConsumer);
                    realtimeConsumer.objectServer = reaModel.objectServer;
                    realtimeConsumer.consumerConfig = consumerConfig;
                }
            }

            this.PropertyChanged += ReaViewModel_PropertyChanged;
        }


        void ReaViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "applicationActive":
                    reaModel.movementMetadata.applicationActive = applicationActive;
                    consumerConfig.applicationActive = applicationActive;
                    break;

                default:
                    break;
            }
        }


        void InstanceManagerPatternRecognizer_ContentChanged(object sender, EventArgs e)
        {
            this.NotifyPropertyChanged("patternRecognizers");
        }


        void reaModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "levelControlled":
                case "isRunning":
                case "readyToRun":
                case "threshold":
                case "multipleActivation":
                case "multipleActivationSupported":
                    this.NotifyPropertyChanged(e.PropertyName);
                    if (e.PropertyName == "levelControlled")
                    {
                        ReconfigureGraphs();
                        BuildConsumerConfigMovements();
                        this.NotifyPropertyChanged("prControlled");
                    }

                    else if (e.PropertyName == "isRunning")
                    {
                        this.NotifyPropertyChanged("thresholdSelectable");
                        this.NotifyPropertyChanged("isNotRunning");
                    }
                    break;

                case "recordingConfig":
                    //This will be used to redefine which graphs are active and which inactive
                    if (reaModel.recordingConfig != null)
                        for (int i = 0; i < reaModel.recordingConfig.channelMask.Length; i++)
                            plotModels[i].enable = reaModel.recordingConfig.channelMask[i];
                    else
                        for (int i = 0; i < plotModels.Length; i++)
                            plotModels[i].enable = false;
                    break;

                case "selectedMovement":
                    if(levelControlled) BuildConsumerConfigMovements();
                    break;

                case "patternRecognizer":
                    if (prControlled) BuildConsumerConfigMovements();
                    break;
                default:
                    break;
            }
        }


        public void Configure()
        {

            if (devices.Contains(Properties.Settings.Default.SelectedDevice))
                selectedDevice = Properties.Settings.Default.SelectedDevice;
            else selectedDevice = devices[0];


            plotGraphs = true;

            plotModels = new GraphModel[Properties.Settings.Default.AcqDeviceNChannels];

            concurrentPointLists = new BlockingCollection<System.Windows.Point>[Properties.Settings.Default.AcqDeviceNChannels];

            for (int i = 0; i < Properties.Settings.Default.AcqDeviceNChannels; i++)
            {
                plotModels[i] = new GraphModel();
                concurrentPointLists[i] = plotModels[i].pointQueue;
            }



            reaModel.nMovements = Properties.Settings.Default.AcqMovementsList.Count +
                     Properties.Settings.Default.AcqAllowedMovements.Count;
            
            reaModel.numSingleMovements = Properties.Settings.Default.AcqMovementsList.Count;

            reaModel.allowedComplexMovements = Properties.Settings.Default.AcqAllowedMovements;

            reaModel.Init();

        }



        /// <summary>
        /// Working method for refreshing the configuration of the graphs
        /// </summary>
        private void ReconfigureGraphs()
        {
            double xMin, xMax, yMin, yMax, yMinAbs, yMaxAbs;

            if (reaModel.recordingConfig != null)
            {
                if (reaModel.levelControlled)
                {
                    yMin = 0; 
                    
                    yMinAbs = Math.Abs(reaModel.recordingConfig.minVoltage);
                    yMaxAbs = Math.Abs(reaModel.recordingConfig.maxVoltage);

                    if (yMinAbs > yMaxAbs) yMax = yMinAbs;
                    else yMax = yMaxAbs;

                }

                else
                {
                    yMin = reaModel.recordingConfig.minVoltage;
                    yMax = reaModel.recordingConfig.maxVoltage;
                }
            }
            else
            {
                if(reaModel.levelControlled)
                {
                    yMin = 0;
                    yMax = 1.0;
                }
                else
                {
                    yMin = -1.0;
                    yMax = 1.0;
                }
            }

            xMin = 0;
            xMax = 10; //This will mean the last 10 seconds if the graph view is working in "conveyor belt" mode

            if (plotModels != null)
            {
                for (int i = 0; i < plotModels.Length; i++)
                {
                    plotModels[i].xMin = xMin;
                    plotModels[i].xMax = xMax;
                    plotModels[i].yMin = yMin;
                    plotModels[i].yMax = yMax;
                    plotModels[i].plotColor = System.Windows.Media.Color.FromArgb(224, 0, 0, 255);//Blue with 50% transparency

                    plotModels[i].plotMode = PlotMode.movingViewPort;
                    plotModels[i].viewPortStride = (xMax - xMin) / 100.0;

                    if (plotModels[i].graph != null) plotModels[i].graph.Clear();
                }
            }
        }


        private Task _fillGraphTask;


        /// <summary>
        /// Called on a separate thread, it listens for incoming data which are placed on a BlockingCollection 
        /// </summary>
        private void FillGraph()
        {
            Frame currentFrame;
            int downsamplingFactor = 1; //This should be stored as a program setting
            int downsamplingCounter = 0;
            int samplesPerBurst = 100; //In samples

            graphPointsToCommit = 0;


            foreach (object currentObject in reaModel.dataMonitor.GetConsumingEnumerable())
            {
                if (!plotGraphs) continue;

                currentFrame = (Frame)currentObject;
                downsamplingCounter++;
                if (downsamplingCounter == downsamplingFactor)
                {
                    for (int i = 0; i < currentFrame.nsamples; i++)
                        concurrentPointLists[i].Add(new System.Windows.Point((double)currentFrame.timeIdx, (double)currentFrame.samples[i]));

                    graphPointsToCommit++;
                    downsamplingCounter = 0;
                }

                if (graphPointsToCommit == samplesPerBurst)
                {
                    this.NotifyPropertyChanged("pointLists");

                    graphPointsToCommit = 0;
                }
            }
            Debug.WriteLine("FillGraph: dataMonitor flow ended.");

            //And we commit to the graph the remaining points.
            if (graphPointsToCommit > 0) this.NotifyPropertyChanged("pointLists");

        }



        /// <summary>
        /// Starts movement recognition
        /// </summary>
        public void Start()
        {
            try
            {
                ReconfigureGraphs();
                movDisplayConsumer.running = true;
                reaModel.RunRecognition();
            }
            catch(ReaModelException ex)
            {
                movDisplayConsumer.running = false;
                throw new ReaViewModelException("ReaModel.RunRecognition:" + ex.Message);
            }
            _fillGraphTask = Task.Run(new Action(FillGraph));


        }

        /// <summary>
        /// Stops movement recognition
        /// </summary>
        public void Stop()
        {
            reaModel.StopRecognition();

            _fillGraphTask.Wait();
            movDisplayConsumer.running = false;
        }


        /// <summary>
        /// Based on the contents of either thresholdControls or the currently selected pattern recognizer, this
        /// method updates the list of movements stored in the ConsumerConfig object that is passed to all 
        /// RealtimeConsumers
        /// </summary>
        private void BuildConsumerConfigMovements()
        {
            //First, of all, we erase the current list
            consumerConfig.availableMovements.Clear();

            List<Movement> tempList = reaModel.supportedMovements;

            foreach (Movement movement in tempList)
                consumerConfig.availableMovements.Add(movement);
            
        }

    }
}
