using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace tef_banpro_app.Forms
{
    public partial class PhoneScreen : XtraForm
    {
        public string Telefono;

        public PhoneScreen()
        {
            InitializeComponent();
            txtTelefono.EditValue = null;
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            Telefono = $"{txtTelefono.EditValue}".Replace("-","");
            DialogResult = DialogResult.OK;
        }
    }
}