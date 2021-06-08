﻿using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Text.Format;
using Android.Views;
using Microsoft.Maui.Graphics.Native;

namespace Microsoft.Maui.Graphics.Controls
{
    public class GraphicsTimePicker : View, IMixedNativeView
    {
        TimeSpan _time;

        readonly NativeCanvas _canvas;
        readonly ScalingCanvas _scalingCanvas;
        readonly float _scale;

        int _width, _height;
        Color? _backgroundColor;
        IDrawable? _drawable;

        TimePickerDialog? _dialog;

        public GraphicsTimePicker(Context context) : base(context)
        {
            _scale = Resources?.DisplayMetrics?.Density ?? 1;
            _canvas = new NativeCanvas(context);
            _scalingCanvas = new ScalingCanvas(_canvas);

            Touch += OnTouch;
        }

        public TimeSpan Time
        {
            get { return _time; }
            set
            {
                _time = value;
                UpdateTime(_time);
            }
        }

        public Color? BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                Invalidate();
            }
        }

        public IDrawable? Drawable
        {
            get => _drawable;
            set
            {
                _drawable = value;
                Invalidate();
            }
        }

        public event EventHandler<TimeSelectedEventArgs>? TimeSelected;

        static readonly string[] DefaultNativeLayers = new[] { nameof(IDatePicker.BackgroundColor) };

        public string[] NativeLayers => DefaultNativeLayers;

        public void DrawBaseLayer(RectangleF dirtyRect) { }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(disposing)
            {
                if (_dialog != null)
                {
                    _dialog.Dispose();
                    _dialog = null;
                }

                Touch -= OnTouch;
            }
        }

        public override void Draw(Canvas? androidCanvas)
        {
            if (_drawable == null)
                return;

            var dirtyRect = new RectangleF(0, 0, _width, _height);

            _canvas.Canvas = androidCanvas;

            if (_backgroundColor != null)
            {
                _canvas.FillColor = _backgroundColor;
                _canvas.FillRectangle(dirtyRect);
                _canvas.FillColor = Colors.White;
            }

            _scalingCanvas.ResetState();
            _scalingCanvas.Scale(_scale, _scale);

            dirtyRect.Height /= _scale;
            dirtyRect.Width /= _scale;
            _drawable.Draw(_scalingCanvas, dirtyRect);
            _canvas.Canvas = null;
        }

        protected override void OnSizeChanged(int width, int height, int oldWidth, int oldHeight)
        {
            base.OnSizeChanged(width, height, oldWidth, oldHeight);
            _width = width;
            _height = height;
        }

        void OnTouch(object sender, TouchEventArgs e)
        {
            if (e.Event?.Action == MotionEventActions.Up)
            {
                CreateDialog(Time);
            }
        }

        void CreateDialog(TimeSpan time)
        {
            if (_dialog == null)
            {
                bool is24HourFormat = DateFormat.Is24HourFormat(Context);
                _dialog = new TimePickerDialog(Context!, OnTimeSelected, time.Hours, time.Minutes, is24HourFormat);

                _dialog.SetCanceledOnTouchOutside(true);

                _dialog.DismissEvent += (sender, args) =>
                {
                    _dialog.Dispose();
                    _dialog = null;
                };

                _dialog.Show();
            }
        }

        void UpdateTime(TimeSpan time)
        {
            TimeSelected?.Invoke(this, new TimeSelectedEventArgs(time));
        }

        void OnTimeSelected(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            var time = new TimeSpan(e.HourOfDay, e.Minute, 0);
            UpdateTime(time);
        }
    }
}