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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace MPTCE.Converters
{
    /// <summary>
    /// Value converter used to convert integers representing movement codes to 
    /// strings describing the movement.
    /// </summary>
    public class MovCodeToStringConverter : IValueConverter
    {
        
        /// <summary>
        /// Performs the conversion
        /// </summary>
        /// <param name="value">Movement code to convert.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>a string describing the movement associated to the input movement code.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            char[] separatorList = new char[] { ',' };

            if (value is int)
            {
                int input = (int)value;
                int nBasicMovs = Properties.Settings.Default.AcqMovementsList.Count;
                if (input < nBasicMovs)
                    return Properties.Settings.Default.AcqMovementsList[input];
                else
                {
                    string output = "";

                    string movlist = Properties.Settings.Default.AcqAllowedMovements[input - nBasicMovs];
                    string[] indMovs = movlist.Split(separatorList);

                    int pos = 0;
                    foreach (string mov in indMovs)
                    {
                        int index = System.Convert.ToInt32(mov);
                        output = output + Properties.Settings.Default.AcqMovementsList[index];
                        if (pos < (indMovs.Length - 1))
                            output = output + " + ";
                        pos++;
                    }
                    return output;
                }
            }

            return null;       
        }

        /// <summary>
        /// This is a dummy method that allows the Converter to comply with the IValueConverter interface. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
