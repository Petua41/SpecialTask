using Aspose.Pdf;
using Aspose.Pdf.Drawing;
using Aspose.Pdf.Text;
using SpecialTaskConverter.Exceptions;
using System.Xml.Linq;

namespace SpecialTaskConverter.Converters
{
    internal class XMLToPDFConverter
    {
        private Graph? graph;
        private Page? page;

        private const double GRAPH_WIDTH = 900;
        private const double GRAPH_HEIGHT = 500;

        private static readonly Dictionary<string, uint> colorValues = new()
        {
                { "purple",  0xFF800080 }, { "black", 0xFF000000 }, { "red", 0xFFCD0000 },
                { "green", 0xFF00CD00 }, { "yellow", 0xFFCDCD00 }, { "blue", 0xFF0000EE },
                { "magenta", 0xFFCD00CD }, { "cyan", 0xFF00CDCD }, { "white", 0xFFE5E5E5 },
                { "gray", 0xFF7E7E7E }, { "brightred", 0xFFFF0000 }, { "brightgreen", 0xFF00FF00 },
                { "brightyellow", 0xFFFFFF00 }, { "brightblue", 0xFF5C5CFF }, { "brightmagenta", 0xFFFF00FF },
                { "brightcyan", 0xFF00FFFF }, { "brightwhite", 0xFFFFFFFF }
        };

        public Document Convert(XDocument doc)
        {
            Document pdf = new();
            page = pdf.Pages.Add();
            page.SetPageSize(GRAPH_WIDTH + 100, GRAPH_HEIGHT + 100);

            graph = new(GRAPH_WIDTH, GRAPH_HEIGHT)
            {
                Border = new(BorderSide.All, 1, Aspose.Pdf.Color.Black),
                Margin = new(50, 50, 50, 50)
            };

            foreach (XElement elem in doc.Root?.Elements() ?? throw new STDParsingException("Root element doesn`t contain any elements!"))
            {
                try { ConvertElement(elem); }
                catch (UnknownElementException) {  /* ingnore unknown elements */ }
            }

            page.Paragraphs.Add(graph);

            return pdf;
        }

        private void ConvertElement(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "circle":
                    graph?.Shapes.Add(ConvertCirlce(element));
                    break;
                case "square":
                    graph?.Shapes.Add(ConvertSquare(element));
                    break;
                case "line":
                    graph?.Shapes.Add(ConvertLine(element));
                    break;
                case "text":
                    ConvertText(element);
                    break;
                case "polygon":
                    graph?.Shapes.Add(ConvertPolygon(element));
                    break;
                default:
                    throw new UnknownElementException($"Unknown tag while converting to PDF: {element.Name.LocalName}", element.Name.LocalName);
            }
        }

        private static Shape ConvertCirlce(XElement element)
        {
            int radius = int.Parse(element.Attribute("radius")?.Value ?? throw new STDParsingException());
            int centerX = int.Parse(element.Attribute("centerX")?.Value ?? throw new STDParsingException());
            int centerY = int.Parse(element.Attribute("centerY")?.Value ?? throw new STDParsingException());
            string color = element.Attribute("color")?.Value ?? throw new STDParsingException();
            int lineThickness = int.Parse(element.Attribute("lineThickness")?.Value ?? throw new STDParsingException());

            Circle circle = new(centerX, (int)GRAPH_HEIGHT - centerY, radius);
            circle.GraphInfo.Color = ConvertColor(color);
            AddStreak(circle, element);
            circle.GraphInfo.LineWidth = lineThickness;

            return circle;
        }

        private static Shape ConvertSquare(XElement element)
        {
            int leftTopX = int.Parse(element.Attribute("leftTopX")?.Value ?? throw new STDParsingException());
            int leftTopY = int.Parse(element.Attribute("leftTopY")?.Value ?? throw new STDParsingException());
            int rightBottomX = int.Parse(element.Attribute("rightBottomX")?.Value ?? throw new STDParsingException());
            int rightBottomY = int.Parse(element.Attribute("rightBottomY")?.Value ?? throw new STDParsingException());
            int lineThickness = int.Parse(element.Attribute("lineThickness")?.Value ?? throw new STDParsingException());
            string color = element.Attribute("color")?.Value ?? throw new STDParsingException();

            int width = Math.Abs(rightBottomX - leftTopX);
            int height = Math.Abs(rightBottomY - leftTopY);

            Aspose.Pdf.Drawing.Rectangle rect = new(leftTopX, (int)GRAPH_HEIGHT - rightBottomY, width, height);
            rect.GraphInfo.Color = ConvertColor(color);
            AddStreak(rect, element);
            rect.GraphInfo.LineWidth = lineThickness;

            return rect;
        }

        private static Shape ConvertLine(XElement element)
        {
            int firstX = int.Parse(element.Attribute("firstX")?.Value ?? throw new STDParsingException());
            int firstY = (int)GRAPH_HEIGHT - int.Parse(element.Attribute("firstY")?.Value ?? throw new STDParsingException());
            int secondX = int.Parse(element.Attribute("secondX")?.Value ?? throw new STDParsingException());
            int secondY = (int)GRAPH_HEIGHT - int.Parse(element.Attribute("secondY")?.Value ?? throw new STDParsingException());
            int lineThickness = int.Parse(element.Attribute("lineThickness")?.Value ?? throw new STDParsingException());
            string color = element.Attribute("color")?.Value ?? throw new STDParsingException();

            float[] points = new float[4] { firstX, firstY, secondX, secondY };

            Line line = new(points);
            line.GraphInfo.Color = ConvertColor(color);
            line.GraphInfo.LineWidth = lineThickness;

            return line;
        }

        private void ConvertText(XElement element)
        {
            int leftTopX = int.Parse(element.Attribute("leftTopX")?.Value ?? throw new STDParsingException());
            int leftTopY = int.Parse(element.Attribute("leftTopY")?.Value ?? throw new STDParsingException());
            string textValue = element.Attribute("textValue")?.Value ?? throw new STDParsingException();
            int fontSize = int.Parse(element.Attribute("fontSize")?.Value ?? throw new STDParsingException());
            string color = element.Attribute("color")?.Value ?? throw new STDParsingException();

            TextFragment frag = new(textValue);
            frag.TextState.ForegroundColor = ConvertColor(color);
            frag.TextState.FontSize = fontSize;
            frag.TextState.Font = FontRepository.FindFont("Calibri");
            frag.Position = new(leftTopX, leftTopY);

            TextBuilder builder = new(page);
            builder.AppendText(frag);
        }

        private static Shape ConvertPolygon(XElement element)
        {
            string prePoints = element.Attribute("points")?.Value ?? throw new STDParsingException();
            int lineThickness = int.Parse(element.Attribute("lineThickness")?.Value ?? throw new STDParsingException());
            string color = element.Attribute("color")?.Value ?? throw new STDParsingException();

            List<string[]> prePointArrs = (from prePreP in
                                        prePoints.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                           select
                                        prePreP.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToList();

            List<float> pointsList = new();
            foreach (string[] arr in prePointArrs)
            {
                pointsList.Add(int.Parse(arr[0]));
                pointsList.Add((int)GRAPH_HEIGHT - int.Parse(arr[1]));
            }
            pointsList.Add(pointsList[0]);
            pointsList.Add(pointsList[1]);
            float[] points = pointsList.ToArray();

            Line line = new(points);
            line.GraphInfo.Color = ConvertColor(color);
            line.GraphInfo.LineWidth = lineThickness;
            AddStreak(line, element);

            return line;
        }

        private static void AddStreak(Shape shape, XElement element)
        {
            try
            {
                string texture = element.Attribute("streakTexture")?.Value ?? throw new STDParsingException();

                switch (texture)
                {
                    case "SolidColor":
                        AddSolidColorBrush(shape, element);
                        break;
                    default:
                        break;      // only solid color is currently supported
                }
            }
            catch (STDParsingException) { /* nothing happened */ }
        }

        private static void AddSolidColorBrush(Shape shape, XElement element)
        {
            try
            {
                string color = element.Attribute("streakColor")?.Value ?? throw new STDParsingException();

                shape.GraphInfo.FillColor = ConvertColor(color);
            }
            catch (STDParsingException) { /* nothing happened */ }
        }

        private static Color ConvertColor(string color)
        {
            if (colorValues.ContainsKey(color.ToLower()))
            {
                System.Drawing.Color c = System.Drawing.Color.FromArgb((int)colorValues[color.ToLower()]);
                return Color.FromRgb(c);
            }
            return Color.Transparent;
        }
    }
}
