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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMGFramework.ValueObjects;


namespace EMGFramework.Features
{
    /// <summary>
    /// Implements the arithmetic mean for each channel of the input frames
    /// </summary>
    public class MeanFeature : Feature
    {

        public new static string ID
        {
            get
            {
                return "mean";
            }
        }

        public override void Process(Frame inputFrame)
        {
            base.Process(inputFrame);

            for (int i = 0; i < inputFrame.nsamples; i++)
                _output[i] += (inputFrame.samples[i] / (double)frameBlockSize);
        }

        public override void Clear()
        {
            base.Clear();
        }
    }
}
