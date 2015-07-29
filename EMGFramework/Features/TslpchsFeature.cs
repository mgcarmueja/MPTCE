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
    public class TslpchsFeature : Feature
    {

        public new static string ID
        {
            get
            {
                return "tslpchs";
            }
        }

        private Frame _prevFrame, _prevPrevFrame;
        private double[] _prevSlopeArray;


        public override void Process(Frame inputFrame)
        {
            base.Process(inputFrame);
            double slope;

            if (_prevFrame != null && _prevPrevFrame!=null)
            {
               
                for (int i = 0; i < inputFrame.nsamples; i++)
                {
                    slope = (float)((inputFrame.samples[i] - _prevFrame.samples[i]) / (inputFrame.timeIdx - _prevFrame.timeIdx));
                    if ((slope >= 0 && _prevSlopeArray[i] < 0) || (slope < 0 && _prevSlopeArray[i] >= 0))
                        _output[i]++;
                    _prevSlopeArray[i] = slope;
                }                   
            }
            else if (_prevSlopeArray == null || _prevSlopeArray.Length < inputFrame.nsamples)
                _prevSlopeArray = new double[inputFrame.nsamples];


            _prevPrevFrame = _prevFrame;
            _prevFrame = inputFrame;
        }

     
         public override void Clear()
        {
            base.Clear();
            _prevFrame = null;
            _prevPrevFrame = null;
            
        }
    }
}
