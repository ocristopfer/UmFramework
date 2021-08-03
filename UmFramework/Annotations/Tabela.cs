using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmFramework.Annotations
{
    [AttributeUsage(System.AttributeTargets.Class)]
    public class Tabela : System.Attribute  
    {
        public string nomeTabela;
    }
}
