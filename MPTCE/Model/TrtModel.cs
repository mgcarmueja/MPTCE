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
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMGFramework.ValueObjects;
using EMGFramework.Pipelines;
using EMGFramework.Utility;


namespace MPTCE.Model
{
    /// <summary>
    /// This class implements the model of the treatment stage. It is responsible of processing a Recording into 
    /// a TrainingPackage containing DataWindow objects distributed across different DataSet objects. Each DataSet object
    /// corresponds to a given movement code. Two DataSet objects are created for each movement: one for training and 
    /// other for verification purposes. Once created, the TrainingPackage will be made available through a public property.
    /// From there, it can be delivered to the TraModel object responsible for training a PatternRecognizer object.
    /// </summary>
    public class TrtModel:INotifyPropertyChanged
    {
        
        private ObservableCollection<Frame> _preTreatedFrames;


        private Pipeline _pipeline;
        private WindowMaker _windowMaker;
        private FeatureExtractor _featureExtractor;
        private List<DataWindow> _treatedWindows;
        private Recording _pretreatedRecording;


        /// <summary>
        /// Event handler that fires whenever a property has been changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propName"></param>
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


        private TreatmentConfig _treatmentConfig;
        /// <summary>
        /// Model treatment configuration
        /// </summary>
        public TreatmentConfig treatmentConfig
        {
            get
            {
                return _treatmentConfig;
            }

            private set
            {
                if (_treatmentConfig != value)
                {
                    _treatmentConfig = value;
                    this.NotifyPropertyChanged("treatmentConfig");
                }
            }
        }



        private ObservableCollection<string> _selectedFeatures;
        /// <summary>
        /// Model list of selected features
        /// </summary>
        public ObservableCollection<string> selectedFeatures
        {
            get
            {
                return _selectedFeatures;
            }

        }


        private Recording _acqRecording;
        /// <summary>
        /// Acquisition recording, either recorded or loaded from a file at the acquisition stage.
        /// </summary>
        public Recording acqRecording
        {
            get
            {
                return _acqRecording;
            }
            set
            {
                if (_acqRecording != value)
                {
                    _acqRecording = value;
                    this.NotifyPropertyChanged("acqRecording");
                }
            }
        }


        private TrainingPackage _trainingPackage;
        /// <summary>
        /// This is the product of the treatment stage: a package containing one training set and one
        /// validation set for each movement in the recording received from the acquisition stage.
        /// </summary>
        public TrainingPackage trainingPackage
        {
            get
            {
                return _trainingPackage;
            }
            set
            {
                if (_trainingPackage != value)
                {
                    trainingPackage = value;
                    if (value == null)
                        treated = false;
                    else treated = true;
                    this.NotifyPropertyChanged("trainingPackage");
                }
            }
        }


        private bool _treated;
        /// <summary>
        /// True if a TrainingPackage was generated in the model.
        /// False if it was not generated or if it was cleared
        /// </summary>
        public bool treated
        {
            get
            {
                return _treated;
            }
            private set
            {
                if (_treated != value)
                {
                    _treated = value;
                    this.NotifyPropertyChanged("treated");
                }
            }
        }



        private bool _randomWindowSelection;
        /// <summary>
        /// True if the windows for a given movement are to be assigned randomly either to the training
        /// set or to the validation set. False otherwise.
        /// </summary>
        public bool randomWindowSelection
        {
            get 
            {
                return _randomWindowSelection;
            }

            set
            {
                if (_randomWindowSelection != value)
                {
                    _randomWindowSelection = value;
                    this.NotifyPropertyChanged("randomWindowSelection");
                }
            }
        }


        private bool _includeRests;
        /// <summary>
        /// True if rest-related DataWindows should be included in the TrainingPackage. False otherwise.
        /// </summary>
        public bool includeRests
        {
            get 
            {
                return _includeRests;
            }

            set 
            {
                if (_includeRests!=value)
                {
                    _includeRests = value;
                    this.NotifyPropertyChanged("includeRests");
                }
            }
        }


        private bool _running;
        /// <summary>
        /// True while the treatment process for feature extraction is running. False otherwise.
        /// </summary>
        public bool running
        {
            get
            {
                return _running;
            }

            private set
            {
                if (_running != value)
                {
                    _running = value;
                    this.NotifyPropertyChanged("running");
                }
            }
                
        }


        /// <summary>
        /// 
        /// </summary>
        public TrtModel()
        {
            _treatmentConfig = new TreatmentConfig();
            _selectedFeatures = new ObservableCollection<string>();
            _selectedFeatures.CollectionChanged += SelectedFeaturesChanged;
            _pipeline = new Pipeline();
            _preTreatedFrames = new ObservableCollection<Frame>();
        }


        /// <summary>
        /// Initialization of components and configurations
        /// </summary>
        public void Init()
        {
            _windowMaker = new WindowMaker();
            _windowMaker.isOnline = false;
            
            _featureExtractor = new FeatureExtractor();
            _featureExtractor.isOnline = false;

            _pipeline.AddStage(_windowMaker);
            _pipeline.AddStage(_featureExtractor);

            _pretreatedRecording = new Recording(_preTreatedFrames);
            _windowMaker.homePipeline = _pipeline;
            _windowMaker.recording = _pretreatedRecording;
            _windowMaker.config = _treatmentConfig;

        }


        /// <summary>
        /// Starting from data recorded or loaded at the acquisition stage, this method produces a training set for the
        /// training stage
        /// </summary>
        public void ProcessRecording()
        {
            running = true;

            _pretreatedRecording.parameters = _acqRecording.parameters;
            _treatmentConfig.features = selectedFeatures.ToList<string>();
            _treatmentConfig.features.Sort();

            _featureExtractor.selectedFeatures = _treatmentConfig.features;
            
            _trainingPackage = new TrainingPackage();

            Pretreat();

            _pipeline.Init();
            _pipeline.Start();

            foreach (object generic in _windowMaker.lockingCollection.GetConsumingEnumerable()) ;
            //A dirty way of waiting for the WindowMaker stage to finish!
            _pipeline.Stop();

            _treatedWindows = _featureExtractor.outputData;


            _trainingPackage.recordingConfig = _acqRecording.parameters;
            //The following line is intended to make the DataProviders work as intended with online data. 
            _trainingPackage.recordingConfig.scheduleWarmupItems = 0;
            _trainingPackage.treatmentConfig = _treatmentConfig;
            

            BuildTrainingPackage();

            treated = true;

            running = false;
            //And hopefully we have now a training package :)
        }



        /// <summary>
        /// Processes a list of input frames discarding all of the frames related to rest periods
        /// and trimming the beginnings and ends of the contraction periods by a factor given by the 
        /// contractionTimePercentage attribute of the TreatmentConfig class. 
        /// </summary>
        private void Pretreat()
        {
            int startPos = 0;
            int endPos = 0;
            int count = 0;
            int trimAmount;

            ObservableCollection<Frame> inputFrames = (ObservableCollection<Frame>)_acqRecording.data;
            _preTreatedFrames.Clear();


            while (startPos < inputFrames.Count)
            {
                //First, we skip any rest-related frames by checking the movement code of each frame for a 0
                
                if(!includeRests)
                while ((startPos < inputFrames.Count) && (inputFrames.ElementAt(startPos).movementCode == 0))
                    startPos++;
                


                //Find the end frame of the movement starting in startpos
                endPos = startPos;

                while ((endPos < inputFrames.Count) && (inputFrames.ElementAt(startPos).movementCode == inputFrames.ElementAt(endPos).movementCode))
                    endPos++;

                count = (endPos - startPos); //number of items contained in this interval

                endPos--; //This is the actual end position

                //Now we see how much we have to trim on each side
                trimAmount = (int)(count * (1 - _treatmentConfig.contractionTimePercentage) / 2.0);

                //and we trim!
                for (int i = startPos + trimAmount; i <= endPos - trimAmount; i++)
                    _preTreatedFrames.Add(inputFrames.ElementAt(i));

                //We move to the element next to endPos and start over
                startPos = endPos + 1;
            }
        }


        /// <summary>
        /// Arranges the treated sets into a TrainingPackage object, where they are grouped by movement
        /// and by type: either training or validation windows 
        /// </summary>
        private void BuildTrainingPackage()
        {
            int movementCode;
            List<DataSet> selectedDataSetList; List<DataSet> otherDataSetList;
            bool isMixed;
            int movCode, count, position;
            List<object> movementComposition;



            Random rnd = new Random();
            foreach (DataWindow window in _treatedWindows)
            {

                isMixed = false;

                int targetSet;

                movementCode = -1;

                //First, check that the dataWindow only contains frames for a given movement
                foreach (Frame frame in window.frames)
                {
                    if ((movementCode != -1) && (frame.movementCode != movementCode))
                    {
                        isMixed = true;
                        break;
                    }
                    movementCode = frame.movementCode;
                }

                if (!isMixed)
                {
                    //We see which movement is encoded in the tagList and add this DataWindow randomly to either 
                    //the training list or the validation list for that movement. If an entry for the movement does not
                    //yet exist at any of those lists, it will be created

                    movCode = window.frames[0].movementCode;
                    movementComposition = window.frames[0].tagList;

                    if (randomWindowSelection) targetSet = rnd.Next(2);
                    else targetSet = 0;
                    

                    //Now, where shoud we put this window, in a training set or in a verification set?
                    if (targetSet == 0)
                    {
                        selectedDataSetList = _trainingPackage.trainingSets; //In a training set
                        otherDataSetList = _trainingPackage.validationSets;
                    }
                    else
                    {
                        selectedDataSetList = _trainingPackage.validationSets; //In a validation set
                        otherDataSetList = _trainingPackage.trainingSets;
                    }

                    count = 0;
                    foreach (DataSet dataSet in selectedDataSetList)
                    {
                        if (dataSet.movementCode == movCode)
                        {
                            dataSet.set.Add(window);
                            break;
                        }
                        count++;
                    }

                    if (count == selectedDataSetList.Count)
                    //That is, we did not found the dataSet we were looking for
                    {
                        List<DataWindow> dataWindowList = new List<DataWindow>();
                        dataWindowList.Add(window);

                        //Just in case, we already create the selected and the accompanying unselected set. 
                        
                        selectedDataSetList.Add(new DataSet(movCode, dataWindowList,movementComposition));
                        otherDataSetList.Add(new DataSet(movCode,movementComposition));
                        
                        if (!_trainingPackage.movementCodes.Contains(movCode)) 
                            _trainingPackage.movementCodes.Add(movCode);

                    }
                }
            }


            //Now we have randomly distributed the windows for each movement into training sets and validation sets,
            //so they will be more or less 50% distributed between training and validation set for a given movement.
            //Alternatively, if windows were not randomized, we have a training set with more windows than we need
            //and a completely empty validation set. Either way, this is not what we normally want, so now we will 
            //randomly (or non-randomly) move DataWindows between training set and validation set for each movement to 
            //get the numbers we need for each set (trainingSetSize and validationsetSize).

            rnd = new Random();


            List<DataWindow> sourceList, destinationList;
            int sourceTargetSize, destinationTargetSize;

            for (int i = 0; i < _trainingPackage.trainingSets.Count; i++)
            {
                if (_treatmentConfig.trainingSetSize > _treatmentConfig.validationSetSize)
                {
                    sourceList = _trainingPackage.validationSets.ElementAt(i).set;
                    destinationList = _trainingPackage.trainingSets.ElementAt(i).set;
                    sourceTargetSize = _treatmentConfig.validationSetSize;
                    destinationTargetSize = _treatmentConfig.trainingSetSize;
                }
                else
                {
                    sourceList = _trainingPackage.trainingSets.ElementAt(i).set;
                    destinationList = _trainingPackage.validationSets.ElementAt(i).set;
                    sourceTargetSize = _treatmentConfig.trainingSetSize;
                    destinationTargetSize = _treatmentConfig.validationSetSize;
                }

                //First, we move enough DataWindow objects from sourceList to destinationList
                //until the targetSize is met

                while (destinationList.Count < destinationTargetSize && sourceList.Count>0)
                {
                    if (randomWindowSelection)
                        position = rnd.Next(sourceList.Count);
                    else position = sourceList.Count - 1;

                    destinationList.Insert(0,sourceList.ElementAt(position));
                    sourceList.RemoveAt(position);
                }


                while (destinationList.Count > destinationTargetSize && destinationList.Count>0)
                {
                    if (randomWindowSelection)
                        position = rnd.Next(destinationList.Count);
                    else position = destinationList.Count - 1;

                    sourceList.Insert(0,destinationList.ElementAt(position));
                    destinationList.RemoveAt(position);
                }

                //And now we remove the excess DataWindows from the sourceList, should there be any.
            
                //TODO (maybe):
                //Before the loop, if sourceList.Count < sourceTargetSize, we clearly have a problem: not enough
                //DataWindow objects to fulfill our goal. WE SHOULD WARN HERE! --> Throw a TrtModelException?
                
                while (sourceList.Count > sourceTargetSize && sourceList.Count>0)
                {
                    if (randomWindowSelection)
                        position = rnd.Next(sourceList.Count);
                    else position = 0;

                    sourceList.RemoveAt(position);
                }

            }
        }


        /// <summary>
        /// Initializes a new TrainingPackage, effectively dereferencing the existing one
        /// </summary>
        public void ClearTreatment()
        {
            _trainingPackage = new TrainingPackage();
            treated = false;
        }


        private void SelectedFeaturesChanged(object sender, NotifyCollectionChangedEventArgs args)
        {

        }

    }
}
