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
    public class TestViewModel : INotifyPropertyChanged
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



        private TestConsumer _testConsumer;
        public TestConsumer testConsumer
        {
            get
            {
                return _testConsumer;
            }

            set
            {
                if (_testConsumer != value)
                {
                    if (_testConsumer != null)
                        _testConsumer.PropertyChanged -= _testConsumer_PropertyChanged;

                    _testConsumer = value;
                    _testConsumer.PropertyChanged += _testConsumer_PropertyChanged;

                    this.NotifyPropertyChanged("testConsumer");
                }
            }
        }


        public bool checkMe
        {
            get
            {
                if (_testConsumer != null)
                    return _testConsumer.checkMe;
                else return false;
            }

            set
            {
                if (_testConsumer != null && _testConsumer.checkMe != value)
                    _testConsumer.checkMe = value;
            }
        }


        public string movement
        {
            get
            {
                if (_testConsumer != null)
                    return _testConsumer.movement;
                else return "";
            }

            set
            {
                if (_testConsumer != null && _testConsumer.movement != value)
                    _testConsumer.movement = value;
            }
        }


        public string applicationActive
        {
            get 
            {
                if (_testConsumer != null)
                    return _testConsumer.applicationActive;
                else return "";
            }

            set 
            {
                if (_testConsumer != null && _testConsumer.applicationActive != value)
                    _testConsumer.applicationActive = value;
            }
        }


        void _testConsumer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "checkMe":
                case "movement":
                case "applicationActive":
                    this.NotifyPropertyChanged(e.PropertyName);

                    break;

                default:
                    break;
            }
        }

    }
}
