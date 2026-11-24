using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using tef_banpro_app.Model;

namespace tef_banpro_app.Tools
{
    public static class TransactionHelper
    {
        /// <summary>
        /// Lee el archivo de entrada y lo deserealiza en un objeto <see cref="OperacionesIcg"/>
        /// </summary>
        /// <param name="ruta">The ruta.</param>
        /// <returns>OperacionesIcg.</returns>
        /// <exception cref="ApplicationException"></exception>
        public static OperacionesIcg LoadInputFile(string ruta)
        {
            try
            {
                if (!File.Exists(ruta))
                {
                    var message = $"El archivo de entrada {Path.GetFileName(ruta)} no existe.";
                    throw new ApplicationException(message);
                }

                // Obtener el texto XML
                var xmlEntrada = File.ReadAllText(ruta);

                // Deserializar e inicializar la variable global
                var transaccion = SerializationHelper.Deserealize<OperacionesIcg>(xmlEntrada);

                if (Global.ConfigurationSettings.VerboseMode)
                {
                    LogHelper.LogToFile("El archivo de entrada fue leído exitosamente:");
                    LogHelper.LogToFile(xmlEntrada,false);
                }

                return transaccion;
            }
            catch (Exception e)
            {
                LogHelper.LogToFile($"Error leyendo el archivo de entrada. {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtener los datos de la transaccion
        /// </summary>
        /// <param name="serie"></param>
        /// <param name="numero"></param>
        /// <param name="icgTipoApp"></param>
        /// <returns></returns>
        public static TransaccionVenta GetTransaccionVenta(string serie, int numero, string icgTipoApp)
        {
            try
            {
                using (var con = new SqlConnection(ConnectionTools.GetSqlConnectionString()))
                {
                    con.Open();

                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = icgTipoApp == Global.IcgTipoApp.MgrFrtFht 
                            ? @"SELECT  TOP(1) TOTALNETO, 
                                        TOTALIMPUESTOS, 
                                        DTOCOMERCIAL 
                                FROM    dbo.FACTURASVENTA
                                WHERE   NUMSERIE = @NUMSERIE
                                        AND NUMFACTURA = @NUMFACTURA 
                                        AND N = 'B'" 
                            : @"SELECT	SUM(TOTAL) AS TOTALNETO, 
		                                SUM(TOTIVA + TOTREQ) AS TOTALIMPUESTOS, 
		                                SUM(TOTDTOCOMERC) AS DTOCOMERCIAL
                                FROM	dbo.TIQUETSTOT
                                WHERE	SERIE = @NUMSERIE
		                                AND NUMERO = @NUMFACTURA
		                                AND N = 'B'";

                        cmd.Parameters.AddWithValue("@NUMSERIE", serie);
                        cmd.Parameters.AddWithValue("@NUMFACTURA", numero);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                return new TransaccionVenta
                                {
                                    Serie = serie,
                                    Numero = numero,
                                    TotalNeto = reader.GetDouble(reader.GetOrdinal("TOTALNETO")),
                                    TotalImpuestos = reader.GetDouble(reader.GetOrdinal("TOTALIMPUESTOS")),
                                    DescuentoComercial = reader.GetDouble(reader.GetOrdinal("DTOCOMERCIAL"))
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.LogToFile($"Error obteniendo los datos adicionales de la transacción de venta. {e.Message}");
            }

            return null;
        }

        /// <summary>
        /// Obtener los datos de la transaccion
        /// </summary>
        /// <param name="serie"></param>
        /// <param name="numero"></param>
        /// <param name="posicion"></param>
        /// <param name="referencia"></param>
        /// <param name="icgTipoApp"></param>
        /// <returns></returns>
        public static TransaccionVenta GetTransaccionVenta(string serie, int numero, int posicion, string referencia, string icgTipoApp)
        {
            try
            {
                using (var con = new SqlConnection(ConnectionTools.GetSqlConnectionString()))
                {
                    con.Open();

                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = icgTipoApp == Global.IcgTipoApp.MgrFrtFht 
                            ? @"SELECT  TOP(1) IMPORTE
                                FROM    dbo.TESORERIA
                                WHERE   SERIE = @SERIE 
                                        AND NUMERO = @NUMERO 
                                        AND POSICION = @POSICION 
                                        AND NUMTXNTEF = @NUMTXNTEF" 
                            : @"SELECT  TOP(1) IMPORTE
                                FROM    dbo.TIQUETSPAG
                                WHERE   SERIE = @SERIE
                                        AND NUMERO = @NUMERO 
                                        AND NUMLINEA = @POSICION 
                                        AND NUMTXNTEF = @NUMTXNTEF";

                        cmd.Parameters.AddWithValue("@SERIE", serie);
                        cmd.Parameters.AddWithValue("@NUMERO", numero);
                        cmd.Parameters.AddWithValue("@POSICION", posicion);
                        cmd.Parameters.AddWithValue("@NUMTXNTEF", referencia);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                return new TransaccionVenta
                                {
                                    Serie = serie,
                                    Numero = numero,
                                    TotalNeto = reader.GetDouble(reader.GetOrdinal("IMPORTE"))
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.LogToFile($"Error obteniendo los datos adicionales de la transacción de venta. {e.Message}");
            }

            return null;
        }
    }
}
