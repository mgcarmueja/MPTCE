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
    class WorkflowTestContainer : TestContainer
    {

        private ADS1298DataProvider _myDataProvider = new ADS1298DataProvider();
        private Queue<Object> _outputData= new Queue<Object>();
        private Pipeline _pipeline = new Pipeline();
        private OutputDisplayer _outputDisplayer = new OutputDisplayer();
        private ThresholdEngine _thresholdEngine = new ThresholdEngine();


        /// <summary>
        /// Loop for reading frames. It will be lancued on a new thread.
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
 
            _myDataProvider.Connect();
            _myDataProvider.Init();

            _thresholdEngine.inputQueue = _myDataProvider.outputQueue;

            _pipeline.AddStage(_thresholdEngine);
            _pipeline.AddStage(_outputDisplayer);

            _pipeline.Init();
            _pipeline.Start();
            _myDataProvider.Start();


            Thread.Sleep(20000);

            //Stopping data collection. 
            _myDataProvider.Stop();
            _pipeline.Stop();
            _myDataProvider.Disconnect();

            //printing results


            for (Int32 i = 0; i < _outputDisplayer.output.Count; i++)
            {
                Console.Out.WriteLine(_outputDisplayer.output.ElementAt(i));
            }
      

 
        }
    }
}
