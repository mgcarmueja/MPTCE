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
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using EMGFramework.Pipelines;
using EMGFramework.DataProvider;
using EMGFramework.ValueObjects;




namespace EMGFramework.Utility
{
    /// <summary>
    /// Final stage for the online pattern recognition pipeline. This stage takes the result from a pattern recognition
    /// process and generates a code representing the detected movement. 
    /// </summary>
    public class MovementGenerator : Stage
    {
        /// <summary>
        /// Converter used to get the movement ID or the list of simple movements depending on the
        /// activation schema used.
        /// </summary>
        private ClassifToMovCodeConverter _converter;

        private MovListToCodeConverter _codeConverter;

        /// <summary>
        /// List containing the most recent matches that are used to determine the output
        /// </summary>
        private List<int> _matchBuffer;

        /// <summary>
        /// Length of the buffer containing the last matches. It defaults to 10.
        /// This buffer is used to determine the output of the stage.
        /// </summary>
        public int matchBufferLength
        {
            get;
            set;
        }

        private int[] _votes;

        private int _nMovements;
        /// <summary>
        /// Number of possible movements. Defaults to 27.
        /// </summary>
        public int nMovements 
        { 
            get
            {
                return _nMovements;
            }
            set
            {
                if (_nMovements != value)
                {
                    _nMovements = value;
                    Array.Resize<int>(ref _votes, _nMovements);
                }
            }
        }

        /// <summary>
        /// Guid obtained from the ObjectServer to where the output of this stage is sent
        /// </summary>
        private Guid _producerGuid;


        private ObjectServer<Movement> _objectServer;
        /// <summary>
        /// Object server used to send the output of this stage to an arbitrary number of receivers
        /// </summary>
        public ObjectServer<Movement> objectServer
        {
            get 
            {
                return _objectServer;
            }
            
            set
            {
                if (_objectServer != value)
                    _objectServer = value;
            }
        }

        //Last output produced. Only changes in the output will be forwarded to the ObjectServer 
        private int _lastOutput;

        //Current output
        private int _output;

        /// <summary>
        /// True if multiple activation is being used for encoding movements, false otherwise
        /// </summary>
        public bool multipleActivation;

        /// <summary>
        /// Level considered to indicate an activation in the input vector
        /// </summary>
        public double activationLevel;


        /// <summary>
        /// Maximum difference allowed between an output and the activation value to be still
        /// considered an activation.
        /// </summary>
        public double activationTolerance;

        /// <summary>
        /// Number of known individual movements
        /// </summary>
        public int numSingleMovements;

        /// <summary>
        /// List of valid combinations of single movements
        /// </summary>
        public StringCollection allowedComplexMovements;

        /// <summary>
        /// True if we are using threshold-based movement detection, false otherwise.
        /// </summary>
        public bool levelControlled;

        private List<ThresholdControl> _thresholdControls;
        /// <summary>
        /// List containing the available ThresholdControl objects for the channels used on
        /// the threshold-based movement detection
        /// </summary>
        public List<ThresholdControl> thresholdControls
        {
            get 
            {
                return _thresholdControls;
            }
        }


        /// <summary>
        /// Metadata that are to be transmitted with the movement. 
        /// </summary>
        public MovementMetadata movementMetadata
        {
            get;
            set;
        }


        private delegate int MovementCodeMaker(object input);

        private MovementCodeMaker movementCodeMaker;

        /// <summary>
        /// List of movement codes used fr matching a classification
        /// to a movement or set of movements.
        /// </summary>
        public List<int> movementCodes { get; set; }


        public MovementGenerator()
            : base(0, true, false)
        {
            matchBufferLength = 10;
            nMovements = 27;
            _matchBuffer = new List<int>();
            _thresholdControls = new List<ThresholdControl>();

        }


        public override void Init()
        {
            base.Init();
            _producerGuid = _objectServer.RegisterProducer();
            _matchBuffer.Clear();
            _lastOutput = int.MinValue;
        
            _converter = new ClassifToMovCodeConverter();
            _converter.multipleActivation = multipleActivation;
            _converter.activationLevel = activationLevel;
            _converter.activationTolerance = activationTolerance;
            _converter.movementCodes = movementCodes;

            _codeConverter = new MovListToCodeConverter(numSingleMovements,allowedComplexMovements);

            if (levelControlled) movementCodeMaker = ThresholdCodeMaker;
            else movementCodeMaker = PatternRecognitionCodeMaker;
         
        }


        /// <summary>
        ///Alternative with threshold-based activation -> we receive a Frame. Whenever one of the channels
        ///listed in ThresholdControls gets over its threshold, its corresponding movement code is returned.
        /// </summary>
        /// <param name="inputItem"></param>
        /// <returns></returns>
        private int ThresholdCodeMaker(object inputItem)
        {
            Frame frame = (Frame)inputItem;
            int movement = 0; //rest

            foreach (ThresholdControl control in thresholdControls)
            {
                if (frame.samples[control.channel-1] >= control.threshold)
                {
                    //Once we find a channel on our list that produces an activation, we don't check further,
                    //so potential composite movements cannot be detected with this code!
                    movement = control.selectedMovement;
                    break;
                }
            }

            return movement;
        }


        /// <summary>
        /// Alternative with pattern recognition -> We receive a classification vector at the input
        /// </summary>
        /// <param name="inputItem"></param>
        /// <returns></returns>
        public int PatternRecognitionCodeMaker(object inputItem)
        {
            int movement=0;
            List<ushort> movementList;

            //This is dependent on the classification method used in the PatternRecognizer 
            //(explicit or implicit movements), so we will be using a converter class

            object output = _converter.Convert(inputItem, null, null, null);

            if (multipleActivation)
            {
                movementList = (List<ushort>)output;
                movement = (int)_codeConverter.Convert(movementList.ToArray(), null, null, null);

                //This is the way we handle the case when the movement combination in the movementList is
                //illegal
                if (movement == -1) movement = movementList.First();
                //TODO?: Perhaps something better could be done, such as making the movement combination valid by removing
                //some movements - But which movement should be removed? Perhaps the one whose activation value was the
                //weakest?
               
            }
            else
            {
                movement = (int)output;
            }

            return movement;
        }



        protected override void TaskFinal(object inputItem)
        {

            ///Frame myItem = (Frame)inputItem;
            //Debug.Write("R");

            
            int movement;

            movement = movementCodeMaker(inputItem);
            

            if (_matchBuffer.Count == matchBufferLength)
                _matchBuffer.RemoveAt(0);

            _matchBuffer.Add(movement);


            //Now the voting
            _output = SelectMovement();

            if (_output != _lastOutput)
            {
                _lastOutput = _output;

                Debug.Write(_output + ":");
                Movement tempMov = new Movement(_output, "");
                tempMov.metadata = movementMetadata.Clone();

                _objectServer.Enqueue(_producerGuid, tempMov);
            }

            
        }



        private int SelectMovement()
        {
            for (int i = 0; i < _votes.Length; i++) _votes[i] = 0;

            foreach (int item in _matchBuffer)
            {
                _votes[item]++;
            }

            int maxVotes = _votes[0];
            int maxPos =0;

            for (int i=0; i< _votes.Length; i++)
                if (_votes[i] > maxVotes)
                {
                    maxPos = i;
                    maxVotes = _votes[i];
                }

            return maxPos;
 
        }

        public override void Stop()
        {
            Debug.WriteLine("Stopping  movement generator...");
            _objectServer.UnregisterProducer(_producerGuid);
            base.Stop();

        }







    }
}
