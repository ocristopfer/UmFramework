﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using UmFramework.Util;

namespace UmFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            IPersitencia db;
       

            MySqlConnection oCnn = new MySqlConnection("server=localhost;user=root;database=teste;port=3306;password=suporte");
            
            db = MetodosAuxiliares.GetPersitencia(oCnn, Enumeradores.TipoServidorDb.MySql, true);

            //var teste = new TesteDados();
            //teste.nome = "um novo teste novo";
            //teste.data = DateTime.Now;
            //db.Salvar(teste);
        
            ////teste = db.CarregarObjeto<TesteDados>(2);

            //teste.nome = "atualizando o id";
            //teste.data = DateTime.Now;
            //teste.ativo = true;
            //db.Salvar(teste);

            List<TesteDados> lstTeste = new List<TesteDados>();
            for (int i = 0; i < 10; i++)
            {
                var oTeste = new TesteDados();
                oTeste.nome = $"Teste Novo{i}";
                oTeste.data = DateTime.Now;
                oTeste.ativo = true;
                
                if (i > 5)
                   oTeste.id = i;
               
                lstTeste.Add(oTeste);
            }
            db.SalvarLista<TesteDados>(lstTeste);

            /*
            var teste3 = db.CarregarObjetos<TesteDados>("", 1, 10);
            var total = db.getTotalRegistros();
            var teste4 = db.ExecutarQuery("SELECT * FROM CADASTRO");
            var count = teste4.Rows.Count;
            */
            

        }
    }
}
