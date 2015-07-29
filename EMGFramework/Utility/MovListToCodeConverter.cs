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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace EMGFramework.Utility
{
    /// <summary>
    /// Singleton performing movement list to movement code conversion
    /// </summary>
    public sealed class MovListToCodeConverter : IValueConverter
    {

        private int _numSingleMovements;

        private StringCollection _allowedComplexMovements;

        private int[, ,] _conversion;


        public MovListToCodeConverter(int numSingleMovements, StringCollection allowedComplexMovements)
        {
            _numSingleMovements = numSingleMovements;
            _allowedComplexMovements = allowedComplexMovements;

            _conversion = new int[_numSingleMovements,_numSingleMovements,_numSingleMovements];
            PrepareConversion();

        }


        /// <summary>
        /// This auxilliary private method prepares the conversion array that will be used to produce 
        /// complex movement codes from lists of simple movement codes. 
        /// </summary>
        private void PrepareConversion()
        {

           /*
            * The conversion array is an array with as much dimensions as maximum simple movements 
            * can be combined together to perform a complex movement. In this implementation, complex movements
            * result from a combination of 3 simple movements at most, so our array will have 3 dimensions.
            * 
            * Movement codes are stored in the array so that the 3-coordinate [i,j,k] index encodes the movement
            * sequence i,j,k as defined in the string list of allowed movement combinations. One- and two-movement 
            * combinations are encoded at positions [i,j,0] and [i,0,0] respectively. Positions of the array 
            * corresponding to unallowed movement combinations, combinations with leading zeroes or movement combinations
            * that, although allowed, are not specified in the same order as that specified on the list of allowed complex
            * movements, will return the value -1.
            */

            char[] separator = new char[] { ',' };
            int[] codes = new int[3];

            //Initializing the complete array to -1

            for (int i = 0; i < _numSingleMovements; i++)
            {
                for (int j = 0; j < _numSingleMovements; j++)
                {
                    for (int k = 0; k < _numSingleMovements; k++)
                        _conversion[i, j, k] = -1;
                }
            }

            //Setting entries for single movements
            for (int i = 0; i < _numSingleMovements; i++) _conversion[i, 0, 0] = i;

            //Now we go searching allowedComplexMovements and parse its strings into three-dimensional array positions

            int movCode = _numSingleMovements;
            foreach(string item in _allowedComplexMovements)
            {
                string[] substrings = item.Split(separator);

                for (int i = 0; i < codes.Length; i++) codes[i] = 0;
                for (int i = 0; i < substrings.Length; i++) codes[i] = System.Convert.ToInt32(substrings[i]);

                _conversion[codes[0],codes[1],codes[2]]=movCode;

                movCode++;
            }

        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int i=0, j=0, k=0;

            ushort[] movementList = (ushort[])value;

            int length = movementList.Length;

            //We work with 3 simultaneous movements at most

            if (length > 0) i = movementList[0];
            if (length > 1) j = movementList[1];
            if (length > 2) k = movementList[2];

            int result = _conversion[i, j, k];

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
