using System;
using System.IO;
using System.Reflection;

namespace tef_banpro_app.Tools
{
    public static class LogHelper
    {
        /// <summary>
        /// Registra un mensaje en el archivo de log
        /// </summary>
        /// <param name="mensaje"></param>
        /// <param name="fechahora"></param>
        public static void LogToFile(string mensaje, bool fechahora = true)
        {
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\log";

            // Crear el directorio en caso de no existir
            if (!Directory.Exists(assemblyFolder)) Directory.CreateDirectory(assemblyFolder);

            var path = Path.Combine(assemblyFolder, $"TEF_BanproDCL_{DateTime.Now:yyyyMMdd}.txt");

            using (var streamWriter = File.AppendText(path))
            {
                streamWriter.WriteLine(fechahora ? $"{DateTime.Now:yyyy/MM/dd: hh:mm:ss tt}: {mensaje}" : $"{mensaje}");
            }
        }
    }
}
