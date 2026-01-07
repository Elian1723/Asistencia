using System;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Asistencia
{
    public static class BaseLocal
    {
        public static string Cadena { get; } = @"Data Source=C:\Asistencia\asistencia.sqlite;Version=3;";

        static BaseLocal()
        {
            // Cargar SQLite.Interop.dll antes de crear conexiones
            LoadSQLiteInterop();
        }

        private static void LoadSQLiteInterop()
        {
            try
            {
                // Obtener la carpeta donde está el ejecutable
                string appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                
                // Detectar arquitectura (x86 o x64)
                string architecture = IntPtr.Size == 8 ? "x64" : "x86";
                
                // Ruta esperada del archivo SQLite.Interop.dll
                string interopPath = Path.Combine(appDirectory, architecture, "SQLite.Interop.dll");

                if (File.Exists(interopPath))
                {
                    // Cargar la DLL de forma explícita
                    IntPtr handle = LoadLibrary(interopPath);
                    if (handle == IntPtr.Zero)
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        throw new DllNotFoundException($"No se pudo cargar SQLite.Interop.dll desde {interopPath}. Error: {errorCode}");
                    }
                }
                else
                {
                    throw new FileNotFoundException($"SQLite.Interop.dll no encontrado en {interopPath}");
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error al cargar SQLite.Interop.dll:\n\n{ex.Message}",
                    "Error de inicialización",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error
                );
                throw;
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        public static bool Comprobar()
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
    }
}