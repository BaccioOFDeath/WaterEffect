using CoreGraphics;
using Foundation;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using System;
using UIKit;
using System.Collections.Generic;

public class EnhancedRippleViewController : UIViewController
{
    #region Constants
    private const float InitialPressureFactor = 0.7f;
    private const float DampingFactor = 0.95f;
    private const bool EnableDiagnosticLogging = false;
    #endregion

    #region Fields
    private Dictionary<UITouch, CGPoint> _lastTouchPoints = new Dictionary<UITouch, CGPoint>();
    private SKPaint _paint = new SKPaint();
    private SKPaint _gradientPaint = new SKPaint();
    private SKCanvasView _canvasView;
    private float[,] _rippleMap, _lastRippleMap;
    private readonly int _mapSize = 325;
    private NSTimer _timer;
    private readonly object _lockObj = new object();
    private int _affectedAreaStartX = 0, _affectedAreaStartY = 0, _affectedAreaEndX, _affectedAreaEndY;
    private bool _isTouchOccurred = false;
    #endregion

    #region Constructor
    public EnhancedRippleViewController()
    {
        _affectedAreaEndX = _mapSize;
        _affectedAreaEndY = _mapSize;
        _paint.IsAntialias = true;
    }
    #endregion

    #region UIViewController Overrides
    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        InitializeViewComponents();
        View.BackgroundColor = UIColor.Black; // Set a dark background color
        SetNeedsStatusBarAppearanceUpdate();
    }

    public override void ViewDidAppear(bool animated)
    {
        base.ViewDidAppear(animated);
        // Additional size adjustments can be made here if needed
    }

    public override UIStatusBarStyle PreferredStatusBarStyle()
    {
        return UIStatusBarStyle.DarkContent;
    }

    public override bool PrefersHomeIndicatorAutoHidden => true;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer?.Invalidate();
            _timer?.Dispose();
            _paint?.Dispose();
            _gradientPaint?.Dispose();
            _canvasView?.RemoveFromSuperview();
            _canvasView?.Dispose();
        }
        base.Dispose(disposing);
    }
    #endregion

    #region Initialization Methods
    private void InitializeViewComponents()
    {
        _canvasView = new SKCanvasView(View.Bounds)
        {
            AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
            IgnorePixelScaling = true,
            MultipleTouchEnabled = true
        };
        _canvasView.PaintSurface += OnPaintSurface;
        View.AddSubview(_canvasView);

        _timer = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromMilliseconds(16), TimerElapsed);

        _gradientPaint.Shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(_mapSize, _mapSize),
            new SKColor[] { SKColors.Blue, SKColors.LightBlue },
            null,
            SKShaderTileMode.Clamp);

        _rippleMap = new float[_mapSize, _mapSize];
        _lastRippleMap = new float[_mapSize, _mapSize];
        Buffer.BlockCopy(_rippleMap, 0, _lastRippleMap, 0, _rippleMap.Length * sizeof(float));
    }
    #endregion



    #region Touch Event Handling Optimized
    public override void TouchesBegan(NSSet touches, UIEvent evt)
    {
        base.TouchesBegan(touches, evt);
        HandleTouches(touches, true);
    }

    public override void TouchesMoved(NSSet touches, UIEvent evt)
    {
        base.TouchesMoved(touches, evt);
        HandleTouches(touches, false);
    }

    public override void TouchesEnded(NSSet touches, UIEvent evt) => RemoveTouchPoints(touches);
    public override void TouchesCancelled(NSSet touches, UIEvent evt) => RemoveTouchPoints(touches);

    private void HandleTouches(NSSet touches, bool touchBegan)
    {
        var uiTouches = touches.ToArray<UITouch>();
        foreach (var touch in uiTouches)
        {
            var location = touch.LocationInView(View);
            // Explicitly cast nfloat to float for the pressure calculation
            float pressure = touch.MaximumPossibleForce == 0 ? 1.0f : (float)(touch.Force / touch.MaximumPossibleForce);
            pressure = Math.Clamp(pressure, 0.1f, 1.0f);

            if (touchBegan)
            {
                ApplyInitialRippleAtPoint(location, pressure);
            }
            else
            {
                if (_lastTouchPoints.TryGetValue(touch, out var lastPoint))
                {
                    InterpolateRipples(lastPoint, location, pressure);
                }
            }

            // Store the last touch point, casting CGPoint's double values to float if necessary
            _lastTouchPoints[touch] = new CGPoint((float)location.X, (float)location.Y);
        }
    }

    private void RemoveTouchPoints(NSSet touches)
    {
        var uiTouches = touches.ToArray<UITouch>();
        foreach (var touch in uiTouches)
        {
            _lastTouchPoints.Remove(touch);
        }
    }
    #endregion


    #region Ripple Effect Logic Optimized
    private void ApplyInitialRippleAtPoint(CGPoint location, float pressure)
    {
        // Direct application of damping within recommended range
        ApplyRippleAtPoint(location, pressure, Math.Clamp(DampingFactor, 0.8f, 0.99f));
        _isTouchOccurred = true;
    }

    private void InterpolateRipples(CGPoint start, CGPoint end, float pressure)
    {
        // Use of dynamic damping for consistency in ripple effect
        float dynamicDamping = Math.Clamp(DampingFactor, 0.8f, 0.99f);
        int steps = (int)Math.Max(Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y));

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            CGPoint interpolatedPoint = new CGPoint(
                start.X + t * (end.X - start.X),
                start.Y + t * (end.Y - start.Y)
            );
            ApplyRippleAtPoint(interpolatedPoint, pressure, dynamicDamping);
        }
    }

    private void ApplyRippleAtPoint(CGPoint point, float pressure, float damping)
    {
        var (mapX, mapY) = GetMapCoordinates(point);
        ApplyRipple(mapX, mapY, pressure, damping);
    }

    private void ApplyRipple(int x, int y, float pressure, float damping)
    {
        lock (_lockObj)
        {
            int impactRadius = (int)Math.Ceiling(3 * pressure);
            for (int dx = -impactRadius; dx <= impactRadius; dx++)
            {
                for (int dy = -impactRadius; dy <= impactRadius; dy++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 1 && nx < _mapSize - 1 && ny >= 1 && ny < _mapSize - 1)
                    {
                        _rippleMap[nx, ny] += InitialPressureFactor * pressure / (Math.Abs(dx) + Math.Abs(dy) + 1) * damping;
                        UpdateAffectedAreaBounds(nx, ny, impactRadius);
                    }
                }
            }
        }
    }

    private void UpdateRippleEffect()
    {
        int size = _rippleMap.GetLength(0);

        for (int x = 1; x < size - 1; x++)
        {
            for (int y = 1; y < size - 1; y++)
            {
                float newHeight = (
                    _rippleMap[x - 1, y] +
                    _rippleMap[x + 1, y] +
                    _rippleMap[x, y - 1] +
                    _rippleMap[x, y + 1]) / 2.0f - _lastRippleMap[x, y];

                _lastRippleMap[x, y] = newHeight * DampingFactor;
            }
        }

        SwapRippleMaps();
    }

    private void SwapRippleMaps()
    {
        var temp = _rippleMap;
        _rippleMap = _lastRippleMap;
        _lastRippleMap = temp;
    }

    private void ResetAffectedArea()
    {
        _affectedAreaStartX = _affectedAreaStartY = 0;
        _affectedAreaEndX = _affectedAreaEndY = _mapSize;
    }

    // Helper Methods
    private (int mapX, int mapY) GetMapCoordinates(CGPoint point)
    {
        return (
            (int)(point.X * (_rippleMap.GetLength(0) / View.Bounds.Width)),
            (int)(point.Y * (_rippleMap.GetLength(1) / View.Bounds.Height))
        );
    }

    private void UpdateAffectedAreaBounds(int x, int y, int radius)
    {
        _affectedAreaStartX = Math.Min(_affectedAreaStartX, x - radius);
        _affectedAreaStartY = Math.Min(_affectedAreaStartY, y - radius);
        _affectedAreaEndX = Math.Max(_affectedAreaEndX, x + radius);
        _affectedAreaEndY = Math.Max(_affectedAreaEndY, y + radius);
    }
    #endregion





    #region Timer and Rendering
    private void TimerElapsed(NSTimer obj)
    {
        if (_isTouchOccurred)
        {
            ResetAffectedArea();
            _isTouchOccurred = false;
        }
        UpdateRippleEffect();
        BeginInvokeOnMainThread(() => _canvasView.SetNeedsDisplay());
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info; // Contains canvas size and other details

        // Correctly cast nfloat to float
        var scale = (float)UIScreen.MainScreen.Scale;

        if (EnableDiagnosticLogging)
        {
            Console.WriteLine($"Canvas Info: {info.Width}x{info.Height}");
            Console.WriteLine($"Device Scale Factor: {scale}");
        }
        canvas.Clear(SKColors.Black);

        // Make sure this method is correctly implemented
        DrawRippleEffect(canvas);
    }

    private void DrawRippleEffect(SKCanvas canvas)
    {
        int width = _rippleMap.GetLength(0);
        int height = _rippleMap.GetLength(1);

        float rectWidth = canvas.LocalClipBounds.Width / width;
        float rectHeight = canvas.LocalClipBounds.Height / height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = _rippleMap[x, y];
                byte alpha = (byte)(Math.Max(0, Math.Min(255, value * 255)));
                _paint.Color = new SKColor(0, 0, 255, alpha);

                float rectX = x * rectWidth;
                float rectY = y * rectHeight;

                canvas.DrawRect(new SKRect(rectX, rectY, rectX + rectWidth, rectY + rectHeight), _paint);
            }
        }
    }

    #endregion
}