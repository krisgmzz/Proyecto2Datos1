using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Krypton.Toolkit;

namespace Aplicacion.WinForms.Servicios
{
    public static class ThemeManager
    {
        public static ThemePalette Light { get; } = new ThemePalette
        {
            WindowBackground = Color.FromArgb(250, 250, 252),
            Surface = Color.White,
            Fore = Color.FromArgb(33, 37, 41),
            Muted = Color.FromArgb(102, 110, 116),
            AccentPrimary = Color.FromArgb(0, 120, 215), // modern Windows blue
            AccentSecondary = Color.FromArgb(0, 153, 136),
            FontFamily = "Segoe UI",
            BaseFontSize = 10f,
            BorderRadius = 6
        };

        public static ThemePalette Dark { get; } = new ThemePalette
        {
            WindowBackground = Color.FromArgb(18, 18, 20),
            Surface = Color.FromArgb(28, 28, 30),
            Fore = Color.FromArgb(235, 235, 238),
            Muted = Color.FromArgb(150, 150, 160),
            AccentPrimary = Color.FromArgb(90, 150, 255),
            AccentSecondary = Color.FromArgb(90, 200, 140),
            FontFamily = "Segoe UI",
            BaseFontSize = 10f,
            BorderRadius = 6
        };

        public static ThemePalette Current { get; private set; } = Light;

        // Apply a theme globally. If Krypton global palette is available, use it; otherwise we style controls recursively.
        public static void ApplyTheme(ThemePalette palette)
        {
            if (palette == null) return;
            // Ensure we run UI work on the UI thread
            try
            {
                if (Application.OpenForms.Count > 0)
                {
                    var main = Application.OpenForms[0];
                    if (main != null && main.InvokeRequired)
                    {
                        main.Invoke(new Action(() => ApplyThemeInternal(palette)));
                        return;
                    }
                }
            }
            catch { }

            ApplyThemeInternal(palette);
        }

        private static void ApplyThemeInternal(ThemePalette palette)
        {
            if (palette == null) return;
            Current = palette;

            // Create and populate a KryptonPalette from our ThemePalette and attempt to assign it to Krypton-aware controls/forms.
            try
            {
                var kp = CreateKryptonPaletteFromTheme(palette);
                // Attempt to assign to any control/form that exposes a `Palette` property (best-effort via reflection)
                try
                {
                    foreach (Form f in Application.OpenForms)
                    {
                        try
                        {
                            var fi = f.GetType().GetProperty("Palette");
                            if (fi != null && fi.CanWrite)
                            {
                                try { fi.SetValue(f, kp); } catch { }
                            }

                            // Assign to direct child controls as well where possible
                            foreach (Control c in f.Controls)
                            {
                                try
                                {
                                    var pi = c.GetType().GetProperty("Palette");
                                    if (pi != null && pi.CanWrite)
                                    {
                                        try { pi.SetValue(c, kp); } catch { }
                                    }
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }
            catch { }

            // Reset per-form applied flags so we force re-application of the theme
            foreach (Form f in Application.OpenForms)
            {
                try { f.Tag = null; } catch { }
            }

            // Apply to existing open forms
            foreach (Form f in Application.OpenForms)
            {
                ApplyToForm(f, palette);
                try { f.Tag = _themeFlag; } catch { }
            }

            // Ensure future forms created at runtime get themed as well: use Application.Idle to check for newly opened forms
            EnsureIdleHandler();
        }

        private static readonly string _themeFlag = "__theme_applied_v1";
        private static bool _idleRegistered = false;

        private static void EnsureIdleHandler()
        {
            if (_idleRegistered) return;
            Application.Idle += Application_Idle_ApplyTheme;
            _idleRegistered = true;
        }

        private static void Application_Idle_ApplyTheme(object? sender, EventArgs e)
        {
            try
            {
                foreach (Form f in Application.OpenForms)
                {
                    try
                    {
                        if (f.Tag == null || !f.Tag.Equals(_themeFlag))
                        {
                            ApplyToForm(f, Current);
                            f.Tag = _themeFlag;
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private static void ApplyToForm(Form f, ThemePalette p)
        {
            if (f == null || p == null) return;
            try
            {
                f.BackColor = p.WindowBackground;
                f.ForeColor = p.Fore;
                f.Font = new Font(p.FontFamily, p.BaseFontSize);

                // Do not change form Region (title bar / window chrome) - leave OS-managed.

                ApplyToControls(f.Controls, p);
                // Ensure form and its important controls repaint using the new theme
                try
                {
                    f.Invalidate(true);
                    f.Refresh();
                    // Force TabControls to redraw their headers
                    foreach (Control c in f.Controls)
                    {
                        try { InvalidateIfTabControlRecursive(c); } catch { }
                    }
                }
                catch { }
            }
            catch { }
        }

        private static void InvalidateIfTabControlRecursive(Control c)
        {
            if (c == null) return;
            try
            {
                if (c is TabControl tc)
                {
                    try { tc.Invalidate(); tc.Refresh(); }
                    catch { }
                }
                if (c.HasChildren)
                {
                    foreach (Control child in c.Controls)
                    {
                        try { InvalidateIfTabControlRecursive(child); } catch { }
                    }
                }
            }
            catch { }
        }

        private static void ApplyToControls(Control.ControlCollection controls, ThemePalette p)
        {
            foreach (Control c in controls)
            {
                try
                {
                    // Krypton-aware controls
                    if (c is KryptonButton kb)
                    {
                        try
                        {
                            // Ensure text sources are synchronized and no image overlays
                            try
                            {
                                var vt = string.Empty;
                                try { vt = kb.Values.Text ?? string.Empty; } catch { }
                                if (string.IsNullOrEmpty(vt) && !string.IsNullOrEmpty(kb.Text))
                                {
                                    try { kb.Values.Text = kb.Text; } catch { }
                                }
                                else if (string.IsNullOrEmpty(kb.Text) && !string.IsNullOrEmpty(vt))
                                {
                                    try { kb.Text = vt; } catch { }
                                }
                                // Remove any image that might overlay text
                                try { if (kb.Values.Image != null) kb.Values.Image = null; } catch { }
                            }
                            catch { }

                            // Compute readable text color for the button background
                            Color textColor = ChooseTextColorForBackground(p.AccentPrimary);

                            // Set common and state-specific palettes so Krypton paints both background and text
                            try
                            {
                                // Apply rounded corners to borders where Krypton exposes rounding
                                try { kb.StateCommon.Border.Rounding = p.BorderRadius; } catch { }
                                try { kb.StateNormal.Border.Rounding = p.BorderRadius; } catch { }
                                try { kb.StatePressed.Border.Rounding = p.BorderRadius; } catch { }
                                try { kb.StateTracking.Border.Rounding = p.BorderRadius; } catch { }

                                kb.StateCommon.Back.Color1 = p.AccentPrimary;
                                kb.StateCommon.Back.Color2 = p.AccentPrimary;
                                kb.StateCommon.Border.Color1 = ControlPaint.Dark(p.AccentPrimary);
                                kb.StateCommon.Border.Color2 = ControlPaint.Dark(p.AccentPrimary);

                                // Use a slightly smaller vertical padding to avoid clipping
                                kb.StateCommon.Content.Padding = new Padding(6, 4, 6, 4);
                                kb.StateCommon.Content.ShortText.Font = new Font(p.FontFamily, p.BaseFontSize + 1.0f, FontStyle.Bold);
                                kb.StateCommon.Content.ShortText.Color1 = textColor;
                                kb.StateCommon.Content.ShortText.Color2 = textColor;
                                try { kb.StateCommon.Content.LongText.Color1 = textColor; kb.StateCommon.Content.LongText.Color2 = textColor; } catch { }

                                // Normal/Pressed/Tracking states
                                try { kb.StateNormal.Back.Color1 = p.AccentPrimary; kb.StateNormal.Back.Color2 = p.AccentPrimary; kb.StateNormal.Content.ShortText.Color1 = textColor; kb.StateNormal.Content.ShortText.Color2 = textColor; } catch { }
                                try { kb.StatePressed.Back.Color1 = ControlPaint.Dark(p.AccentPrimary); kb.StatePressed.Back.Color2 = ControlPaint.Dark(p.AccentPrimary); kb.StatePressed.Content.ShortText.Color1 = textColor; kb.StatePressed.Content.ShortText.Color2 = textColor; } catch { }
                                try { kb.StateTracking.Back.Color1 = ControlPaint.Light(p.AccentPrimary); kb.StateTracking.Back.Color2 = ControlPaint.Light(p.AccentPrimary); kb.StateTracking.Content.ShortText.Color1 = textColor; kb.StateTracking.Content.ShortText.Color2 = textColor; } catch { }

                                // Ensure alignment (center) where possible
                                try { kb.StateCommon.Content.ShortText.TextH = PaletteRelativeAlign.Center; kb.StateCommon.Content.ShortText.TextV = PaletteRelativeAlign.Center; } catch { }
                            }
                            catch { }
                        }
                        catch
                        {
                            try { kb.BackColor = p.AccentPrimary; kb.ForeColor = p.Surface; kb.Font = new Font(p.FontFamily, p.BaseFontSize + 1.0f, FontStyle.Bold); } catch { }
                        }
                    }
                    else if (c is KryptonPanel kp)
                    {
                        kp.BackColor = p.Surface;
                        kp.ForeColor = p.Fore;
                        kp.Font = new Font(p.FontFamily, p.BaseFontSize);
                    }
                    else if (c is KryptonLabel kw)
                    {
                        kw.ForeColor = p.Fore;
                        kw.Font = new Font(p.FontFamily, p.BaseFontSize + 1.0f, FontStyle.Bold);
                    }
                    // Standard WinForms controls
                    else if (c is Button b)
                    {
                        try { b.UseVisualStyleBackColor = false; } catch { }
                        try { b.FlatStyle = FlatStyle.Flat; } catch { }
                        try { b.Padding = new Padding(8, 6, 8, 6); } catch { }
                        try { b.Font = new Font(p.FontFamily, p.BaseFontSize + 1.0f, FontStyle.Regular); } catch { }
                        try { b.TextAlign = ContentAlignment.MiddleCenter; } catch { }

                        // Use computed readable text color over accent background
                        try
                        {
                            var textColor = ChooseTextColorForBackground(p.AccentPrimary);
                            b.BackColor = p.AccentPrimary;
                            b.ForeColor = textColor;
                            try { b.FlatAppearance.BorderColor = ControlPaint.Dark(p.AccentPrimary); } catch { }
                            try { b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(p.AccentPrimary); } catch { }
                            try { b.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(p.AccentPrimary); } catch { }
                            // Ensure minimum height so padding doesn't clip
                            try { if (b.Height < 28) b.Height = 28; } catch { }
                        }
                        catch
                        {
                            b.BackColor = p.AccentPrimary;
                            try { b.ForeColor = p.Surface; } catch { }
                        }
                    }
                    else if (c is Label l)
                    {
                        l.ForeColor = p.Fore;
                        l.BackColor = Color.Transparent;
                        l.Font = new Font(p.FontFamily, p.BaseFontSize);
                    }
                    else if (c is Panel panel)
                    {
                        panel.BackColor = p.Surface;
                        panel.ForeColor = p.Fore;
                        // Apply a rounded region to plain panels for a softer look (best-effort)
                        try
                        {
                            if (panel.Width > 0 && panel.Height > 0)
                            {
                                var gp = RoundedRect(new System.Drawing.Rectangle(0, 0, panel.Width, panel.Height), p.BorderRadius);
                                panel.Region = new System.Drawing.Region(gp);
                            }
                        }
                        catch { }
                    }
                    else if (c is TabControl tc)
                    {
                        tc.BackColor = p.Surface;
                        tc.ForeColor = p.Fore;
                        // Use owner-draw to ensure tab headers respect the theme
                        try
                        {
                            tc.DrawMode = TabDrawMode.OwnerDrawFixed;
                            // remove existing to avoid duplicate subscriptions
                            tc.DrawItem -= TabControl_DrawItem;
                            tc.DrawItem += TabControl_DrawItem;
                        }
                        catch { }

                        foreach (TabPage tp in tc.TabPages)
                        {
                            tp.BackColor = p.Surface;
                            tp.ForeColor = p.Fore;
                        }
                    }
                    else if (c is ListBox lb)
                    {
                        lb.BackColor = p.Surface;
                        lb.ForeColor = p.Fore;
                    }
                    else if (c is ListView lv)
                    {
                        lv.BackColor = p.Surface;
                        lv.ForeColor = p.Fore;
                    }
                    else if (c is DataGridView dgv)
                    {
                        dgv.BackgroundColor = p.Surface;
                        dgv.DefaultCellStyle.BackColor = p.Surface;
                        dgv.DefaultCellStyle.ForeColor = p.Fore;
                        try { dgv.ColumnHeadersDefaultCellStyle.BackColor = p.Surface; dgv.ColumnHeadersDefaultCellStyle.ForeColor = p.Fore; } catch { }
                    }
                    else if (c is TextBox tb)
                    {
                        tb.BackColor = p.Surface;
                        tb.ForeColor = p.Fore;
                    }
                    else if (c is ComboBox cb)
                    {
                        cb.BackColor = p.Surface;
                        cb.ForeColor = p.Fore;
                    }
                    else
                    {
                        // If the control provides a custom AplicarTema(bool oscuro) method, call it so custom drawing controls can update internal state
                        try
                        {
                            var mi = c.GetType().GetMethod("AplicarTema", new Type[] { typeof(bool) });
                            if (mi != null)
                            {
                                bool oscuro = object.ReferenceEquals(p, Dark);
                                try { mi.Invoke(c, new object[] { oscuro }); } catch { }
                            }
                            else
                            {
                                c.BackColor = p.Surface;
                                c.ForeColor = p.Fore;
                                c.Font = new Font(p.FontFamily, p.BaseFontSize);
                            }
                        }
                        catch { }
                    }
                }
                catch { }

                // Recurse into children and then force redraw
                try { if (c.HasChildren) ApplyToControls(c.Controls, p); } catch { }
                try { c.Invalidate(); c.Refresh(); } catch { }
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var gp = new GraphicsPath();
            int d = radius * 2;
            gp.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
            gp.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            gp.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            gp.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }

        private static void TabControl_DrawItem(object? sender, DrawItemEventArgs e)
        {
            try
            {
                if (sender is TabControl tc && e.Index >= 0 && e.Index < tc.TabCount)
                {
                    var g = e.Graphics;
                    var page = tc.TabPages[e.Index];
                    var rect = e.Bounds;

                    Color back = Current.Surface;
                    Color fore = Current.Fore;
                    if (e.Index == tc.SelectedIndex)
                    {
                        back = Current.AccentPrimary;
                        fore = Current.Surface;
                    }

                    using (var b = new SolidBrush(back)) g.FillRectangle(b, rect);
                    using (var fnt = new Font(Current.FontFamily, Current.BaseFontSize))
                    {
                        TextRenderer.DrawText(g, page.Text, fnt, rect, fore, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }
            }
            catch { }
        }

        private static Color ChooseTextColorForBackground(Color bg)
        {
            // Compute relative luminance to pick white or black for contrast
            double r = bg.R / 255.0;
            double g = bg.G / 255.0;
            double b = bg.B / 255.0;
            double lum = 0.2126 * r + 0.7152 * g + 0.0722 * b;
            return lum < 0.5 ? Color.White : Color.Black;
        }

        // Create a basic KryptonPalette instance mapped from our ThemePalette.
        // This is best-effort: Krypton versions may expose different properties; we set common values inside try/catch blocks.
        private static KryptonPalette CreateKryptonPaletteFromTheme(ThemePalette t)
        {
            try
            {
                var kp = new KryptonPalette();
                try
                {
                    // Forms
                    kp.FormStyles.FormMain.StateCommon.Back.Color1 = t.WindowBackground;
                    kp.FormStyles.FormMain.StateCommon.Back.Color2 = t.WindowBackground;
                    kp.FormStyles.FormMain.StateCommon.Border.Color1 = ControlPaint.Dark(t.WindowBackground);

                    // Buttons
                    kp.ButtonStyles.ButtonCommon.StateCommon.Back.Color1 = t.AccentPrimary;
                    kp.ButtonStyles.ButtonCommon.StateCommon.Back.Color2 = t.AccentPrimary;
                    kp.ButtonStyles.ButtonCommon.StateCommon.Border.Color1 = ControlPaint.Dark(t.AccentPrimary);
                    kp.ButtonStyles.ButtonCommon.StateCommon.Content.ShortText.Color1 = ChooseTextColorForBackground(t.AccentPrimary);
                    kp.ButtonStyles.ButtonCommon.StateCommon.Content.ShortText.Color2 = ChooseTextColorForBackground(t.AccentPrimary);
                    // Header / Palette general (best-effort; specific properties may not exist on all Krypton versions)
                    try
                    {
                        kp.HeaderStyles.HeaderForm.StateCommon.Back.Color1 = t.AccentPrimary;
                        kp.HeaderStyles.HeaderForm.StateCommon.Back.Color2 = ControlPaint.Light(t.AccentPrimary);
                        kp.HeaderStyles.HeaderPrimary.StateCommon.Back.Color1 = t.AccentPrimary;
                        kp.HeaderStyles.HeaderPrimary.StateCommon.Back.Color2 = ControlPaint.Light(t.AccentPrimary);
                        kp.HeaderStyles.HeaderSecondary.StateCommon.Back.Color1 = t.AccentSecondary;
                        kp.HeaderStyles.HeaderSecondary.StateCommon.Back.Color2 = ControlPaint.Light(t.AccentSecondary);
                    }
                    catch { }
                }
                catch { }
                return kp;
            }
            catch { return new KryptonPalette(); }
        }
    }
}
