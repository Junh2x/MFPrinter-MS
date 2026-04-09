using System.Drawing;
using System.Windows.Forms;

namespace Scanlink;

public class TrayMenuRenderer : ToolStripProfessionalRenderer
{
    public TrayMenuRenderer() : base(new TrayMenuColors()) { }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        var rc = new Rectangle(Point.Empty, e.Item.Size);
        var color = e.Item.Selected ? Color.FromArgb(230, 230, 230) : Color.White;
        using var brush = new SolidBrush(color);
        e.Graphics.FillRectangle(brush, rc);
    }

    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
    {
        // 이미지 마진 영역 안 그림
    }
}

public class TrayMenuColors : ProfessionalColorTable
{
    public override Color MenuBorder => Color.FromArgb(210, 210, 210);
    public override Color MenuItemSelected => Color.FromArgb(230, 230, 230);
    public override Color MenuItemBorder => Color.Transparent;
    public override Color ToolStripDropDownBackground => Color.White;
    public override Color ImageMarginGradientBegin => Color.White;
    public override Color ImageMarginGradientMiddle => Color.White;
    public override Color ImageMarginGradientEnd => Color.White;
    public override Color SeparatorDark => Color.FromArgb(220, 220, 220);
    public override Color SeparatorLight => Color.White;
}
