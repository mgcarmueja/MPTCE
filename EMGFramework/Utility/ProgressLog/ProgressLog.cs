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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMGFramework.Utility
{
    public class ProgressLog
    {
        private BlockingCollection<ProgressLogItem> _logItems;
        public BlockingCollection<ProgressLogItem> logItems
        {
            get 
            {
                return _logItems;
            }
        }

        public ProgressLog()
        {
            _logItems = new BlockingCollection<ProgressLogItem>();
        }

        public void Log(int type, string message)
        {
            if(!logItems.IsAddingCompleted)
            logItems.Add(new ProgressLogItem(type, message));
        }

        public void RefreshQueue()
        {
            _logItems.CompleteAdding();
            _logItems = new BlockingCollection<ProgressLogItem>();
        }

    }
}
