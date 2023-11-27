using Avalonia.Controls;
using Avalonia.Input;

namespace WinPass.UI.Views;

public partial class TitleBar : UserControl
{
    #region Events

    public delegate void WindowDraggedEvent(int x, int y);

    public event WindowDraggedEvent? OnWindowDragged;

    #endregion
    
    #region Members

    private bool _isDraggingWindow;

    #endregion

    #region Constructors

    public TitleBar()
    {
        InitializeComponent();
    }

    #endregion

    #region Private methods

    private void GridTitleBar_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isDraggingWindow = true;
    }

    private void GridTitleBar_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDraggingWindow = false;
    }

    private void GridTitleBar_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDraggingWindow) return;

        var pos = e.GetPosition(this);
        OnWindowDragged?.Invoke((int)pos.X, (int)pos.Y);
    }

    private void Ignore_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
    }

    #endregion
}