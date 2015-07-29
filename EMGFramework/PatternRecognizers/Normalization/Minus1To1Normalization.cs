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
    class Minus1To1Normalization : Normalization
    {
        new public static string ID
        {
            get { return "-1 to 1"; }
        }


        new public static string displayName
        {
            get { return "-1 to 1"; }
        }


        private double[] _delta;
        private double[] _shift;

        public override void RunOnline(double[] featureVector)
        {
            for (int j = 0; j < featureVector.Length; j++)
            {
                featureVector[j] = ((featureVector[j] + _shift[j]) * 2 / _delta[j]) - 1;
            }
        }

        public override void RunOffline(List<double[,]> featureVectors)
        {
            base.RunOffline(featureVectors);

            int dimj = featureVectors.ElementAt(0).GetLength(1);

            _delta = new double[dimj];
            _shift = new double[dimj];

            for (int j = 0; j < dimj; j++)
            {
                _delta[j] = maxValues[j] - minValues[j];
                _shift[j] = -minValues[j];
            }


            foreach (double[,] set in featureVectors)
            {
                int dimi = set.GetLength(0);

                for (int i = 0; i < dimi; i++)
                    for (int j = 0; j < dimj; j++)
                        set[i, j] = ((set[i, j] + _shift[j]) * 2 / _delta[j]) - 1;
            }
        }

    }
}
