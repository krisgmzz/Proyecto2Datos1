using System.Drawing;

namespace Aplicacion.WinForms.Servicios
{
    public class ThemePalette
    {
        public Color WindowBackground { get; init; }
        public Color Surface { get; init; }
        public Color Fore { get; init; }
        public Color Muted { get; init; }
        public Color AccentPrimary { get; init; }
        public Color AccentSecondary { get; init; }
        public string FontFamily { get; init; } = "Segoe UI";
        public float BaseFontSize { get; init; } = 9f;
        public int BorderRadius { get; init; } = 8;
    }
}
