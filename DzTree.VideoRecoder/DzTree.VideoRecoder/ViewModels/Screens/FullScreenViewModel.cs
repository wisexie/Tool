using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DzTree.VideoRecoder.ViewModels.Screens
{
    public class FullScreenViewModel: Screen, IFullScreenViewModel
    {
        private Point startPoint;
        private bool isClose;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseDown(object sender, MouseButtonEventArgs e)
        {
            Window win = sender as Window;
            startPoint = e.GetPosition(win);
            RectLeftoffset = startPoint.X;
            RectTopoffset = startPoint.X;

            RectHeight = 0;
            RectWidth = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                return;
            Window win = sender as Window;
            var pos = e.GetPosition(win);

            // Set the position of rectangle
            var x = Math.Min(pos.X, startPoint.X);
            var y = Math.Min(pos.Y, startPoint.Y);

            // Set the dimenssion of the rectangle
            var w = Math.Max(pos.X, startPoint.X) - x;
            var h = Math.Max(pos.Y, startPoint.Y) - y;

            RectLeftoffset = x;
            RectTopoffset = y;

            RectHeight = h;
            RectWidth = w;
        }

        public void MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            isClose = true;
            this.TryClose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!isClose)
                RectSelectArea = new Rect(Math.Max(0,RectLeftoffset-7), Math.Max(0,RectTopoffset-7), RectWidth, RectHeight);
        }

        double _RectLeftoffset;
        public double RectLeftoffset
        {
            get { return _RectLeftoffset; }
            set
            {
                _RectLeftoffset = value;
                NotifyOfPropertyChange(() => RectLeftoffset);
            }
        }

        double _RectTopoffset;
        public double RectTopoffset
        {
            get { return _RectTopoffset; }
            set
            {
                _RectTopoffset = value;
                NotifyOfPropertyChange(() => RectTopoffset);
            }
        }

        double _RectWidth;
        public double RectWidth
        {
            get { return _RectWidth; }
            set
            {
                _RectWidth = value;
                NotifyOfPropertyChange(() => RectWidth);
            }
        }

        double _RectHeight;
        public double RectHeight
        {
            get { return _RectHeight; }
            set
            {
                _RectHeight = value;
                NotifyOfPropertyChange(() => RectHeight);
            }
        }

        Rect _RectSelectArea;

        public Rect RectSelectArea
        {
            get { return _RectSelectArea; }
            set
            {
                _RectSelectArea = value;
                NotifyOfPropertyChange(() => RectSelectArea);
            }
        }
    }
}
