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
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace QDGraph
{
    public class GraphModel : INotifyPropertyChanged
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


        private Graph _graph;
        private BlockingCollection<Point> _pointQueue;
        private LinkedList<Point> _pointList;


        private ObservableCollection<GraphMarker> _graphMarkers;
        public ObservableCollection<GraphMarker> graphMarkers
        {
            get
            {
                if (_graph != null)
                    return _graph.graphMarkers;
                else return _graphMarkers;
            }
        }


        private double _viewPortStride;
        public double viewPortStride
        {
            get
            {
                if (_graph != null)
                    return _graph.viewPortStride;
                return _viewPortStride;
            }

            set
            {
                if (_graph != null && _graph.viewPortStride != value)
                    _graph.viewPortStride = value;

                _viewPortStride = value;
            }
        }


        private PlotMode _plotMode;
        public PlotMode plotMode
        {
            get
            {
                if (_graph != null)
                    return _graph.plotMode;
                return _plotMode;
            }

            set
            {
                if (graph != null && _graph.plotMode != value)
                    _graph.plotMode = value;

                _plotMode = value;
            }

        }


        private double _xMin;
        public double xMin
        {
            get
            {
                if (_graph != null)
                    return _graph.xMin;
                return _xMin;
            }
            set
            {
                if (_graph != null && _graph.xMin != value)
                    _graph.xMin = value;

                _xMin = value;
            }
        }


        private double _xMax;
        public double xMax
        {
            get
            {
                if (_graph != null)
                    return _graph.xMax;
                return _xMax;
            }
            set
            {
                if (_graph != null && _graph.xMax != value)
                    _graph.xMax = value;

                _xMax = value;
            }
        }


        private double _yMin;
        public double yMin
        {
            get
            {
                if (_graph != null)
                    return _graph.yMin;
                return _yMin;
            }
            set
            {
                if (_graph != null && _graph.yMin != value)
                    _graph.yMin = value;

                _yMin = value;
            }
        }


        private double _yMax;
        public double yMax
        {
            get
            {
                if (_graph != null)
                    return _graph.yMax;
                return _yMax;
            }
            set
            {
                if (_graph != null && _graph.yMax != value)
                    _graph.yMax = value;

                _yMax = value;
            }
        }


        private int _topEdge = 5;
        public int topEdge
        {
            get
            {
                if (_graph != null)
                    return _graph.topEdge;
                return _topEdge;
            }
            set
            {
                if (_graph != null && _graph.topEdge != value)
                    _graph.topEdge = value;

                _topEdge = value;
            }
        }


        private int _bottomEdge = 5;
        public int bottomEdge
        {
            get
            {
                if (_graph != null)
                    return _graph.bottomEdge;
                return _bottomEdge;
            }
            set
            {
                if (_graph != null && _graph.bottomEdge != value)
                    _graph.bottomEdge = value;

                _bottomEdge = value;
            }
        }


        private int _leftEdge = 5;
        public int leftEdge
        {
            get
            {
                if (_graph != null)
                    return _graph.leftEdge;
                return _leftEdge;
            }
            set
            {
                if (_graph != null && _graph.leftEdge != value)
                    _graph.leftEdge = value;

                _leftEdge = value;
            }
        }


        private int _rightEdge = 5;
        public int rightEdge
        {
            get
            {
                if (_graph != null)
                    return _graph.rightEdge;
                return _rightEdge;
            }
            set
            {
                if (_graph != null && _graph.rightEdge != value)
                    _graph.rightEdge = value;

                _rightEdge = value;
            }
        }


        private string _topTitle;
        public string topTitle
        {
            get
            {
                if (_graph != null)
                    return _graph.topTitle;
                return _topTitle;
            }

            set
            {
                if (_graph != null && _graph.topTitle != value)
                    _graph.topTitle = value;

                _topTitle = value;

            }
        }


        private string _bottomTitle;
        public string bottomTitle
        {
            get
            {
                if (_graph != null)
                    return _graph.bottomTitle;
                return _bottomTitle;
            }

            set
            {
                if (_graph != null && _graph.bottomTitle != value)
                    _graph.bottomTitle = value;

                _bottomTitle = value;

            }
        }


        private string _leftTitle;
        public string leftTitle
        {
            get
            {
                if (_graph != null)
                    return _graph.leftTitle;
                return _leftTitle;
            }

            set
            {
                if (_graph != null && _graph.leftTitle != value)
                    _graph.leftTitle = value;

                _leftTitle = value;

            }
        }


        private string _rightTitle;
        public string rightTitle
        {
            get
            {
                if (_graph != null)
                    return _graph.rightTitle;
                return _rightTitle;
            }

            set
            {
                if (_graph != null && _graph.rightTitle != value)
                    _graph.rightTitle = value;

                _rightTitle = value;

            }
        }


        private string _toggleTitle;
        public string toggleTitle
        {
            get
            {
                if (_graph != null)
                    return _graph.toggleTitle;
                return _toggleTitle;
            }

            set
            {
                if (_graph != null && _graph.toggleTitle != value)
                    _graph.toggleTitle = value;

                _toggleTitle = value;

            }
        }


        private bool _enable;
        public bool enable
        {
            get
            {
                if (_graph != null)
                    return _graph.enable;
                return _enable;
            }

            set
            {
                if (_graph != null && _graph.enable != value)
                    _graph.enable = value;

                _enable = value;
            }
        }



        private bool _isToggleable;
        public bool isToogleable
        {
            get
            {
                if (_graph != null)
                    return _graph.isToggleable;
                return _isToggleable;
            }

            set
            {
                if (_graph != null && _graph.isToggleable != value)
                    _graph.isToggleable = value;

                _isToggleable = value;
            }
        }


        private bool _toggle;
        public bool toggle
        {
            get
            {
                if (_graph != null)
                    return _graph.toggle;
                return _toggle;
            }

            set
            {
                if (_graph != null && _graph.toggle != value)
                    _graph.toggle = value;
                _toggle = value;
                this.NotifyPropertyChanged("toggle");
            }
        }



        private Color _plotColor = Colors.Blue;

        public Color plotColor
        {
            get
            {
                if (_graph != null)
                    return _graph.plotColor;
                return _plotColor;
            }
            set
            {
                if (_graph != null && _graph.plotColor != value)
                    _graph.plotColor = value;

                _plotColor = value;
            }
        }


        private Color _axisColor = Colors.Red;

        public Color axisColor
        {
            get
            {
                if (_graph != null)
                    return _graph.axisColor;
                return _axisColor;
            }

            set
            {
                if (_graph != null && _graph.axisColor != value)
                    _graph.axisColor = value;

                _axisColor = value;

            }
        }



        public Graph graph
        {
            get
            {
                return _graph;
            }
            set
            {
                if (_graph != value)
                {
                    _graph = value;
                    _graph.xMin = xMin;
                    _graph.xMax = xMax;
                    _graph.yMin = yMin;
                    _graph.yMax = yMax;

                    _graph.viewPortStride = viewPortStride;
                    _graph.plotMode = plotMode;

                    _graph.topEdge = topEdge;
                    _graph.bottomEdge = bottomEdge;
                    _graph.leftEdge = leftEdge;
                    _graph.rightEdge = rightEdge;
                    _graph.plotColor = plotColor;
                    _graph.axisColor = axisColor;

                    _graph.topTitle = topTitle;
                    _graph.bottomTitle = bottomTitle;
                    _graph.leftTitle = leftTitle;
                    _graph.rightTitle = rightTitle;

                    _graph.enable = enable;
                    _graph.toggle = toggle;
                    _graph.isToggleable = isToogleable;
                    _graph.toggleTitle = toggleTitle;

                    if (_graphMarkers != null)
                    {
                        foreach (GraphMarker item in _graphMarkers)
                        {
                            _graph.graphMarkers.Add(item);
                        }
                        _graphMarkers.Clear();
                    }

                    _graph.PropertyChanged += _graph_PropertyChanged;
                }
            }
        }



        public BlockingCollection<Point> pointQueue
        {
            get
            {
                return _pointQueue;
            }
        }


        void _graph_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "plotMode": _plotMode = _graph.plotMode; break;
                case "viewPortStride": _viewPortStride = _graph.viewPortStride; break;
                case "xMin": _xMin = _graph.xMin; break;
                case "xMax": _xMax = _graph.xMax; break;
                case "yMin": _yMin = _graph.yMin; break;
                case "yMax": _yMax = _graph.yMax; break;
                case "plotColor": _plotColor = _graph.plotColor; break;
                case "topEdge": _topEdge = _graph.topEdge; break;
                case "bottomEdge": _bottomEdge = _graph.bottomEdge; break;
                case "leftEdge": _leftEdge = _graph.leftEdge; break;
                case "rightEdge": _rightEdge = _graph.rightEdge; break;
                case "topTitle": _topTitle = _graph.topTitle; break;
                case "bottomTitle": _bottomTitle = _graph.bottomTitle; break;
                case "leftTitle": _leftTitle = _graph.leftTitle; break;
                case "rightTitle": _rightTitle = _graph.rightTitle; break;
                case "toggleTitle": _toggleTitle = _graph.toggleTitle; break;
                case "isToggleable": _isToggleable = _graph.isToggleable; break;
                case "toggle":
                    {
                        _toggle = _graph.toggle;
                        this.NotifyPropertyChanged("toggle");
                    }
                    break;

                default: break;

            }
        }



        private void Init()
        {
            _pointQueue = new BlockingCollection<Point>();
            _pointList = new LinkedList<Point>();
            _graphMarkers = new ObservableCollection<GraphMarker>();
        }

        public GraphModel()
        {
            Init();
        }

        public GraphModel(Graph graph)
        {
            Init();
            this._graph = graph;
        }


        /// <summary>
        ///Takes items from the BlockingCollection and adds them to a list 
        ///which cill be used to feed the PlotList method. This runs always in the
        ///UI thread, while the method pushing the points can run elsewhere.
        ///A call through the Dispatcher is in that case mandatory.
        /// </summary>
        /// <param name="itemsToPeek"></param>
        public void CommitToGraph(int itemsToCommit)
        {
            Point myPoint;


            for (int i = 0; i < itemsToCommit; i++)
            {
                _pointQueue.TryTake(out myPoint);
                if (myPoint != null)
                {

                    _pointList.AddLast(myPoint);
                    graph.cachedPoints.AddLast(myPoint);
                }
            }

            graph.PlotList(_pointList);
            _pointList.Clear();

        }



    }
}
