using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Testing_V1
{
    public class Requirement
    {
        public string Value { get; set; }
        public bool Completed { get; set; }
        Requirement(string value)
        {
            Value = value;
            Completed = false;
        }
    }
}
