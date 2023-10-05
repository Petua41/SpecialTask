using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialTask
{
    static class SaveLoadFacade
    {
        private static bool isSaved;

        public static bool NeedsSave
        {
            get => true;//!isSaved && SomethingDrawn();                                 // FOR TESTS
        }

        private static bool SomethingDrawn()
        {
            // TODO
            return true;
        }
    }
}
