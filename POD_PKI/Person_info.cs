using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POD_PKI
{
    class Person_info
    {
        public int p;
        public int g;
        public int x; //klucz prywatny
        public int X; //klucz publiczny
        public int K; //klucz sesji
        public int F; //klucz obcy
        public String Name; //Alicja czy Bob

        public Person_info(String Name_)
        {
            p = g = x = X = K = F = 0;
            Name = Name_;
        }

        public void printAll()
        {
            Console.WriteLine
                (
                Name +
                ": p=" + p +
                ", g=" + g +
                ", x=" + x +
                ", X=" + X +
                ", K=" + K +
                ", F=" + F
                );
        }
    }
}
