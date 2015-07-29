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
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMGFramework.ValueObjects;
using EMGFramework.Utility;

namespace EMGFramework.File
{
    /// <summary>
    /// This abstract class defines a generic implementation for a way to obtain a list of the movements
    /// that a user wants to select from a movement recording file to be loaded.
    /// </summary>
    public abstract class MovementSelector
    {

        private List<ScheduleItem> _avalilableMovements;
        /// <summary>
        /// List of all available movements in the recording file
        /// </summary>
        public List<ScheduleItem> availableMovements
        {
            get 
            {
                return _avalilableMovements;
            }

            set 
            {
                if (_avalilableMovements != value)
                    _avalilableMovements = value;
            }
        }



        private StringCollection _movementNames;
        /// <summary>
        /// Collection with the known single movement names
        /// </summary>
        public StringCollection movementNames
        {
            get 
            {
                return _movementNames;
            }

            set 
            {
                if (_movementNames != value)
                    _movementNames = value;
            }
        }



        public MovementSelector(StringCollection movementNames, List<ScheduleItem> availableMovements)
        {

            _movementNames = movementNames;
            _avalilableMovements = availableMovements;
 
        }


        public MovementSelector()
        {
            _movementNames = null;
            _avalilableMovements = null;
        }


        /// <summary>
        /// To be implemented in a derived class. it uses movementNames and acailableMovements to
        /// ask the users for the movements they want to select and returns a list with them
        /// </summary>
        /// <returns></returns>
        public abstract List<ScheduleItem> SelectMovements();


    }
}
