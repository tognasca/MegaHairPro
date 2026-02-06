
using DinkToPdf;
using DinkToPdf.Contracts;
using MegaHairPro.Web.Services.Interfaces;

namespace MegaHairPro.Web.Services
{
    public class PdfService : IPdfService
    {
        private readonly IConverter _converter;

        public PdfService(IConverter converter)
        {
            _converter = converter;
        }

        public byte[] GerarPdf(string html)
        {
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings =
                {
                    PaperSize = PaperKind.A4
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = html
                    }
                }
            };

            return _converter.Convert(doc);
        }
    }
}
