using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using tef_banpro_app.Tools;

namespace tef_banpro_app.Forms
{
    public partial class ContrasenaForm : XtraForm
    {
        public ContrasenaForm()
        {
            InitializeComponent();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            // Validar que la contrasena sea igual
            if (txtPassword.Text.Trim() != txtPassword2.Text.Trim())
            {
                XtraMessageBox.Show("Las contraseñas no coinciden. Verifique por favor.", Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtPassword2.Focus();
                return;
            }

            // Validar que la contraseña no contenga caracteres inválidos
            var encriptado = SecurityTools.Encrypt(txtPassword.Text.Trim());
            var esPar = encriptado.Length % 2 == 0;
            if (!esPar)
            {
                XtraMessageBox.Show($"La contraseña digitada contiene caracteres inválidos.{Environment.NewLine}Por favor, intente con una contraseña diferente.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtPassword.Focus();
                return;
            }

            // Actualizar la contrasena
            Global.ConfigurationSettings.AppPassword = encriptado;
            
            // Retornar el resultado
            DialogResult = DialogResult.OK;
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            // Retornar el resultado
            DialogResult = DialogResult.Cancel;
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) txtPassword2.Focus();
        }

        private void txtPassword2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) btnAceptar.Focus();
        }
    }
}