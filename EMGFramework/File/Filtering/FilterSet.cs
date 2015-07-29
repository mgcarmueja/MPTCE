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

namespace EMGFramework.File
{
    public class FilterSet
    {
        private List<FilterEntry> _filters;

        public List<FilterEntry> filters
        {
            get
            {
                return _filters;
            }
        }

        public FilterSet()
        {
            _filters = new List<FilterEntry>();
        }

        public new string ToString()
        {
            string output = "";

            for (int i = 0; i < _filters.Count; i++)
            {
                output = output + _filters.ElementAt(i).ToString();
                //output = output + "All Files (*.*)|*.*";
                if (i<_filters.Count-1)
                    output = output + "|";
            }
            return output;
        }

    }
}
