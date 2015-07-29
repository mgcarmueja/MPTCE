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

    /// <summary>
    /// Auxiliary class implementing a filter entry to be used on an "open file" dialog
    /// </summary>
    public class FilterEntry
    {
        private string _description;
        private List<string> _extensions;

        public FilterEntry(string description, List<string> extensions)
        {
            _description = description;
            _extensions = extensions;
        }

        public FilterEntry(string description, string[] extensions)
        {
            _description = description;

            _extensions = new List<string>();

            if (extensions != null)
                foreach (string extension in extensions)
                    _extensions.Add(extension);
        }

        /// <summary>
        /// It generates a filter entry as expected by "open file" dialogs in Windows
        /// </summary>
        public override string ToString()
        {

            string output = _description + " (";
            string filter = "";

            for (int i = 0; i < _extensions.Count; i++)
            {
                output = output + "*." + _extensions.ElementAt(i);
                filter = filter + "*." + _extensions.ElementAt(i);

                if (i < (_extensions.Count - 1))
                {
                    output = output + ", ";
                    filter = filter + "; ";
                }
            }
            output = output + ")|" + filter;
            return output;
        }
    }
}
