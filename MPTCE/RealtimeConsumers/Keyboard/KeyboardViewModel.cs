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
using MPTCE.RealtimeConsumers.Keyboard;


namespace MPTCE.RealtimeConsumers
{
    public class KeyboardViewModel : INotifyPropertyChanged
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



        private KeyboardConsumer _keyboardConsumer;
        public KeyboardConsumer keyboardConsumer
        {
            get
            {
                return _keyboardConsumer;
            }

            set
            {
                if (_keyboardConsumer != value)
                {
                    if (_keyboardConsumer != null)
                        _keyboardConsumer.PropertyChanged -= _keyboardConsumer_PropertyChanged;

                    _keyboardConsumer = value;
                    _keyboardConsumer.PropertyChanged += _keyboardConsumer_PropertyChanged;

                    this.NotifyPropertyChanged("keyboardConsumer");
                }
            }
        }



        public string keyboardActive
        {
            get 
            {
                if (_keyboardConsumer != null)
                    return _keyboardConsumer.keyboardActive;
                else return "";
            }

            set 
            {
                if (_keyboardConsumer != null && _keyboardConsumer.keyboardActive != value)
                    _keyboardConsumer.keyboardActive = value;
            }
        }


        public bool singleStroke

        {
            get
            {
                if (_keyboardConsumer != null)
                    return _keyboardConsumer.singleStroke;
                else return false;
                
            }

            set 
            {
                if (_keyboardConsumer != null && _keyboardConsumer.singleStroke != value)
                    _keyboardConsumer.singleStroke = value;
            }
        }



        void _keyboardConsumer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "keyboardActive":
                case "singleStroke":
                    this.NotifyPropertyChanged(e.PropertyName);

                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// List of known keys
        /// </summary>
        public List<Key> keys
        {
            get
            {
                if (keyboardConsumer != null)
                    return keyboardConsumer.keys;
                else return null;
            }
        }

        /// <summary>
        /// List of current key mappings, derived from the list of selectable movements.
        /// </summary>
        public BindingList<Keymapping> keymappings
        {
            get
            {
                if (keyboardConsumer != null)
                    return keyboardConsumer.keymappings;
                else return null;
            }
        }



        public KeyboardViewModel()
        {
            this.PropertyChanged += KeyboardViewModel_PropertyChanged;
        }


        /// <summary>
        /// This handler was created to propagate "fake" events when a keyboard consumer 
        /// is assigned to the ViewModel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void KeyboardViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "keyboardConsumer":
                    this.NotifyPropertyChanged("keymappings");
                    this.NotifyPropertyChanged("keys");
                    break;

                default:
                    break;
            }
        }


    }
}
