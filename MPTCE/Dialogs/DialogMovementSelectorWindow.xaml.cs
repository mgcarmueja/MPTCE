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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using EMGFramework.ValueObjects;

namespace MPTCE.Dialogs
{
    /// <summary>
    /// Interaction logic for DialogMovementSelectorWindow.xaml
    /// </summary>
    public partial class DialogMovementSelectorWindow : Window
    {



        public IList SelectedItems
        {
            get
            {
                return movementsListBox.SelectedItems;
            }
        }

        
        public DialogMovementSelectorWindow(List<ScheduleItem> availableMovs)
        {
          
            
            InitializeComponent();

            foreach (ScheduleItem item in availableMovs) viewModel.availableMovements.Add(item); 
          
        }


           

        private void movementsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
