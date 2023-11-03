using SpecialTask.Drawing;
using SpecialTask.Exceptions;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SpecialTask.Helpers.WindowSystem
{
    internal class Window
    {
        private readonly DrawingWindow assotiatedWindow;

        private readonly List<Shape> allShapesOnThisWindow = new();     // Stored in creation-time order
        private readonly List<int> zOrder = new();                      // Indices in previous list

        public Window(int number)
        {
            assotiatedWindow = new();
            assotiatedWindow.DrawingWindowClosedEvent += OnAssotiatedWindowClosed;
            assotiatedWindow.Show();
            ChangeTitle(number);
        }

        public void ChangeTitle(int newNumber)
        {
            assotiatedWindow.ChangeTitle($"Drawing window {newNumber}");
        }

        public List<Shape> ShapesOnThisWindow => allShapesOnThisWindow;

        public void Destroy()
        {
            assotiatedWindow.Close();
        }

        public void AddShape(Shape shape)
        {
            System.Windows.Shapes.Shape wpfShape = shape.WPFShape;
            int z = allShapesOnThisWindow.Count;

            allShapesOnThisWindow.Add(shape);
            Canvas.Children.Add(wpfShape);

            zOrder.Add(z);
            Panel.SetZIndex(wpfShape, z);
        }

        public void RemoveShape(Shape shape)
        {
            if (Canvas.Children.Contains(shape.WPFShape))     // If we cannot find shape on canvas, it won`t be removed from lists
            {
                Canvas.Children.Remove(shape.WPFShape);
                RemoveShapeFromLists(shape.UniqueName);
            }
        }

        public int SendBackwards(string uniqueName)
        {
            if (allShapesOnThisWindow.Count <= 1) throw new InvalidOperationException();

            int idx = allShapesOnThisWindow.FindIndex(sh => sh.UniqueName == uniqueName);

            if (idx < 0) throw new ShapeNotFoundException();

            if (idx == 0) throw new InvalidOperationException();        // shape is already on back

            (zOrder[idx], zOrder[idx - 1]) = (zOrder[idx - 1], zOrder[idx]);

            int firstCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[idx].WPFShape);
            int secondCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[idx - 1].WPFShape);

            Panel.SetZIndex(allShapesOnThisWindow[idx].WPFShape, secondCanvasZ);
            Panel.SetZIndex(allShapesOnThisWindow[idx - 1].WPFShape, firstCanvasZ);

            return idx;
        }

        public int BringForward(string uniqueName) 
        {
            if (allShapesOnThisWindow.Count <= 1) throw new InvalidOperationException();

            int idx = allShapesOnThisWindow.FindIndex(sh => sh.UniqueName == uniqueName);

            if (idx < 0) throw new ShapeNotFoundException();

            if (idx == allShapesOnThisWindow.Count - 1) throw new InvalidOperationException();        // shape is already on top

            (zOrder[idx], zOrder[idx + 1]) = (zOrder[idx + 1], zOrder[idx]);

            int firstCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[idx].WPFShape);
            int secondCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[idx + 1].WPFShape);

            Panel.SetZIndex(allShapesOnThisWindow[idx].WPFShape, secondCanvasZ);
            Panel.SetZIndex(allShapesOnThisWindow[idx + 1].WPFShape, firstCanvasZ);

            return idx;
        }

        public int SendToBack(string uniqueName)
        {
            if (allShapesOnThisWindow.Count <= 1) throw new InvalidOperationException();

            int idx = allShapesOnThisWindow.FindIndex(sh => sh.UniqueName == uniqueName);

            if (idx < 0) throw new ShapeNotFoundException();

            for (int j = idx; j > 0; j--) SendBackwards(allShapesOnThisWindow[j].UniqueName);
            return idx;
        }

        public int BringToFront(string uniqueName)
        {
            if (allShapesOnThisWindow.Count <= 1) throw new InvalidOperationException();

            int idx = allShapesOnThisWindow.FindIndex(sh => sh.UniqueName == uniqueName);

            if (idx < 0) throw new ShapeNotFoundException();

            for (int j = idx; j < allShapesOnThisWindow.Count - 1; j++) BringForward(allShapesOnThisWindow[j].UniqueName);
            return idx;
        }

        public void MoveToLayer(string uniqueName, int newLayer)
        {
            if (allShapesOnThisWindow.Count <= 1) throw new InvalidOperationException();

            if (newLayer < 0 || newLayer > allShapesOnThisWindow.Count) throw new InvalidOperationException();

            int idx = allShapesOnThisWindow.FindIndex(sh => sh.UniqueName == uniqueName);

            if (idx < 0) throw new ShapeNotFoundException();

            (zOrder[idx], zOrder[newLayer]) = (zOrder[newLayer], zOrder[idx]);

            int firstCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[idx].WPFShape);
            int secondCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[newLayer].WPFShape);

            Panel.SetZIndex(allShapesOnThisWindow[idx].WPFShape, secondCanvasZ);
            Panel.SetZIndex(allShapesOnThisWindow[newLayer].WPFShape, firstCanvasZ);

            return;
        }

        private void RemoveShapeFromLists(string uniqueName)
        {
            int index = allShapesOnThisWindow.FindIndex(sh => sh.UniqueName == uniqueName);

            if (index >= 0)             // if cannot find, no problem
            {
                allShapesOnThisWindow.RemoveAt(index);
                zOrder.Remove(index);
            }
        }

        private void OnAssotiatedWindowClosed(object? sender, EventArgs e)
        {
            WindowManager.Instance.OnSomeAssotiatedWindowClosed(this);
        }

        public Canvas Canvas => assotiatedWindow.DrawingCanvas;
    }
}
