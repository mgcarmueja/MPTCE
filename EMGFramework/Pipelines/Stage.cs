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
using System.Threading;
using System.Threading.Tasks;


namespace EMGFramework.Pipelines
{
    /// <summary>
    /// Abstract class defining a generic pipeline stage
    /// </summary>
    public abstract class Stage
    {
        private Stage _nextStage = null;

        public Stage nextStage
        {
            get { return _nextStage; }
            set { _nextStage = value; }
        }

        private Thread _runThread;

        private Task _runTask;

        protected Int32 _outputQueueLength = 5; //This is an arbitrary value. 

        protected volatile bool _shouldRun;

        private BlockingCollection<Object> _inputQueue = null;

        private BlockingCollection<Object> _outputQueue = null;

        private bool _enableInput = false;

        private bool _enableOutput = false;

        

        /// <summary>
        /// The input queue is initialised when the task is added to the pipeline and corresponds to the
        /// output queue of the previous stage in the pipeline 
        /// </summary>
        public BlockingCollection<Object> inputQueue
        {
            get { return _inputQueue; }
            set { _inputQueue = value; }
        }

        /// <summary>
        /// The output queue is initialised with the class constructor and cannot be assigned form outside
        /// </summary>
        public BlockingCollection<Object> outputQueue
        {
            get { return _outputQueue; }

            //set { _outputQueue = value; }
        }

        public Int32 outputQueueLength
        {
            get { return _outputQueueLength; }
        }


        /// <summary>
        /// Indicates whether or not the input queue is used by the stage
        /// </summary>
        public bool enableInput
        {
            get 
            { 
                return _enableInput; 
            }

            protected set 
            {
                if (_enableInput != value)
                    _enableInput = value;
            }
        }

        /// <summary>
        /// Indicates whether or not the output queue is used by the stage
        /// </summary>
        public bool enableOutput
        {
            get 
            { 
                return _enableOutput; 
            }

            protected set
            {
                if (_enableOutput != value)
                    _enableOutput = value;
            }
        }


        /// <summary>
        /// Creates a Stage with a default output queue length and both input and output queues enabled
        /// </summary>
        protected Stage()
        {
            // Initialising a BlockingCollection without specifying the underlying type
            // should get us a ConcurrentQueue automatically
            _outputQueue = new BlockingCollection<Object>(_outputQueueLength);
            _enableInput = true;
            _enableOutput = true;
        }

        /// <summary>
        /// Creates a Stage with a specified output queue length and both input and output queues enabled
        /// </summary>
        /// <param name="outputQueueLength"></param>
        protected Stage(Int32 outputQueueLength)
        {
            _enableInput = true;
            _enableOutput = true;
            _outputQueueLength = outputQueueLength;
            _outputQueue = new BlockingCollection<Object>(_outputQueueLength);
        }

        /// <summary>
        /// Creates a Stage with a defined output queue length and activation status for input and output queues
        /// </summary>
        /// <param name="inputQueueLength"></param>
        protected Stage(Int32 outputQueueLength, bool enableInput, bool enableOutput)
        {
            _enableInput = enableInput;
            _enableOutput = enableOutput;
            _outputQueueLength = outputQueueLength;
            if (_enableOutput == true) _outputQueue = new BlockingCollection<Object>(_outputQueueLength);
        }

        /// <summary>
        /// Initialisation method to be overriden by a concrete child class. A call to this method 
        /// should however be included in the child class implementation. 
        /// </summary>
        public virtual void Init()
        {
            //This code takes care that, if the pipeline is reutilized several times, the
            //communication queues between pipelines that were marked as adding completed are replaced
            //by fresh ones.

            if ((_enableOutput == true) && (_outputQueue != null) && (_outputQueue.IsAddingCompleted))
            {
                _outputQueue = new BlockingCollection<Object>(_outputQueueLength);
                if (_nextStage != null) _nextStage.inputQueue = _outputQueue;
            }
        }


        /// <summary>
        /// Starts running a new thread with the method Run
        /// </summary>
        public virtual void Start()
        {

            if (_runTask == null || _runTask.Status == TaskStatus.RanToCompletion)
            {
                _shouldRun = true;
                _runTask = Task.Run(new Action(Run));
            }

            /*
            ThreadStart runMethod;


            if ((_runThread == null) || (!_runThread.IsAlive))
            {
                _shouldRun = true;

                runMethod = new ThreadStart(Run);
                _runThread = new Thread(runMethod);
                _runThread.Start();
            }
            */

            if (_nextStage != null) _nextStage.Start();

        }



        /// <summary>
        /// Signals the stage to stop running its workload
        /// </summary>
        public virtual void Stop()
        {
            //Avoid a thread to join itself
            

            if (Task.CurrentId != _runTask.Id)
            {
                _shouldRun = false;

                _runTask.Wait();

                if (_outputQueue != null) _outputQueue.CompleteAdding();
                if (_nextStage != null) _nextStage.Stop();
            }


            /*
            if (_runThread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                _shouldRun = false;
                _runThread.Join();
                if (_outputQueue != null) _outputQueue.CompleteAdding();
                if (_nextStage != null) _nextStage.Stop();
            }//else throw exception!!
             */
 

        }




        /// <summary>
        /// Run method to be optionally overriden by a child class. It provides the dequeue-Task-enqueue loop.
        /// </summary>
        protected virtual void Run()
        {
            Object outputItem = default(Object);


            //Intermediate stage
            if ((_enableInput == true) && (_enableOutput == true))
            {
                while (_shouldRun)
                {
                    foreach (Object inputItem in _inputQueue.GetConsumingEnumerable())
                    {
                        TaskIntermediate(inputItem, out outputItem);

                        if (outputItem != null) _outputQueue.Add(outputItem);
                        else break;
                    }
                }
            }

            //Final stage
            else if ((_enableInput == true) && (_enableOutput == false))
            {
                while (_shouldRun)
                {
                    foreach (Object inputItem in _inputQueue.GetConsumingEnumerable())
                    {
                        TaskFinal(inputItem);
                    }
                }
            }

            //Initial stage
            else if ((_enableInput == false) && (_enableOutput == true))
            {
                while (_shouldRun)
                {
                    TaskInitial(out outputItem);

                    if (outputItem != null) _outputQueue.Add(outputItem);
                }
            }
        }



        /// <summary>
        /// Workload for an intermediate stage accepting input and producing output elements
        /// </summary>
        protected virtual void TaskIntermediate(Object inputItem, out Object outputItem)
        {
            outputItem = null;
        }

        /// <summary>
        /// Workload for a final stage only accepting input elements
        /// </summary>
        protected virtual void TaskFinal(Object inputItem)
        {
        }

        /// <summary>
        /// Workload for an initial stage only producing output elements
        /// </summary>
        protected virtual void TaskInitial(out Object outputItem)
        {
            outputItem = null;
        }



    }
}
