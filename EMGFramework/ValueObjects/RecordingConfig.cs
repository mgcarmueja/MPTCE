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
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMGFramework.ValueObjects
{
    /// <summary>
    /// This class holds the configuration parameters required for a recording session with an IEMGDataProvider
    /// </summary>
    public class RecordingConfig
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


        private UInt32 _sampleFreq;

        /// <summary>
        /// Sampling frequence. 
        /// </summary>
        public UInt32 sampleFreq
        {
            get
            {
                return _sampleFreq;
            }
            set
            {
                if (_sampleFreq != value)
                {
                    _sampleFreq = value;
                    this.NotifyPropertyChanged("sampleFreq");
                }


            }
        }

        private bool[] _channelMask;
        /// <summary>
        /// Mask indicating which of the available channels are enabled or disabled. 
        /// It defaults to all enabled.
        /// </summary>
        public bool[] channelMask
        {
            get 
            {
                return _channelMask;
            }
        }



        private UInt32 _nChannels;

        /// <summary>
        /// Number of available recording channels.
        /// </summary>
        public UInt32 nChannels
        {
            get
            {
                return _nChannels;
            }
            set
            {
                if (_nChannels != value)
                {
                    _nChannels = value;
                    this.NotifyPropertyChanged("nChannels");
                }

            }
        }

        /// <summary>
        /// Number of active channels as per the channelMask
        /// </summary>
        public uint activeChannels
        {
            get 
            {
               uint _activeChannels = 0; 
               for (int i=0; i<channelMask.Length;i++)
                   if (channelMask[i]) _activeChannels++;

               return _activeChannels;
            }
        }


        private UInt32 _gain;

        /// <summary>
        /// Level of signal amplification.
        /// </summary>
        public UInt32 gain
        {
            get
            {
                return _gain;
            }
            set
            {
                if (_gain != value)
                {
                    _gain = value;
                    this.NotifyPropertyChanged("gain");
                }
            }
        }

        private double _minVoltage;

        /// <summary>
        /// Lowest input signal voltage.
        /// </summary>
        public double minVoltage
        {
            get
            {
                return _minVoltage;
            }
            set
            {
                if (_minVoltage != value)
                {
                    _minVoltage = value;
                    this.NotifyPropertyChanged("minVoltage");
                }
            }
        }

        private double _maxVoltage;

        /// <summary>
        /// Highest input signal voltage.
        /// </summary>
        public double maxVoltage
        {
            get
            {
                return _maxVoltage;
            }
            set
            {
                if (_maxVoltage != value)
                {
                    _maxVoltage = value;
                    this.NotifyPropertyChanged("maxVoltage");
                }

            }
        }


        private bool _scheduleActive = false;

        /// <summary>
        /// Specifies whether or not the defined schedule should be used by the EMGDataProvider.
        /// </summary>
        public bool scheduleActive
        {
            get
            {
                return _scheduleActive;
            }

            set
            {

                if (_schedule.Count() > 0) _scheduleActive = false;
                else if (_scheduleActive != value)
                {
                    _scheduleActive = value;
                    this.NotifyPropertyChanged("scheduleActive");
                }
            }

        }


        private ObservableCollection<ScheduleItem> _schedule = new ObservableCollection<ScheduleItem>();

        /// <summary>
        /// Specifies a recording schedule. Each element in the list represents a schedule item 
        /// defined by an array of one or more integer numbers that will be used as identification labels
        /// attached to the corresponding samples obtained by the EMGDataProvider.
        /// </summary>
        public ObservableCollection<ScheduleItem> schedule
        {
            get
            {
                return _schedule;
            }

        }


        private int _scheduleLength = 0;

        public int scheduleLength
        {
            get
            {
                return _scheduleLength;
            }

            private set
            {
                if (_scheduleLength != value)
                {
                    _scheduleLength = value;
                    this.NotifyPropertyChanged("scheduleLength");
                }
            }
        }

        
        private int _scheduleWarmupItems = 0;
        /// <summary>
        /// Number of items at the start of the schedule that sill be used as warmup and
        /// thus will not be recorded. This is usually an initial rest.
        /// </summary>
        public int scheduleWarmupItems
        {
            get
            {
                return _scheduleWarmupItems;
            }

            set
            {
                if (_scheduleWarmupItems != value)
                {
                    _scheduleWarmupItems = value;
                    this.NotifyPropertyChanged("scheduleWarmupItems");
                }
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            scheduleLength = schedule.Count;
        }


        void RecordingConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "nChannels":
                    if (nChannels <= 0) _channelMask = null;
                    else
                    { 
                        _channelMask = new bool[nChannels];
                        for (int i = 0; i < _channelMask.Length; i++) _channelMask[i] = true;
                        
                        if(thresholdSet!=null)
                        thresholdSet.nChannels = nChannels;
                    } 
                    break;
                default:
                    break;
            }
        }



        private void Init()
        {
            _schedule = new ObservableCollection<ScheduleItem>();
            _schedule.CollectionChanged += OnCollectionChanged;
            this.PropertyChanged += RecordingConfig_PropertyChanged;
            thresholdSet = new ThresholdSet(nChannels);
        }


        public RecordingConfig()
        {
            Init();
        }


        public RecordingConfig(UInt32 sampleFreq, UInt32 nChannels)
        {
            _sampleFreq = sampleFreq;
            _nChannels = nChannels;
            Init();
        }


        public RecordingConfig(UInt32 sampleFreq, UInt32 nChannels, UInt32 gain)
        {
            _sampleFreq = sampleFreq;
            _nChannels = nChannels;
            _gain = gain;
            Init();
        }

        public RecordingConfig(UInt32 sampleFreq, UInt32 nChannels, UInt32 gain, float minVoltage, float maxVoltage)
        {
            _sampleFreq = sampleFreq;
            _nChannels = nChannels;
            _gain = gain;
            _minVoltage = minVoltage;
            _maxVoltage = maxVoltage;
            Init();
        }

        /// <summary>
        /// Copies the configuration parameters from another RecordingConfig instance into this one
        /// </summary>
        /// <param name="copiedConfig"></param>
        public void Copy(RecordingConfig sourceConfig)
        {
            gain = sourceConfig.gain;
            maxVoltage = sourceConfig.maxVoltage;
            minVoltage = sourceConfig.minVoltage;
            nChannels = sourceConfig.nChannels;

            for (uint i = 0; i < channelMask.Length; i++)
                SetChannelEnable(i, sourceConfig.channelMask[i]);

            repetitions = sourceConfig.repetitions;
            sampleFreq = sourceConfig.sampleFreq;
            scheduleActive = sourceConfig.scheduleActive;
            scheduleItemnSamples = sourceConfig.scheduleItemnSamples;
            scheduleWarmupItems = sourceConfig.scheduleWarmupItems;
            thresholdSet.Copy(sourceConfig.thresholdSet);
        }


        /// <summary>
        /// Enables or disables a given channel.
        /// </summary>
        /// <param name="nChannel">number of the channel to disable or enable</param>
        /// <param name="status">true for enabling the channel, false for disabling it</param>
        public void SetChannelEnable(uint nChannel, bool status)
        {
            if(nChannel<channelMask.Length && channelMask[nChannel]!=status)
                channelMask[nChannel] = status;
        }



        /// <summary>
        /// Clears the recording configuration and recursivelly calls the Clear() methods of its list-based attributes
        /// </summary>
        public void Clear()
        {
            this.gain = 0;
            this.maxVoltage = 0;
            this.minVoltage = 0;
            this.nChannels = 0;
            this.repetitions = 0;
            this.sampleFreq = 0;
            this.schedule.Clear();
            this.scheduleActive = false;
            this.scheduleItemnSamples = 0;
            this.scheduleItemTime = 0;
            this.scheduleLength = 0;
            this.scheduleWarmupItems = 0;
            this.thresholdSet = null;
        }


        private float _scheduleItemTime;

        private UInt32 _scheduleItemnSamples;

        /// <summary>
        /// Each schedule item will be active during the recording for this specified time in seconds.
        /// This time can be also defined as a number of samples using the property scheduleItemSamples.
        /// Both properties will be updated simultaneously accordingly to the defined value of sampleFreq.
        /// </summary>
        public float scheduleItemTime
        {
            get
            {
                return _scheduleItemTime;
            }

            set
            {
                if (_scheduleItemTime != value)
                {
                    _scheduleItemTime = value;
                    _scheduleItemnSamples = (UInt32)(_scheduleItemTime * _sampleFreq);
                    this.NotifyPropertyChanged("scheduleItemTime");
                }
            }
        }

        /// <summary>
        /// Each schedule item will be active during the recording until this number of samples have been recorded.
        /// This time can be also defined in seconds using the property scheduleItemTime.
        /// Both properties will be updated simultaneously accordingly to the defined value of sampleFreq.
        /// </summary>
        public UInt32 scheduleItemnSamples
        {
            get
            {
                return _scheduleItemnSamples;
            }

            set
            {
                if (_scheduleItemnSamples != value)
                {
                    _scheduleItemnSamples = value;
                    _scheduleItemTime = (float)_scheduleItemnSamples / (float)_sampleFreq;
                    this.NotifyPropertyChanged("scheduleItemnSamples");
                }
            }
        }

        private uint _repetitions;

        public uint repetitions
        {
            get
            {
                return _repetitions;
            }

            set
            {
                if ((value > 0) && (value != _repetitions))
                {
                    _repetitions = value;
                    this.NotifyPropertyChanged("repetitions");
                }
            }

        }

        private ThresholdSet _thresholdSet;

        public ThresholdSet thresholdSet
        {
            get 
            {
                return _thresholdSet;
            }

            set 
            {
                if (_thresholdSet != value)
                {
                    _thresholdSet = value;
                    this.NotifyPropertyChanged("thresholdSet");
                }
            }
        }


    }
}
