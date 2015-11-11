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
using System.Threading.Tasks;
using System.ComponentModel;

namespace MPTCE.RealtimeConsumers
{
    /// <summary>
    /// Base View model upon which every RealtimeConsumer is based
    /// </summary>
    public class BaseViewModel:INotifyPropertyChanged
    {


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
        /// True if the RealTimeConsumer is running, false otherwise
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


        private string _itemName="*Empty*";
        /// <summary>
        /// Item name used as on-screen title for the RealTimeConsumer
        /// </summary>
        public string itemName
        {
            get
            {
                return _itemName;
            }
            set
            {
                if (_itemName != value)
                {
                    _itemName = value;
                    this.NotifyPropertyChanged("itemName");

                }
            }
        }


        private RealtimeConsumer _realtimeConsumer;
        /// <summary>
        /// RealTimeConsumer object associated with this ViewModel
        /// </summary>
        public RealtimeConsumer realtimeConsumer
        {
            get
            {
                return _realtimeConsumer;
            }
            set 
            {
                if (_realtimeConsumer != value)
                {
                   if(_realtimeConsumer!=null)
                       _realtimeConsumer.PropertyChanged -= _realtimeConsumer_PropertyChanged;

                    _realtimeConsumer = value;
                    _realtimeConsumer.PropertyChanged += _realtimeConsumer_PropertyChanged;
                    this.NotifyPropertyChanged("realtimeConsumer");
                }
            }
        }


        public BaseViewModel()
        {
            this.PropertyChanged += BaseViewModel_PropertyChanged;
        }


        /// <summary>
        /// Starts the RealtimeConsumer if stopped, stops it if started, and does nothing if none was defined
        /// </summary>
        public void StartStop()
        {
            if(_realtimeConsumer!=null) _realtimeConsumer.StartStop();
        }

        void _realtimeConsumer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "running":
                    running = _realtimeConsumer.running;
                    break;
                default:
                    break;
            }
        }


        void BaseViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "realtimeConsumer":
                
                    if (realtimeConsumer != null)
                    {
                        itemName = (string) realtimeConsumer.GetType().GetProperty("displayName").GetValue(null,null);
                    }
                    else itemName = "*Empty*";

                    break;

                default:
                    break;
            }
        }


    }
}
