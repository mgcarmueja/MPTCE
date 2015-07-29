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
using System.Threading.Tasks;
using EMGFramework.Pipelines;
using EMGFramework.ValueObjects;
//using OxyPlot.Series;
//using OxyPlot;

namespace Tester.Stages
{

    /// <summary>
    /// Final pipeline stage that updates an given array of OxyPlot PointList objects,
    /// which are used somewhere to display the output of the pipeline.
    /// </summary>
    class OutputDisplayer : Stage
    {

        private double _lastTime = 0;

        public List<String> output { get; private set; }

        public OutputDisplayer()
            :base (2,true,false)
        {

        }

        public override void Init()
        {
            //base.Init();
            output=new List<String>();
        }

        public override void Stop()
        {
            //DEBUG
            Console.Out.WriteLine("Stopping OutputDisplayer...");
            //DEBUG/
            base.Stop();
        }



        /// <summary>
        /// Takes a Frame object out of the input queue, generates a string and adds that string to a list 
        /// </summary>
        /// <param name="inputItem"></param>
        protected override void TaskFinal(object inputItem)
        {
            //base.TaskFinal(inputItem);

            Frame myItem = (Frame)inputItem;
            if (myItem.timeIdx < _lastTime) output.Add(myItem.ToString() + " <=====!!!!!!!");
            else
            {
                output.Add(myItem.ToString());
                _lastTime = myItem.timeIdx;
            }

            /*
            Console.Out.Write("[" + myItem.sequenceIdx + "] [" + myItem.timeIdx + "]");

            for (UInt32 i=0;i<myItem.nsamples;i++)
            {
                Console.Out.Write(" "+ myItem.samples[i] +" ");
            }

            Console.Out.WriteLine();
            */
        }

    }
}
