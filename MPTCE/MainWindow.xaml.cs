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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using QDGraph;
using RecordingPlan;
using MPTCE.Model;
using MPTCE.ViewModel;
using MPTCE.RealtimeConsumers;
using EMGFramework.ValueObjects;
using EMGFramework.Utility;



namespace MPTCE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private System.Windows.Forms.OpenFileDialog openFileDialog = new OpenFileDialog();
        private System.Windows.Forms.SaveFileDialog saveFileDialog = new SaveFileDialog();
        private Graph[] _acqGraphs = null;
        private IntervalMarker[] _acqIntervals = null;
        private IntervalMarker[] _reaIntervals = null;
        private ThresholdMarker[] _reaThresholds = null;
        private Graph[] _reaGraphs = null;


        /// <summary>
        /// Action for setting the button content of the record and load buttons of the acquistion stage
        /// through the Dispatcher
        /// </summary>
        /// <param name="recordButtonContent"></param>
        /// <param name="loadButtonContent"></param>
        private void AcqSetButtons(string recordButtonContent, string loadButtonContent)
        {
            this.acqRecordButton.Content = recordButtonContent;
            this.acqLoadButton.Content = loadButtonContent;
        }


        /// <summary>
        /// Action for setting the button content of the treat and load buttons of the treatment stage
        /// through the Dispatcher
        /// </summary>
        /// <param name="treatButtonContent"></param>
        /// <param name="loadButtonContent"></param>
        private void TrtSetButtons(string treatButtonContent)
        {
            this.trtTreatButton.Content = treatButtonContent;
        }


        /// <summary>
        /// Action for setting the button content of the train and load buttons of the training stage
        /// through the Dispatcher
        /// </summary>
        /// <param name="trainButtonContent"></param>
        /// <param name="loadButtonContent"></param>
        private void TraSetButtons(string trainButtonContent)
        {
            this.traTrainButton.Content = trainButtonContent;
        }



        /// <summary>
        /// Action for setting the button content of the start button of the realtime stage
        /// through the Dispatcher
        /// </summary>
        /// <param name="startButtonContent"></param>
        private void ReaSetButtons(string startButtonContent)
        {
            this.reaStartButton.Content = startButtonContent;

        }



        /// <summary>
        /// Action for showing a save dialog and then saving the acquisition data in a file through 
        /// the Dispatcher.
        /// </summary>
        private void AcqSaveFile()
        {

            saveFileDialog.Filter = acqViewModel.fileFilters;
            //Show save dialog
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    //Save to the selected filename
                    acqViewModel.SaveToFile(saveFileDialog.FileName);
                }
                catch (AcqViewModelException)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show(this, "There was an error saving the recording.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        } 
 


        public MainWindow()
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            InitializeComponent();

            //ACQUISITION

            //Here we configure the acqMovementsListBox with some default fields active.
            //This values should come from some configuration source.
            //Even with that, this is probably not the best way to deal with the task.
            acqMovementsListBox.SelectedItems.Add(acqMovementsListBox.Items[0]);
            acqMovementsListBox.SelectedItems.Add(acqMovementsListBox.Items[2]);

            acqMovementsListBox.Items.Refresh();
            //******
            acqViewModel.reaViewModel = reaViewModel;
            trtViewModel.acqViewModel = acqViewModel;
            trtViewModel.reaViewModel = reaViewModel;
            traViewModel.acqViewModel = acqViewModel;
            traViewModel.reaViewModel = reaViewModel;
            traViewModel.trtViewModel = trtViewModel;


            acqViewModel.Configure();
            acqViewModel.recordingPlanModel.recordingPlan = recordingPlan;
            acqViewModel.PropertyChanged += AcqViewModelPropertyChanged;

            //TREATMENT
            trtViewModel.Configure();
            trtViewModel.PropertyChanged += TrtViewModelPropertyChanged;

            //TRAINING
            traViewModel.dispatcher = Dispatcher;
            traViewModel.Configure();
            traViewModel.PropertyChanged += traViewModel_PropertyChanged;

            //REALTIME
            reaComboBoxColumn.ItemsSource = reaViewModel.movements;
            reaViewModel.Configure();
            reaViewModel.PropertyChanged += reaViewModel_PropertyChanged;


            //Populate realtime pane with RealtimeConsumer objects from the viewModel
            foreach (RealtimeConsumer item in reaViewModel.consumerList)
                reaWrapPanel.Children.Add(item.consumerControl);

            //Add the movement display user control to its Grid
            reaMovDisplayGrid.Children.Clear();
            reaMovDisplayGrid.Children.Add(reaViewModel.movDisplayConsumer.consumerControl);


            PrepareGraphs();

            System.Windows.Application.Current.Activated += Application_Activated;
            System.Windows.Application.Current.Deactivated += Application_Deactivated;

        }

        /// <summary>
        /// Handler for the event generated when the application becomes active
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Activated(object sender, EventArgs e)
        {
            Console.Out.WriteLine("Application active.");

            if (reaViewModel != null)
                reaViewModel.applicationActive = true;
        }

        /// <summary>
        /// Handler for the event generated when the application becomes inactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Deactivated(object sender, EventArgs e)
        {
            Console.Out.WriteLine("Application inactive.");

            if (reaViewModel != null)
                reaViewModel.applicationActive = false;
        }



        /// <summary>
        /// Initialization of the view components for all graphs in the program
        /// </summary>
        private void PrepareGraphs()
        {
            //Acquisition
            _acqGraphs = new Graph[acqViewModel.plotModels.Length];
            _acqIntervals = new IntervalMarker[acqViewModel.plotModels.Length];

            for (int i = 0; i < acqViewModel.plotModels.Length; i++)
            {
                _acqGraphs[i] = new Graph();
                _acqGraphs[i].leftTitle = (i+1).ToString();
                _acqGraphs[i].isToggleable = true;

                _acqIntervals[i] = new IntervalMarker();
                _acqIntervals[i].visualIndex = -1;
                _acqIntervals[i].color = Color.FromArgb(64, 0, 255, 0);
                _acqIntervals[i].visible = false;
                _acqGraphs[i].graphMarkers.Add(_acqIntervals[i]);


                acqViewModel.plotModels[i].graph = _acqGraphs[i];
                _acqGraphs[i].Margin = new Thickness(2);
                _acqGraphs[i].bottomEdge = 1;
                _acqGraphs[i].topEdge = 1;
                
                acqGraphsGrid.Children.Add(_acqGraphs[i]);

                _acqGraphs[i].toggle = true;
            }

            //Realtime -> Should be something similar
            _reaGraphs = new Graph[reaViewModel.plotModels.Length];
            _reaIntervals = new IntervalMarker[reaViewModel.plotModels.Length];
            _reaThresholds = new ThresholdMarker[reaViewModel.plotModels.Length];

            for (int i = 0; i < reaViewModel.plotModels.Length; i++)
            {
                _reaGraphs[i] = new Graph();
                _reaGraphs[i].leftTitle = (i+1).ToString();
                _reaGraphs[i].isToggleable = false;

                _reaIntervals[i] = new IntervalMarker();
                _reaIntervals[i].visualIndex = -1;
                _reaIntervals[i].color = Color.FromArgb(64, 0, 255, 0);
                _reaIntervals[i].visible = false;
                _reaGraphs[i].graphMarkers.Add(_reaIntervals[i]);

                _reaThresholds[i] = new ThresholdMarker();
                _reaThresholds[i].visualIndex = 1;
                _reaThresholds[i].color = Color.FromArgb(255, 255, 0, 255);
                _reaThresholds[i].visible = false;
                _reaGraphs[i].graphMarkers.Add(_reaThresholds[i]);

                reaViewModel.plotModels[i].graph = _reaGraphs[i];
                _reaGraphs[i].Margin = new Thickness(2);
                reaGraphsGrid.Children.Add(_reaGraphs[i]);
            }
        }


        /// <summary>
        /// This event handler provides visual feedback on the user interface each time the 
        /// recording and paused properties in the acquisition view model change. This covers
        /// the case when those properties are changed from inside the view model itself and
        /// not as a result of pressing a button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void AcqViewModelPropertyChanged(Object sender, PropertyChangedEventArgs eventArgs)
        {
            string recordButtonContent, loadButtonContent;

            switch (eventArgs.PropertyName)
            {
                case "recording":
                    if (acqViewModel.recording)
                    {
                        recordButtonContent = "Pause";
                        loadButtonContent = "Cancel";
                    }
                    else
                    {
                        recordButtonContent = "Record";
                        loadButtonContent = "Load";
                    }
                    Dispatcher.Invoke(new Action<string, string>(AcqSetButtons), recordButtonContent, loadButtonContent);
                    break;

                case "paused":
                    if (acqViewModel.paused)
                    {
                        recordButtonContent = "Resume";
                        loadButtonContent = "Cancel";
                    }
                    else
                    {
                        recordButtonContent = "Pause";
                        loadButtonContent = "Cancel";
                    }

                    Dispatcher.Invoke(new Action<string, string>(AcqSetButtons), recordButtonContent, loadButtonContent);
                    break;

                case "pointLists":
                    int graphPointsToCommit = acqViewModel.graphPointsToCommit;
                    Dispatcher.BeginInvoke((Action)(() => { foreach (GraphModel model in acqViewModel.plotModels) model.CommitToGraph(graphPointsToCommit); }));
                    break;

                case "readyToSave":
                    if (acqViewModel.readyToSave)
                    {
                        Dispatcher.Invoke(new Action(AcqSaveFile));
                    }
                    break;

                case "recordingStatus":

                    Dispatcher.BeginInvoke((Action)(() => { acqViewModel.recordingPlanModel.SetSelectedItem(acqViewModel.recordingStatus); }));

                    break;

                case "recordingItemProgress":

                    Dispatcher.BeginInvoke((Action)(() => { acqViewModel.recordingPlanModel.SetProgress(acqViewModel.recordingItemProgress); }));

                    break;

                case "recordingValid":
                case "detectThresholds":

                    if (acqViewModel.detectThresholds && acqViewModel.recordingValid)
                    {
                        Dispatcher.BeginInvoke((Action)(() =>
                            {
                                for (int i = 0; i < _acqIntervals.Length; i++)
                                {
                                    _acqIntervals[i].lowerLimit = acqViewModel.recordedData.parameters.thresholdSet.minValues[i];
                                    _acqIntervals[i].upperLimit = acqViewModel.recordedData.parameters.thresholdSet.maxValues[i];
                                    _acqIntervals[i].visible = true;
                                }
                            }
                        ));
                    }

                    else
                    {
                        Dispatcher.BeginInvoke((Action)(() =>
                            {

                                for (int i = 0; i < _acqIntervals.Length; i++)
                                {
                                    _acqIntervals[i].visible = false;
                                }
                            }
                        ));
                    }


                    break;
                case "thresholdRecordingConfig":

                    Dispatcher.BeginInvoke((Action)(() =>
                           {
                               reaViewModel.thresholdRecordingConfig = acqViewModel.thresholdRecordingConfig;
                           }
                   ));

                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// This event handler provides visual feedback on the user interface each time the 
        /// recording and paused properties in the acquisition view model change. This covers
        /// the case when those properties are changed from inside the view model itself and
        /// not as a result of pressing a button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void TrtViewModelPropertyChanged(Object sender, PropertyChangedEventArgs eventArgs)
        {
            string treatButtonContent;

            switch (eventArgs.PropertyName)
            {
                case "treated":
                    if (trtViewModel.treated)
                    {
                        treatButtonContent = "Clear";
                    }
                    else
                    {
                        treatButtonContent = "Treat";
                    }
                    Dispatcher.Invoke(new Action<string>(TrtSetButtons), treatButtonContent);
                    break;

                default:
                    break;
            }
        }


        void traViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string trainButtonContent;

            switch (e.PropertyName)
            {
                case "trained":
                case  "training":
                    if (traViewModel.trained)
                    {
                        trainButtonContent = "Clear";
                    }
                    else
                    {
                        if (traViewModel.training) trainButtonContent = "Stop";
                        else trainButtonContent = "Train";
                    }
                    Dispatcher.Invoke(new Action<string>(TraSetButtons), trainButtonContent);
                    break;
                    
                case "concurrentLogList":

                    Dispatcher.BeginInvoke((Action)(() =>
                        {
                            ProgressLogItem item;

                            while (traViewModel.concurrentLogList.TryTake(out item, 10))
                                traViewModel.logItems.Add(item);

                        }));

                    break;

                default:
                    break;
            }
        }


        void reaViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string startButtonContent;

            switch (e.PropertyName)
            {
                case "isRunning":
                    if (reaViewModel.isRunning)
                        startButtonContent = "Stop";
                    else startButtonContent = "Start";
                    Dispatcher.Invoke(new Action<string>(ReaSetButtons), startButtonContent);
                    break;

                case "pointLists":
                    int graphPointsToCommit = reaViewModel.graphPointsToCommit;
                    Dispatcher.BeginInvoke((Action)(() => { foreach (GraphModel model in reaViewModel.plotModels) model.CommitToGraph(graphPointsToCommit); }));
                    break;

                case "levelControlled":


                    if (reaViewModel.levelControlled && reaViewModel.readyToRun)
                    {
                        Dispatcher.BeginInvoke((Action)(() =>
                        {

                            foreach (ThresholdControl control in reaViewModel.thresholdControls)
                            {
                                int pos = (int)control.channel-1;

                                _reaIntervals[pos].lowerLimit = control.thresholdMin;
                                _reaIntervals[pos].upperLimit = control.thresholdMax;
                                _reaIntervals[pos].visible = true;

                                _reaThresholds[pos].thresholdValue = control.threshold;
                                _reaThresholds[pos].visible = true;
                            }
                        }
                        ));
                    }

                    else
                    {
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            foreach (ThresholdControl control in reaViewModel.thresholdControls)
                            {
                                int pos = (int)control.channel-1;
                                _reaIntervals[pos].visible = false;
                                _reaThresholds[pos].visible = false;
                            }
                        }
                        ));
                    }
                    break;

                case "threshold":

                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        foreach (ThresholdControl control in reaViewModel.thresholdControls)
                        {
                            int pos = (int)control.channel-1;

                            _reaThresholds[pos].thresholdValue = control.threshold;

                        }
                    }));

                    break;

                default:
                    break;
            }
        }



        private void acqLoadButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button thisButton = (System.Windows.Controls.Button)sender;

            if ((!acqViewModel.recording) && (!acqViewModel.paused))
            {

                //We load a recording from file and FILTER the files by name 
                //by using a filter defined in the configuration file
                openFileDialog.Filter = acqViewModel.fileFilters;
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        acqViewModel.LoadFromFile(openFileDialog.FileName);
                    }
                    catch (AcqViewModelException)
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show(this, "There was an error loading the recording.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
  
                }
            }
            else
            {
                //We cancel the current recording -> stop + erase the recorded contents
                acqViewModel.StopRecording();
                //TODO Now we erase the already recorded data.
            }
        }


        private void acqRecordButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button thisButton = (System.Windows.Controls.Button)sender;

            if (acqViewModel.recording)
            {
                //We pause the recording, which is the function of the button now.
                if (!acqViewModel.paused) acqViewModel.PauseRecording();
                else acqViewModel.ResumeRecording();
            }
            else
            {
                //We start recording. 
                //Perhaps also here we should do something to make a panel with indications for 
                //the recording process appear and behave itself.
                try
                {
                    acqViewModel.StartRecording();
                }
                catch (AcqViewModelException)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show(this, "Could not start acquisition!\nPlease make sure that your device is connected to the computer and its server program is running.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void trtTreatButton_Click(object sender, RoutedEventArgs e)
        {
            //If we haven't already treated something:

            if (!trtViewModel.treated)
            {
                //We should do this only if there is a recording stored in the acqViewModel
                if (acqViewModel.recordingValid)
                {
                    trtViewModel.trtModel.acqRecording = acqViewModel.recordedData;
                    trtViewModel.Treat();
                }
                else
                {
                    //and if not, popup window with an error.
                }
            }

            else
            {
                trtViewModel.ClearData();
            }

        }

        /*
        private void trtLoadButton_Click(object sender, RoutedEventArgs e)
        {
            //If we haven't already treated something, we show a load dialog, load 
            //a file and change Contents of "treat" and "load" buttons to "save" and "clear"
            //and set the flag that says "we have already treated something" in the ViewModel
            if (!trtViewModel.treated)
            {
                //TODO We load a recording from file and FILTER the files by name 
                //by using a filter defined in the configuration file

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    trtViewModel.LoadFromFile(openFileDialog.FileName);
                }

            }
            else
            {
                trtViewModel.ClearData();
            }

        }
        */

        private void traTrainButton_Click(object sender, RoutedEventArgs e)
        {
            //If we haven't already treated something:

            if (!traViewModel.trained)
            {
                if (traViewModel.training) traViewModel.interruptTraining = true;
                else
                    //We should do this only if there is a recording stored in the acqViewModel
                    if (trtViewModel.treated)
                    {
                        traViewModel.RunTraining();
                    }
                    else
                    {
                        //and if not, popup window with an error.
                    }
            }

            else
            {
                traViewModel.ClearData();
            }

        }

        /*
        private void traLoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (!traViewModel.trained)
            {
                //TODO We load a recording from file and FILTER the files by name 
                //by using a filter defined in the configuration file

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    traViewModel.LoadFromFile(openFileDialog.FileName);
                }

            }
            else
            {
                traViewModel.ClearData();
            }

        }
        */

        private void reaStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!reaViewModel.isRunning)
            {
                try
                {
                    reaViewModel.Start();
                }
                catch (ReaViewModelException)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show(this, "Could not start acquisition!\nPlease make sure that your device is connected to the computer and its server program is running.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                reaViewModel.Stop();
            }
        }


        private void acqMovementsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ListBox myListBox = (System.Windows.Controls.ListBox)sender;
            List<int> posToErase = new List<int>();

            //We add to the list selectedMovements in the viewModel the elements of myListBox.SelectedItems that are not in it
            //and remove the elements that are no longer in myListBox.SelectedItems

            foreach (Movement movement in myListBox.SelectedItems)
                if (!acqViewModel.selectedMovements.Contains(movement))
                {
                    int position = 0;

                    if (acqViewModel.selectedMovements.Count > 0)
                    {
                        int maxIndex = acqViewModel.selectedMovements.Count;

                        while ((position < maxIndex) &&
                            (acqViewModel.selectedMovements.ElementAt(position).idTag < movement.idTag))
                            position++;
                    }

                    acqViewModel.selectedMovements.Insert(position, movement);
                }

            foreach (Movement movement in acqViewModel.selectedMovements)
                if (!myListBox.SelectedItems.Contains(movement)) posToErase.Add(acqViewModel.selectedMovements.IndexOf(movement));

            foreach (int index in posToErase)
                acqViewModel.selectedMovements.Remove(acqViewModel.selectedMovements.ElementAt(index));
        }


        private void trtFeaturesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ListBox myListBox = (System.Windows.Controls.ListBox)sender;


            trtViewModel.selectedFeatures.Clear();
            foreach (string feature in myListBox.SelectedItems)
                trtViewModel.selectedFeatures.Add(feature);

        }


        private void traSelectionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ListBox myListBox = (System.Windows.Controls.ListBox)sender;
            List<DataSet> posToErase = new List<DataSet>();

            foreach (DataSet dataset in myListBox.SelectedItems)
            {

                if (!traViewModel.selectedTrainingSets.Contains(dataset))
                {
                    int position = 0;
                    if (traViewModel.selectedTrainingSets.Count > 0)
                    {
                        int maxIndex = traViewModel.selectedTrainingSets.Count;

                        while ((position < maxIndex) &&
                            (traViewModel.selectedTrainingSets.ElementAt(position).movementCode < dataset.movementCode))
                            position++;

                    }

                    traViewModel.selectedTrainingSets.Insert(position, dataset);

                }

            }

            foreach (DataSet dataSet in traViewModel.selectedTrainingSets)
                if (!myListBox.SelectedItems.Contains(dataSet)) posToErase.Add(dataSet);

            foreach (DataSet dataSet in posToErase)
                traViewModel.selectedTrainingSets.Remove(dataSet);

        }



    }
}