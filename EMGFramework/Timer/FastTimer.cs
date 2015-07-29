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
using System.Threading;
using System.Runtime.InteropServices;


namespace EMGFramework.Timer
{
    /// <summary>
    /// Implements a timer working in one of the following modes:
    /// 
    /// 0 = System.Threading (default)
    /// 1 = Windows.Forms
    /// 3 = Multimedia timer (windows mm dll).
    /// </summary>
    public class FastTimer
    {
        private int _timerType;
        private System.Threading.Timer _timer0;
        private System.Windows.Forms.Timer _timer1;
        private int _interval;

        private ElapsedTimer0Delegate _elapsedTimer0Handler;
        private ElapsedTimer1Delegate _elapsedTimer1Handler;
        private ElapsedTimerDelegate _elapsedTimerHandler;


        /// <summary>
        /// Initlaizes a FastTimer object
        /// </summary>
        /// <param name="timerType">0 = System.Threading (default), 1 = Windows.Forms, 3 = Multimedia timer (windows mm dll)</param>
        /// <param name="intervalMS">Time interval between calls</param>
        /// <param name="callback">Method to be called each time intervalMS elapses</param>
        public FastTimer(int timerType, int intervalMS, ElapsedTimerDelegate callback)
        {
            _timerType = timerType;
            _interval = intervalMS;
            _elapsedTimerHandler = callback;

            switch (timerType)
            {
                case 0:
                    _elapsedTimer0Handler = Timer0Handler;
                    break;
                case 1:
                    _elapsedTimer1Handler = Timer1Handler;
                    _timer1 = new System.Windows.Forms.Timer();
                    _timer1.Interval = _interval;
                    _timer1.Tick += Timer1Handler;
                    break;
                case 3:

                    break;
                default:
                    throw (new NotImplementedException());

            }
        }

        public delegate void ElapsedTimerDelegate();

        public delegate void ElapsedTimer0Delegate(object sender);

        public delegate void ElapsedTimer1Delegate(object sender, EventArgs e);

        public delegate void ElapsedTimer3Delegate(int tick, TimeSpan span);

        public void Timer0Handler(object sender)
        {
            _elapsedTimerHandler();
        }

        public void Timer1Handler(object sender, EventArgs e)
        {
            _elapsedTimerHandler();
        }


        private void Timer3Handler(int id, int msg, IntPtr user, int dw1, int dw2)
        {
            _elapsedTimerHandler();
        }


        public void Start()
        {
            switch (_timerType)
            {
                case 0:
                    _timer0 = new System.Threading.Timer((new TimerCallback(_elapsedTimer0Handler)), null, 0, _interval);
                    break;
                case 1:
                    _timer1.Start();
                    break;
                case 3:
                    timeBeginPeriod(1);
                    mHandler = new TimerEventHandler(Timer3Handler);
                    mTimerId = timeSetEvent(_interval, 0, mHandler, IntPtr.Zero, EVENT_TYPE);
                    mTestStart = DateTime.Now;
                    break;
                default:
                    throw (new NotImplementedException());
            }
        }


        public void Stop()
        {
            switch (_timerType)
            {
                case 0:
                    _timer0.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    break;
                case 1:
                    _timer1.Stop();
                    break;
                case 3:
                    int err = timeKillEvent(mTimerId);
                    timeEndPeriod(1);
                    mTimerId = 0;
                    break;
                default:
                    throw (new NotImplementedException());
            }
        }

        private int mTimerId;
        private TimerEventHandler mHandler;
        private DateTime mTestStart;

        // P/Invoke declarations for the multimedia timer
        private delegate void TimerEventHandler(int id, int msg, IntPtr user, int dw1, int dw2);

        private const int TIME_PERIODIC = 1;
        private const int EVENT_TYPE = TIME_PERIODIC;// + 0x100;  // TIME_KILL_SYNCHRONOUS causes a hang ?!
        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution,
            TimerEventHandler handler, IntPtr user, int eventType);
        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);
        [DllImport("winmm.dll")]
        private static extern int timeBeginPeriod(int msec);
        [DllImport("winmm.dll")]
        private static extern int timeEndPeriod(int msec);

    }
}

