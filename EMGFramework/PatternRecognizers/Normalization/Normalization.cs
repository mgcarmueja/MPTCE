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


namespace EMGFramework.PatternRecognizers
{
    public abstract class Normalization
    {
        public static string ID
        {
            get { return "Undefined"; }
        }


        public static string displayName
        {
            get { return "Undefined normalization"; }
        }


        protected bool _initialized=false;
        /// <summary>
        /// Set to true means that an offline normalization has ben performed, so minValues and maxValues have
        /// valid values
        /// </summary>
        public bool initialized
        {
            get { return _initialized;}
        }


        /// <summary>
        /// Minimum values vector used by some normalization algorithms.
        /// Initialized when running an offline normalization
        /// </summary>
        public double[] minValues;

        /// <summary>
        /// Maximum values vector used by some normalization algorithms.
        /// Initialized when running and offline normalization
        /// </summary>
        public double[] maxValues;


        /// <summary>
        /// Runs an online training.
        /// </summary>
        /// <param name="featureVector"></param>
        public abstract void RunOnline(double[] featureVector);


        /// <summary>
        /// Runs an offline training. This base method should be called first of all in any inheriting class
        /// </summary>
        /// <param name="featureVectors"></param>
        public virtual void RunOffline(List<double[,]> featureVectors)
        {
            if (featureVectors == null || featureVectors.Count == 0) return;

            int dimj = featureVectors.ElementAt(0).GetLength(1);

            maxValues = new double[dimj];
            minValues = new double[dimj];

            for (int j = 0; j < dimj; j++)
            {
                maxValues[j] = featureVectors.ElementAt(0)[0, j];
                minValues[j] = maxValues[j];
            }


            foreach (double[,] featureVector in featureVectors)
            {
                int dimi = featureVector.GetLength(0);

                for (int i = 0; i < dimi; i++)
                {
                    for (int j = 0; j < dimj; j++)
                    {
                        if (featureVector[i, j] > maxValues[j]) maxValues[j] = featureVector[i, j];
                        else if (featureVector[i, j] < minValues[j]) minValues[j] = featureVector[i, j];
                    }
                }
            }

            _initialized=true;
        }


    }
}
