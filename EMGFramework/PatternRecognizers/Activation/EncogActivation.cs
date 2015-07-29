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

using Encog.Engine.Network.Activation;

namespace EMGFramework.PatternRecognizers
{
    /// <summary>
    /// Abstract class descending from Activation user to wrap Encog activation functions
    /// </summary>
    public abstract class EncogActivation:Activation
    {
        /// <summary>
        /// Do NOT forget to override this property in the derived classes that you might create. Each
        /// activation function should have a unique name.
        /// </summary>
        new public static string ID
        {
            get { return "Encog Undefined"; }
        }

        private IActivationFunction _activationFunction;

        public IActivationFunction activationFunction
        {
            get
            {
                return _activationFunction;
            }

            set 
            {
                if (_activationFunction != value)
                {
                    _activationFunction = value;
                    base.NotifyPropertyChanged("activationFunction");
                }
            }
        }

        /// <summary>
        /// Creates an instance of the Encog activation function class being wrapped.
        /// </summary>
        /// <returns></returns>
        public abstract IActivationFunction CreateInstance();
         

        
        /// <summary>
        /// This method allows Encog functions to be used like the ones 
        /// provided with MPTCE
        /// </summary>
        /// <param name="input"></param>
        public override void Execute(double[] input)
        {
            if (activationFunction != null)
            {
                activationFunction.ActivationFunction(input, 0, input.Length);
            }
        }


    }
}
