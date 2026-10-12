using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoFisca.Models;
using ProyectoFisca.Sources.Interface;

namespace ProyectoFisca.Pages
{
    public class HomicidiosModel : PageModel
    {
        private readonly IHomicidiosDataAccess _dataAccess;

        public HomicidiosModel(IHomicidiosDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public List<HomicidiosViewModel> ListaHomicidios { get; set; } = new();

        public async Task OnGetAsync()
        {
            ListaHomicidios = await _dataAccess.ObtenerHomicidiosViewModelAsync(100);
        }
    }
}
