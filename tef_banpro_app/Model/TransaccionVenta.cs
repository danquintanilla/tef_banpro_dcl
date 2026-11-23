namespace tef_banpro_app.Model
{
    public class TransaccionVenta
    {
        public string Serie { get; set;}
        public int Numero { get; set;}
        public double TotalNeto { get; set;}
        public double TotalImpuestos { get; set;}
        public double DescuentoComercial { get; set; } = 0.0;
    }
}
