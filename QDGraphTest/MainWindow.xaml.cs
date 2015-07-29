/*
 * Copyright 2015 Martin Garcia Carmueja 
 * 
 *  This file is part of QDGraphTest.
 *
 *  QDGraphTest is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  QDGraphTest is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with QDGraphTest.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
using System.Diagnostics;
using EMGFramework.Timer;
using QDGraph;




namespace QDGraphTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private int _maxSamples = 120000;
        private Random _rnd;
        private Point[] _buffer;

        private List<Point> _bufferList;
        private int _bufferLength;

        private int _sampleCounter = 0;
        private int _samplesPerCall = 200;

        private FastTimer _timer;

        private GraphModel[] _graphModels;
        private Graph[] _graphs;

        private int _nChannels;

        public MainWindow()
        {

            InitializeComponent();


            _bufferLength = _maxSamples;

            _nChannels = 8;
            _graphModels = new GraphModel[_nChannels];
            _graphs = new Graph[_nChannels];

            for (int i = 0; i < _nChannels; i++)
            {
                _graphs[i] = new Graph();
                _graphModels[i] = new GraphModel(_graphs[i]);
                
                _graphs[i].xMin = 0;
                _graphs[i].xMax = _maxSamples-1;
                _graphs[i].yMin = -1500;
                _graphs[i].yMax = 1500;

                _graphs[i].topEdge = 5;
                _graphs[i].bottomEdge = 5;
                _graphs[i].rightEdge = 5;
                _graphs[i].leftEdge = 5;

                _graphs[i].plotColor = Color.FromArgb(200,0,0,255); //Because writable bitmaps use premultiplied alpha!

                graphsGrid.Children.Add(_graphs[i]);
            }


            //Initialising plot data
            PrepareData();

            //Setting and starting timer;
            _timer = new FastTimer(3, 100, InsertPoints);
            _timer.Start();

        }


        private void PrepareData()
        {
            _buffer = new Point[_bufferLength];
            _rnd = new Random();

            for (int i = 0; i < _bufferLength; i++)
                _buffer[i] = new Point((double)(i), (1500.0 * (Math.Sin((i) / _rnd.NextDouble() * (10.0)) + Math.Sin((i) / _rnd.NextDouble() * 23.0) + Math.Sin((i) / _rnd.NextDouble() * 31.0)) / 3.0));

            _bufferList = _buffer.ToList();
        }



        /// <summary>
        /// This code simulates client use of the QDGraph library.
        /// </summary>
        private void InsertPoints()
        {
            if (_sampleCounter == _maxSamples)
            {
                _sampleCounter = 0;
                Dispatcher.BeginInvoke((Action)(() => { foreach (Graph graph in _graphs) graph.Clear(); }));
            }
            else
            {
                for (int i = 0; i < _samplesPerCall; i++)
                {
                    foreach (GraphModel graphModel in _graphModels) graphModel.pointQueue.Add(_buffer[(_sampleCounter + i) % _bufferLength]);
                }

                Dispatcher.InvokeAsync((Action)(() => { foreach (GraphModel graphModel in _graphModels) graphModel.CommitToGraph(_samplesPerCall); }));

                _sampleCounter += _samplesPerCall;

            }
        }
    }
}
