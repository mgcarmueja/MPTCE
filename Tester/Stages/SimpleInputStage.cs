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
    /// It implements a very simple input stage that produces a random unsigned integer from 0 to 5000
    /// It requires no special initialisation 
    /// </summary>
    class SimpleInputStage : Stage
    {
        private UInt32 _generatedItems = 0;

        
        public UInt32 generatedItems
        {
            get { return _generatedItems; }
        }

        public SimpleInputStage()
            : base(0,false,true)
        {
            
        }

        protected override void TaskInitial(out object outputItem)
        {
            Random rnd = new Random();

            outputItem = (UInt32)rnd.Next(0, 5000);
            _generatedItems++;
        }
    }
}
