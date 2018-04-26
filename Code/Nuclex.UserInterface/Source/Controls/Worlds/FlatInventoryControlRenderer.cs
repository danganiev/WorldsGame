using Nuclex.UserInterface.Visuals.Flat;

namespace Nuclex.UserInterface.Source.Controls.Worlds
{
    /// <summary>
    /// Inventory control renderer
    /// </summary>
    public class FlatInventoryControlRenderer : IFlatControlRenderer<InventoryControl>
    {
        public void Render(
          InventoryControl control, IFlatGuiGraphics graphics
        )
        {
            graphics.DrawElement("inventory", control.GetAbsoluteBounds());
        }
    }
}