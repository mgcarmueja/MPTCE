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
using EMGFramework.DataProvider.ADS1298;
using EMGFramework.DataProvider.Dummy;
using EMGFramework.ValueObjects;

namespace EMGClient.Testing
{
    class DataProviderTestContainer : TestContainer
    {

        private EMGDataProvider _myDataProvider = new DummyDataProvider(); //ADS1298DataProvider();
        private Queue<Object> _outputData= new Queue<Object>();

        /// <summary>
        /// Loop for reading frames. It will be launched on a new thread.
        /// </summary>
        private void FrameLoop()
        {
            BlockingCollection<Object> myBlockingCollection;
            myBlockingCollection = _myDataProvider.Start();
            foreach (EMGFramework.ValueObjects.Frame inputFrame in myBlockingCollection.GetConsumingEnumerable())
            {
                //Enqueue frame in a queue buffer
                _outputData.Enqueue(inputFrame);
            }

            Console.Out.WriteLine("Leaving frame reading loop!");

        }
        
        public override void Run()
        {

            ThreadStart runMethod;
            Thread myThread;
            Int32 i, nItems;

            _myDataProvider.Connect();
            _myDataProvider.Init();

            //Start the thread that will read and display data frames
            runMethod = new ThreadStart(FrameLoop);
            myThread = new Thread(runMethod);

            Console.Out.WriteLine("Starting data collection...");
            myThread.Start();

            //Wait a bit, so that we display some data
            Thread.Sleep(1000);

            //Pausing data collection. 
            Console.Out.WriteLine("Pausing data collection...");
            _myDataProvider.Pause();
            
            Thread.Sleep(1000);
            
            //Resuming data collection.
            Console.Out.WriteLine("Resuming data collection...");
            _myDataProvider.Start();
            
            Thread.Sleep(1000);

            //Definitively stopping data collection. 
            _myDataProvider.Stop();

            Console.Out.WriteLine("Data collection complete!");

            //Wait for our thread to finish
            myThread.Join();

            _myDataProvider.Disconnect();

            //Now we print all the enqueued frames

            nItems = _outputData.Count();

            Console.Out.WriteLine("{0} processed frames in total. Press Enter to view them.", nItems);
            Console.ReadLine();

            for (i = 0; i < nItems; i++)
            {
                Console.Out.WriteLine((Frame) _outputData.Dequeue());
            }


        }
    }
}
