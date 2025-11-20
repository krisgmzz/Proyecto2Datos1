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

        // Slide transition between TabControl pages. toIndex is target tab index.
        // direction: if toIndex > fromIndex, we consider it a move to the right.
        // Returns true if the animation was started, false if fallback to direct selection should be used.
        public static bool SlideTabTransition(TabControl tab, int toIndex, int durationMs = 260)
        {
            if (tab == null) return false;
            if (toIndex < 0 || toIndex >= tab.TabCount) return false;
            try
            {
                int fromIndex = tab.SelectedIndex;
                if (fromIndex == toIndex) return false;
                if (tab.Width <= 10 || tab.Height <= 10) { tab.SelectedIndex = toIndex; return false; }

                // Try to capture bitmaps; if it fails, fall back
                Bitmap fromBmp;
                Bitmap toBmp;
                try
                {
                    fromBmp = new Bitmap(tab.Width, tab.Height);
                    toBmp = new Bitmap(tab.Width, tab.Height);
                    try { tab.TabPages[fromIndex].DrawToBitmap(fromBmp, new Rectangle(0, 0, tab.Width, tab.Height)); } catch { }
                    try { tab.TabPages[toIndex].DrawToBitmap(toBmp, new Rectangle(0, 0, tab.Width, tab.Height)); } catch { }
                }
                catch
                {
                    try { tab.SelectedIndex = toIndex; } catch { }
                    return false;
                }

                InvokeOnUi(tab, () =>
                {
                    try
                    {
                        var parent = tab.Parent ?? tab;
                        var overlay = new Panel { Parent = parent, Location = tab.Location, Size = tab.Size, BackColor = Color.Transparent, Enabled = false };
                        overlay.BringToFront();

                        var pbFrom = new PictureBox { Image = fromBmp, SizeMode = PictureBoxSizeMode.Normal, Size = tab.Size, Location = Point.Empty };
                        var pbTo = new PictureBox { Image = toBmp, SizeMode = PictureBoxSizeMode.Normal, Size = tab.Size };

                        int dir = toIndex > fromIndex ? 1 : -1; // moving right or left
                        pbTo.Left = -dir * tab.Width;

                        overlay.Controls.Add(pbFrom);
                        overlay.Controls.Add(pbTo);

                        int steps = Math.Max(4, durationMs / 15);
                        int interval = Math.Max(10, durationMs / steps);
                        double delta = (double)tab.Width / steps;

                        var timer = new System.Windows.Forms.Timer { Interval = interval };
                        timer.Tick += (s, e) =>
                        {
                            try
                            {
                                pbFrom.Left += (int)(delta * dir);
                                pbTo.Left += (int)(delta * dir);

                                bool finished = (dir > 0 && pbTo.Left >= 0) || (dir < 0 && pbTo.Left <= 0);
                                if (finished)
                                {
                                    timer.Stop();
                                    try { timer.Dispose(); } catch { }
                                    try { tab.SelectedIndex = toIndex; } catch { }
                                    try { overlay.Dispose(); } catch { }
                                }
                            }
                            catch { try { timer.Stop(); timer.Dispose(); } catch { } }
                        };
                        timer.Start();
                    }
                    catch
                    {
                        try { tab.SelectedIndex = toIndex; } catch { }
                    }
                });
                return true;
            }
            catch
            {
                try { tab.SelectedIndex = toIndex; } catch { }
                return false;
            }
        }
    }
}
