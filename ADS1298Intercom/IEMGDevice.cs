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
using System.ServiceModel;

namespace ADS1298Intercom
{

    [ServiceContract(SessionMode = SessionMode.Required,
     CallbackContract = typeof(IEMGCallbacks))]
    public interface IEMGDevice
    {
        [OperationContract]
        ushort GetNChannels();

        [OperationContract]
        UInt32 GetSamplesPerSec();

        [OperationContract]
        void SetSamplesPerSec(ushort samplesPerSecond);

        [OperationContract]
        UInt32 GetGain(UInt32 nChannel);

        [OperationContract]
        void SetGlobalGain(UInt32 value);
        
        [OperationContract]
        float GetVoltagePos();
        
        [OperationContract]
        float GetVoltageNeg();

        [OperationContract]
        void SetChannelActivation(UInt32 channel, bool status);

        [OperationContract]
        void Subscribe();

        [OperationContract]
        void Unsubscribe();

        [OperationContract]
        void ProcessData();

        [OperationContract]
        void SaveSettings();
    }


    


}
