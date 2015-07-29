using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPdevice
{
    public class RawDataPacket
    {
        private Byte[] _payload;
        private Int32 _payloadSize;
        private UInt32 _sampleFreq;
        private UInt32 _nChannels;
        private UInt32 _frameSize;
        private UInt32 _nFrames;
        private bool _lastPacket = false;
        private double _timeStamp = 0;
        private double _timeDelta = 0;

        /// <summary>
        /// The actual data read form the device
        /// </summary>
        public Byte[] payload
        {
            get { return _payload; }
        }

        /// <summary>
        /// Size of the payload in bytes
        /// </summary>
        public Int32 payloadSize
        {
            get { return _payloadSize; }
        }

        /// <summary>
        /// Sampling frequency in Hz
        /// </summary>
        public UInt32 sampleFreq
        {
            get { return _sampleFreq; }
        }

        public UInt32 nChannels
        {
            get{return _nChannels;}
        }

        public UInt32 frameSize
        {
            get{return _frameSize;}
        }

        public UInt32 nFrames
        {
            get{return _nFrames;}
        }

        public bool lastPacket
        {
            get { return _lastPacket; }
            set { _lastPacket = value; }
        }

        public double timeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        public double timeDelta
        {
            get { return _timeDelta; }
            set { _timeDelta = value; }
        }


        public RawDataPacket(Byte[] payload, Int32 payloadSize, UInt32 sampleFreq, UInt32 nChannels, UInt32 frameSize, UInt32 nFrames, double timeStamp, double timeDelta)
        {
            _payloadSize = Buffer.ByteLength(payload);
            _payload = new Byte[_payloadSize];
            Buffer.BlockCopy(payload,0,_payload,0,payloadSize);
            _sampleFreq = sampleFreq;
            _nChannels = nChannels;
            _frameSize = frameSize;
            _nFrames = nFrames;
            _timeStamp = timeStamp;
            _timeDelta = timeDelta;
        }
    }
}
