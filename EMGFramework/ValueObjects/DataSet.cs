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

namespace EMGFramework.ValueObjects
{
    /// <summary>
    /// A Data set containing a description of the movement it represents and associated DataWindow objects
    /// with extracted features, which is to be part of a TrainingPackage 
    /// </summary>
    public class DataSet
    {

        private int _movementCode;
        public int movementCode
        {
            get 
            {
                return _movementCode;
            }
        }


        private List<object> _movementComposition;
        public List<object> movementComposition
        {
            get
            {
                return _movementComposition;
            }
        }


        private List<DataWindow> _set;
        public List<DataWindow> set
        {
            get 
            {
                return _set;
            }
        }


        public DataSet(int movement)
        {
            _movementCode = movement;
            _set = new List<DataWindow>();
            _movementComposition = new List<object>();
        }


        public DataSet(int movement, List<DataWindow> dataSet)
        {
            _movementCode = movement;
            _set = new List<DataWindow>();
            _movementComposition = new List<object>();
            foreach (DataWindow item in dataSet) _set.Add(item);
        }

        public DataSet(int movement, List<DataWindow> dataSet, List<object> composition)
        {
            _movementCode = movement;
            _set = new List<DataWindow>();
            _movementComposition = new List<object>();
            foreach (DataWindow item in dataSet) _set.Add(item);
            foreach(object item in composition) _movementComposition.Add(item);
        }

        public DataSet(int movement, List<object> composition)
        {
            _movementCode = movement;
            _set = new List<DataWindow>();
            _movementComposition = new List<object>();
            foreach (object item in composition) _movementComposition.Add(item);
        }


    }
}
