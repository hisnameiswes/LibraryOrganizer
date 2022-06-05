using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Models
{
    internal class Book
    {
        public int Database_Id { get; set; }
        public int Unique_Id { get; set; }
        public string Title { get; set; }

        public string Author { get; set; }

        public string Illustrator { get; set; }

        public string Format { get; set; }

        public string Classification { get; set; }
        public string Call_Number { get; set; }

        public int Check_Outs { get; set; }

    }
}
