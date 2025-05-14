using System;
using System.Data.SQLite;
using System.IO;

namespace Asistencia
{
    public static class BaseLocal
    {
        public static string Cadena { get; } = @"Data Source=C:\Asistencia\asistencia.sqlite;Version=3;";

        public static bool Comprobar()
        {
            try
            {
                if (!Directory.Exists(@"C:\Asistencia"))
                {
                    Directory.CreateDirectory(@"C:\Asistencia");
                }

                if (!File.Exists(@"C:\Asistencia\asistencia.sqlite"))
                {
                    SQLiteConnection.CreateFile(@"C:\Asistencia\asistencia.sqlite");
                }

                using (var connection = new SQLiteConnection(Cadena))
                {
                    connection.Open();

                    string query = @"
                            CREATE TABLE IF NOT EXISTS Alumno(
	                            Id INTEGER PRIMARY KEY,
	                            Nombre TEXT NOT NULL
                            );";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
