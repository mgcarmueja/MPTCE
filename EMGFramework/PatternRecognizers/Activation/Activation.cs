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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMGFramework.PatternRecognizers
{
    /// <summary>
    /// Base abstract class for every activation function
    /// </summary>
    public abstract class Activation : INotifyPropertyChanged
    {

        /// <summary>
        /// Event handler that fires whenever a property has been changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }




        /// <summary>
        /// Do NOT forget to override this property in the derived classes that you might create. Each
        /// activation function should have a unique name.
        /// </summary>
        public static string ID
        {
            get { return "Undefined"; }
        }

        /// <summary>
        /// This is required to give a derived class an user-friendly name used at the UI.
        /// </summary>
        public static string displayName
        {
            get { return "Undefined activation function"; }
        }


        private double _activationLevel;
        public double activationLevel
        {
            get
            {
                return _activationLevel;
            }

            protected set
            {
                if (_activationLevel != value)
                {
                    _activationLevel = value;
                    this.NotifyPropertyChanged("activationLevel");

                }
            }
        }


        private double _activationMin=double.MinValue;
        /// <summary>
        /// Minimum possible value produced by the activation function.
        /// If not initialised by the derived class, it defaults to double.MinValue.
        /// </summary>
        public double activationMin
        {
            get
            {
                return _activationMin;
            }

            set 
            {
                if (_activationMin != value)
                {
                    _activationMin = value;
                    this.NotifyPropertyChanged("activationMin");
                }
            }
 
        }



        private double _activationMax=double.MaxValue;
        /// <summary>
        /// Maximum possible value produced by the activation function.
        /// If not initialised by the derived class, it defaults to double.MaxValue.
        /// </summary>
        public double activationMax
        {
            get
            {
                return _activationMax;
            }

            set 
            {
                if (_activationMax != value)
                {
                    _activationMax = value;
                    this.NotifyPropertyChanged("activationMax");
                }
            }
        }



        /// <summary>
        /// This is the method called to  apply the activation function. The input vector is overwritten with
        /// the result.
        /// </summary>
        /// <param name="input"></param>
        public abstract void Execute(double[] input);

    }
}
