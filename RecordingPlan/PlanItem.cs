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
 *  along with MPTCE. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;

namespace RecordingPlan
{
    /// <summary>
    /// An item of the schedule list
    /// </summary>
    public class PlanItem : INotifyPropertyChanged
    {
        private double _duration;
        private string _description;
        private bool _selected;
        private bool _completed;
        private int _itemCode;

        public event PropertyChangedEventHandler PropertyChanged;


        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public double duration
        {
            get { return _duration; }
        }

        public string description
        {
            get { return _description; }
        }

        public bool selected
        {
            get { return _selected; }
            set 
            {
                if (_selected != value)
                {
                    _selected = value;
                    if (_selected) completed = false;
                    this.NotifyPropertyChanged("selected");
                    this.NotifyPropertyChanged("selectedVisibility");
                    this.NotifyPropertyChanged("durationVisibility");
                }
            }
        }


        public bool completed
        {
            get { return _completed; }
            set 
            {
                if (_completed != value)
                {
                    _completed = value;
                    if (_completed) selected = false;
                    this.NotifyPropertyChanged("completed");
                    this.NotifyPropertyChanged("completedVisibility");
                    this.NotifyPropertyChanged("durationVisibility");
                }
            }
        }


        public int itemCode
        {
            get 
            {
                return _itemCode; 
            }
            set
            {
                if (_itemCode != value)
                {
                    _itemCode = value;
                    this.NotifyPropertyChanged("itemCode");
                }
            }
        }


        public Visibility selectedVisibility
        {
            get
            {
                if (_selected) return Visibility.Visible;
                else return Visibility.Hidden;
            }
        }

        public Visibility completedVisibility
        {
            get
            {
                if (_completed) return Visibility.Visible;
                else return Visibility.Hidden;
            }
        }


        public Visibility durationVisibility
        {
            get 
            {
                if (!(selected || completed))
                    return Visibility.Visible;
                else return Visibility.Hidden;
            }
        }


        public PlanItem(double itemDuration, string itemDescription, int itemCode)
        {
            this._duration = itemDuration;
            this._description = itemDescription;
            this._itemCode = itemCode;
            this._completed = false;
            this._selected = false;
        }


    }
}
