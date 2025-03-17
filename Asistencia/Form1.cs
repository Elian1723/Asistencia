using Asistencia.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        private void buttonGenerar_Click(object sender, EventArgs e)
        {

        }
    }
}
