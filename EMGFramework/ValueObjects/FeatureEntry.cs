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
    /// Represents a feature to be extracted in the treatment phase. Used mostly to fill up the
    /// list of features in the UI and to always have an updated list of selected features 
    /// </summary>
    public class FeatureEntry
    {
        public int idTag { get { return _idTag; } }
        public string name { get { return _name; } }

        private int _idTag;
        private string _name;

        public FeatureEntry(int idTag, string name)
        {
            _idTag = idTag;
            _name = name;
        }
    }
}
