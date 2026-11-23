using System.ComponentModel;

namespace tef_banpro_app.Enums
{
    public enum DclTransactionTypes
    {
        [Description("Compra")]
        Sale = 1,
        [Description("Compra con Puntos")]
        Points = 2,
        [Description("Compra con Billetera")]
        Wallet = 3,
        [Description("Extrafinanciamiento")]
        Extrafinancing = 4,
        [Description("Intrafinanciamiento")]
        Intrafinancing = 5,
        [Description("Payphone")]
        Payphone = 6,
        [Description("Anulacion")]
        Void = 7,
        [Description("Devolucion")]
        Refund = 8
    }
}
