using System;
using System.Collections.Generic;
using System.Text;
using UmFramework.Annotations;

namespace UmFramework
{
    [Tabela(nomeTabela = "cadastro")]
    class TesteDados
    {
        [Campo(chavePrimaria = true)]
        public int id { get; set; } = 1;
        public string nome { get; set; } = "";
    }
}
