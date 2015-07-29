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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPdevice;
using ADS1298Intercom;
using System.ServiceModel;
using System.Diagnostics;


namespace ADS1298Server
{
    /// <summary>
    /// Server side of the WCF interface for accessing the EMG device 
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class EMGDevice : IEMGDevice
    {

        private GENdevice senderInstance;
        Int32 deviceNum = 0; //This implementations only listens to the first device found
        Int32 dequeuedPackets = 0;

        IEMGCallbacks serviceCallback = null;

        /// <summary>
        /// True if incoming packets should be ingored and not forwarded to the client,
        /// false otherwise. It is used to avoid processing any more packets once an exception
        /// by sending packets to a client is produced.
        /// </summary>
        private bool _ignorePackets;


        public EMGDevice()
        {
            _ignorePackets = true; //Because acquisition is not running or should not run
        }

        /// <summary>
        /// Adds a new listening client to our list of subscribers.
        /// </summary>
        public void Subscribe()
        {
            //We must check that there are devices ready to be used!

            if (EMGSingleton.Instance.EPdevices.Count > 0)
            {
                senderInstance = EMGSingleton.Instance.EPdevices.ElementAt(deviceNum);
                senderInstance.NewPacket += new NewPacketEventHandler(NewPacketHandler);
                serviceCallback = OperationContext.Current.GetCallbackChannel<IEMGCallbacks>();
            }
        }

        /// <summary>
        /// Removes a listening client from our list of subscribers.
        /// </summary>
        public void Unsubscribe()
        {
            senderInstance.NewPacket -= new NewPacketEventHandler(NewPacketHandler);
            senderInstance = null;
        }


        /// <summary>
        /// Handler for the NewPacket event. The packet is sent to the client by means of a callback function.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        
        public void NewPacketHandler(object sender, EventArgs e)
        {
            ADS1298device targetDevice;
            ADS1298deviceVM targetDeviceVM;
            RawDataPacket packet;
            XDataPacket xPacket;

            if (!_ignorePackets)
            {
                //We retrieve a raw packet, convert it to a serializable iDataPacket 
                //and send it to the client through a callback function.
                try
                {
                    targetDevice = (ADS1298device)(EMGSingleton.Instance.EPdevices.ElementAt(deviceNum));
                    targetDeviceVM = (ADS1298deviceVM)(EMGSingleton.Instance.EPdevicesViewModel.ElementAt(deviceNum));

                    while (!targetDevice.packetQueue.TryDequeue(out packet)) ;

                    xPacket = new XDataPacket();
                    xPacket.payloadSize = packet.payloadSize;
                    xPacket.sampleFreq = packet.sampleFreq;
                    //For our purpose, we assume that the gain has the same value for all channels.
                    xPacket.gain = targetDeviceVM.GAIN_LIST[0];
                    xPacket.inputDifferentialVoltagePositive = targetDeviceVM.InputDifferantialVoltagePositive;
                    xPacket.inputDifferentialVoltageNegative = targetDeviceVM.InputDifferantialVoltageNegative;
                    xPacket.payload = System.Convert.ToBase64String(packet.payload);
                    xPacket.resolutionBits = (Byte)targetDeviceVM.ResolutionBits;
                    xPacket.frameSize = packet.frameSize;
                    xPacket.nChannels = packet.nChannels;
                    xPacket.nFrames = packet.nFrames;
                    xPacket.lastPacket = packet.lastPacket;
                    xPacket.timeStamp = packet.timeStamp;
                    xPacket.timeDelta = packet.timeDelta;

                    //DEBUG

                    //Console.Out.WriteLine(xPacket);

                    //DEBUG/

                    //And then we should use a callback that allows a IDataPacket as parameter 
                    serviceCallback.NewPacket(xPacket);

                    //DEBUG
                    dequeuedPackets++;

                    //DEBUG/
                    //Console.Out.Write("> ");
                }
                catch (Exception ex)
                {
                    //Something happened that prevented us from sending the packet!
                    //In this case, we stop the acquisition process.
                    Debug.WriteLine("An exception was caught while running the packet handler!");
                    Debug.WriteLine(ex.ToString());
                    Debug.WriteLine("Data acquisition will be stopped.");

                    ProcessData();
                }
            }
        }


        public ushort GetNChannels()
        {
            ADS1298deviceVM targetDevice;
            
            try
            {
                targetDevice = (ADS1298deviceVM)(EMGSingleton.Instance.EPdevicesViewModel.ElementAt(deviceNum));
                return (targetDevice.NumberOfChannels);
            }
            catch (ArgumentOutOfRangeException)
            {
                return (0);
            }

        }


        public UInt32 GetSamplesPerSec()
        {
            ADS1298deviceVM targetDevice;

            try
            {
                targetDevice = (ADS1298deviceVM)(EMGSingleton.Instance.EPdevicesViewModel.ElementAt(deviceNum));
                return (targetDevice.DR_LIST[targetDevice.DR]);
            }
            catch (ArgumentOutOfRangeException)
            {
                return 0;
            }
        }


        /// <summary>
        /// It sets the device sampling frequency to one of the caonfigurable values
        /// by selecting the one that is closer to the desired sampling frequency.
        /// </summary>
        /// <param name="samplesPerSecond">The desired sampling frequency</param>
        public void SetSamplesPerSec(ushort samplesPerSecond)
        {
            ADS1298deviceVM targetDevice;

            try
            {
                targetDevice = (ADS1298deviceVM)(EMGSingleton.Instance.EPdevicesViewModel.ElementAt(deviceNum));
                //targetDevice.SamplesPerSecond = samplesPerSecond;
                
                int minDistance = (int)targetDevice.DR_LIST[0] - (int)samplesPerSecond;
                if(minDistance<0) minDistance = -1 * minDistance;
                byte position = 0;

                for(byte i=0; i<targetDevice.DR_LIST.Length;i++)
                {
                    int currentDistance = (int)targetDevice.DR_LIST[i] - (int)samplesPerSecond;
                    if (currentDistance < 0) currentDistance = -1 * currentDistance;

                    if (currentDistance < minDistance)
                    {
                        minDistance = currentDistance;
                        position = i;
                    }
                }

                targetDevice.DR = position;

            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }




        public UInt32 GetGain(UInt32 nChannel)
        {
            ADS1298deviceVM targetDevice;
            try
            {
                targetDevice = (ADS1298deviceVM)(EMGSingleton.Instance.EPdevicesViewModel.ElementAt(deviceNum));
                return (targetDevice.GAIN_LIST[nChannel]);
            }
            catch (ArgumentOutOfRangeException)
            {
                return 0;
            }
        }


        public void SetGlobalGain(UInt32 value)
        { 
            ADS1298deviceVM targetDevice;
            try
            {
                targetDevice = (ADS1298deviceVM)(EMGSingleton.Instance.EPdevicesViewModel.ElementAt(deviceNum));
                targetDevice.GAIN_VALUE = value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception at SetGlobalGain: {0}", ex.Message);
            }
        }



        public float GetVoltagePos()
        {
            float value=0;

            ADS1298deviceVM targetDevice;
            try
            {
                targetDevice = (ADS1298deviceVM)(EMGSingleton.Instance.EPdevicesViewModel.ElementAt(deviceNum));
                value = targetDevice.InputDifferantialVoltagePositive;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception at GetVoltagePos: {0}", ex.Message);
            }

            return value;
        }


        public float GetVoltageNeg()
        {
            float value = 0;

            ADS1298deviceVM targetDevice;
            try
            {
                targetDevice = (ADS1298deviceVM)(EMGSingleton.Instance.EPdevicesViewModel.ElementAt(deviceNum));
                value = targetDevice.InputDifferantialVoltageNegative;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception at GetVoltageNeg: {0}", ex.Message);
            }

            return value;
        }



        public void SetChannelActivation(UInt32 channel, bool status)
        {

            ADS1298deviceVM targetDevice;
            try
            {
                targetDevice = (ADS1298deviceVM)(EMGSingleton.Instance.EPdevicesViewModel.ElementAt(deviceNum));
                
                switch(channel)
                {
                    case 0:
                        targetDevice.CH1_PD=status;
                        break;

                    case 1:
                        targetDevice.CH2_PD = status;
                        break;

                    case 2:
                        targetDevice.CH3_PD = status;
                        break;

                    case 3:
                        targetDevice.CH4_PD = status;
                        break;

                    case 4:
                        targetDevice.CH5_PD = status;
                        break;

                    case 5:
                        targetDevice.CH6_PD = status;
                        break;

                    case 6:
                        targetDevice.CH7_PD = status;
                        break;

                    case 7:
                        targetDevice.CH8_PD = status;
                        break;

                    
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception at SetChannelActivation: {0}", ex.Message);
            }
 
        }


        /// <summary>
        /// It calls the ProcessData method at the DeviceManager singleton. Calling this 
        /// method starts or stops the capture of data frames from one EMG device connected to the computer.
        /// </summary>
        public void ProcessData()
        {
            ADS1298deviceVM targetDeviceVM;

            try
            {
                targetDeviceVM = (ADS1298deviceVM)(EMGSingleton.Instance.EPdevicesViewModel.ElementAt(deviceNum));
                targetDeviceVM.LiveData = true; //Because we are only interested in live data.

                //This should start/stop the data flow form the device.
                EMGSingleton.Instance.ProcessData();
                _ignorePackets = !_ignorePackets;

            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine("ProcessData: Exception -> {0}", ex.Message);
            }
        }

        public void SaveSettings()
        {
            ADS1298deviceVM targetDeviceVM;

            try
            {
                targetDeviceVM = (ADS1298deviceVM)(EMGSingleton.Instance.EPdevicesViewModel.ElementAt(deviceNum));
                targetDeviceVM.SaveSettingsCommand.Execute(null);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine("SaveSettings: Exception -> {0}", ex.Message);
            }
        }
    }
}
