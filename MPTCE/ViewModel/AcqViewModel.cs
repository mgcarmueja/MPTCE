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
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using MPTCE.Model;
using MPTCE.Properties;
using EMGFramework.ValueObjects;
using EMGFramework.DataProvider;
using EMGFramework.Utility;
using System.Diagnostics;
using QDGraph;
using RecordingPlan;
using System.Windows;
using System.Windows.Media;
using System.Drawing;



namespace MPTCE.ViewModel
{
    public class AcqViewModel : INotifyPropertyChanged
    {
        private bool _paused = false, _readyToSave = false;
        private bool _movement1, _movement2, _movement3;
        private List<Movement> _movements;
        //private List<AcqDevice> _devices;
        private ObservableCollection<Movement> _selectedMovements;
        protected Thread _fillGraphThread, _recordCommanderThread;
        private int _samplesPerBurst = 1000;


        /// <summary>
        /// It indicates how many points will be sent to the concurrent point lists before an event
        /// is raised to update the graphs
        /// </summary>
        public int samplesPerBurst
        {
            get
            {
                return _samplesPerBurst;
            }
            private set
            {
                if (_samplesPerBurst != value)
                {
                    _samplesPerBurst = value;
                    this.NotifyPropertyChanged("samplesPerBurst");
                }
            }
        }


        /// <summary>
        /// Array of plot models user to display the data gathered through each channel
        /// </summary>
        public GraphModel[] plotModels { get; private set; }

        /// <summary>
        /// The model for the recording plan display widget
        /// </summary>
        public RecordingPlanViewModel recordingPlanModel { get; private set; }


        /// <summary>
        /// Array of point BlockingCollections used to communicate between the point reader thread and the GUI thread
        /// </summary>
        public BlockingCollection<System.Windows.Point>[] concurrentPointLists { get; private set; }

        /// <summary>
        /// An instance to the underlying model
        /// </summary>
        public AcqModel acqModel { get; private set; }

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
        /// True if after the recording we are ready to save the data we just recorded 
        /// </summary>
        public bool readyToSave
        {
            get
            {
                return _readyToSave;
            }
            private set
            {
                if (_readyToSave != value)
                {
                    _readyToSave = value;
                    this.NotifyPropertyChanged("readyToSave");
                }
            }
        }


        private bool _recording = false;
        /// <summary>
        /// True if a recording operation has been started. False otherwise
        /// </summary>
        public bool recording
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
                    this.NotifyPropertyChanged("notRecording");
                }
            }
        }

        /// <summary>
        /// Complementary of the recording property. Useful to control the activation status of
        /// widgets in the view that should be disabled while recording.
        /// </summary>
        public bool notRecording
        {
            get
            {
                return !recording;
            }
        }


        public bool paused
        {
            get
            {
                return _paused;
            }
            set
            {
                if (_paused != value)
                {
                    _paused = value;
                    this.NotifyPropertyChanged("paused");
                }
            }
        }


        public bool movement1
        {
            get
            {
                return _movement1;
            }

            set
            {
                if ((_movement1 != value) /*&& (_selectedMovements.Count >= 1)*/)
                {
                    _movement1 = value;
                    //if (_movement1) acqModel.simultMovements = 1;
                    this.NotifyPropertyChanged("movement1");
                }
            }
        }


        public bool movement2
        {
            get
            {
                return _movement2;
            }

            set
            {
                if ((_movement2 != value) /*&& (_selectedMovements.Count >= 2)*/)
                {
                    _movement2 = value;
                    //if (_movement2) acqModel.simultMovements = 2;
                    this.NotifyPropertyChanged("movement2");
                }
            }
        }

        public bool movement3
        {
            get
            {
                return _movement3;
            }

            set
            {
                if ((_movement3 != value) /*&& (_selectedMovements.Count >= 3)*/)
                {
                    _movement3 = value;
                    //if (_movement3) acqModel.simultMovements = 3;
                    this.NotifyPropertyChanged("movement3");
                }
            }
        }

        public List<Movement> movements
        {
            get
            {
                return _movements;
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


        public ObservableCollection<Movement> selectedMovements
        {
            get { return _selectedMovements; }
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
                    acqModel.dataProvider = _selectedDevice;
                    this.NotifyPropertyChanged("selectedDevice");
                }
            }
        }


        /// <summary>
        /// A reference to the Recording object inside the underlying model
        /// </summary>
        public Recording recordedData
        {
            get
            {
                return acqModel.recordedData;
            }
        }

        public bool recordingValid
        {
            get
            {
                return acqModel.recordingValid;
            }
        }


        private int _recordingStatus = -1;
        /// <summary>
        /// While recording it is updated to the position in the schedule of the item being processed
        /// The event generated by this property can be used to trigger a status change in the RecordingPlan widget
        /// </summary>
        public int recordingStatus
        {
            get
            {
                return _recordingStatus;
            }
            private set
            {
                if (_recordingStatus != value)
                {
                    _recordingStatus = value;
                    this.NotifyPropertyChanged("recordingStatus");
                }
            }
        }


        private int _recordingItemProgress = 0;
        /// <summary>
        /// Progress percentage of the current recording item
        /// </summary>
        public int recordingItemProgress
        {
            get
            {
                return _recordingItemProgress;
            }
            set
            {
                if (_recordingItemProgress != value)
                {
                    _recordingItemProgress = value;
                    this.NotifyPropertyChanged("recordingItemProgress");
                }
            }
        }


        /// <summary>
        /// File filters string to be used on an open file dialog window 
        /// </summary>
        public string fileFilters
        {
            get
            {
                return acqModel.fileFilters;
            }
        }


        public RecordingConfig thresholdRecordingConfig
        {
            get
            {
                return acqModel.thresholdRecordingConfig;
            }
        }



        public bool detectThresholds
        {
            get { return acqModel.detectThresholds; }

            set
            {
                if (acqModel.detectThresholds != value)
                {
                    acqModel.detectThresholds = value;

                    this.NotifyPropertyChanged("detectThresholds");
                    this.NotifyPropertyChanged("dontDetectThresholds");
                }
            }
        }



        public bool dontDetectThresholds
        {
            get
            {
                return !acqModel.detectThresholds;
            }
        }


        public int simultMovements
        {
            get { return acqModel.simultMovements; }
            set
            {
                acqModel.simultMovements = value;
            }
        }



        private int _graphPointsToCommit;
        /// <summary>
        /// Stores the number of points in the graph point queue of each graph that are still pending to be 
        /// committed
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
        /// This is currently used only to obtain its running status
        /// </summary>
        public ReaViewModel reaViewModel
        {
            get;
            set;
        }


        /// <summary>
        /// true if the tab corresponding with this ViewModel at the UI should be active, false otherwise
        /// </summary>
        public bool tabActive
        {
            get
            {
                return (!reaViewModel.isRunning);
            }
        }


        // Tasks in which the FillGraph, RecordCommander and ShowProgress methods will be run.
        private Task _fillGraphTask;
        private Task _recordCommanderTask;
        private Task _showProgressTask;


        public AcqViewModel()
        {
            this.PropertyChanged += AcqViewModel_PropertyChanged;
            _movements = new List<Movement>();

            //We fill up the movement list here, so that the ListBox in the acquisition pane can use it 
            //as source to fill up its list BEFORE we continue loading configuration at Configure().
            //This is done so because the default selection of movements on startup must be done at MainWindow.xaml by now
            //and for the Configure() method to work properly, this selection must already be done before calling it.

            for (int i = 1; i < Properties.Settings.Default.AcqMovementsList.Count; i++)
                _movements.Add(new Movement(i, Properties.Settings.Default.AcqMovementsList[i]));

            acqModel = new AcqModel();
            acqModel.PropertyChanged += acqModel_PropertyChanged;
            _selectedMovements = acqModel.selectedMovements;
            recording = false;
            paused = false;
        }



        void AcqViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "movement1":
                    if (_movement1) acqModel.simultMovements = 1;
                    break;

                case "movement2":
                    if (_movement2) acqModel.simultMovements = 2;
                    break;

                case "movement3":
                    if (_movement3) acqModel.simultMovements = 3;
                    break;

                default:
                    break;
            }
        }



        /// <summary>
        /// Handler for changes in the properties of the model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void acqModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "recordedData":
                case "recordingValid":
                case "thresholdRecordingConfig":
                    //Forwarded for the treatment stage. Used to keep updated its reference to the recorded data
                    this.NotifyPropertyChanged(e.PropertyName);
                    break;
                case "simultMovements":
                    UpdateSimultMovements();
                    break;

                default:

                    break;
            }
        }



        /// <summary>
        /// Event listener that refreshes the configuration of the graphs when
        /// some of the involved parameters change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ReconfigureGraphsOnEvent(object sender, PropertyChangedEventArgs args)
        {
            string val = args.PropertyName;

            switch (args.PropertyName)
            {
                case "detectThresholds":
                case "sampleFreq":
                case "minVoltage":
                case "maxVoltage":
                case "scheduleItemTime":
                case "scheduleItemnSamples":
                case "repetitions":
                case "scheduleLength":
                case "recordedData":
                    ReconfigureGraphs();
                    break;

                default:
                    break;
            }
        }



        /// <summary>
        /// Updates the status of the simultaneous movements radio buttons in the View
        /// based on the value of the simultMovements property in the model
        /// </summary>
        private void UpdateSimultMovements()
        {
            switch (acqModel.simultMovements)
            {
                case 1:
                    movement1 = true; movement2 = false; movement3 = false;
                    break;
                case 2:
                    movement1 = false; movement2 = true; movement3 = false;
                    break;
                case 3:
                    movement1 = false; movement2 = false; movement3 = true;
                    break;
                default:
                    break;
            }
        }



        /// <summary>
        /// Working method for refreshing the configuration of the graphs
        /// </summary>
        private void ReconfigureGraphs()
        {
            double xMin, xMax, yMin, yMax, yMinAbs, yMaxAbs;

            if (acqModel.detectThresholds)
            {
                yMin = 0;
                if (acqModel.rectify)
                {
                    yMinAbs = Math.Abs(acqModel.recordingConfig.minVoltage);
                    yMaxAbs = Math.Abs(acqModel.recordingConfig.maxVoltage);

                    if (yMinAbs > yMaxAbs) yMax = yMinAbs;
                    else yMax = yMaxAbs;
                }
                else
                    yMax = acqModel.recordingConfig.maxVoltage - acqModel.recordingConfig.minVoltage;
            }
            else
            {
                yMin = acqModel.recordingConfig.minVoltage;
                yMax = acqModel.recordingConfig.maxVoltage;
            }
            xMin = 0;
            xMax = 60;


            if (acqModel.recordingConfig.schedule.Count > 0)
                //We set xmax to the projected duration of the recording schedule
                xMax = (acqModel.recordingConfig.schedule.Count - acqModel.recordingConfig.scheduleWarmupItems) * acqModel.recordingConfig.scheduleItemTime;
            if (acqModel.recordingValid && (acqModel.recordedData.duration != xMax))
                xMax = acqModel.recordedData.duration;


            if (plotModels != null)
            {
                for (int i = 0; i < plotModels.Length; i++)
                {
                    plotModels[i].xMin = xMin;
                    plotModels[i].xMax = xMax;
                    plotModels[i].yMin = yMin;
                    plotModels[i].yMax = yMax;
                    plotModels[i].plotColor = System.Windows.Media.Color.FromArgb(224, 0, 0, 255);//Blue with 50% transparency
                    //if (plotModels[i].graph != null) plotModels[i].graph.Clear();
                }
            }

        }



        /// <summary>
        /// It configures the viewModel with its default values.
        /// </summary>
        public void Configure()
        {

            reaViewModel.PropertyChanged += reaViewModel_PropertyChanged;

            this.acqModel.allowedComplexMovements = Properties.Settings.Default.AcqAllowedMovements;
            this.acqModel.numSingleMovements = Properties.Settings.Default.AcqMovementsList.Count;


            simultMovements = Properties.Settings.Default.AcqSimultMovements;
            detectThresholds = Properties.Settings.Default.AcqDetectThresholds;
            if (devices.Contains(Properties.Settings.Default.SelectedDevice))
                selectedDevice = Properties.Settings.Default.SelectedDevice;
            else selectedDevice = devices[0];

            plotModels = new GraphModel[Properties.Settings.Default.AcqDeviceNChannels];

            concurrentPointLists = new BlockingCollection<System.Windows.Point>[Properties.Settings.Default.AcqDeviceNChannels];

            for (int i = 0; i < Properties.Settings.Default.AcqDeviceNChannels; i++)
            {
                plotModels[i] = new GraphModel();
                plotModels[i].PropertyChanged += GraphModel_PropertyChanged;
                concurrentPointLists[i] = plotModels[i].pointQueue;
            }


            this.acqModel.PropertyChanged += ReconfigureGraphsOnEvent;

            LoadRecordingConfig(Properties.Settings.Default, acqModel.recordingConfig);


            //All the configuration parameters are in place. We can now build the initial recording schedule
            acqModel.ComposeSchedule();

            //And initialize the model
            acqModel.Init();

            //Now we can initialize the model for the  RecordingPlan display widget as well

            recordingPlanModel = new RecordingPlanViewModel(acqModel.recordingConfig, Properties.Settings.Default.AcqMovementsList);
            recordingPlanModel.PropertyChanged += recordingPlanModel_PropertyChanged;
            recordingPlanModel.SetImages(PrepareImages(Properties.Settings.Default.AcqMovementsList.Count + Properties.Settings.Default.AcqAllowedMovements.Count));
        }


        void reaViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "isRunning":
                    this.NotifyPropertyChanged("tabActive");
                    break;

                default:
                    break;
            }
        }



        private void GraphModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "toggle":
                    for (int i = 0; i < plotModels.Length; i++)
                        if (plotModels[i] == sender)
                        {
                            acqModel.recordingConfig.SetChannelEnable((uint)i, plotModels[i].toggle);
                            plotModels[i].enable = plotModels[i].toggle;
                            break;
                        }
                    break;
                default:
                    break;

            }
        }



        void recordingPlanModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "recordingPlan":
                    recordingPlanModel.BuildPlan();
                    recordingPlanModel.SetSelectedItem(-1);
                    break;
                default:
                    break;
            }
        }



        /// <summary>
        /// Composes a list of images that can be used for the RecordingPlan control to display 
        /// the appropriate image for each item in the schedule. If there is no image for a given movement, 
        /// a placeholder image is used.
        /// </summary>
        /// 
        public List<Bitmap> PrepareImages(int nMovs)
        {
            List<Bitmap> imageList = new List<Bitmap>();
            Bitmap tempBitmap;

            for (int i = 0; i < nMovs; i++)
            {
                tempBitmap = (Bitmap)Properties.Resources.ResourceManager.GetObject("m" + i);
                if (tempBitmap == null)
                    tempBitmap = (Bitmap)Properties.Resources.ResourceManager.GetObject("nofoto");

                //Now we have to convert this to a BitmapImage and add it to the list 

                imageList.Add(tempBitmap);
            }

            return imageList;
        }



        /// <summary>
        /// Loads configuration form a Settings object into a recordingConfig object;
        /// </summary>
        /// <param name="recordingConfig"></param>
        /// <param name="settings"></param>
        private void LoadRecordingConfig(Settings settings, RecordingConfig recordingConfig)
        {
            recordingConfig.gain = settings.AcqDeviceGain;
            recordingConfig.maxVoltage = settings.AcqDeviceMaxV;
            recordingConfig.minVoltage = settings.AcqDeviceMinV;
            recordingConfig.nChannels = settings.AcqDeviceNChannels;
            recordingConfig.sampleFreq = settings.AcqDeviceSampleFreq;
            recordingConfig.scheduleActive = settings.AcqScheduleActive;
            recordingConfig.scheduleItemTime = settings.AcqScheduleItemTime;
            recordingConfig.repetitions = settings.AcqRepetitionsPerMovement;
            recordingConfig.scheduleWarmupItems = settings.AcqScheduleWarmupItems;
        }



        /// <summary>
        /// Called on a separate thread, it listens for incoming data which are placed on a 
        /// BlockingCollection in the model
        /// </summary>
        private void FillGraph()
        {
            Frame currentFrame;
            int downsamplingFactor = 1; //This should be stored as a program setting
            int downsamplingCounter = 0;
            samplesPerBurst = 100; //In samples

            graphPointsToCommit = 0;

            //int progressModulo = (int) acqModel.recordingConfig.scheduleItemnSamples;


            foreach (object currentObject in acqModel.dataMonitor.GetConsumingEnumerable())
            {

                currentFrame = (Frame)currentObject;
                //recordingItemProgress = (int)((((int)currentFrame.sequenceIdx % progressModulo) / (float)progressModulo) * 100); 
                downsamplingCounter++;
                if (downsamplingCounter == downsamplingFactor)
                {
                    for (int i = 0; i < currentFrame.nsamples; i++)
                    {
                        //if (plotModels[i].toggle)
                        concurrentPointLists[i].Add(new System.Windows.Point((double)currentFrame.timeIdx, (double)currentFrame.samples[i]));
                    }
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
            recordingItemProgress = 0;

        }


        /// <summary>
        /// Uses the exposed Recording instance from the model to feed the concurrent pointLists 
        /// that provide the graphs with data. This is used after reading recording data from a file.
        /// </summary>
        private void RefreshGraphData()
        {
            ReconfigureGraphs();

            foreach (Frame frame in acqModel.recordedData.data)
            {
                for (int i = 0; i < frame.nsamples; i++)
                    concurrentPointLists[i].Add(new System.Windows.Point(frame.timeIdx, frame.samples[i]));
            }

            graphPointsToCommit = acqModel.recordedData.data.Count;
            this.NotifyPropertyChanged("pointLists");
            graphPointsToCommit = 0;
        }



        /// <summary>
        /// Called on a separate thread, this method serves as connection between the recording controller in the
        /// model and the user interface. Basically, it will react to status changes published by the recording
        /// controller thread through its event queue and stop the recording when the recording controller says so
        /// </summary>
        private void RecordCommander()
        {
            int lastStatus = 0;

            foreach (int currentStatus in acqModel.recordingEventQueue.GetConsumingEnumerable())
            {
                Debug.WriteLine("Recordcommander: Now recording schedule element {0} ...", currentStatus);
                //And here we can generate an event in the UI to refresh our RecordingPlan
                recordingStatus = currentStatus;
                if (currentStatus < 0)
                {
                    lastStatus = currentStatus;
                    break;
                }
            }

            switch (lastStatus)
            {
                case -1:
                    Debug.WriteLine("Recordcommander: Stopping the recording process!");
                    StopRecording();
                    break;
                case -2:
                    Debug.WriteLine("Recordcommander: The recording process was stopped elsewhere. Quitting...");
                    break;
                default:
                    break;
            }
        }



        /// <summary>
        /// Called on a separate thread, it listens to the progress monitor in the model
        /// </summary>
        void ShowProgress()
        {
            Debug.WriteLine("== ShowProgress starts");

            foreach (int currentProgress in acqModel.progressMonitor.GetConsumingEnumerable())
            {
                if (currentProgress == -1)
                {
                    recordingItemProgress = 0;
                    break;
                }

                else recordingItemProgress = currentProgress;

                Debug.WriteLine("Progress: " + currentProgress);
            }

            Debug.WriteLine("== ShowProgress ends");
        }



        public void StartRecording()
        {
            recording = true;

            //We empty the current graphs and set the first element in the recording plan as active


            foreach (GraphModel model in plotModels)
                model.graph.Clear();


            recordingPlanModel.SetSelectedItem(0);


            try
            {
                //Apply default configuration before recording
                LoadRecordingConfig(Properties.Settings.Default, acqModel.recordingConfig);
                //Start recording
                acqModel.StartRecording();
            }
            catch (AcqModelException ex)
            {
                recording = false;
                recordingPlanModel.SetSelectedItem(-1);
                throw new AcqViewModelException("AcqModel.StartRecording:" + ex.Message);
            }

            //A new thread listens on the recording event queue and triggers events that eventually will be listened to 
            //by some GUI code to generate on-screen instructions for the user while recording.

            _recordCommanderTask = Task.Run(new Action(RecordCommander));

            //A new thread monitorizes the data that are being recorded, converts them
            //to Datapoints and send them to the Pointlists.

            _fillGraphTask = Task.Run(new Action(FillGraph));

            //A new thread that will listen on the progress monitor of the model. 

            _showProgressTask = Task.Run(new Action(ShowProgress));

            readyToSave = false;
        }

        /// <summary>
        /// Calls the StopRecording method in the underlying model
        /// </summary>
        public void StopRecording()
        {
            try
            {
                acqModel.StopRecording();
            }
            catch (AcqModelException ex)
            {
                recordingPlanModel.SetSelectedItem(-1);
                throw new AcqViewModelException("AcqModel.StopRecording:" + ex.Message);
            }
            finally
            {
                recording = false;
                readyToSave = true;
            }
        }

        /// <summary>
        /// Calls the PauseRecording method in the underlying model
        /// </summary>
        public void PauseRecording()
        {
            try
            {
                acqModel.PauseRecording();
                paused = true;
            }
            catch (AcqModelException ex)
            {
                recordingPlanModel.SetSelectedItem(-1);
                throw new AcqViewModelException("AcqModel.PauseRecording:" + ex.Message);
            }

        }


        /// <summary>
        /// Calls the ResumeRecording method in the underlying model
        /// </summary>
        public void ResumeRecording()
        {
            try
            {
                acqModel.ResumeRecording();
                paused = false;
            }
            catch (AcqModelException ex)
            {
                recordingPlanModel.SetSelectedItem(-1);
                throw new AcqViewModelException("AcqModel.ResumeRecording:" + ex.Message);
            }
        }


        /// <summary>
        /// Calls the LoadFromFile method in the underlying model
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadFromFile(string fileName)
        {
            try
            {
                foreach (GraphModel model in plotModels)
                    model.graph.Clear();

                acqModel.LoadFromFile(fileName);
                RefreshGraphData();
            }
            catch (AcqModelException e)
            {
                throw new AcqViewModelException("LoadFromFile: Error loading file", e);
            }

        }


        /// <summary>
        /// Calls the SaveToFile method in the underlying model
        /// </summary>
        /// <param name="fileName">Name of the file to save</param>
        public void SaveToFile(string fileName)
        {
            try
            {
                acqModel.SaveToFile(fileName);
            }
            catch (AcqModelException e)
            {
                throw new AcqViewModelException("SaveToFile: Error saving file", e);
            }
        }

    }
}
