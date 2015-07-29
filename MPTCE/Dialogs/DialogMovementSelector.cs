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
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMGFramework.File;
using EMGFramework.ValueObjects;

namespace MPTCE.Dialogs
{

    /// <summary>
    /// Movement selector operating through a dialog window
    /// </summary>
    public class DialogMovementSelector : MovementSelector
    {

        public DialogMovementSelector(StringCollection movementNames, List<ScheduleItem> availableMovements)
            : base(movementNames, availableMovements)
        { }


        public DialogMovementSelector()
            : base()
        { }


        public override List<ScheduleItem> SelectMovements()
        {
            List<ScheduleItem> selected = new List<ScheduleItem>();

            DialogMovementSelectorWindow dialog = new DialogMovementSelectorWindow(availableMovements);

            if(dialog.ShowDialog()==true)
            {
                foreach (object item in dialog.SelectedItems) selected.Add((ScheduleItem)item);
            }

            return selected;
        }



    }
}
