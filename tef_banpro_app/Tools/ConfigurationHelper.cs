using System;
using System.IO;
using System.Text;
using tef_banpro_app.Model;

namespace tef_banpro_app.Tools
{
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Lee el archivo especificado y deserealiza los datos en un objeto de tipo <see cref="ConfigurationSettings"/>
        /// </summary>
        /// <param name="ruta"></param>
        /// <returns>Configuracion</returns>
        /// <exception cref="ApplicationException"></exception>
        public static ConfigurationSettings Load(string ruta)
        {
            try
            {
                if (!File.Exists(ruta))
                {
                    var message = $"El archivo de configuración {Path.GetFileName(ruta)} no existe.";
                    LogHelper.LogToFile(message);
                    return null;
                }

                // Obtener el texto XML
                var xmlConfig = File.ReadAllText(ruta);

                // Deserializar e inicializar la variable global
                var config = SerializationHelper.Deserealize<ConfigurationSettings>(xmlConfig);

                if (config.VerboseMode)
                {
                    LogHelper.LogToFile("El archivo de configuración fue leído exitosamente.");
                    LogHelper.LogToFile(xmlConfig,false);
                }

                return config;
            }
            catch (Exception e)
            {
                LogHelper.LogToFile($"Error leyendo el archivo de configuración. {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Guarda el objeto <see cref="ConfigurationSettings"/> en un archivo de texto.
        /// </summary>
        /// <param name="configurationSettings">The configuracion.</param>
        public static void Save(ConfigurationSettings configurationSettings)
        {
            try
            {
                // Serializar
                var xmlConfiguracion = SerializationHelper.Serialize(configurationSettings);

                // Guardar los cambios en el archivo
                File.WriteAllText(Global.ConfigFilePath, xmlConfiguracion, Encoding.Unicode);

                if (Global.ConfigurationSettings.VerboseMode) LogHelper.LogToFile("El archivo de configuración fue generado exitosamente.");
            }
            catch (Exception e)
            {
                LogHelper.LogToFile($"Error generando el archivo de configuración. {e.Message}");
                throw;
            }
        }
    }
}
