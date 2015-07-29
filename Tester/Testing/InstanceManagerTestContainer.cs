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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EMGFramework.DataProvider;
using EMGFramework.PatternRecognizers;
using EMGFramework.Utility;
using EMGFramework.ValueObjects;
using System.Windows;
using System.Windows.Forms;

namespace Tester.Testing
{
    class InstanceManagerTestContainer : TestContainer
    {

        private PatternRecognizer _patternRecognizer;


        public override void Run()
        {
            List<string> itemList = GenericFactory<PatternRecognizer>.Instance.SupportedProducts;

            foreach (string item in itemList)
                InstanceManager<PatternRecognizer>.Instance.Register(GenericFactory<PatternRecognizer>.Instance.CreateProduct(item));
            
            //And one more time to check what happens when overwriting :)


            foreach (string item in itemList)
                InstanceManager<PatternRecognizer>.Instance.Register(GenericFactory<PatternRecognizer>.Instance.CreateProduct(item));

            List<string> instances = InstanceManager<PatternRecognizer>.Instance.RegisteredInstances;

            _patternRecognizer = InstanceManager<PatternRecognizer>.Instance.Retrieve("LDA");

        }
    }
}
