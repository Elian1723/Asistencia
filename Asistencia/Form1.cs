using Asistencia.Models;
using Dapper;
using Excel = Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.ReportingServices.Diagnostics.Internal;

namespace Asistencia
{
    public partial class Form1 : Form
    {
        private string _valorCeldaTmp;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bool existeBaseLocal = BaseLocal.Comprobar();

            if (!existeBaseLocal)
            {
                MessageBox.Show("No se pudo comprobar la base de datos", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            CargarAlumnos();
        }

        private void CargarAlumnos()
        {
            using (var connection = new SQLiteConnection(BaseLocal.Cadena))
            {
                connection.Open();

                string query = "SELECT * FROM Alumno;";

                dataGridViewAlumnos.DataSource = connection.Query<Alumno>(query).ToList();
                dataGridViewAlumnos.Columns[0].Visible = false;
            }
        }

        private void AgregarAlumno()
        {
            using (var connection = new SQLiteConnection(BaseLocal.Cadena))
            {
                connection.Open();

                string query = "INSERT INTO Alumno (Nombre) VALUES (@Nombre);";
                connection.Execute(query, new { Nombre = textBoxNombre.Text });

                CargarAlumnos();

                textBoxNombre.Clear();
            }
        }

        private void buttonAgregar_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxNombre.Text))
            {
                AgregarAlumno();
            }
        }

        private void textBoxNombre_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                if (!string.IsNullOrEmpty(textBoxNombre.Text))
                {
                    AgregarAlumno();
                }
            }
        }

        private void dataGridViewAlumnos_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            _valorCeldaTmp = dataGridViewAlumnos.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
        }

        private void dataGridViewAlumnos_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string id = dataGridViewAlumnos.Rows[e.RowIndex].Cells[0].Value.ToString();

            if (dataGridViewAlumnos.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null && !string.IsNullOrEmpty(dataGridViewAlumnos.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString().Trim()))
            {
                using (var connection = new SQLiteConnection(BaseLocal.Cadena))
                {
                    connection.Open();

                    string query = "UPDATE Alumno SET Nombre = @Nombre WHERE Id = @Id;";
                    connection.Execute(query, new { Id = id, Nombre = dataGridViewAlumnos.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() });
                }
            }
            else
            {
                dataGridViewAlumnos.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = _valorCeldaTmp;
            }

            _valorCeldaTmp = string.Empty;
        }

        private void dataGridViewAlumnos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 & e.ColumnIndex == -1)
            {
                string id = dataGridViewAlumnos.Rows[e.RowIndex].Cells[0].Value.ToString();

                if (MessageBox.Show("¿Está seguro de eliminar este registro?", "Eliminar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (var connection = new SQLiteConnection(BaseLocal.Cadena))
                    {
                        connection.Open();

                        string query = "DELETE FROM Alumno WHERE Id = @Id;";
                        connection.Execute(query, new { Id = id });

                        CargarAlumnos();
                    }
                }

                textBoxNombre.Focus();
            }
        }

        static List<DateTime> ObtenerDiasHabiles()
        {
            List<DateTime> diasHabiles = new List<DateTime>();

            DateTime primerDia = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            DateTime ultimoDia = primerDia.AddMonths(1).AddDays(-1);

            for (DateTime dia = primerDia; dia <= ultimoDia; dia = dia.AddDays(1))
            {
                if (dia.DayOfWeek != DayOfWeek.Saturday && dia.DayOfWeek != DayOfWeek.Sunday)
                {
                    diasHabiles.Add(dia);
                }
            }

            return diasHabiles;
        }

        private string ObtenerUbicacion()
        {
            string ubicacion = null;
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ubicacion = saveFileDialog.FileName;
                }
            }

            return ubicacion;
        }

        private Excel.Range CreateCell(Excel.Range range, object valor)
        {
            range.Merge();

            range.Value = valor.ToString();
            range.Font.Name = "Arial";
            range.Font.Size = 11;

            return range;
        }

        private void GenerarExcel(string ubicacion)
        {
            var diasHabiles = ObtenerDiasHabiles();

            var app = new Excel.Application();

            var workbooks = app.Workbooks;
            var workbook = workbooks.Add();
            var worksheet = workbook.Sheets[1] as Excel.Worksheet;

            CreateCell(worksheet.Range["A1", "B1"], DateTime.Now.Year)
                .SetFontBold(true)
                .SetFontSize(16)
                .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignCenter)
                .SetBorder("#CCCCFF");

            CreateCell(worksheet.Range["C1", "X1"], DateTime.Now.ToString("MMMM").ToUpper())
                .SetFontBold(true)
                .SetFontSize(16)
                .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignCenter)
                .SetBorder("#CCCCFF");

            CreateCell(worksheet.Range["A2", "B2"], "Fecha")
                .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignCenter)
                .SetBackgroundColor("#E7E7FF")
                .SetBorder("#CCCCFF");

            worksheet.Columns["A"].ColumnWidth = 5;
            CreateCell(worksheet.Range["A3", "A3"], "No.")
                .SetFontBold(true)
                .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignCenter)
                .SetBackgroundColor("#CCCCFF")
                .SetBorder("#CCCCFF");

            worksheet.Columns["B"].ColumnWidth = 38;
            CreateCell(worksheet.Range["B3", "B3"], "Nombre")
                .SetFontBold(true)
                .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignCenter)
                .SetBackgroundColor("#CCCCFF")
                .SetBorder("#CCCCFF");


            int columna = 3;
            int numeroColor = 1;
            for (int i = 0; i < diasHabiles.Count; i++)
            {
                worksheet.Columns[columna].ColumnWidth = 3.57;

                CreateCell(worksheet.Cells[2, columna] as Excel.Range, diasHabiles[i].Day)
                    .SetFontBold(true)
                    .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignCenter)
                    .SetBackgroundColor(fondosPrimarios[numeroColor])
                    .SetBorder("#CCCCFF");

                CreateCell(worksheet.Cells[3, columna] as Excel.Range, inicialesDias[diasHabiles[i].ToString("dddd").ToLower()])
                    .SetFontBold(true)
                    .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignCenter)
                    .SetBackgroundColor(fondosSecundarios[numeroColor])
                    .SetBorder("#CCCCFF");

                columna += 1;
                numeroColor += diasHabiles[i].ToString("dddd") == "viernes" ? 1 : 0;
            }

            var alumnos = new List<Alumno>();
            using (var connection = new SQLiteConnection(BaseLocal.Cadena))
            {
                connection.Open();

                string query = "SELECT * FROM Alumno;";

                alumnos = connection.Query<Alumno>(query).ToList();
            }

            for (int i = 0; i < alumnos.Count; i++)
            {
                CreateCell(worksheet.Cells[i + 4, 1] as Excel.Range, i + 1)
                    .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignCenter)
                    .SetBackgroundColor((i + 1) % 2 == 0 ? "#E7E7FF" : "#FFFFFF")
                    .SetBorder("#CCCCFF");

                CreateCell(worksheet.Cells[i + 4, 2] as Excel.Range, alumnos[i].Nombre.Trim())
                    .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignLeft)
                    .SetBackgroundColor((i + 1) % 2 == 0 ? "#E7E7FF" : "#FFFFFF")
                    .SetBorder("#CCCCFF");

                for (int j = 0; j < diasHabiles.Count; j++)
                {
                    CreateCell(worksheet.Cells[i + 4, j + 3] as Excel.Range, string.Empty)
                        .SetBackgroundColor((i + 1) % 2 == 0 ? "#E7E7FF" : "#FFFFFF")
                        .SetBorder("#CCCCFF");
                }
            }

            workbook.SaveAs(ubicacion);

            workbook.Close();
        }

        private void buttonGenerar_Click(object sender, EventArgs e)
        {
            string ubicacion = ObtenerUbicacion();

            if (ubicacion != null)
            {
                GenerarExcel(ubicacion);

                MessageBox.Show("Archivo generado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        Dictionary<string, string> inicialesDias = new Dictionary<string, string>()
        {
            { "lunes", "L" },
            { "martes", "M" },
            { "miércoles", "Mi" },
            { "jueves", "J" },
            { "viernes", "V" }
        };

        Dictionary<int, string> fondosPrimarios = new Dictionary<int, string>()
        {
            { 1, "#DDEBF7" },
            { 2, "#FCE4D6" },
            { 3, "#FFF2CC" },
            { 4, "#E2EFDA" },
            { 5, "#D9E1F2" }
        };

        Dictionary<int, string> fondosSecundarios = new Dictionary<int, string>()
        {
            { 1, "#BDD7EE" },
            { 2, "#F8CBAD" },
            { 3, "#FFE699" },
            { 4, "#C6E0B4" },
            { 5, "#B4C6E7" }
        };
    }
}
