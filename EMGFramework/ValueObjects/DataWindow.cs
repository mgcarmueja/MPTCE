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
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EMGFramework.ValueObjects
{
    /// <summary>
    /// Represents a window containing a set of contiguous frames. Objects of this class are used to detect features
    /// in the EMG signal. These features will be added to a dictionary that is also part of the window
    /// </summary>
    public class DataWindow
    {

        private List<Frame> _frames;

        /// <summary>
        /// List containing all the frames belonging to the DataWindow
        /// </summary>
        public List<Frame> frames
        {
            get
            {
                return _frames;
            }
        }

        /// <summary>
        /// Dictionary containing the features that must be detected. 
        /// </summary>
        private Dictionary<string, object> _features;

        public Dictionary<string, object> features
        {
            get 
            {
                return _features;
            }
        }

        public DataWindow()
        {
            _frames = new List<Frame>();
            _features=new System.Collections.Generic.Dictionary<string,object>();
        }
    }
}
