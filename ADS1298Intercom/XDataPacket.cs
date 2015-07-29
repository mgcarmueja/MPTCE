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
using System.Runtime.Serialization;

namespace ADS1298Intercom
{
    /// <summary>
    /// Serialization-friendly class for transmitting data packets (eXchange Data Packets) containing one or multiple frames from server to client.
    /// </summary>
    [DataContract]
    public class XDataPacket
    {
        private String _payload;
        private Int32 _payloadSize;
        private UInt32 _sampleFreq, _gain;
        private float _inputDifferentialVoltagePositive, _inputDifferentialVoltageNegative;
        private Byte _resolutionBits;
        private UInt32 _frameSize;
        private UInt32 _nChannels;
        private UInt32 _nFrames;
        private bool _lastPacket = false;
        private double _timeStamp = 0;
        private double _timeDelta = 0;

        public XDataPacket()
        {
        }

        /// <summary>
        /// Set of data frames encoded in Base64
        /// </summary>
        [DataMember]
        public String payload
        {
            get 
            {
                return _payload;
            }
            set
            {
                _payload = value;
            }
        }

        /// <summary>
        /// Number of data frames encoded in the payload
        /// </summary>
        [DataMember]
        public Int32 payloadSize
        {
            get 
            {
                return _payloadSize;
            }

            set
            {
                _payloadSize = value;
            }
        }

        /// <summary>
        /// Frequency at which the payload frames were sampled
        /// </summary>
        [DataMember]
        public UInt32 sampleFreq
        {
            get
            {
                return _sampleFreq;
            }

            set
            {
                _sampleFreq = value;
            }
        }


        /// <summary>
        /// Common gain for all channels in the ADS1298.
        /// </summary>
        [DataMember]
        public UInt32 gain
        {
            get
            {
                return _gain;
            }

            set
            {
                _gain = value;
            }
        }


        ///  <summary>
        ///  Get/set method for device positive input voltage.
        ///  </summary>
        [DataMember]
        public float inputDifferentialVoltagePositive
        {
            get { return _inputDifferentialVoltagePositive; }
            set { _inputDifferentialVoltagePositive = value; }
        }

        ///  <summary>
        ///  Get/set method for device negative input voltage.
        ///  </summary>
        [DataMember]
        public float inputDifferentialVoltageNegative
        {
            get { return _inputDifferentialVoltageNegative; }
            set { _inputDifferentialVoltageNegative = value; }
        }

        /// <summary>
        /// Number of bits per sample. The ADS1298 supports either 16 or 24 bits per sample.
        /// </summary>
        [DataMember]
        public Byte resolutionBits
        {
            get { return _resolutionBits; }
            set { _resolutionBits = value; }
        }

        /// <summary>
        /// Frame size in bytes.
        /// </summary>
        [DataMember]
        public UInt32 frameSize
        {
            get { return _frameSize; }
            set { _frameSize = value; }
        }

        /// <summary>
        /// Number of channels per sample
        /// </summary>
        [DataMember]
        public UInt32 nChannels
        {
            get { return _nChannels; }
            set { _nChannels = value; }
        }

        /// <summary>
        /// Number of frames contained in the packet
        /// </summary>
        [DataMember]
        public UInt32 nFrames
        {
            get { return _nFrames; }
            set { _nFrames = value; }
        }

        /// <summary>
        /// True if this is a dummy packet indicating the end of the data stream
        /// </summary>
        [DataMember]
        public bool lastPacket
        {
            get { return _lastPacket; }
            set { _lastPacket = value; }
        }

        /// <summary>
        /// Packet timestamp in fractions of a second. Starts counting at the beginning of a live reading session
        /// </summary>
        [DataMember]
        public double timeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        /// <summary>
        /// Packet time delta  in fractions of a second. 
        /// It measures the difference between the begin and the end of the USB block read operation.
        /// </summary>
        [DataMember]
        public double timeDelta
        {
            get { return _timeDelta; }
            set { _timeDelta = value; }
        }

        public override string ToString()
        {

            string outputString = "<s:" + _payloadSize + "; f:" + _sampleFreq + "; g:" + _gain + "; v:" + _inputDifferentialVoltagePositive + ", v-:" + _inputDifferentialVoltageNegative + "; r:" + _resolutionBits + "; fs:" + _frameSize + "; #c:" + _nChannels + "; #f:" + _nFrames + "; lp:" + _lastPacket + ">";
            return outputString;
        }


    }
}
