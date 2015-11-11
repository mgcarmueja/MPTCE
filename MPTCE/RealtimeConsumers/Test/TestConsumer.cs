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
    public class TestConsumer:RealtimeConsumer
    {

        /// <summary>
        /// 
        /// </summary>
        public new static string ID
        {
            get { return "test"; }
        }

        /// <summary>
        /// 
        /// </summary>
        public new static string displayName
        {
            get { return "Test item"; }
        }


        private bool _checkMe;
        /// <summary>
        /// It stores the status of a checkbox in the GUI
        /// </summary>
        public bool checkMe
        {
            get 
            {
                return _checkMe;
            }

            set 
            { 
                if(_checkMe!=value)
                {
                    _checkMe = value;
                    this.NotifyPropertyChanged("checkMe");
                }
            }
        }


       

        private string _movement;
        /// <summary>
        /// Name of the movement being detected during real time movement recognition.
        /// </summary>
        public string movement
        {
            get 
            {
                return _movement;
            }

            set 
            {
                if (_movement != value)
                {
                    _movement = value;
                    this.NotifyPropertyChanged("movement");
                }
            }
        }


        private string _applicationActive;
        /// <summary>
        /// True if the current active window belongs to this application. False otherwise.
        /// </summary>
        public string applicationActive
        { 
            get
            {
                return _applicationActive;
            }

            set
            {
                if (_applicationActive != value)
                {
                    _applicationActive = value;
                    this.NotifyPropertyChanged("applicationActive");
                }
            }
        }


        private TestControl _testControl;

        private MovCodeToStringConverter _converter;

        private Task _taskToRun;

        /// <summary>
        /// 
        /// </summary>
        public TestConsumer()
        :base()
        {
            _converter = new MovCodeToStringConverter();

            _testControl = new TestControl();
            _testControl.viewModel.testConsumer = this;

            consumerControl = new BaseControl();

            ((BaseControl)consumerControl).viewModel.realtimeConsumer = this;
            ((BaseControl)consumerControl).itemsGrid.Children.Clear();
            ((BaseControl)consumerControl).itemsGrid.Children.Add(_testControl);

        }

        /// <summary>
        /// Executes the TestLoad() method as a new Task
        /// </summary>
        public override void Start()
        {
            //Initialization tasks

            //Nothing to do in this implementation

            //Create a task that will do something. This task should use "running"
            //for determinig whether it should continue the processing or not.

            _taskToRun = Task.Run(new Action(TestLoad));
        }

        /// <summary>
        /// This method does currently nothing.
        /// </summary>
        public override void Stop()
        {
            //Wait for the task to finish
            //_taskToRun.Wait();
        }


        /// <summary>
        /// Basic implementation of a worker thread in a RealTimeConsumer. It takes items from a queue
        /// and ends when there are no more elements in the queue and the queue is marked as added complete
        /// or when running becomes false.
        /// </summary>
        private void TestLoad()
        {
           

            foreach (Movement movementObject in movementQueue.GetConsumingEnumerable())
            {
                movement="[" + (string)_converter.Convert(movementObject.idTag, null, null, null) + "]";
                applicationActive = "Active: " + movementObject.metadata.applicationActive.ToString();
            }

            //This covers the case when the object server invalidates the queue.
            running = false;
        }


    }
}
