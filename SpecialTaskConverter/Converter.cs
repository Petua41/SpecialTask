using Aspose.Pdf;
using SpecialTaskConverter.Converters;
using SpecialTaskConverter.Exceptions;
using System.Xml.Linq;

// All this file is kinda YANDERE

namespace SpecialTaskConverter
{
    /// <summary>
    /// Converts SpecialTaskDrawing (.std) files to other formats
    /// </summary>
    public class STConverter
    {
        private readonly XDocument doc;
        private readonly XMLToSVGConverter svgConv;
        private readonly XMLToPDFConverter pdfConv;

        /// <summary>
        /// Creates <see cref="SpecialTaskConverter"/> from <see cref="XDocument"/>
        /// </summary>
        public STConverter(XDocument document)
        {
            doc = document;

            svgConv = new();
            pdfConv = new();
        }

        /// <summary>
        /// Creates <see cref="SpecialTaskConverter"/> from <see cref="StreamReader"/>
        /// </summary>
        /// <exception cref="InvalidOperationException">Invalid XML file</exception>
        public STConverter(StreamReader stream) : this(XDocument.Load(stream)) { }

        /// <summary>
        /// Loads <see cref="SpecialTaskConverter"/> from .std file
        /// </summary>
        /// <exception cref="FileNotFoundException">File not found</exception>
        /// <exception cref="DirectoryNotFoundException">Some directory in file path not found</exception>
        public STConverter(string filename) : this(new StreamReader(filename)) { }

        /// <summary>
        /// Converts to Scalable Vector Graphics (.svg)
        /// </summary>
        /// <returns><see cref="XDocument"/>, containing result</returns>
        /// <exception cref="STDParsingException"/>
        public XDocument ToSVG()
        {
            return svgConv.Convert(doc);
        }

        /// <summary>
        /// Converts to SVG and writes to existing <see cref="Stream"/>
        /// </summary>
        /// <exception cref="STDParsingException"/>
        public void ToSVG(Stream outStream)
        {
            ToSVG().Save(outStream);
        }

        /// <summary>
        /// Converts to SVG and writes to file
        /// </summary>
        /// <exception cref="STDParsingException"/>
        public void ToSVG(string outFilename)
        {
            ToSVG(new FileStream(outFilename, FileMode.OpenOrCreate));
        }


        /// <summary>
        /// Converts to Portable Document Format (.pdf)
        /// </summary>
        /// <returns><see cref="Report"/>, containing result</returns>
        /// <exception cref="STDParsingException"/>
        public Document ToPDF()
        {
            return pdfConv.Convert(doc);
        }

        /// <summary>
        /// Converts to PDF and writes to existing <see cref="Stream"/>
        /// </summary>
        /// <exception cref="STDParsingException"/>
        public void ToPDF(Stream outStream)
        {
            ToPDF().Save(outStream);
        }

        /// <summary>
        /// Converts to PDF and writes to file
        /// </summary>
        /// <exception cref="STDParsingException"/>
        /// <exception cref="IOException"/>
        public async void ToPDF(string outFilename)
        {
            if (!File.Exists(outFilename))
            {
                File.Create(outFilename);
            }

            Document pdf = await Task.Run(() => pdf = ToPDF());
            pdf.Save(outFilename);
        }
    }
}