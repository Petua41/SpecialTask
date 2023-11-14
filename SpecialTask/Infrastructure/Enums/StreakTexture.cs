namespace SpecialTask.Infrastructure.Enums
{
    public enum StreakTexture
    {
        None,
        SolidColor,
        HorizontalLines, VerticalLines, Dots, TransparentCircles,               // We can add lines (diagonal, vawes, etc.) endlessly
        HorizontalTransparentToColorGradient, HorizontalRainbow,                // We can add linear gradients endlessly
        RadialColorToTransparentGradient,                                       // We can add radial gradients endlessly
        Water                                                                   // We can add seamless textures really endlessly
    }

    internal static class TextureController
    {
        public static Dictionary<string, string> TexturesWithDescriptions { get; } = new()
        {
            { "solid", "Solid color" }, { "horizontallines", "Horizontal lines" }, { "verticallines", "Vertical lines" },
            { "horizontaltransparencygradient", "Horizontal gradient with transparent on the left and color on the right" },
            { "rainbow", "Horizontal rainbow gradient. Color is ignored" },
            { "radialtransparencygradient", "Radial gradient with color in center and transparency on the edge" },
            { "water", "Water texture. Color is ignored" }, { "dots", "Dots" }, { "holes", "Solid color with transparent round \"holes\""}
        };
    }
}
