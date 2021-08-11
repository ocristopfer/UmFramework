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
        public long id { get; set; }
        public string nome { get; set; }
        public DateTime data { get; set; }
        public bool ativo { get; set; }

        public DateTime nascimento { get; set; }

        public string endereco { get; set; }
        
        [Campo(ignorarPersitencia = true)]
        public string teste { get; set; }
    }
}
