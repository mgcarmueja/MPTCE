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

namespace EMGFramework.PatternRecognizers
{

    /// <summary>
    /// Logistic activation function as defined in
    /// http://www.heatonresearch.com/online/programming-neural-networks-encog-java/chapter-3/page2.html
    /// </summary>
    public class LogisticActivation : Activation
    {

        new public static string ID
        {
            get { return "Logarithmic"; }
        }

        new public static string displayName
        {
            get { return "Logarithmic"; }
        }


        public LogisticActivation()
            : base()
        {
            activationLevel = 1.0;
            activationMin = -1.0;
            activationMax = 1.0;
        }

        public override void Execute(double[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] >= 0)
                    input[i] = Math.Log(1 + input[i]);
                else input[i] = -Math.Log(1 - input[i]);
            }
        }

    }
}
