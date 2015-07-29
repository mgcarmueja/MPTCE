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
using System.Linq;
using System.Text;
using ADS1298Intercom;
using System.ServiceModel;
using EMGFramework.ValueObjects;


namespace Tester.Testing
{
    [CallbackBehaviorAttribute(UseSynchronizationContext = false)]
    class EMGCallbacks : IEMGCallbacks
    {
        public void NewPacket(XDataPacket packet)
        {
            //Console.Out.Write("* ");
            Byte[] buffer;
            Int32 nFrames;
            double[] sample = new double[8];
            Byte[] frame = new Byte[27];
            UInt32 countvalue;
            Byte DATALENGTH = 27;
            float inputDifferentialVoltage = packet.inputDifferentialVoltagePositive - packet.inputDifferentialVoltageNegative;
            UInt32 powerOfResBits = (UInt32)(1 << packet.resolutionBits);

            Frame currentFrame;


            Console.Out.WriteLine("sampleFreq: {0}",packet.sampleFreq);
            Console.Out.WriteLine("payloadSize: {0}", packet.payloadSize);

            //We transform back the frame data into a byte array
            buffer = System.Convert.FromBase64String(packet.payload);

            nFrames = packet.payloadSize/DATALENGTH; //We should get DATALENGTH from the device model!

            for (Int32 i = 0; i < nFrames; i++)
            {
                Buffer.BlockCopy(buffer, DATALENGTH * i, frame, 0, DATALENGTH);
                
                //An unrolled loop to obtain the counter and float values for each frame

                countvalue = (UInt32)(frame[00]<<16|frame[01]<<8|frame[02]);
                sample[0]=frame[03]<<24|frame[04]<<16|frame[05]<<8; sample[0]=(sample[0]/256)*inputDifferentialVoltage/(double)(powerOfResBits*packet.gain);
                sample[1]=frame[06]<<24|frame[07]<<16|frame[08]<<8; sample[1]=(sample[1]/256)*inputDifferentialVoltage/(double)(powerOfResBits*packet.gain);
                sample[2]=frame[09]<<24|frame[10]<<16|frame[11]<<8; sample[2]=(sample[2]/256)*inputDifferentialVoltage/(double)(powerOfResBits*packet.gain);
                sample[3]=frame[12]<<24|frame[13]<<16|frame[14]<<8; sample[3]=(sample[3]/256)*inputDifferentialVoltage/(double)(powerOfResBits*packet.gain);
                sample[4]=frame[15]<<24|frame[16]<<16|frame[17]<<8; sample[4]=(sample[4]/256)*inputDifferentialVoltage/(double)(powerOfResBits*packet.gain);
                sample[5]=frame[18]<<24|frame[19]<<16|frame[20]<<8; sample[5]=(sample[5]/256)*inputDifferentialVoltage/(double)(powerOfResBits*packet.gain);
                sample[6]=frame[21]<<24|frame[22]<<16|frame[23]<<8; sample[6]=(sample[6]/256)*inputDifferentialVoltage/(double)(powerOfResBits*packet.gain);
                sample[7]=frame[24]<<24|frame[25]<<16|frame[26]<<8; sample[7]=(sample[7]/256)*inputDifferentialVoltage/(double)(powerOfResBits*packet.gain);

                currentFrame = new Frame(sample, countvalue, countvalue / (double) packet.sampleFreq,packet.inputDifferentialVoltageNegative,packet.inputDifferentialVoltagePositive);

                //DEBUG
                /*
                Console.Out.Write("{0} ", countvalue);
                Console.Out.Write("{0} ", sample[0]);
                Console.Out.Write("{0} ", sample[1]);
                Console.Out.Write("{0} ", sample[2]);
                Console.Out.Write("{0} ", sample[3]);
                Console.Out.Write("{0} ", sample[4]);
                Console.Out.Write("{0} ", sample[5]);
                Console.Out.Write("{0} ", sample[6]);
                Console.Out.Write("{0} ", sample[7]);
                */
                //DEBUG/

            }

            //DEBUG
            //And the, print out what we got formated by samples and fields
            /*
            Console.Out.WriteLine("Length of the recovered data: {0}", Buffer.ByteLength(buffer));
            
            for (Int32 i = 0; i < frame.payloadSize; i++)
            {
                Console.Out.Write("{0} ", buffer[i]);
            }
            */
            //DEBUG/
        }
    }
}
