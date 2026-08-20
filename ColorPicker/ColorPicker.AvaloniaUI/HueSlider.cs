using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorPicker.Models;

namespace ColorPicker;

[TemplatePart(Name = "PART_Handle", Type = typeof(Control))]
internal class HueSlider : TemplatedControl
{
    public static readonly StyledProperty<double> SmallChangeProperty = AvaloniaProperty.Register<HueSlider, double>(
        nameof(SmallChange), 1);

    public static readonly StyledProperty<double> ValueProperty = AvaloniaProperty.Register<HueSlider, double>(
        nameof(Value));

    private Control _handlePart;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _handlePart = e.NameScope.Find<Control>("PART_Handle");

        if (_handlePart != null)
        {
            _handlePart.AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
            _handlePart.AddHandler(PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
            _handlePart.AddHandler(PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
            _handlePart.AddHandler(PointerWheelChangedEvent, OnPreviewMouseWheel, RoutingStrategies.Tunnel);
        }
        
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
        AddHandler(PointerWheelChangedEvent, OnPreviewMouseWheel, RoutingStrategies.Tunnel);
    }

    public double SmallChange
    {
        get => GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (_handlePart != null)
        {
            e.Pointer.Capture(_handlePart);
        }
        else
        {
            e.Pointer.Capture(this);
        }
        UpdateValue(e.GetPosition(this));
        e.Handled = true;
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        var captured = e.Pointer.Captured;
        if (captured != null && (Equals(captured, _handlePart) || Equals(captured, this)))
        {
            UpdateValue(e.GetPosition(this));
            e.Handled = true;
        }
    }

    private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        e.Pointer.Capture(null);
    }

    private void UpdateValue(Point mousePos)
    {
        double ratio = Math.Clamp(mousePos.Y / Bounds.Height, 0, 1);
        Value = ratio * 360;
    }

    private void OnPreviewMouseWheel(object sender, PointerWheelEventArgs args)
    {
        Value = MathHelper.Mod(Value - (SmallChange * args.Delta.Y), 360);
        args.Handled = true;
    }
}