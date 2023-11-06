﻿using SpecialTask.Infrastructure.CommandInfrastructure.SaveLoad;
using System.IO;
using System.Xml.Linq;

namespace SpecialTask.Infrastructure.CommandInfrastructure
{
    internal class SaveLoadFacade
    {
        public string currentFilename;

        private bool isSaved = true;
        private const string defaultFilename = "SpecialTaskDrawing";

        private static SaveLoadFacade? singleton;

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
            filename = PathsController.CorrectFilename(filename, PathsController.DefaultSaveDirectory, ".std");

            SaveXML(filename);

            FilenameChanged(filename);
        }

        public void Load(string filename)
        {
            filename = PathsController.CorrectFilename(filename, PathsController.DefaultSaveDirectory, ".std");

            XDocument doc = XDocument.Load(filename);
            XMLParser.Parse(doc);

            FilenameChanged(filename);
        }

        private void FilenameChanged(string newFilename)
        {
            currentFilename = newFilename;

            isSaved = true;
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