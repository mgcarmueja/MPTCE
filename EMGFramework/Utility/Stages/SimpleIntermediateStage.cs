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
using EMGFramework.Pipelines;


namespace EMGFramework.Utility
{
    /// <summary>
    /// Very simple example intermediate pipeline stage. It takes an unsigned int as an input and returns
    /// true if the value is greater or equal than the last input received, otherwise it returns false.
    /// </summary>
    public class SimpleIntermediateStage : Stage
    {

        private UInt32 _lastInput=0;

        public SimpleIntermediateStage()
            : base(2, true, true)
        {

        }

        protected override void TaskIntermediate(object inputItem, out object outputItem)
        {   
            //DEBUG
            //Console.Out.WriteLine("Last:{0} Current:{1}", _lastInput, (UInt32) inputItem);
            //DEBUG/

            if (_lastInput < ((UInt32) inputItem)) outputItem = false;
            else outputItem = true;
            _lastInput = (UInt32) inputItem;

        }
    
    }

}
