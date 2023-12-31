﻿using SpecialTask.Drawing;
using SpecialTask.Drawing.Shapes;
using SpecialTask.Drawing.Shapes.Decorators;
using SpecialTask.Infrastructure.Enums;
using SpecialTask.Infrastructure.Exceptions;
using System.Xml.Linq;
using static SpecialTask.Infrastructure.Extensoins.ArgumentTypeExtensions;
using static SpecialTask.Infrastructure.Extensoins.StringExtensions;

namespace SpecialTask.Infrastructure.CommandHelpers.SaveLoad
{
    internal static class XMLParser
    {
        public static void Parse(XDocument doc)
        {
            XElement root = doc.Root ?? throw new LoadXMLException();
            foreach (XElement elem in root.Elements())
            {
                try { ParseElement(elem); }
                catch (UnknownShapeTagException) { /* Ignore unknown tags */ }
            }
        }

        private static void ParseElement(XElement elem)
        {
            Dictionary<string, string> dict = new(elem.Attributes().Select(attr => new KeyValuePair<string, string>(attr.Name.LocalName, attr.Value)));

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
                    _ => throw new UnknownShapeTagException($"Unknown shape XML tag: {shapeType}", shapeType)
                };

                if (dict.TryGetValue("streak", out string? value) && value == "true")
                {
                    shape = ParseStreakDecorator(dict, shape);
                }

                shape.Display();
            }
            catch (Exception ex) when (ex is FormatException or KeyNotFoundException) { throw new LoadXMLException($"Error while parsing {shapeType}", ex); }
        }

        private static Circle ParseCircle(Dictionary<string, string> dict)
        {
            int radius = int.Parse(dict["radius"]);
            int centerX = int.Parse(dict["centerX"]);
            int centerY = int.Parse(dict["centerY"]);
            InternalColor color = dict["color"].ParseColor();
            int lineThickness = int.Parse(dict["lineThickness"]);

            return new(centerX, centerY, color, radius, lineThickness);
        }

        private static Square ParseSquare(Dictionary<string, string> dict)
        {
            int leftTopX = int.Parse(dict["leftTopX"]);
            int leftTopY = int.Parse(dict["leftTopY"]);
            int rightBottomX = int.Parse(dict["rightBottomX"]);
            int rightBottomY = int.Parse(dict["rightBottomY"]);
            InternalColor color = dict["color"].ParseColor();
            int lineThickness = int.Parse(dict["lineThickness"]);

            return new(leftTopX, leftTopY, rightBottomX, rightBottomY, color, lineThickness);
        }

        private static Line ParseLine(Dictionary<string, string> dict)
        {
            int firstX = int.Parse(dict["firstX"]);
            int firstY = int.Parse(dict["firstY"]);
            int secondX = int.Parse(dict["secondX"]);
            int secondY = int.Parse(dict["secondY"]);
            InternalColor color = dict["color"].ParseColor();
            int lineThickness = int.Parse(dict["lineThickness"]);

            return new(firstX, firstY, secondX, secondY, color, lineThickness);
        }

        private static Text ParseText(Dictionary<string, string> dict)
        {
            int leftTopX = int.Parse(dict["leftTopX"]);
            int leftTopY = int.Parse(dict["leftTopY"]);
            int fontSize = int.Parse(dict["fontSize"]);
            string textValue = dict["textValue"];
            InternalColor color = dict["color"].ParseColor();

            return new(leftTopX, leftTopY, fontSize, textValue, color);
        }

        private static Polygon ParsePolygon(Dictionary<string, string> dict)
        {
            List<Point> points = (List<Point>)ArgumentType.Points.ParseValue(dict["points"]);
            int lineThickness = int.Parse(dict["lineThickness"]);
            InternalColor color = dict["color"].ParseColor();

            return new(points, lineThickness, color);
        }

        private static StreakDecorator ParseStreakDecorator(Dictionary<string, string> dict, Shape shape)
        {
            try
            {
                InternalColor streakColor = dict["streakColor"].ParseColor();
                StreakTexture streakTexture = dict["streakTexture"].ParseStreakTexture();

                return new(shape, streakColor, streakTexture);
            }
            catch (KeyNotFoundException e) { throw new LoadXMLException("Error while parsing StreakDecorator", e); }    // so that it have right message (StreakDecorator but not Shape subclass)
        }
    }
}
