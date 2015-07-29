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

namespace EMGFramework.Pipelines
{
    public class Pipeline
    {

        // An event that clients can use to be notified whenever the value of the attribute stopPending changes
        public event EventHandler StopPendingChanged;

        // Invoke the StopPendingCanged event; called whenever the attribute stopPending changes
        protected virtual void OnStopPendingChanged(EventArgs e)
        {
            if ( StopPendingChanged!= null)
                StopPendingChanged(this, e);
        }


        private bool _stopPending = false;

        private List<Stage> _Stages;


        /// <summary>
        /// Can be used to indicate that the pipelime can be stopped by another running thread 
        /// </summary>
        public bool stopPending
        {
            get 
            {
                return _stopPending;
            }

            set
            {
                _stopPending = value;
                OnStopPendingChanged(EventArgs.Empty);
            }
        }
        


        public List<Stage> Stages
        {
            get { return _Stages; }
        }

        public Pipeline()
        {
            _Stages = new List<Stage>();
            _stopPending = false;
        }

        /// <summary>
        /// Add a stage to the pipeline and binds its input queue with the output queue of the last stage
        /// currently in the pipeline.
        /// </summary>
        /// <param name="stage"></param>
        public void AddStage(Stage stage)
        {

            if (_Stages.Count == 0) _Stages.Add(stage);
            else
            {
                // We are connecting the outputQueue of the former last stage
                // to the inputQueue of the newly added stage, which is now the last one.

                if ((_Stages[_Stages.Count - 1].enableOutput == true)
                    && (stage.enableInput == true))
                {
                    _Stages.Add(stage);
                    _Stages[_Stages.Count - 1].inputQueue = _Stages[_Stages.Count - 2].outputQueue;
                    _Stages[_Stages.Count - 2].nextStage = _Stages[_Stages.Count - 1];
                }
                // We could think about throwing exceptions when the output is disabled on a stage that is not the last
                // one, or when the input is disabled on a stage that is not the first one in the pipeline

            
            }
        }

        /// <summary>
        /// Sequentially calls the Init method of every Stage object in the pipeline. 
        /// </summary>
        public void Init()
        {
            foreach (Stage item in _Stages) item.Init();
        }

        /// <summary>
        /// Starts the first Stage object in the pipeline. The start command is then cascaded through the pipeline. 
        /// </summary>
        public void Start()
        {
            _Stages[0].Start();
        }

        /// <summary>
        /// Stops the first Stage object in the pipeline. The stop command is then cascaded through the pipeline.
        /// </summary>
        public void Stop()
        {
            _Stages[0].Stop();
            _stopPending = false;

        }

    }
}
