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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QDGraph
{

    public abstract class GraphMarker:INotifyPropertyChanged
    {

        /// <summary>
        /// Event delegate that is called whenever an observed property changes. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }



        public ViewportInfo viewportInfo;


        private bool _visible;
        /// <summary>
        /// True if the GraphMarker should be drawn, false otherwise
        /// </summary>
        public bool visible
        {
            get 
            {
                return _visible;
            }

            set 
            { 
                if(_visible!=value)
                {
                    _visible = value;
                    this.NotifyPropertyChanged("visible");
                }
            }
        }


        private Color _color;

        public Color color
        {
            get
            {
                return _color;
            }

            set
            {
                if (_color != value)
                    _color = value;
                this.NotifyPropertyChanged("color");
            }
        }


        private int _visualIndex;
        /// <summary>
        /// Determines the order in which GraphMarker objecs are drawn. The reference value is 0 for the 
        /// plot. GraphMarker onjects with negative indexes will be drawn BEFORE the plot itself. 
        /// Those with a positive index will be drawn AFTER it.
        /// </summary>
        public int visualIndex
        {
            get
            {
                return _visualIndex;
            }

            set 
            {
                if (_visualIndex != value)
                {
                    _visualIndex = value;
                    this.NotifyPropertyChanged("visualIndex");
                }
            }

        }


        public GraphMarker(ViewportInfo info)
        {
            this.viewportInfo = info;
        }

        public GraphMarker()
        {
        }

        public abstract void Draw();

        /// <summary>
        /// Draws a filled rectangle onto a WritableBitmap with alpha blending.
        /// Adapted form: https://writeablebitmapex.codeplex.com/discussions/258703
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="color"></param>
        protected static void FillRectangleWithBlending(WriteableBitmap bmp, int x1, int y1, int x2, int y2, Color color)
        {
            // Use refs for faster access (really important!) speeds up a lot!
            int w = bmp.PixelWidth;
            int h = bmp.PixelHeight;

            using (var context = bmp.GetBitmapContext())
            {
                

                // Check boundaries
                if (x1 < 0) { x1 = 0; }
                if (y1 < 0) { y1 = 0; }
                if (x2 < 0) { x2 = 0; }
                if (y2 < 0) { y2 = 0; }
                if (x1 >= w) { x1 = w - 1; }
                if (y1 >= h) { y1 = h - 1; }
                if (x2 >= w) { x2 = w - 1; }
                if (y2 >= h) { y2 = h - 1; }

                unsafe
                {
                    int* pixels = context.Pixels;

                    for (int y = y1; y <= y2; y++)
                    {
                        for (int i = y * w + x1; i <= y * w + x2; i++)
                        {
                            byte oneOverAlpha = (byte)(255 - color.A);
                            int c = pixels[i];

                            int r = ((byte)(c >> 16) * oneOverAlpha + color.R * color.A) >> 8;
                            int g = ((byte)(c >> 8) * oneOverAlpha + color.G * color.A) >> 8;
                            int b = ((byte)(c >> 0) * oneOverAlpha + color.B * color.A) >> 8;

                            pixels[i] = 255 << 24 | r << 16 | g << 8 | b;
                        }
                    }
                }
            }
        }


    }
}
