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
using EMGFramework.PatternRecognizers;
using EMGFramework.Utility;
using EMGFramework.ValueObjects;
using System.Windows;
using System.Windows.Forms;

namespace Tester.Testing
{
    class GenericFactoryTestContainer : TestContainer
    {


        public override void Run()
        {
            for (int i = 0; i < 2; i++)
            {
                Console.Out.WriteLine("\n\n++++++++++ Iteration {0}", i);

                //GenericFactory applied to objects that share a common base abstract class
                List<string> patternRecognizerCatalogue = GenericFactory<PatternRecognizer>.Instance.SupportedProducts;
                List<string> activationCatalogue = GenericFactory<Activation>.Instance.SupportedProducts;
                List<string> normalizationCatalogue = GenericFactory<Normalization>.Instance.SupportedProducts;
                
                //GenericFactory applied to objects that implement an interface
                List<string> dataProviderCatalogue = GenericFactory<EMGDataProvider>.Instance.SupportedProducts;


                Console.Out.WriteLine("******* {0}", patternRecognizerCatalogue);
                foreach (string item in patternRecognizerCatalogue)
                {
                    PatternRecognizer patternRecognizer = GenericFactory<PatternRecognizer>.Instance.CreateProduct(item);
                    Console.Out.WriteLine(item);
                }

                Console.Out.WriteLine("******* {0}", activationCatalogue);
                foreach (string item in activationCatalogue)
                {
                    Activation activation = GenericFactory<Activation>.Instance.CreateProduct(item);    
                    Console.Out.WriteLine(item);

                }

                Console.Out.WriteLine("******* {0}", normalizationCatalogue);
                foreach (string item in normalizationCatalogue)
                {
                    Normalization normalization = GenericFactory<Normalization>.Instance.CreateProduct(item);
                    Console.Out.WriteLine(item);
                }

                Console.Out.WriteLine("******* {0}", dataProviderCatalogue);
                foreach (string item in dataProviderCatalogue)
                {
                    EMGDataProvider dataProvider = GenericFactory<EMGDataProvider>.Instance.CreateProduct(item);
                    Console.Out.WriteLine(item);
                }

            }
        }
    }
}
