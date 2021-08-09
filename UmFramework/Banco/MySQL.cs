using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UmFramework.Util;

namespace UmFramework.Banco
{
    class MySQL: IPersitencia
    {
        private readonly MySqlConnection conSql;
        private readonly bool transaction = false;
        public MySQL(MySqlConnection conSql, bool transaction = false)
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
            if (lstOjeto == null)
                return false;

            string nomeTabela = "";
            string nomeChavePrimaria = "";
            long valorChavePrimaria = 0;
            List<string> ignorarPersistencia = new List<string>();

            if (nomeTabela == "" || nomeChavePrimaria == "")
                return false;

            var insertQuery = "";
            var updateQuery = "";
            foreach (var item in lstOjeto)
            {
                MetodosAuxiliares.getAnnotations(item, ref nomeTabela, ref nomeChavePrimaria, ref valorChavePrimaria, ref ignorarPersistencia);
                List<ListaDePropriedades> lstPropriedades = MetodosAuxiliares.getListaDePropriedades(item, ignorarPersistencia);
                if ((int)valorChavePrimaria == 0)
                {
                    insertQuery = this.getInsertQuery(nomeTabela, lstPropriedades, nomeChavePrimaria);
                }
                else
                {
                    updateQuery = this.getUpdateQuery(nomeTabela, lstPropriedades, nomeChavePrimaria);
                }
            }
            throw new NotImplementedException();
            
        }

        private bool Inserir(Object objeto, string nomeTabela, string nomeChavePrimaria, List<string> ignorarPersistencia)
        {
            MySqlTransaction trans = null;
            try
            {

                MySqlCommand oCmd = conSql.CreateCommand();
                
                conSql.Open();
                
                if (this.transaction)
                    trans = conSql.BeginTransaction();

                List<ListaDePropriedades> lstPropriedades = MetodosAuxiliares.getListaDePropriedades(objeto, ignorarPersistencia);

                oCmd.CommandText = getInsertQuery(nomeTabela, lstPropriedades, nomeChavePrimaria);

                foreach (var oProp in lstPropriedades)
                {
                    oCmd.Parameters.AddWithValue(oProp.nomeCampoRef, oProp.valorCampo);
                }

                oCmd.ExecuteNonQuery();
                var result = oCmd.LastInsertedId;

                if (this.transaction)
                    trans.Commit();

                var objetoPropriedades = objeto.GetType().GetRuntimeProperties();

                PropertyInfo chave = (PropertyInfo)objetoPropriedades.Where(x => x.Name == nomeChavePrimaria).FirstOrDefault();
                chave.SetValue(objeto, oCmd.LastInsertedId);

                conSql.Close();
                return true;
            }
            catch (Exception)
            {
                if (this.transaction)
                    trans.Rollback();
                conSql.Close();
                return false;
            }

        }

        private string getInsertQuery(string nomeTabela, List<ListaDePropriedades> lstPropriedades, string nomeChavePrimaria = "")
        {
            string query = $@"INSERT INTO {nomeTabela} ({string.Join(",", lstPropriedades.Select(x => x.nomeCampo))}) VALUES ({string.Join(",", lstPropriedades.Select(x => x.nomeCampoRef))});";
            if (nomeChavePrimaria != "")
            {
                query += "SELECT LAST_INSERT_ID();";
            }
            return query;
        }
        private bool Atualizar(Object objeto, string nomeTabela, string nomeChavePrimaria, List<string> ignorarPersistencia)
        {
            MySqlTransaction trans = null;
            try
            {
                using MySqlCommand oCmd = conSql.CreateCommand();
                conSql.Open();
                if (this.transaction)
                    trans = conSql.BeginTransaction();


                List<ListaDePropriedades> lstPropriedades = MetodosAuxiliares.getListaDePropriedades(objeto, ignorarPersistencia);
                
                oCmd.CommandText = this.getUpdateQuery(nomeTabela, lstPropriedades, nomeChavePrimaria);

                foreach (var oProp in lstPropriedades)
                {
                    oCmd.Parameters.AddWithValue(oProp.nomeCampoRef, oProp.valorCampo);
                }

                var result = oCmd.ExecuteNonQuery();
                if (this.transaction)
                    trans.Commit();

                conSql.Close();
                return true;
            }
            catch (Exception)
            {
                if (this.transaction)
                    trans.Rollback();
                conSql.Close();
                return false;
            }
        }

        private string getUpdateQuery(string nomeTabela, List<ListaDePropriedades>  lstPropriedades, string nomeChavePrimaria)
        {
            string query = $@"UPDATE {nomeTabela} SET {string.Join(",", lstPropriedades.Select(x => x.nomeCampo + " = " + x.nomeCampoRef))}";

            if (nomeChavePrimaria != "")
            {
                query += $" WHERE {nomeChavePrimaria} = " + lstPropriedades.Where(x => x.nomeCampo == nomeChavePrimaria).ToList()[0].valorCampo;
            }
            return query;
        }
        public bool Excluir(Object objeto)
        {
            string nomeTabela = "";
            string nomeChavePrimaria = "";
            long valorChavePrimaria = 0;
            List<string> ignorarPersistencia = new List<string>();
            MySqlTransaction trans = null;

            MetodosAuxiliares.getAnnotations(objeto, ref nomeTabela, ref nomeChavePrimaria, ref valorChavePrimaria, ref ignorarPersistencia);

            try
            {
                using MySqlCommand oCmd = conSql.CreateCommand();
                conSql.Open();
                if (this.transaction)
                    trans = conSql.BeginTransaction();

                string query = $@"DELETE FROM {nomeTabela} WHERE {nomeChavePrimaria} = {valorChavePrimaria}";
                oCmd.CommandText = query;
                var result = oCmd.ExecuteNonQuery();
                if (this.transaction)
                    trans.Commit();

                conSql.Close();
                return true;
            }
            catch (Exception)
            {
                if (this.transaction)
                    trans.Rollback();
                conSql.Close();
                return false;
            }

        }
        public DataTable ExecutarQuery(string query, int pagina = 0, int tamanhoPagina = 0)        {
            try
            {
                using MySqlCommand oCmd = conSql.CreateCommand();
                var oDataTable = new DataTable();
                query = this.PaginarQuery(query, pagina, tamanhoPagina);
                
                oCmd.CommandText = query;
                MySqlDataAdapter oAdap = new MySqlDataAdapter(oCmd);
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
            query = this.PaginarQuery(query, pagina, tamanhoPagina);
    
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
                using MySqlCommand oCmd = conSql.CreateCommand();
                var oDataTable = new DataTable();
                oCmd.CommandText = query;
                MySql.Data.MySqlClient.MySqlDataAdapter oAdap = new MySqlDataAdapter(oCmd);
                oAdap.Fill(oDataTable);
                return MetodosAuxiliares.getListFromDataTable<T>(oDataTable, ignorarPersistencia);

            }
            catch (Exception)
            {
                conSql.Close();
                return null;
            }

        }

        private string PaginarQuery(string query, int pagina, int tamanhoPagina)
        {
            if(tamanhoPagina == 0)
            {
                return query;
            }
            else
            {
                pagina = (pagina - 1) * tamanhoPagina;
                return query += $@" LIMIT {pagina},{tamanhoPagina}";
            }
            
        }


    }
}
