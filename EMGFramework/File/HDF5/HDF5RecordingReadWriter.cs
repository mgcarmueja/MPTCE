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
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using EMGFramework.File;
using EMGFramework.Utility;
using EMGFramework.ValueObjects;
using HDF5DotNet;


namespace EMGFramework.File
{
    public class HDF5RecordingReadWriter : IFileReadWriter
    {
        private FilterSet _supportedFormats;

        public FilterSet supportedFormats
        {
            get
            {
                if (_supportedFormats == null)
                {
                    _supportedFormats = new FilterSet();
                    string[] extensions = { "h5r" };
                    _supportedFormats.filters.Add(new FilterEntry("HDF5 recording files", extensions));
                }
                return _supportedFormats;
            }
        }


        private StringCollection _knownMovements;
        public StringCollection knownMovements
        {
            get
            {
                return _knownMovements;
            }

            set
            {
                if (_knownMovements != value)
                    _knownMovements = value;
            }
        }


        private StringCollection _allowedComplexMovements;
        public StringCollection allowedComplexMovements
        {
            get
            {
                return _allowedComplexMovements;
            }

            set
            {
                if (_allowedComplexMovements != value)
                    _allowedComplexMovements = value;
            }
        }



        private List<ScheduleItem> _requestedMovements;
        public List<ScheduleItem> requestedMovements
        {
            get
            {
                return _requestedMovements;
            }

            set
            {
                if (_requestedMovements != value)
                    _requestedMovements = value;
            }
        }


        private MovementSelector _movementSelector;
        public MovementSelector movementSelector
        {
            get
            {
                return _movementSelector;
            }

            set
            {
                if (_movementSelector != value)
                    _movementSelector = value;
            }
        }



        private string[] _movements;
        private double[] _movSeq;
        private double _restTime;
        private double[] _date;
        private string _deviceName;
        private string _comments;
        private double _version;
        private bool _minmaxDefined;
        private double _numMovs;
        private double _partialDuration;


        private MovListToCodeConverter _converter;
        private MovCodeToStringConverter _movCodeToStringConverter;

        private void Init()
        {
            _version = -1;
            _minmaxDefined = false;
            knownMovements = null;
            requestedMovements = null;
            _converter = null;
            _movCodeToStringConverter = null;
        }

        public HDF5RecordingReadWriter()
        {
            Init();
        }

        public HDF5RecordingReadWriter(MovementSelector movementSelector, StringCollection knownMovements, StringCollection allowedComplexMovements)
        {
            Init();

            _movementSelector = movementSelector;
            _knownMovements = knownMovements;
            _allowedComplexMovements = allowedComplexMovements;
        }


        public object ReadFile(string filename, Recording recording)
        {

            RecordingConfig recordingConfig = recording.parameters;

            byte[] byteArray;
            double[] doubleArray = new double[1];
            string result = "";

            //This list will contain the positions of the movements to load.
            List<int> positionSelection;

            try
            {
                H5FileId fileId = H5F.open(filename, H5F.OpenMode.ACC_RDONLY);
                H5GroupId groupId = H5G.open(fileId, "/recSession");
                H5ObjectInfo objectInfo = H5O.getInfoByName(fileId, "/recSession");

                int nAttributes = (int)objectInfo.nAttributes;

                H5AttributeId[] attributeIds = new H5AttributeId[nAttributes];

                //Attribute extraction and filling of the RecordingConfig object 

                for (int i = 0; i < nAttributes; i++)
                {
                    attributeIds[i] = H5A.openByIndex(groupId, "/recSession", H5IndexType.CRT_ORDER, H5IterationOrder.INCREASING, (long)i);
                    string attributeName = H5A.getName(attributeIds[i]);
                    H5DataTypeId attributeType = H5A.getType(attributeIds[i]);
                    H5AttributeInfo attributeInfo = H5A.getInfo(attributeIds[i]);

                    int dataSize = (int)attributeInfo.dataSize;
                    int typeSize = H5T.getSize(attributeType);

                    if ((attributeName == "mov") || (attributeName == "dev") || (attributeName == "cmt"))
                    {
                        byteArray = new byte[typeSize];
                        H5Array<byte> byteBuffer = new H5Array<byte>(byteArray);
                        H5A.read<byte>(attributeIds[i], attributeType, byteBuffer);
                        result = System.Text.Encoding.UTF8.GetString(byteArray);
                    }
                    else
                    {
                        doubleArray = new double[(int)(dataSize / typeSize)];
                        H5Array<double> doubleBuffer = new H5Array<double>(doubleArray);
                        H5A.read<double>(attributeIds[i], attributeType, doubleBuffer);
                    }

                    switch (attributeName)
                    {
                        case "cT": //contraction time
                            recordingConfig.scheduleItemTime = (float)doubleArray[0];
                            break;
                        case "rT": //rest time
                            _restTime = doubleArray[0];
                            break;
                        case "cmt": //comments
                            _comments = result;
                            break;
                        case "date":
                            _date = doubleArray;
                            break;
                        case "dev": //device name
                            _deviceName = result;
                            break;
                        case "mov": //list of movements
                            //TODO: Extract it to a label sequence. We will use these labels to construct the frame sequence
                            //Beware: each movement will possibly include rest periods. We can use the value of rT to determine
                            //if that is the case --  to guess where the rests are and label them as such!
                            //it seems that all movement recordings in Matlab exports begin with a CONTRACTION

                            _movements = result.Split(';');

                            //plan: concatenate all movements on all channels one after another

                            //This list will be used when writing MPTCE files as a way to store the movements known
                            //by the program. NOT ALL THE MOVEMENTS WILL NORMALLY APPEAR ON A SINGLE RECORDING.
                            //For the BioPatRec samples, it seems that this list actually holds the movements 
                            //encoded in the data structure. That would mean that the only way to describe the movement
                            //would be the string content itself!

                            break;
                        case "nCh": //number of channels
                            recordingConfig.nChannels = (uint)doubleArray[0];
                            break;
                        case "nM": //number of movements
                            //Redundant with list of movements?
                            _numMovs = doubleArray[0];
                            break;
                        case "nR": //number of repetitions
                            recordingConfig.repetitions = (uint)doubleArray[0];
                            break;
                        case "sF": //sampling frequency
                            recordingConfig.sampleFreq = (uint)doubleArray[0];
                            break;
                        case "sT": //sample time -> duration of the recording for each movement, including rests (?)
                            _partialDuration = doubleArray[0];
                            break;

                        //  The following are MPTCE-specific attributes not present in BioPatRec files 
                        case "version": //Format version. If defined, this means that some version of MPTCE created it. If not, it
                            // surely is a Matlab export.
                            _version = doubleArray[0];
                            break;
                        case "mV":
                            recordingConfig.minVoltage = doubleArray[0];
                            _minmaxDefined = true;
                            break;
                        case "MV":
                            recordingConfig.maxVoltage = doubleArray[0];
                            _minmaxDefined = true;
                            break;
                        case "movSeq": //Array defining the sequence of and which movements listed in mov were recorded
                            _movSeq = doubleArray;
                            break;
                        default:
                            break;
                    }

                    H5A.close(attributeIds[i]);
                }



                //Now we do the data reading

                H5DataSetId dataSetId = H5D.open(groupId, "tdata");
                H5DataTypeId dataSetType = H5D.getType(dataSetId);
                int dataTypeSize = (int)H5T.getSize(dataSetType);

                H5DataSpaceId dataSpaceId = H5D.getSpace(dataSetId);
                int nDims = H5S.getSimpleExtentNDims(dataSpaceId);
                long[] dims = H5S.getSimpleExtentDims(dataSpaceId);
                //On Matlab-created HDF5 files, the dimensions are as follows:
                // 0 -> number of movement
                // 1 -> number of channel
                // 2-> samples per movement
                double[, ,] data = new double[dims[0], dims[1], dims[2]];

                H5Array<double> dataBuffer = new H5Array<double>(data);

                H5D.read<double>(dataSetId, dataSetType, dataBuffer);
                H5D.close(dataSetId);

                H5G.close(groupId);
                H5F.close(fileId);
                //We try to identify the movements stored in the data array with a known movement in MPTCE

                List<ScheduleItem> movCodes = GuessMovs(_movements, knownMovements, allowedComplexMovements);

                //With this list, we can now select which movements we want to load. If requestedMovements is defined
                //and has elements, we satisfy that request and load only those movements. Otherwise we present some kind
                //of dialog, so that users can select the movements that they want

                if (requestedMovements != null && requestedMovements.Count > 0)
                {
                    //Initialize positionSelection with the position of each of the desired movements
                    //in the data array
                    positionSelection = FindMovPositions(requestedMovements, movCodes);

                }
                else
                {
                    //Prompt for movements
                    movementSelector.movementNames = knownMovements;
                    movementSelector.availableMovements = movCodes;
                    positionSelection = FindMovPositions(_movementSelector.SelectMovements(), movCodes);
                }


                //We have the information needed to compose our recording data

                recordingConfig.scheduleActive = false;


                FillRecording(data, recording, positionSelection,movCodes);
            }
            catch (HDFException e)
            {
                throw new IFileReadWriterException("Error while reading HDF5 file.", e);
            }


            return recording;
        }



        /// <summary>
        /// Translates the recorded data inside the HDF5 file into a Recording object
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private void FillRecording(double[, ,] data, Recording recording, List<int> positionSelection, List<ScheduleItem> movCodes)
        {
            RecordingConfig recordingConfig = recording.parameters;

            double minVal, maxVal;

            int nMovs = data.GetLength(0);
            int nChannels = data.GetLength(1);
            int nSamples = data.GetLength(2);


            minVal = data[0, 0, 0];
            maxVal = minVal;

            foreach (int i in positionSelection)
            {
                for (int j = 0; j < nChannels; j++)
                    for (int k = 0; k < nSamples; k++)
                    {
                        if (data[i, j, k] < minVal) minVal = data[i, j, k];
                        if (data[i, j, k] > maxVal) maxVal = data[i, j, k];
                    }
            }


            recordingConfig.minVoltage = minVal;
            recordingConfig.maxVoltage = maxVal;

            int count = 0;

            foreach (int movIndex in positionSelection)
            {
                for (int sampleIndex = 0; sampleIndex < nSamples; sampleIndex++)
                {
                    double min = 0;
                    double max = 0;

                    //Here we compose the frame
                    double[] samples = new double[nChannels];

                    for (int channelIndex = 0; channelIndex < nChannels; channelIndex++)
                    {
                        samples[channelIndex] = data[movIndex, channelIndex, sampleIndex];

                        if (channelIndex == 0)
                        {
                            min = samples[channelIndex];
                            max = samples[channelIndex];
                        }
                        else
                        {
                            if (samples[channelIndex] < min)
                                min = samples[channelIndex];
                            else if (samples[channelIndex] > max)
                                max = samples[channelIndex];
                        }
                    }


                    uint sequenceIdx = (uint)((count * nSamples) + sampleIndex);

                    double timeIdx = sequenceIdx / (double)recordingConfig.sampleFreq;

                    Frame newFrame = new Frame(samples, sequenceIdx, timeIdx, min, max);

                    if (_version < 0)
                    //We are reading a file exported form the BioPatRec samples. Rests are recorded but not
                    //marked as movements, thus the movement at position 0 of the movement list is never
                    //a rest. We change this to an MPTCE-friendly codification where 0 means rest
                    {
                        if (sampleIndex % (recordingConfig.sampleFreq * (recordingConfig.scheduleItemTime + _restTime))
                            < (recordingConfig.sampleFreq * recordingConfig.scheduleItemTime))
                            newFrame.movementCode = movCodes[movIndex].movementCode;
                        else newFrame.movementCode = 0;
                    }
                    else //This is an MPTCE-generated file. Rests are always a movement, and the movement code for each 
                        //movement in movIndex is stored in the movSeq attribute
                        newFrame.movementCode = (int)_movSeq[movIndex];

                    recording.data.Add(newFrame);
                }

                count++;
            }
        }



        /// <summary>
        /// Given a list of requested movements and a list of detected movements in the file, it returns a list
        /// with the positions of the requested movements whithin the HDF5 dataset.
        /// </summary>
        /// <param name="requested"></param>
        /// <param name="detected"></param>
        /// <returns></returns>
        private List<int> FindMovPositions(List<ScheduleItem> requested, List<ScheduleItem> detected)
        {
            List<int> foundPositions = new List<int>();

            //We will check against the movement code if each ScheduleItem, because it is faster.

            foreach (ScheduleItem requestedItem in requested)
            {
                for (int i = 0; i < detected.Count; i++)
                {
                    if ((requestedItem.movementCode == detected.ElementAt(i).movementCode)
                        && !foundPositions.Contains(requestedItem.movementCode))
                        foundPositions.Add(i);
                }
            }
            return foundPositions;
        }


        /// <summary>
        /// Scans the list of movements obtained form the HDF5 for each of the known individual movements
        /// The output is an list such as the one used by RecordingConfig as schedule.
        /// </summary>
        /// <param name="movsFromFile"></param>
        /// <param name="knownMovs"></param>
        /// <returns></returns>
        private List<ScheduleItem> GuessMovs(string[] movsFromFile, StringCollection knownMovs, StringCollection allowedMovs)
        {
            List<ScheduleItem> movSchedule = new List<ScheduleItem>();
            List<ushort> movComposition = new List<ushort>();
            int movCode;

            _converter = null;
            if (knownMovs != null && allowedMovs != null)
                _converter = new MovListToCodeConverter(knownMovs.Count, allowedMovs);

            for (int i = 0; i < movsFromFile.Length; i++)
            {
                string movName = movsFromFile[i];

                movComposition.Clear();

                for (int j = 0; j < knownMovs.Count; j++)
                {
                    string knownMov = knownMovs[j];
                    if (System.Text.RegularExpressions.Regex.IsMatch(movName, knownMov, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        //We have found a basic movement inside this movement name
                        movComposition.Add((ushort)j);
                    }
                }

                //Here we need to guess the movement code from the movement composition. We need a converter!
                //For now, we just write a 0
                ushort[] compositionArray = movComposition.ToArray();
                movCode = 0;
                if (_converter != null)
                {
                    movCode = (int)_converter.Convert(compositionArray, null, null, null);
                }
                ScheduleItem scheduleItem = new ScheduleItem((ushort)movCode, movComposition.ToArray());
                movSchedule.Add(scheduleItem);

            }
            return movSchedule;
        }



        public void WriteFile(string filename, Recording recording)
        {

            byte[] byteArray;
            double[] doubleArray = new double[1];
            string result;
            RecordingConfig recordingConfig = recording.parameters;

            H5DataTypeId dataTypeId;
            H5DataSpaceId dataSpaceId;
            H5AttributeId attributeId;

            long[] attributeDims;

            _movCodeToStringConverter = new MovCodeToStringConverter(knownMovements, allowedComplexMovements);

            try
            {
                H5FileId fileId = H5F.create(filename, H5F.CreateMode.ACC_TRUNC);

                H5GroupId groupId = H5G.create(fileId, "/recSession");

                //Writing recSession attributes

                //sF - sampling frequency
                dataTypeId = H5T.copy(H5T.H5Type.NATIVE_DOUBLE);
                attributeDims = new long[1];
                attributeDims[0] = doubleArray.Length;
                dataSpaceId = H5S.create_simple(1, attributeDims);
                attributeId = H5A.create(groupId, "sF", dataTypeId, dataSpaceId);
                doubleArray[0] = recording.parameters.sampleFreq;
                H5A.write<double>(attributeId, dataTypeId, new H5Array<double>(doubleArray));

                H5A.close(attributeId);
                H5T.close(dataTypeId);
                H5S.close(dataSpaceId);


                //sT - sample time
                dataTypeId = H5T.copy(H5T.H5Type.NATIVE_DOUBLE);
                attributeDims = new long[1];
                attributeDims[0] = doubleArray.Length;
                dataSpaceId = H5S.create_simple(1, attributeDims);
                attributeId = H5A.create(groupId, "sT", dataTypeId, dataSpaceId);
                doubleArray[0] = recording.parameters.scheduleItemTime * recording.parameters.repetitions * 2; //*2 because rests also count!
                H5A.write<double>(attributeId, dataTypeId, new H5Array<double>(doubleArray));

                H5A.close(attributeId);
                H5T.close(dataTypeId);
                H5S.close(dataSpaceId);


                //cT - contraction time
                dataTypeId = H5T.copy(H5T.H5Type.NATIVE_DOUBLE);
                attributeDims = new long[1];
                attributeDims[0] = doubleArray.Length;
                dataSpaceId = H5S.create_simple(1, attributeDims);
                attributeId = H5A.create(groupId, "cT", dataTypeId, dataSpaceId);
                doubleArray[0] = recording.parameters.scheduleItemTime;
                H5A.write<double>(attributeId, dataTypeId, new H5Array<double>(doubleArray));

                H5A.close(attributeId);
                H5T.close(dataTypeId);
                H5S.close(dataSpaceId);


                //rT - relaxation time
                dataTypeId = H5T.copy(H5T.H5Type.NATIVE_DOUBLE);
                attributeDims = new long[1];
                attributeDims[0] = doubleArray.Length;
                dataSpaceId = H5S.create_simple(1, attributeDims);
                attributeId = H5A.create(groupId, "rT", dataTypeId, dataSpaceId);
                doubleArray[0] = recording.parameters.scheduleItemTime;
                H5A.write<double>(attributeId, dataTypeId, new H5Array<double>(doubleArray));

                H5A.close(attributeId);
                H5T.close(dataTypeId);
                H5S.close(dataSpaceId);


                //nM - number of movements
                dataTypeId = H5T.copy(H5T.H5Type.NATIVE_DOUBLE);
                attributeDims = new long[1];
                attributeDims[0] = doubleArray.Length;
                dataSpaceId = H5S.create_simple(1, attributeDims);
                attributeId = H5A.create(groupId, "nM", dataTypeId, dataSpaceId);
                doubleArray[0] = (recording.parameters.schedule.Count - recording.parameters.scheduleWarmupItems) / (recording.parameters.repetitions * 2);
                H5A.write<double>(attributeId, dataTypeId, new H5Array<double>(doubleArray));

                H5A.close(attributeId);
                H5T.close(dataTypeId);
                H5S.close(dataSpaceId);


                //nR - number of repetitions
                dataTypeId = H5T.copy(H5T.H5Type.NATIVE_DOUBLE);
                attributeDims = new long[1];
                attributeDims[0] = doubleArray.Length;
                dataSpaceId = H5S.create_simple(1, attributeDims);
                attributeId = H5A.create(groupId, "nR", dataTypeId, dataSpaceId);
                doubleArray[0] = recording.parameters.repetitions;
                H5A.write<double>(attributeId, dataTypeId, new H5Array<double>(doubleArray));

                H5A.close(attributeId);
                H5T.close(dataTypeId);
                H5S.close(dataSpaceId);


                //nCh - number of channels
                dataTypeId = H5T.copy(H5T.H5Type.NATIVE_DOUBLE);
                attributeDims = new long[1];
                attributeDims[0] = doubleArray.Length;
                dataSpaceId = H5S.create_simple(1, attributeDims);
                attributeId = H5A.create(groupId, "nCh", dataTypeId, dataSpaceId);
                doubleArray[0] = recording.parameters.nChannels;
                H5A.write<double>(attributeId, dataTypeId, new H5Array<double>(doubleArray));

                H5A.close(attributeId);
                H5T.close(dataTypeId);
                H5S.close(dataSpaceId);


                //date
                doubleArray = new double[6];
                dataTypeId = H5T.copy(H5T.H5Type.NATIVE_DOUBLE);
                attributeDims = new long[1];
                attributeDims[0] = doubleArray.Length;
                dataSpaceId = H5S.create_simple(1, attributeDims);
                DateTime now = DateTime.Now;
                attributeId = H5A.create(groupId, "date", dataTypeId, dataSpaceId);

                doubleArray[0] = now.Year;
                doubleArray[1] = now.Month;
                doubleArray[2] = now.Day;
                doubleArray[3] = now.Hour;
                doubleArray[4] = now.Minute;
                doubleArray[5] = now.Second;

                H5A.write<double>(attributeId, dataTypeId, new H5Array<double>(doubleArray));


                //mov - list of movements
                int lastUsedCode = 0;
                int movsProcessed = 0;
                result = "";

                //foreach (ScheduleItem item in recording.parameters.schedule)
                for (int i = 0; i < recording.parameters.schedule.Count; i++)
                {
                    ScheduleItem item = recording.parameters.schedule[i];

                    int currentCode = item.movementCode;

                    if (currentCode != 0 && currentCode != lastUsedCode)
                    {
                        string movementName = (string)_movCodeToStringConverter.Convert(currentCode, null, null, null);


                        if (movsProcessed > 0)
                            result = result + ";";

                        result = result + movementName;


                        lastUsedCode = currentCode;
                        movsProcessed++;
                    }
                }

                byteArray = System.Text.Encoding.UTF8.GetBytes(result);

                dataTypeId = H5T.copy(H5T.H5Type.C_S1);
                H5T.setSize(dataTypeId, byteArray.Length);
                dataSpaceId = H5S.create(H5S.H5SClass.SCALAR);
                attributeId = H5A.create(groupId, "mov", dataTypeId, dataSpaceId);

                H5A.write<byte>(attributeId, dataTypeId, new H5Array<byte>(byteArray));

                H5A.close(attributeId);
                H5T.close(dataTypeId);
                H5S.close(dataSpaceId);


                //dev - device name
                result = "MPTCE";

                byteArray = System.Text.Encoding.UTF8.GetBytes(result);

                dataTypeId = H5T.copy(H5T.H5Type.C_S1);
                H5T.setSize(dataTypeId, byteArray.Length);
                dataSpaceId = H5S.create(H5S.H5SClass.SCALAR);
                attributeId = H5A.create(groupId, "dev", dataTypeId, dataSpaceId);

                H5A.write<byte>(attributeId, dataTypeId, new H5Array<byte>(byteArray));

                H5A.close(attributeId);
                H5T.close(dataTypeId);
                H5S.close(dataSpaceId);


                //cmt -comments
                result = "Created with MPTCE";

                byteArray = System.Text.Encoding.UTF8.GetBytes(result);

                dataTypeId = H5T.copy(H5T.H5Type.C_S1);
                H5T.setSize(dataTypeId, byteArray.Length);
                dataSpaceId = H5S.create(H5S.H5SClass.SCALAR);
                attributeId = H5A.create(groupId, "cmt", dataTypeId, dataSpaceId);

                H5A.write<byte>(attributeId, dataTypeId, new H5Array<byte>(byteArray));

                H5A.close(attributeId);
                H5T.close(dataTypeId);
                H5S.close(dataSpaceId);

                /*
                 * Now writing the recording data
                 */

                //We will reuse the movsProcessed variable to help on defining the dimensions of the data array
                attributeDims = new long[3];
                attributeDims[0] = movsProcessed;
                attributeDims[1] = recording.parameters.nChannels;
                attributeDims[2] = (int)(recording.parameters.scheduleItemnSamples * recording.parameters.repetitions * 2);


                double[, ,] data = new double[attributeDims[0], attributeDims[1], attributeDims[2]];

                //Filling up the data array
                for (int i = 0; i < attributeDims[0]; i++)
                {
                    for (int j = 0; j < attributeDims[1]; j++)
                    {
                        for (int k = 0; k < attributeDims[2]; k++)
                        {
                            Frame currentFrame = recording.data[(int)((i * attributeDims[2]) + k)];
                            data[i, j, k] = currentFrame.samples[j];
                        }
                    }
                }

                //Writing the data array to the file
                dataTypeId = H5T.copy(H5T.H5Type.NATIVE_DOUBLE);
                dataSpaceId = H5S.create_simple(attributeDims.Length, attributeDims);

                H5DataSetId dataSetId = H5D.create(groupId, "tdata", dataTypeId, dataSpaceId);
                H5D.write(dataSetId, dataTypeId, new H5Array<double>(data));
                H5D.close(dataSetId);

                H5G.close(groupId);

                H5F.close(fileId);
            }
            catch (HDFException e)
            {
                Debug.WriteLine("Error saving HDF5 file");
                throw new IFileReadWriterException("Error saving HDF5 file", e);
            }

        }
    }
}
