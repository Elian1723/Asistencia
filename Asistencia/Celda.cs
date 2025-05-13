using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace Asistencia
{
    public class Celda
    {
        private readonly Excel.Range _worksheet;

        public Celda(Excel.Range worksheet)
        {
            _worksheet = worksheet;
        }

        public Celda SetCelda(Point inicio, Point fin, string valor)
        {
            (_worksheet.Range[_worksheet.Cells[inicio.Y, inicio.X], _worksheet.Cells[fin.Y, fin.X]] as Excel.Range).Merge();



            

            return this;
        }

        public SetStyle()
        {

        }
    }
}
