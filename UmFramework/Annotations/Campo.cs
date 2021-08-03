using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmFramework.Annotations
{
    [AttributeUsage(System.AttributeTargets.Property)]
    class Campo : System.Attribute  
    {
        public bool chavePrimaria;
    }
}
