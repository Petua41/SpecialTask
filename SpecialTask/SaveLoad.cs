using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialTask
{
    static class SaveLoadFacade
    {
        private static bool isSaved = true;

        public static bool NeedsSave
        {
            get => !isSaved && SomethingDrawn();
        }

        private static bool SomethingDrawn()
        {
            // TODO
            return true;        // temporary
        }

        public static void Save()
        {
            // TODO
            isSaved = true;
        }

        public static void SaveAs(string filename)
        {
            // TODO
            isSaved = true;
        }

        public static void Load(string filename)
        {
            // TODO
            isSaved = true;
        }
    }
}
