using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using DevExpress.Skins.XtraForm;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraSplashScreen;
using DevExpress.XtraTab.ViewInfo;
using Kinpos.comm;
using Kinpos.Dcl;
using Kinpos.Dcl.Core;
using Kinpos.Dcl.Util;
using tef_banpro_app.Enums;
using tef_banpro_app.Model;
using tef_banpro_app.Tools;

namespace tef_banpro_app.Forms
{
    public partial class ConfigurationScreen : XtraForm
    {
        public ConfigurationScreen()
        {
            InitializeComponent();
            Text = $@"Integración TEF ICG / {Properties.Settings.Default.P1} (DCL)";

            if (Global.ConfigurationSettings == null)
            {
                // Crear el objeto
                Global.ConfigurationSettings = new ConfigurationSettings();

                // Generar el archivo xml
                ConfigurationHelper.Save(Global.ConfigurationSettings);

                // Cargar la configuración
                Global.ConfigurationSettings = ConfigurationHelper.Load(Global.ConfigFilePath);
            }

            // Preparar
            CargarMonedas();
            CargarTiposTransacciones();
            CargarImpresoras();

            // Cargar los parametros en la pantalla
            MostrarParametros();
            CargarFormasPago();
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        /// <summary>
        /// Se encarga de centrar el titulo del formulario
        /// </summary>
        /// <returns></returns>
        protected override FormPainter CreateFormBorderPainter()
        {
            const HorzAlignment formCaptionAlignment = HorzAlignment.Center;
            return new CustomFormPainter(this, LookAndFeel, formCaptionAlignment);
        }

        /// <summary>
        /// Carga el listado de monedas disponibles
        /// </summary>
        private void CargarMonedas()
        {
            var currencies = new List<Currency>
            {
                new Currency {Code = 340, Name = "340 - Lempira"}, 
                new Currency {Code = 558, Name = "558 - Córdoba"}, 
                new Currency {Code = 840, Name = "840 - Dólar"}
            };

            rlkpCurrencyCode.DataSource = currencies;
            rlkpCurrencyCode.Columns.Add(new LookUpColumnInfo("Code"));
            rlkpCurrencyCode.Columns.Add(new LookUpColumnInfo("Name"));
            rlkpCurrencyCode.Columns[0].Visible = false;
            rlkpCurrencyCode.ShowHeader = false;
            rlkpCurrencyCode.ValueMember = "Code";
            rlkpCurrencyCode.DisplayMember = "Name";
        }

        /// <summary>
        /// Carga el listado de tipos de transacciones soportadas
        /// </summary>
        private void CargarTiposTransacciones()
        {
            var transactionTypes = new List<DclTransactionType>();

            if (Properties.Settings.Default.T1) transactionTypes.Add(new DclTransactionType {Code = (int)DclTransactionTypes.Sale, Description = DclTransactionTypes.Sale.GetDescription()});
            if (Properties.Settings.Default.T2) transactionTypes.Add(new DclTransactionType {Code = (int)DclTransactionTypes.Points, Description = DclTransactionTypes.Points.GetDescription()});
            if (Properties.Settings.Default.T3) transactionTypes.Add(new DclTransactionType {Code = (int)DclTransactionTypes.Payphone, Description = DclTransactionTypes.Payphone.GetDescription()});
            if (Properties.Settings.Default.T4) transactionTypes.Add(new DclTransactionType {Code = (int)DclTransactionTypes.Extrafinancing, Description = DclTransactionTypes.Extrafinancing.GetDescription()});
            if (Properties.Settings.Default.T5) transactionTypes.Add(new DclTransactionType {Code = (int)DclTransactionTypes.Intrafinancing, Description = DclTransactionTypes.Intrafinancing.GetDescription()});

            #region Opciones que dependen de tipos de transacciones

            // Boton para consulta de puntos
            btnConsultaPuntos.Visible = Properties.Settings.Default.T2;
            
            // Columna de cuotas
            colQuotas.Visible = Properties.Settings.Default.T4 & Properties.Settings.Default.T5;

            #endregion

            rlkpTransactionType.DataSource = transactionTypes;
            rlkpTransactionType.Columns.Add(new LookUpColumnInfo("Code"));
            rlkpTransactionType.Columns.Add(new LookUpColumnInfo("Description"));
            rlkpTransactionType.Columns[0].Visible = false;
            rlkpTransactionType.ShowHeader = false;
            rlkpTransactionType.ValueMember = "Code";
            rlkpTransactionType.DisplayMember = "Description";
        }

        /// <summary>
        /// Mostrar los datos de la configuracion en pantalla
        /// </summary>
        private void MostrarParametros()
        {
            txtIp.EditValue = Global.ConfigurationSettings.DeviceSettings.HostIp;
            spinPort.EditValue = Convert.ToInt32(Global.ConfigurationSettings.DeviceSettings.Port);
            spinTimeout.EditValue = Convert.ToInt32(Global.ConfigurationSettings.DeviceSettings.Timeout);
            chkVerboseMode.Checked = Global.ConfigurationSettings.VerboseMode;
            chkTransactionInfoScreen.Checked = Global.ConfigurationSettings.TransactionInfoScreen;
            cboRegion.SelectedIndex = Global.ConfigurationSettings.RegionalSettings.RegionId;
            chkReportarImpuestos.Checked = Global.ConfigurationSettings.RegionalSettings.ReportarImpuestos;
            chkUsarAnularEnLugarDeDevolver.Checked = Global.ConfigurationSettings.RegionalSettings.UsarAnularEnLugarDeDevolver;
            chkRegistrarTransacciones.Checked = Global.ConfigurationSettings.TransactionLogging;
            chkImpresionLocal.Checked = Global.ConfigurationSettings.LocalPrinting;
            cboPrinter.Text = Global.ConfigurationSettings.PrinterName;
            txtMerchantId.Text = Global.ConfigurationSettings.TransactionLoggingSettings.MerchantId;
            txtTerminalId.Text = Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId;
            txtHeader1.Text = Global.ConfigurationSettings.TransactionLoggingSettings.Header1;
            txtHeader2.Text = Global.ConfigurationSettings.TransactionLoggingSettings.Header2;
            txtHeader3.Text = Global.ConfigurationSettings.TransactionLoggingSettings.Header3;
        }

        /// <summary>
        /// Enlaza la lista de formas de pago con el grid
        /// </summary>
        private void CargarFormasPago()
        {
            try
            {
                // Enlazar el grid con la lista de terminales
                var formasPago = new BindingList<FormaPago>(Global.ConfigurationSettings.FormasPago);
                gridControlFormasPago.DataSource = formasPago;
            }
            catch (Exception e)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {e.Message}";
                LogHelper.LogToFile(mensaje);
                XtraMessageBox.Show(e.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Cargar la lisa de impresoras del sistema
        /// </summary>
        private void CargarImpresoras()
        {
            try
            {
                cboPrinter.Properties.Items.Clear();
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    cboPrinter.Properties.Items.Add(printer);
                }
            }
            catch (Exception e)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {e.Message}";
                LogHelper.LogToFile(mensaje);
                XtraMessageBox.Show(e.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Guarda la configuracion en el archivo XML
        /// </summary>
        private bool Guardar()
        {
            IOverlaySplashScreenHandle handle = null;

            try
            {
                var options = new OverlayWindowOptions
                (
                    opacity: 0.5,
                    fadeIn: false,
                    fadeOut: false
                );

                handle = SplashScreenManager.ShowOverlayForm(this, options);

                #region Validaciones de parámetros

                if (Global.ConfigurationSettings.TransactionLogging)
                {
                    if (string.IsNullOrEmpty(txtMerchantId.Text))
                    {
                        XtraMessageBox.AllowHtmlText = true;
                        XtraMessageBox.Show("El campo <b>Comercio ID</b> es requerido.", xtraTabPageRegistroTransacciones.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        txtMerchantId.Focus();
                        return false;
                    }

                    if (string.IsNullOrEmpty(txtTerminalId.Text))
                    {
                        XtraMessageBox.AllowHtmlText = true;
                        XtraMessageBox.Show("El campo <b>Terminal ID</b> es requerido.", xtraTabPageRegistroTransacciones.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        txtTerminalId.Focus();
                        return false;
                    }
                }

                #endregion

                #region Validaciones adicionales

                // Validar existemcia de BD
                var existeBaseDatos = DataHelper.ExisteBaseDatos();
                if (!existeBaseDatos)
                {
                    XtraMessageBox.AllowHtmlText = true;
                    XtraMessageBox.Show("La base de datos de gestión no existe.", xtraTabPageRegistroTransacciones.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);

                    return false;
                }

                // Validar existencia de la tabla de registro de transacciones
                var existeTabla = DataHelper.ExisteTabla();
                if (!existeTabla)
                {
                    // Crear la tabla
                    var scriptEjecutado = DataHelper.EjecutarScript();
                    if (!scriptEjecutado)
                    {
                        return false;
                    }
                }

                #endregion

                // Crear/actualizar el archivo xml
                ConfigurationHelper.Save(Global.ConfigurationSettings);

                if (handle != null)
                {
                    SplashScreenManager.CloseOverlayForm(handle);
                    handle = null;
                }

                return true;
            }
            catch (Exception e)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {e.Message}";
                LogHelper.LogToFile(mensaje);
                XtraMessageBox.Show(e.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (handle != null) SplashScreenManager.CloseOverlayForm(handle);
            }

            return false;
        }

        /// <summary>
        /// Determina si un host determinado responde a una solicitud de ping
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        private bool ProbarHost(string host)
        {
            var pingable = false;
            var pinger = new Ping();

            try
            {
                var reply = pinger.Send(host);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                pinger.Dispose();
            }

            return pingable;
        }

        /// <summary>
        /// Determina si un puerto esta abierto en el host especificado
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private bool ProbarPuerto(string host, int port, TimeSpan timeout)
        {
            try
            {
                using(var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    client.EndConnect(result);
                    return success;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verificar los parametros de conexion con el dispositivo
        /// </summary>
        /// <returns></returns>
        private bool ValidarDatos()
        {
            if (txtIp.Text.Length == 0)
            {
                XtraMessageBox.AllowHtmlText = true;     
                XtraMessageBox.Show($"La dirección <b>IP</b> no es válida.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            var isValidIp = IPAddress.TryParse(txtIp.Text.Trim(), out _);
            if (!isValidIp)
            {
                XtraMessageBox.AllowHtmlText = true;     
                XtraMessageBox.Show($"La dirección <b>IP</b> no es válida.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Realiza una prueba de conectividad para determinar si el puerto se encuentra abierto
        /// </summary>
        private void ProbarConectividad()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Validar datos
                var datosValidos = ValidarDatos();
                if (!datosValidos) return;

                var host = txtIp.Text.Trim();
                var puerto = Convert.ToInt32(spinPort.EditValue);
                var segundos = Convert.ToInt32(spinTimeout.EditValue) / 1000;

                // Verificar si el host responde a ping
                var alcanzable = ProbarHost(host);
                if (!alcanzable)
                {
                    Cursor.Current = Cursors.Default;
                    XtraMessageBox.AllowHtmlText = true;                
                    XtraMessageBox.Show($"La dirección <b>{host}</b> no se encuentra disponible.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Verificar si el puerto esta abierto
                var abierto = ProbarPuerto(host, puerto, TimeSpan.FromSeconds(segundos));
                if (!abierto)
                {
                    Cursor.Current = Cursors.Default;
                    XtraMessageBox.AllowHtmlText = true;                
                    XtraMessageBox.Show($"El puerto <b>{puerto}</b> no se encuentra disponible en la dirección <b>{host}</b>.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                XtraMessageBox.Show("La conexión con el dispositivo fue exitosa", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception e)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {e.Message}";
                LogHelper.LogToFile(mensaje);
                XtraMessageBox.Show(e.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            ProbarConectividad();
        }

        private void txtIp_EditValueChanged(object sender, EventArgs e)
        {
            Global.ConfigurationSettings.DeviceSettings.HostIp = txtIp.Text.Trim();
        }

        private void spinPort_EditValueChanged(object sender, EventArgs e)
        {
            Global.ConfigurationSettings.DeviceSettings.Port = Convert.ToInt32(spinPort.Value);
        }

        private void spinTimeout_EditValueChanged(object sender, EventArgs e)
        {
            Global.ConfigurationSettings.DeviceSettings.Timeout = Convert.ToInt32(spinTimeout.Value);
        }

        private void chkVerboseMode_CheckedChanged(object sender, EventArgs e)
        {
            Global.ConfigurationSettings.VerboseMode = chkVerboseMode.Checked;
        }

        private void chkTransactionInfoScreen_CheckedChanged(object sender, EventArgs e)
        {
            Global.ConfigurationSettings.TransactionInfoScreen = chkTransactionInfoScreen.Checked;
        }

        private void ConfigurationScreen_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                XtraMessageBox.AllowHtmlText = true;
                var result = XtraMessageBox.Show("¿Desea guardar los cambios?" + Environment.NewLine 
                                                        + "Si responde <b>No</b>, los cabios serán descartados", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // Guardar los cambios
                    var guardado = Guardar();
                    if (!guardado)
                    {
                        // Cancelar el cierre de la pantalla
                        e.Cancel = true;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    // Cancelar el cierre de la pantalla
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {ex.Message}";
                LogHelper.LogToFile(mensaje);
                XtraMessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBeep_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Validar datos
                var datosValidos = ValidarDatos();
                if (!datosValidos) return;

                // Preparar la solicitud
                var dcl = new DCL_Ethernet
                {
                    IP = Global.ConfigurationSettings.DeviceSettings.HostIp,
                    Port = Global.ConfigurationSettings.DeviceSettings.Port,
                    Timeout = Global.ConfigurationSettings.DeviceSettings.Timeout,
                    OpenChannel = true
                };

                // Enviar la solicitud
                dcl.Beep();

                // Cerrar el canal
                dcl.CloseChannel();

                // Retroalimentar
                XtraMessageBox.Show("El comando fue enviado correctamente.", btnBeep.Text, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {ex.Message}";
                LogHelper.LogToFile(mensaje);
                XtraMessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnConsultaPuntos_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Validar datos
                var datosValidos = ValidarDatos();
                if (!datosValidos) return;

                // Preparar la solicitud
                var dcl = new DCL_Ethernet
                {
                    IP = Global.ConfigurationSettings.DeviceSettings.HostIp,
                    Port = Global.ConfigurationSettings.DeviceSettings.Port,
                    Timeout = Global.ConfigurationSettings.DeviceSettings.Timeout,
                    OpenChannel = true
                };

                #region Parametros de la transaccion

                dcl.setAcquirerId(0x00);
                dcl.setMerchantId(0x00);
                dcl.setPaymentPlan(2); // Plan de puntos
                dcl.setPrintBeforeSendData(true);

                #endregion

                #region Preparar el objeto para almacenar la respuesta

                var dclResponse = new List<byte[]>
                {
                    DCL_TAG._TxnRspCode,
                    DCL_TAG._TxnAuthNum,
                    DCL_TAG._TxnRRN,
                    DCL_TAG._TxnInvoice,
                    DCL_TAG._TxnSTAN,
                    DCL_TAG._TxnRspText,
                    DCL_TAG._TxnBaseAmt
                };

                #endregion

                #region Enviar la peticion

                var dclResult = dcl.Balance(dclResponse);

                if (dclResult != null)
                {
                    if (dcl.DCL_ResponseCode == DclResponseCodes.Ok)
                    {
                        // Cerrar la comunicacion
                        dcl.CloseChannel();

                        Debug.Print($"TxnRspCode:{dclResult.GetValue_ASCII2String(0)}");
                        Debug.Print($"TxnAuthNum:{dclResult.GetValue_ASCII2String(1)}");
                        Debug.Print($"TxnRRN:{dclResult.GetValue_ASCII2String(2)}");
                        Debug.Print($"TxnInvoice:{dclResult.GetValue_BCD2String(3)}");
                        Debug.Print($"TxnSTAN:{dclResult.GetValue_BCD2String(4)}");
                        Debug.Print($"TxnRspText:{dclResult.GetValue_ASCII2String(5)}\n");
                        Debug.Print($"TxnBaseAmt:{dclResult.GetValue_BCD2String(6)}");

                        // Procesar la respusta
                        var saldo =  Convert.ToDecimal(dclResult.GetValue_BCD2String(6))/100;
                        XtraMessageBox.Show($"El saldo es {saldo:N2}", "Consulta Puntos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // Cerrar la comunicacion
                        dcl.CloseChannel();

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
                XtraMessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnBatchInquiry_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Validar datos
                var datosValidos = ValidarDatos();
                if (!datosValidos) return;

                // Preparar la solicitud
                var dcl = new DCL_Ethernet
                {
                    IP = Global.ConfigurationSettings.DeviceSettings.HostIp,
                    Port = Global.ConfigurationSettings.DeviceSettings.Port,
                    Timeout = Global.ConfigurationSettings.DeviceSettings.Timeout,
                    OpenChannel = true
                };

                #region Parametros de la transaccion

                dcl.setAcquirerId(0x00);
                dcl.setMerchantId(0x00);
                dcl.setPaymentPlan(2); // Plan de puntos
                dcl.setPrintBeforeSendData(true);

                #endregion

                #region Enviar la peticion

                // Enviar el comando
                var dclResult = dcl.DownloadReport(Global.DetailedReport);

                // Cerrar la comunicacion
                dcl.CloseChannel();

                if (dclResult != null)
                {
                    // Retroalimentar
                    XtraMessageBox.Show("El comando fue enviado correctamente.", btnBeep.Text, MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    throw new ApplicationException("Sin transacciones.");
                }

                #endregion
            }
            catch (Exception ex)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {ex.Message}";
                LogHelper.LogToFile(mensaje);
                XtraMessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnBatchSettlement_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Validar datos
                var datosValidos = ValidarDatos();
                if (!datosValidos) return;

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
                        XtraMessageBox.Show("El cierre de lote fue realizado con exitosamente.", "Cierre de Lote", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                XtraMessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnSettlementReport_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Validar datos
                var datosValidos = ValidarDatos();
                if (!datosValidos) return;

                // Preparar la solicitud
                var dcl = new DCL_Ethernet
                {
                    IP = Global.ConfigurationSettings.DeviceSettings.HostIp,
                    Port = Global.ConfigurationSettings.DeviceSettings.Port,
                    Timeout = Global.ConfigurationSettings.DeviceSettings.Timeout,
                    OpenChannel = true
                };

                #region Parametros de la transaccion

                dcl.ProcessBIN = false;
                dcl.OpenChannel = true;

                #endregion

                #region Enviar la peticion

                // Enviar el comando
                var dclResult = dcl.DownloadReport(Global.SettlementReport);

                // Cerrar la comunicacion
                dcl.CloseChannel();

                if (dclResult != null)
                {
                    if (Global.SettlementReport[1] == 0x07)
                    {

                        var data = Utilidad.GetData(dclResult, 0);

                        var result = new DCL_Result();
                        result.setData(data);
                        var reporte = Utilidad.Bcd2str(dclResult, 0, dclResult.Length * 2, true);
                        XtraMessageBox.Show(reporte, "Reporte de Cierre", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        var reporte = Encoding.Default.GetString(dclResult);
                        XtraMessageBox.Show(reporte, "Reporte de Cierre", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    throw new ApplicationException("Sin transacciones.");
                }

                #endregion
            }
            catch (Exception ex)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {ex.Message}";
                LogHelper.LogToFile(mensaje);
                XtraMessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void chkReportarImpuestos_CheckedChanged(object sender, EventArgs e)
        {
            Global.ConfigurationSettings.RegionalSettings.ReportarImpuestos = chkReportarImpuestos.Checked;
        }

        private void cboRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Global.ConfigurationSettings.RegionalSettings.RegionId = cboRegion.SelectedIndex;
        }

        private void xtraTabControl1_CustomHeaderButtonClick(object sender, CustomHeaderButtonEventArgs e)
        {
            if (e.Button.Index == 0)
            {
                XtraMessageBox.AllowHtmlText = true;
                var result = XtraMessageBox.Show("¿Guardar los cambios?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes) return;

                // Guardar los cambios
                var guardado = Guardar();
                if (!guardado) return;

                // Recargar la configuración
                Global.ConfigurationSettings = ConfigurationHelper.Load(Global.ConfigFilePath);

                // Refrescar la configuración en pantalla
                MostrarParametros();
            }
            else if (e.Button.Index == 1)
            {
                // Solicitar la nueva contrasena
                var contrasenaForm = new ContrasenaForm();
                var result = contrasenaForm.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    XtraMessageBox.Show($"La contraseña fue actualizada.{Environment.NewLine}Recuerde guardar los cambios al cerrar la pantalla.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void chkUsarAnularEnLugarDeDevolver_CheckedChanged(object sender, EventArgs e)
        {
            Global.ConfigurationSettings.RegionalSettings.UsarAnularEnLugarDeDevolver = chkUsarAnularEnLugarDeDevolver.Checked;
        }

        private void chkImpresionLocal_CheckedChanged(object sender, EventArgs e)
        {
            // Actualizar el valor en memoria
            Global.ConfigurationSettings.LocalPrinting = chkImpresionLocal.Checked;

            // Activar o desactivar la seleccion de impresora
            cboPrinter.Enabled = chkImpresionLocal.Checked;

            // Para usar la impresión local, se requiere tener activado el registro de transacciones
            if (chkImpresionLocal.Checked & !chkRegistrarTransacciones.Checked)
            {
                chkRegistrarTransacciones.Checked = true;

                XtraMessageBox.AllowHtmlText = true;
                XtraMessageBox.Show($"Para poder utilizar la opción de <b>Impresión Local</b>, el registro de transacciones debe estar activo.{Environment.NewLine}El registro de transacciones fue activado de manera automática.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void chkRegistrarTransacciones_CheckedChanged(object sender, EventArgs e)
        {
            Global.ConfigurationSettings.TransactionLogging = chkRegistrarTransacciones.Checked;

            // Habilitar la pestaña de manera condicional
            xtraTabPageRegistroTransacciones.PageVisible = chkRegistrarTransacciones.Checked;
        }

        private void cboPrinter_SelectedIndexChanged(object sender, EventArgs e)
        {
            Global.ConfigurationSettings.PrinterName = cboPrinter.Text;
        }

        private void txtMerchantId_EditValueChanged(object sender, EventArgs e)
        {
            Global.ConfigurationSettings.TransactionLoggingSettings.MerchantId = txtMerchantId.Text;
        }

        private void txtTerminalId_EditValueChanged(object sender, EventArgs e)
        {
            Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId = txtTerminalId.Text;
        }

        private void txtHeader1_EditValueChanged(object sender, EventArgs e)
        {
            Global.ConfigurationSettings.TransactionLoggingSettings.Header1 = txtHeader1.Text;
        }

        private void txtHeader2_EditValueChanged(object sender, EventArgs e)
        {
            Global.ConfigurationSettings.TransactionLoggingSettings.Header2 = txtHeader2.Text;
        }

        private void txtHeader3_EditValueChanged(object sender, EventArgs e)
        {
            Global.ConfigurationSettings.TransactionLoggingSettings.Header3 = txtHeader3.Text;
        }
    }
}
