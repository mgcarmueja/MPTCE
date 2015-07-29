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
using System.Linq;
using System.Text;
using ADS1298Intercom;
using System.ServiceModel;
using EMGFramework.ValueObjects;


namespace EMGFramework.DataProvider
{
    /// <summary>
    ///Implements the interface used by the ADS1298 data server to push packets to a listening client 
    /// </summary>
    [CallbackBehaviorAttribute(UseSynchronizationContext = false)]
    class ADS1298Callbacks : IEMGCallbacks
    {

        private double[,] _avgArray;
        private double[] _avgValues;
        private int avgPos = 0;

        public void NewPacket(XDataPacket packet)
        {

            Byte[] buffer;
            double[] sample = new double[packet.nChannels];
            Byte[] frame = new Byte[packet.frameSize];
            UInt32 countvalue;
            float inputDifferentialVoltage = ADS1298DataQueue.Instance.posVoltage - ADS1298DataQueue.Instance.negVoltage;
            UInt32 powerOfResBits = (UInt32)(1 << packet.resolutionBits);
            Frame currentFrame;
            BlockingCollection<Object> queue = ADS1298DataQueue.Instance.queue;
            
            //Length of the moving average buffer used when correcting signal offset
            UInt32 correctLength = packet.sampleFreq/8;

            bool correctOffset = ADS1298DataQueue.Instance.correctOffset;

            if (correctOffset)
            {
                //Using an average array with 0.125 seconds of samples (packet.sampleFreq/8)
                if (_avgArray == null) _avgArray = new double[correctLength, packet.nChannels];
                if (_avgValues == null) _avgValues = new double[packet.nChannels];
            }

            if (queue.IsAddingCompleted) return;
            if (ADS1298DataQueue.Instance.paused) return;

            //We transform back the frame data into a byte array
            buffer = System.Convert.FromBase64String(packet.payload);

            if (packet.lastPacket == true)
            {
                //We have received a dummy packet indicating that there will be no more frame packets coming.
                //The current capture session has finished, but we only mark the queue as complete for adding
                //if the data acquisition is not paused.

                if (!ADS1298DataQueue.Instance.paused) queue.CompleteAdding();
                
                return;
            }



            int temp;

            for (Int32 i = 0; i < packet.nFrames; i++)
            {
                Buffer.BlockCopy(buffer, (Int32) packet.frameSize * i, frame, 0, (Int32) packet.frameSize);
                
                countvalue = (UInt32)(frame[00]<<16|frame[01]<<8|frame[02]);
                int channel = 0;

                double min = 0;
                double max = 0;

                for (int j = 3; j <= frame.Length-3; j=j+3 )
                {
                    temp = (frame[j] << 24 | frame[j+1] << 16 | frame[j+2] << 8) / 256;

                    //Experimenting with a moving average to detect the channel amplitude offset
                    if (correctOffset)
                    {
                        /*
                        _avgArray[avgPos, channel] = (double)temp / (double)correctLength;
                        _avgValues[channel] = _avgValues[channel] + _avgArray[avgPos, channel] - _avgArray[(int)((avgPos + 1) % correctLength), channel];
                        */

                        _avgValues[channel] = _avgValues[channel] - _avgArray[avgPos, channel];
                        _avgArray[avgPos, channel] = (double)temp / (double)correctLength;
                        _avgValues[channel] += _avgArray[avgPos, channel];

                        temp -= (int)_avgValues[channel];
                    }
                    //End of the experiment

                    sample[channel] = ((double)temp / (double)powerOfResBits) * inputDifferentialVoltage;

                    if (channel == 0)
                    {
                        min = sample[channel];
                        max = sample[channel];
                    }
                    else
                    {
                        if (sample[channel] < min) min = sample[channel];
                        else if (sample[channel] > max) max = sample[channel];
                    }

                    channel++;
                }


                //This is only used when performing offset correction
                avgPos = (int)((avgPos + 1) % correctLength);

                currentFrame = new Frame(sample, ADS1298DataQueue.Instance.frameIdx, ADS1298DataQueue.Instance.timeIdx, min, max);

                ADS1298DataQueue.Instance.frameIdx++;
                ADS1298DataQueue.Instance.timeIdx = ADS1298DataQueue.Instance.frameIdx / (double)packet.sampleFreq; 

                queue.Add(currentFrame);
            }
        }
    }
}
