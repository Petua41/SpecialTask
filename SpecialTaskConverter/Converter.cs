using System.Xml.Linq;

namespace SpecialTaskConverter
{
    /// <summary>
    /// Converts SpecialTaskDrawing (.std) files to other formats
    /// </summary>
    public class STConverter
    {
        private readonly XDocument doc;
        private readonly XMLToSVGConverter svgConv;

        /// <summary>
        /// Creates <see cref="SpecialTaskConverter"/> from <see cref="XDocument"/>
        /// </summary>
        public STConverter(XDocument document)
        {
            doc = document;

            svgConv = new();
        }

        /// <summary>
        /// Creates <see cref="SpecialTaskConverter"/> from <see cref="StreamReader"/>
        /// </summary>
        /// <exception cref="InvalidOperationException">Invalid XML file</exception>
        public STConverter(StreamReader stream) : this(XDocument.Load(stream)) { }

        /// <summary>
        /// Loads <see cref="SpecialTaskConverter"/> from .std file
        /// </summary>
        /// <exception cref="FileNotFoundException">File not found</exception>
        /// <exception cref="DirectoryNotFoundException">Some directory in file path not found</exception>
        public STConverter(string filename) : this(new StreamReader(filename)) { }

        /// <summary>
        /// Converts to Scalable Vector Graphics (.svg)
        /// </summary>
        /// <returns><see cref="XDocument"/>, containing result</returns>
        /// <exception cref="STDParsingException">Invalid .std file</exception>
        public XDocument ToSVG()
        {
            return svgConv.Convert(doc);
        }

        /// <summary>
        /// Converts to SVG and writes to existing <see cref="Stream"/>
        /// </summary>
        /// <exception cref="STDParsingException">Invalid .std file</exception>
        public void ToSVG(Stream outStream)
        {
            ToSVG().Save(outStream);
        }

        /// <summary>
        /// Converts to SVG and writes to file
        /// </summary>
        /// <exception cref="STDParsingException">Invalid .std file</exception>
        public void ToSVG(string outFilename)
        {
            ToSVG(new FileStream(outFilename, FileMode.OpenOrCreate));
        }
    }

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
                _ => throw new UnknownElementException()
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

            if (streakColor == null || streakTexture == null) return;

            XAttribute? streak = ConvertTexture(streakColor, streakTexture);
            if (streak != null) outElem.Add(streak);
        }

        private XAttribute? ConvertTexture(string color, string texture)
        {
            return texture switch
            {
                "SolidColor" => new("fill", ConvertColor(color)),
                "RadialColorToTransparentGradient" => new("fill", $"url('#{AddRadialTransparencyGradient(ConvertColor(color))}')"),
                "HorizontalTransparentToColorGradient" => new("fill", $"url('#{AddLinearTransparenceGradient(ConvertColor(color))}')"),
                _ => null           // other values are not currently supported
            };
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

        private string AddLinearTransparenceGradient(string color)
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
                _ => color
            };
        }
    }

    public class STDParsingException : Exception { }
    internal class UnknownElementException : Exception { }
}