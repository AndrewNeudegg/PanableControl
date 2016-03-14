/*
Copyright (c) 2016 AndrewNeudegg

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace PanableArea
{
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    public class PanArea : Control
    {
        public PanArea()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.UserPaint, true);
        }

        #region Overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            // Call the OnPaint method of the base class.
            base.OnPaint(e);
            // Call methods of the System.Drawing.Graphics object.
            DrawGrid(e.Graphics);
        }

        private void DrawGrid(Graphics graphics)
        {
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            graphics.Clear(BackColor);

            float LargeGridSize = MajorGridLineSize;
            float SmallGridSize = MinorGridLineSize;

            float smallXOffset = Math.Abs(GridCenter.X)%SmallGridSize;
            float largeXOffset = Math.Abs(GridCenter.X)%LargeGridSize;

            float smallYOffset = Math.Abs(GridCenter.Y)%SmallGridSize;
            float largeYOffset = Math.Abs(GridCenter.Y)%LargeGridSize;

            Pen SmallGridPen = new Pen(MinorGridLineColor);
            Pen LargeGridPen = new Pen(MajorGridLineColor);

            for (float x = smallXOffset; x < ClientRectangle.Right; x += SmallGridSize)
            {
                graphics.DrawLine(SmallGridPen, x, ClientRectangle.Top, x, ClientRectangle.Bottom);
            }

            for (float y = smallYOffset; y < ClientRectangle.Bottom; y += SmallGridSize)
            {
                graphics.DrawLine(SmallGridPen, ClientRectangle.Left, y, ClientRectangle.Right, y);
            }

            for (float x = largeXOffset; x < ClientRectangle.Right; x += LargeGridSize)
            {
                graphics.DrawLine(LargeGridPen, x, ClientRectangle.Top, x, ClientRectangle.Bottom);
            }

            for (float y = largeYOffset; y < ClientRectangle.Bottom; y += LargeGridSize)
            {
                graphics.DrawLine(LargeGridPen, ClientRectangle.Left, y, ClientRectangle.Right, y);
            }

            graphics.DrawRectangle(new Pen(BorderColor, 2), ClientRectangle.X, ClientRectangle.Y,
                ClientRectangle.Width - 2, ClientRectangle.Height - 2);

#if (DEBUG)
            string values = string.Format("X:{0}, Y:{1}", GridCenter.X, GridCenter.Y);
            Font font = new Font("Arial", 8);
            var sizeF = graphics.MeasureString(values, font);
            Rectangle topRightRectangle = new Rectangle(5, 5, (int) (sizeF.Width + 10), (int) (sizeF.Height));
            graphics.FillRectangle(new SolidBrush(Color.WhiteSmoke), topRightRectangle.X, topRightRectangle.Y,
                topRightRectangle.Width, topRightRectangle.Height);
            graphics.DrawRectangle(new Pen(Brushes.Black), topRightRectangle);
            graphics.DrawString(values, font, new SolidBrush(Color.Black), 5, 5);
#endif
        }

        #endregion


        #region Properties

        #region VisualProperties
        private Color _majorGridLineColor = Color.CadetBlue;
        public Color MajorGridLineColor
        {
            get { return _majorGridLineColor; }
            set { _majorGridLineColor = value; }
        }

        private Color _minorGridLineColor = Color.AliceBlue;
        public Color MinorGridLineColor
        {
            get { return _minorGridLineColor; }
            set
            {
                _minorGridLineColor = value;
                Invalidate();
            }
        }

        private float _minorGridLineSize = 20f;
        public float MinorGridLineSize
        {
            get { return _minorGridLineSize; }
            set
            {
                _minorGridLineSize = value;
                Invalidate();
            }
        }

        private float _majorGridLineSize = 100f;
        public float MajorGridLineSize
        {
            get { return _majorGridLineSize; }
            set
            {
                _majorGridLineSize = value;
                Invalidate();
            }
        }

        private Color _borderColor = Color.Black;
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                Invalidate();
            }
        }

        #endregion

        #endregion

        #region GridMovement

        private bool _isMouseDownOnGrid;
        private PointF _mouseLocation;

        private bool MouseDownOnGrid
        {
            get { return _isMouseDownOnGrid; }
            set { _isMouseDownOnGrid = value; }
        }

        private PointF MousePreviousLocation
        {
            get { return _mouseLocation; }
            set { _mouseLocation = value; }
        }

        private Point _gridCenter = new Point(0, 0);
        private Point GridCenter
        {
            get { return _gridCenter; }
            set { _gridCenter = value; }
        }



        #endregion


      

        #region events
        protected override void OnMouseUp(MouseEventArgs e)
        {
            MouseDownOnGrid = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                MouseDownOnGrid = true;
                MousePreviousLocation = new Point(e.X, e.Y);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (MouseDownOnGrid)
            {
                if (e.Button == MouseButtons.Left)
                {
                    PanToLocation(e.Location, MousePreviousLocation);
                    MousePreviousLocation = new Point(e.X, e.Y);
                }
            }

        }
        #endregion


        #region MovementLogic

        public void PanToLocation(PointF mousePosition, PointF originPointF)
        {
            var nodes = this.Controls.OfType<Control>();
            foreach (var node in nodes)
            {
                node.Top = node.Location.Y + (int)(mousePosition.Y - originPointF.Y);
                node.Left = node.Location.X + (int)(mousePosition.X - originPointF.X);
            }
            GridCenter = new Point(GridCenter.X + (int)(mousePosition.X - originPointF.X), GridCenter.Y + (int)(mousePosition.Y - originPointF.Y));
            Refresh();
        }
        #endregion

        #region Designer
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;



        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        #endregion
        #endregion
    }
}
