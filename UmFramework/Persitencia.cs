using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Data;

namespace UmFramework
{
    class Persitencia
    {
        private MySqlConnection conSql;
        public Persitencia(MySqlConnection conSql)
        {
            this.conSql = conSql;
        }
        public bool Salvar(Object objeto)
        {
            if (objeto == null)
                return false;

            string nomeTabela = "";
            string nomeChavePrimaria = "";
            long valorChavePrimaria = 0;

            this.getAnnotations(objeto, ref nomeTabela, ref nomeChavePrimaria, ref valorChavePrimaria);

            if (nomeTabela == "" || nomeChavePrimaria == "")
                return false;

            if ((int)valorChavePrimaria == 0)
            {
                return this.Inserir(objeto, nomeTabela, nomeChavePrimaria);
            }
            else
            {
                return this.Atualizar(objeto, nomeTabela, nomeChavePrimaria);
            }


        }

        private bool Inserir(Object objeto, string nomeTabela, string nomeChavePrimaria)
        {

            using (MySqlCommand oCmd = conSql.CreateCommand())
            {
                List<ListaDePropriedades> lstPropriedades = this.getListaDePropriedades(objeto);

                string query = $@"INSERT INTO {nomeTabela} ({string.Join(",", lstPropriedades.Select(x => x.nomeCampo))}) VALUES ({string.Join(",", lstPropriedades.Select(x => x.nomeCampoRef))});";
                if (nomeChavePrimaria != "")
                {
                    query += "SELECT SCOPE_IDENTITY() ID;";
                }

                oCmd.CommandText = query;

                foreach (var oProp in lstPropriedades)
                {
                    oCmd.Parameters.AddWithValue(oProp.nomeCampoRef, oProp.valorCampo);
                }

                var result = oCmd.ExecuteNonQuery();
            }

            return false;

        }
        private bool Atualizar(Object objeto, string nomeTabela, string nomeChavePrimaria)
        {

            using (MySqlCommand oCmd = conSql.CreateCommand())
            {
                List<ListaDePropriedades> lstPropriedades = this.getListaDePropriedades(objeto);

                string query = $@"UPDATE {nomeTabela} SET {string.Join(",", lstPropriedades.Select(x => x.nomeCampo + " = " + x.nomeCampoRef))}";

                if (nomeChavePrimaria != "")
                {
                    query += $"WHERE {nomeChavePrimaria} = " + lstPropriedades.Where(x => x.nomeCampo == nomeChavePrimaria).ToList()[0].valorCampo;
                }

                oCmd.CommandText = query;

                foreach (var oProp in lstPropriedades)
                {
                    oCmd.Parameters.AddWithValue(oProp.nomeCampoRef, oProp.valorCampo);
                }

                var result = oCmd.ExecuteNonQuery();
            }

            return false;
        }

        public bool Excluir(Object objeto)
        {
            string nomeTabela = "";
            string nomeChavePrimaria = "";
            long valorChavePrimaria = 0;

            this.getAnnotations(objeto, ref nomeTabela, ref nomeChavePrimaria, ref valorChavePrimaria);

            using (MySqlCommand oCmd = conSql.CreateCommand())
            {
                string query = $@"DELETE FROM {nomeTabela} WHERE {nomeChavePrimaria} = {valorChavePrimaria}";
                oCmd.CommandText = query;
                var result = oCmd.ExecuteNonQuery();
                return true;
            }
        }

        public DataTable ExecutarQuery()
        {
            return new DataTable();
        }

        public T CarregarObjeto<T>(long id) where T : new()
        {

            var objeto = new T();
            var objetoPropriedades = objeto.GetType().GetRuntimeProperties();
            string nomeTabela = "";
            string nomeChavePrimaria = "";
            long valorChavePrimaria = 0;
            this.getAnnotations(objeto, ref nomeTabela, ref nomeChavePrimaria, ref valorChavePrimaria);

            string query = $@"SELECT * FROM {nomeTabela} WHERE {nomeChavePrimaria} = {id}";
            return this.CarregarLista<T>(query)[0];
        }

        public List<T> CarregarLista<T>(string query) where T : new()
        {
            var objeto = new T();
            var objetoPropriedades = objeto.GetType().GetRuntimeProperties();
            var lstObjeto = new List<T>();
            string nomeTabela = "";
            string nomeChavePrimaria = "";
            long valorChavePrimaria = 0;

            this.getAnnotations(objeto, ref nomeTabela, ref nomeChavePrimaria, ref valorChavePrimaria);

            using (MySqlCommand oCmd = conSql.CreateCommand())
            {
                var oDataTable = new DataTable();
                oCmd.CommandText = query;
                MySql.Data.MySqlClient.MySqlDataAdapter oAdap = new MySqlDataAdapter(oCmd);
                oAdap.Fill(oDataTable);

                if (oDataTable.Rows.Count > 0)
                {
                    foreach (DataRow objetoDb in oDataTable.Rows)
                    {
                        var newObjeto = new T();
                        foreach (var item in objetoPropriedades)
                        {
                            item.SetValue(newObjeto, objetoDb[item.Name]);
                        }
                        lstObjeto.Add(newObjeto);
                    }

                }
            }
            return lstObjeto;
        }


        private void getAnnotations(Object objeto, ref string nomeTabela, ref string nomeChavePrimaria, ref long valorChavePrimaria)
        {
            var objetoType = objeto.GetType();
            nomeTabela = ((Annotations.Tabela)objetoType.GetCustomAttribute(typeof(Annotations.Tabela))).nomeTabela;

            foreach (var oProp in objetoType.GetProperties())
            {
                Annotations.Campo atributo = (Annotations.Campo)oProp.GetCustomAttribute(typeof(Annotations.Campo), true);
                if (atributo != null)
                {
                    if (atributo.chavePrimaria == true)
                    {
                        nomeChavePrimaria = oProp.Name;
                        valorChavePrimaria = Convert.ToInt64(oProp.GetValue(objeto, null));
                        break;
                    }
                }
            }
        }
        private List<ListaDePropriedades> getListaDePropriedades(Object objeto)
        {
            List<ListaDePropriedades> lstPropriedades = new List<ListaDePropriedades>();
            foreach (var prop in objeto.GetType().GetProperties())
            {
                ListaDePropriedades aPropriedade = new ListaDePropriedades();
                aPropriedade.nomeCampo = prop.Name;
                aPropriedade.nomeCampoRef = "@" + prop.Name;
                aPropriedade.valorCampo = prop.GetValue(objeto, null);
                lstPropriedades.Add(aPropriedade);
            }
            return lstPropriedades;
        }

    }
    class ListaDePropriedades
    {
        public string nomeCampo;
        public string nomeCampoRef;
        public Object valorCampo;
    }
}

