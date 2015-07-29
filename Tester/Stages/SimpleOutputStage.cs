/*
 * Copyright 2015 Martin Garcia Carmueja 
 * 
 *  This file is part of Tester.
 *
 *  Tester is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Tester is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Tester. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EMGFramework.Pipelines;

namespace Tester.Stages
{  
    /// <summary>
    /// Very simple final pipeline stage. It receives a boolean and prints a string on the standard output
    /// </summary>
    class SimpleOutputStage : Stage
    {
        private UInt32 _processedItems = 0;

        public UInt32 processedItems
        {
            get { return _processedItems; }
        }

        public SimpleOutputStage()
            : base(2, true, false)
        {
        }

        protected override void TaskFinal(object inputItem)
        {
            _processedItems++;
        }

    }
}
