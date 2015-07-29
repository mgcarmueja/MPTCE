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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMGFramework.DataProvider
{
 

    public interface IEMGDataProvider
    {

        /// <summary>
        /// This is the queue from which clients will read the data returned by an IEMGDataProvider
        /// </summary>
        BlockingCollection<Object> outputQueue
        {
            get;
        }

        /// <summary>
        /// If the IEMGDataProvider needs to connect to some sort of data source (e.g. a local or remote
        /// data server), it should be done whithin this method.
        /// </summary>
        void Connect();

        /// <summary>
        /// If the IEMGDataProvider needs to disconnect from some sort of data source (e.g. a local or remote
        /// data server), it should happen whithin this method.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Things such as configuring a data acquisition device after having access to it
        /// should be done here.
        /// </summary>
        void Init();

        /// <summary>
        /// Data acquisition starts. The output queue is filled with data objects.
        /// </summary>
        /// <returns></returns>
        BlockingCollection<Object> Start();

        /// <summary>
        /// Data acquisition stops definitely. The output data queue will not be filled again with data 
        /// </summary>
        void Stop();

        /// <summary>
        /// Data acquisition is paused. The output data queue is not invalidated for new additions.
        /// </summary>
        void Pause();

    }
}
