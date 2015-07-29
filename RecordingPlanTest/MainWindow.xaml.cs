/*
 * Copyright 2015 Martin Garcia Carmueja 
 * 
 *  This file is part of RecordingPlanTest.
 *
 *  RecordingPlanTest is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  RecordingPlanTest is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with RecordingPlanTest. If not, see <http://www.gnu.org/licenses/>.
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
using System.Drawing;
using RecordingPlan;
using System.IO;
using EMGFramework.Timer;



namespace RecordingPlanTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FastTimer myTimer;
        int progress, delta;
        //RecordingPlanViewModel myModel;  

        public MainWindow()
        {
            InitializeComponent();

            myRecordingPlan.planItemList.Add(new PlanItem(3.0, "rest",0));
            myRecordingPlan.planItemList.Add(new PlanItem(3.0, "close + flexion + pronation",20));
            myRecordingPlan.planItemList.Add(new PlanItem(3.0, "rest",0));
            myRecordingPlan.planItemList.Add(new PlanItem(3.0, "close + flexion + pronation",20));
            myRecordingPlan.planItemList.Add(new PlanItem(3.0, "rest",0));
            myRecordingPlan.planItemList.Add(new PlanItem(3.0, "close + flexion + pronation",20));
            myRecordingPlan.planItemList.Add(new PlanItem(3.0, "rest",0));
            myRecordingPlan.planItemList.Add(new PlanItem(3.0, "close + extension + pronation",24));
            myRecordingPlan.planItemList.Add(new PlanItem(3.0, "rest",0));
            myRecordingPlan.planItemList.Add(new PlanItem(3.0, "close + extension + pronation",24));
            myRecordingPlan.planItemList.Add(new PlanItem(3.0, "rest",0));
            myRecordingPlan.planItemList.Add(new PlanItem(3.0, "close + extension + pronation",24));


            myRecordingPlan.planItemList.ElementAt(3).completed = true;
            myRecordingPlan.planItemList.ElementAt(4).selected = true;
            
            



            

            List<BitmapImage> images = PrepareImages(27); //26 movements + rest
            myRecordingPlan.movementBitmap = images.ElementAt(1);

            myRecordingPlan.activeItem = 8;

            progress = 0;
            delta = 1;
            myTimer = new FastTimer(3, 33, MoveBar);
            myTimer.Start();

        }

        /// <summary>
        /// Sets the progress bar in the widget to a certain percentage
        /// </summary>
        private void MoveBar()
        {
            Dispatcher.InvokeAsync((Action)(() => { myRecordingPlan.movementProgress = progress; }));
            
            if(progress == 100) delta = -1;
            if(progress == 0) delta = 1;
            progress=progress+delta;
        }


        /// <summary>
        /// Composes a list of images that can be used for the RecordingPlan control to display 
        /// the appropriate image for each item in the schedule. If there is no image for a given movement, 
        /// a placeholder image is used. The number of total movements should either have to be read from the
        /// settings or, better passed as an argument
        /// </summary>
        public List<BitmapImage> PrepareImages(int nMovs)
        {
            List<BitmapImage> imageList = new List<BitmapImage>();
            Bitmap tempBitmap;


            for (int i = 0; i < nMovs; i++)
            {
                tempBitmap = (Bitmap)Properties.Resources.ResourceManager.GetObject("m"+i);
                if(tempBitmap == null)
                    tempBitmap = (Bitmap)Properties.Resources.ResourceManager.GetObject("nofoto");

                //Now we have to convert this to a BitmapImage and add it to the list 

                imageList.Add(Convert(tempBitmap));

            }

                return imageList;
        }


        private BitmapImage Convert(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();

            return image;

        }

    }
}
