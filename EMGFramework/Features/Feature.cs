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
    /// Abstract class implementing the bases for all feature extraction classes. This family of classes operate
    /// on a per-frame basis and are thus stateful. Client code should collect the final results from the output
    /// property once all frames in a frame block have been processed.
    /// </summary>
    public abstract class Feature
    {
        /// <summary>
        /// This attribute should be present in every class inheriting from this one. This allows a
        /// GenericFactory to be used to create instances of them.
        /// </summary>
        public static string ID 
        {
            get 
            {
                return "Undefined";
            }
        }


        protected double[] _output;

        /// <summary>
        /// A copy of the internal array used for calculating the output. Each call returns a new array.
        /// </summary>
        public double[] output
        {
            get 
            {   
                if(_output==null) return null;

                double[] temp = new double[_output.Length];
                _output.CopyTo(temp, 0);
                return temp;
            }
        }

        /// <summary>
        /// Size of the current frame block. This is can be useful for Feature derived classes that perform averaging.
        /// </summary>
        public int frameBlockSize { get; set;}


        /// <summary>
        /// Process an input frame and updates the output. This method should be Invoked at the very beginning
        /// of any derived class.
        /// </summary>
        public virtual void Process(Frame inputFrame)
        {
            if (_output == null || output.Length!=inputFrame.nsamples) 
                _output = new double[inputFrame.nsamples];


        }


        /// <summary>
        /// Clears the state of the Feature object
        /// </summary>
        public virtual void Clear()
        {
            if (_output != null)
                for (int i = 0; i < _output.Length; i++) _output[i] = 0;
      
        }

    }
}
