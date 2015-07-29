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
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using EMGFramework.Utility;
using EMGFramework.ValueObjects;



namespace MPTCE.RealtimeConsumers
{
    /// <summary>
    /// Abstract parent class defining a generic realtime consumer object. This family of classes define the code
    /// behind the various items in the realtime pane.
    /// </summary>
    public abstract class RealtimeConsumer : INotifyPropertyChanged
    {
        /// <summary>
        /// ID used by a GenericFactory to register this classe. This should never be attempted
        /// in the case of an abstract class like this one. It is left uncommented to remind that
        /// all descendant classes needing to be instantiable through a GenericFactory 
        /// should implement this.
        /// </summary>
        public static string ID
        {
            get { return "Undefined"; }
        }

        /// <summary>
        /// User-friendly name to be shown by a GUI element acting as interface for this class
        /// </summary>
        public static string displayName
        {
            get { return "Base class"; }
        }



        /// <summary>
        /// Event handler that fires whenever a property has been changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }



        private bool _running;
        /// <summary>
        /// True if the RealtimeConsumer is running, false otherwise.
        /// </summary>
        public bool running
        {
            get
            {
                return _running;
            }

            set
            {
                if (_running != value)
                {
                    _running = value;
                    this.NotifyPropertyChanged("running");
                }
            }

        }



        private ObjectServer<Movement> _objectServer;
        /// <summary>
        /// Object server used to obtain the stream of movement codes
        /// </summary>
        public ObjectServer<Movement> objectServer
        {
            get
            {
                return _objectServer;
            }

            set
            {
                if (_objectServer != value && !_running)
                {
                    if (_objectServer != null) _objectServer.UnregisterConsumer(_guid);
                    _objectServer = value;
                    this.NotifyPropertyChanged("objectServer");
                }
            }
        }


        private UserControl _consumerControl;
        /// <summary>
        /// Control used to display and/or set the configuration of a RealtimeConsumer
        /// </summary>
        public UserControl consumerControl
        {
            get
            {
                return _consumerControl;
            }

            set
            {
                if (_consumerControl != value)
                {
                    _consumerControl = value;
                    this.NotifyPropertyChanged("consumerControl");
                }
            }
        }



        /// <summary>
        /// Guid returned by the ObjectServer in use after subscribing
        /// </summary>
        private Guid _guid;

        /// <summary>
        /// Queue containing the movement objects received from the ObjectServer
        /// </summary>
        protected BlockingCollection<Movement> movementQueue;


        private ConsumerConfig _consumerConfig;
        /// <summary>
        /// Configuration object provided by the realtime acquisition stage. Concrete RealtimeConsumer
        /// implementations can subscribe for receiving the PropertyChanged events produced by this object.
        /// </summary>
        public ConsumerConfig consumerConfig
        {
            get
            {
                return _consumerConfig;
            }

            set
            {
                if (_consumerConfig != value)
                {
                    if (_consumerConfig != null)
                    {
                        _consumerConfig.PropertyChanged -= consumerConfig_PropertyChanged;
                        ConsumerConfigUnhook(_consumerConfig);
                    }
                    if (value != null)
                    {
                        value.PropertyChanged += consumerConfig_PropertyChanged;
                        ConsumerConfigHook(value);
                    }
                    _consumerConfig = value;
                }
            }
        }


        /// <summary>
        /// Used when the ConsumerConfig instance changes value. When overriden, this method allows descendant classes
        /// to unregister event handlers that may be attached to attributes od the ConsumerConfig instance
        /// (for instance, an ObservableCollection).
        /// </summary>
        /// <param name="consumerConfig"></param>
        protected virtual void ConsumerConfigUnhook(ConsumerConfig consumerConfig)
        { 
        }

        /// <summary>
        /// Used when the COnsumerConfig instance changes value. When overriden, this method allows descendant clases
        /// to register event handlers attached to attributes of the ConsumerConfig instance
        /// (for instance, an ObservableCollection).
        /// </summary>
        /// <param name="consumerConfig"></param>
        protected virtual void ConsumerConfigHook(ConsumerConfig consumerConfig)
        { 
        }



        /// <summary>
        /// This method should be overrided by the descendant classes where reacting to changes
        /// in the ConsumerConfig object is required.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void consumerConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Nothig to do. This is just a placeholder method.
        }


        public RealtimeConsumer()
        {
            _running = false;
            this.PropertyChanged += RealtimeConsumer_PropertyChanged;
            //consumerConfig = providedConsumerConfig;
        }

        void RealtimeConsumer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "running":

                    if (running)
                    {
                        if (_objectServer != null)
                        {
                            _guid = _objectServer.RegisterConsumer();
                            movementQueue = _objectServer.GetConsumerQueue(_guid);

                        }
                        Start();

                    }
                    else
                    {

                        if (_objectServer != null)
                        {
                            _objectServer.UnregisterConsumer(_guid);
                            _guid = Guid.Empty;
                        }

                        Stop();
                    }

                    break;

                default:
                    break;
            }
        }





        /// <summary>
        /// This method is intended to be called by a client to start/stop the RealtimeConsumer.
        /// </summary>
        public void StartStop()
        {
            running = !running;
        }



        public abstract void Start();

        public abstract void Stop();



    }
}
