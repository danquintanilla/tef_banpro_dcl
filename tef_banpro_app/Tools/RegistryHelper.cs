using Microsoft.Win32;
using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace tef_banpro_app.Tools
{
    public static class RegistryHelper
    {
        /// <summary>
        /// Obtener los nombres del servidor y base de datos General desde el registro de windows
        /// </summary>
        /// <returns></returns>
        public static bool LeerRegistro()
        {
            try
            {
                // Abrir la subllave
                var key = Registry.CurrentUser.OpenSubKey(@"Software\ICG\RetailSQL2007");

                // Si existe la subllave, leer los valores
                if (key != null)
                {
                    Global.ServerName = key.GetValue("SQL_ServerName").ToString();
                    Global.GeneralDatabaseName = key.GetValue("SQL_General").ToString();
                    Global.CompanyId = (int) key.GetValue("EMPGESTFRONT");

                    // Cerra la llave
                    key.Close();

                    return true;
                }

                XtraMessageBox.AllowHtmlText = true;
                XtraMessageBox.Show(@"La llave de registro <b>RetailSQL2007</b> no fue encontrada.", @"Leer registro de Windows", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Ocurrió un error al intentar obtener los datos desde el registro de Windows.{Environment.NewLine}{ex.Message}",
                    @"Leer registro de Windows", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }
    }
}
