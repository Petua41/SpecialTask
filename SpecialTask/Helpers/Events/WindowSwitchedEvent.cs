﻿using System;

namespace SpecialTask.Helpers
{
    public class WindowSwitchedEventArgs : EventArgs
    {
        public WindowSwitchedEventArgs(int newNumber)
        {
            NewNumber = newNumber;
        }

        public int NewNumber { get; set; }
    }

    public delegate void WindowSwitchedEventHandler(object sender, WindowSwitchedEventArgs args);
}
