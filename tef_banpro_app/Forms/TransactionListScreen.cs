using System;
using System.Reflection;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraSplashScreen;
using tef_banpro_app.Model;
using tef_banpro_app.Tools;

namespace tef_banpro_app.Forms
{
    public partial class TransactionListScreen : XtraForm
    {
        public TransactionListScreen()
        {
            InitializeComponent();
            Text = $@"Integración TEF ICG / {Properties.Settings.Default.P1} (DCL) [Transacciones]";

            // Inicializar
            dtpDesde.EditValue = DateTime.Now.Date;
            dtpHasta.EditValue = DateTime.Now.Date;
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        /// <summary>
        /// Crear reglas de formato para el grid
        /// </summary>
        private void CrearReglasFormato()
        {
            try
            {
                gridView1.FormatRules.Clear();
                var gridFormatRule = new GridFormatRule();
                var formatConditionRuleExpression = new FormatConditionRuleExpression();
                gridFormatRule.Column = gridView1.Columns["TransactionType"];
                gridFormatRule.ApplyToRow = true;
                formatConditionRuleExpression.PredefinedName = "Red Fill, Red Text";
                formatConditionRuleExpression.Expression = "[TransactionType] == 'Anulación'";
                gridFormatRule.Rule = formatConditionRuleExpression;
                gridView1.FormatRules.Add(gridFormatRule);
            }
            catch (Exception ex)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {ex.Message}";
                LogHelper.LogToFile(mensaje);
                XtraMessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnListar_Click(object sender, EventArgs e)
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

                var transacciones = DataHelper.Listar(dtpDesde.DateTime.Date, dtpHasta.DateTime.Date);
                gridControl1.DataSource = transacciones;

                // Dar formato al grid
                gridView1.BestFitColumns();
                CrearReglasFormato();
                gridView1.Columns["Amount"].AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
                gridView1.Columns["TaxAmount"].AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;

                if (handle != null)
                {
                    SplashScreenManager.CloseOverlayForm(handle);
                    handle = null;
                }
            }
            catch (Exception ex)
            {
                var mensaje = $@"Error en {MethodBase.GetCurrentMethod().Name}. {ex.Message}";
                LogHelper.LogToFile(mensaje);
                XtraMessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (handle != null) SplashScreenManager.CloseOverlayForm(handle);
            }
        }

        private void gridView1_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "Amount" | e.Column.FieldName == "TaxAmount")
            {
                var value = Convert.ToDouble(e.Value) / 100;
                e.DisplayText = $"{value:N2}";
            }
        }

        private void gridView1_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            var transaccion = (DclTransaction) gridView1.GetFocusedRow();
            btnImprimir.Enabled = transaccion != null;
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            var transaccion = (DclTransaction) gridView1.GetFocusedRow();
            if (transaccion == null) return;

            XtraMessageBox.AllowHtmlText = true;
            var dialogResult =
                XtraMessageBox.Show(
                    $"¿Reimprimir voucher de <b>{transaccion.TransactionType} STAN: {transaccion.SystemTraceNumber}</b>?",
                    btnImprimir.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                var impreso = PrinterHelper.ImprimirTransaccion(transaccion, false, true);
                if (impreso)
                {
                    var imprimir = XtraMessageBox.Show($"La orden de impresión fue enviada de manera correcta.{Environment.NewLine}¿Desea imprimir la copia del comercio?", btnImprimir.Text,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (imprimir == DialogResult.Yes) PrinterHelper.ImprimirTransaccion(transaccion, true, true);
                }
            }
        }
    }
}