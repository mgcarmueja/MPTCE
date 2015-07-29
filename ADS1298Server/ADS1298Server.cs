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
using System.ServiceModel.Description;
using System.Diagnostics;
using EPdevice;
using ADS1298Intercom;
using System.Drawing;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Interop;






namespace ADS1298Server
{

    public class HiddenWindow : Window
    {

        private HwndSource source;
        private HwndSourceHook hook;
        private IntPtr hwnd;

        private EPdeviceManager _EPdevices;


		public HiddenWindow(EPdeviceManager deviceManager)
		{

            _EPdevices = deviceManager;

            hwnd = new WindowInteropHelper(this).EnsureHandle(); //.Handle;
            source = HwndSource.FromHwnd(hwnd);
            if (source != null)
            {
                hook = new HwndSourceHook(WndProc);
                source.AddHook(hook);
            }

            _EPdevices.Initialize(hwnd);

            this.Visibility = Visibility.Hidden;
		
		}

        /*
        ~HiddenWindow()
        {

        }
        */

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

        }

        /// <summary>
        /// Creating an equivalent to WndProc from Froms.
        /// </summary>
        /// <param name="hwnd"> window handle.</param>
        /// <param name="msg"> message ID. </param>
        /// <param name="wParam"> message's wParam value. </param>
        /// <param name="lParam"> message's lParam value. </param>
        /// <param name="handled"> value that indicates whether the message was handled. </param>
        /// <returns> </returns>
        internal IntPtr WndProc(IntPtr hwnd, Int32 Msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            try
            {
                // The OnDeviceChange routine processes WM_DEVICECHANGE messages.
                if (Msg == _EPdevices.Get_WM_DEVICECHANGE())
                {
                    Debug.WriteLine("WndProc - Device change -> message code {0}", Msg);
                    Debug.WriteLine("Number of devices: {0}", EMGSingleton.Instance.EPdevices.Count);
                    _EPdevices.OnDeviceChange(this.hwnd, Msg, wParam, lParam);
                    handled = true;
                }
            }
            catch (NullReferenceException)
            {
                _EPdevices.Close();
                //_EPdevices = null;
                //_EPdevices = new EPdeviceManager();
                _EPdevices.Initialize(this.hwnd);
            }
            catch (IndexOutOfRangeException ex)
            {
                Debug.WriteLine("Exception: {0}",ex.Message);
                //throw;

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: {0}", ex.Message);
                //throw;
            }

            return IntPtr.Zero;
        }

    }
    


    class Program
    {
     
        [STAThread]
        static void Main(string[] args)
        {
            Debug.WriteLine("Number of devices: {0}",EMGSingleton.Instance.EPdevices.Count);

            using (ServiceHost host = new ServiceHost(typeof(EMGDevice),
                                      new Uri[] { new Uri("net.pipe://localhost") }))
            {

                //Adding a behavior to include exception details in the FaultExceptions received
                //on the client side whenever an exception on the server side occurs.
                //for security reasons, THIS SHOULD BE DISABLED ON A PRODUCTION VERSION!!

                ServiceDebugBehavior debug = host.Description.Behaviors.Find<ServiceDebugBehavior>();

                if (debug == null)
                {
                    host.Description.Behaviors.Add(
                         new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
                }
                else
                {
                    // make sure setting is turned ON
                    if (!debug.IncludeExceptionDetailInFaults)
                    {
                        debug.IncludeExceptionDetailInFaults = true;
                    }
                }

                //Finished adding behavior


                System.Windows.Application myApp = new System.Windows.Application();
                HiddenWindow myWindow = new HiddenWindow(EMGSingleton.Instance); 

                host.AddServiceEndpoint(typeof(IEMGDevice), new NetNamedPipeBinding(), "PipeEMGServer");

               

                host.Open();
                
                Console.WriteLine("Service is available");
                myApp.Run(myWindow);

                host.Close();
            }
        }
    }
}
