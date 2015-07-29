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
using EMGFramework.ValueObjects;
using EMGFramework.Utility;
using System.Windows.Data;

namespace EMGFramework.File
{
    /// <summary>
    /// Implementation of a simple MovementSelector that uses the console to communicate with the user
    /// </summary>
    public class ConsoleMovementSelector:MovementSelector
    {
        public ConsoleMovementSelector(StringCollection movementNames, List<ScheduleItem> availableMovements)
            : base(movementNames, availableMovements) 
        { }


        public ConsoleMovementSelector()
            : base()
        { }


        public override List<ScheduleItem> SelectMovements()
        {
            List<ScheduleItem> selected = new List<ScheduleItem>();
            MovListToStringConverter converter = new MovListToStringConverter();
            converter.basicMovements = movementNames; 
            
            Console.WriteLine("The following movements are available on the file that is being opened:\n");

            foreach (ScheduleItem item in availableMovements)
            {
                Console.WriteLine("{0}: {1}",item.movementCode,converter.Convert(item.movementComposition,null,null,null));
            }

            Console.WriteLine("\n\nPlease type in the codes of the movements to select");
            Console.Write("separed by commas: ");
            string selectStr = Console.ReadLine();


            string[] selections = selectStr.Split(',');


            foreach (string selection in selections)
            {
                int movCode = Convert.ToInt32(selection);

                foreach (ScheduleItem item in availableMovements)
                {
                    if (item.movementCode == movCode)
                        selected.Add(item);
                }
 
            }

            Console.WriteLine("\n\nOK!\n");

            return selected;
        }


    }
}
