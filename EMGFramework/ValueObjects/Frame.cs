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

namespace EMGFramework.ValueObjects
{
    /// <summary>
    /// Implements a data frame. Data frames are the data units used to transmit samples between processing units in the pipeline.
    /// </summary>
    public class Frame
    {
        private uint _sequenceIdx;
        private int _nsamples;
        private double _timeIdx;
        private double[] _samples;
        private double _minVal = 0;
        private double _maxVal = 0;
        private List<Object> _tagList;
        private int _movementCode;

        public Frame(double[] samples, UInt32 sequenceIdx, double timeIdx, double minVal, double maxVal)
        {
            _sequenceIdx = sequenceIdx;
            _timeIdx = timeIdx;
            if (samples != null) _nsamples = samples.Length;
            else _nsamples = 0;
            _samples = new double[_nsamples];
            _minVal = minVal;
            _maxVal = maxVal;
            if(samples!=null) samples.CopyTo(_samples, 0);
            _tagList = new List<Object>();

            _movementCode = -1;
        }


        public UInt32 sequenceIdx
        {
            get { return _sequenceIdx; }
            set { _sequenceIdx = value; }
        }

        public double timeIdx
        {
            get { return _timeIdx; }
            set { _timeIdx = value; }
        }

        public int nsamples
        {
            get { return _nsamples; }
        }

        public double[] samples
        {
            get { return _samples; }
        }

        public double minVal
        {
            get { return _minVal; }
            set { _minVal = value; }
        }

        public double maxVal
        {
            get { return _maxVal; }
            set { _maxVal = value; }
        }

        public override string ToString()
        {
            string output = "<k:" + _movementCode + "; s:" + _sequenceIdx + "; t:" + _timeIdx + "; m:" + _minVal + "; M:" + _maxVal + "; ";

            for (UInt32 i = 0; i < _nsamples; i++)
            {
                output = output + " " + _samples[i];
            }


            output = output + ">";
            return output;
        }

        /// <summary>
        /// A list of generic objects that are used as tags for labelling a frame
        /// </summary>
        public List<Object> tagList
        {
            get { return _tagList; }
        }

        /// <summary>
        /// Alternate way of labelling a frame by using an integer to encode the movement
        /// </summary>
        public int movementCode
        {
            get { return _movementCode; }
            set { _movementCode = value; }
        }

        /// <summary>
        /// Returns an exact copy of the current Frame object
        /// </summary>
        /// <returns></returns>
        public Frame Clone()
        {
            Frame output;

            output = new Frame(samples,sequenceIdx,timeIdx,minVal,maxVal);


            return output;
        }

    }
}
