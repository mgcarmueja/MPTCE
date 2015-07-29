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
    /// Converter for the generation of movement codes or a set of movement codes from a vector
    /// obtained from a pattern recognizer.
    /// </summary>
    public sealed class ClassifToMovCodeConverter : IValueConverter
    {

        /// <summary>
        /// Tells the converter whether to expect a single winning value or several ones 
        /// that must be together converted to a movement code.
        /// </summary>
        public bool multipleActivation;

        /// <summary>
        /// Tolerance for output values to be considered active when using multiple activation.
        /// </summary>
        public double activationTolerance;

        /// <summary>
        /// Value to be regarded as an activation.
        /// </summary>
        public double activationLevel;

        /// <summary>
        /// Used to translate an activation at position X in its corresponding movement code.
        /// </summary>
        public List<int> movementCodes { get; set; }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int movement=0;
            List<ushort> composedMovement;
            double[] classification = (double[])value;
           

            if (multipleActivation)
            {
                //We seek the maximum and minimum values of the input classification vector. Values considered active
                //should be as a minimum a given percentage of the maximum value
                composedMovement=new List<ushort>();

                for(ushort i=0; i<classification.Length;i++)
                {
                    double item = classification[i];
                    if ((item >= activationLevel - activationTolerance)
                        && (item <= activationLevel + activationTolerance))
                        composedMovement.Add((ushort)movementCodes[i]);
                }

                return composedMovement;

            }
            else
            {
                
                double maxVal = classification[0];

                for (int i = 0; i < classification.Length; i++)
                    if (classification[i] > maxVal)
                    {
                        movement = i;
                        maxVal = classification[i];
                    }

                return (int) movementCodes[movement];

            }

            
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
