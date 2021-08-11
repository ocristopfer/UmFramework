using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmFramework
{
    interface IPersitencia
    {
        public bool Salvar(Object objeto);
        public bool SalvarLista(List<Object> lstOjeto);
        public bool Excluir(Object objeto);
        public DataTable ExecutarQuery(string query, int pagina = 0, int tamanhoPagina = 0);
        public T CarregarObjeto<T>(long id) where T : new();
        public List<T> CarregarObjetos<T>(string CustomQuery= "", int pagina = 0, int tamanhoPagina = 0) where T : new();
        public int getTotalRegistros();
    }
}
