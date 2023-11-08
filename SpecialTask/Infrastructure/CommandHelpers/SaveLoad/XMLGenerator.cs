using SpecialTask.Drawing;
using SpecialTask.Drawing.Shapes;
using SpecialTask.Drawing.Shapes.Decorators;
using SpecialTask.Infrastructure.Exceptions;
using System.Xml.Linq;
using static SpecialTask.Infrastructure.Extensoins.PointListExtensions;

namespace SpecialTask.Infrastructure.CommandHelpers.SaveLoad
{
    internal static class XMLGeneratorVisitor
    {
        public static XDocument GenerateXML(List<Shape> shapes)
        {
            XDocument document = new();
            XElement parent = new("shapes");

            foreach (Shape shape in shapes)
            {
                try { parent.Add(Visit(shape)); }
                catch (Exception ex) when (ex is UnknownShapeClassException or InvalidOperationException) { /* Ignore unknown shapes and hanging decorators */ }
            }

            document.Add(parent);
            return document;
        }

        public static XElement Visit(Shape shape)
        {
            Dictionary<string, object> shapeAttributes = shape.Accept();

            if (shape is Circle)
            {
                return VisitCircle(shapeAttributes);
            }
            else if (shape is Square)
            {
                return VisitSquare(shapeAttributes);
            }
            else if (shape is Line)
            {
                return VisitLine(shapeAttributes);
            }
            else if (shape is StreakDecorator)
            {
                return VisitStreakDecorator(shapeAttributes);
            }
            else if (shape is Text)
            {
                return VisitText(shapeAttributes);
            }
            else if (shape is Polygon)
            {
                return VisitPolygon(shapeAttributes);
            }

            throw new UnknownShapeClassException($"Unknown shape subclass: {typeof(Shape).Name}", typeof(Shape).Name);
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
                string textValue = shapeAttrubutes["textValue"].ToString() ?? string.Empty;
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

            element.Add(nameValuePairs.Select(kvp => new XAttribute(kvp.Key, kvp.Value)).ToArray());    // we call ToArray so that it will be passed as params but not as object

            return element;
        }
    }
}
