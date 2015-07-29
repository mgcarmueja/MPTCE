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
    public class Movement : INotifyPropertyChanged
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



        private int _idTag;
        public int idTag 
        { 
            get 
            { 
                return _idTag; 
            }
 
            set
            {
                if (_idTag != value)
                {
                    _idTag = value;
                    this.NotifyPropertyChanged("idTag");
                }
            }
        }


        private string _name;
        public string name 
        { 
            get 
            { 
                return _name; 
            }

            set 
            {
                if (_name != value)
                {
                    _name = value;
                    this.NotifyPropertyChanged("name");
                }
            }
        }


        private MovementMetadata _metadata;
        /// <summary>
        /// Movement metadata used to provide additional information to movement consumers.
        /// </summary>
        public MovementMetadata metadata
        {
            get
            {
                return _metadata;
            }
            set
            {
                if (_metadata != value)
                {
                    _metadata = value;
                    this.NotifyPropertyChanged("metadata");
                }
            }
        }



        public Movement(int idTag, string name)
        {
            _idTag = idTag;
            _name = name;
        }
    }
}
