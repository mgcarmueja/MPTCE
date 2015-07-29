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
using System.ServiceModel;
using System.ServiceModel.Channels;
using ADS1298Intercom;
using Tester.Stages;
using EMGFramework.Pipelines;
using EMGFramework.ValueObjects;
using EMGFramework.Utility;
using Tester.Testing;
using System.Windows.Forms;


namespace Tester
{
    class Tester
    {
        [STAThread]
        public static void Main(string[] args)
        {
            //TestContainer myTestContainer;
            
            Console.SetWindowSize(130, 50);
            Console.SetBufferSize(130, 5000);


            
            EMGCallbacks myCallbacks = new EMGCallbacks();              
            // TEST - EMG Server
            DuplexChannelFactory<IEMGDevice> pipeFactory = new DuplexChannelFactory<IEMGDevice>(
                            myCallbacks,
                            new NetNamedPipeBinding(),
                            new EndpointAddress("net.pipe://localhost/PipeEMGServer"));

            IEMGDevice pipeProxy = pipeFactory.CreateChannel();

            pipeProxy.Subscribe();

            //Console.WriteLine("Obtained GUID {0}", myGuid.ToString());
            pipeProxy.SetSamplesPerSec(2000);
            pipeProxy.SaveSettings();

            Console.Out.WriteLine("Summary of the connected device:");
            Console.Out.WriteLine("Channels: {0}", pipeProxy.GetNChannels());
            Console.Out.WriteLine("Samples per second: {0}", pipeProxy.GetSamplesPerSec());
            Console.Out.WriteLine("Gain: {0}", pipeProxy.GetGain(1));

            //Let's try if we can get some data and become the event in the client.
            pipeProxy.ProcessData();
            
            Thread.Sleep(20);

            pipeProxy.ProcessData();

            pipeProxy.Unsubscribe();
            Console.WriteLine("Device test finished. Press <ENTER> to exit.");
            Console.ReadLine();
            Console.WriteLine("Starting simple pipeline test...");
            
            //TEST - EMG Server/
             
           

            /*
            //TEST - BlockingCollection

            BlockingCollection<UInt32> testCollection = new BlockingCollection<uint>(4);

            //When CompleteAdding is called on testCollection, if GetConsumingEnumerable finds an empty collection, it won't wait

            testCollection.CompleteAdding();

            foreach (UInt32 intItem in testCollection.GetConsumingEnumerable())
            {
                Console.Out.WriteLine("{0}", intItem);
            }

            //TEST - BlockingCollection/
            */
            
            /*
            //TEST - Pipeline
            
            Pipeline testPipeline = new Pipeline();

            testPipeline.AddStage(new SimpleInputStage());
            testPipeline.AddStage(new SimpleIntermediateStage());
            testPipeline.AddStage(new SimpleOutputStage());

            testPipeline.Init();
            testPipeline.Start();
            Thread.Sleep(10000);
            testPipeline.Stop();

            Console.Out.WriteLine("Items generated at input: {0}", ((SimpleInputStage)(testPipeline.Stages[0])).generatedItems);
            Console.Out.WriteLine("Items processed at output: {0}", ((SimpleOutputStage)(testPipeline.Stages[2])).processedItems);

            Console.WriteLine("Pipeline test finished. Press <ENTER> to exit.");
            Console.ReadLine();

            //TEST - Pipeline/
            */

            //TEST - DataProvider
            /*
            DataProviderTestContainer dpTestContainer = new DataProviderTestContainer();

            dpTestContainer.Run();
            */
            
            //TEST - Workflow
            /*
            WorkflowTestContainer wTestContainer = new WorkflowTestContainer();
            
            wTestContainer.Run();
            */
            /*
            //TEST - RecordingController
            
            RecordingControllerTestContainer recordingControllerTestContainer = new RecordingControllerTestContainer();
            recordingControllerTestContainer.Run();
            */

            //TEST - File access
            //myTestContainer = new FileAccessTestContainer();
            
            //TEST - LDAPatternRecognizer
            //myTestContainer = new LDAPatternRecognizerTestContainer();

            //Test - PatternRecognizerFactory

            //myTestContainer = new PatternRecognizerFactoryTestContainer();

            //Test - GenericFactory

            //myTestContainer = new GenericFactoryTestContainer();

            //Test - InstanceManager

            //myTestContainer = new InstanceManagerTestContainer();

            //Test - ObjectServer

            //myTestContainer = new ObjectServerTestContainer();

            //Test - MovListToCodeConverter

            //myTestContainer = new MovListToCodeConverterTestContainer();

            //Test - EncogTestContainer
            /*
            myTestContainer = new EncogTestContainer();

            myTestContainer.Run();

            Console.WriteLine("\n\n{0} test finished. Press <ENTER> to exit.",myTestContainer.ToString());

            Console.ReadLine();
            */



        }
    }
}
