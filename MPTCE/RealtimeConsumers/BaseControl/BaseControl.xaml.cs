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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MPTCE.RealtimeConsumers
{
    /// <summary>
    /// Interaction logic for TestControl.xaml
    /// </summary>
    public partial class BaseControl : UserControl
    {


        public BaseViewModel viewModel
        {
            get
            {
                return _baseViewModel;
            }
        }


        public RealtimeConsumer realtimeConsumer
        {
            get
            {
                return _baseViewModel.realtimeConsumer;
            }

            set
            {
                if (_baseViewModel.realtimeConsumer != value)
                    _baseViewModel.realtimeConsumer = value;
            }
        }



        public Grid itemsGrid
        {
            get
            {
                return _itemsGrid;
            }
        }


        public BaseControl()
        {
            InitializeComponent();
            _baseViewModel.PropertyChanged += _baseViewModel_PropertyChanged;
        }


        private void SetStartStopButton(string text)
        {
            startStopButton.Content = text;
        }


        void _baseViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "running":
                    if (_baseViewModel.running)
                    {
                        Dispatcher.Invoke(new Action<string>(SetStartStopButton), "Stop");
                    }

                    else
                    {
                        Dispatcher.Invoke(new Action<string>(SetStartStopButton), "Start");
                    }

                    break;
                    /*
                case "realtimeConsumer":
                    itemsGrid.Children.Clear();
                    itemsGrid.Children.Add(realtimeConsumer.consumerControl);
                    break;
                    */
                default:
                    break;
            }
        }


        private void startStopButton_Click(object sender, RoutedEventArgs e)
        {
            _baseViewModel.StartStop();
        }
    }
}
