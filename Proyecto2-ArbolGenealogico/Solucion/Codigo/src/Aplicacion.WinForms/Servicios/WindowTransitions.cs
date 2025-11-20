using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aplicacion.WinForms.Servicios
{
    public static class WindowTransitions
    {
        // Helper to invoke an action on the control's UI thread if required.
        private static void InvokeOnUi(Control control, Action action)
        {
            if (control == null || action == null) return;
            try
            {
                if (control.IsDisposed) return;
                if (control.InvokeRequired)
                {
                    control.Invoke((MethodInvoker)(() => action()));
                }
                else
                {
                    action();
                }
            }
            catch { }
        }
        // Fade in a form over the specified duration (ms) using a UI Timer (safe for message loop)
        public static void FadeIn(Form f, int durationMs = 250)
        {
            if (f == null) return;
            try
            {
                if (f.IsDisposed) return;
                InvokeOnUi(f, () =>
                {
                    f.Opacity = 0.0;
                    if (!f.Visible) f.Show();
                    int steps = Math.Max(4, durationMs / 15);
                    int interval = Math.Max(10, durationMs / steps);
                    double delta = 1.0 / steps;
                    var timer = new System.Windows.Forms.Timer { Interval = interval };
                    timer.Tick += (s, e) =>
                    {
                        try
                        {
                            f.Opacity = Math.Min(1.0, f.Opacity + delta);
                            if (f.Opacity >= 1.0 - 0.001)
                            {
                                timer.Stop();
                                timer.Dispose();
                                f.Opacity = 1.0;
                            }
                        }
                        catch { try { timer.Stop(); timer.Dispose(); } catch { } }
                    };
                    timer.Start();
                });
            }
            catch { }
        }

        // Fade out a form over the specified duration (ms) using a UI Timer and optionally close it.
        public static void FadeOut(Form f, int durationMs = 200, bool closeAfter = false)
        {
            if (f == null) return;
            try
            {
                if (f.IsDisposed) return;
                InvokeOnUi(f, () =>
                {
                    int steps = Math.Max(4, durationMs / 15);
                    int interval = Math.Max(10, durationMs / steps);
                    double delta = 1.0 / steps;
                    var timer = new System.Windows.Forms.Timer { Interval = interval };
                    timer.Tick += (s, e) =>
                    {
                        try
                        {
                            f.Opacity = Math.Max(0.0, f.Opacity - delta);
                            if (f.Opacity <= 0.001)
                            {
                                timer.Stop();
                                timer.Dispose();
                                f.Opacity = 0.0;
                                try { if (closeAfter) f.Close(); else f.Hide(); } catch { }
                            }
                        }
                        catch { try { timer.Stop(); timer.Dispose(); } catch { } }
                    };
                    timer.Start();
                });
            }
            catch { }
        }

        // Show a new form with fade in and fade out the owner (UI-timer based, safe)
        public static void ShowFormWithFade(Form owner, Form toShow, int durationMs = 250)
        {
            try
            {
                if (owner == null || toShow == null) return;

                // If toShow will be displayed modally by caller (ShowDialog), do not run this helper.
                // This helper assumes non-modal show.

                InvokeOnUi(owner, () =>
                {
                    try
                    {
                        // Prepare target
                        toShow.Opacity = 0.0;
                        toShow.Show(owner);

                        // Fade in target, then fade out owner
                        FadeIn(toShow, durationMs);

                        // Delay starting owner fade slightly so new form can initialize
                        var delayTimer = new System.Windows.Forms.Timer { Interval = Math.Max(20, durationMs / 4) };
                        delayTimer.Tick += (s, e) =>
                        {
                            try
                            {
                                delayTimer.Stop(); delayTimer.Dispose();
                                FadeOut(owner, durationMs, closeAfter: false);
                            }
                            catch { try { delayTimer.Stop(); delayTimer.Dispose(); } catch { } }
                        };
                        delayTimer.Start();
                    }
                    catch { }
                });
            }
            catch { }
        }
    }
}
