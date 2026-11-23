using System.Collections.Generic;
using System.Xml.Serialization;

namespace tef_banpro_app.Model
{
    [XmlRoot(ElementName="SqlSettings")]
    public class SqlSettings
    {
        [XmlAttribute(AttributeName = "User")]
        public string User { get; set; } = "ICGAdmin";

        [XmlAttribute(AttributeName = "Password")]
        public string Password { get; set; } = "BBB0C5C1A6BEB6AAD2";
    }

    [XmlRoot(ElementName="DeviceSettings")]
    public class DeviceSettings
    {
        [XmlAttribute(AttributeName = "HostIp")]
        public string HostIp { get; set; } = "0.0.0.0";

        [XmlAttribute(AttributeName = "Port")]
        public int Port { get; set; } = 5000;

        [XmlAttribute(AttributeName = "Timeout")]
        public int Timeout { get; set; } = 15000;
    }

    [XmlRoot(ElementName="FormaPago")]
    public class FormaPago
    {
        [XmlAttribute(AttributeName="FormaPagoId")]
        public string FormaPagoId { get; set; }

        [XmlAttribute(AttributeName="CurrencyNumber")]
        public string CurrencyNumber { get; set; }

        [XmlAttribute(AttributeName = "DclTransactionType")]
        public int DclTransactionType { get; set; } = 1;

        [XmlAttribute(AttributeName = "Quotas")]
        public int Quotas { get; set; }

        [XmlAttribute(AttributeName = "AcquirerId")]
        public int AcquirerId { get; set; }

        [XmlAttribute(AttributeName = "MerchantId")]
        public int MerchantId { get; set; }
    }

    [XmlRoot(ElementName="RegionalSettings")]
    public class RegionalSettings
    {
        [XmlAttribute(AttributeName = "RegionId")]
        public int RegionId { get; set; }

        [XmlAttribute(AttributeName = "ReportarImpuestos")]
        public bool ReportarImpuestos { get; set; }

        [XmlAttribute(AttributeName = "UsarAnularEnLugarDeDevolver")]
        public bool UsarAnularEnLugarDeDevolver { get; set; }
    }

    [XmlRoot(ElementName="TransactionLoggingSettings")]
    public class TransactionLoggingSettings
    {
        [XmlAttribute(AttributeName = "MerchantId")]
        public string MerchantId { get; set; } = "";

        [XmlAttribute(AttributeName = "TerminalId")]
        public string TerminalId { get; set; } = "";

        [XmlAttribute(AttributeName = "Header1")]
        public string Header1 { get; set; } = "NOMBRE COMERCIO";

        [XmlAttribute(AttributeName = "Header2")]
        public string Header2 { get; set; } = "NOMBRE LOCALIDAD";

        [XmlAttribute(AttributeName = "Header3")]
        public string Header3 { get; set; } = "GRACIAS POR PREFERIRNOS";
    }

    [XmlRoot(ElementName="ConfigurationSettings")]
    public class ConfigurationSettings
    {
        [XmlElement(ElementName="ParametrosRegionales")]
        public DeviceSettings DeviceSettings  { get; set; } = new DeviceSettings();

        [XmlArray("FormasPago"), XmlArrayItem(typeof(FormaPago), ElementName = "FormaPago")]
        public List<FormaPago> FormasPago { get; set; } = new List<FormaPago>();

        [XmlAttribute(AttributeName = "VerboseMode")]
        public bool VerboseMode { get; set; }

        [XmlAttribute(AttributeName = "TransactionInfoScreen")]
        public bool TransactionInfoScreen { get; set; }

        [XmlAttribute(AttributeName = "TransactionLogging")]
        public bool TransactionLogging { get; set; }

        [XmlAttribute(AttributeName = "LocalPrinting")]
        public bool LocalPrinting { get; set; }

        [XmlAttribute(AttributeName = "PrinterName")]
        public string PrinterName { get; set; }

        [XmlElement(ElementName="RegionalSettings")]
        public RegionalSettings RegionalSettings  { get; set; } = new RegionalSettings();

        [XmlElement(ElementName="SqlSettings")]
        public SqlSettings SqlSettings  { get; set; } = new SqlSettings();

        [XmlAttribute(AttributeName = "AppPassword")]
        public string AppPassword { get; set; } = "";

        [XmlElement(ElementName="TransactionLoggingSettings")]
        public TransactionLoggingSettings TransactionLoggingSettings  { get; set; } = new TransactionLoggingSettings();
    }
}
