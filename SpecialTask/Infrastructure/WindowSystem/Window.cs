using SpecialTask.Drawing.Shapes;
using SpecialTask.Infrastructure.Exceptions;
using System.Windows.Controls;

namespace SpecialTask.Infrastructure.WindowSystem
{
    internal class Window
    {
        private readonly DrawingWindow assotiatedWindow;
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

        public List<Shape> ShapesOnThisWindow { get; } = new();

        public void Destroy()
        {
            assotiatedWindow.Close();
        }

        public void AddShape(Shape shape)
        {
            System.Windows.Shapes.Shape wpfShape = shape.WPFShape;
            int z = ShapesOnThisWindow.Count;

            ShapesOnThisWindow.Add(shape);
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
            if (ShapesOnThisWindow.Count <= 1)
            {
                throw new InvalidOperationException();
            }

            int idx = ShapesOnThisWindow.FindIndex(sh => sh.UniqueName == uniqueName);

            if (idx < 0)
            {
                throw new ShapeNotFoundException($"Shape \"{uniqueName}\" not found on current window", uniqueName);
            }

            if (idx == 0)
            {
                throw new InvalidOperationException();        // shape is already on back
            }

            (zOrder[idx], zOrder[idx - 1]) = (zOrder[idx - 1], zOrder[idx]);

            int firstCanvasZ = Panel.GetZIndex(ShapesOnThisWindow[idx].WPFShape);
            int secondCanvasZ = Panel.GetZIndex(ShapesOnThisWindow[idx - 1].WPFShape);

            Panel.SetZIndex(ShapesOnThisWindow[idx].WPFShape, secondCanvasZ);
            Panel.SetZIndex(ShapesOnThisWindow[idx - 1].WPFShape, firstCanvasZ);

            return idx;
        }

        public int BringForward(string uniqueName)
        {
            if (ShapesOnThisWindow.Count <= 1)
            {
                throw new InvalidOperationException();
            }

            int idx = ShapesOnThisWindow.FindIndex(sh => sh.UniqueName == uniqueName);

            if (idx < 0)
            {
                throw new ShapeNotFoundException($"Shape \"{uniqueName}\" not found on current window", uniqueName);
            }

            if (idx == ShapesOnThisWindow.Count - 1)
            {
                throw new InvalidOperationException();        // shape is already on top
            }

            (zOrder[idx], zOrder[idx + 1]) = (zOrder[idx + 1], zOrder[idx]);

            int firstCanvasZ = Panel.GetZIndex(ShapesOnThisWindow[idx].WPFShape);
            int secondCanvasZ = Panel.GetZIndex(ShapesOnThisWindow[idx + 1].WPFShape);

            Panel.SetZIndex(ShapesOnThisWindow[idx].WPFShape, secondCanvasZ);
            Panel.SetZIndex(ShapesOnThisWindow[idx + 1].WPFShape, firstCanvasZ);

            return idx;
        }

        public int SendToBack(string uniqueName)
        {
            if (ShapesOnThisWindow.Count <= 1)
            {
                throw new InvalidOperationException();
            }

            int idx = ShapesOnThisWindow.FindIndex(sh => sh.UniqueName == uniqueName);

            if (idx < 0)
            {
                throw new ShapeNotFoundException($"Shape \"{uniqueName}\" not found on current window", uniqueName);
            }

            for (int j = idx; j > 0; j--)
            {
                SendBackwards(ShapesOnThisWindow[j].UniqueName);
            }

            return idx;
        }

        public int BringToFront(string uniqueName)
        {
            if (ShapesOnThisWindow.Count <= 1)
            {
                throw new InvalidOperationException();
            }

            int idx = ShapesOnThisWindow.FindIndex(sh => sh.UniqueName == uniqueName);

            if (idx < 0)
            {
                throw new ShapeNotFoundException($"Shape \"{uniqueName}\" not found on current window", uniqueName);
            }

            for (int j = idx; j < ShapesOnThisWindow.Count - 1; j++)
            {
                BringForward(ShapesOnThisWindow[j].UniqueName);
            }

            return idx;
        }

        public void MoveToLayer(string uniqueName, int newLayer)
        {
            if (ShapesOnThisWindow.Count <= 1)
            {
                throw new InvalidOperationException();
            }

            if (newLayer < 0 || newLayer > ShapesOnThisWindow.Count)
            {
                throw new InvalidOperationException();
            }

            int idx = ShapesOnThisWindow.FindIndex(sh => sh.UniqueName == uniqueName);

            if (idx < 0)
            {
                throw new ShapeNotFoundException($"Shape \"{uniqueName}\" not found on current window", uniqueName);
            }

            (zOrder[idx], zOrder[newLayer]) = (zOrder[newLayer], zOrder[idx]);

            int firstCanvasZ = Panel.GetZIndex(ShapesOnThisWindow[idx].WPFShape);
            int secondCanvasZ = Panel.GetZIndex(ShapesOnThisWindow[newLayer].WPFShape);

            Panel.SetZIndex(ShapesOnThisWindow[idx].WPFShape, secondCanvasZ);
            Panel.SetZIndex(ShapesOnThisWindow[newLayer].WPFShape, firstCanvasZ);

            return;
        }

        private void RemoveShapeFromLists(string uniqueName)
        {
            int index = ShapesOnThisWindow.FindIndex(sh => sh.UniqueName == uniqueName);

            if (index >= 0)             // if cannot find, no problem
            {
                ShapesOnThisWindow.RemoveAt(index);
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
