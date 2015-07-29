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
using System.Collections.ObjectModel;
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

namespace QDGraph
{

    /*This enumeration defines the available plotting modes for the Graph class.
     * 
     * staticDisplay: The default. It plots the data that are visible using an absolute coordinate system
     *          and ignores points that fall outside of the viewport coordinates.
     *          
     * movingWindow: Intended for realtime data, it displays the data with the highest x axis value up to a given
     *               window size and moves the window forward when new data whose x axis value falls outside
     *               of the window arrive.
     */
    public enum PlotMode : int { staticDisplay, movingViewPort };


    /// <summary>
    /// Interaction logic for Graph.xaml
    /// </summary>
    //[ProvideGraph("QDGraph", true)]

    public partial class Graph : UserControl
    {

        private WriteableBitmap _wBitmap;
        private int _bitmapWidth = 1;
        private int _bitmapHeight = 1;


        private double _xCoeff, _yCoeff;

        private bool _useLines = false; //This is not used yet...


        private ObservableCollection<GraphMarker> _graphMarkers;
        public ObservableCollection<GraphMarker> graphMarkers
        {
            get
            {
                return _graphMarkers;
            }
        }


        private PlotMode _plotMode = PlotMode.staticDisplay;
        /// <summary>
        /// Plotting mode for the graph. This determines its behaviour when adding new points.
        /// See description of enum PlotMode in Graph.xaml.cs
        /// </summary>
        public PlotMode plotMode
        {
            get
            {
                return _plotMode;
            }

            set
            {
                if (_plotMode != value)
                {
                    _plotMode = value;
                    this.NotifyPropertyChanged("plotMode");
                }
            }
        }


        private double _viewPortStride = 50;
        /// <summary>
        /// When the Graph is operating in window mode, it defines the length of the window jump when moving the scope
        /// to display new data. That is, how far the windows moves to the "right". This is given in units of the x axis.
        /// </summary>
        public double viewPortStride
        {
            get
            {
                return _viewPortStride;
            }

            set
            {
                if (_viewPortStride != value)
                {
                    _viewPortStride = value;
                    this.NotifyPropertyChanged("viewPortStride");
                }
            }
        }


        private double _xMin = 0;
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


        private double _xMax = 100;
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


        private double _yMin = -1;
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


        private double _yMax = 1;
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


        private Color _plotColor = Colors.Blue;
        public Color plotColor
        {
            get
            {
                return _plotColor;
            }
            set
            {
                if (_plotColor != value)
                {
                    _plotColor = value;
                    this.NotifyPropertyChanged("plotColor");
                }
            }
        }


        private Color _axisColor = Colors.Red;

        public Color axisColor
        {
            get
            {
                return _axisColor;
            }

            set
            {
                if (_axisColor != value)
                {
                    _axisColor = value;
                    this.NotifyPropertyChanged("axisColor");
                }
            }
        }



        private int _topEdge = 5;
        public int topEdge
        {
            get
            {
                return _topEdge;
            }
            set
            {
                if (_topEdge != value)
                {
                    _topEdge = value;
                    this.NotifyPropertyChanged("topEdge");
                }
            }
        }


        private int _bottomEdge = 5;
        public int bottomEdge
        {
            get
            {
                return _bottomEdge;
            }
            set
            {
                if (_bottomEdge != value)
                {
                    _bottomEdge = value;
                    this.NotifyPropertyChanged("bottomEdge");
                }
            }
        }


        private int _leftEdge = 5;
        public int leftEdge
        {
            get
            {
                return _leftEdge;
            }
            set
            {
                if (_leftEdge != value)
                {
                    _leftEdge = value;
                    this.NotifyPropertyChanged("leftEdge");
                }
            }
        }


        private int _rightEdge = 5;
        public int rightEdge
        {
            get
            {
                return _rightEdge;
            }
            set
            {
                if (_rightEdge != value)
                {
                    _rightEdge = value;
                    this.NotifyPropertyChanged("rightEdge");
                }
            }
        }


        private string _topTitle;
        public string topTitle
        {
            get
            {
                return _topTitle;
            }
            set
            {
                if (_topTitle != value)
                {
                    _topTitle = value;
                    this.NotifyPropertyChanged("topTitle");
                }
            }
        }


        private string _bottomTitle;
        public string bottomTitle
        {
            get
            {
                return _bottomTitle;
            }
            set
            {
                if (_bottomTitle != value)
                {
                    _bottomTitle = value;
                    this.NotifyPropertyChanged("bottomTitle");
                }
            }
        }


        private string _leftTitle;
        public string leftTitle
        {
            get
            {
                return _leftTitle;
            }
            set
            {
                if (_leftTitle != value)
                {
                    _leftTitle = value;
                    this.NotifyPropertyChanged("leftTitle");
                }
            }
        }



        private string _rightTitle;
        public string rightTitle
        {
            get
            {
                return _rightTitle;
            }
            set
            {
                if (_rightTitle != value)
                {
                    _rightTitle = value;
                    this.NotifyPropertyChanged("rightTitle");
                }
            }
        }


        private string _toggleTitle;
        public string toggleTitle
        {
            get
            {
                return _toggleTitle;
            }

            set
            {
                if (_toggleTitle != value)
                {
                    _toggleTitle = value;
                    this.NotifyPropertyChanged("toggleTitle");
                }
            }
        }



        private LinkedList<Point> _cachedPoints;
        public LinkedList<Point> cachedPoints
        {
            get
            {
                return _cachedPoints;
            }
        }

        private Label _topLabel;
        private Label _bottomLabel;
        private Label _leftLabel;
        private Label _rightLabel;

        private CheckBox _toggleCheckBox;


        private bool _enable = true;
        public bool enable
        {
            get
            {
                return _enable;
            }
            set
            {
                if (_enable != value)
                {
                    _enable = value;
                    this.NotifyPropertyChanged("enable");
                }
            }
        }


        private bool _isToggleable;
        public bool isToggleable
        {
            get
            {
                return _isToggleable;
            }

            set
            {
                if (_isToggleable != value)
                {
                    _isToggleable = value;
                    this.NotifyPropertyChanged("isToggleable");
                }
            }
        }



        private bool _toggle;

        public bool toggle
        {
            get
            {
                return _toggle;
            }

            set
            {
                if (_toggle != value)
                {
                    _toggle = value;
                    this.NotifyPropertyChanged("toggle");
                }
            }
        }


        private ViewportInfo _viewportInfo;


        /// <summary>
        /// Event handler that fires whenever a property in the objects has been changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


        void _graphMarkers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            foreach (GraphMarker item in graphMarkers)
            {
                if (item.viewportInfo != _viewportInfo) item.viewportInfo = _viewportInfo;

                item.PropertyChanged -= GraphMarker_PropertyChanged;
                item.PropertyChanged += GraphMarker_PropertyChanged;
            }

            Redraw();
        }

        private void GraphMarker_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Redraw();
        }


        private void Preinit()
        {
            _toggleCheckBox = new CheckBox();
            _toggleCheckBox.HorizontalAlignment = HorizontalAlignment.Center;
            _toggleCheckBox.VerticalAlignment = VerticalAlignment.Center;
            Binding checkBoxBinding = new Binding("toggle");
            checkBoxBinding.Source = this;
            checkBoxBinding.Mode = BindingMode.TwoWay;
            checkBoxBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            _toggleCheckBox.SetBinding(CheckBox.IsCheckedProperty, checkBoxBinding);

            _topLabel = new Label();
            _topLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _topLabel.VerticalAlignment = VerticalAlignment.Center;

            _bottomLabel = new Label();
            _bottomLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _bottomLabel.VerticalAlignment = VerticalAlignment.Center;

            _leftLabel = new Label();
            _leftLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _leftLabel.VerticalAlignment = VerticalAlignment.Center;

            _rightLabel = new Label();
            _rightLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _rightLabel.VerticalAlignment = VerticalAlignment.Center;

            _graphMarkers = new ObservableCollection<GraphMarker>();
            _graphMarkers.CollectionChanged += _graphMarkers_CollectionChanged;
        }



        public Graph()
        {

            Preinit();

            InitializeComponent();



            imageGrid.SizeChanged += ImageGrid_SizeChanged;
            this.PropertyChanged += Graph_PropertyChanged;

            _wBitmap = BitmapFactory.New(_bitmapWidth, _bitmapHeight);

            _cachedPoints = new LinkedList<Point>();

            image.Source = _wBitmap;

            _lastPoint = new Point(Double.MinValue, Double.MinValue);
            _lastX = int.MinValue;
            _scanColumn = new bool[_bitmapHeight];

            _viewportInfo = new ViewportInfo(xMin, xMax, yMin, yMax, _wBitmap, ToBitmap);

        }


        /// <summary>
        /// Checks if a given point falls into the WriteableBitmap bounds
        /// </summary>
        /// <param name="X"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool CheckVisible(Point physicalPoint)
        {
            return (((int)physicalPoint.X >= _leftEdge)
                && ((int)physicalPoint.Y >= _topEdge)
                && ((int)physicalPoint.X < (_bitmapWidth - _rightEdge))
                && ((int)physicalPoint.Y < (_bitmapHeight - bottomEdge)));
        }


        /// <summary>
        /// Calculates the translation coefficients between bitmap and graph coordinates
        /// </summary>
        private void CalculateCoefficients()
        {
            _xCoeff = (_bitmapWidth - 1 - _leftEdge - _rightEdge) / (_xMax - _xMin);
            _yCoeff = (_bitmapHeight - 1 - _bottomEdge - _topEdge) / (_yMax - _yMin);
        }


        private void Graph_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case "toggle":
                    _toggleCheckBox.IsChecked = toggle;
                    break;

                case "isToggleable":
                    toggleGrid.Children.Clear();
                    if (isToggleable)
                    {
                        _toggleCheckBox.Content = toggleTitle;
                        _toggleCheckBox.IsChecked = toggle;
                        toggleGrid.Children.Add(_toggleCheckBox);
                    }
                    break;

                case "topTitle":
                    topTitleGrid.Children.Clear();
                    if (topTitle != null && topTitle != "")
                    {
                        _topLabel.Content = topTitle;
                        topTitleGrid.Children.Add(_topLabel);
                    }
                    break;

                case "bottomTitle":
                    bottomTitleGrid.Children.Clear();
                    if (bottomTitle != null && bottomTitle != "")
                    {
                        _bottomLabel.Content = bottomTitle;
                        bottomTitleGrid.Children.Add(_bottomLabel);
                    }
                    break;

                case "leftTitle":
                    leftTitleGrid.Children.Clear();
                    if (leftTitle != null && leftTitle != "")
                    {
                        _leftLabel.Content = leftTitle;
                        leftTitleGrid.Children.Add(_leftLabel);
                    }
                    break;

                case "rightTitle":
                    rightTitleGrid.Children.Clear();
                    if (rightTitle != null && rightTitle != "")
                    {
                        _rightLabel.Content = rightTitle;
                        rightTitleGrid.Children.Add(_rightLabel);
                    }
                    break;

                case "xMin":
                    _viewportInfo.xMin = xMin;
                    break;

                case "xMax":
                    _viewportInfo.xMax = xMax;
                    break;

                case "yMin":
                    _viewportInfo.yMin = yMin;
                    break;

                case "yMax":
                    _viewportInfo.yMax = yMax;
                    break;

                default:
                    break;
            }

            CalculateCoefficients();

            Redraw();
        }


        /// <summary>
        /// Each time the ImageGrid is resized, we redraw the control by generating a new bitmap to suit the
        /// new size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageGrid_SizeChanged(object sender, RoutedEventArgs e)
        {
            _bitmapWidth = (int)imageGrid.ActualWidth;
            _bitmapHeight = (int)imageGrid.ActualHeight;

            _scanColumn = new bool[_bitmapHeight];

            _wBitmap = BitmapFactory.New(_bitmapWidth, _bitmapHeight);
            _viewportInfo.wBitmap = _wBitmap;

            image.Source = _wBitmap;

            CalculateCoefficients();

            Redraw();
        }



        /// <summary>
        /// It performs a complete redraw of the graph.
        /// </summary>
        public void Redraw()
        {

            Dispatcher.BeginInvoke((Action)(() =>
            {
                DrawBackground();
                PlotList(_cachedPoints);

                if (enable)
                    foreach (GraphMarker item in _graphMarkers)
                    {
                        if (item.visible)
                            item.Draw();
                    }
            }
            ));

        }


        /// <summary>
        /// Converts virtual graph coordinates to bitmap coordinates
        /// </summary>
        /// <param name="virtualPoint"></param>
        /// <returns></returns>
        Point ToBitmap(Point virtualPoint)
        {
            Point bitmapPoint = new Point();

            bitmapPoint.X = _leftEdge + (int)((virtualPoint.X - _xMin) * _xCoeff);
            bitmapPoint.Y = _bitmapHeight - 1 - (_bottomEdge + (int)((virtualPoint.Y - _yMin) * _yCoeff));

            return bitmapPoint;
        }


        /// <summary>
        /// Draws the background and clears the point list 
        /// </summary>
        public void Clear()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                DrawBackground();
                _cachedPoints.Clear();
            }
            ));

        }



        /// <summary>
        /// Draws a background and a line in the middle if the bitmap.
        /// Used as background for our graphs :D
        /// </summary>
        private void DrawBackground()
        {
            //Coordinates conversion for the Zero line
            Point lineStart = ToBitmap(new Point(_xMin, 0.0));
            Point lineEnd = ToBitmap(new Point(_xMax, 0.0));

            Color drawColor;

            if (enable) drawColor = axisColor;
            else drawColor = Colors.LightGray;

            //Bitmap is refreshed after the using block ends
            using (_wBitmap.GetBitmapContext())
            {
                _wBitmap.Clear(Colors.White);

                if (CheckVisible(lineStart) && CheckVisible(lineEnd))

                    _wBitmap.DrawLine((int)lineStart.X, (int)lineStart.Y, (int)lineEnd.X, (int)lineEnd.Y, drawColor);
            }
        }


        /// <summary>
        /// Plots a point by using the virtual coordinate system defined by _xMin, _XMax, _yMin, _yMax
        /// </summary>
        /// <param name="newPoint"></param>
        public void Plot(Point point, Color color)
        {
            Point realPoint = ToBitmap(point);

            if (CheckVisible(realPoint))
            {

                using (_wBitmap.GetBitmapContext())
                {
                    _wBitmap.SetPixel((int)realPoint.X, (int)realPoint.Y, color);
                }
            }
        }


        private Point _lastPoint;
        private int _lastX;
        private bool[] _scanColumn;

        /// <summary>
        /// Plots point list using an optimized method :)
        /// </summary>
        public void PlotList(LinkedList<Point> points)
        {
            if (points.Count == 0) return;

            Color drawColor;
            if (enable) drawColor = plotColor;
            else drawColor = Colors.Gray;

            if (plotMode == PlotMode.movingViewPort && points != _cachedPoints)
            {
                //Checking if the viewport needs to be moved, and if so, move it and redraw

                Point lastPoint = points.Last();
                if (lastPoint.X > _xMax)
                {

                    while (lastPoint.X > _xMax)
                    {
                        _xMin += _viewPortStride;
                        _viewportInfo.xMin = _xMin;
                        _xMax += _viewPortStride;
                        _viewportInfo.xMax = _xMax;
                    }



                    while (_cachedPoints.First().X < xMin)
                        _cachedPoints.RemoveFirst();


                    Redraw();

                }
            }


            using (var context = _wBitmap.GetBitmapContext())
            {

                foreach (Point point in points)
                {


                    Point realPoint = ToBitmap(point);


                    if (CheckVisible(realPoint) && (!realPoint.Equals(_lastPoint)))
                    {

                        if ((int)realPoint.X != _lastX)
                        {
                            _lastX = (int)realPoint.X;
                            for (int i = 0; i < _scanColumn.Length; i++) _scanColumn[i] = false;
                        }

                        if (!_scanColumn[(int)realPoint.Y])
                        {
                            unsafe
                            {
                                int temp = context.Pixels[(int)realPoint.Y * context.Width + (int)realPoint.X];
                                byte tempR = (byte)((0x00FF0000 & temp) >> 16);
                                byte tempG = (byte)((0x0000FF00 & temp) >> 8);
                                byte tempB = (byte)(0x000000FF & temp);
                                byte tempA = (byte)((0xFF000000 & temp) >> 24);
                                byte Ainv = (byte)(255 - drawColor.A);


                                context.Pixels[(int)realPoint.Y * context.Width + (int)realPoint.X] =
                                      (0xFF << 24)
                                    | ((((drawColor.R * drawColor.A) + (tempR * Ainv)) >> 8) << 16)
                                    | ((((drawColor.G * drawColor.A) + (tempG * Ainv)) >> 8) << 8)
                                    | (((drawColor.B * drawColor.A) + (tempB * Ainv)) >> 8);
                            }

                            _scanColumn[(int)realPoint.Y] = true;
                            _lastPoint = realPoint;
                        }
                    }
                }
            }
        }


    }
}
