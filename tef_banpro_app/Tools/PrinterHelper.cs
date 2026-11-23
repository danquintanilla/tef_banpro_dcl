using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using InnovacionesCloud.Printing;
using tef_banpro_app.Model;

namespace tef_banpro_app.Tools
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public static class PrinterHelper
    {
        private static readonly RawPrinterHelper ERawPrinterHelper = new RawPrinterHelper();

        #region Constantes para formato de impresión ESC/POS

        private static readonly string EClear = "" + (char)27 + "@";
        private static readonly string ECentre = "" + (char)27 + (char)97 + (char)1;
        private static readonly string ELeft = "" + (char)27 + (char)97 + (char)0;
        private static readonly string ERight = "" + (char)27 + (char)97 + "2";
        private static readonly string EDrawer = "" + (char)27 + "@" + (char)27 + "p" + (char)0 + ".}";
        private static readonly string ECut = "" + (char)27 + "i" + "\r\n";
        private static readonly string ESmlText = "" + (char)27 + "!" + 1;
        private static readonly string ENmlText = "" + (char)27 + "!" + 0;
        private static readonly string EEmphOn = "" + (char)27 + "!" + 8;
        private static readonly string EInit = "" + (char)27 + "!" + 0 + (char)13 + (char)27 + "c6" + (char)1 + (char)27 + "R3" + "\r\n";
        private static readonly string EBigCharOn = "" + (char)27 + "!" + (char)56;
        private static readonly string EBigCharOff = "" + (char)27 + "!" + (char)0;

        #endregion

        #region Métodos para control de impresión

        /// <summary>
        /// Envia el comando de impresión para una línea.
        /// </summary>
        /// <param name="linea">The linea.</param>
        private static void Enviar(string linea)
        {
            ERawPrinterHelper.SendStringToPrinter(Global.ConfigurationSettings.PrinterName, linea + "\n");
        }

        /// <summary>
        /// Imprime una línea de texto libre. Se utiliza alineación Centrada.
        /// </summary>
        private static void TextoLibre(string texto)
        {
            Enviar( ECentre + texto);
        }

        /// <summary>
        /// Imprime una línea de sepatador
        /// </summary>
        private static void Separador()
        {
            const string myString = "";
            ERawPrinterHelper.SendStringToPrinter(Global.ConfigurationSettings.PrinterName, ECentre + myString.PadRight(38, Convert.ToChar("-")) + "\n");
        }

        /// <summary>
        /// Envía un comando de corte de papel
        /// </summary>
        private static void Cortar()
        {
            // Realizar el corte
            Enviar("\n" + "\n" + "\n" + ECut);
        }

        #endregion

        #region Métodos para impresión de transacciones y reportes

        /// <summary>
        /// Imprime o reimprime una transaccion DCL
        /// </summary>
        /// <param name="transaccion"></param>
        /// <param name="copiaComercio"></param>
        /// <param name="reimpresion"></param>
        /// <returns></returns>
        public static bool ImprimirTransaccion(DclTransaction transaccion, bool copiaComercio = true, bool reimpresion = false)
        {
            try
            {
                // Abrir el puerto de la impresora
                if (ERawPrinterHelper.PrinterIsOpen == false) 
                    ERawPrinterHelper.OpenPrint(Global.ConfigurationSettings.PrinterName);

                // Si es reimpresión, agregar la etiqueta
                if (reimpresion) TextoLibre("- REIMPRESION -");

                #region Transformaciones de datos

                var anio = Convert.ToInt32(transaccion.TransactionDate.Substring(0, 4));
                var mes = Convert.ToInt32(transaccion.TransactionDate.Substring(4, 2));
                var dia = Convert.ToInt32(transaccion.TransactionDate.Substring(6, 2));
                var fecha = new DateTime(anio, mes, dia);

                var importe = Convert.ToDouble(transaccion.Amount) / 100;

                #endregion

                // Establecer el formato
                Enviar(EClear + EInit + ECentre + ESmlText + EBigCharOff);

                #region Encabezado

                // Enviar las lineas
                Enviar(Properties.Settings.Default.P1);
                Enviar(Global.ConfigurationSettings.TransactionLoggingSettings.Header1);
                Enviar(Global.ConfigurationSettings.TransactionLoggingSettings.Header2);
                Enviar(Global.ConfigurationSettings.TransactionLoggingSettings.Header3);
                Enviar("");

                #endregion

                #region Detalle

                Enviar($"{fecha:yyyy/MM/dd} {transaccion.TransactionTime}");
                Enviar($"AUT: {transaccion.AuthorizationNumber} STAN: {transaccion.SystemTraceNumber}");
                Enviar(transaccion.MaskedPan);
                Enviar($"COMERCIO ID: {transaccion.MerchantId}");
                Enviar($"TERMINAL ID: {transaccion.TerminalId}");
                Enviar($"REFERENCIA: {transaccion.ReferenceNumber}");
                Enviar($"BOLETA: {transaccion.Invoice}");
                Enviar("");
                Enviar($"{transaccion.TransactionType.ToUpper()}: {transaccion.CurrencySymbol}{importe:N2}");
                Enviar("");

                if (copiaComercio)
                {
                    if (!string.IsNullOrEmpty(transaccion.CardHolderName))
                    {
                        Enviar("x_____________________________________");
                        Enviar($"{transaccion.CardHolderName}");
                    }
                    else
                    {
                        Enviar("x_____________________________________");
                        Enviar("FIRMA DEL TARJETAHABIENTE");
                    }
                    Enviar("COMERCIO COPIA");
                }
                else
                {
                    Enviar("CLIENTE COPIA");
                }

                #endregion

                #region Pie de página

                Enviar("");
                Enviar("");
                Enviar("");

                #endregion

                // Cortar el papel
                Cortar();

                // Cerrar la impresión
                ERawPrinterHelper.ClosePrint();

                return true;
            }
            catch (Exception e)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {e.Message}";
                LogHelper.LogToFile(mensaje);
            }

            return false;
        }

        /// <summary>
        /// Imprime el reporte de lote
        /// </summary>
        /// <param name="transacciones"></param>
        /// <param name="cierre"></param>
        /// <returns></returns>
        public static bool ImprimirReporte(List<DclTransaction> transacciones, bool cierre = false)
        {
            try
            {
                // Abrir el puerto de la impresora
                if (ERawPrinterHelper.PrinterIsOpen == false) 
                    ERawPrinterHelper.OpenPrint(Global.ConfigurationSettings.PrinterName);

                // Establecer el formato
                Enviar(EClear + EInit + ECentre + ESmlText + EBigCharOff);

                #region Transformaciones y totalizaciones

                var total = 0.0;
                foreach (var transaccion in transacciones)
                {
                    var monto = Convert.ToDouble(transaccion.Amount) / 100;
                    total += monto;
                }

                #endregion

                #region Encabezado

                // Enviar las lineas
                Enviar(Properties.Settings.Default.P1);
                Enviar(Global.ConfigurationSettings.TransactionLoggingSettings.Header1);
                Enviar(Global.ConfigurationSettings.TransactionLoggingSettings.Header2);
                Enviar(Global.ConfigurationSettings.TransactionLoggingSettings.Header3);
                Enviar("");
                Enviar(cierre ? "REPORTE DE CIERRE DE LOTE" : "CONSULTA DE LOTES");
                Enviar("");
                Enviar($"{DateTime.Now.Date:dd/MM/yyyy} {DateTime.Now:HH:mm:ss}");
                Enviar("");
                Enviar($"MERCHANT ID: {Global.ConfigurationSettings.TransactionLoggingSettings.MerchantId}");
                Enviar($"TERMINAL ID: {Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId}");
                Enviar("");
                Enviar($"TRANSACCIONES: {transacciones.Count}");
                Enviar($"TOTAL COMPRAS: {total:N2}");
                Enviar("");

                #endregion

                #region Detalle

                Separador();

                foreach (var transaccion in transacciones)
                {
                    var monto = Convert.ToDouble(transaccion.Amount) / 100;
                    Enviar($"{transaccion.TransactionType} {transaccion.MaskedPan.Right(6)} {transaccion.CurrencySymbol} {monto:N2}");
                }

                Separador();

                #endregion

                #region Pie de página

                Enviar("");
                Enviar("");
                Enviar("");

                #endregion

                // Cortar el papel
                Cortar();

                // Cerrar la impresión
                ERawPrinterHelper.ClosePrint();

                return true;
            }
            catch (Exception e)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {e.Message}";
                LogHelper.LogToFile(mensaje);
            }

            return false;
        }

        #endregion
    }
}