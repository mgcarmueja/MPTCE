/*
 * Copyright 2015 Martin Garcia Carmueja 
 * 
 *  This file is part of the Myoelectric Personal Training and Control Environment (MPTCE).
 *
 *  MPTCE is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  MPTCE is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with MPTCE.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPdevice;

namespace ADS1298Server
{
    /// <summary>
    /// A Singleton containing one instance of EPdeviceManager.This is used by the class EMGDevice
    /// to simplify access to the device(s). 
    /// </summary>
    public sealed class EMGSingleton
    {
        private static volatile EPdeviceManager instance;
        private static object syncRoot = new Object();

        private EMGSingleton() { }

        public static EPdeviceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new EPdeviceManager();
                    }
                }

                return instance;
            }
        }
    }
}
