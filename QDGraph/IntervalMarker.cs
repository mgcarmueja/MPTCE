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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace QDGraph
{
    /// <summary>
    /// Class implementing an interval marker for intervals defined on the Y axis. 
    /// Originally, it is designed to look just like a rectangle, optionally with a certain amount of alpha blending.
    /// </summary>
    public class IntervalMarker:GraphMarker
    {

        private double _lowerLimit;
        /// <summary>
        /// Interval lower limit on the Y axis
        /// </summary>
        public double lowerLimit
        {
            get 
            {
                return _lowerLimit;
            }

            set
            {
                if (_lowerLimit != value)
                {
                    _lowerLimit = value;
                    NotifyPropertyChanged("lowerLimit");
                }
            }
        }

        private double _upperLimit;
        /// <summary>
        /// Interval upper limit on the Y axis
        /// </summary>
        public double upperLimit
        {
            get
            {
                return _upperLimit;
            }

            set
            {
                if(_upperLimit!=value)
                {
                    _upperLimit = value;
                    NotifyPropertyChanged("upperLimit");
                }
            }
        }


        public IntervalMarker(ViewportInfo info,double lowerLimit, double upperLimit)
            : base(info)
        {
            this.lowerLimit = lowerLimit;
            this.upperLimit = upperLimit;
        }


        public IntervalMarker()
            : base()
        {
            this.lowerLimit = lowerLimit;
            this.upperLimit = upperLimit;
        }


        public override void Draw()
        {
            Point startPoint = viewportInfo.converter(new Point(viewportInfo.xMin, lowerLimit));
            Point endPoint = viewportInfo.converter(new Point(viewportInfo.xMax, upperLimit));

            double temp;

            if (startPoint.X > endPoint.X)
            {
                temp = startPoint.X;
                startPoint.X = endPoint.X;
                endPoint.X = temp;
            }

            if (startPoint.Y > endPoint.Y)
            {
                temp = startPoint.Y;
                startPoint.Y = endPoint.Y;
                endPoint.Y = temp;
            }

            FillRectangleWithBlending(viewportInfo.wBitmap
                                    , (int)startPoint.X
                                    , (int)startPoint.Y
                                    , (int)endPoint.X
                                    , (int)endPoint.Y
                                    , color);
          
        }

    }
}
