using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using DevExpress.Skins.XtraForm;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using Kinpos.comm;
using Kinpos.Dcl;
using Kinpos.Dcl.Core;
using Kinpos.Dcl.Util;
using tef_banpro_app.Enums;
using tef_banpro_app.Model;
using tef_banpro_app.Tools;

namespace tef_banpro_app.Forms
{
    public partial class TransactionScreen : XtraForm
    {
        private OperacionesIcg _transaction;
        private double _importe;
        private string _moneda;
        private FormaPago _formaPago;
        private DclTransactionTypes _dclTransactionType;
        private string _invoice;
        private readonly bool _hidden;

        public TransactionScreen()
        {
            InitializeComponent();
            Text = $@"Integración TEF ICG / {Properties.Settings.Default.P1} (DCL)";
            
            #region Regionalizacion

            switch (Global.ConfigurationSettings.RegionalSettings.RegionId)
            {
                case 0:
                    pictureBoxLogo.Image = Properties.Resources.PROMERICA_NI;
                    break;
            }

            #endregion

            if (!Global.ConfigurationSettings.TransactionInfoScreen)
            {
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                _hidden = true;
            }
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

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        /// <summary>
        /// Muestra informacion de la transaccio en pantalla
        /// </summary>
        private void DisplayTransactionInformation()
        {
            try
            {
                if (_transaction == null) return;

                // Calcular el importe a procesar
                _importe = Convert.ToDouble(_transaction.Peticion.Importe) / 100;

                #region Obtener parametros para la transaccion

                _formaPago = Global.ConfigurationSettings.FormasPago.FirstOrDefault(t =>
                    Convert.ToInt32(t.FormaPagoId) == Convert.ToInt32(_transaction.Peticion.Fp));
                if (_formaPago == null)
                {
                    throw new ApplicationException(
                        "La forma de pago no está configurada para ser utilizada con esta integración.");
                }

                // Obtener el codigo ISO de la moneda
                _moneda = _formaPago.CurrencyNumber;

                #region Determinar el tipo de transaccion DCL que debe ser utilizada

                switch (_transaction.Peticion.TipoTx)
                {
                    case "V":
                        _dclTransactionType = (DclTransactionTypes)_formaPago.DclTransactionType;
                        break;
                    case "A":
                        _dclTransactionType = DclTransactionTypes.Void;
                        break;
                    case "D":
                        _dclTransactionType = Global.ConfigurationSettings.RegionalSettings.UsarAnularEnLugarDeDevolver 
                            ? DclTransactionTypes.Void
                            : DclTransactionTypes.Refund;
                        break;
                }

                #endregion

                // Obtener el simbolo de la moneda
                var simbolo = ((Currencies) Convert.ToInt32(_moneda)).GetDescription();

                #endregion

                var transaccion = string.Empty;
                switch (_transaction.Peticion.TipoTx)
                {
                    case "V":

                        #region Determinar el tipo transaccion de venta

                        switch (_dclTransactionType)
                        {
                            case DclTransactionTypes.Sale:
                                transaccion = $"{DclTransactionTypes.Sale.GetDescription()} => <b>{simbolo} {_importe:N2}</b>".ToUpper();
                                break;
                            case DclTransactionTypes.Points:
                                transaccion = $"{DclTransactionTypes.Points.GetDescription()} => <b>{simbolo} {_importe:N2}</b>".ToUpper();
                                break;
                            case DclTransactionTypes.Wallet:
                                transaccion = $"{DclTransactionTypes.Wallet.GetDescription()} => <b>{simbolo} {_importe:N2}</b>".ToUpper();
                                break;
                            case DclTransactionTypes.Extrafinancing:
                                transaccion = $"{DclTransactionTypes.Extrafinancing.GetDescription()} => <b>{simbolo} {_importe:N2}</b>".ToUpper();
                                break;
                            case DclTransactionTypes.Intrafinancing:
                                transaccion = $"{DclTransactionTypes.Intrafinancing.GetDescription()} => <b>{simbolo} {_importe:N2}</b>".ToUpper();
                                break;
                            case DclTransactionTypes.Payphone:
                                transaccion = $"{DclTransactionTypes.Payphone.GetDescription()} => <b>{simbolo} {_importe:N2}</b>".ToUpper();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        #endregion
                        
                        break;
                    case "A":
                        transaccion = $"{DclTransactionTypes.Void.GetDescription()} => <b>{simbolo} {_importe:N2}</b>".ToUpper();
                        
                        break;
                    case "D":
                        transaccion = Global.ConfigurationSettings.RegionalSettings.UsarAnularEnLugarDeDevolver 
                            ? $"{DclTransactionTypes.Void.GetDescription()} => <b>{simbolo} {_importe:N2}</b>".ToUpper()
                            : $"{DclTransactionTypes.Refund.GetDescription()} => <b>{simbolo} {_importe:N2}</b>".ToUpper();
                        
                        break;
                }

                // Recuperar el numero de factura
                _invoice = _transaction.Respuesta.IdOperacion;

                // Mostrar la informacion
                lblTransaction.AllowHtmlString = true;
                lblTransaction.Text = transaccion;

                if (!string.IsNullOrEmpty(_invoice)) lblTransaction.Text += $@" ({_invoice})";
            }
            catch (Exception e)
            {
                var message = $@"Error en {MethodBase.GetCurrentMethod().Name}. {e.Message}";
                throw new ApplicationException(message);
            }
        }

        /// <summary>
        /// Realiza una transaccion de venta a traves de la libreria DCL
        /// </summary>
        private void Sale()
        {
            try
            {
                // Convertir el monto
                var amount = Convert.ToDouble(_transaction.Peticion.Importe) / 100;
                var taxAmount = 0.00;

                #region Preparar la solicitud (Parametros generales)

                var dcl = new DCL_Ethernet
                {
                    IP = Global.ConfigurationSettings.DeviceSettings.HostIp,
                    Port = Global.ConfigurationSettings.DeviceSettings.Port,
                    Timeout = Global.ConfigurationSettings.DeviceSettings.Timeout,
                    OpenChannel = true
                };

                #endregion

                #region Datos adicionales que dependen de la configuracion regional

                if (Global.ConfigurationSettings.RegionalSettings.ReportarImpuestos)
                {
                    // Obtener el total neto del documento desde la base de datos
                    var transaccion = TransactionHelper.GetTransaccionVenta(_transaction.Peticion.Serie, Convert.ToInt32(_transaction.Peticion.NumTicket), _transaction.Peticion.IcgTipoApp);
                    if (transaccion == null)
                    {
                        var mensaje = "Error obteniendo los datos adicionales de la transacción de venta.";
                        LogHelper.LogToFile(mensaje);

                        var xmlSalida = new OperacionesIcg
                        {
                            Peticion = new Peticion(),
                            Respuesta = new Respuesta { InfoError = mensaje }
                        };

                        // Serializar los datos
                        var xmlData = SerializationHelper.Serialize(xmlSalida);

                        // Escribir en el archivo
                        File.WriteAllText(Global.OutputFilePath, xmlData, Encoding.Unicode);
                    }

                    // Calcular el importe de impuestos a partir del monto pagado y el total neto
                    taxAmount = Math.Round(transaccion.TotalImpuestos * (amount / transaccion.TotalNeto), 2);

                    // Pasar el valor de impuestos
                    dcl.setTaxAmount($"{taxAmount * 100}");
                }

                #endregion

                #region Bitacora detallada

                if (Global.ConfigurationSettings.VerboseMode)
                {
                    LogHelper.LogToFile($"Moneda: {_moneda}");
                    LogHelper.LogToFile($"Importe: {amount:N2}");
                    if (Global.ConfigurationSettings.RegionalSettings.ReportarImpuestos) LogHelper.LogToFile($"Impuestos: {taxAmount:N2}");
                }

                #endregion

                #region Pasar los parametros especificos para el tipo de transaccion de venta

                if (_dclTransactionType == DclTransactionTypes.Sale)
                {
                    dcl.setAcquirerId(_formaPago.AcquirerId != -1 ? _formaPago.AcquirerId : 0);
                    dcl.setMerchantId(_formaPago.MerchantId != -1 ? _formaPago.MerchantId : 0);
                    dcl.setCurrency(_moneda);

                    if (Global.ConfigurationSettings.RegionalSettings.ReportarImpuestos)
                    {
                        // Calcular el importe base (bruto)
                        var importeBase = amount - taxAmount;
                        dcl.setAmount($"{importeBase * 100}");
                    }
                    else
                    {
                        dcl.setAmount(_transaction.Peticion.Importe);
                    }
                }
                else if (_dclTransactionType == DclTransactionTypes.Points)
                {
                    dcl.setAcquirerId(_formaPago.AcquirerId != -1 ? _formaPago.AcquirerId : 0);
                    dcl.setMerchantId(_formaPago.MerchantId != -1 ? _formaPago.MerchantId : 0);
                    dcl.setCurrency(_moneda);
                    
                    if (Global.ConfigurationSettings.RegionalSettings.ReportarImpuestos)
                    {
                        // Calcular el importe base (bruto)
                        var importeBase = amount - taxAmount;
                        dcl.setAmount($"{importeBase * 100}");
                    }
                    else
                    {
                        dcl.setAmount(_transaction.Peticion.Importe);
                    }

                    dcl.setPaymentPlan(2); // Puntos
                }
                else if (_dclTransactionType == DclTransactionTypes.Payphone)
                {
                    #region Pedir el numero de telefono

                    var phoneScreen = new PhoneScreen();
                    var dialogResult = phoneScreen.ShowDialog(this);
                    if (dialogResult != DialogResult.OK)
                    {
                        #region Cancelar la transaccion

                        // Generar un XML de salida indicando el motivo del error
                        if (_transaction == null)
                        {
                            throw new ApplicationException("La transacción fue cancelada por el usuario.");
                        }
                        
                        // Salir retornando un valor que indica que ocurrió un error
                        Application.Exit();

                        #endregion
                    }

                    #endregion

                    dcl. setTelephoneNumber(phoneScreen.Telefono);
                    dcl.setAcquirerId(_formaPago.AcquirerId != -1 ? _formaPago.AcquirerId : 0);
                    dcl.setMerchantId(_formaPago.MerchantId != -1 ? _formaPago.MerchantId : 0);
                    dcl.setPayphoneRetries(4);
                    dcl.setCurrency(_moneda);
                    
                    if (Global.ConfigurationSettings.RegionalSettings.ReportarImpuestos)
                    {
                        // Calcular el importe base (bruto)
                        var importeBase = amount - taxAmount;
                        dcl.setAmount($"{importeBase * 100}");
                    }
                    else
                    {
                        dcl.setAmount(_transaction.Peticion.Importe);
                    }
                }
                else if (_dclTransactionType == DclTransactionTypes.Extrafinancing)
                {
                    dcl.setAcquirerId(_formaPago.AcquirerId != -1 ? _formaPago.AcquirerId : 0);
                    dcl.setMerchantId(_formaPago.MerchantId != -1 ? _formaPago.MerchantId : 0);
                    dcl.setCurrency(_moneda);
                    
                    if (Global.ConfigurationSettings.RegionalSettings.ReportarImpuestos)
                    {
                        // Calcular el importe base (bruto)
                        var importeBase = amount - taxAmount;
                        dcl.setAmount($"{importeBase * 100}");
                    }
                    else
                    {
                        dcl.setAmount(_transaction.Peticion.Importe);
                    }

                    dcl.setQuotas(_formaPago.Quotas);
                    dcl.setPaymentPlan(1);  //Extra Financiamiento
                }
                else if (_dclTransactionType == DclTransactionTypes.Intrafinancing)
                {
                    dcl.setAcquirerId(_formaPago.AcquirerId != -1 ? _formaPago.AcquirerId : 0);
                    dcl.setMerchantId(_formaPago.MerchantId != -1 ? _formaPago.MerchantId : 0);
                    dcl.setCurrency(_moneda);
                    
                    if (Global.ConfigurationSettings.RegionalSettings.ReportarImpuestos)
                    {
                        // Calcular el importe base (bruto)
                        var importeBase = amount - taxAmount;
                        dcl.setAmount($"{importeBase * 100}");
                    }
                    else
                    {
                        dcl.setAmount(_transaction.Peticion.Importe);
                    }

                    dcl.setQuotas(_formaPago.Quotas);
                    dcl.setPaymentPlan(3);  //Intra Financiamiento
                }

                #endregion

                #region Preparar el objeto para almacenar la respuesta

                var dclResponse = new List<byte[]>();

                if (_dclTransactionType == DclTransactionTypes.Sale || _dclTransactionType == DclTransactionTypes.Points)
                {
                    dclResponse = new List<byte[]>
                    {
                        DCL_TAG._TxnRspCode,        // 0 Código de Respuesta                                        ASCII
                        DCL_TAG._TxnAuthNum,        // 1 Número de Autorización                                     ASCII
                        DCL_TAG._TxnDate,           // 2 Fecha de la transacción                                    BCD (HHMMSS)
                        DCL_TAG._TxnTime,           // 3 Hora de la transacción                                     BCD (AAAAMMDD)
                        DCL_TAG._TxnTID,            // 4 Terminal Id                                                ASCII    
                        DCL_TAG._TxnMerName,        // 5 Id del comercio                                            ASCII
                        DCL_TAG._TxnMaskPAN,        // 6 Tarjeta Enmascarada                                        ASCII
                        DCL_TAG._TxnInvoice,        // 7 Factura                                                    BCD 
                        DCL_TAG._TxnIssName,        // 8 Emisor                                                     ASCII                
                        DCL_TAG._TxnEMVDataEND,     // 9 Datos EMV para Chip y Contactless                          BCD
                        DCL_TAG._TxnRRN,            // 10 Referencia                                                ASCII     
                        DCL_TAG._TxnSTAN,           // 11 System Trace Number                                       BCD           
                        DCL_TAG._TxnHolderName,     // 12 Nombre del Tarjeta Habiente                               ASCII    
                        DCL_TAG._TxnPosEntryMode,   // 13 Entry Mode : 'I' / CHIP  , 'C' - Contacless, 'S' - Banda  BCD
                        DCL_TAG._AppVersion,        // 14 Version                                                   ASCII
                        DCL_TAG._AppRelease,        // 15 Release                                                   ASCII
                        DCL_TAG._TxnRPS_RecordID    // 16 RPS_Id                                                    BCD                          

                    };
                }
                else if (_dclTransactionType == DclTransactionTypes.Extrafinancing || _dclTransactionType == DclTransactionTypes.Intrafinancing)
                {
                    dclResponse = new List<byte[]>
                    {
                        DCL_TAG._TxnRspCode,
                        DCL_TAG._TxnAuthNum,
                        DCL_TAG._TxnRRN,
                        DCL_TAG._TxnInvoice,
                        DCL_TAG._TxnSTAN,
                        DCL_TAG._TxnRspText,
                        DCL_TAG._TxnTID,
                        DCL_TAG._TxnIssName,
                        DCL_TAG._TxnMerName,
                        DCL_TAG._AppName,
                        DCL_TAG._idBankCode
                    };
                }
                else if (_dclTransactionType == DclTransactionTypes.Payphone)
                {
                    dclResponse = new List<byte[]>
                    {
                        DCL_TAG._TxnRspCode,
                        DCL_TAG._TxnAuthNum, 
                        DCL_TAG._TxnDate,
                        DCL_TAG._TxnTime,  
                        DCL_TAG._TxnTID,     
                        DCL_TAG._TxnMaskPAN, 
                        DCL_TAG._TxnBaseAmt,  
                        DCL_TAG._TxnInvoice,  
                        DCL_TAG._AppVersion,  
                        DCL_TAG._TxnIssName,     
                        DCL_TAG._TxnEMVDataEND,  
                        DCL_TAG._TxnMerName,
                        DCL_TAG._TxnRRN,
                        DCL_TAG._TxnHolderName,  
                        DCL_TAG._BatchNum,
                        DCL_TAG._AppName,
                        DCL_TAG._AppRelease, 
                        DCL_TAG._TxnTaxAmt 
                    };
                }

                #endregion

                #region Enviar la peticion

                DCL_Result dclResult;

                if (_dclTransactionType == DclTransactionTypes.Sale || _dclTransactionType == DclTransactionTypes.Points
                                                                    || _dclTransactionType == DclTransactionTypes.Intrafinancing 
                                                                    || _dclTransactionType == DclTransactionTypes.Extrafinancing)
                {
                    dclResult = dcl.Sale(dclResponse);
                }
                else
                {
                    dclResult = dcl.Payphone(dclResponse);
                }

                // Cerrar la comunicacion
                dcl.CloseChannel();

                if (dclResult != null)
                {
                    Cursor.Current = Cursors.Default;

                    // Obtener el codigo de respuesta
                    var responseCode = dclResult.GetValue_ASCII2String(0);
                    if (responseCode == "00")
                    {
                        // Procesar la respuesta
                        SuccessfulTransaction(_dclTransactionType, dclResult, taxAmount);
                    }
                    else if (dcl.DCL_ResponseCode == DclResponseCodes.Duplicate)
                    {
                        // Tansaccion duplicada
                        var descripcion = Utilidad.GetDCLResponseText(responseCode);
                        UnsuccessfulTransaction(responseCode, descripcion);
                    }
                    else
                    {
                        // Otro error
                        var descripcion = Utilidad.GetDCLResponseText(responseCode);
                        UnsuccessfulTransaction(responseCode, descripcion);
                    }
                }
                else
                {
                    Cursor.Current = Cursors.Default;
                    UnsuccessfulTransaction("-1", "No se obtuvo respuesta del dispositivo. Valide que el dispositivo este conectado a la red.");
                }

                #endregion
            }
            catch (Exception e)
            {
                LogHelper.LogToFile(e.Message);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Realiza una transaccion de devolucion a traves de la libreria DCL
        /// </summary>
        private void Refund()
        {
            try
            {
                var taxAmount = 0.00;

                #region Preparar la solicitud

                var dcl = new DCL_Ethernet
                {
                    IP = Global.ConfigurationSettings.DeviceSettings.HostIp,
                    Port = Global.ConfigurationSettings.DeviceSettings.Port,
                    Timeout = Global.ConfigurationSettings.DeviceSettings.Timeout,
                    OpenChannel = true
                };

                #endregion

                #region Parametros de la transaccion

                dcl.setCurrency(_moneda);
                dcl.setAmount(_transaction.Peticion.Importe);

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
                    DCL_TAG._TxnTID,
                    DCL_TAG._TxnIssName,
                    DCL_TAG._TxnMerName,
                    DCL_TAG._TxnBaseAmt,
                    DCL_TAG._TxnTaxAmt,
                    DCL_TAG._TxnTipAmt
                };

                #endregion

                #region Enviar la peticion

                var dclResult = dcl.Refund(dclResponse);

                // Cerrar la comunicacion
                dcl.CloseChannel();

                if (dclResult != null)
                {
                    // Obtener el codigo de respuesta
                    var responseCode = dclResult.GetValue_ASCII2String(0);

                    if (responseCode == "00")
                    {
                        // Procesar la respusta
                        SuccessfulTransaction(_dclTransactionType, dclResult, taxAmount);
                    }
                    else
                    {
                        // Otro error
                        var descripcion = Utilidad.GetDCLResponseText(responseCode);
                        UnsuccessfulTransaction(responseCode, descripcion);
                    }
                }
                else
                {
                    Cursor.Current = Cursors.Default;
                    UnsuccessfulTransaction("-1", "No se obtuvo respuesta del dispositivo. Valide que el dispositivo este conectado a la red.");
                }

                #endregion
            }
            catch (Exception e)
            {
                var message = $@"Error en {MethodBase.GetCurrentMethod().Name}. {e.Message}";
                throw new ApplicationException(message);
            }
        }

        /// <summary>
        /// Realiza una transaccion de anulacion a traves de la libreria DCL
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns></returns>
        private void Void(string invoice)
        {
            try
            {
                // Convertir el monto
                var amount = Convert.ToDouble(_transaction.Peticion.Importe) / 100;
                var taxAmount = 0.00;

                #region Validar que el importe no sea parcial

                var transaccion = TransactionHelper.GetTransaccionVenta(_transaction.Peticion.SerieAbono, Convert.ToInt32(_transaction.Peticion.NumeroAbono), Convert.ToInt32(_transaction.Peticion.NumLinPago), 
                    invoice, _transaction.Peticion.IcgTipoApp);
                if (transaccion == null)
                {
                    UnsuccessfulTransaction("-1", $@"No es posible procesar la transacción de anulación.{Environment.NewLine}Ocurrió un error al intentar recuperar los datos de la transacción de venta original.");
                }
                
                if (!transaccion.TotalNeto.Equals(amount))
                {
                    UnsuccessfulTransaction("-1", $@"No es posible procesar la transacción de anulación.{Environment.NewLine}El importe de la devolución debe ser al importe cobrado en la transacción de venta original.");
                }

                if (Global.ConfigurationSettings.RegionalSettings.ReportarImpuestos)
                {
                    // Calcular el importe de impuestos a partir del monto pagado y el total neto
                    taxAmount = transaccion.TotalImpuestos * (amount / transaccion.TotalNeto);
                }

                #endregion

                #region Preparar la solicitud

                var dcl = new DCL_Ethernet
                {
                    IP = Global.ConfigurationSettings.DeviceSettings.HostIp,
                    Port = Global.ConfigurationSettings.DeviceSettings.Port,
                    Timeout = Global.ConfigurationSettings.DeviceSettings.Timeout,
                    OpenChannel = true
                };

                #endregion

                #region Parametros de la transaccion

                dcl.setInvoice(invoice);

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
                    DCL_TAG._TxnTID,
                    DCL_TAG._TxnIssName,
                    DCL_TAG._TxnMerName,
                    DCL_TAG._TxnBaseAmt,
                    DCL_TAG._TxnMaskPAN
                };

                #endregion

                #region Enviar la peticion

                var dclResult = dcl.Void(dclResponse);

                // Cerrar la comunicacion
                dcl.CloseChannel();

                if (dclResult != null)
                {
                    // Obtener el codigo de respuesta
                    var responseCode = dclResult.GetValue_ASCII2String(0);

                    if (responseCode == "00")
                    {
                        // Procesar la respusta
                        SuccessfulTransaction(_dclTransactionType, dclResult, taxAmount, invoice);
                    }
                    else
                    {
                        // Otro error
                        var descripcion = Utilidad.GetDCLResponseText(responseCode);
                        UnsuccessfulTransaction(responseCode, descripcion);
                    }
                }
                else
                {
                    Cursor.Current = Cursors.Default;
                    UnsuccessfulTransaction("-1", "No se obtuvo respuesta del dispositivo. Valide que el dispositivo este conectado a la red.");
                }

                #endregion
            }
            catch (Exception e)
            {
                var message = $@"Error en {MethodBase.GetCurrentMethod().Name}. {e.Message}";
                throw new ApplicationException(message);
            }
        }

        /// <summary>
        /// Realiza el proceso de autorizacion de la transaccion con el dispositivo
        /// </summary>
        private void ProcessTransaction()
        {
            try
            {
                switch (_transaction.Peticion.TipoTx)
                {
                    case "V":
                        Sale();
                        break;
                    case "A":
                        Void(_invoice);
                        break;
                    case "D":
                        if (Global.ConfigurationSettings.RegionalSettings.UsarAnularEnLugarDeDevolver)
                        {
                            Void(_invoice);
                        }
                        else
                        {
                            Refund();   
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                throw new ApplicationException(message);
            }
        }

        /// <summary>
        /// Transacion exitosa
        /// </summary>
        /// <param name="transactionType"></param>
        /// <param name="result"></param>
        /// <param name="taxAmount"></param>
        /// <param name="invoice">Invoice de la transacción original en caso de Anulación</param>
        /// <autogeneratedoc />
        private void SuccessfulTransaction(DclTransactionTypes transactionType, DCL_Result result, double taxAmount, string invoice = null)
        {
            try
            {
                // Calcular el importe a procesar
                _importe = Convert.ToDouble(_transaction.Peticion.Importe) / 100;

                if (transactionType == DclTransactionTypes.Sale || transactionType == DclTransactionTypes.Points)
                {
                    var responseCode = result.GetValue_ASCII2String(0);

                    #region Log

                    LogHelper.LogToFile($"Transaction Type: {transactionType.GetDescription()}");
                    LogHelper.LogToFile($"Transaction Response Code: {responseCode}");
                    LogHelper.LogToFile($"Fecha: {result.GetValue_BCD2String(2)}");
                    LogHelper.LogToFile($"Hora: {result.GetValue_BCD2String(3)}");
                    LogHelper.LogToFile($"Documento: {_transaction.Peticion.Serie}-{_transaction.Peticion.NumTicket}");
                    LogHelper.LogToFile($"Amount: {_importe:N2}");
                    if (Global.ConfigurationSettings.RegionalSettings.ReportarImpuestos) LogHelper.LogToFile($"TaxAmount: {taxAmount:N2}");
                    LogHelper.LogToFile($"Autorizado: {result.GetValue_ASCII2String(1)}");
                    LogHelper.LogToFile($"RRN: {result.GetValue_ASCII2String(10)}");
                    LogHelper.LogToFile($"STAN: {result.GetValue_BCD2String(11)}");
                    LogHelper.LogToFile($"Invoice: {result.GetValue_BCD2String(7)}");
                    LogHelper.LogToFile($"Tarjetahabiente: {result.GetValue_ASCII2String(12)}");
                    LogHelper.LogToFile($"Tarjeta: {result.GetValue_ASCII2String(6)}");
                    LogHelper.LogToFile("", false);

                    #endregion

                    // Actualizar el archivo de salida
                    _transaction.Respuesta.InfoError = string.Empty;
                    _transaction.Respuesta.CodAutorizacion = result.GetValue_BCD2String(7); // Campo NUMTXNTEF (15 caracteres)
                    _transaction.Respuesta.IdRts = $@"{result.GetValue_ASCII2String(6)}"; // NUMRTSTEF en Tesorería (40 caracteres) Tarjeta

                    #region Registrar la transaccion (dependiendo de la configuración)

                    if (Global.ConfigurationSettings.TransactionLogging)
                    {
                        var dclTransaction = new DclTransaction
                        {
                            MerchantId = Global.ConfigurationSettings.TransactionLoggingSettings.MerchantId,
                            TerminalId = Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId,
                            TransactionType = transactionType.GetDescription(),
                            ExternalReference = $"{_transaction.Peticion.Serie}-{_transaction.Peticion.NumTicket}",
                            Invoice = $"{result.GetValue_BCD2String(7)}",
                            CurrencySymbol = ((Currencies) Convert.ToInt32(_moneda)).GetDescription(),
                            TransactionDate = $"{result.GetValue_BCD2String(2)}",
                            TransactionTime = $"{result.GetValue_BCD2String(3)}",
                            ReferenceNumber = $"{result.GetValue_ASCII2String(10)}",
                            AuthorizationNumber = $"{result.GetValue_ASCII2String(1)}",
                            SystemTraceNumber = $"{result.GetValue_BCD2String(11)}",
                            Amount = _transaction.Peticion.Importe,
                            TaxAmount = $"{taxAmount * 100}",
                            MaskedPan = $"{result.GetValue_ASCII2String(6)}",
                            CardHolderName = $"{result.GetValue_ASCII2String(12)}"
                        };

                        // Insertar el registro
                        DataHelper.RegistrarTransaccion(dclTransaction);

                        #region Imprimir la transaccion (dependiendo de la configuración)

                        if (Global.ConfigurationSettings.LocalPrinting)
                        {
                            // Imprimir la copia del comercio
                            PrinterHelper.ImprimirTransaccion(dclTransaction);

                            // Imprimir la copia del cliente
                            PrinterHelper.ImprimirTransaccion(dclTransaction, false);
                        }

                        #endregion
                    }

                    #endregion
                }
                else if (transactionType == DclTransactionTypes.Payphone)
                {
                    var responseCode = result.GetValue_ASCII2String(0);

                    #region Log

                    LogHelper.LogToFile($"Transaction Type: {transactionType.GetDescription()}");
                    LogHelper.LogToFile($"Transaction Response Code: {responseCode}");
                    LogHelper.LogToFile($"Fecha: {result.GetValue_BCD2String(2)}");
                    LogHelper.LogToFile($"Hora: {result.GetValue_BCD2String(3)}");
                    LogHelper.LogToFile($"Documento: {_transaction.Peticion.Serie}-{_transaction.Peticion.NumTicket}");
                    LogHelper.LogToFile($"Amount: {_importe:N2}");
                    LogHelper.LogToFile($"Autorizado: {result.GetValue_ASCII2String(1)}");
                    LogHelper.LogToFile($"RRN: {result.GetValue_ASCII2String(12)}");
                    LogHelper.LogToFile($"STAN: {result.GetValue_BCD2String(11)}");
                    LogHelper.LogToFile($"Invoice: {result.GetValue_BCD2String(7)}");
                    LogHelper.LogToFile($"Tarjetahabiente: {result.GetValue_ASCII2String(13)}");
                    LogHelper.LogToFile($"Tarjeta: {result.GetValue_ASCII2String(5)}");
                    LogHelper.LogToFile("", false);

                    #endregion

                    // Actualizar el archivo de salida
                    _transaction.Respuesta.InfoError = string.Empty;
                    _transaction.Respuesta.CodAutorizacion = result.GetValue_BCD2String(7); // Campo NUMTXNTEF (15 caracteres)
                    _transaction.Respuesta.IdRts = $@"{result.GetValue_ASCII2String(13)}"; // NUMRTSTEF en Tesorería (40 caracteres) Tarjeta

                    #region Registrar la transaccion (dependiendo de la configuración)

                    if (Global.ConfigurationSettings.TransactionLogging)
                    {
                        var dclTransaction = new DclTransaction
                        {
                            MerchantId = Global.ConfigurationSettings.TransactionLoggingSettings.MerchantId,
                            TerminalId = Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId,
                            TransactionType = transactionType.GetDescription(),
                            ExternalReference = $"{_transaction.Peticion.Serie}-{_transaction.Peticion.NumTicket}",
                            Invoice = $"{result.GetValue_BCD2String(7)}",
                            CurrencySymbol = ((Currencies) Convert.ToInt32(_moneda)).GetDescription(),
                            TransactionDate = $"{result.GetValue_BCD2String(2)}",
                            TransactionTime = $"{result.GetValue_BCD2String(3)}",
                            ReferenceNumber = $"{result.GetValue_ASCII2String(12)}",
                            AuthorizationNumber = $"{result.GetValue_ASCII2String(1)}",
                            SystemTraceNumber = $"{result.GetValue_BCD2String(11)}",
                            Amount = _transaction.Peticion.Importe,
                            TaxAmount = $"{taxAmount * 100}",
                            MaskedPan = $"{result.GetValue_ASCII2String(5)}",
                            CardHolderName = $"{result.GetValue_ASCII2String(13)}"
                        };

                        // Insertar el registro
                        DataHelper.RegistrarTransaccion(dclTransaction);

                        #region Imprimir la transaccion (dependiendo de la configuración)

                        if (Global.ConfigurationSettings.LocalPrinting)
                        {
                            // Imprimir la copia del comercio
                            PrinterHelper.ImprimirTransaccion(dclTransaction);

                            // Imprimir la copia del cliente
                            PrinterHelper.ImprimirTransaccion(dclTransaction, false);
                        }

                        #endregion
                    }

                    #endregion
                }
                else if (transactionType == DclTransactionTypes.Extrafinancing || transactionType == DclTransactionTypes.Intrafinancing)
                {
                    var responseCode = result.GetValue_ASCII2String(0);

                    #region Log

                    LogHelper.LogToFile($"Transaction Type: {transactionType.GetDescription()}");
                    LogHelper.LogToFile($"Transaction Response Code: {responseCode}");
                    LogHelper.LogToFile($"Documento: {_transaction.Peticion.Serie}-{_transaction.Peticion.NumTicket}");
                    LogHelper.LogToFile($"Amount: {_importe:N2}");
                    LogHelper.LogToFile($"Autorizado: {result.GetValue_ASCII2String(1)}");
                    LogHelper.LogToFile($"RRN: {result.GetValue_ASCII2String(2)}");
                    LogHelper.LogToFile($"STAN: {result.GetValue_BCD2String(4)}");
                    LogHelper.LogToFile($"Invoice: {result.GetValue_BCD2String(3)}");
                    LogHelper.LogToFile("",false);

                    #endregion

                    // Actualizar el archivo de salida
                    _transaction.Respuesta.InfoError = string.Empty;
                    _transaction.Respuesta.CodAutorizacion = result.GetValue_BCD2String(3); // Campo NUMTXNTEF (15 caracteres)

                    #region Registrar la transaccion (dependiendo de la configuración)

                    if (Global.ConfigurationSettings.TransactionLogging)
                    {
                        var dclTransaction = new DclTransaction
                        {
                            MerchantId = Global.ConfigurationSettings.TransactionLoggingSettings.MerchantId,
                            TerminalId = Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId,
                            TransactionType = transactionType.GetDescription(),
                            ExternalReference = $"{_transaction.Peticion.Serie}-{_transaction.Peticion.NumTicket}",
                            Invoice = $"{result.GetValue_BCD2String(3)}",
                            CurrencySymbol = ((Currencies) Convert.ToInt32(_moneda)).GetDescription(),
                            ReferenceNumber = $"{result.GetValue_ASCII2String(2)}",
                            AuthorizationNumber = $"{result.GetValue_ASCII2String(1)}",
                            SystemTraceNumber = $"{result.GetValue_BCD2String(4)}",
                            Amount = _transaction.Peticion.Importe,
                            TaxAmount = $"{taxAmount * 100}"
                        };

                        // Insertar el registro
                        DataHelper.RegistrarTransaccion(dclTransaction);

                        #region Imprimir la transaccion (dependiendo de la configuración)

                        if (Global.ConfigurationSettings.LocalPrinting)
                        {
                            // Imprimir la copia del comercio
                            PrinterHelper.ImprimirTransaccion(dclTransaction);

                            // Imprimir la copia del cliente
                            PrinterHelper.ImprimirTransaccion(dclTransaction, false);
                        }

                        #endregion
                    }

                    #endregion
                }
                else if (transactionType == DclTransactionTypes.Void)
                {
                    var responseCode = result.GetValue_ASCII2String(0);

                    #region Log

                    LogHelper.LogToFile($"Transaction Type: {transactionType.GetDescription()}");
                    LogHelper.LogToFile($"Transaction Response Code: {responseCode}");
                    LogHelper.LogToFile($"Documento: {_transaction.Peticion.Serie}-{_transaction.Peticion.NumTicket}");
                    LogHelper.LogToFile($"Autorizado: {result.GetValue_ASCII2String(1)}");
                    LogHelper.LogToFile($"RRN: {result.GetValue_ASCII2String(2)}");
                    LogHelper.LogToFile($"Referenced Invoice: {invoice}");
                    LogHelper.LogToFile($"Invoice: {result.GetValue_BCD2String(3)}");
                    LogHelper.LogToFile($"STAN: {result.GetValue_BCD2String(4)}");
                    LogHelper.LogToFile("",false);

                    #endregion

                    // Actualizar el archivo de salida
                    _transaction.Respuesta.InfoError = string.Empty;
                    _transaction.Respuesta.CodAutorizacion = result.GetValue_BCD2String(3); // Campo NUMTXNTEF (15 caracteres)

                    #region Registrar la transaccion (dependiendo de la configuración)

                    if (Global.ConfigurationSettings.TransactionLogging)
                    {
                        // Intentar obtener la fecha y hora desde el registro de la venta original
                        var venta = DataHelper.Obtener(invoice);
                        
                        var dclTransaction = new DclTransaction
                        {
                            MerchantId = Global.ConfigurationSettings.TransactionLoggingSettings.MerchantId,
                            TerminalId = Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId,
                            TransactionType = transactionType.GetDescription(),
                            ExternalReference = $"{_transaction.Peticion.Serie}-{_transaction.Peticion.NumTicket}",
                            Invoice = $"{result.GetValue_BCD2String(3)}",
                            CurrencySymbol = ((Currencies) Convert.ToInt32(_moneda)).GetDescription(),
                            TransactionDate = venta?.TransactionDate,
                            TransactionTime = venta?.TransactionTime,
                            ReferenceNumber = $"{result.GetValue_ASCII2String(2)}",
                            AuthorizationNumber = $"{result.GetValue_ASCII2String(1)}",
                            SystemTraceNumber = $"{result.GetValue_BCD2String(4)}",
                            Amount = _transaction.Peticion.Importe,
                            TaxAmount = $"{taxAmount * 100}"
                        };

                        // Insertar el registro
                        DataHelper.RegistrarTransaccion(dclTransaction);

                        #region Imprimir la transaccion (dependiendo de la configuración)

                        if (Global.ConfigurationSettings.LocalPrinting)
                        {
                            if (venta != null)
                            {
                                // Actualizar el STAN de la transaccióm original con el de la anulación para imprimir
                                venta.TransactionType = dclTransaction.SystemTraceNumber;
                                venta.SystemTraceNumber = dclTransaction.SystemTraceNumber;

                                // Imprimir la copia del comercio
                                PrinterHelper.ImprimirTransaccion(venta);

                                // Imprimir la copia del cliente
                                PrinterHelper.ImprimirTransaccion(venta, false);
                            }
                            else
                            {
                                var message = "No es posible imprimir el voucher. Ocurrió un error al obtener la transacción abonada.";
                                XtraMessageBox.Show(message, 
                                    "Impresión de voucher", MessageBoxButtons.OK, MessageBoxIcon.Information);   
                            }
                        }

                        #endregion
                    }

                    #endregion
                }
                else if (transactionType == DclTransactionTypes.Refund)
                {
                    var responseCode = result.GetValue_ASCII2String(0);

                    #region Log

                    LogHelper.LogToFile($"Transaction Type: {transactionType.GetDescription()}");
                    LogHelper.LogToFile($"Transaction Response Code: {responseCode}");
                    LogHelper.LogToFile($"Amount: {_importe:N2}");
                    LogHelper.LogToFile($"Documento: {_transaction.Peticion.Serie}-{_transaction.Peticion.NumTicket}");
                    LogHelper.LogToFile($"Autorizado: {result.GetValue_ASCII2String(1)}");
                    LogHelper.LogToFile($"RRN: {result.GetValue_ASCII2String(2)}");
                    LogHelper.LogToFile($"STAN: {result.GetValue_BCD2String(3)}");
                    LogHelper.LogToFile($"Invoice: {result.GetValue_BCD2String(4)}");
                    LogHelper.LogToFile("",false);

                    #endregion

                    // Actualizar el archivo de salida
                    _transaction.Respuesta.InfoError = string.Empty;
                    _transaction.Respuesta.CodAutorizacion = result.GetValue_BCD2String(4); // Campo NUMTXNTEF (15 caracteres)

                    #region Registrar la transaccion (dependiendo de la configuración)

                    if (Global.ConfigurationSettings.TransactionLogging)
                    {
                        var dclTransaction = new DclTransaction
                        {
                            MerchantId = Global.ConfigurationSettings.TransactionLoggingSettings.MerchantId,
                            TerminalId = Global.ConfigurationSettings.TransactionLoggingSettings.TerminalId,
                            TransactionType = transactionType.GetDescription(),
                            ExternalReference = $"{_transaction.Peticion.Serie}-{_transaction.Peticion.NumTicket}",
                            Invoice = $"{result.GetValue_BCD2String(4)}",
                            CurrencySymbol = ((Currencies) Convert.ToInt32(_moneda)).GetDescription(),
                            ReferenceNumber = $"{result.GetValue_ASCII2String(2)}",
                            AuthorizationNumber = $"{result.GetValue_ASCII2String(1)}",
                            SystemTraceNumber = $"{result.GetValue_BCD2String(3)}",
                            Amount = _transaction.Peticion.Importe,
                            TaxAmount = $"{taxAmount * 100}"
                        };

                        // Insertar el registro
                        DataHelper.RegistrarTransaccion(dclTransaction);

                        #region Imprimir la transaccion (dependiendo de la configuración)

                        if (Global.ConfigurationSettings.LocalPrinting)
                        {
                            // Imprimir la copia del comercio
                            PrinterHelper.ImprimirTransaccion(dclTransaction);

                            // Imprimir la copia del cliente
                            PrinterHelper.ImprimirTransaccion(dclTransaction, false);
                        }

                        #endregion
                    }

                    #endregion
                }

                // Serializar los datos
                var xmlData = SerializationHelper.Serialize(_transaction);

                // Escribir en el archivo
                File.WriteAllText(Global.OutputFilePath, xmlData, Encoding.Unicode);

                // Salir de la aplicación devolviendo mensaje de exito
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                LogHelper.LogToFile($"{MethodBase.GetCurrentMethod().Name}." + e.Message);
                throw new ApplicationException(e.Message);
            }
        }

        /// <summary>
        /// Se llamara cuando se da una transacción fallida. Se retorna el mensaje de error al front.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="reason">The reason.</param>
        /// <autogeneratedoc />
        private void UnsuccessfulTransaction(string result, string reason)
        {
            try
            {
                LogHelper.LogToFile($"Transaction Type: {_dclTransactionType.GetDescription()}");
                LogHelper.LogToFile($"Documento: {_transaction.Peticion.Serie}-{_transaction.Peticion.NumTicket}");
                LogHelper.LogToFile($"Error => {result.ToUpper()}: {reason}");
                LogHelper.LogToFile("",false);

                // Actualizar el archivo de salida
                _transaction.Respuesta.InfoError = $"{result}: {reason}";
            
                // Serializar los datos
                var xmlData = SerializationHelper.Serialize(_transaction);

                // Escribir en el archivo
                File.WriteAllText(Global.OutputFilePath, xmlData, Encoding.Unicode);

                // Salir de la aplicación devolviendo mensaje de fallo
                Environment.Exit(1);
            }
            catch (Exception e)
            {
                LogHelper.LogToFile($"{MethodBase.GetCurrentMethod().Name}." + e.Message);
                throw new ApplicationException(e.Message);
            }
        }

        private void TransactionScreen_Load(object sender, EventArgs e)
        {
            try
            {
                if (_hidden) Size = new Size(0, 0);

                // Cargar el archivo de entrada
                _transaction = TransactionHelper.LoadInputFile(Global.InputFilePath);

                // Mostrar la informacion de la transaccion a realizar
                DisplayTransactionInformation();

                // Habilitar el boton para procesar
                btnAccept.Enabled = true;
            }
            catch (Exception ex)
            {
                var message = $@"Error en {MethodBase.GetCurrentMethod().Name}. No fue posible cargar el archivo de entrada. {ex.Message}";
                throw new ApplicationException(message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                var result = XtraMessageBox.Show(@"¿Desea cancelar la transacción?", Text, MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Generar un XML de salida indicando el motivo del error
                    if (_transaction == null)
                    {
                        var mensaje = "La transacción fue cancelada por el usuario";
                        LogHelper.LogToFile(mensaje);

                        var xmlSalida = new OperacionesIcg
                        {
                            Peticion = new Peticion(),
                            Respuesta = new Respuesta { InfoError = mensaje }
                        };

                        // Serializar los datos
                        var xmlData = SerializationHelper.Serialize(xmlSalida);

                        // Escribir en el archivo
                        File.WriteAllText(Global.OutputFilePath, xmlData, Encoding.Unicode);

                    }
                    else
                    {
                        var mensaje = "La transacción fue cancelada por el usuario";
                        LogHelper.LogToFile(mensaje);

                        // Actualizar el mensaje de error
                        _transaction.Respuesta.InfoError = mensaje;

                        // Serializar los datos
                        var xmlData = SerializationHelper.Serialize(_transaction);

                        // Escribir en el archivo
                        File.WriteAllText(Global.OutputFilePath, xmlData, Encoding.Unicode);
                    }

                    // Salir retornando un valor que indica que ocurrió un error
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogToFile($"{MethodBase.GetCurrentMethod().Name}." + ex.Message);
                throw new ApplicationException(ex.Message);
            }
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            ProcessTransaction();
        }

        private void TransactionScreen_Shown(object sender, EventArgs e)
        {
            if (_hidden) btnAccept.PerformClick();
        }
    }
}