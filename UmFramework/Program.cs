using System;

namespace UmFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var teste = new TesteDados();
            MySql.Data.MySqlClient.MySqlConnection oCnn = new MySql.Data.MySqlClient.MySqlConnection("");
            var db = new Persitencia(oCnn);
            db.CarregarObjeto<TesteDados>(1);

            

        }
    }
}
