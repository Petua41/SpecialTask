using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

// ALL this file is kinda YANDERE

namespace SpecialTask
{
    class SaveLoadFacade
    {
        public string currentFilename;

        private bool isSaved = true;
        private static readonly string defaultFolder;
        private const string defaultFilename = "SpecialTaskDrawing";

        private static SaveLoadFacade? singleton;

        static SaveLoadFacade()
        {
            defaultFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private SaveLoadFacade()
        {
            WindowManager.Instance.SomethingDisplayed += OnSomethingDisplayed;

            currentFilename = defaultFilename + DateTime.Now.ToString().Replace(':', '.');
        }

        public static SaveLoadFacade Instance
        {
            get
            {
                singleton ??= new SaveLoadFacade();
                return singleton;
            }
        }

        public bool NeedsSave => !isSaved && WindowManager.Instance.ShapesOnCurrentWindow.Count > 0;

        public void Save()
        {
            if (!NeedsSave) throw new InvalidOperationException();

            SaveAs(currentFilename);
        }

        public void SaveAs(string filename)
        {
            filename = CorrectFilename(filename);

            SaveXML(filename);

            FilenameChanged(filename);
        }

        public void Load(string filename)
        {
            filename = CorrectFilename(filename);

            XDocument doc = XDocument.Load(filename);
            XMLHandler.LoadFile(doc);

            FilenameChanged(filename);
        }

        private void FilenameChanged(string newFilename)
        {
            currentFilename = newFilename;

            isSaved = true;
        }

        public static string CorrectFilename(string filename, string neededExtension=".std")
        {
            if (!Path.IsPathFullyQualified(filename)) filename = Path.Combine(defaultFolder, filename);
            if (!Path.HasExtension(filename)) filename += neededExtension;
            return filename;
        }

        private void OnSomethingDisplayed(object? sender, EventArgs e)
        {
            isSaved = false;
        }

        private static void SaveXML(string filename)
        {
            XDocument doc = XMLHandler.GenerateXML(WindowManager.Instance.ShapesOnCurrentWindow);

            StreamWriter writer = new(filename);
            // Pass on:
            //      IOException: filename contains invalid characters
            //      UnaothorizedAcessException: no permissions

            doc.Save(writer);
            writer.Close();
        }
    }

    static class XMLHandler
    {
        public static XDocument GenerateXML(List<Shape> shapes)
        {
            XDocument document = new();
            XElement parent = new("shapes");

            foreach (Shape shape in shapes)
            {
                try { parent.Add(XMLGeneratorVisitor.Visit(shape)); }
                catch (Exception ex) when (ex is UnknownShapeException or InvalidOperationException) { /* Ignore unknown shapes and hanging decorators */ }
            }

            document.Add(parent);
            return document;
        }

        public static void LoadFile(XDocument document)
        {
            XMLParser.Parse(document);
        }
    }

    static class XMLGeneratorVisitor
    {
        public static XElement Visit(Shape shape)
        {
            Dictionary<string, object> shapeAttributes = shape.Accept();

            if (shape is Circle) return VisitCircle(shapeAttributes);
            else if (shape is Square) return VisitSquare(shapeAttributes);
            else if (shape is Line) return VisitLine(shapeAttributes);
            else if (shape is StreakDecorator) return VisitStreakDecorator(shapeAttributes);
            else if (shape is Text) return VisitText(shapeAttributes);
            else if (shape is Polygon) return VisitPolygon(shapeAttributes);

            throw new UnknownShapeException();
        }

        private static XElement VisitCircle(Dictionary<string, object> shapeAttrubutes)
        {
            try
            {
                string radius = shapeAttrubutes["radius"].ToString() ?? "0";
                string centerX = shapeAttrubutes["centerX"].ToString() ?? "0";
                string centerY = shapeAttrubutes["centerY"].ToString() ?? "0";
                string color = shapeAttrubutes["color"].ToString() ?? "none";
                string lineThickness = shapeAttrubutes["lineThickness"].ToString() ?? "0";

                return GenerateXML("circle", new()
                    { { "radius", radius }, { "centerX", centerX }, { "centerY", centerY }, { "color", color }, { "lineThickness", lineThickness } });
            }
            catch (Exception ex) when (ex is InvalidCastException or KeyNotFoundException) { throw new VisitorInvalidAcceptError(); }
        }

        private static XElement VisitSquare(Dictionary<string, object> shapeAttrubutes)
        {
            try
            {
                string leftTopX = shapeAttrubutes["leftTopX"].ToString() ?? "0";
                string leftTopY = shapeAttrubutes["leftTopY"].ToString() ?? "0";
                string rightBottomX = shapeAttrubutes["rightBottomX"].ToString() ?? "0";
                string rightBottomY = shapeAttrubutes["rightBottomY"].ToString() ?? "0";
                string color = shapeAttrubutes["color"].ToString() ?? "none";
                string lineThickness = shapeAttrubutes["lineThickness"].ToString() ?? "0";

                return GenerateXML("square", new()
                {
                    { "leftTopX", leftTopX }, { "leftTopY", leftTopY }, { "rightBottomX", rightBottomX }, { "rightBottomY", rightBottomY },
                    { "color", color }, { "lineThickness", lineThickness }
                });
            }
            catch (Exception ex) when (ex is InvalidCastException or KeyNotFoundException) { throw new VisitorInvalidAcceptError(); }
        }

        private static XElement VisitLine(Dictionary<string, object> shapeAttrubutes)
        {
            try
            {
                string firstX = shapeAttrubutes["firstX"].ToString() ?? "0";
                string firstY = shapeAttrubutes["firstY"].ToString() ?? "0";
                string secondX = shapeAttrubutes["secondX"].ToString() ?? "0";
                string secondY = shapeAttrubutes["secondY"].ToString() ?? "0";
                string color = shapeAttrubutes["color"].ToString() ?? "none";
                string lineThickness = shapeAttrubutes["lineThickness"].ToString() ?? "0";

                return GenerateXML("line", new()
                {
                    { "firstX", firstX }, { "firstY", firstY }, { "secondX", secondX }, { "secondY", secondY }, { "color", color },
                    { "lineThickness", lineThickness }
                });
            }
            catch (Exception ex) when (ex is InvalidCastException or KeyNotFoundException) { throw new VisitorInvalidAcceptError(); }
        }

        private static XElement VisitStreakDecorator(Dictionary<string, object> shapeAttrubutes)
        {
            try
            {
                XElement elem = Visit((Shape)shapeAttrubutes["decoratedShape"]);

                string streakColor = shapeAttrubutes["streakColor"].ToString() ?? "none";
                string streakTexture = shapeAttrubutes["streakTexture"].ToString() ?? "none";

                XElement newElem = GenerateXML("decorator", new() 
                    { { "streak", "true" }, { "streakColor", streakColor }, { "streakTexture", streakTexture } });

                elem.Add(newElem.Attributes().ToArray());
                return elem;
            }
            catch (Exception ex) when (ex is InvalidCastException or KeyNotFoundException) { throw new VisitorInvalidAcceptError(); }
        }

        private static XElement VisitText(Dictionary<string, object> shapeAttrubutes)
        {
            try
            {
                string leftTopX = shapeAttrubutes["leftTopX"].ToString() ?? "0";
                string leftTopY = shapeAttrubutes["leftTopY"].ToString() ?? "0";
                string fontSize = shapeAttrubutes["fontSize"].ToString() ?? "0";
                string textValue = shapeAttrubutes["textValue"].ToString() ?? "";
                string color = shapeAttrubutes["color"].ToString() ?? "none";

                return GenerateXML("text", new()
                    { { "leftTopX", leftTopX }, { "leftTopY", leftTopY }, { "fontSize", fontSize }, { "textValue", textValue }, { "color", color } });
            }
            catch (Exception ex) when (ex is InvalidCastException or KeyNotFoundException) { throw new VisitorInvalidAcceptError(); }
        }

        private static XElement VisitPolygon(Dictionary<string, object> shapeAttrubutes)
        {
            try
            {
                string points = ((List<Point>)shapeAttrubutes["points"]).PointsToString();
                string lineThickness = shapeAttrubutes["lineThickness"].ToString() ?? "0";
                string color = shapeAttrubutes["color"].ToString() ?? "none";

                return GenerateXML("polygon", new() { { "points", points }, { "lineThickness", lineThickness }, { "color", color } });
            }
            catch (Exception ex) when (ex is InvalidCastException or KeyNotFoundException) { throw new VisitorInvalidAcceptError(); }
        }

        private static XElement GenerateXML(string tag, Dictionary<string, string> nameValuePairs)
        {
            XName name = XName.Get(tag);
            XElement element = new(name);

            element.Add((from kvp in nameValuePairs select new XAttribute(kvp.Key, kvp.Value)).ToArray());

            return element;
        }
    }

    static class XMLParser
    {
        public static void Parse(XDocument doc)
        {
            XElement root = doc.Root ?? throw new LoadXMLError();
            foreach (XElement elem in root.Elements())
            {
                try { ParseElement(elem); }
                catch (UnknownShapeException) { /* Ignore unknown tags */ }
            }
        }

        private static void ParseElement(XElement elem)
        {
            Dictionary<string, string> dict = new(from attr in elem.Attributes() select new KeyValuePair<string, string>(attr.Name.LocalName, attr.Value));

            ParseShape(dict, elem.Name.LocalName);
        }

        private static void ParseShape(Dictionary<string, string> dict, string shapeType)
        {
            try                 // it`s like Template method (general behaviour taken out here)
            {
                Shape shape = shapeType switch
                {
                    "circle" => ParseCircle(dict),
                    "square" => ParseSquare(dict),
                    "line" => ParseLine(dict),
                    "text" => ParseText(dict),
                    "polygon" => ParsePolygon(dict),
                    _ => throw new UnknownShapeException()
                };

                if (dict.ContainsKey("streak") && dict["streak"] == "true") ParseStreakDecorator(dict, shape);
            }
            catch (Exception ex) when (ex is FormatException or KeyNotFoundException) { throw new LoadXMLError(); }
        }

        private static Circle ParseCircle(Dictionary<string, string> dict)
        {
            int radius = int.Parse(dict["radius"]);
            int centerX = int.Parse(dict["centerX"]);
            int centerY = int.Parse(dict["centerY"]);
            EColor color = ColorsController.Parse(dict["color"]);
            int lineThickness = int.Parse(dict["lineThickness"]);

            return new(centerX, centerY, color, radius, lineThickness);
        }

        private static Square ParseSquare(Dictionary<string, string> dict)
        {
            int leftTopX = int.Parse(dict["leftTopX"]);
            int leftTopY = int.Parse(dict["leftTopY"]);
            int rightBottomX = int.Parse(dict["rightBottomX"]);
            int rightBottomY = int.Parse(dict["rightBottomY"]);
            EColor color = ColorsController.Parse(dict["color"]);
            int lineThickness = int.Parse(dict["lineThickness"]);

            return new(leftTopX, leftTopY, rightBottomX, rightBottomY, color, lineThickness);
        }

        private static Line ParseLine(Dictionary<string, string> dict)
        {
            int firstX = int.Parse(dict["firstX"]);
            int firstY = int.Parse(dict["firstY"]);
            int secondX = int.Parse(dict["secondX"]);
            int secondY = int.Parse(dict["secondY"]);
            EColor color = ColorsController.Parse(dict["color"]);
            int lineThickness = int.Parse(dict["lineThickness"]);

            return new(firstX, firstY, secondX, secondY, color, lineThickness);
        }

        private static Text ParseText(Dictionary<string, string> dict)
        {
            int leftTopX = int.Parse(dict["leftTopX"]);
            int leftTopY = int.Parse(dict["leftTopY"]);
            int fontSize = int.Parse(dict["fontSize"]);
            string textValue = dict["textValue"];
            EColor color = ColorsController.Parse(dict["color"]);

            return new(leftTopX, leftTopY, fontSize, textValue, color);
        }

        private static Polygon ParsePolygon(Dictionary<string, string> dict)
        {
            List<Point> points = (List<Point>)EArgumentType.Points.ParseValue(dict["points"]);
            int lineThickness = int.Parse(dict["lineThickness"]);
            EColor color = ColorsController.Parse(dict["color"]);

            return new(points, lineThickness, color);
        }

        private static void ParseStreakDecorator(Dictionary<string, string> dict, Shape shape)
        {
            try
            {
                EColor streakColor = ColorsController.Parse(dict["streakColor"]);
                EStreakTexture streakTexture = TextureController.Parse(dict["streakTexture"]);

                StreakDecorator _ = new(shape, streakColor, streakTexture);
            }
            catch (KeyNotFoundException) { throw new LoadXMLError(); }
        }
    }
}