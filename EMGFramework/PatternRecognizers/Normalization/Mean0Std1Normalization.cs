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
    class Mean0Std1Normalization : Normalization
    {
        new public static string ID
        {
            get { return "Mean=0 Std=1"; }
        }


        public override void RunOnline(double[] featureVector)
        {
            throw (new NotImplementedException("Online "+ID));
        }


        public override void RunOffline(List<double[,]> featureVectors)
        {
            base.RunOffline(featureVectors);

            int nVectors = 0;
            double[] mu = new double[featureVectors.ElementAt(0).GetLength(1)];
            double[] sigma = new double[featureVectors.ElementAt(0).GetLength(1)];

            //First, count how many feature vectors we have
            foreach (double[,] set in featureVectors) nVectors += set.GetLength(0);

            //Calculate the mu vector
            foreach (double[,] set in featureVectors)
            {
                int dimi = set.GetLength(0);
                int dimj = set.GetLength(1);

                for (int j = 0; j < dimj; j++)
                    for (int i = 0; i < dimi; i++)
                        mu[j] += set[i, j] / (double)nVectors;
            }

            //Calculate the sigma vector
            foreach (double[,] set in featureVectors)
            {
                int dimi = set.GetLength(0);
                int dimj = set.GetLength(1);

                for (int j = 0; j < dimj; j++)
                    for (int i = 0; i < dimi; i++)
                        sigma[j] += (set[i, j] - mu[j]) * (set[i, j] - mu[j]) / (double)nVectors;
            }

            for (int j = 0; j < sigma.Length; j++) sigma[j] = Math.Sqrt(sigma[j]);

            //Normalize using the mu and sigma vectors;
            foreach (double[,] set in featureVectors)
            {
                int dimi = set.GetLength(0);
                int dimj = set.GetLength(1);

                for (int j = 0; j < dimj; j++)
                    for (int i = 0; i < dimi; i++)
                        set[i, j] = (set[i, j] - mu[j]) / sigma[j];
            }
        }
    }
}
