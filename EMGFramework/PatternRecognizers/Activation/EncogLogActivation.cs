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
    public class EncogLogActivation:EncogActivation
    {
        new public static string ID
        {
            get { return "EncogLog"; }
        }

        new public static string displayName
        {
            get { return "Logarithmic (Encog)"; }
        }

        public override IActivationFunction CreateInstance()
        {
            return new ActivationLOG();
        }


        public EncogLogActivation()
            :base()
        {
            activationFunction = new ActivationLOG();
            activationLevel = 1.0;
            activationMin = double.NegativeInfinity;//-1.0;
            activationMax = double.PositiveInfinity;//1.0;
        }

    }
}
