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
using EMGFramework.ValueObjects;
using EMGFramework.PatternRecognizers;
using EMGFramework.Utility;
using System.Windows;
using System.Windows.Forms;

namespace Tester.Testing
{
    class ObjectServerTestContainer : TestContainer
    {

        private ObjectServer<int> _objectServer;

        private Guid _consumerGuid;
        private Guid _producerGuid;

        public override void Run()
        {
            _objectServer = new ObjectServer<int>();

            _consumerGuid = _objectServer.RegisterConsumer();

            _producerGuid = _objectServer.RegisterProducer();

            _objectServer.UnregisterProducer(_producerGuid);
            
        }
    }
}
