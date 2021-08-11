using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UmFramework.Util;

namespace UmFramework.Banco
{
    class SqlServer : IPersitencia
    {
        private readonly SqlConnection conSql;
        private readonly bool transaction;
        public SqlServer(SqlConnection conSql, bool transaction)
        {
            this.conSql = conSql;
            this.transaction = transaction;
        }
        public bool Salvar(Object objeto)
        {
            if (objeto == null)
                return false;

            string nomeTabela = "";
            string nomeChavePrimaria = "";
            long valorChavePrimaria = 0;
            List<string> ignorarPersistencia = new List<string>();

            MetodosAuxiliares.getAnnotations(objeto, ref nomeTabela, ref nomeChavePrimaria, ref valorChavePrimaria, ref ignorarPersistencia);

            if (nomeTabela == "" || nomeChavePrimaria == "")
                return false;


            if ((int)valorChavePrimaria == 0)
            {
                return this.Inserir(objeto, nomeTabela, nomeChavePrimaria, ignorarPersistencia);
            }
            else
            {
                return this.Atualizar(objeto, nomeTabela, nomeChavePrimaria, ignorarPersistencia);
            }

        }

        public bool SalvarLista(List<object> lstOjeto)
        {
            throw new NotImplementedException();
        }

        private bool Inserir(Object objeto, string nomeTabela, string nomeChavePrimaria, List<string> ignorarPersistencia)
        {
            try
            {
                using SqlCommand oCmd = conSql.CreateCommand();
                conSql.Open();
                List<ListaDePropriedades> lstPropriedades = MetodosAuxiliares.getListaDePropriedades(objeto, ignorarPersistencia);
                string query = $@"INSERT INTO {nomeTabela} ({string.Join(",", lstPropriedades.Select(x => x.nomeCampo))}) VALUES ({string.Join(",", lstPropriedades.Select(x => x.nomeCampoRef))});";
                if (nomeChavePrimaria != "")
                {
                    query += "SELECT LAST_INSERT_ID();";
                }
                oCmd.CommandText = query;
                foreach (var oProp in lstPropriedades)
                {
                    oCmd.Parameters.AddWithValue(oProp.nomeCampoRef, oProp.valorCampo);
                }
                oCmd.ExecuteNonQuery();
                var objetoPropriedades = objeto.GetType().GetRuntimeProperties();
                PropertyInfo chave = (PropertyInfo)objetoPropriedades.Where(x => x.Name == nomeChavePrimaria).FirstOrDefault();
                conSql.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        private bool Atualizar(Object objeto, string nomeTabela, string nomeChavePrimaria, List<string> ignorarPersistencia)
        {
            try
            {
                using SqlCommand oCmd = conSql.CreateCommand();
                conSql.Open();
                List<ListaDePropriedades> lstPropriedades = MetodosAuxiliares.getListaDePropriedades(objeto, ignorarPersistencia);
                string query = $@"UPDATE {nomeTabela} SET {string.Join(",", lstPropriedades.Select(x => x.nomeCampo + " = " + x.nomeCampoRef))}";
                if (nomeChavePrimaria != "")
                {
                    query += $" WHERE {nomeChavePrimaria} = " + lstPropriedades.Where(x => x.nomeCampo == nomeChavePrimaria).ToList()[0].valorCampo;
                }
                oCmd.CommandText = query;
                foreach (var oProp in lstPropriedades)
                {
                    oCmd.Parameters.AddWithValue(oProp.nomeCampoRef, oProp.valorCampo);
                }
                var result = oCmd.ExecuteNonQuery();
                conSql.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool Excluir(Object objeto)
        {
            string nomeTabela = "";
            string nomeChavePrimaria = "";
            long valorChavePrimaria = 0;
            List<string> ignorarPersistencia = new List<string>();

            MetodosAuxiliares.getAnnotations(objeto, ref nomeTabela, ref nomeChavePrimaria, ref valorChavePrimaria, ref ignorarPersistencia);
            try
            {
                using SqlCommand oCmd = conSql.CreateCommand();
                conSql.Open();
                string query = $@"DELETE FROM {nomeTabela} WHERE {nomeChavePrimaria} = {valorChavePrimaria}";
                oCmd.CommandText = query;
                var result = oCmd.ExecuteNonQuery();
                conSql.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public DataTable ExecutarQuery(string query, int pagina = 0, int tamanhoPagina = 0)
        {
            try
            {
                using SqlCommand oCmd = conSql.CreateCommand();
                var oDataTable = new DataTable();
                oCmd.CommandText = query;
                SqlDataAdapter oAdap = new SqlDataAdapter(oCmd);
                oAdap.Fill(oDataTable);
                return oDataTable;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public T CarregarObjeto<T>(long id) where T : new()
        {
            var objeto = new T();
            string nomeTabela = "";
            string nomeChavePrimaria = "";
            long valorChavePrimaria = 0;
            List<string> ignorarPersistencia = new List<string>();

            MetodosAuxiliares.getAnnotations(objeto, ref nomeTabela, ref nomeChavePrimaria, ref valorChavePrimaria, ref ignorarPersistencia);

            string query = $@"SELECT * FROM {nomeTabela} WHERE {nomeChavePrimaria} = {id}";
            List<T> aLista = this.CarregarLista<T>(query);
            if (aLista.Count > 0)
            {
                return aLista[0];
            }
            else
            {
                return default(T);
            }

        }
        public List<T> CarregarObjetos<T>(string CustomQuery = "", int pagina = 0, int tamanhoPagina = 0) where T : new()
        {
            var objeto = new T();
            var nomeTabela = ((Annotations.Tabela)objeto.GetType().GetCustomAttribute(typeof(Annotations.Tabela))).nomeTabela;
            string query = CustomQuery != "" ? CustomQuery : $@"SELECT * FROM {nomeTabela}";
            return this.CarregarLista<T>(query);
        }
        private List<T> CarregarLista<T>(string query) where T : new()
        {
            var objeto = new T();
            string nomeTabela = "";
            string nomeChavePrimaria = "";
            long valorChavePrimaria = 0;
            List<string> ignorarPersistencia = new List<string>();

            MetodosAuxiliares.getAnnotations(objeto, ref nomeTabela, ref nomeChavePrimaria, ref valorChavePrimaria, ref ignorarPersistencia);
            try
            {
                using SqlCommand oCmd = conSql.CreateCommand();
                var oDataTable = new DataTable();
                oCmd.CommandText = query;
                SqlDataAdapter oAdap = new SqlDataAdapter(oCmd);
                oAdap.Fill(oDataTable);
                return MetodosAuxiliares.getListFromDataTable<T>(oDataTable, ignorarPersistencia);

            }
            catch (Exception)
            {
                return null;
            }

        }
        private string PaginarQuery(string query, int pagina = 0, int tamanhoPagina = 0)
        {
            if (tamanhoPagina == 0 && !query.Contains("ORDER BY"))
            {
                return query;
            }
            else
            {
                return query + $@" OFFSET {tamanhoPagina} * ({pagina} - 1) ROWS
                                            FETCH NEXT {tamanhoPagina}
                                                ROWS ONLY;";
            }
        }

        public int getTotalRegistros()
        {
            throw new NotImplementedException();
        }

        public bool SalvarLista<T>(List<T> lstOjeto)
        {
            throw new NotImplementedException();
        }
    }
}
