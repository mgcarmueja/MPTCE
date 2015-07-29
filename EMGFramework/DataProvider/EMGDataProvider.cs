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
using EMGFramework.File;

namespace EMGFramework.DataProvider
{

    public enum DataProviderStatus : int { unknown, connected, disconnected, initialized, started, stopped, paused };

    /// <summary>
    /// Event argument class for passing the current status of the EMGDataProvider to the listener.
    /// </summary>
    public class EMGDataProviderStatusEventArgs : EventArgs
    {
        public DataProviderStatus status { get; private set; }

        public EMGDataProviderStatusEventArgs(DataProviderStatus statusValue)
            : base()
        {
            status = statusValue;
        }

    }



    public abstract class EMGDataProvider
    {

        public static string ID
        {
            get
            {
                return "undefined";
            }
        }


        /// <summary>
        /// An event that clients can use to be notified whenever the status of the EMGDataProvider changes
        /// due to transitions between schedule items. Implemented following the .NET Framework guidelines.
        /// </summary>
        public event EventHandler StatusChanged;


        /// <summary>
        /// Invoke the StatusChanged event; called whenever the status of the PlaybackDataProvider changes
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnStatusChanged(EventArgs e)
        {
            if (StatusChanged != null)
                StatusChanged(this, e);
        }



        /// <summary>
        /// True if the EMGDataProvider reads data from file, false otherwise
        /// </summary>
        public virtual bool readsFromFile
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// True if the chosen movements should be played in a loop, false otherwise. It makes sense only
        /// for playback data providers.
        /// </summary>
        private bool _loop;
        /// <summary>
        /// True if the chosen movements should be played in a loop, false otherwise
        /// </summary>
        public bool loop
        {
            get
            {
                return _loop;
            }

            set
            {
                if (_loop != value)
                {
                    _loop = value;
                }
            }
        }


        public StringCollection knownMovements { get; set; }

        public StringCollection allowedComplexMovements { get; set; }

        public MovementSelector movementSelector { get; set; }


        private DataProviderStatus _status;
        public DataProviderStatus status
        {
            get
            {
                return _status;
            }
            protected set
            {
                if (_status != value)
                {
                    _status = value;
                    OnStatusChanged(new EMGDataProviderStatusEventArgs(_status));
                }

            }
        }


        private BlockingCollection<Object> _outputQueue;
        /// <summary>
        /// This is the queue from which clients will read the data returned by an EMGDataProvider
        /// </summary>
        public BlockingCollection<Object> outputQueue
        {
            get
            {
                return _outputQueue;
            }
            protected set
            {
                if(_outputQueue!=value)
                _outputQueue = value;
            }
        }


        private RecordingConfig _recordingConfig;
        /// <summary>
        /// Contains configuration parameters for the EMGDataProvider. These are typically used during initialisation
        /// </summary>
        public RecordingConfig recordingConfig
        {
            get
            {
                return _recordingConfig;
            }

            set
            {
                _recordingConfig = value;
            }
        }


        private bool _isPaused;
        /// <summary>
        /// True if the EMGDataProvider is currently in pause. False otherwise
        /// </summary>
        public virtual bool isPaused
        {
            get
            {
                return _isPaused;
            }
            protected set
            {
                if (_isPaused != value)
                    _isPaused = value;
            }
        }


        public EMGDataProvider()
        {
            _isPaused = false;
            _loop = false;
            _recordingConfig = new RecordingConfig(2000, 8, 6, -1500, 1500);
        }



        /// <summary>
        /// If the EMGDataProvider needs to connect to some sort of data source (e.g. a local or remote
        /// data server), it should be done whithin this method.
        /// </summary>
        public abstract void Connect();

        /// <summary>
        /// If the EMGDataProvider needs to disconnect from some sort of data source (e.g. a local or remote
        /// data server), it should happen whithin this method.
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Things such as configuring a data acquisition device after having access to it
        /// should be done here.
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Data acquisition starts. The output queue is filled with data objects.
        /// </summary>
        /// <returns></returns>
        public abstract BlockingCollection<Object> Start();

        /// <summary>
        /// Data acquisition stops definitely. The output data queue will not be filled again with data 
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Data acquisition is paused. The output data queue is not invalidated for new additions.
        /// </summary>
        public abstract void Pause();

    }
}
