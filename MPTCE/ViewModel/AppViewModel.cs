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

namespace MPTCE.ViewModel
{
    /// <summary>
    /// Global ViewModel for the complete application. This class was projected but currently not used by the program.
    /// </summary>
    public class AppViewModel
    {   
        /// <summary>
        /// 
        /// </summary>
        public AcqViewModel acqViewModel;
        
        /// <summary>
        /// 
        /// </summary>
        public TrtViewModel trtViewModel;
        
        /// <summary>
        /// 
        /// </summary>
        public TraViewModel traViewModel;
        
        /// <summary>
        /// 
        /// </summary>
        public ReaViewModel reaViewModel;

        /// <summary>
        /// 
        /// </summary>
        public AppViewModel()
        {

        }
    }
}
