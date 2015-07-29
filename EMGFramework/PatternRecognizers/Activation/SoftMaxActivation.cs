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
    /// Softmax activation function as defined in
    /// http://www.mathworks.com/help/nnet/ref/softmax.html?refresh=true
    /// </summary>
    public class SoftMaxActivation : Activation
    {
        new public static string ID
        {
            get { return "SoftMax"; }
        }

        new public static string displayName
        {
            get { return "SoftMax"; }
        }

        public SoftMaxActivation()
            : base()
        {
            activationLevel = 1.0;
            activationMin = 0;
            activationMax = 1.0;
        }

        public override void Execute(double[] input)
        {
            double expSum = 0;
            double newArgument = 100;

            //First of all, we normalize the input vector to reasonable values. 
            //Although not required, we use it to avoid overflow problems
            //when calculatig the exponential of large numbers
            Normalize(input);

            //Now we choose an arbitrary value as argument
            for (int i = 0; i < input.Length; i++)
                input[i] = input[i] * newArgument;



                    for (int i = 0; i < input.Length; i++)
                    {
                        input[i] = Math.Exp(input[i]);
                        expSum += input[i];
                    }

            for (int i = 0; i < input.Length; i++)
            {
                input[i] = input[i] / expSum;
            }
        }


        private void Normalize(double[] input)
        {
            double sum = 0, mod;
            foreach (double v in input)
            {
                sum += v * v;
            }
            mod = Math.Sqrt(sum);

            for (int i = 0; i < input.Length; i++)
            {
                input[i] = input[i] / mod;
            }

        }

    }
}
