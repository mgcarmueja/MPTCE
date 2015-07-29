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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Diagnostics;
using MPTCE.Converters;
using EMGFramework.Timer;
using EMGFramework.ValueObjects;
using WindowsInput;
using WindowsInput.Native;
using MPTCE.RealtimeConsumers.Keyboard;



namespace MPTCE.RealtimeConsumers
{
    /// <summary>
    /// Test implementation of derived class of RealtimeConsumer
    /// </summary>
    public class KeyboardConsumer : RealtimeConsumer
    {


        public new static string ID
        {
            get { return "keyboard"; }
        }


        public new static string displayName
        {
            get { return "Keystroke generator"; }
        }



        private string _keyboardActive;
        public string keyboardActive
        {
            get
            {
                return _keyboardActive;
            }

            set
            {
                if (_keyboardActive != value)
                {
                    _keyboardActive = value;
                    this.NotifyPropertyChanged("keyboardActive");
                }
            }
        }


        private bool _singleStroke;
        /// <summary>
        /// True if single keystrokes instead of repeated key activations
        /// should be produced.
        /// </summary>
        public bool singleStroke
        {
            get
            {
                return _singleStroke;
            }

            set
            {

                if (_singleStroke != value)
                {
                    _singleStroke = value;
                    this.NotifyPropertyChanged("singleStroke");
                }
            }
        }


        private bool _alreadyPressed;

        private KeyboardControl _keyboardControl;

        private MovCodeToStringConverter _converter;

        private Task _taskToRun;

        private InputSimulator _simulator;

        private VirtualKeyCode _virtualKeyCode;

        private VirtualKeyCode _lastVirtualKeyCode;

        private FastTimer _timer;


        //Synchronization object used for accessing the dictionary
        private readonly object syncRoot = new object();

        /// <summary>
        /// Contains the associations between movements and keyboard events to be generated
        /// </summary>
        private Dictionary<int, VirtualKeyCode> _movsToKeyCodes;


        private BindingList<Keymapping> _keymappings;
        /// <summary>
        /// List containing the keymappings for each available movement
        /// </summary>
        public BindingList<Keymapping> keymappings
        {
            get
            {
                return _keymappings;
            }
            private set
            {
                if (_keymappings != value)
                {
                    _keymappings = value;
                    this.NotifyPropertyChanged("keymappings");
                }
            }
        }



        private List<Key> _keys;
        /// <summary>
        /// List of known keys 
        /// </summary>
        public List<Key> keys
        {
            get
            {
                return _keys;
            }
            private set
            {
                if (_keys != value)
                {
                    _keys = value;
                    this.NotifyPropertyChanged("keys");
                }
            }
        }


        public KeyboardConsumer()
            : base()
        {

            singleStroke = false;

            keymappings = new BindingList<Keymapping>();
            InitKeys();
            _converter = new MovCodeToStringConverter();

            _movsToKeyCodes = new Dictionary<int, VirtualKeyCode>();



            _keyboardControl = new KeyboardControl();
            _keyboardControl.viewModel.keyboardConsumer = this;

            consumerControl = new BaseControl();

            ((BaseControl)consumerControl).viewModel.realtimeConsumer = this;
            ((BaseControl)consumerControl).itemsGrid.Children.Clear();
            ((BaseControl)consumerControl).itemsGrid.Children.Add(_keyboardControl);


            _simulator = new InputSimulator();



            //UpdateKeymappings();

            keymappings.ListChanged += keymappings_ListChanged;

            //Configuring a timer that will call the Keystroke method each 100 milliseconds.
            _timer = new FastTimer(3, 100, Keystroke);

        }



        /// <summary>
        /// Handler used to regenerate the dictionary that is accessed by the
        /// thread running the KeyboardLoad method. This update process needs to be thread-safe.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void keymappings_ListChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    lock (syncRoot)
                    {
                        Keymapping item = keymappings[e.NewIndex];
                        _movsToKeyCodes.Add(item.movement.idTag, item.key);
                    }
                    break;

                case ListChangedType.ItemChanged:
                    //The only changes that can be managed here are key mapping changes.
                    //After being added to the keymappings list, the movement attribute of
                    //a keymapping should never change.
                    lock (syncRoot)
                    {
                        Keymapping item = keymappings[e.NewIndex];
                        _movsToKeyCodes.Remove(item.movement.idTag);
                        _movsToKeyCodes.Add(item.movement.idTag, item.key);
                    }
                    break;

                case ListChangedType.Reset:
                case ListChangedType.ItemDeleted:
                    //In these two cases we just regenerate the whole dictionary.
                    lock (syncRoot)
                    {
                        _movsToKeyCodes.Clear();
                        foreach (Keymapping item in keymappings)
                        {
                            _movsToKeyCodes.Add(item.movement.idTag, item.key);
                        }
                    }
                    break;

                default:
                    break;
            }
        }


        
        /// <summary>
        /// Overriding the consumerConfig_PropertyChanged default implementation inherited from RealtimeConsumer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void consumerConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();

            switch (e.PropertyName)
            {
                case "applicationActive":
                    if (consumerConfig.applicationActive || !running) keyboardActive = "Virtual keyboard OFF";
                    else keyboardActive = "Virtual keyboard ON";
                    break;

                default:
                    break;
            }
        }
        


        /// <summary>
        /// Unhooks the list of available movements in the ConsumerConfig object from the handler defined for it in this class.
        /// </summary>
        /// <param name="consumerConfig"></param>
        protected override void ConsumerConfigUnhook(ConsumerConfig consumerConfig)
        {
            base.ConsumerConfigUnhook(consumerConfig);

            consumerConfig.availableMovements.CollectionChanged -= availableMovements_CollectionChanged;
        }


        /// <summary>
        /// Hooks the list of available movements in the ConsumerConfig object to the handler defined for it in this class.
        /// </summary>
        /// <param name="consumerConfig"></param>
        protected override void ConsumerConfigHook(ConsumerConfig consumerConfig)
        {
            base.ConsumerConfigHook(consumerConfig);

            consumerConfig.availableMovements.CollectionChanged += availableMovements_CollectionChanged;
        }


        /// <summary>
        /// Manages the updating of the movement collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void availableMovements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            int pos;
            
            switch (e.Action)
            {
                case (NotifyCollectionChangedAction.Add):

                    pos = e.NewStartingIndex;

                    foreach (Movement movement in e.NewItems)
                    {
                        Keymapping keymapping = new Keymapping(movement, _keys[pos].keyCode);
                        keymappings.Add(keymapping);
                        pos++; 
                    }

                    break;

                case (NotifyCollectionChangedAction.Remove):
                case (NotifyCollectionChangedAction.Reset):
                    
                    keymappings.Clear();
                    pos = 0;

                    foreach (Movement movement in consumerConfig.availableMovements)
                    {
                        Keymapping keymapping = new Keymapping(movement, _keys[pos].keyCode);
                        keymappings.Add(keymapping);
                        pos++;
                    }

                    break;

                default:
                    break;

            }
        }


        /// <summary>
        /// Initializing the hardwired list of known keys 
        /// </summary>
        private void InitKeys()
        {
            keys = new List<Key>();

            keys.Add(new Key("Up", VirtualKeyCode.UP));
            keys.Add(new Key("Down", VirtualKeyCode.DOWN));
            keys.Add(new Key("Left", VirtualKeyCode.LEFT));
            keys.Add(new Key("Right", VirtualKeyCode.RIGHT));
            keys.Add(new Key("Space", VirtualKeyCode.SPACE));
            keys.Add(new Key("Control", VirtualKeyCode.CONTROL));
            keys.Add(new Key("Shift", VirtualKeyCode.SHIFT));

            keys.Add(new Key("Q", VirtualKeyCode.VK_Q));
            keys.Add(new Key("W", VirtualKeyCode.VK_W));
            keys.Add(new Key("E", VirtualKeyCode.VK_E));
            keys.Add(new Key("R", VirtualKeyCode.VK_R));
            keys.Add(new Key("T", VirtualKeyCode.VK_T));
            keys.Add(new Key("Y", VirtualKeyCode.VK_Y));
            keys.Add(new Key("U", VirtualKeyCode.VK_U));
            keys.Add(new Key("I", VirtualKeyCode.VK_I));
            keys.Add(new Key("O", VirtualKeyCode.VK_O));
            keys.Add(new Key("P", VirtualKeyCode.VK_P));
            keys.Add(new Key("A", VirtualKeyCode.VK_A));
            keys.Add(new Key("S", VirtualKeyCode.VK_S));
            keys.Add(new Key("D", VirtualKeyCode.VK_D));
            keys.Add(new Key("F", VirtualKeyCode.VK_F));
            keys.Add(new Key("G", VirtualKeyCode.VK_G));
            keys.Add(new Key("H", VirtualKeyCode.VK_H));
            keys.Add(new Key("J", VirtualKeyCode.VK_J));
            keys.Add(new Key("K", VirtualKeyCode.VK_K));
            keys.Add(new Key("L", VirtualKeyCode.VK_L));
            keys.Add(new Key("Z", VirtualKeyCode.VK_Z));
            keys.Add(new Key("X", VirtualKeyCode.VK_X));
            keys.Add(new Key("C", VirtualKeyCode.VK_C));
            keys.Add(new Key("V", VirtualKeyCode.VK_V));
            keys.Add(new Key("B", VirtualKeyCode.VK_B));
            keys.Add(new Key("N", VirtualKeyCode.VK_N));
            keys.Add(new Key("M", VirtualKeyCode.VK_M));
        }


        public override void Start()
        {
            //Initialization tasks

            //Nothing to do in this implementation

            //Create a task that will do something. This task should use "running"
            //for determinig whether it should continue the processing or not.

            _virtualKeyCode = default(VirtualKeyCode);
            _lastVirtualKeyCode = default(VirtualKeyCode);

            _alreadyPressed = false;

            //Here we should start a timer that will send repeated keyboard events
            _timer.Start();

            _taskToRun = Task.Run(new Action(KeyboardLoad));

        }


        public override void Stop()
        {
            //Wait for the task to finish
            //_taskToRun.Wait();
            _timer.Stop();
        }


        /// <summary>
        /// Basic implementation of a worker thread in a RealTimeConsumer. It takes items from a queue
        /// and ends when there are no more elements in the queue and the queue is marked as added complete
        /// or when running becomes false.
        /// </summary>
        private void KeyboardLoad()
        {

            VirtualKeyCode tempKeyCode;
            bool getResult;

            foreach (Movement movementObject in movementQueue.GetConsumingEnumerable())
            {
                /*
                 * Here we lookup the the movement code in a dictionary and retrieve 
                 * its associated virtual keycode if exists. 
                 */

                lock (syncRoot)
                {
                    getResult = _movsToKeyCodes.TryGetValue(movementObject.idTag,out tempKeyCode);
                }

                if (getResult)
                    _virtualKeyCode = tempKeyCode;
                else _virtualKeyCode = default(VirtualKeyCode);

                Debug.WriteLine("KeyboardConsumer - movCode: " + movementObject.idTag + "; keyCode: ", tempKeyCode);

            }

            //This covers the case when the object server invalidates the queue.
            running = false;
        }


        /// <summary>
        /// This method will be called periadically by a timer. Each time, a sequence of keyboard
        /// events will be generated depending on the key to be generated
        /// </summary>
        private void Keystroke()
        {
            VirtualKeyCode keyCode = _virtualKeyCode;
            ConsumerConfig myConsumerConfig = consumerConfig;


            if (myConsumerConfig != null && myConsumerConfig.applicationActive == false)
            {

                if (_lastVirtualKeyCode != keyCode && _lastVirtualKeyCode != default(VirtualKeyCode))
                {
                    _simulator.Keyboard.KeyUp(_lastVirtualKeyCode);
                    _alreadyPressed = false;
                }

                if (keyCode != default(VirtualKeyCode))
                {
                    if ((!_alreadyPressed && singleStroke) || !singleStroke)
                    {
                        _simulator.Keyboard.KeyDown(keyCode);
                        _alreadyPressed = true;
                    }
                    //The following is not required. Just calling KeyDown makes keys that
                    //generate a character to produce a KeyPress event automatically!
                    //_simulator.Keyboard.KeyPress(keyCode);
                }
                
            }
            else
            {
                //Do nothing by now
            }

            _lastVirtualKeyCode = keyCode;

        }


    }
}
