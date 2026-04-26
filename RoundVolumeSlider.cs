using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace yt_panel;

[DefaultEvent(nameof(ValueChanged))]
[DesignerCategory("Code")]
[ToolboxItem(true)]
public class RoundVolumeSlider : Control
{
    private int minimum;
    private int maximum = 100;
    private int value = 50;
    private int trackHeight = 8;
    private int thumbRadius = 11;
    private Color trackColor = Color.FromArgb(50, 50, 50);
    private Color fillColor = Color.FromArgb(255, 255, 255);
    private Color thumbColor = Color.FromArgb(255, 255, 255);
    private Color thumbBorderColor = Color.FromArgb(25, 25, 25);

    public RoundVolumeSlider()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.Selectable |
                 ControlStyles.UserPaint, true);

        BackColor = Color.Black;
        Cursor = Cursors.Hand;
        MinimumSize = new Size(90, 28);
        Size = new Size(220, 34);
        TabStop = true;
    }

    public event EventHandler? ValueChanged;

    [Category("Volume Slider")]
    [DefaultValue(0)]
    public int Minimum
    {
        get => minimum;
        set
        {
            minimum = value;
            if (maximum < minimum)
            {
                maximum = minimum;
            }

            Value = Math.Max(this.value, minimum);
            Invalidate();
        }
    }

    [Category("Volume Slider")]
    [DefaultValue(100)]
    public int Maximum
    {
        get => maximum;
        set
        {
            maximum = Math.Max(value, minimum);
            Value = Math.Min(this.value, maximum);
            Invalidate();
        }
    }

    [Category("Volume Slider")]
    [DefaultValue(50)]
    public int Value
    {
        get => value;
        set
        {
            var next = Math.Max(minimum, Math.Min(maximum, value));
            if (this.value == next)
            {
                return;
            }

            this.value = next;
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    [Category("Volume Slider")]
    [DefaultValue(8)]
    public int TrackHeight
    {
        get => trackHeight;
        set
        {
            trackHeight = Math.Max(2, value);
            Invalidate();
        }
    }

    [Category("Volume Slider")]
    [DefaultValue(11)]
    public int ThumbRadius
    {
        get => thumbRadius;
        set
        {
            thumbRadius = Math.Max(4, value);
            Invalidate();
        }
    }

    [Category("Volume Slider")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color TrackColor
    {
        get => trackColor;
        set
        {
            trackColor = value;
            Invalidate();
        }
    }

    [Category("Volume Slider")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color FillColor
    {
        get => fillColor;
        set
        {
            fillColor = value;
            Invalidate();
        }
    }

    [Category("Volume Slider")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color ThumbColor
    {
        get => thumbColor;
        set
        {
            thumbColor = value;
            Invalidate();
        }
    }

    [Category("Volume Slider")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color ThumbBorderColor
    {
        get => thumbBorderColor;
        set
        {
            thumbBorderColor = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = GetTrackRectangle();
        var percent = maximum == minimum ? 0F : (value - minimum) / (float)(maximum - minimum);
        var thumbX = rect.Left + rect.Width * percent;
        var fillRect = RectangleF.FromLTRB(rect.Left, rect.Top, thumbX, rect.Bottom);

        using var trackBrush = new SolidBrush(trackColor);
        using var fillBrush = new SolidBrush(fillColor);
        using var thumbBrush = new SolidBrush(thumbColor);
        using var borderPen = new Pen(thumbBorderColor, 2F);

        FillRoundRect(e.Graphics, trackBrush, rect, rect.Height / 2F);
        if (fillRect.Width > 0.5F)
        {
            FillRoundRect(e.Graphics, fillBrush, fillRect, fillRect.Height / 2F);
        }

        var thumbRect = new RectangleF(
            thumbX - thumbRadius,
            Height / 2F - thumbRadius,
            thumbRadius * 2F,
            thumbRadius * 2F);

        e.Graphics.FillEllipse(thumbBrush, thumbRect);
        e.Graphics.DrawEllipse(borderPen, thumbRect);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        Focus();
        Capture = true;
        SetValueFromX(e.X);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (Capture && e.Button == MouseButtons.Left)
        {
            SetValueFromX(e.X);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        Capture = false;
    }

    protected override bool IsInputKey(Keys keyData)
    {
        return keyData is Keys.Left or Keys.Right or Keys.Home or Keys.End || base.IsInputKey(keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.KeyCode == Keys.Left)
        {
            Value -= 5;
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Right)
        {
            Value += 5;
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Home)
        {
            Value = minimum;
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.End)
        {
            Value = maximum;
            e.Handled = true;
        }
    }

    private RectangleF GetTrackRectangle()
    {
        var pad = thumbRadius + 2;
        var width = Math.Max(1, Width - pad * 2);
        return new RectangleF(pad, Height / 2F - trackHeight / 2F, width, trackHeight);
    }

    private void SetValueFromX(int x)
    {
        var rect = GetTrackRectangle();
        var ratio = Math.Max(0F, Math.Min(1F, (x - rect.Left) / rect.Width));
        Value = minimum + (int)Math.Round((maximum - minimum) * ratio);
    }

    private static void FillRoundRect(Graphics graphics, Brush brush, RectangleF rect, float radius)
    {
        using var path = new GraphicsPath();
        var diameter = radius * 2F;

        path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        graphics.FillPath(brush, path);
    }
}
