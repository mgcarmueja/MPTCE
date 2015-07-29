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
using EMGFramework.Pipelines;
using EMGFramework.ValueObjects;
using EMGFramework.PatternRecognizers;

namespace EMGFramework.Utility
{
    /// <summary>
    /// Pattern recognition stage. It receives DataWindow objects as inputs and returns a classification
    /// vector of doubles as output. The element at index i in this output vector corresponds to 
    /// the movement i from a list of known movements.
    /// </summary>
    public class PatternRecognition : Stage
    {


        private PatternRecognizer _patternRecognizer;
        /// <summary>
        /// PatternRecognizer object to be used for classification in this stage
        /// </summary>
        public PatternRecognizer patternRecognizer
        {
            get 
            {
                return _patternRecognizer;
            }
            set 
            {
                if (_patternRecognizer != value)
                    _patternRecognizer = value;
            }

        }

        public PatternRecognition()
            : base(2, true, true)
        {

        }

        protected override void TaskIntermediate(object inputItem, out object outputItem)
        {
            DataWindow inputWindow = (DataWindow)inputItem;

            double[] inputVector = new double[_patternRecognizer.inputDim];

            int pos = 0;

            for (int j = 0; j < inputWindow.features.Values.Count; j++)
            {
                double[] channelVector = (double[])inputWindow.features.Values.ElementAt(j);
                for (int k = 0; k < channelVector.Length; k++)
                {
                    if (_patternRecognizer.trainingPackage.recordingConfig.channelMask[k])
                    {
                        inputVector[pos] = channelVector[k];
                        pos++;
                    }
                }
            }

            outputItem = _patternRecognizer.Classify(inputVector);
        }
    
    }

}
