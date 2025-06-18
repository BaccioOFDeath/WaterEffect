using System;
using System.Linq;
using OpenTK.Graphics.ES30;
using UIKit;
using CoreGraphics;
using OpenGLES;
using Foundation;
using GLKit;

public class OpenGLWaterEffectViewController : GLKViewController
{
    private EAGLContext context;
    private int program;
    private int positionAttribLocation;
    private int timeUniformLocation;
    private int rippleCenterUniformLocation;
    private float totalTime = 0.0f;
    private const int MaxTouches = 10; // Limit the number of simultaneous touches
    private CGPoint[] touchPoints = new CGPoint[MaxTouches];

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        context = new EAGLContext(EAGLRenderingAPI.OpenGLES3);
        if (context == null || !EAGLContext.SetCurrentContext(context))
        {
            Console.WriteLine("Failed to create EAGLContext");
            return;
        }

        var glkView = (GLKView)View;
        glkView.Context = context;
        glkView.DrawableDepthFormat = GLKViewDrawableDepthFormat.Format24;

        SetupShaders();
    }

    private void SetupShaders()
    {
        // Vertex shader source
        string vertexShaderSource = @"
            attribute vec4 position;
            varying vec2 texCoords;
            void main()
            {
                texCoords = position.xy * 0.5 + 0.5;
                gl_Position = position;
            }";

        // Fragment shader source
        string fragmentShaderSource = @"
            precision mediump float;
            varying vec2 texCoords;
            uniform vec2 rippleCenter; // Uniform to be set from application
            void main()
            {
                float dist = distance(rippleCenter, texCoords); // Correct usage of distance function
                float radius = 0.01; // Radius of the dot
                float alpha = 1.0 - smoothstep(0.0, radius, dist);
                vec3 color = vec3(1.0, 0.0, 0.0); // Red color
                gl_FragColor = vec4(color, alpha);
            }";



        int vertexShader = LoadShader(ShaderType.VertexShader, vertexShaderSource);
        int fragmentShader = LoadShader(ShaderType.FragmentShader, fragmentShaderSource);

        program = GL.CreateProgram();
        GL.AttachShader(program, vertexShader);
        GL.AttachShader(program, fragmentShader);
        GL.LinkProgram(program);

        positionAttribLocation = GL.GetAttribLocation(program, "position");
        timeUniformLocation = GL.GetUniformLocation(program, "time");
        rippleCenterUniformLocation = GL.GetUniformLocation(program, "rippleCenter");

        GL.UseProgram(program);
    }

    private int LoadShader(ShaderType type, string source)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
        if (status == 0)
        {
            string infoLog = GL.GetShaderInfoLog(shader);
            Console.WriteLine($"Shader compilation failed: {infoLog}");
            GL.DeleteShader(shader);
            return -1;
        }

        return shader;
    }

    private void UpdateRippleEffect(float deltaTime)
    {
        totalTime += deltaTime;
        GL.Uniform1(timeUniformLocation, totalTime);
    }

    private void UpdateRippleCenter(CGPoint newCenter)
    {
        float[] center = { (float)newCenter.X, (float)newCenter.Y };
        GL.Uniform2(rippleCenterUniformLocation, 1, ref center[0]);
    }




    private void DrawRipple()
    {
        float[] vertices = {
            -1.0f, -1.0f, 0.0f, // Bottom Left
             1.0f, -1.0f, 0.0f, // Bottom Right
            -1.0f,  1.0f, 0.0f, // Top Left
             1.0f,  1.0f, 0.0f  // Top Right
        };

        GL.EnableVertexAttribArray(positionAttribLocation);
        GL.VertexAttribPointer(positionAttribLocation, 3, VertexAttribPointerType.Float, false, 0, vertices);
        GL.DrawArrays((BeginMode)PrimitiveType.TriangleStrip, 0, 4);
    }

    public override void DrawInRect(GLKView view, CGRect rect)
    {
        GL.ClearColor(0f, 0f, 0f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.UseProgram(program);

        UpdateRippleEffect(0.016f); // Assuming a frame rate of ~60fps

        foreach (var touchPoint in touchPoints.Where(p => p != CGPoint.Empty))
        {
            UpdateRippleCenter(touchPoint); // Update with the touch point
            DrawRipple();
        }
    }


    public override void TouchesBegan(NSSet touches, UIEvent evt)
    {
        base.TouchesBegan(touches, evt);

        foreach (var touch in touches.Cast<UITouch>())
        {
            var location = touch.LocationInView(View);
            CGPoint normalizedLocation = NormalizeTouchLocation(location);
            Console.WriteLine($"Normalized Touch Began at: {normalizedLocation.X}, {normalizedLocation.Y}");
            // GetHashCode can be negative, so use Math.Abs to keep the index within array bounds
            touchPoints[Math.Abs(touch.GetHashCode()) % MaxTouches] = normalizedLocation;
        }
    }



    public override void TouchesMoved(NSSet touches, UIEvent evt)
    {
        base.TouchesMoved(touches, evt);
        TouchesBegan(touches, evt); // Handle moved touches as new touches
    }

    public override void TouchesEnded(NSSet touches, UIEvent evt)
    {
        base.TouchesEnded(touches, evt);

        foreach (var touch in touches.Cast<UITouch>())
        {
            // GetHashCode can be negative, so use Math.Abs to keep the index within array bounds
            touchPoints[Math.Abs(touch.GetHashCode()) % MaxTouches] = CGPoint.Empty;
        }
    }

    private CGPoint NormalizeTouchLocation(CGPoint location)
    {
        var view = (GLKView)View;
        float normalizedX = (float)(location.X / view.Bounds.Size.Width) * 2.0f - 1.0f;
        float normalizedY = (float)(location.Y / view.Bounds.Size.Height) * 2.0f - 1.0f;
        normalizedY = -normalizedY; // Invert the Y-coordinate
        return new CGPoint(normalizedX, normalizedY);
    }






    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (context != null)
            {
                if (EAGLContext.CurrentContext == context)
                {
                    EAGLContext.SetCurrentContext(null);
                }
                context.Dispose();
            }
            if (program != 0)
            {
                GL.DeleteProgram(program);
            }
        }
        base.Dispose(disposing);
    }
}
