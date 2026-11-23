using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Kinpos.comm;
using Kinpos.Dcl.Util;
using tef_banpro_app.Enums;
using tef_banpro_app.Forms;
using tef_banpro_app.Tools;

namespace tef_banpro_app
{
    static class Program
    {
        /// <summary>
        /// Entrada principal del programa
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                #region Configuracion regional

                var culture = CultureInfo.CreateSpecificCulture("es-SV");
                Thread.CurrentThread.CurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;

                #endregion

                #region Parametros de configuracion

                // Cargar la configuración
                Global.ConfigurationSettings = ConfigurationHelper.Load(Global.ConfigFilePath);

                // Intentar leer el registro de Windows para obtener los datos necesarios para acceder a la base de datos
                if (!RegistryHelper.LeerRegistro()) return 1;

                #endregion

                if (args.Length == 0)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new TransactionScreen());
                }
                else if (args.Length == 1 && args[0].ToLower() == "/l" | args[0].ToLower() == "-l")
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new TransactionListScreen());
                }
                else if (args.Length == 1 && args[0].ToLower() == "/x" | args[0].ToLower() == "-x")
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    XtraMessageBox.AllowHtmlText = true;
                    var dialogResult = XtraMessageBox.Show($@"¿Desea realizar la consulta de lote de la terminal <b>{Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId}</b>?", $@"Integración TEF ICG / {Properties.Settings.Default.P1} (DCL)", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.Yes)
                    {
                        // Obtener el listado de las transacciones
                        var transacciones = DataHelper.ListarLote();
                        if (!transacciones.Any())
                        {
                            XtraMessageBox.Show("No hay transacciones.", "Consulta de Lote", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            #region Totalizar

                            var total = 0.0;
                            foreach (var transaccion in transacciones)
                            {
                                var monto = Convert.ToDouble(transaccion.Amount) / 100;
                                total += monto;
                            }

                            #endregion

                            dialogResult = XtraMessageBox.Show($"Transacciones: {transacciones.Count}{Environment.NewLine}" +
                                                               $"Total: {total:N2}" +
                                                               $"{Environment.NewLine}" +
                                                               $"{Environment.NewLine}" +
                                                               "¿Desea imprimir el reporte?", "Consulta de Lote", 
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (dialogResult == DialogResult.Yes) PrinterHelper.ImprimirReporte(transacciones);
                        }
                    }
                }
                else if (args.Length == 1 && args[0].ToLower() == "/z" | args[0].ToLower() == "-z")
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    XtraMessageBox.AllowHtmlText = true;
                    var dialogResult = XtraMessageBox.Show($@"¿Desea realizar el cierre de lote de la terminal <b>{Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId}</b>?", $@"Integración TEF ICG / {Properties.Settings.Default.P1} (DCL)", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.Yes)
                    {
                        // Obtener el listado de las transacciones
                        var transacciones = DataHelper.ListarLote();
                        if (!transacciones.Any())
                        {
                            XtraMessageBox.Show("No hay transacciones.", "Cierre de Lote", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            #region Totalizar

                            var total = 0.0;
                            foreach (var transaccion in transacciones)
                            {
                                var monto = Convert.ToDouble(transaccion.Amount) / 100;
                                total += monto;
                            }

                            #endregion

                            // Cerrar las transacciones a nivel de DCL
                            var cerrado = CerrarLoteDcl();
                            if (cerrado)
                            {
                                // Cerrar las transacciones en la tabla de registro
                                var transaccionesCerradas = DataHelper.CerrarLote(DateTime.Now);

                                dialogResult = XtraMessageBox.Show($"Transacciones: {transacciones.Count}" +
                                                                   $"{Environment.NewLine}" +
                                                                   $"Total: {total:N2}" +
                                                                   $"{Environment.NewLine}{Environment.NewLine}" +
                                                                   "¿Desea imprimir el reporte de cierre?", "Cierre de Lote", 
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (dialogResult == DialogResult.Yes) PrinterHelper.ImprimirReporte(transaccionesCerradas, true);
                            }
                        }
                    }
                }
                else if (args.Length >= 1 && args[0].ToLower() == "/config" | args[0].ToLower() == "-config" | args[0].ToLower() == "/c" | args[0].ToLower() == "-c")
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    if (Global.ConfigurationSettings != null)
                    {
                        // Obtener la clave actual
                        var currentPassword = Global.ConfigurationSettings.AppPassword;

                        if (!string.IsNullOrEmpty(currentPassword))
                        {
                            #region Solicitar la contraseña

                            // Mostrar la pantalla para el comentario
                            var arguments = new XtraInputBoxArgs();

                            // Inicializar las propiedades de la ventana mergente
                            arguments.Caption = Properties.Settings.Default.P1;
                            arguments.Prompt = "Ingrese la contraseña por favor:";
                            arguments.DefaultButtonIndex = 0;
                            arguments.Showing += Args_Showing;

                            // Inicializar el control y establecer las propiedades necesarias
                            var editor = new TextEdit();
                            editor.Properties.MaxLength = 100;
                            editor.Properties.PasswordChar = '*';
                            arguments.Editor = editor;

                            // Mostrar la ventana emergente
                            var inputResult = XtraInputBox.Show(arguments);
                            var contrasena = (string) inputResult;

                            // Encriptar la clave digitada
                            var password = SecurityTools.Encrypt(contrasena);

                            if (!password.Equals(currentPassword))
                            {
                                XtraMessageBox.Show("La contraseña digitada no es correcta.", Properties.Settings.Default.P1, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return 0;
                            }

                            #endregion
                        }
                    }
                    
                    // Cargar la pantalla
                    Application.Run(new ConfigurationScreen());
                }
                else
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    XtraMessageBox.AllowHtmlText = true;
                    XtraMessageBox.Show($@"Argumentos no reconocidos: <b>{string.Join(" ", args)}</b>", $@"Integración TEF ICG / {Properties.Settings.Default.P1} (DCL)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Retornar 0 al POS indicando que no hubo problema
                return 0;
            }
            catch (Exception e)
            {
                LogHelper.LogToFile($@"Integración TEF ICG / {Properties.Settings.Default.P1} (DCL): {e.Message}");
                
                // Retornar 1 al POS indicando que ocurrió un problema
                return 1;
            }
        }

        /// <summary>
        /// Realizar el cierre de un lote mediante la libreria DCL
        /// </summary>
        /// <returns></returns>
        private static bool CerrarLoteDcl()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Preparar la solicitud
                var dcl = new DCL_Ethernet
                {
                    IP = Global.ConfigurationSettings.DeviceSettings.HostIp,
                    Port = Global.ConfigurationSettings.DeviceSettings.Port,
                    Timeout = Global.ConfigurationSettings.DeviceSettings.Timeout,
                    OpenChannel = true
                };

                #region Parametros de la transaccion

                dcl.setSettlement_AskForPassword(false);
                dcl.setSettlement_ShowTotal(true);
                dcl.setSettlement_SelectMerchant(true);

                #endregion

                #region Enviar la peticion

                var dclResult = dcl.Settlement(null);

                // Cerrar la comunicacion
                dcl.CloseChannel();

                if (dclResult != null)
                {
                    if (dcl.DCL_ResponseCode == DclResponseCodes.Ok)
                    {
                        LogHelper.LogToFile(@"Transacción: Cierre de Lote");
                        LogHelper.LogToFile($@"Response Code: {dclResult.GetValue_ASCII2String(0)}");
                        LogHelper.LogToFile($@"STAN: {dclResult.GetValue_BCD2String(4)}");
                        LogHelper.LogToFile($@"Totales: {dclResult.GetValue_ASCII2String(2)}");
                    }
                    else
                    {
                        // Otro error
                        var descripcion = Utilidad.GetDCLResponseText(dcl.DCL_ResponseCode);
                        throw new ApplicationException(descripcion);
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {ex.Message}";
                LogHelper.LogToFile(mensaje);
                XtraMessageBox.Show(ex.Message, "Cierre de Lote", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            return true;
        }

        private static void Args_Showing(object sender, XtraMessageShowingArgs e)
        {
            e.Form.Icon = null;
        }
    }
}
