using SpecialTask.Helpers.CommandHelpers.SaveLoad;
using System.IO;
using System.Xml.Linq;

namespace SpecialTask.Helpers.CommandHelpers
{
    class SaveLoadFacade
    {
        public string currentFilename;

        private bool isSaved = true;
        private static readonly string defaultFolder;
        private const string defaultFilename = "SpecialTaskDrawing";

        private static SaveLoadFacade? singleton;

        static SaveLoadFacade()
        {
            defaultFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private SaveLoadFacade()
        {
            CurrentWindow.SomethingDisplayed += OnSomethingDisplayed;

            currentFilename = defaultFilename + DateTime.Now.ToString().Replace(':', '.');
        }

        public static SaveLoadFacade Instance
        {
            get
            {
                singleton ??= new SaveLoadFacade();
                return singleton;
            }
        }

        public bool NeedsSave => !isSaved && CurrentWindow.Shapes.Count > 0;

        public void Save()
        {
            if (!NeedsSave) throw new InvalidOperationException();

            SaveAs(currentFilename);
        }

        public void SaveAs(string filename)
        {
            filename = CorrectFilename(filename);

            SaveXML(filename);

            FilenameChanged(filename);
        }

        public void Load(string filename)
        {
            filename = CorrectFilename(filename);

            XDocument doc = XDocument.Load(filename);
            XMLParser.Parse(doc);

            FilenameChanged(filename);
        }

        private void FilenameChanged(string newFilename)
        {
            currentFilename = newFilename;

            isSaved = true;
        }

        public static string CorrectFilename(string filename, string neededExtension = ".std")
        {
            if (!Path.IsPathFullyQualified(filename)) filename = Path.Combine(defaultFolder, filename);
            if (!Path.HasExtension(filename)) filename += neededExtension;
            return filename;
        }

        private void OnSomethingDisplayed(object? sender, EventArgs e)
        {
            isSaved = false;
        }

        private static void SaveXML(string filename)
        {
            XDocument doc = XMLGeneratorVisitor.GenerateXML(CurrentWindow.Shapes);

            StreamWriter writer = new(filename);
            // Pass on:
            //      IOException: filename contains invalid characters
            //      UnaothorizedAcessException: no permissions

            doc.Save(writer);
            writer.Close();
        }
    }
}
