using System;
using System.Collections.Generic;
using SpecialTask.Commands.CommandClasses;

namespace SpecialTask.Commands
{
    static class CommandCreator
    {
        private static readonly Dictionary<string, Func<Dictionary<string, object>, ICommand>> commandCreators = new()
        {
            { "clear", x => new ClearCommand() }, { "colors", x => new ColorsCommand() },
            { "createcircle", x => new CreateCircleCommand(x.Unpack("centerX centerY color radius lineThickness streak streakColor streakTexture")) },
            { "createline", x => new CreateLineCommand(x.Unpack("firstX firstY color lineThickness streak streakColor streakTexture")) },
            { "createpolygon", x => new CreatePolygonCommand(x.Unpack("points linethickness color streak streakColor streakTexture")) },
            { "createsquare", x => new CreateSquareCommand(x.Unpack("leftTopX leftTopY rightBottomX rightBottomY color lineThickness streak streakColor streakTexture")) },
            { "createtext", x => new CreateTextCommand(x.Unpack("leftTopX leftTopY fontSize textValue color streak streakColor streakTexture")) },
            { "createwindow", x => new CreateWindowCommand() }, { "deletewindow", x => new DeleteWindowCommand(x.Unpack("number")) },
            { "edit", x => new EditCommand(x.Unpack("coordinates")) }, { "exit", x => new ExitCommand() },
            { "exportpdf", x => new ExportPDFCommand(x.Unpack("inFilename outFilename")) },
            { "exportsvg", x => new ExportSVGCommand(x.Unpack("inFilename outFilename")) },
            { "load", x => new LoadCommand(x.Unpack("filename clearScreen")) },
            { "paste", x => new PasteCommand(x.Unpack("leftTopX leftTopY")) },
            { "redo", x => new RedoCommand(x.Unpack("number")) }, { "saveas", x => new SaveAsCommand(x.Unpack("filename")) },
            { "save", x => new SaveCommand() }, { "select", x => new SelectCommand(x.Unpack("leftTopX leftTopY rightBottomX rightBottomY")) },
            { "switchwindow", x => new SwitchWindowCommand(x.Unpack("number")) }, { "textures", x => new TexturesCommand() },
            { "undo", x => new UndoCommand(x.Unpack("number")) }
        };

        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public static ICommand CreateCommand(string name, Dictionary<string, object> dict)
        {
            name = name.Trim().ToLower();

            if (commandCreators.TryGetValue(name, out Func<Dictionary<string, object>, ICommand>? creator)) return creator(dict);
            else throw new ArgumentException($"Cannot create a command of type {name}");
        }

        /// <summary>
        /// Creates an array of values, that can be passed as params.
        /// Alternative to ** in Python.
        /// </summary>
        /// <param name="dict">Dictionary, containing parameter names and values</param>
        /// <param name="keysOrder">ORDERED array of parameter names</param>
        /// <exception cref="KeyNotFoundException"></exception>
        private static object[] Unpack(this Dictionary<string, object> dict, string[] keysOrder)
        {
            object[] result = new object[keysOrder.Length];

            for (int i = 0; i < keysOrder.Length; i++)
            {
                string key = keysOrder[i];

                if (dict.TryGetValue(key, out object? value)) result[i] = value;
                else throw new KeyNotFoundException($"Key {key} not found in dictionary while unpacking it");
            }

            return result;
        }

        /// <summary>
        /// Creates an array of values, that can be passed as params.
        /// Alternative to ** in Python.
        /// </summary>
        /// <param name="dict">Dictionary, containing parameter names and values</param>
        /// <param name="keysOrder">Space-separated parameter names</param>
        /// <exception cref="KeyNotFoundException"></exception>
        private static object[] Unpack(this Dictionary<string, object> dict, string keysOrder)
        {
            return dict.Unpack(keysOrder.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }
    }
}
