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


namespace EMGFramework.File
{
    public interface IFileReadWriter
    {
        FilterSet supportedFormats { get; }

        /// <summary>
        /// Strings describing the known basic movements
        /// </summary>
        StringCollection knownMovements { get; set; }

        /// <summary>
        /// Strings describing the allowed combinations of basic movements
        /// </summary>
        StringCollection allowedComplexMovements { get; set; }

        /// <summary>
        /// Requested movement combinations
        /// </summary>
        List<ScheduleItem> requestedMovements { get; set; }

        /// <summary>
        /// Movement selector object to use when asking the user for the movements to select
        /// </summary>
        MovementSelector movementSelector { get; set; }
        
        /// <summary>
        /// Given a filename and an initialized recording, it fills the latter with the data
        /// read from the former.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        object ReadFile(string filename, Recording recording);

        /// <summary>
        /// Creates a file with the specified name and writes the contents of the Recording object into it
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="recordig"></param>
        void WriteFile(string filename, Recording recordig);
        

    }
}
