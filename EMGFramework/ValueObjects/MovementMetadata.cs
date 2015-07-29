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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMGFramework.ValueObjects
{
    /// <summary>
    /// Metadata used inside of Movement objects when sending them to consumer in the realtime stage
    /// </summary>
    public class MovementMetadata:INotifyPropertyChanged
    {

        /// <summary>
        /// Event handler that fires whenever a property in has been changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


        private bool _applicationActive;
        /// <summary>
        /// True if the current application is active, false otherwise. This property gets refreshed with every 
        /// movement sent to the clients.
        /// </summary>
        public bool applicationActive
        {
            get
            {
                return _applicationActive;
            }

            set
            {
                if (_applicationActive != value)
                {
                    _applicationActive = value;
                    this.NotifyPropertyChanged("applicationActive");
                }
            }
        }


        /// <summary>
        /// Clones the MovementMetadata object
        /// </summary>
        /// <returns>A new MovementMetadata object that is an exact copy of the one upon which he Clone method is invoked</returns>
        public MovementMetadata Clone()
        {
            MovementMetadata clone = new MovementMetadata();

            clone.applicationActive = applicationActive;

            return clone;
        }

    }
}
