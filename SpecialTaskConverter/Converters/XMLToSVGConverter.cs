using SpecialTaskConverter.Exceptions;
using System.Xml.Linq;

namespace SpecialTaskConverter.Converters
{
    internal class XMLToSVGConverter
    {
        private int nextTextureNumber = 0;

        private XElement? root;

        public XDocument Convert(XDocument document)
        {
            XDocument result = new();
            root = new("svg");
            result.Add(root);

            foreach (XElement element in document.Root?.Elements() ?? throw new STDParsingException())
            {
                try { root.Add(ParseShape(element)); }
                catch (UnknownElementException) { /* nothing happened */ }
            }

            return result;
        }

        private XElement ParseShape(XElement element)
        {
            return element.Name.LocalName switch
            {
                "circle" => ConvertCircle(element),
                "square" => ConvertSquare(element),
                "line" => ConvertLine(element),
                "text" => ConvertText(element),
                "polygon" => ConvertPolygon(element),
                _ => throw new UnknownElementException($"Unknown tag while converting to PDF: {element.Name.LocalName}", element.Name.LocalName)
            };
        }

        private XElement ConvertCircle(XElement element)
        {
            string radius = element.Attribute("radius")?.Value ?? throw new STDParsingException();
            string centerX = element.Attribute("centerX")?.Value ?? throw new STDParsingException();
            string centerY = element.Attribute("centerY")?.Value ?? throw new STDParsingException();
            string color = element.Attribute("color")?.Value ?? throw new STDParsingException();
            string lineThickness = element.Attribute("lineThickness")?.Value ?? throw new STDParsingException();


            XElement result = new("circle");
            result.Add(new XAttribute("r", $"{radius}px"));
            result.Add(new XAttribute("cx", $"{centerX}px"));
            result.Add(new XAttribute("cy", $"{centerY}px"));
            result.Add(new XAttribute("stroke", ConvertColor(color)));
            result.Add(new XAttribute("stroke-width", lineThickness));

            AddStreak(element, result);

            return result;
        }

        private XElement ConvertSquare(XElement element)
        {
            string leftTopX = element.Attribute("leftTopX")?.Value ?? throw new STDParsingException();
            string leftTopY = element.Attribute("leftTopY")?.Value ?? throw new STDParsingException();
            string rightBottomX = element.Attribute("rightBottomX")?.Value ?? throw new STDParsingException();
            string rightBottomY = element.Attribute("rightBottomY")?.Value ?? throw new STDParsingException();
            string lineThickness = element.Attribute("lineThickness")?.Value ?? throw new STDParsingException();
            string color = element.Attribute("color")?.Value ?? throw new STDParsingException();

            string width = Math.Abs(int.Parse(rightBottomX) - int.Parse(leftTopX)).ToString();
            string height = Math.Abs(int.Parse(rightBottomY) - int.Parse(leftTopY)).ToString();

            XElement result = new("rect");
            result.Add(new XAttribute("x", $"{leftTopX}px"));
            result.Add(new XAttribute("y", $"{leftTopY}px"));
            result.Add(new XAttribute("width", $"{width}px"));
            result.Add(new XAttribute("height", $"{height}px"));
            result.Add(new XAttribute("stroke", ConvertColor(color)));
            result.Add(new XAttribute("stroke-width", lineThickness));

            AddStreak(element, result);

            return result;
        }

        private static XElement ConvertLine(XElement element)
        {
            string firstX = element.Attribute("firstX")?.Value ?? throw new STDParsingException();
            string firstY = element.Attribute("firstY")?.Value ?? throw new STDParsingException();
            string secondX = element.Attribute("secondX")?.Value ?? throw new STDParsingException();
            string secondY = element.Attribute("secondY")?.Value ?? throw new STDParsingException();
            string lineThickness = element.Attribute("lineThickness")?.Value ?? throw new STDParsingException();
            string color = element.Attribute("color")?.Value ?? throw new STDParsingException();

            XElement result = new("line");
            result.Add(new XAttribute("x1", $"{firstX}px"));
            result.Add(new XAttribute("y1", $"{firstY}px"));
            result.Add(new XAttribute("x2", $"{secondX}px"));
            result.Add(new XAttribute("y2", $"{secondY}px"));
            result.Add(new XAttribute("stroke", ConvertColor(color)));
            result.Add(new XAttribute("stroke-width", lineThickness));

            return result;
        }

        private static XElement ConvertText(XElement element)
        {
            string leftTopX = element.Attribute("leftTopX")?.Value ?? throw new STDParsingException();
            string leftTopY = element.Attribute("leftTopY")?.Value ?? throw new STDParsingException();
            string textValue = element.Attribute("textValue")?.Value ?? throw new STDParsingException();
            string fontSize = element.Attribute("fontSize")?.Value ?? throw new STDParsingException();
            string color = element.Attribute("color")?.Value ?? throw new STDParsingException();

            XElement result = new("text");
            result.Add(new XAttribute("x", $"{leftTopX}"));
            result.Add(new XAttribute("y", $"{leftTopY}"));
            result.Add(new XAttribute("fill", ConvertColor(color)));
            result.Add(new XAttribute("font-size", fontSize));
            result.Add(new XAttribute("font-family", "Calibri"));
            result.Value = textValue;

            return result;
        }

        private XElement ConvertPolygon(XElement element)
        {
            string prePoints = element.Attribute("points")?.Value ?? throw new STDParsingException();
            string lineThickness = element.Attribute("lineThickness")?.Value ?? throw new STDParsingException();
            string color = element.Attribute("color")?.Value ?? throw new STDParsingException();

            List<string> prePointTuples = prePoints.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            string points = string.Join(' ', from prePtp in prePointTuples select prePtp.Replace(' ', ','));

            XElement result = new("polygon");
            result.Add(new XAttribute("points", points));
            result.Add(new XAttribute("stroke", ConvertColor(color)));
            result.Add(new XAttribute("stroke-width", lineThickness));

            AddStreak(element, result);


            return result;
        }

        private void AddStreak(XElement inElem, XElement outElem)
        {
            string? streakColor = inElem.Attribute("streakColor")?.Value;
            string? streakTexture = inElem.Attribute("streakTexture")?.Value;

            if (streakColor is null || streakTexture is null)
            {
                return;
            }

            XAttribute? streak = ConvertTexture(streakColor, streakTexture);
            if (streak is not null)
            {
                outElem.Add(streak);
            }
        }

        private XAttribute? ConvertTexture(string color, string texture)
        {
            string fill = texture switch
            {
                "SolidColor" => ConvertColor(color),
                "HorizontalLines" => $"url('#{AddGeometryPattern(HorizontalLinesPattern(color))}')",
                "VerticalLines" => $"url('#{AddGeometryPattern(VerticalLinesPattern(color))}')",
                "Dots" => $"url('#{AddGeometryPattern(DotsPattern(color))}')",
                "TransparentCircles" => $"url('#{AddGeometryPattern(HolesPattern(color))}')",
                "RadialColorToTransparentGradient" => $"url('#{AddRadialTransparencyGradient(color)}')",
                "HorizontalTransparentToColorGradient" => $"url('#{AddLinearTransparencyGradient(color)}')",
                "HorizontalRainbow" => $"url('#{AddHorizontalRainbowGradient()}')",
                "Water" => $"url('#{AddWaterTexture()}')",
                _ => string.Empty			// other values are not currently supported
            };

            return fill.Length > 0 ? new("fill", fill) : null;
        }

        private string AddRadialTransparencyGradient(string color)
        {
            string gradientName = $"Gradient_{nextTextureNumber++}";

            XElement defs = new("defs");
            root?.Add(defs);

            XElement gradElem = new("radialGradient");
            defs.Add(gradElem);
            gradElem.Add(new XAttribute("id", gradientName));

            XElement stop = new("stop");
            gradElem.Add(stop);
            stop.Add(new XAttribute("offset", "0%"));
            stop.Add(new XAttribute("stop-opacity", "0"));

            XElement stop2 = new("stop");
            gradElem.Add(stop2);
            stop2.Add(new XAttribute("offset", "100%"));
            stop2.Add(new XAttribute("stop-color", ConvertColor(color)));

            return gradientName;
        }

        private string AddLinearTransparencyGradient(string color)
        {
            string gradientName = $"Gradient_{nextTextureNumber++}";

            XElement defs = new("defs");
            root?.Add(defs);

            XElement gradElem = new("linearGradient");
            defs.Add(gradElem);
            gradElem.Add(new XAttribute("id", gradientName));

            XElement stop = new("stop");
            gradElem.Add(stop);
            stop.Add(new XAttribute("offset", "0%"));
            stop.Add(new XAttribute("stop-opacity", "0"));

            XElement stop2 = new("stop");
            gradElem.Add(stop2);
            stop2.Add(new XAttribute("offset", "100%"));
            stop2.Add(new XAttribute("stop-color", ConvertColor(color)));

            return gradientName;
        }

        private string AddHorizontalRainbowGradient()
        {
            string gradientName = $"Gradient_{nextTextureNumber++}";

            XElement defs = new("defs");
            root?.Add(defs);

            XElement gradElem = new("linearGradient");
            defs.Add(gradElem);
            gradElem.Add(new XAttribute("id", gradientName));
            gradElem.Add(new XAttribute("gradientTransform", "rotate(90)"));

            XElement stop = new("stop");
            gradElem.Add(stop);
            stop.Add(new XAttribute("offset", "0%"));
            stop.Add(new XAttribute("stop-color", "navy"));

            stop = new("stop");
            gradElem.Add(stop);
            stop.Add(new XAttribute("offset", "16%"));
            stop.Add(new XAttribute("stop-color", "blue"));

            stop = new("stop");
            gradElem.Add(stop);
            stop.Add(new XAttribute("offset", "32%"));
            stop.Add(new XAttribute("stop-color", "deepskyblue"));

            stop = new("stop");
            gradElem.Add(stop);
            stop.Add(new XAttribute("offset", "48%"));
            stop.Add(new XAttribute("stop-color", "green"));

            stop = new("stop");
            gradElem.Add(stop);
            stop.Add(new XAttribute("offset", "64%"));
            stop.Add(new XAttribute("stop-color", "yellow"));

            stop = new("stop");
            gradElem.Add(stop);
            stop.Add(new XAttribute("offset", "80%"));
            stop.Add(new XAttribute("stop-color", "orange"));

            stop = new("stop");
            gradElem.Add(stop);
            stop.Add(new XAttribute("offset", "100%"));
            stop.Add(new XAttribute("stop-color", "red"));

            return gradientName;
        }

        private string AddGeometryPattern(XElement pattern)
        {
            string patternName = $"Gradient_{nextTextureNumber++}";

            XElement defs = new("defs");
            root?.Add(defs);

            XElement patElem = new("pattern");
            defs.Add(patElem);
            patElem.Add(new XAttribute("id", patternName));
            patElem.Add(new XAttribute("viewBox", "0,0,10,10"));
            patElem.Add(new XAttribute("width", "10%"));
            patElem.Add(new XAttribute("height", "10%"));

            patElem.Add(pattern);

            return patternName;
        }

        private static XElement HorizontalLinesPattern(string color)
        {
            XElement elem = new("line");
            elem.Add(new XAttribute("x1", "0px"));
            elem.Add(new XAttribute("y1", "0px"));
            elem.Add(new XAttribute("x2", "10px"));
            elem.Add(new XAttribute("y2", "0px"));
            elem.Add(new XAttribute("stroke", ConvertColor(color)));
            elem.Add(new XAttribute("stroke-width", "1.5px"));

            return elem;
        }

        private static XElement VerticalLinesPattern(string color)
        {
            XElement elem = new("line");
            elem.Add(new XAttribute("x1", "0px"));
            elem.Add(new XAttribute("y1", "0px"));
            elem.Add(new XAttribute("x2", "0px"));
            elem.Add(new XAttribute("y2", "10px"));
            elem.Add(new XAttribute("stroke", ConvertColor(color)));
            elem.Add(new XAttribute("stroke-width", "1.5px"));

            return elem;
        }

        private static XElement DotsPattern(string color)
        {
            XElement elem = new("circle");
            elem.Add(new XAttribute("r", "0.2px"));
            elem.Add(new XAttribute("cx", "5px"));
            elem.Add(new XAttribute("cy", "5px"));
            elem.Add(new XAttribute("stroke", ConvertColor(color)));
            elem.Add(new XAttribute("fill", ConvertColor(color)));

            return elem;
        }

        private static XElement HolesPattern(string color)
        {
            XElement elem = new("path");
            elem.Add(new XAttribute("fill", ConvertColor(color)));
            elem.Add(new XAttribute("d", "M 0 0 L 0 10 L 10 10 L 10 0 L 0 0 z M 5 2 A 3 3 0 0 1 8 5 A 3 3 0 0 1 5 8 A 3 3 0 0 1 2 5 A 3 3 0 0 1 5 2 z"));

            return elem;
        }
        private string AddWaterTexture()
        {
            string url = @"https://junior3d.ru/texture/%D0%92%D0%BE%D0%B4%D0%B0/%D0%A1%D0%BF%D0%BE%D0%BA%D0%BE%D0%B9%D0%BD%D0%B0%D1%8F%D0%92%D0%BE%D0%B4%D0%B0/%D1%81%D0%BF%D0%BE%D0%BA%D0%BE%D0%B9%D0%BD%D0%B0%D1%8F-%D0%B2%D0%BE%D0%B4%D0%B0_31.jpg";
            string textureName = $"Texture_{nextTextureNumber++}";

            XElement defs = new("defs");
            root?.Add(defs);

            XElement textElem = new("pattern");
            defs.Add(textElem);
            textElem.Add(new XAttribute("viewBox", "0,0,500,500"));
            textElem.Add(new XAttribute("width", "50%"));
            textElem.Add(new XAttribute("height", "50%"));
            textElem.Add(new XAttribute("id", textureName));

            XElement imElem = new("image");
            imElem.Add(new XAttribute("href", url));
            imElem.Add(new XAttribute("width", "1034"));
            imElem.Add(new XAttribute("height", "774"));
            textElem.Add(imElem);

            return textureName;
        }

        private static string ConvertColor(string color)
        {
            return color switch
            {
                "BrightRed" => "orangered",
                "BrightGreen" => "limegreen",
                "BrightYellow" => "yellow",
                "Yellow" => "gold",
                "BrightBlue" => "royalblue",
                "BrightMagenta" => "magenta",
                "Magenta" => "orchid",
                "BrightCyan" => "aqua",
                "BrightWhite" => "white",
                "White" => "lightgray",
                _ => color.ToLower()      // other colors match
            };
        }
    }
}
