using System.Xml.Linq;
using Aspose.Pdf;
using Aspose.Pdf.Drawing;
using Aspose.Pdf.Text;

namespace SpecialTaskConverter
{
	/// <summary>
	/// Converts SpecialTaskDrawing (.std) files to other formats
	/// </summary>
	public class STConverter
	{
		private readonly XDocument doc;
		private readonly XMLToSVGConverter svgConv;
		private readonly XMLToPDFConverter pdfConv;

		/// <summary>
		/// Creates <see cref="SpecialTaskConverter"/> from <see cref="XDocument"/>
		/// </summary>
		public STConverter(XDocument document)
		{
			doc = document;

			svgConv = new();
			pdfConv = new();
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
		/// <exception cref="STDParsingException"/>
		public XDocument ToSVG()
		{
			return svgConv.Convert(doc);
		}

		/// <summary>
		/// Converts to SVG and writes to existing <see cref="Stream"/>
		/// </summary>
		/// <exception cref="STDParsingException"/>
		public void ToSVG(Stream outStream)
		{
			ToSVG().Save(outStream);
		}

		/// <summary>
		/// Converts to SVG and writes to file
		/// </summary>
		/// <exception cref="STDParsingException"/>
		public void ToSVG(string outFilename)
		{
			ToSVG(new FileStream(outFilename, FileMode.OpenOrCreate));
		}


		/// <summary>
		/// Converts to Portable Document Format (.pdf)
		/// </summary>
		/// <returns><see cref="Report"/>, containing result</returns>
		/// <exception cref="STDParsingException"/>
		public Document ToPDF()
		{
			return pdfConv.Convert(doc);
		}

		/// <summary>
		/// Converts to PDF and writes to existing <see cref="Stream"/>
		/// </summary>
		/// <exception cref="STDParsingException"/>
		public void ToPDF(Stream outStream)
		{
			ToPDF().Save(outStream);
		}

		/// <summary>
		/// Converts to PDF and writes to file
		/// </summary>
		/// <exception cref="STDParsingException"/>
		/// <exception cref="IOException"/>
		public void ToPDF(string outFilename)
		{
			if (!File.Exists(outFilename)) { File.Create(outFilename); }
			Document pdf = ToPDF();
			pdf.Save(outFilename);
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
                _ => ""			// other values are not currently supported
            };

			if (fill.Length > 0) return new("fill", fill);
			return null;
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

	internal class XMLToPDFConverter
	{
		private Graph graph;
		private Page page;

		private const double GRAPH_WIDTH = 900;
		private const double GRAPH_HEIGHT = 500;

		private static readonly Dictionary<string, uint> colorValues = new()
			{
				{ "purple",  0xFF800080 }, { "black", 0xFF000000 }, { "red", 0xFFCD0000 },
				{ "green", 0xFF00CD00 }, { "yellow", 0xFFCDCD00 }, { "blue", 0xFF0000EE },
				{ "magenta", 0xFFCD00CD }, { "cyan", 0xFF00CDCD }, { "white", 0xFFE5E5E5 },
				{ "gray", 0x7E7E7E }, { "brightred", 0xFFFF0000 }, { "brightgreen", 0xFF00FF00 },
				{ "brightyellow", 0xFFFFFF00 }, { "brightblue", 0xFF5C5CFF }, { "brightmagenta", 0xFFFF00FF },
				{ "brightcyan", 0xFFFF0000 }, { "brightwhite", 0xFFFFFFFF }
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

			foreach (XElement elem in doc.Root?.Elements() ?? throw new STDParsingException())
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
					graph.Shapes.Add(ConvertCirlce(element));
					break;
				case "square":
					graph.Shapes.Add(ConvertSquare(element));
					break;
				case "line":
					graph.Shapes.Add(ConvertLine(element));
					break;
				case "text":
					ConvertText(element);
					break;
				case "polygon":
					graph.Shapes.Add(ConvertPolygon(element));
					break;
				default:
					throw new UnknownElementException();
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

			List<string> prePointTuples = prePoints.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
			List<string[]> prePointArrs = (from tp in prePointTuples select
										   tp.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToList();

			List<(int, int)> prePointInts = (from arr in prePointArrs select (int.Parse(arr[0]), int.Parse(arr[1]))).ToList();
			List<float> pointsList = new();
			foreach ((int, int) tp in prePointInts)
			{
				pointsList.Add(tp.Item1);
				pointsList.Add((int)GRAPH_HEIGHT - tp.Item2);
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

		private static Aspose.Pdf.Color ConvertColor(string color)
		{
			// YANDERE
			if (colorValues.ContainsKey(color.ToLower()))
			{
				System.Drawing.Color c = System.Drawing.Color.FromArgb((int)colorValues[color.ToLower()]);
				return Aspose.Pdf.Color.FromRgb(c);
			}
			return Aspose.Pdf.Color.Transparent;
		}
	}

	public class STDParsingException : Exception { }
	internal class UnknownElementException : Exception { }
}