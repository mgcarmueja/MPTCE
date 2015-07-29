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
using EMGFramework.ValueObjects;
using WindowsInput.Native;

namespace MPTCE.RealtimeConsumers.Keyboard
{
    /// <summary>
    /// Defines the mapping of a movement to a key. This class implements INotifyPropertyChanged because this is
    /// required for a BindingList<Keymapping> to produce ListChanged events when the contents of one of its elements is changed.
    /// </summary>
    public class Keymapping:INotifyPropertyChanged
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


        private Movement _movement;
        public Movement movement
        {
            get
            {
                return _movement;
            }

            private set
            {
                Movement oldMovement = _movement;

                if (_movement != value)
                {
                    if (oldMovement != null) oldMovement.PropertyChanged -= movement_PropertyChanged;
                    _movement = value;
                    if(_movement!=null) _movement.PropertyChanged += movement_PropertyChanged;
                    this.NotifyPropertyChanged("movement");
                }
            }
        }


        void movement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.NotifyPropertyChanged("movement");
        }


        private VirtualKeyCode _key;
        public VirtualKeyCode key
        {
            get
            {
                return _key; 
            }

            set
            {
                if (_key != value)
                {
                    _key = value;
                    this.NotifyPropertyChanged("key");
                }
            }
        }

        public Keymapping(Movement newMovement, VirtualKeyCode newKey)
        {
            movement = newMovement;
            key = newKey;
        }
    }
}
