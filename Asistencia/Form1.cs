using Asistencia.Models;
using Dapper;
using Excel = Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;

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
            CargarMeses();
        }

        private void CargarAlumnos()
        {
            using (var connection = new SQLiteConnection(BaseLocal.Cadena))
            {
                connection.Open();

                BindingList<Alumno> alumnos = new BindingList<Alumno>(connection.Query<Alumno>("SELECT * FROM Alumno;").ToList());

                dataGridViewAlumnos.DataSource = alumnos;
                dataGridViewAlumnos.Columns[0].ReadOnly = true;
                dataGridViewAlumnos.Columns[0].Width = 55;
            }
        }

        private void CargarMeses()
        {
            comboBoxMeses.DataSource = new BindingSource(_meses, null);
            comboBoxMeses.ValueMember = "Key";
            comboBoxMeses.DisplayMember = "Value";
            comboBoxMeses.SelectedValue = DateTime.Now.Month;
        }

        private void AgregarAlumno(string nombre)
        {
            using (var connection = new SQLiteConnection(BaseLocal.Cadena))
            {
                connection.Open();

                int maxId = connection.Query<int>("SELECT IFNULL(MAX(Id), 0) Id FROM Alumno;").First();

                connection.Execute("INSERT INTO Alumno (Id, Nombre) VALUES (@Id, @Nombre);", new { Id = maxId + 1, Nombre = nombre });

                CargarAlumnos();

                textBoxNombre.Clear();
                textBoxId.Clear();

                textBoxId.Focus();
            }
        }

        private bool ValidarId(int id)
        {
            using (var connection = new SQLiteConnection(BaseLocal.Cadena))
            {
                connection.Open();

                Alumno alumnoEncontrado = connection.Query<Alumno>("SELECT * FROM Alumno WHERE Id = @Id LIMIT 1;", new { Id = id }).FirstOrDefault();

                return alumnoEncontrado == null;
            }
        }

        private void AgregarAlumno(int id, string nombre)
        {
            using (var connection = new SQLiteConnection(BaseLocal.Cadena))
            {
                connection.Open();

                connection.Execute("INSERT INTO Alumno (Id, Nombre) VALUES (@Id, @Nombre);", new { Id = id, Nombre = nombre });

                CargarAlumnos();

                textBoxNombre.Clear();
                textBoxId.Clear();

                textBoxId.Focus();
            }
        }


        private void buttonAgregar_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxId.Text.Trim()) && !string.IsNullOrEmpty(textBoxNombre.Text.Trim()))
            {
                if (int.TryParse(textBoxId.Text, out int id))
                {
                    if (ValidarId(id))
                    {
                        AgregarAlumno(id, textBoxNombre.Text.Trim());
                    }
                    else
                    {
                        MessageBox.Show($"Ya existe un alumno con el Id {id}", "Alumno existente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("El Id debe ser un número entero", "Formato incorrecto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if (!string.IsNullOrEmpty(textBoxNombre.Text.Trim()))
            {
                AgregarAlumno(textBoxNombre.Text.Trim());
            }
            else
            {
                MessageBox.Show("Debe proporcionar como mínimo el nombre del alumno para poder agregarlo", "Campos vacíos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void textBoxId_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter)) textBoxNombre.Focus();
        }

        private void textBoxNombre_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                buttonAgregar.PerformClick();
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

        private void buttonEliminarTodo_Click(object sender, EventArgs e)
        {
            var confirmacion = MessageBox.Show("¿Está seguro de eliminar todos los alumnos?", "Eliminar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmacion == DialogResult.Yes)
            {
                using (var connection = new SQLiteConnection(BaseLocal.Cadena))
                {
                    connection.Open();

                    connection.Execute("DELETE FROM Alumno;");
                }

                MessageBox.Show("Todos los alumnos han sido eliminados correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarAlumnos();
            }
        }
        private List<DateTime> ObtenerDiasHabiles(int mes)
        {
            List<DateTime> diasHabiles = new List<DateTime>();

            DateTime primerDia = new DateTime(DateTime.Now.Year, mes, 1);

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

        private string ObtenerUbicacion(int mes)
        {
            string ubicacion = null;
            var nombreMes = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mes);
            
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = $"asistencia_{nombreMes}{DateTime.Now.Year}";

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
            range.Font.Name = "Calibri";
            range.Font.Size = 11;

            return range;
        }

        private void GenerarExcel(string ubicacion, int mes)
        {
            var app = new Excel.Application();
            var workbooks = app.Workbooks;
            var workbook = workbooks.Add();
            var worksheet = workbook.Sheets[1] as Excel.Worksheet;

            worksheet.PageSetup.PaperSize = Excel.XlPaperSize.xlPaperLetter;
            worksheet.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape;
            worksheet.PageSetup.LeftMargin = app.InchesToPoints(0.25);
            worksheet.PageSetup.RightMargin = app.InchesToPoints(0.25);
            worksheet.PageSetup.TopMargin = app.InchesToPoints(0.75);
            worksheet.PageSetup.BottomMargin = app.InchesToPoints(0.75);

            // Obtener nombre del mes
            var nombreMes = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mes).ToUpper();

            CreateCell(worksheet.Range["A1", "B1"], DateTime.Now.Year)
                .SetFontBold(true)
                .SetFontSize(13)
                .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignCenter)
                .SetBorder("#CCCCFF");

            CreateCell(worksheet.Range["C1", "X1"], nombreMes)
                .SetFontBold(true)
                .SetFontSize(13)
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
                .SetColumnWidth(3)
                .SetBorder("#CCCCFF");

            worksheet.Columns["B"].ColumnWidth = 38;
            CreateCell(worksheet.Range["B3", "B3"], "Nombre")
                .SetFontBold(true)
                .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignCenter)
                .SetBackgroundColor("#CCCCFF")
                .SetBorder("#CCCCFF");

            var diasHabiles = ObtenerDiasHabiles(mes);
            int columna = 3;
            int numeroColor = 1;
            for (int i = 0; i < diasHabiles.Count; i++)
            {
                worksheet.Columns[columna].ColumnWidth = 3.57;

                CreateCell(worksheet.Cells[2, columna] as Excel.Range, diasHabiles[i].Day)
                    .SetFontBold(true)
                    .SetFontSize(8)
                    .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignCenter)
                    .SetBackgroundColor(_fondosPrimarios[numeroColor])
                    .SetBorder("#CCCCFF")
                    .SetColumnWidth(2.57);

                CreateCell(worksheet.Cells[3, columna] as Excel.Range, _inicialesDias[diasHabiles[i].DayOfWeek])
                    .SetFontBold(true)
                    .SetFontSize(8)
                    .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignCenter)
                    .SetBackgroundColor(_fondosSecundarios[numeroColor])
                    .SetBorder("#CCCCFF");

                columna += 1;
                numeroColor += diasHabiles[i].DayOfWeek == DayOfWeek.Friday ? 1 : 0;
            }

            worksheet.Columns[columna].ColumnWidth = 12;
            CreateCell(worksheet.Range[worksheet.Cells[2, columna], worksheet.Cells[3, columna]] as Excel.Range, "Asist.")
                .SetFontBold(true)
                .SetFontSize(8)
                .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignCenter)
                .SetVerticalAlignment(Excel.XlVAlign.xlVAlignCenter)
                .SetBackgroundColor("#CCCCFF")
                .SetBorder("#CCCCFF")
                .SetColumnWidth(5);

            columna += 1;
            worksheet.Columns[columna].ColumnWidth = 12;
            CreateCell(worksheet.Range[worksheet.Cells[2, columna], worksheet.Cells[3, columna]] as Excel.Range, "Inasist.")
                .SetFontBold(true)
                .SetFontSize(8)
                .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignCenter)
                .SetVerticalAlignment(Excel.XlVAlign.xlVAlignCenter)
                .SetBackgroundColor("#CCCCFF")
                .SetBorder("#CCCCFF")
                .SetColumnWidth(5);

            var alumnos = new List<Alumno>();
            using (var connection = new SQLiteConnection(BaseLocal.Cadena))
            {
                connection.Open();

                string query = "SELECT * FROM Alumno ORDER BY Id ASC;";

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

                CreateCell(worksheet.Cells[i + 4, 3 + diasHabiles.Count] as Excel.Range, string.Empty)
                    .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignCenter)
                    .SetBackgroundColor((i + 1) % 2 == 0 ? "#E7E7FF" : "#FFFFFF")
                    .SetBorder("#CCCCFF");

                CreateCell(worksheet.Cells[i + 4, 4 + diasHabiles.Count] as Excel.Range, string.Empty)
                    .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignCenter)
                    .SetBackgroundColor((i + 1) % 2 == 0 ? "#E7E7FF" : "#FFFFFF")
                    .SetBorder("#CCCCFF");
            }

            int filaObservaciones = alumnos.Count + 5;
            CreateCell(worksheet.Range[worksheet.Cells[filaObservaciones, 1], worksheet.Cells[filaObservaciones, 4 + diasHabiles.Count]] as Excel.Range, "Observaciones")
                .SetFontBold(true)
                .SetHorizontalAlignment(Excel.XlHAlign.xlHAlignCenter)
                .SetBackgroundColor("#CCCCFF")
                .SetBorder("#CCCCFF");


            for (int i = 0; i < 10; i++)
            {
                filaObservaciones += 1;

                CreateCell(worksheet.Range[worksheet.Cells[filaObservaciones, 1], worksheet.Cells[filaObservaciones, 4 + diasHabiles.Count]] as Excel.Range, string.Empty)
                    .SetFontBold(true)
                    .SetBorder("#CCCCFF");
            }

            (worksheet.Columns["B"] as Excel.Range).AutoFit();

            workbook.SaveAs(ubicacion);

            workbook.Close();
        }

        private void buttonGenerar_Click(object sender, EventArgs e)
        {
            int mesSeleccionado = (int)comboBoxMeses.SelectedValue;
            string ubicacion = ObtenerUbicacion(mesSeleccionado);

            if (ubicacion != null)
            {
                GenerarExcel(ubicacion, mesSeleccionado);

                MessageBox.Show("Archivo generado correctamente" + Environment.NewLine + $"Ubicación: {ubicacion}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                System.Diagnostics.Process.Start(ubicacion);

                this.Activate();
            }
        }

        private readonly Dictionary<DayOfWeek, string> _inicialesDias = new Dictionary<DayOfWeek, string>()
        {
            { DayOfWeek.Monday, "L" },
            { DayOfWeek.Tuesday, "M" },
            { DayOfWeek.Wednesday, "Mi" },
            { DayOfWeek.Thursday, "J" },
            { DayOfWeek.Friday, "V" }
        };

        private readonly Dictionary<int, string> _fondosPrimarios = new Dictionary<int, string>()
        {
            { 1, "#DDEBF7" },
            { 2, "#FCE4D6" },
            { 3, "#FFF2CC" },
            { 4, "#E2EFDA" },
            { 5, "#D9E1F2" }
        };

        private readonly Dictionary<int, string> _fondosSecundarios = new Dictionary<int, string>()
        {
            { 1, "#BDD7EE" },
            { 2, "#F8CBAD" },
            { 3, "#FFE699" },
            { 4, "#C6E0B4" },
            { 5, "#B4C6E7" }
        };

        private readonly Dictionary<int, string> _meses = new Dictionary<int, string>()
        {
            { 1, "Enero" },
            { 2, "Febrero" },
            { 3, "Marzo" },
            { 4, "Abril" },
            { 5, "Mayo" },
            { 6, "Junio" },
            { 7, "Julio" },
            { 8, "Agosto" },
            { 9, "Septiembre" },
            { 10, "Octubre" },
            { 11, "Noviembre" },
            { 12, "Diciembre" }
        };
    }
}
