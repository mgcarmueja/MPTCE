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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using EMGFramework.ValueObjects;

namespace MPTCE.Dialogs
{



    public class DialogMovementSelectorViewModel:INotifyPropertyChanged
    {

        /// <summary>
        /// Event handler that fires whenever a property in the ViewModel has been changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


        /// <summary>
        /// This is the list of movements that will be displayed on the Listboz of the dialog
        /// </summary>
        public ObservableCollection<ScheduleItem> availableMovements{get; set;}


        public DialogMovementSelectorViewModel()
        {
            availableMovements = new ObservableCollection<ScheduleItem>();
        }

    }
}
