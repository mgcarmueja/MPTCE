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
using System.Threading.Tasks;
using EMGFramework.ValueObjects;



namespace EMGFramework.DataProvider
{

    /// <summary>
    /// Singleton implementing the common data queue that is to be shared between 
    /// an ADS1298Callbacks object adding elements to the queue and a client removing them
    /// </summary>
    internal sealed class ADS1298DataQueue
    {
        // This implementation follow the recommendations for the implementation of the Singleton pattern 
        // for multithreading environments at http://csharpindepth.com/articles/general/singleton.aspx

        private static volatile ADS1298DataQueue _instance = new ADS1298DataQueue();
        private static object syncRoot = new Object();


        private BlockingCollection<Object> _queue;
        /// <summary>
        /// Queue where the incoming frames from the server are stored.
        /// </summary>
        public BlockingCollection<Object> queue
        {
            get { return _queue; }
        }


        private bool _paused;
        /// <summary>
        /// True if the recording has been paused, false otherwise. While paused, incoming frames will
        /// simply be discarded, though the server is not stopped.
        /// </summary>
        public bool paused
        {
            get { return _paused; }
            set { _paused = value; }

        }


        private double _timeIdx;
        /// <summary>
        /// Temporal index for the next frame to be created
        /// </summary>
        public double timeIdx
        {
            get { return _timeIdx; }
            set { _timeIdx = value; }
        }


        private uint _frameIdx;
        /// <summary>
        /// Frame index in relation to the start of the recording
        /// </summary>
        public uint frameIdx
        {
            get { return _frameIdx; }
            set { _frameIdx = value; }
        }


        private float _negVoltage;
        /// <summary>
        /// Minimum negative voltage in millivolts over the reference voltage that the device is able to detect.
        /// The effects prduced by the configured gain are already taken into account.
        /// </summary>
        public float negVoltage
        {
            get { return _negVoltage; }
            set { _negVoltage = value; }
        }

        private float _posVoltage;
        /// <summary>
        /// Maximum negative voltage in millivolts over the reference voltage that the device is able to detect.
        /// The effects prduced by the configured gain are already taken into account.
        /// </summary>
        public float posVoltage
        {
            get { return _posVoltage; }
            set { _posVoltage = value; }
        }

        private bool _correctOffset;
        /// <summary>
        /// True if offset should be corrected, so that the signals in all channels are centered at 0. False otherwise
        /// </summary>
        public bool correctOffset
        {
            get { return _correctOffset; }
            set { _correctOffset = value; }
        }


        private ADS1298DataQueue()
        {
            //No limit for this BlockingCollection for now.
            _queue = new BlockingCollection<Object>();
            _paused = false;
        }

        /// <summary>
        /// Recreates the BlockingCollection if it is marked as adding completed. 
        /// </summary>
        public void Refresh()
        {
            lock (syncRoot)
            {
                if (_queue.IsAddingCompleted)
                {
                    _queue.Dispose();
                    _queue = new BlockingCollection<Object>();
                }
            }
        }

        /// <summary>
        /// Returns the singleton instance
        /// </summary>
        public static ADS1298DataQueue Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new ADS1298DataQueue();
                    }
                }
                return _instance;
            }
        }
    }
}
