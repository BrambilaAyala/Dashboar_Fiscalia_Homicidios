namespace ProyectoFisca.Sources.Interface
{
    public interface INormalizadorGeoService
    {
        void Inicializar();
        (string Entidad, string Municipio, bool RequiereRevision, string Detalle) Resolver(string entidadRaw, string municipioRaw, string fuenteRaw);
        void ReiniciarContexto();
        List<string> ObtenerEntidadesOficiales();
    }
}
