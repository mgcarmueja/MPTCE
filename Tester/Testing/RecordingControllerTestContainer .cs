/*
 * Copyright 2015 Martin Garcia Carmueja 
 * 
 *  This file is part of Tester.
 *
 *  Tester is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Tester is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Tester. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EMGFramework.DataProvider;
using EMGFramework.ValueObjects;
using EMGFramework.Pipelines;
using EMGFramework.Utility;
using Tester.Stages;


namespace Tester.Testing
{
    class RecordingControllerTestContainer : TestContainer
    {

        private EMGDataProvider _myDataProvider = new PlaybackDataProvider();
        private Queue<Object> _outputData = new Queue<Object>();
        private Pipeline _pipeline = new Pipeline();

        private OutputDisplayer _outputDisplayer = new OutputDisplayer();
        private ThresholdEngine _thresholdEngine = new ThresholdEngine();

        private AcquisitionController _recordingController;
        private RecordingConfig _recordingConfig;

        private int _recordingStatus;

        public RecordingControllerTestContainer()
        {

        }

        /// <summary>
        /// Method for testing the reading of the current recording status through the concurrent queue from the RecordingController 
        /// This method will be called from an independent thread.
        /// </summary>
        public void GetRecordingStatusChange()
        { 
            BlockingCollection<int> statusQueue;

            statusQueue = _recordingController.recordingEventQueue;

            foreach (int currentItem in statusQueue.GetConsumingEnumerable())
            {
                Console.Out.WriteLine("Queue reports switch to schedule item {0}",currentItem);
                if (currentItem == -1) break;
            }

            Console.Out.WriteLine("The queue reading loop has finished!");

        }

        /// <summary>
        /// Handler for the OnStatusChange event from RecordingController. It will just display the status change for now
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RecordingStatusChange(object sender, EventArgs e)
        {
            RecordingStatusEventArgs myArgs = (RecordingStatusEventArgs)e;

            _recordingStatus = myArgs.status;

            Console.Out.WriteLine("Event reports switch to schedule item {0}", _recordingStatus);

        }

        /// <summary>
        /// Handler for the StopPendingChange event. Used to stop the pipeline when the pipeline itself demands it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StopPendingChange(object sender, EventArgs e)
        {

            if (_pipeline.stopPending == true)
            {
                Console.Out.WriteLine("Pipeline stop pending!");
                /*
                Console.Out.WriteLine("Pipeline stop pending! Stopping pipeline...");
                _pipeline.Stop();
                Console.Out.WriteLine("Pipeline stopped.");
                */

            }
        }



        /// <summary>
        /// Handler for the  OnStatusChange event from EMGDataProvider.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DataProviderStatusChange(object sender, EventArgs e)
        {
            EMGDataProviderStatusEventArgs myArgs = (EMGDataProviderStatusEventArgs)e;
            string status;
            
            switch (myArgs.status)
            {
                case DataProviderStatus.connected:
                    status = "connected";
                    break;
                case DataProviderStatus.disconnected:
                    status = "disconnected";
                    break;
                case DataProviderStatus.initialized:
                    status = "initialized";
                    break;
                case DataProviderStatus.paused:
                    status = "paused";
                    break;
                case DataProviderStatus.started:
                    status = "started";
                    break;
                case DataProviderStatus.stopped:
                    status = "stopped";
                    break;
                case DataProviderStatus.unknown:
                    status = "unknown";
                    break;
                default:
                    status = "null";
                    break;
            }

            Console.Out.WriteLine("our EMGDataProvider is now in status <{0}>", status);
        }




        /// <summary>
        /// Loop for reading frames. It will be launched on a new thread.
        /// </summary>
        private void Loop()
        {
            //BlockingCollection<Object> myBlockingCollection;

            /*
            myBlockingCollection = _myDataProvider.Start();
            foreach (EMGFramework.ValueObjects.Frame inputFrame in myBlockingCollection.GetConsumingEnumerable())
            {
                //Enqueue frame in a queue buffer
                _outputData.Enqueue(inputFrame);
            }
             * */

        }

        public override void Run()
        {
            ThreadStart runMethod;
            Thread myThread;

            runMethod = new ThreadStart(GetRecordingStatusChange);
            myThread = new Thread(runMethod);

            _recordingConfig = new RecordingConfig();
            _recordingConfig.schedule.Add(new ScheduleItem(7,new ushort[] { 1, 3 }));
            _recordingConfig.schedule.Add(new ScheduleItem (0, new ushort[] { 0 }));
            _recordingConfig.schedule.Add(new ScheduleItem (9,new ushort[] { 1, 4 }));
            _recordingConfig.schedule.Add(new ScheduleItem (0,new ushort[] { 0 }));
            _recordingConfig.schedule.Add(new ScheduleItem (8,new ushort[] { 2, 3 }));
            _recordingConfig.schedule.Add(new ScheduleItem (0, new ushort[] { 0 }));
            _recordingConfig.schedule.Add(new ScheduleItem (10, new ushort[] { 2, 4 }));

            _recordingController = new AcquisitionController(_pipeline);
            _recordingController.dataProvider = _myDataProvider;
            _myDataProvider.recordingConfig = _recordingConfig;
            _recordingController.recordingConfig = _recordingConfig;

            _recordingController.StatusChanged += new EventHandler(RecordingStatusChange);
            _pipeline.StopPendingChanged += new EventHandler(StopPendingChange);
            _myDataProvider.StatusChanged += new EventHandler(DataProviderStatusChange);

            _pipeline.AddStage(_recordingController);
            _pipeline.AddStage(_outputDisplayer);

            _pipeline.Init();

            //Ather the pipeline (and consequently the stages) are initialised, we can already create the
            //thread that will read the status change queue of the RecordingController object HERE
            myThread.Start();



            _pipeline.Start();



            while (_recordingController.currentScheduleItem != -1)
            {
                //Wait for the RecordingController to finish the job  
            }
            
            //The thread running GetRecordingStatusChange() is ready to finish or has already finished. We do things right
            //by waiting for it here.
            myThread.Join();


            if (_pipeline.stopPending)
            {
                Console.Out.WriteLine("Stopping pipeline on Run method...");
                _pipeline.Stop();
                Console.Out.WriteLine("Pipeline stopped!");
            }


            Console.WriteLine("\n\nRecordingController execution finished. Press <ENTER> to view the captured data.");
            Console.ReadLine();

            //printing results

            for (Int32 i = 0; i < _outputDisplayer.output.Count; i++)
            {
                Console.Out.WriteLine(_outputDisplayer.output.ElementAt(i));
            }

            _recordingController.StatusChanged -= new EventHandler(RecordingStatusChange);
            _pipeline.StopPendingChanged -= new EventHandler(StopPendingChange);
            _myDataProvider.StatusChanged -= new EventHandler(DataProviderStatusChange);


        }
    }
}
