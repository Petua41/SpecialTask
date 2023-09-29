using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialTask
{
    class WPFConsole
    {
        private static WPFConsole? singleton;

        private WPFConsole()
        {
            if (singleton != null) throw new SingletonError();

        }

        public static WPFConsole Instance 
        { 
            get
            {
                singleton ??= new WPFConsole();
                return singleton;
            } 
        }

        public void Display(string message, EColor color=EColor.None)
        {
            // TODO
        }

        public void NewLine()
        {
            Display(Environment.NewLine);
        }

        public void DisplayPrompt()
        {
            Display(">> ", EColor.Green);
        }

        public int HeightInCharacterCells
        {
            get
            {
                // TODO
                throw new NotImplementedException();
            }
        }

        public int WidthInCharacterCells
        {
            get
            {
                // TODO
                throw new NotImplementedException();
            }
        }
    }
}
