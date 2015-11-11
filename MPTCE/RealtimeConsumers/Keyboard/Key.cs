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
using System.Threading.Tasks;
using WindowsInput.Native;


namespace MPTCE.RealtimeConsumers.Keyboard
{

    /// <summary>
    /// This class models a correspondence between the display name of a key and its associated VirtualKeyCode object
    /// </summary>
    public class Key
    {
        /// <summary>
        /// Display name
        /// </summary>
        public string name
        {
            get;
            set;
        }

        /// <summary>
        /// Code associated to the key. This is what will be sent to other programs by the virtual keyboard.  
        /// </summary>
        public VirtualKeyCode keyCode
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newName"></param>
        /// <param name="newKeyCode"></param>
        public Key(string newName, VirtualKeyCode newKeyCode)
        {
            name = newName;
            keyCode = newKeyCode;
        }


    }
}
