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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace MPTCE.RealtimeConsumers
{
    public class MovDisplayViewModel : INotifyPropertyChanged
    {

        /// <summary>
        /// Event handler that fires whenever a property has been changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


        private MovDisplayConsumer _movDisplayConsumer;

        public MovDisplayConsumer movDisplayConsumer
        {
            get
            {
                return _movDisplayConsumer;
            }

            set
            {
                if (_movDisplayConsumer != value)
                {
                    if (_movDisplayConsumer != null)
                        _movDisplayConsumer.PropertyChanged -= movDisplayConsumer_PropertyChanged;

                    _movDisplayConsumer = value;
                    _movDisplayConsumer.PropertyChanged += movDisplayConsumer_PropertyChanged;

                    this.NotifyPropertyChanged("movDisplayConsumer");
                }
            }

        }

        public string movementName
        {
            get
            {
                if (_movDisplayConsumer != null)
                    return _movDisplayConsumer.movementName;
                else return "";
            }
        }

        public int movementCode
        {
            get
            {
                return _movDisplayConsumer.movementCode;
            }
        }


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
                //movementImage.Source = _movementBitmap;
                this.NotifyPropertyChanged("movementBitmap");
            }
        }


        private List<BitmapImage> _imageList;


        public MovDisplayViewModel()
        {
            this.PropertyChanged += MovDisplayViewModel_PropertyChanged;
            SetImages(PrepareImages(Properties.Settings.Default.AcqMovementsList.Count + Properties.Settings.Default.AcqAllowedMovements.Count));
        }


        void MovDisplayViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "movDisplayConsumer":
                    this.NotifyPropertyChanged("movementName");
                    this.NotifyPropertyChanged("movementCode");
                    //Change image
                    if(movDisplayConsumer!=null && movDisplayConsumer.movementCode>=0)
                    movementBitmap = _imageList.ElementAt(movDisplayConsumer.movementCode);

                    break;

                default:
                    break;
            }
        }


        private void movDisplayConsumer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "movementName":
                case "movementCode":
                    this.NotifyPropertyChanged(e.PropertyName);
                    //Change image
                    movementBitmap = _imageList.ElementAt(_movDisplayConsumer.movementCode);
                    break;

                default:
                    break;
            }

        }


        /// <summary>
        /// Composes a list of images that can be used for the RecordingPlan control to display 
        /// the appropriate image for each item in the schedule.
        /// </summary>
        public void SetImages(List<Bitmap> images)
        {
            _imageList = new List<BitmapImage>();

            foreach (Bitmap image in images) if (image != null) _imageList.Add(Convert(image));

        }


        /// <summary>
        /// Converts GDI+ type bitmap into a WPF type bitmap
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Composes a list of images that can be used for the RecordingPlan control to display 
        /// the appropriate image for each item in the schedule. If there is no image for a given movement, 
        /// a placeholder image is used.
        /// </summary>
        /// 
        public List<Bitmap> PrepareImages(int nMovs)
        {
            List<Bitmap> imageList = new List<Bitmap>();
            Bitmap tempBitmap;

            for (int i = 0; i < nMovs; i++)
            {
                tempBitmap = (Bitmap)Properties.Resources.ResourceManager.GetObject("m" + i);
                if (tempBitmap == null)
                    tempBitmap = (Bitmap)Properties.Resources.ResourceManager.GetObject("nofoto");

                //Now we have to convert this to a BitmapImage and add it to the list 

                imageList.Add(tempBitmap);
            }

            return imageList;
        }




    }
}
