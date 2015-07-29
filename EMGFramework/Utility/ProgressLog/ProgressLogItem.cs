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

namespace EMGFramework.Utility
{
    public class ProgressLogItem
    {
        private int _type;
        public int type
        {
            get { return _type; }
        }

        private string _message;
        public string message
        {
            get { return _message; }
        }

        public ProgressLogItem(int type, string message)
        {
            _type = type;
            _message = message;
        }


        /*
         Static definitions for the supported log entry types
        */

        public static int Info
        {
            get { return 1; }
        }

        public static int Warning
        {
            get { return 2; }
        }

        public static int Error
        {
            get { return 3; }
        }

    }
}
