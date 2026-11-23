using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using tef_banpro_app.Enums;
using tef_banpro_app.Model;

namespace tef_banpro_app.Tools
{
    /// <summary>
    /// Contiene funciones para facilitar el manejo de registro de datos local
    /// </summary>
    public static class DataHelper
    {
        /// <summary>
        /// Determina si la base de datos de gestión existe
        /// </summary>
        public static bool ExisteBaseDatos()
        {
            try
            {
                using (var con = new SqlConnection(ConnectionTools.GetSqlConnectionString()))
                {
                    con.Open();
                }

                return true;
            }
            catch (SqlException sqlex)
            {
                if (sqlex.Number.Equals(4060))
                {
                    return false;
                }

                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {sqlex.Message}";
                LogHelper.LogToFile(mensaje);
                throw new ApplicationException(mensaje);
            }
        }

        /// <summary>
        /// Determina si la tabla existe
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public static bool ExisteTabla(string tableName = "DCL_TRANSACTION", string schema = "dbo")
        {
            try
            {
                using (var con = new SqlConnection(ConnectionTools.GetSqlConnectionString()))
                {
                    con.Open();

                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"SELECT COUNT (*) 
                                            FROM INFORMATION_SCHEMA.TABLES 
                                            WHERE TABLE_NAME = @TABLE_NAME AND TABLE_SCHEMA = @TABLE_SCHEMA";

                        cmd.Parameters.AddWithValue("@TABLE_NAME", tableName);
                        cmd.Parameters.AddWithValue("@TABLE_SCHEMA", schema);

                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            return reader.GetInt32(0) >= 1;
                        }
                    }
                }
            }
            catch (SqlException sqlex)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {sqlex.Message}";
                LogHelper.LogToFile(mensaje);
                throw new ApplicationException(mensaje);
            }
        }

        /// <summary>
        /// Ejecuta el script SQL
        /// </summary>
        /// <returns></returns>
        public static bool EjecutarScript()
        {
            try
            {
                var ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SqlScripts\CreateDclTransactionTable.sql");

                // Leer el script y reemplazar el nombre generico de la base de datos por el nombre deseado
                var script = File.ReadAllText(ruta, Encoding.UTF8);

                // Separar el script por comandos GO
                var commandStrings = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                using (var connection = new SqlConnection(ConnectionTools.GetSqlConnectionString()))
                {
                    connection.Open();
                    foreach (var commandString in commandStrings)
                    {
                        if (commandString.Trim() != "")
                        {
                            using (var command = new SqlCommand(commandString, connection))
                            {
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (SqlException ex)
                                {
                                    var spError = commandString.Length > 100 ? commandString.Substring(0, 100) + " ...\n..." : commandString;
                                    XtraMessageBox.Show(
                                        $"Por favor, verifique el script SQL.\nArchivo: {ruta} \nLínea: {ex.LineNumber} \nError: {ex.Message} \nComando SQL: \n{spError}", 
                                        "Creación de tabla para registro de transacciones", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return false;
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {ex.Message}";
                LogHelper.LogToFile(mensaje);
                return false;
            }
        }

        /// <summary>
        /// Inserta un registro de transacción
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static bool RegistrarTransaccion(DclTransaction transaction)
        {
            try
            {
                using (var con = new SqlConnection(ConnectionTools.GetSqlConnectionString()))
                {
                    con.Open();

                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"INSERT INTO dbo.DCL_TRANSACTION
                                            (
                                                MerchantId,
                                                TerminalId,
                                                TransactionType,
                                                ExternalReference,
                                                Invoice,
                                                CurrencySymbol,
                                                TransactionDate,
                                                TransactionTime,
                                                ReferenceNumber,
                                                AuthorizationNumber,
                                                SystemTraceNumber,
                                                Amount,
                                                TaxAmount,
                                                MaskedPan,
                                                CardHolderName
                                            )
                                            VALUES
                                            (   
	                                            @MerchantId,
                                                @TerminalId,
                                                @TransactionType,
                                                @ExternalReference,
                                                @Invoice,
                                                @CurrencySymbol,
                                                @TransactionDate,
                                                @TransactionTime,
                                                @ReferenceNumber,
                                                @AuthorizationNumber,
                                                @SystemTraceNumber,
                                                @Amount,
                                                @TaxAmount,
                                                @MaskedPan,
                                                @CardHolderName
                                            )";

                        #region Pasar valores a los párametros

                        cmd.Parameters.AddWithValue("@MerchantId", transaction.MerchantId);
                        cmd.Parameters.AddWithValue("@TerminalId", transaction.TerminalId);
                        cmd.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
                        cmd.Parameters.AddWithValue("@ExternalReference", transaction.ExternalReference ?? string.Empty);
                        cmd.Parameters.AddWithValue("@Invoice", transaction.Invoice ?? string.Empty);
                        cmd.Parameters.AddWithValue("@CurrencySymbol", transaction.CurrencySymbol ?? string.Empty);
                        cmd.Parameters.AddWithValue("@TransactionDate", transaction.TransactionDate ?? string.Empty);
                        cmd.Parameters.AddWithValue("@TransactionTime", transaction.TransactionTime ?? string.Empty);
                        cmd.Parameters.AddWithValue("@ReferenceNumber", transaction.ReferenceNumber ?? string.Empty);
                        cmd.Parameters.AddWithValue("@AuthorizationNumber", transaction.AuthorizationNumber ?? string.Empty);
                        cmd.Parameters.AddWithValue("@SystemTraceNumber", transaction.SystemTraceNumber ?? string.Empty);
                        cmd.Parameters.AddWithValue("@Amount", transaction.Amount ?? string.Empty);
                        cmd.Parameters.AddWithValue("@TaxAmount", transaction.TaxAmount ?? string.Empty);
                        cmd.Parameters.AddWithValue("@MaskedPan", transaction.MaskedPan ?? string.Empty);
                        cmd.Parameters.AddWithValue("@CardHolderName", transaction.CardHolderName ?? string.Empty);

                        #endregion

                        return cmd.ExecuteNonQuery() >= 1;
                    }
                }
            }
            catch (Exception sqlex)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {sqlex.Message}";
                LogHelper.LogToFile(mensaje);
                throw new ApplicationException(mensaje);
            }
        }

        /// <summary>
        /// Actualiza el estado de cierre de todas las transacciones pendientes de cierre.
        /// Retorna el listado de transacciones pertenecientes al lote
        /// </summary>
        /// <returns></returns>
        public static List<DclTransaction> CerrarLote(DateTime date)
        {
            try
            {
                // Simplificar la fecha
                var closingDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);

                using (var con = new SqlConnection(ConnectionTools.GetSqlConnectionString()))
                {
                    con.Open();

                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"UPDATE dbo.DCL_TRANSACTION
                                            SET Closed = 1, 
                                                ClosedDate = @ClosedDate
                                            WHERE MerchantId = @MerchantId 
	                                            AND TerminalId = @TerminalId 
	                                            AND Closed = 0";

                        

                        cmd.Parameters.AddWithValue("@ClosedDate", closingDate);
                        cmd.Parameters.AddWithValue("@MerchantId", Global.ConfigurationSettings.TransactionLoggingSettings.MerchantId);
                        cmd.Parameters.AddWithValue("@TerminalId", Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId);

                        cmd.ExecuteNonQuery();
                    }
                }

                // Retornar el listado de las transacciones del lote cerrado
                return ListarLoteCerrado(closingDate);
            }
            catch (Exception sqlex)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {sqlex.Message}";
                LogHelper.LogToFile(mensaje);
                throw new ApplicationException(mensaje);
            }
        }

        /// <summary>
        /// Retorna el listado de las transacciones de un lote cerrado
        /// </summary>
        /// <returns></returns>
        public static List<DclTransaction> ListarLoteCerrado(DateTime fechaCierre)
        {
            var transacciones = new List<DclTransaction>();

            try
            {
                using (var con = new SqlConnection(ConnectionTools.GetSqlConnectionString()))
                {
                    con.Open();

                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"SELECT MerchantId,
                                                   TerminalId,
                                                   TransactionType,
                                                   ExternalReference,
                                                   Invoice,
                                                   CurrencySymbol,
                                                   TransactionDate,
                                                   TransactionTime,
                                                   ReferenceNumber,
                                                   AuthorizationNumber,
                                                   SystemTraceNumber,
                                                   Amount,
                                                   TaxAmount,
                                                   MaskedPan,
                                                   CardHolderName,
                                                   Closed,
                                                   CreationDate,
                                                   ClosedDate
                                            FROM dbo.DCL_TRANSACTION
                                            WHERE ClosedDate = @ClosedDate
                                                    AND TransactionType <> @TransactionType
                                            ORDER BY MerchantId, TerminalId, Invoice, SystemTraceNumber";

                        cmd.Parameters.AddWithValue("@MerchantId", Global.ConfigurationSettings.TransactionLoggingSettings.MerchantId);
                        cmd.Parameters.AddWithValue("@TerminalId", Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId);
                        cmd.Parameters.AddWithValue("@TransactionType", DclTransactionTypes.Void.GetDescription());
                        cmd.Parameters.AddWithValue("@ClosedDate", fechaCierre);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows) return transacciones;

                            while (reader.Read())
                            {
                                var transaccion = new DclTransaction
                                {
                                    MerchantId = Global.ConfigurationSettings.TransactionLoggingSettings.MerchantId,
                                    TerminalId = Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId,
                                    TransactionType = reader.GetSafeString("TransactionType",""),
                                    ExternalReference = reader.GetSafeString("ExternalReference",""),
                                    Invoice = reader.GetSafeString("Invoice",""),
                                    CurrencySymbol = reader.GetSafeString("CurrencySymbol",""),
                                    TransactionDate = reader.GetSafeString("TransactionDate",""),
                                    TransactionTime = reader.GetSafeString("TransactionTime",""),
                                    ReferenceNumber = reader.GetSafeString("ReferenceNumber",""),
                                    AuthorizationNumber = reader.GetSafeString("AuthorizationNumber",""),
                                    SystemTraceNumber = reader.GetSafeString("SystemTraceNumber",""),
                                    Amount = reader.GetSafeString("Amount",""),
                                    TaxAmount = reader.GetSafeString("TaxAmount",""),
                                    MaskedPan = reader.GetSafeString("MaskedPan",""),
                                    CardHolderName = reader.GetSafeString("CardHolderName","")
                                };

                                // Agregar a la lista
                                transacciones.Add(transaccion);
                            }
                        }
                    }
                }

                return transacciones;
            }
            catch (Exception sqlex)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {sqlex.Message}";
                LogHelper.LogToFile(mensaje);
                throw new ApplicationException(mensaje);
            }
        }

        /// <summary>
        /// Retorna el listado de las transacciones del lote actual (pendientes de cerrar)
        /// </summary>
        /// <returns></returns>
        public static List<DclTransaction> ListarLote()
        {
            var transacciones = new List<DclTransaction>();

            try
            {
                using (var con = new SqlConnection(ConnectionTools.GetSqlConnectionString()))
                {
                    con.Open();

                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"SELECT MerchantId,
                                                    TerminalId,
                                                    TransactionType,
                                                    ExternalReference,
                                                    Invoice,
                                                    CurrencySymbol,
                                                    TransactionDate,
                                                    TransactionTime,
                                                    ReferenceNumber,
                                                    AuthorizationNumber,
                                                    SystemTraceNumber,
                                                    Amount,
                                                    TaxAmount, 
                                                    MaskedPan,
                                                    CardHolderName,
                                                    Closed,
                                                    CreationDate,
                                                    ClosedDate 
                                            FROM dbo.DCL_TRANSACTION D1
                                            WHERE MerchantId = @MerchantId
	                                            AND TerminalId = @TerminalId
                                                AND Closed = 0
	                                            AND TransactionType <> @TransactionType
	                                            AND NOT EXISTS (SELECT * 
					                                            FROM dbo.DCL_TRANSACTION D2 
					                                            WHERE D2.MerchantId = D1.MerchantId 
						                                            AND D2.TerminalId = D1.TerminalId 
						                                            AND D2.Invoice = D1.Invoice 
						                                            AND D2.TransactionType = @TransactionType)
                                            ORDER BY MerchantId, TerminalId, Invoice, SystemTraceNumber";

                        cmd.Parameters.AddWithValue("@MerchantId", Global.ConfigurationSettings.TransactionLoggingSettings.MerchantId);
                        cmd.Parameters.AddWithValue("@TerminalId", Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId);
                        cmd.Parameters.AddWithValue("@TransactionType", DclTransactionTypes.Void.GetDescription());

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows) return transacciones;

                            while (reader.Read())
                            {
                                var transaccion = new DclTransaction
                                {
                                    MerchantId = Global.ConfigurationSettings.TransactionLoggingSettings.MerchantId,
                                    TerminalId = Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId,
                                    TransactionType = reader.GetSafeString("TransactionType",""),
                                    ExternalReference = reader.GetSafeString("ExternalReference",""),
                                    Invoice = reader.GetSafeString("Invoice",""),
                                    CurrencySymbol = reader.GetSafeString("CurrencySymbol",""),
                                    TransactionDate = reader.GetSafeString("TransactionDate",""),
                                    TransactionTime = reader.GetSafeString("TransactionTime",""),
                                    ReferenceNumber = reader.GetSafeString("ReferenceNumber",""),
                                    AuthorizationNumber = reader.GetSafeString("AuthorizationNumber",""),
                                    SystemTraceNumber = reader.GetSafeString("SystemTraceNumber",""),
                                    Amount = reader.GetSafeString("Amount",""),
                                    TaxAmount = reader.GetSafeString("TaxAmount",""),
                                    MaskedPan = reader.GetSafeString("MaskedPan",""),
                                    CardHolderName = reader.GetSafeString("CardHolderName","")
                                };

                                // Agregar a la lista
                                transacciones.Add(transaccion);
                            }
                        }
                    }
                }

                return transacciones;
            }
            catch (Exception sqlex)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {sqlex.Message}";
                LogHelper.LogToFile(mensaje);
                throw new ApplicationException(mensaje);
            }
        }

        /// <summary>
        /// Retorna el listado de las transacciones en un rango de fechas
        /// </summary>
        /// <returns></returns>
        public static List<DclTransaction> Listar(DateTime desde, DateTime hasta)
        {
            var transacciones = new List<DclTransaction>();

            try
            {
                using (var con = new SqlConnection(ConnectionTools.GetSqlConnectionString()))
                {
                    con.Open();

                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"SELECT MerchantId,
                                                   TerminalId,
                                                   TransactionType,
                                                   ExternalReference,
                                                   Invoice,
                                                   CurrencySymbol,
                                                   TransactionDate,
                                                   TransactionTime,
                                                   ReferenceNumber,
                                                   AuthorizationNumber,
                                                   SystemTraceNumber,
                                                   Amount,
                                                   TaxAmount, 
                                                   MaskedPan,
                                                   CardHolderName,
                                                   Closed,
                                                   CreationDate,
                                                   ClosedDate 
                                            FROM dbo.DCL_TRANSACTION
                                            WHERE MerchantId = @MerchantId
	                                            AND TerminalId = @TerminalId
	                                            AND CONVERT(DATE,TransactionDate) BETWEEN @Desde AND @Hasta
                                            ORDER BY MerchantId, TerminalId, Invoice, SystemTraceNumber";

                        cmd.Parameters.AddWithValue("@MerchantId", Global.ConfigurationSettings.TransactionLoggingSettings.MerchantId);
                        cmd.Parameters.AddWithValue("@TerminalId", Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId);
                        cmd.Parameters.AddWithValue("@Desde", desde.Date);
                        cmd.Parameters.AddWithValue("@Hasta", hasta.Date);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows) return transacciones;

                            while (reader.Read())
                            {
                                var transaccion = new DclTransaction
                                {
                                    MerchantId = Global.ConfigurationSettings.TransactionLoggingSettings.MerchantId,
                                    TerminalId = Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId,
                                    TransactionType = reader.GetSafeString("TransactionType",""),
                                    ExternalReference = reader.GetSafeString("ExternalReference",""),
                                    Invoice = reader.GetSafeString("Invoice",""),
                                    CurrencySymbol = reader.GetSafeString("CurrencySymbol",""),
                                    TransactionDate = reader.GetSafeString("TransactionDate",""),
                                    TransactionTime = reader.GetSafeString("TransactionTime",""),
                                    ReferenceNumber = reader.GetSafeString("ReferenceNumber",""),
                                    AuthorizationNumber = reader.GetSafeString("AuthorizationNumber",""),
                                    SystemTraceNumber = reader.GetSafeString("SystemTraceNumber",""),
                                    Amount = reader.GetSafeString("Amount",""),
                                    TaxAmount = reader.GetSafeString("TaxAmount",""),
                                    MaskedPan = reader.GetSafeString("MaskedPan",""),
                                    CardHolderName = reader.GetSafeString("CardHolderName","")
                                };

                                // Agregar a la lista
                                transacciones.Add(transaccion);
                            }
                        }
                    }
                }

                return transacciones;
            }
            catch (Exception sqlex)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {sqlex.Message}";
                LogHelper.LogToFile(mensaje);
                throw new ApplicationException(mensaje);
            }
        }

        /// <summary>
        /// Retorna la transaccion correspondiente al tipo y número de factura dados
        /// </summary>
        /// <param name="invoice">Número de factura</param>
        /// <returns></returns>
        public static DclTransaction Obtener(string invoice)
        {
            try
            {
                using (var con = new SqlConnection(ConnectionTools.GetSqlConnectionString()))
                {
                    con.Open();

                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"SELECT TOP(1) * 
                                            FROM dbo.DCL_TRANSACTION
                                            WHERE MerchantId = @MerchantId
	                                            AND TerminalId = @TerminalId
	                                            AND Invoice = @Invoice
                                            ORDER BY MerchantId, TerminalId, Invoice, SystemTraceNumber";

                        cmd.Parameters.AddWithValue("@MerchantId", Global.ConfigurationSettings.TransactionLoggingSettings.MerchantId);
                        cmd.Parameters.AddWithValue("@TerminalId", Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId);
                        cmd.Parameters.AddWithValue("@Invoice", invoice);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows) return null;
                            reader.Read();

                            var transaccion = new DclTransaction
                            {
                                MerchantId = Global.ConfigurationSettings.TransactionLoggingSettings.MerchantId,
                                TerminalId = Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId,
                                TransactionType = reader.GetSafeString("TransactionType",""),
                                ExternalReference = reader.GetSafeString("ExternalReference",""),
                                Invoice = reader.GetSafeString("Invoice",""),
                                CurrencySymbol = reader.GetSafeString("CurrencySymbol",""),
                                TransactionDate = reader.GetSafeString("TransactionDate",""),
                                TransactionTime = reader.GetSafeString("TransactionTime",""),
                                ReferenceNumber = reader.GetSafeString("ReferenceNumber",""),
                                AuthorizationNumber = reader.GetSafeString("AuthorizationNumber",""),
                                SystemTraceNumber = reader.GetSafeString("SystemTraceNumber",""),
                                Amount = reader.GetSafeString("Amount",""),
                                TaxAmount = reader.GetSafeString("TaxAmount",""),
                                MaskedPan = reader.GetSafeString("MaskedPan",""),
                                CardHolderName = reader.GetSafeString("CardHolderName","")
                            };

                            return transaccion;
                        }
                    }
                }
            }
            catch (Exception sqlex)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {sqlex.Message}";
                LogHelper.LogToFile(mensaje);
                throw new ApplicationException(mensaje);
            }
        }
    }
}
