using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace UmFramework.Util
{
    class MetodosAuxiliares
    {
        public static IPersitencia GetPersitencia(DbConnection oCon, Enumeradores.TipoServidorDb enumTipoServidor, bool transaction = false)
        {
            return enumTipoServidor switch
            {
                Enumeradores.TipoServidorDb.MySql => new Banco.MySQL((MySqlConnection)oCon, transaction),
                Enumeradores.TipoServidorDb.SqlServer => new Banco.SqlServer((SqlConnection)oCon, transaction),
                Enumeradores.TipoServidorDb.Oracle => new Banco.Oracle((OracleConnection)oCon, transaction),
                _ => new Banco.MySQL((MySqlConnection)oCon, transaction),
            };
        }
        public static List<T> getListFromDataTable<T>(DataTable oDataTable, List<string> ignorarPersistencia) where T : new()
        {
            var objetoPropriedades = new T().GetType().GetRuntimeProperties();
            var lstObjeto = new List<T>();
            if (oDataTable.Rows.Count > 0)
            {
                foreach (DataRow objetoDb in oDataTable.Rows)
                {
                    var newObjeto = new T();
                    foreach (var item in objetoPropriedades)
                    {
                        var campo = ignorarPersistencia.FirstOrDefault(x => x == item.Name);
                        if (campo == null)
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
            return lstObjeto;
        }
        public static void getAnnotations(Object objeto, ref string nomeTabela, ref string nomeChavePrimaria, ref long valorChavePrimaria, ref List<string> ignorarPersistencia)
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
        public static List<ListaDePropriedades> getListaDePropriedades(Object objeto, List<string> ignorarPersistencia)
        {
            List<ListaDePropriedades> lstPropriedades = new List<ListaDePropriedades>();
            foreach (var prop in objeto.GetType().GetProperties())
            {
                var campo = ignorarPersistencia.FirstOrDefault(x => x == prop.Name);
                if (campo == null)
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
}
