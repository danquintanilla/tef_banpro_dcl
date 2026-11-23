using System.Collections.Generic;
using System.Xml.Serialization;

namespace tef_banpro_app.Model
{
    [XmlRoot(ElementName = "Registros")]
    public class Registros
    {
        [XmlAttribute(AttributeName = "CodArticulo")]
        public string CodArticulo { get; set; }

        [XmlAttribute(AttributeName = "Descripcion")]
        public string Descripcion { get; set; }

        [XmlAttribute(AttributeName = "Unidades")]
        public string Unidades { get; set; }

        [XmlAttribute(AttributeName = "Precio")]
        public string Precio { get; set; }

        [XmlAttribute(AttributeName = "Importe")]
        public string Importe { get; set; }

        [XmlAttribute(AttributeName = "Impuesto")]
        public string Impuesto { get; set; }

        [XmlAttribute(AttributeName = "ImporteIva")]
        public string ImporteIva { get; set; }

        [XmlAttribute(AttributeName = "CodBarras")]
        public string CodBarras { get; set; }

        [XmlAttribute(AttributeName = "Talla")]
        public string Talla { get; set; }

        [XmlAttribute(AttributeName = "Color")]
        public string Color { get; set; }
    }

    [XmlRoot(ElementName="Peticion")]
	public class Peticion 
    {
        [XmlArray("Lineas"), XmlArrayItem(typeof(Registros), ElementName = "Registros")]
        public List<Registros> Lineas { get; set; }
        
        //      [XmlElement(ElementName="Lineas")]
        //public Lineas Lineas { get; set; }

        [XmlAttribute(AttributeName="TipoTx")]
		public string TipoTx { get; set; }

		[XmlAttribute(AttributeName="NumTicket")]
		public string NumTicket { get; set; }

		[XmlAttribute(AttributeName="CodCajera")]
		public string CodCajera { get; set; }

		[XmlAttribute(AttributeName="NomCajera")]
		public string NomCajera { get; set; }

		[XmlAttribute(AttributeName="EsSupervisor")]
		public string EsSupervisor { get; set; }

		[XmlAttribute(AttributeName="Importe")]
		public string Importe { get; set; }

		[XmlAttribute(AttributeName="CashBack")]
		public string CashBack { get; set; }

		[XmlAttribute(AttributeName="ImporteTotalMinuta")]
		public string ImporteTotalMinuta { get; set; }

		[XmlAttribute(AttributeName="NumDec")]
		public string NumDec { get; set; }

		[XmlAttribute(AttributeName="Fp")]
		public string Fp { get; set; }

		[XmlAttribute(AttributeName="CodTienda")]
		public string CodTienda { get; set; }

		[XmlAttribute(AttributeName="Caja")]
		public string Caja { get; set; }

		[XmlAttribute(AttributeName="FechaOp")]
		public string FechaOp { get; set; }

		[XmlAttribute(AttributeName="HoraOp")]
		public string HoraOp { get; set; }

		[XmlAttribute(AttributeName="DescMoneda")]
		public string DescMoneda { get; set; }

		[XmlAttribute(AttributeName="CodMoneda")]
		public string CodMoneda { get; set; }

		[XmlAttribute(AttributeName="ICGServer")]
		public string IcgServer { get; set; }

		[XmlAttribute(AttributeName="ICGUser")]
		public string IcgUser { get; set; }

		[XmlAttribute(AttributeName="ICGNameDB")]
		public string IcgNameDb { get; set; }

		[XmlAttribute(AttributeName="ICGPasswordDB")]
		public string IcgPasswordDb { get; set; }

		[XmlAttribute(AttributeName="ICGTipoApp")]
		public string IcgTipoApp { get; set; }

		[XmlAttribute(AttributeName="PrecioIncluyeImpuesto")]
		public string PrecioIncluyeImpuesto { get; set; }

		[XmlAttribute(AttributeName="Serie")]
		public string Serie { get; set; }

		[XmlAttribute(AttributeName="NumLinPago")]
		public string NumLinPago { get; set; }

		[XmlAttribute(AttributeName="CodCliente")]
		public string CodCliente { get; set; }

		[XmlAttribute(AttributeName="SerieAbono")]
		public string SerieAbono { get; set; }

		[XmlAttribute(AttributeName="NumeroAbono")]
		public string NumeroAbono { get; set; }

		[XmlAttribute(AttributeName="FechaAbono")]
		public string FechaAbono { get; set; }

		[XmlAttribute(AttributeName="EsNegativo")]
		public string EsNegativo { get; set; }

		[XmlAttribute(AttributeName="MedioDePago")]
		public string MedioDePago { get; set; }
	}

	[XmlRoot(ElementName="Respuesta")]
	public class Respuesta {
		[XmlAttribute(AttributeName="Deslizada")]
		public string Deslizada { get; set; }

		[XmlAttribute(AttributeName="Pista1")]
		public string Pista1 { get; set; }

		[XmlAttribute(AttributeName="Pista2")]
		public string Pista2 { get; set; }

		[XmlAttribute(AttributeName="Pan")]
		public string Pan { get; set; }

		[XmlAttribute(AttributeName="DataCaduca")]
		public string DataCaduca { get; set; }

		[XmlAttribute(AttributeName="CVC")]
		public string Cvc { get; set; }

		[XmlAttribute(AttributeName="Status")]
		public string Status { get; set; }

		[XmlAttribute(AttributeName="InfoError")]
		public string InfoError { get; set; }

		[XmlAttribute(AttributeName="CodAutorizacion")]
		public string CodAutorizacion { get; set; }

		[XmlAttribute(AttributeName="IdTransaccion")]
		public string IdTransaccion { get; set; }

		[XmlAttribute(AttributeName="CentroAutorizador")]
		public string CentroAutorizador { get; set; }

		[XmlAttribute(AttributeName="IdOperacion")]
		public string IdOperacion { get; set; }

		[XmlAttribute(AttributeName="IdLote")]
		public string IdLote { get; set; }

		[XmlAttribute(AttributeName="TerminalOriginador")]
		public string TerminalOriginador { get; set; }

		[XmlAttribute(AttributeName="ComercioOriginador")]
		public string ComercioOriginador { get; set; }

		[XmlAttribute(AttributeName="EstadoOnOff")]
		public string EstadoOnOff { get; set; }

		[XmlAttribute(AttributeName="Afiliacion")]
		public string Afiliacion { get; set; }

		[XmlAttribute(AttributeName="EsquemaValidacion")]
		public string EsquemaValidacion { get; set; }

		[XmlAttribute(AttributeName="AutPalomeo")]
		public string AutPalomeo { get; set; }

		[XmlAttribute(AttributeName="BinTarjeta")]
		public string BinTarjeta { get; set; }

		[XmlAttribute(AttributeName="MarcaTarjeta")]
		public string MarcaTarjeta { get; set; }

		[XmlAttribute(AttributeName="IdRTS")]
		public string IdRts { get; set; }

		[XmlAttribute(AttributeName="sDtoPremia")]
		public string SDtoPremia { get; set; }

		[XmlAttribute(AttributeName="AceptaPagoParcial")]
		public string AceptaPagoParcial { get; set; }

		[XmlAttribute(AttributeName="InternalCode")]
		public string InternalCode { get; set; }

		[XmlAttribute(AttributeName="Collective")]
		public string Collective { get; set; }

		[XmlAttribute(AttributeName="Id")]
		public string Id { get; set; }

		[XmlAttribute(AttributeName="Id1")]
		public string Id1 { get; set; }

		[XmlAttribute(AttributeName="Id2")]
		public string Id2 { get; set; }

		[XmlAttribute(AttributeName="Saldo")]
		public string Saldo { get; set; }
	}

    [XmlRoot("OperacionesICG", Namespace = "A")]
    public class OperacionesIcg
    {
        [XmlElement(ElementName = "Peticion")]
        public Peticion Peticion { get; set; }

        [XmlElement(ElementName = "Respuesta")]
        public Respuesta Respuesta { get; set; }
    }
}
