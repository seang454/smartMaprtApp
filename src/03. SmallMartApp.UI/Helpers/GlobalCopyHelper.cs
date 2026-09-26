using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SmallMartApp.UI.Helpers;

/// <summary>
/// Provides global copy capabilities across the entire application:
/// 1. Right-click any text (cells, badges, cards, receipts, phone numbers) -> "📋 Copy Text".
/// 2. If inside a table row, also provides "📄 Copy Row Data".
/// 3. Double-click any text to automatically copy it with a brief "✓ Copied!" indicator.
/// 4. Pressing Ctrl+C while hovering over text or selecting a table row copies it to the clipboard.
/// </summary>
public static class GlobalCopyHelper
{
    private static bool _isInitialized = false;

    public static void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        // 1. Global Right-Click: ContextMenu on TextBlocks
        EventManager.RegisterClassHandler(
            typeof(UIElement),
            UIElement.PreviewMouseRightButtonUpEvent,
            new MouseButtonEventHandler(OnGlobalPreviewMouseRightButtonUp),
            true);

        // 2. Global Keyboard Shortcut: Ctrl + C
        EventManager.RegisterClassHandler(
            typeof(UIElement),
            UIElement.PreviewKeyDownEvent,
            new KeyEventHandler(OnGlobalPreviewKeyDown),
            true);

        // 3. Global Double Click: Quick Copy
        EventManager.RegisterClassHandler(
            typeof(UIElement),
            Control.MouseDoubleClickEvent,
            new MouseButtonEventHandler(OnGlobalMouseDoubleClick),
            true);
    }

    private static void OnGlobalPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject depObj) return;

        // Skip if right-clicked on an interactive control (Buttons, MenuItems, ComboBoxes, TextBoxes)
        if (IsInsideInteractiveControl(depObj)) return;

        // Find nearest TextBlock
        var textBlock = FindVisualParent<TextBlock>(depObj);
        if (textBlock == null || string.IsNullOrWhiteSpace(textBlock.Text)) return;

        // If TextBlock already has a custom context menu explicitly assigned, do not override
        if (textBlock.ContextMenu != null) return;

        string textToCopy = textBlock.Text.Trim();

        var contextMenu = new ContextMenu
        {
            Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(4)
        };

        // Item 1: Copy this specific text
        string displaySnippet = textToCopy.Length > 28 ? textToCopy.Substring(0, 25) + "..." : textToCopy;
        var copyCellItem = new MenuItem
        {
            Header = $"📋 Copy \"{displaySnippet}\"",
            FontWeight = FontWeights.SemiBold,
            Padding = new Thickness(10, 6, 10, 6),
            Cursor = Cursors.Hand
        };
        copyCellItem.Click += (s, ev) =>
        {
            CopyToClipboard(textToCopy);
            ShowCopyTooltip(textBlock, "Copied!");
        };
        contextMenu.Items.Add(copyCellItem);

        // Item 2: If inside a ListViewItem / DataGridRow, offer to copy the entire row
        var listViewItem = FindVisualParent<ListViewItem>(textBlock);
        if (listViewItem != null)
        {
            var copyRowItem = new MenuItem
            {
                Header = "📄 Copy Entire Row Data",
                FontWeight = FontWeights.Normal,
                Padding = new Thickness(10, 6, 10, 6),
                Cursor = Cursors.Hand
            };
            copyRowItem.Click += (s, ev) =>
            {
                CopyListViewItem(listViewItem);
            };
            contextMenu.Items.Add(copyRowItem);
        }

        contextMenu.Placement = PlacementMode.MousePoint;
        contextMenu.IsOpen = true;
        e.Handled = true;
    }

    private static void OnGlobalPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.C || (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            return;

        // If focused in an active TextBox with selection, let native copy handle it
        if (Keyboard.FocusedElement is TextBox textBox && textBox.SelectionLength > 0)
            return;

        // Check if mouse is hovering over a TextBlock
        if (Mouse.DirectlyOver is DependencyObject hoveredDep)
        {
            var textBlock = FindVisualParent<TextBlock>(hoveredDep);
            if (textBlock != null && !string.IsNullOrWhiteSpace(textBlock.Text) && !IsInsideInteractiveControl(hoveredDep))
            {
                CopyToClipboard(textBlock.Text.Trim());
                ShowCopyTooltip(textBlock, "Copied!");
                e.Handled = true;
                return;
            }
        }

        // Check if a ListView item is currently focused or selected
        if (Keyboard.FocusedElement is ListViewItem lvi)
        {
            CopyListViewItem(lvi);
            e.Handled = true;
            return;
        }

        if (Keyboard.FocusedElement is ListView listView && listView.SelectedItem != null)
        {
            if (listView.ItemContainerGenerator.ContainerFromItem(listView.SelectedItem) is ListViewItem container)
            {
                CopyListViewItem(container);
                e.Handled = true;
                return;
            }
        }
    }

    private static void OnGlobalMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left) return;
        if (e.OriginalSource is not DependencyObject depObj) return;

        if (IsInsideInteractiveControl(depObj)) return;

        var textBlock = FindVisualParent<TextBlock>(depObj);
        if (textBlock != null && !string.IsNullOrWhiteSpace(textBlock.Text))
        {
            CopyToClipboard(textBlock.Text.Trim());
            ShowCopyTooltip(textBlock, "Copied!");
            e.Handled = true;
        }
    }

    public static void CopyListViewItem(ListViewItem item)
    {
        var textBlocks = FindVisualChildren<TextBlock>(item)
            .Where(tb => !string.IsNullOrWhiteSpace(tb.Text) && !IsInsideInteractiveControl(tb))
            .Select(tb => tb.Text.Trim())
            .Distinct()
            .ToList();

        if (textBlocks.Count > 0)
        {
            string rowData = string.Join(" \t| ", textBlocks);
            CopyToClipboard(rowData);
            ShowCopyTooltip(item, "Row Copied!");
        }
    }

    public static void CopyToClipboard(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        try
        {
            Clipboard.SetDataObject(text, true);
        }
        catch
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch
            {
                // Ignore locked clipboard conflicts
            }
        }
    }

    public static void ShowCopyTooltip(FrameworkElement element, string message)
    {
        try
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 4, 10, 4)
            };
            var text = new TextBlock
            {
                Text = $"✓ {message}",
                Foreground = new SolidColorBrush(Color.FromRgb(52, 211, 153)), // Emerald green
                FontSize = 12,
                FontWeight = FontWeights.Bold
            };
            border.Child = text;

            var popup = new Popup
            {
                Child = border,
                PlacementTarget = element,
                Placement = PlacementMode.Top,
                AllowsTransparency = true,
                IsOpen = true
            };

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1100) };
            timer.Tick += (s, e) =>
            {
                popup.IsOpen = false;
                timer.Stop();
            };
            timer.Start();
        }
        catch
        {
            // Fallback: silently proceed if popup cannot be rendered
        }
    }

    private static bool IsInsideInteractiveControl(DependencyObject element)
    {
        var current = element;
        while (current != null)
        {
            if (current is ButtonBase
                || current is MenuItem
                || current is ComboBox
                || current is ScrollBar
                || current is TextBox
                || current is PasswordBox)
            {
                return true;
            }

            if (current is Visual || current is System.Windows.Media.Media3D.Visual3D)
                current = VisualTreeHelper.GetParent(current);
            else if (current is FrameworkContentElement fce)
                current = fce.Parent;
            else
                break;
        }
        return false;
    }

    private static T? FindVisualParent<T>(DependencyObject? element) where T : DependencyObject
    {
        while (element != null)
        {
            if (element is T match) return match;

            if (element is Visual || element is System.Windows.Media.Media3D.Visual3D)
                element = VisualTreeHelper.GetParent(element);
            else if (element is FrameworkContentElement fce)
                element = fce.Parent;
            else
                break;
        }
        return null;
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj == null) yield break;

        int count = VisualTreeHelper.GetChildrenCount(depObj);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);
            if (child is T t) yield return t;

            foreach (var grandchild in FindVisualChildren<T>(child))
                yield return grandchild;
        }
    }
}
