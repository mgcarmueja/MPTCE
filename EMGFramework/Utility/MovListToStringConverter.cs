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
using System.Windows.Data;

namespace EMGFramework.Utility
{
    /// <summary>
    /// Value converter to obtain the movement name from a source array of simple movement codes.
    /// </summary>
    class MovListToStringConverter:IValueConverter
    {

        private StringCollection _basicMovements;
        public StringCollection basicMovements
        {
            get
            {
                return _basicMovements;
            }

            set
            {
                if (_basicMovements != value)
                    _basicMovements = value;
            }
        }


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string movString = "";

            ushort[] movementList = (ushort[])value;

            for (int i = 0; i < movementList.Length; i++)
            {
                movString = movString + basicMovements[movementList[i]];
                if (i < movementList.Length - 1)
                    movString = movString + " + ";
            }

            return movString;
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
