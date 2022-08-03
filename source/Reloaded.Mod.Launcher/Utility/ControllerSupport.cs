﻿using static Sewer56.UI.Controller.Core.Enums.Button;
using static Sewer56.UI.Controller.WPF.WpfPlatform;
using Image = System.Windows.Controls.Image;

namespace Reloaded.Mod.Launcher.Utility;

public static class ControllerSupport
{
    public static ReloadedInputControllerWithConfigurator Controller { get; private set; } = null!;

    public static WpfPlatform Platform { get; private set; } = null!;

    public static Navigator Navigator { get; private set; } = null!;

    private static List<ProcessCustomInputsRoutedDelegate> _processCustomInputsPreview = new();
    private static List<ProcessCustomInputsRoutedDelegate> _processCustomInputs = new();

    public static void Init()
    {
        var miscConfDir = Lib.IoC.Get<LoaderConfig>().GetMiscConfigDirectory();
        var savePath    = Path.Combine(miscConfDir, "Controller.json");

        if (!File.Exists(savePath))
            File.Copy(Path.Combine(AppContext.BaseDirectory, "Assets\\DefaultSettings\\Controller.json"), savePath);

        Controller = new ReloadedInputControllerWithConfigurator(savePath, new ReloadedInputLocalizationProvider());

        // This code is invoked from non-primary thread; but we need to register for events on primary thread, so here we go.
        ActionWrappers.ExecuteWithApplicationDispatcherAsync(() =>
        {
            Platform  = new WpfPlatform();
            Navigator = new Navigator(Platform, Controller);
            Platform.ProcessCustomInputs += ProcessCustomInputs;
        });
    }

    // Subscriptions for events handling custom preview.
    public static void SubscribePreviewCustomInputs(ProcessCustomInputsRoutedDelegate processEvents) => _processCustomInputsPreview.Add(processEvents);
    public static void UnsubscribePreviewCustomInputs(ProcessCustomInputsRoutedDelegate processEvents) => _processCustomInputsPreview.Remove(processEvents);
    public static void SubscribeCustomInputs(ProcessCustomInputsRoutedDelegate processEvents) => _processCustomInputs.Add(processEvents);
    public static void UnsubscribeCustomInputs(ProcessCustomInputsRoutedDelegate processEvents) => _processCustomInputs.Remove(processEvents);

    /// <summary>
    /// Tries to get the page scroll direction based on input.
    /// </summary>
    internal static bool TryGetPageScrollDirection(ControllerState state, out int direction)
    {
        if (state.PressedButtons.HasAnyFlag(NextPage))
        {
            direction = 1;
            return true;
        }

        if (state.PressedButtons.HasAnyFlag(LastPage))
        {
            direction = -1;
            return true;
        }

        direction = 0;
        return false;
    }

    /// <summary>
    /// Gets the list/array scroll direction based on pressed controller button.
    /// </summary>
    internal static bool TryGetListScrollDirection(ControllerState state, out int direction)
    {
        if (state.PressedButtons.HasAnyFlag(Down))
        {
            direction = 1;
            return true;
        }

        if (state.PressedButtons.HasAnyFlag(Up))
        {
            direction = -1;
            return true;
        }

        direction = 0;
        return false;
    }

    private static void ProcessCustomInputs(in ControllerState state)
    {
        ProcessCustomControls(state);

        // Send out preview event.
        bool isHandled = false;
        foreach (var previewEvent in _processCustomInputsPreview)
        {
            previewEvent(state, ref isHandled);
            if (isHandled)
                return;
        }

        foreach (var bubbleUpEvent in _processCustomInputs)
        {
            bubbleUpEvent(state, ref isHandled);
            if (isHandled)
                return;
        }
    }

    private static void ProcessCustomControls(in ControllerState state)
    {
        if (!WpfUtilities.TryGetFocusedElementAndWindow(out var window, out var focused))
            return;

        // Handle per-control actions
        switch (focused)
        {
            case NumericUpDown nud:
                HandleNumeric(state, nud);
                break;

            case WatermarkTextBox watermarkBox:
                var parent = WpfUtilities.FindParent<NumericUpDown>(watermarkBox);
                if (parent != null)
                    HandleNumeric(state, parent);

                break;

            case Image image:

                if (!state.IsButtonPressed(Accept))
                    break;

                image.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = UIElement.PreviewMouseDownEvent
                });
                break;

            case TextBlock textBlock:

                if (!state.IsButtonPressed(Accept))
                    break;

                textBlock.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent,
                });

                break;
        }
    }

    private static void HandleNumeric(ControllerState state, NumericUpDown nud)
    {
        void DecrementValue() => nud.Value = Math.Clamp(nud.Value - 1, nud.Minimum, nud.Maximum);
        void IncrementValue() => nud.Value = Math.Clamp(nud.Value + 1, nud.Minimum, nud.Maximum);

        if (state.IsButtonPressed(Increment) || (state.IsButtonHeld(Modifier) && state.IsButtonPressed(Down | Right)))
            IncrementValue();

        if (state.IsButtonPressed(Decrement) || (state.IsButtonHeld(Modifier) && state.IsButtonPressed(Up | Left)))
            DecrementValue();
    }
    
    /// <param name="state">The current state of the controller.</param>
    /// <param name="handled">Whether the event has been handled. If handled state is signaled, no further events will be called.</param>
    public delegate void ProcessCustomInputsRoutedDelegate(in ControllerState state, ref bool handled);
}