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
 *  along with MPTCE. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Drawing;


namespace RecordingPlan
{
    /// <summary>
    /// Interaction logic for ToolboxControl.xaml
    /// </summary>
    //[ProvideRecordingPlan("RecordingPlan", true)]
    public partial class RecordingPlan : UserControl
    {
        private BitmapImage _movementBitmap;

        public BitmapImage movementBitmap
        {
            get
            {
                return _movementBitmap;

            }
            set
            {
                _movementBitmap = value;
                movementImage.Source = _movementBitmap;
            }
        }

        private int _activeItem;


        public int activeItem
        {
            get { return _activeItem; }
            set
            {
                if ((_activeItem != value) && (value >= 0) && (value < recordingDataGrid.Items.Count))
                {
                    _activeItem = value;
                    recordingDataGrid.ScrollIntoView(recordingDataGrid.Items[value]);
                }
            }
        }



        private BindingList<PlanItem> _planItemList;

        public BindingList<PlanItem> planItemList
        {
            get
            {
                return _planItemList;
            }
        }

        public int movementProgress
        {
            set
            {
                if ((value >= 0) && (value <= 100))
                    movementProgressBar.Value = value;
            }
        }


        public RecordingPlan()
        {

            InitializeComponent();

            _planItemList = new BindingList<PlanItem>();
            recordingDataGrid.ItemsSource = _planItemList;
            recordingDataGrid.SelectionChanged += recordingDataGrid_SelectionChanged;

        }

        void recordingDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)sender;
            //dataGrid.UnselectAllCells();
        }

    }
}
