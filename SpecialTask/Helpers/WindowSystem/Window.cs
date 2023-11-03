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

        // TODO: this and further methods MUST be rewritten using .FindIndex
        public int SendBackward(string uniqueName)
        {
            if (allShapesOnThisWindow.Count <= 1) throw new InvalidOperationException();

            for (int i = 0; i < allShapesOnThisWindow.Count; i++)
            {
                if (uniqueName == allShapesOnThisWindow[i].UniqueName)
                {
                    if (i == 0) throw new InvalidOperationException();

                    (zOrder[i], zOrder[i - 1]) = (zOrder[i - 1], zOrder[i]);

                    int firstCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[i].WPFShape);
                    int secondCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[i - 1].WPFShape);

                    Panel.SetZIndex(allShapesOnThisWindow[i].WPFShape, secondCanvasZ);
                    Panel.SetZIndex(allShapesOnThisWindow[i - 1].WPFShape, firstCanvasZ);

                    return i;
                }
            }
            throw new ShapeNotFoundException();
        }

        public int BringForward(string uniqueName)
        {
            if (allShapesOnThisWindow.Count <= 1) throw new InvalidOperationException();

            for (int i = 0; i < allShapesOnThisWindow.Count; i++)
            {
                if (uniqueName == allShapesOnThisWindow[i].UniqueName)
                {
                    if (i == allShapesOnThisWindow.Count - 1) throw new InvalidOperationException();

                    (zOrder[i], zOrder[i + 1]) = (zOrder[i + 1], zOrder[i]);

                    int firstCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[i].WPFShape);
                    int secondCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[i + 1].WPFShape);

                    Panel.SetZIndex(allShapesOnThisWindow[i].WPFShape, secondCanvasZ);
                    Panel.SetZIndex(allShapesOnThisWindow[i + 1].WPFShape, firstCanvasZ);

                    return i;
                }
            }
            throw new ShapeNotFoundException();
        }

        public int SendToBack(string uniqueName)
        {
            if (allShapesOnThisWindow.Count <= 1) throw new InvalidOperationException();

            for (int i = 0; i < allShapesOnThisWindow.Count; i++)
            {
                if (uniqueName == allShapesOnThisWindow[i].UniqueName)
                {
                    for (int j = i; j > 0; j--) SendBackward(allShapesOnThisWindow[j].UniqueName);
                    return i;
                }
            }
            throw new ShapeNotFoundException();
        }

        public int BringToFront(string uniqueName)
        {
            if (allShapesOnThisWindow.Count <= 1) throw new InvalidOperationException();

            for (int i = 0; i < allShapesOnThisWindow.Count; i++)
            {
                if (uniqueName == allShapesOnThisWindow[i].UniqueName)
                {
                    for (int j = i; j < allShapesOnThisWindow.Count - 1; j++) BringForward(allShapesOnThisWindow[j].UniqueName);
                    return i;
                }
            }
            throw new ShapeNotFoundException();
        }

        public void MoveToLayer(string uniqueName, int newLayer)
        {
            if (allShapesOnThisWindow.Count <= 1) throw new InvalidOperationException();

            if (newLayer < 0 || newLayer > allShapesOnThisWindow.Count) throw new InvalidOperationException();

            for (int i = 0; i < allShapesOnThisWindow.Count; i++)
            {
                if (uniqueName == allShapesOnThisWindow[i].UniqueName)
                {
                    (zOrder[i], zOrder[newLayer]) = (zOrder[newLayer], zOrder[i]);

                    int firstCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[i].WPFShape);
                    int secondCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[newLayer].WPFShape);

                    Panel.SetZIndex(allShapesOnThisWindow[i].WPFShape, secondCanvasZ);
                    Panel.SetZIndex(allShapesOnThisWindow[newLayer].WPFShape, firstCanvasZ);

                    return;
                }
            }
            throw new ShapeNotFoundException();
        }

        private void RemoveShapeFromLists(string uniqueName)
        {
            int index = -1;
            for (int i = 0; i < allShapesOnThisWindow.Count; i++)
            {
                if (allShapesOnThisWindow[i].UniqueName == uniqueName)
                {
                    index = i;
                    break;
                }
            }
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
