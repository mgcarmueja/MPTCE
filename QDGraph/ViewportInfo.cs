/*
 * Copyright 2015 Martin Garcia Carmueja 
 * 
 *  This file is part of the Quick and Dirty Graphing System (QDGraph).
 *
 *  QDGraph is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  QDGraph is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with QDGraph.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace QDGraph
{

    public delegate Point CoordinateConverter(Point virtualPoint);


    /// <summary>
    /// Information about the Viewport used to draw a GraphMarker object on. It also contains a reference
    /// to the bitmap and the coordinate converter associated with it.
    /// </summary>
    public class ViewportInfo:INotifyPropertyChanged
    {

        /// <summary>
        /// Event handler that fires whenever a property in the objects has been changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


        private double _xMin;

        public double xMin 
        {
            get 
            {
                return _xMin;
            }

            set 
            {
                if (_xMin != value)
                {
                    _xMin = value;
                    this.NotifyPropertyChanged("xMin");
                }
            }
        }



        private double _xMax;

        public double xMax
        {
            get
            {
                return _xMax;
            }

            set
            {
                if (_xMax != value)
                {
                    _xMax = value;
                    this.NotifyPropertyChanged("xMax");
                }
            }
        }

        private double _yMin;

        public double yMin
        {
            get
            {
                return _yMin;
            }

            set
            {
                if (_yMin != value)
                {
                    _yMin = value;
                    this.NotifyPropertyChanged("yMin");
                }
            }
        }



        private double _yMax;

        public double yMax
        {
            get
            {
                return _yMax;
            }

            set
            {
                if (_yMax != value)
                {
                    _yMax = value;
                    this.NotifyPropertyChanged("yMax");
                }
            }
        }

        private WriteableBitmap _wBitmap;

        public WriteableBitmap wBitmap
        {
            get
            {
                return _wBitmap;
            }

            set 
            {
                if (_wBitmap != value)
                {
                    _wBitmap = value;
                    this.NotifyPropertyChanged("wBitmap");
                }
            }
        }


        private CoordinateConverter _converter;

        public CoordinateConverter converter
        {
            get 
            {
                return _converter;
            }

            set 
            {
                if (_converter != value)
                {
                    _converter = value;
                    this.NotifyPropertyChanged("converter");
                }
            }
        }


        public ViewportInfo(double xmin, double xmax, double ymin, double ymax, WriteableBitmap wbitmap, CoordinateConverter converter)
        {
            this.xMin = xmin;
            this.xMax = xmax;
            this.yMin = ymin;
            this.yMax = ymax;

            this.wBitmap = wbitmap;
            this.converter = converter;

        }


    }
}
