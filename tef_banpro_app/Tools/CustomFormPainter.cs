using System.Drawing;
using System.Windows.Forms;
using DevExpress.Skins;
using DevExpress.Skins.XtraForm;
using DevExpress.Utils;
using DevExpress.Utils.Drawing;

namespace tef_banpro_app.Tools 
{
    public class CustomFormPainter : FormPainter 
    {
        public HorzAlignment CaptionAlignment { get; set; }

        public CustomFormPainter(Control owner, ISkinProvider provider, HorzAlignment captionAlignment)
            : base(owner, provider) 
        {
            CaptionAlignment = captionAlignment;
        }

        protected override void DrawText(GraphicsCache cache) 
        {
            var text = Text;
            if (string.IsNullOrEmpty(text) || TextBounds.IsEmpty) 
            {
                return;
            }
            
            var appearance = new AppearanceObject(GetDefaultAppearance());
            appearance.TextOptions.Trimming = Trimming.EllipsisCharacter;
            appearance.TextOptions.HAlignment = CaptionAlignment;
            
            if (AllowHtmlDraw) 
            {
                DrawHtmlText(cache, appearance);
                return;
            }
            
            var r = RectangleHelper.GetCenterBounds(TextBounds, new Size(TextBounds.Width, CalcTextHeight(cache, appearance)));
            DrawTextShadow(cache, appearance, r);
            cache.DrawString(text, appearance.Font, appearance.GetForeBrush(cache), r, appearance.GetStringFormat());
        }
    }
}
