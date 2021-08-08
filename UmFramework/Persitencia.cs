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
            List<string> ignorarPersistencia = new List<string>();

            this.getAnnotations(objeto, ref nomeTabela, ref nomeChavePrimaria, ref valorChavePrimaria, ref ignorarPersistencia);

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
        private bool Inserir(Object objeto, string nomeTabela, string nomeChavePrimaria, List<string> ignorarPersistencia)
        {
            try
            {

                using (MySqlCommand oCmd = conSql.CreateCommand())
                {
                    conSql.Open();
                    List<ListaDePropriedades> lstPropriedades = this.getListaDePropriedades(objeto, ignorarPersistencia);

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
                    var result = oCmd.LastInsertedId;
                    var objetoPropriedades = objeto.GetType().GetRuntimeProperties();

                    PropertyInfo chave = (PropertyInfo)objetoPropriedades.Where(x => x.Name == nomeChavePrimaria).FirstOrDefault();
                    chave.SetValue(objeto, oCmd.LastInsertedId);

                    conSql.Close();
                    return true;
                }
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
                using (MySqlCommand oCmd = conSql.CreateCommand())
                {
                    conSql.Open();
                    List<ListaDePropriedades> lstPropriedades = this.getListaDePropriedades(objeto, ignorarPersistencia);

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

            this.getAnnotations(objeto, ref nomeTabela, ref nomeChavePrimaria, ref valorChavePrimaria, ref ignorarPersistencia);
            try
            {
                using (MySqlCommand oCmd = conSql.CreateCommand())
                {
                    conSql.Open();
                    string query = $@"DELETE FROM {nomeTabela} WHERE {nomeChavePrimaria} = {valorChavePrimaria}";
                    oCmd.CommandText = query;
                    var result = oCmd.ExecuteNonQuery();
                    conSql.Close();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }
        public DataTable ExecutarQuery(string query)
        {
            try
            {
                using (MySqlCommand oCmd = conSql.CreateCommand())
                {
                    var oDataTable = new DataTable();
                    oCmd.CommandText = query;
                    MySql.Data.MySqlClient.MySqlDataAdapter oAdap = new MySqlDataAdapter(oCmd);
                    oAdap.Fill(oDataTable);
                    return oDataTable;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public T CarregarObjeto<T>(long id) where T : new()
        {
            var objeto = new T();
            var objetoPropriedades = objeto.GetType().GetRuntimeProperties();
            string nomeTabela = "";
            string nomeChavePrimaria = "";
            long valorChavePrimaria = 0;
            List<string> ignorarPersistencia = new List<string>();

            this.getAnnotations(objeto, ref nomeTabela, ref nomeChavePrimaria, ref valorChavePrimaria, ref ignorarPersistencia);

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
        public List<T> CarregarObjetos<T>(string CustomQuery = "") where T : new()
        {
            var objeto = new T();
            var nomeTabela = ((Annotations.Tabela)objeto.GetType().GetCustomAttribute(typeof(Annotations.Tabela))).nomeTabela;
            string query = CustomQuery != "" ? CustomQuery : $@"SELECT * FROM {nomeTabela}";
            return this.CarregarLista<T>(query);
        }
        private List<T> CarregarLista<T>(string query) where T : new()
        {
            var objeto = new T();
            var objetoPropriedades = objeto.GetType().GetRuntimeProperties();
            var lstObjeto = new List<T>();
            string nomeTabela = "";
            string nomeChavePrimaria = "";
            long valorChavePrimaria = 0;
            List<string> ignorarPersistencia = new List<string>();

            this.getAnnotations(objeto, ref nomeTabela, ref nomeChavePrimaria, ref valorChavePrimaria, ref ignorarPersistencia);
            try
            {
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
                                var campo = ignorarPersistencia.FirstOrDefault(x => x == item.Name);
                                if(campo == null)
                                {
                                    var valor = objetoDb[item.Name] != DBNull.Value ? objetoDb[item.Name] : default;
                                    if (item.PropertyType.Name == "Boolean")
                                    {
                                        item.SetValue(newObjeto, Convert.ToInt32(valor) == 1 ? true : false);
                                    }
                                    else
                                    {
                                        item.SetValue(newObjeto, valor);
                                    }
                                }


                            }
                            lstObjeto.Add(newObjeto);
                        }

                    }
                }
                return lstObjeto;
            }
            catch (Exception)
            {
                return null;
            }

        }
        private void getAnnotations(Object objeto, ref string nomeTabela, ref string nomeChavePrimaria, ref long valorChavePrimaria, ref List<string> ignorarPersistencia)
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
                    }
                    else if (atributo.ignorarPersitencia)
                    {
                        ignorarPersistencia.Add(oProp.Name);
                    }

                }
            }
        }
        private List<ListaDePropriedades> getListaDePropriedades(Object objeto, List<string> ignorarPersistencia)
        {
            List<ListaDePropriedades> lstPropriedades = new List<ListaDePropriedades>();
            foreach (var prop in objeto.GetType().GetProperties())
            {
                var campo = ignorarPersistencia.FirstOrDefault(x => x == prop.Name);
                if(campo == null)
                {
                    ListaDePropriedades aPropriedade = new ListaDePropriedades();
                    aPropriedade.nomeCampo = prop.Name;
                    aPropriedade.nomeCampoRef = "@" + prop.Name;
                    aPropriedade.valorCampo = prop.GetValue(objeto, null);
                    lstPropriedades.Add(aPropriedade);
                }
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

