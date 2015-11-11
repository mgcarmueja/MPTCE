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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MPTCE.Converters;
using EMGFramework.ValueObjects;

namespace MPTCE.RealtimeConsumers
{
    /// <summary>
    /// Test implementation of derived class of RealtimeConsumer
    /// </summary>
    public class MovDisplayConsumer:RealtimeConsumer
    {

        /// <summary>
        /// 
        /// </summary>
        public new static string ID
        {
            get { return "movDisplay"; }
        }

        /// <summary>
        /// 
        /// </summary>
        public new static string displayName
        {
            get { return "Movement display"; }
        }

        private string _movementName="No movement";
        /// <summary>
        /// Name of the movement being displayed by the NovDisplayConsumer object
        /// </summary>
        public string movementName
        {
            get 
            {
                return _movementName;
            }

            set 
            {
                if (_movementName != value)
                {
                    _movementName = value;
                    this.NotifyPropertyChanged("movementName");
                }
            }
        }


        private int _movementCode=-1;
        /// <summary>
        /// Code associated to the movement being displayed.
        /// </summary>
        public int movementCode
        {
            get
            {
                return _movementCode;
            }

            set
            {
                if (_movementCode != value)
                {
                    _movementCode = value;
                    this.NotifyPropertyChanged("movementCode");
                }
            }
        }


        private MovCodeToStringConverter _converter;

        private Task _taskToRun;

        /// <summary>
        /// 
        /// </summary>
        public MovDisplayConsumer()
        :base()
        {
            _converter = new MovCodeToStringConverter();

            consumerControl = new MovDisplayControl();

            ((MovDisplayControl)consumerControl).viewModel.movDisplayConsumer = this;

        }

        
        /// <summary>
        /// Starts the GetMovements() method as a new Task.
        /// </summary>
        public override void Start()
        {
            //Initialization tasks

            //Nothing to do in this implementation

            //Create a task that will do something. This task should use "running"
            //for determinig whether it should continue the processing or not.

            _taskToRun = Task.Run(new Action(GetMovements));
        }

        /// <summary>
        /// Waits for the the Task running the GetMovements() method to finish.
        /// </summary>
        public override void Stop()
        {
            //Wait for the task to finish
            _taskToRun.Wait();
        }


        /// <summary>
        /// Basic implementation of a worker thread in a RealTimeConsumer. It takes items from a queue
        /// and ends when there are no more elements in the queue and the queue is marked as added complete
        /// or when running becomes false.
        /// </summary>
        private void GetMovements()
        {
           
            foreach (Movement item in movementQueue.GetConsumingEnumerable())
            {
                movementCode = item.idTag;
                movementName= (string)_converter.Convert(movementCode, null, null, null);
            }

            //This covers the case when the object server invalidates the queue.
            running = false;
        }


    }
}
