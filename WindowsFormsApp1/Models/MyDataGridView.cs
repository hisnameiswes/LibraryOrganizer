using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1.Models
{
    class MyDataGridView : DataGridView
    {
        public MyDataGridView()
        {
            base.DataBindingComplete += Sort;
        }

        public void Sort(object sender, EventArgs e)
        {
            Sort(Columns[0], ListSortDirection.Ascending);
            Columns[0].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
        }
    }
}
