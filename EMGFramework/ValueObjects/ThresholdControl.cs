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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMGFramework.ValueObjects
{
    public class ThresholdControl:INotifyPropertyChanged
    {

        /// <summary>
        /// Event delegate that is called whenever an observed property changes. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }




        private uint _channel;
        /// <summary>
        /// Number of the channel whose threshold control parameters are encoded.
        /// </summary>
        public uint channel
        {
            get
            {
                return _channel;
            }

            set 
            {
                if (_channel != value)
                {
                    _channel = value;
                    this.NotifyPropertyChanged("channel");
                }
            }
        }


        private int _selectedMovement;
        /// <summary>
        /// Current selected movement from the list of selectable movements
        /// </summary>
        public int selectedMovement
        {
            get
            {
                return _selectedMovement;
            }

            set 
            {
                if (_selectedMovement != value)
                {
                    _selectedMovement = value;
                    this.NotifyPropertyChanged("selectedMovement");
                }
            }
        }


        private double _threshold;
        /// <summary>
        /// Selected threshold for the channel
        /// </summary>
        public double threshold
        {
            get
            {
                return _threshold;
            }

            set 
            {
                if (_threshold != value)
                {
                    _threshold = value;
                    this.NotifyPropertyChanged("threshold");
                }
            }
        }

        
        private double _thresholdMin;
        /// <summary>
        /// Minimum possible threshold value for this channel
        /// </summary>
        public double thresholdMin
        {
            get 
            {
                return _thresholdMin;
            }

            set
            {
                if (_thresholdMin != value)
                {
                    _thresholdMin = value;
                    this.NotifyPropertyChanged("thresholdMin");
                }
            }
        }




        private double _thresholdMax;
        /// <summary>
        /// Maximum possible threshold value for this channel
        /// </summary>
        public double thresholdMax
        {
            get
            {
                return _thresholdMax;
            }

            set
            {
                if (_thresholdMax != value)
                {
                    _thresholdMax = value;
                    this.NotifyPropertyChanged("thresholdMax");
                }
            }
        }


        public ThresholdControl(uint channel, double thresholdMin, double thresholdMax)
        {
            _channel = channel;
            this.thresholdMin = thresholdMin;
            this.thresholdMax = thresholdMax;
            this.threshold = thresholdMin + (thresholdMax - thresholdMin) / 2.0;

            this.selectedMovement = 1;
         
        }




    }
}
