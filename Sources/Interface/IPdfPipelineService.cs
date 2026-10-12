using ProyectoFisca.Models;

namespace ProyectoFisca.Sources.Interface
{
    public interface IPdfPipelineService
    {
        Task<string?> DescargarPdfAsync(DateTime fecha, string carpetaDestino);
        List<RegistroHomicidioDto> ExtraerRegistrosDePdf(string pdfPath, DateTime fecha);
    }
}
