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
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EMGFramework.ValueObjects;
using EMGFramework.Utility;


namespace EMGFramework.PatternRecognizers
{
    /// <summary>
    /// Class for pattern recognition using Linear Discriminant Analysis
    /// </summary>
    public class LDAPatternRecognizer : PatternRecognizer
    {

        private List<double[]> _groupFeatureMeans;
        private double[] _globalFeatureMeans;

        private List<double[,]> _dataArrayList;

        private double[] _lnp;
        private double[,] _iPooledCovarMat;


        new public static string ID
        {
            get
            {
                return "LDA";
            }
        }

        new public static string displayName
        {
            get { return "Linear Discriminant Analysis"; }
        }



        /// <summary>
        /// This static property should be redefined in any children class to indicate whether the pattern recognizer
        /// supports the simultaneous activation of several classes at its output or not.
        /// </summary>
        new public static bool multipleActivationSupport
        {
            get
            {
                return false;
            }
        }



        /// <summary>
        /// Common initialization for all constructors
        /// </summary>
        private void Init()
        {
            //activationFunctionIdx = 0;
            _supportedActivationFunctions = new List<string> { "SoftMax" };
            _supportedNormalizers = new List<string> { "None", "0 to 1", "-1 to 1" };

        }


        public LDAPatternRecognizer(TrainingPackage trainingPackage)
            : base(trainingPackage)
        {
            Init();
        }


        public LDAPatternRecognizer()
            : base()
        {
            Init();
        }



        /// <summary>
        /// Takes the trainigPackage and generates a list of bidimensional arrays. Each array containing the feature vectors
        /// for one trainingSet;
        /// </summary>
        private void DataAsArrayList()
        {
            DataWindow currentWindow;

            _dataArrayList = new List<double[,]>();

            foreach (DataSet trainingSet in trainingPackage.trainingSets)
            {
                double[,] currentArray = new double[trainingSet.set.Count, base.inputDim];

                _dataArrayList.Add(currentArray);
                

                //And now we fill it up with data!

                for (int i = 0; i < trainingSet.set.Count; i++)
                {
                    currentWindow = trainingSet.set.ElementAt(i);

                    for (int j = 0; j < currentWindow.features.Values.Count; j++)
                    {
                        double[] channelVector = (double[]) currentWindow.features.Values.ElementAt(j);
                        for (int k = 0; k < channelVector.Length;k++ )
                            currentArray[i, (j * channelVector.Length) + k] = channelVector[k];
                    }
                }
            }
        }



        /// <summary>
        /// Runs the LDA training algorithm 
        /// </summary>
        public override void RunTraining()
        {
            //base.Runtraining();

            //Before anything, we initialize our data structures
            int dimi, dimj;
            int totalVectors = 0;


            dimj = inputDim;

            _groupFeatureMeans = new List<double[]>();

            foreach (DataSet trainingSet in trainingPackage.trainingSets)
                _groupFeatureMeans.Add(new double[inputDim]);

            _globalFeatureMeans = new double[inputDim];

            DataAsArrayList();

            normalizer.RunOffline(_dataArrayList);

            //First, calculate group and global feature means

            //Group feature means

            int pos = 0;
            foreach (double[,] dataArray in _dataArrayList)
            {
                dimi = dataArray.GetLength(0);
                dimj = dataArray.GetLength(1);
                totalVectors += dimi; //We will use this later for calculating the global feature means

                for (int i = 0; i < dimi; i++)
                {
                    for (int j = 0; j < dimj; j++)
                    {
                        _groupFeatureMeans.ElementAt(pos)[j] += dataArray[i, j] / (double)dimi;
                    }
                }
                pos++;
            }


            //Global feature means

            //First, we count how many feature vectors we have in total

            for (int i = 0; i < _groupFeatureMeans.Count; i++)
            {
                double[] groupMean = _groupFeatureMeans.ElementAt(i);

                for (int j = 0; j < groupMean.Length; j++)
                    _globalFeatureMeans[j] += groupMean[j] * _dataArrayList.ElementAt(i).GetLength(0) / (double)totalVectors;
            }


            //Mean-corrected data

            List<double[,]> meanCorrectedList = new List<double[,]>();

            foreach (double[,] dataArray in _dataArrayList)
            {
                dimi = dataArray.GetLength(0);
                dimj = dataArray.GetLength(1);

                double[,] meanCorrected = new double[dimi, dimj];

                for (int i = 0; i < dimi; i++)
                {
                    for (int j = 0; j < dimj; j++)
                    {
                        meanCorrected[i, j] = dataArray[i, j] - _globalFeatureMeans[j];
                    }
                }

                meanCorrectedList.Add(meanCorrected);
            }

            //Covariance matrix
            List<double[,]> covarMatList = new List<double[,]>();

            foreach (double[,] meanCorrected in meanCorrectedList)
            {
                dimi = meanCorrected.GetLength(0);
                dimj = meanCorrected.GetLength(1);

                double[,] covarMat = new double[dimj, dimj];

                for (int i = 0; i < dimj; i++)
                    for (int j = 0; j < dimj; j++)
                    {
                        for (int k = 0; k < dimi; k++)
                            covarMat[i, j] += meanCorrected[k, i] * meanCorrected[k, j];

                        covarMat[i, j] = covarMat[i, j] / (double)dimi;
                    }

                covarMatList.Add(covarMat);
            }


            //Pooled covariance matrix
            double[,] pooledCovarMat = new double[dimj, dimj];
            int item = 0;

            foreach (double[,] covarMat in covarMatList)
            {
                dimi = covarMat.GetLength(0);
                dimj = covarMat.GetLength(1);

                int trainingSetSize = trainingPackage.trainingSets.ElementAt(item).set.Count;

                for (int i = 0; i < dimi; i++)
                {


                    for (int j = 0; j < dimj; j++)
                    {
                        pooledCovarMat[i, j] += (covarMat[i, j] / (double)totalVectors) * trainingSetSize;
                    }
                }

                item++;
            }

            //Inverse of the pooled covariance matrix

            _iPooledCovarMat = InvertMatrix(pooledCovarMat);

            //Prior probability vector
            _lnp = new double[trainingPackage.trainingSets.Count];

            for (int i = 0; i < _lnp.Length; i++)
            {
                _lnp[i] = Math.Log(trainingPackage.trainingSets.ElementAt(i).set.Count / (double)totalVectors);
            }

            trained = true;

        }


        /// <summary>
        /// Performs classification of an input feature vector and returns the category the vector 
        /// has been classified into. 
        /// </summary>
        /// <param name="inputVector"></param>
        /// <returns></returns>
        public override object Classify(object inputVector)
        {
            //base.Classify();


            //TODO: Use the matrices initialized during training to calculate the discriminant function 
            //and DON'T FORGET to use the activation function if available!

            double[] input = (double[])inputVector;
            double[] output = new double[base.outputDim];

            double[] result = new double[base.inputDim];

            double muiCmuiT;
            double muiCxkT;

            //First, we normalize the input vector using the online normalization function 
            normalizer.RunOnline(input);

            for (int i = 0; i < trainingPackage.movementCodes.Count; i++)
            {
                double[] mu_i = _groupFeatureMeans.ElementAt(i);
                muiCmuiT = 0;
                muiCxkT = 0;

                for (int j = 0; j < base.inputDim; j++)
                {
                    for (int k = 0; k < inputDim; k++)
                    {
                        result[j] += mu_i[j] * _iPooledCovarMat[k, j];
                    }
                    muiCxkT += result[j] * input[j];
                    muiCmuiT += result[j] * mu_i[j];
                    result[j] = 0;
                }

                //We have changed that because now we look up the movement
                //corresponding to the classification directly at the classification to movement code converter
                //output[trainingPackage.movementCodes[i]] =
                   output[i] =
                   muiCxkT - (0.5 * muiCmuiT) + _lnp[i];

            }


            activationFunction.Execute(output);

            return output;
        }


        /// <summary>
        /// We don't do very much in this implementation, as any training information will be
        /// overwritten by a subsequent training. The PatternRecognizer is flagged as untrained though.
        /// </summary>
        public override void ClearTraining()
        {
            trained = false;
        }


        /// <summary>
        /// Given an nXn matrix A, solve n linear equations to find the inverse of A.
        /// this and its auxilliary methods were adapted from 
        /// http://www.rkinteractive.com/blogs/SoftwareDevelopment/post/2013/05/21/Algorithms-In-C-Finding-The-Inverse-Of-A-Matrix.aspx
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        private static double[,] InvertMatrix(double[,] A)
        {
            int n = A.GetLength(0);
            //e will represent each column in the identity matrix
            double[] e;
            //x will hold the inverse matrix to be returned
            double[,] x = new double[n, A.GetLength(1)];

            /*
            * solve will contain the vector solution for the LUP decomposition as we solve
            * for each vector of x.  We will combine the solutions into the double[][] array x.
            * */
            double[] solve;

            //Get the LU matrix and P matrix (as an array)
            Tuple<double[,], int[]> results = LUPDecomposition(A);

            double[,] LU = results.Item1;
            int[] P = results.Item2;

            /*
            * Solve AX = e for each column ei of the identity matrix using LUP decomposition
            * */
            for (int i = 0; i < n; i++)
            {
                e = new double[A.GetLength(1)];
                e[i] = 1;
                solve = LUPSolve(LU, P, e);
                for (int j = 0; j < solve.Length; j++)
                {
                    x[j, i] = solve[j];
                }
            }
            return x;
        }



        /// <summary>
        /// Given L,U,P and b solve for x.
        /// LU will be a n+1xm+1 matrix where the first row and columns are zero.
        /// This is for ease of computation and consistency with Cormen et al.
        /// </summary>
        /// <param name="LU">the L and U matrices as a single matrix LU</param>
        /// <param name="pi">permutation matrix.</param>
        /// <param name="b"></param>
        /// <returns>the solution as a double[]</returns>
        private static double[] LUPSolve(double[,] LU, int[] pi, double[] b)
        {
            int n = LU.GetLength(0) - 1;
            double[] x = new double[n + 1];
            double[] y = new double[n + 1];
            double suml = 0;
            double sumu = 0;
            double lij = 0;

            /*
            * Solve for y using formward substitution
            * */
            for (int i = 0; i <= n; i++)
            {
                suml = 0;
                for (int j = 0; j <= i - 1; j++)
                {
                    /*
                    * Since we've taken L and U as a singular matrix as an input
                    * the value for L at index i and j will be 1 when i equals j, not LU[i][j], since
                    * the diagonal values are all 1 for L.
                    * */
                    if (i == j)
                    {
                        lij = 1;
                    }
                    else
                    {
                        lij = LU[i, j];
                    }
                    suml = suml + (lij * y[j]);
                }
                y[i] = b[pi[i]] - suml;
            }
            //Solve for x by using back substitution
            for (int i = n; i >= 0; i--)
            {
                sumu = 0;
                for (int j = i + 1; j <= n; j++)
                {
                    sumu = sumu + (LU[i, j] * x[j]);
                }
                x[i] = (y[i] - sumu) / LU[i, i];
            }
            return x;
        }


        /*
        * Perform LUP decomposition on a matrix A.
        * Return L and U as a single matrix(double[][]) and P as an array of ints.
        * We implement the code to compute LU "in place" in the matrix A.
        * In order to make some of the calculations more straight forward and to 
        * match Cormen's et al. pseudocode the matrix A should have its first row and first columns
        * to be all 0.
        * */
        private static Tuple<double[,], int[]> LUPDecomposition(double[,] A)
        {
            int n = A.GetLength(0) - 1;
            /*
            * pi represents the permutation matrix.  We implement it as an array
            * whose value indicates which column the 1 would appear.  We use it to avoid 
            * dividing by zero or small numbers.
            * */
            int[] pi = new int[n + 1];
            double p = 0;
            int kp = 0;
            int pik = 0;
            int pikp = 0;
            double aki = 0;
            double akpi = 0;

            //Initialize the permutation matrix, will be the identity matrix
            for (int j = 0; j <= n; j++)
            {
                pi[j] = j;
            }

            for (int k = 0; k <= n; k++)
            {
                /*
                * In finding the permutation matrix p that avoids dividing by zero
                * we take a slightly different approach.  For numerical stability
                * We find the element with the largest 
                * absolute value of those in the current first column (column k).  If all elements in
                * the current first column are zero then the matrix is singluar and throw an
                * error.
                * */
                p = 0;
                for (int i = k; i <= n; i++)
                {
                    if (Math.Abs(A[i, k]) > p)
                    {
                        p = Math.Abs(A[i, k]);
                        kp = i;
                    }
                }
                if (p == 0)
                {
                    throw new Exception("singular matrix");
                }
                /*
                * These lines update the pivot array (which represents the pivot matrix)
                * by exchanging pi[k] and pi[kp].
                * */
                pik = pi[k];
                pikp = pi[kp];
                pi[k] = pikp;
                pi[kp] = pik;

                /*
                * Exchange rows k and kpi as determined by the pivot
                * */
                for (int i = 0; i <= n; i++)
                {
                    aki = A[k, i];
                    akpi = A[kp, i];
                    A[k, i] = akpi;
                    A[kp, i] = aki;
                }

                /*
                    * Compute the Schur complement
                    * */
                for (int i = k + 1; i <= n; i++)
                {
                    A[i, k] = A[i, k] / A[k, k];
                    for (int j = k + 1; j <= n; j++)
                    {
                        A[i, j] = A[i, j] - (A[i, k] * A[k, j]);
                    }
                }
            }
            return Tuple.Create(A, pi);
        }




    }
}
