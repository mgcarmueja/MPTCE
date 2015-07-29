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
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace EMGFramework.Utility
{
    /// <summary>
    /// Value converter used to convert integers representing movement codes to 
    /// strings describing the movement.
    /// </summary>
    public class MovCodeToStringConverter : IValueConverter
    {
        private StringCollection _movementsList;

        private StringCollection _allowedMovements;


        public MovCodeToStringConverter(StringCollection movementsList, StringCollection allowedMovements)
        {
            _movementsList = movementsList;
            _allowedMovements = allowedMovements;
        }


        /// <summary>
        /// Produces the string describing the movement corresponding to a given movement code
        /// </summary>
        /// <param name="value">Movement code used as input</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>a string containing the name of the movement</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            char[] separatorList = new char[] { ',' };

            if (value is int)
            {
                int input = (int)value;
                int nBasicMovs = _movementsList.Count;
                if (input < nBasicMovs)
                    return _movementsList[input];
                else
                {
                    string output = "";

                    string movlist = _allowedMovements[input - nBasicMovs];
                    string[] indMovs = movlist.Split(separatorList);

                    int pos = 0;
                    foreach (string mov in indMovs)
                    {
                        int index = System.Convert.ToInt32(mov);
                        output = output + _movementsList[index];
                        if (pos < (indMovs.Length - 1))
                            output = output + " + ";
                        pos++;
                    }
                    return output;
                }
            }

            return null;       
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
