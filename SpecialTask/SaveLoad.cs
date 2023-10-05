using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SpecialTask
{
    class SaveLoadFacade : IDisposable
    {
        public string currentFilename;

        private bool isSaved = true;
        private static readonly string defaultFolder;
        private const string defaultFilename = "SpecialTaskDrawing";
        private StreamWriter? writer;

        private static SaveLoadFacade? singleton;

        static SaveLoadFacade()
        {
            defaultFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private SaveLoadFacade()
        {
            if (singleton != null) throw new SingletonError();

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

        public bool NeedsSave
        {
            get => !isSaved;
        }

        public void Save()
        {
            if (!NeedsSave) throw new NothingToSaveException();

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

        public void Dispose()
        {
            singleton = null;
        }

        private void FilenameChanged(string newFilename)
        {
            currentFilename = newFilename;
            writer?.Close();
            writer = null;

            isSaved = true;
        }

        private string CorrectFilename(string filename)
        {
            if (!filename.Contains('\\')) filename = defaultFolder + "\\" + filename;
            if (!filename.EndsWith(".std") && !filename.EndsWith(".xml")) filename += ".std";
            return filename;
        }

        private void OnSomethingDisplayed(object? sender, EventArgs e)
        {
            isSaved = false;
        }

        private void SaveXML(string filename)
        {
            XDocument doc = XMLHandler.GenerateXML(WindowManager.Instance.ShapesOnCurrentWindow());

            try { writer ??= new(filename); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { throw; }
            // IOException: file contains invalid characters
            // UnaothorizedAcessException: no permissions

            doc.Save(writer);
        }

        ~SaveLoadFacade()
        {
            writer?.Close();
        }
    }

    static class XMLHandler
    {
        public const string xmlns = "some_namespace";      // it doesn`t matter, but it must be the same in all elements. Xml.Linq requires it

        public static XDocument GenerateXML(List<Shape> shapes)
        {
            XDocument document = new();
            XElement parent = new(XName.Get("shapes", xmlns));
            XMLGeneratorVisitor genVisitor = new();

            foreach (Shape shape in shapes)
            {
                if (shape is Circle circle) parent.Add(genVisitor.VisitCircle(circle));
                else if (shape is Square square) parent.Add(genVisitor.VisitSquare(square));
                else if (shape is Line line) parent.Add(genVisitor.VisitLine(line));
            }

            document.Add(parent);
            return document;
        }

        public static void LoadFile(XDocument document)
        {
            XMLParser.Parse(document);
        }
    }

    class XMLGeneratorVisitor
    {
        public XElement VisitCircle(Circle circle)
        {
            Dictionary<string, object> shapeAttrubutes = circle.Accept();
            try
            {
                try
                {
                    string radius = shapeAttrubutes["radius"].ToString() ?? "0";
                    string centerX = shapeAttrubutes["centerX"].ToString() ?? "0";
                    string centerY = shapeAttrubutes["centerY"].ToString() ?? "0";
                    string color = shapeAttrubutes["color"].ToString() ?? "none";
                    string lineThickness = shapeAttrubutes["lineThickness"].ToString() ?? "0";
                    if (radius == null || centerX == null || centerY == null || color == null || lineThickness == null)
                        throw new VisitorInvalidAcceptError();

                    return GenerateXML("circle", new()
                    {
                        { "radius", radius }, { "centerX", centerX }, { "centerY", centerY }, { "color", color }, { "lineThickness", lineThickness }
                    });
                }
                catch (Exception ex) when (ex is KeyNotFoundException or InvalidCastException) { throw new VisitorInvalidAcceptError(); }
            }
            catch (Exception ex) when (ex is InvalidOperationException or InvalidCastException) { throw new VisitorInvalidAcceptError(); }
        }

        public XElement VisitSquare(Square square)
        {
            Dictionary<string, object> shapeAttrubutes = square.Accept();
            try
            {
                try
                {
                    string leftTopX = shapeAttrubutes["leftTopX"].ToString() ?? "0";
                    string leftTopY = shapeAttrubutes["leftTopY"].ToString() ?? "0";
                    string rightBottomX = shapeAttrubutes["rightBottomX"].ToString() ?? "0";
                    string rightBottomY = shapeAttrubutes["rightBottomY"].ToString() ?? "0";
                    string color = shapeAttrubutes["color"].ToString() ?? "none";
                    string lineThickness = shapeAttrubutes["lineThickness"].ToString() ?? "0";
                    if (leftTopX == null || leftTopY == null || rightBottomX == null || rightBottomY == null || color == null || lineThickness == null)
                        throw new VisitorInvalidAcceptError();

                    return GenerateXML("square", new()
                    {
                        { "leftTopX", leftTopX }, { "leftTopY", leftTopY }, { "rightBottomX", rightBottomX }, { "rightBottomY", rightBottomY },
                        { "color", color }, { "lineThickness", lineThickness }
                    });
                }
                catch (Exception ex) when (ex is KeyNotFoundException or InvalidCastException) { throw new VisitorInvalidAcceptError(); }
            }
            catch (Exception ex) when (ex is InvalidOperationException or InvalidCastException) { throw new VisitorInvalidAcceptError(); }
        }

        public XElement VisitLine(Line line)
        {
            Dictionary<string, object> shapeAttrubutes = line.Accept();
            try
            {
                try
                {
                    string firstX = shapeAttrubutes["firstX"].ToString() ?? "0";
                    string firstY = shapeAttrubutes["firstY"].ToString() ?? "0";
                    string secondX = shapeAttrubutes["secondX"].ToString() ?? "0";
                    string secondY = shapeAttrubutes["secondY"].ToString() ?? "0";
                    string color = shapeAttrubutes["color"].ToString() ?? "none";
                    string lineThickness = shapeAttrubutes["lineThickness"].ToString() ?? "0";
                    if (firstX == null || firstY == null || secondX == null || secondY == null || color == null || lineThickness == null)
                        throw new VisitorInvalidAcceptError();

                    return GenerateXML("line", new()
                    {
                        { "firstX", firstX }, { "firstY", firstY }, { "secondX", secondX }, { "secondY", secondY }, { "color", color },
                        { "lineThickness", lineThickness }
                    });
                }
                catch (Exception ex) when (ex is KeyNotFoundException or InvalidCastException) { throw new VisitorInvalidAcceptError(); }
            }
            catch (Exception ex) when (ex is InvalidOperationException or InvalidCastException) { throw new VisitorInvalidAcceptError(); }
        }

        private XElement GenerateXML(string tag, Dictionary<string, string> nameValuePairs)
        {
            XName name = XName.Get(tag, XMLHandler.xmlns);
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
                catch (UnknownShapeWhileLoadingException) { /* Ignore unknown tags */ }
            }
		}

		private static void ParseElement(XElement elem)
		{
            Dictionary<string, string> dict = new();
            foreach (XAttribute attr in elem.Attributes())
            {
                dict.Add(attr.Name.LocalName, attr.Value);
            }

            switch (elem.Name.LocalName)
            {
                case "circle":
                    ParseCircle(dict);
                    break;
                case "square":
                    ParseSquare(dict);
                    break;
                case "line":
                    ParseLine(dict);
                    break;
                default:
                    throw new UnknownShapeWhileLoadingException();
            }
		}

		private static void ParseCircle(Dictionary<string, string> dict)
		{
            try
            {
                int radius = int.Parse(dict["radius"]);
                int centerX = int.Parse(dict["centerX"]);
                int centerY = int.Parse(dict["centerY"]);
                EColor color = ColorsController.GetColorFromString(dict["color"]);
                int lineThickness = int.Parse(dict["lineThickness"]);

                Circle _ = new(centerX, centerY, color, radius, lineThickness);
            }
            catch (Exception ex) when (ex is FormatException or KeyNotFoundException) { throw new LoadXMLError(); }
		}

        private static void ParseSquare(Dictionary<string, string> dict)
        {
            try
            {
                int leftTopX = int.Parse(dict["leftTopX"]);
                int leftTopY = int.Parse(dict["leftTopY"]);
                int rightBottomX = int.Parse(dict["rightBottomX"]);
                int rightBottomY = int.Parse(dict["rightBottomY"]);
                EColor color = ColorsController.GetColorFromString(dict["color"]);
                int lineThickness = int.Parse(dict["lineThickness"]);

                Square _ = new(leftTopX, leftTopY, rightBottomX, rightBottomY, color, lineThickness);
            }
            catch (Exception ex) when (ex is FormatException or KeyNotFoundException) { throw new LoadXMLError(); }
        }

        private static void ParseLine(Dictionary<string, string> dict)
        {
            try
            {
                int firstX = int.Parse(dict["firstX"]);
                int firstY = int.Parse(dict["firstY"]);
                int secondX = int.Parse(dict["secondX"]);
                int secondY = int.Parse(dict["secondY"]);
                EColor color = ColorsController.GetColorFromString(dict["color"]);
                int lineThickness = int.Parse(dict["lineThickness"]);

                Line _ = new(firstX, firstY, secondX, secondY, color, lineThickness);
            }
            catch (Exception ex) when (ex is FormatException or KeyNotFoundException) { throw new LoadXMLError(); }
        }
	}
}