/*
 * Copyright 2015 Martin Garcia Carmueja 
 * 
 *  This file is part of Tester.
 *
 *  Tester is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Tester is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Tester. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EMGFramework.DataProvider;
using EMGFramework.ValueObjects;
using EMGFramework.PatternRecognizers;
using EMGFramework.Utility;
using System.Windows;
using System.Windows.Forms;

namespace Tester.Testing
{
    class LDAPatternRecognizerTestContainer : TestContainer
    {

        private PatternRecognizer _patternRecognizer;
        private TrainingPackage _trainingPackage;



        /// <summary>
        ///Prepare the training package with a training set based on the example solved in 
        ///http://people.revoledu.com/kardi/tutorial/LDA/Numerical%20Example.html
        ///One set of vectors will be assigned to movement code 1, the other to movement code 2.
        /// </summary>
        /// <returns></returns>
        private TrainingPackage MakeTrainingPackage()
        {
            DataWindow tempWindow;
            TrainingPackage trainingPackage;

            List<DataWindow> windowList1 = new List<DataWindow>();
            List<DataWindow> windowList2 = new List<DataWindow>();

            //Filling the window lists with feature vectors
            //List 1
            tempWindow = new DataWindow();
            tempWindow.features.Add("curvature", 2.95);
            tempWindow.features.Add("diameter", 6.63);
            windowList1.Add(tempWindow);

            tempWindow = new DataWindow();
            tempWindow.features.Add("curvature", 2.53);
            tempWindow.features.Add("diameter", 7.79);
            windowList1.Add(tempWindow);

            tempWindow = new DataWindow();
            tempWindow.features.Add("curvature", 3.57);
            tempWindow.features.Add("diameter", 5.65);
            windowList1.Add(tempWindow);

            tempWindow = new DataWindow();
            tempWindow.features.Add("curvature", 3.16);
            tempWindow.features.Add("diameter", 5.47);
            windowList1.Add(tempWindow);

            //List 2
            tempWindow = new DataWindow();
            tempWindow.features.Add("curvature", 2.58);
            tempWindow.features.Add("diameter", 4.46);
            windowList2.Add(tempWindow);

            tempWindow = new DataWindow();
            tempWindow.features.Add("curvature", 2.16);
            tempWindow.features.Add("diameter", 6.22);
            windowList2.Add(tempWindow);

            tempWindow = new DataWindow();
            tempWindow.features.Add("curvature", 3.27);
            tempWindow.features.Add("diameter", 3.52);
            windowList2.Add(tempWindow);

            trainingPackage = new TrainingPackage();
            trainingPackage.trainingSets.Add(new DataSet(1, windowList1));
            trainingPackage.trainingSets.Add(new DataSet(2, windowList2));

            return trainingPackage;

        }


        public override void Run()
        {
            double[] itemToClassify = new double[2] {2.81,5.46};
            double[] result = new double[27];

            _trainingPackage = MakeTrainingPackage();

            //Now we inintialize and train the LDAPatternRecognizer. 
            //Input dimension is 2 because we have 2 features per feature vector. Output dimension is 27 because
            //on the final application there will be 27 possible movements including rest (movement 0)
            _patternRecognizer = GenericFactory<PatternRecognizer>.Instance.CreateProduct("LDA");//new LDAPatternRecognizer(_trainingPackage, 2, 27);

            //_patternRecognizer.inputDim = 2;
            //_patternRecognizer.outputDim = 27;
            _patternRecognizer.trainingPackage = _trainingPackage;

            _patternRecognizer.activationFunctionIdx = 0;
            _patternRecognizer.normalizerIdx = 0;

            _patternRecognizer.RunTraining();


            result = (double[])_patternRecognizer.Classify(itemToClassify);


        }
    }
}
