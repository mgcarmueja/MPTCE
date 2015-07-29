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

namespace EMGFramework.ValueObjects
{
    public class ScheduleItem
    {

        private ushort[] _movementComposition;

        public ushort[] movementComposition
        {
            get 
            {
                return _movementComposition;
            }
        }

        private int _movementCode;

        public int movementCode
        {
            get
            {
                return _movementCode;
            }
            set
            {
                _movementCode = value;
            }

        }

        public ScheduleItem()
        {
            _movementCode = -1;
            _movementComposition = null;
        }

        public ScheduleItem(int itemMovementCode)
        {
            _movementCode = itemMovementCode;
            _movementComposition = null;
        }

        public ScheduleItem(int itemMovementCode, ushort[] itemMovementComposition)
        {
            _movementCode = itemMovementCode;

            _movementComposition = new ushort[itemMovementComposition.Length];
            itemMovementComposition.CopyTo(_movementComposition, 0);
        }

        public override string ToString()
        {
            string output="";

                for(int i=0;i<_movementComposition.Length;i++)
                {
                    output = output + _movementComposition[i];
                    if (i < _movementComposition.Length - 1)
                        output = output + ",";
                }

                return output;
        }


    }
}
