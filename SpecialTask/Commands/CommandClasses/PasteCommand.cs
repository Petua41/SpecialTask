﻿using System.Collections.Generic;

namespace SpecialTask.Commands.CommandClasses
{
    /// <summary>
    /// Command to paste selected shapes
    /// </summary>
    class PasteCommand : ICommand
    {
        private readonly int leftTopX;
        private readonly int leftTopY;

        private List<Shape> pastedShapes = new();

        public PasteCommand(object[] args)
        {
            leftTopX = (int)args[0];
            leftTopY = (int)args[1];
        }

        public void Execute()
        {
            pastedShapes = SelectPasteHandler.PasteArea(leftTopX, leftTopY);
        }

        public void Unexecute()
        {
            foreach (Shape shape in pastedShapes) shape.Destroy();
        }
    }
}
