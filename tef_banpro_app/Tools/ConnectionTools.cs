using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace tef_banpro_app.Tools
{
    public static class ConnectionTools
    {
        /// <summary>
        /// Retorna la cadena de conexion para la base de datos de gestion
        /// </summary>
        /// <returns>Un objeto tipo SqlConnection</returns>
        public static string GetSqlConnectionString()
        {
            try
            {
                return string.Format(Global.ConnectionString, 
                    Global.ServerName, 
                    GetDatabaseName(),
                    Global.ConfigurationSettings.SqlSettings.User, 
                    SecurityTools.Decrypt(Global.ConfigurationSettings.SqlSettings.Password));
            }
            catch (Exception e)
            {
                var message = $@"Error en {MethodBase.GetCurrentMethod().Name}. {e.Message}";
                LogHelper.LogToFile(message);
                return null;
            }
        }

        /// <summary>
        /// Obtiene el nombre de la base de datos de gestion
        /// </summary>
        /// <returns></returns>
        private static string GetDatabaseName()
        {
            try
            {
                using (var con = new SqlConnection(string.Format(Global.ConnectionString,
                    Global.ServerName,
                    Global.GeneralDatabaseName,
                    Global.ConfigurationSettings.SqlSettings.User,
                    SecurityTools.Decrypt(Global.ConfigurationSettings.SqlSettings.Password))))
                {
                    con.Open();

                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"SELECT TOP(1) PATHBD 
                                            FROM dbo.EMPRESAS
                                            WHERE CODEMPRESA = @CODEMPRESA";
                        cmd.Parameters.AddWithValue("@CODEMPRESA", Global.CompanyId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows) return null;

                            reader.Read();
                            var ruta = reader.GetString(0);
                            var valores = ruta.Split(':');
                            return valores[1];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var message = $@"Error en {MethodBase.GetCurrentMethod().Name}. {e.Message}";
                LogHelper.LogToFile(message);
            }

            return null;
        }
    }
}
