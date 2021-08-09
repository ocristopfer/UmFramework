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
        public bool Excluir(Object objeto);
        public DataTable ExecutarQuery(string query);
        public T CarregarObjeto<T>(long id) where T : new();
        public List<T> CarregarObjetos<T>(string CustomQuery = "") where T : new();
 
    }
}
