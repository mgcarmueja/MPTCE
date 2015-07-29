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
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMGFramework.ValueObjects;
using EMGFramework.DataProvider;
using EMGFramework.File;
using ADS1298Intercom;
using System.ServiceModel;
using System.ServiceModel.Channels;


namespace EMGFramework.DataProvider
{
    /// <summary>
    /// Implements a data provider that connects to the ADS1298 data server to obtain data from the DAS1298 EMG device. 
    /// </summary>
    public class ADS1298DataProvider : EMGDataProvider
    {
        
        public new static string ID
        {
            get
            {
                return "ADS1298";
            }
        }


        
        public override bool readsFromFile 
        {
            get 
            {
                return false;
            }
        }



        private IEMGDevice _pipeProxy;
        private DuplexChannelFactory<IEMGDevice> _pipeFactory;
        private bool _isRunning = false;



        public override bool isPaused
        {
            get
            {
                return ADS1298DataQueue.Instance.paused;
            }
        }



        /// <summary>
        /// True if signals should be corrected so that their average value is 0. False otherwise.
        /// This is intended to cancel the offset produced on some channels of the ADS1298 under certain circumstances.
        /// </summary>
        public bool correctOffset
        {
            get
            {
                return ADS1298DataQueue.Instance.correctOffset;
            }
            set
            {
                ADS1298DataQueue.Instance.correctOffset = value;
            }
        }



        public ADS1298DataProvider()
            :base()
        { 
            //We initialise a default recording configuration just in case we receive none from elsewhere.
            outputQueue = ADS1298DataQueue.Instance.queue;
            loop = false;
        }



        /// <summary>
        /// Attempts to connect to a running server through a named pipe.
        /// </summary>
        public override void Connect()
        {
            ADS1298DataQueue.Instance.Refresh();

            ADS1298Callbacks myCallbacks = new ADS1298Callbacks();

            _pipeFactory = new DuplexChannelFactory<IEMGDevice>(
                myCallbacks,
                new NetNamedPipeBinding(),
                new EndpointAddress("net.pipe://localhost/PipeEMGServer"));

            try
            {
                _pipeProxy = _pipeFactory.CreateChannel();
            }
            catch (Exception e)
            {
                throw (new DataProviderException("Conect: creating channel", e));
            }
            
            try
            {
                _pipeProxy.Subscribe();
            }
            catch (Exception e)
            {
                throw (new DataProviderException("Connect: subscribing",e));
            }

            status = DataProviderStatus.connected;
        }



        /// <summary>
        /// Sends the initial configuration parameters for the device on a connected server.  
        /// </summary>
        public override void Init()
        {
            try
            {
                _pipeProxy.SetSamplesPerSec((ushort)recordingConfig.sampleFreq);
                _pipeProxy.SetGlobalGain(recordingConfig.gain);

                //This is given in millivolts and defines the voltage amplitude 
                //the device can measure
                float negVoltage = _pipeProxy.GetVoltageNeg();
                float posVoltage = _pipeProxy.GetVoltagePos();

                //According to the ADS1298 datasheet, that is that it happens to the maximum and minimum voltages
                //when using a gain > 1
                ADS1298DataQueue.Instance.negVoltage = negVoltage / (float)recordingConfig.gain;
                ADS1298DataQueue.Instance.posVoltage = posVoltage / (float)recordingConfig.gain;

                //EXPERIMENTAL!! This should be defined in a configuration process and not here!!
                correctOffset = true;

                _pipeProxy.SaveSettings();
            }
            catch (Exception e)
            {
                throw new DataProviderException("Init: configuring device settings",e);
            }

            outputQueue = ADS1298DataQueue.Instance.queue;

            status = DataProviderStatus.initialized;
        }



        /// <summary>
        /// Disconnects the DataProvider from the server.
        /// </summary>
        public override void Disconnect()
        {
            if (_isRunning) Stop();

            try
            {
                _pipeProxy.Unsubscribe();
                _pipeFactory.Close();
            }
            catch (Exception e)
            {
                throw new DataProviderException("Disconnect: unsubscribing and closing connection", e);
            }
            finally
            {
                status = DataProviderStatus.disconnected;
            }    
        }



        /// <summary>
        /// Begins the data capture.
        /// </summary>
        /// <returns>A BlockingCollection containing Frame objects obtained form the server</returns>
        public override BlockingCollection<Object> Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                
                if (ADS1298DataQueue.Instance.paused)
                    ADS1298DataQueue.Instance.paused = false;

                else //Starting a new recording session 
                {
                    ADS1298DataQueue.Instance.frameIdx = 0;
                    ADS1298DataQueue.Instance.timeIdx = 0.0;
                    

                    try
                    {
                        _pipeProxy.ProcessData();
                    }
                    catch (Exception e)
                    {
                        throw new DataProviderException("Start: Enabling data acquisition", e);
                    }
                }


                status = DataProviderStatus.started;

            }
            return outputQueue;

        }



        /// <summary>
        /// Tells the server to stop sending data, waits for the reader to consume the enqueued items and marks 
        /// the queue as complete for adding.
        /// </summary>
        public override void Stop()
        {
            if (_isRunning)
            {
                try
                {
                    _pipeProxy.ProcessData();
                }
                catch (Exception e)
                {
                    throw new DataProviderException("While stopping ADS1298DataProvider",e);
                }
                finally
                {

                    //while (outputQueue.Count > 0);

                    outputQueue.CompleteAdding();
                    _isRunning = false;
                    status = DataProviderStatus.stopped;
                }
            }
        }



        /// <summary>
        /// Tells the server to stop sending data, but doesn't mark de output queue as complete for adding. 
        /// A process reading from this queue will thus be effectively paused until Start() is called again.
        /// </summary>
        public override void Pause()
        {
            if (_isRunning)
            {
                //_pipeProxy.ProcessData();

                ADS1298DataQueue.Instance.paused = true;
                while (outputQueue.Count > 0);
                _isRunning = false;
                
                status = DataProviderStatus.paused;
            }
        }

    }
}
