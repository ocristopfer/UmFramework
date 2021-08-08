using System;

namespace UmFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            MySql.Data.MySqlClient.MySqlConnection oCnn = new MySql.Data.MySqlClient.MySqlConnection("server=localhost;user=root;database=teste;port=3306;password=suporte");
            var db = new Persitencia(oCnn);

            var teste = new TesteDados();
            teste.nome = "um novo teste novo";
            teste.data = DateTime.Now;
            db.Salvar(teste);
        
            //teste = db.CarregarObjeto<TesteDados>(2);

            teste.nome = "atualizando o id";
            teste.data = DateTime.Now;
            teste.ativo = true;
            db.Salvar(teste);

            
            var teste3 = db.CarregarObjetos<TesteDados>();
            var teste4 = db.ExecutarQuery("SELECT * FROM CADASTRO");
            var count = teste4.Rows.Count;

            

        }
    }
}
