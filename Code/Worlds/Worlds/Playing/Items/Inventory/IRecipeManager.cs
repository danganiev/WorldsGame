using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorldsGame.Playing.DataClasses;

namespace WorldsGame.Playing.Items.Inventory
{
    internal interface IRecipeManager
    {
        InventoryItem Craft(int recipeXSize, int recipeYSize, Dictionary<int, string> items);
    }

    // Client on multiplayer uses this
    internal class NetworkRecipeManager : IRecipeManager
    {
        public InventoryItem Craft(int recipeXSize, int recipeYSize, Dictionary<int, string> items)
        {
            if (recipeXSize == 0 || recipeYSize == 0)
            {
                return null;
            }

            throw new NotImplementedException();
        }
    }

    // Server and singleplayer use this
    internal class LocalRecipeManager : IRecipeManager
    {
        public InventoryItem Craft(int recipeXSize, int recipeYSize, Dictionary<int, string> items)
        {
            if (recipeXSize == 0 || recipeYSize == 0)
            {
                return null;
            }

            CompiledRecipe recipe = RecipeHelper.FindByItems(items.Values, recipeXSize, recipeYSize);

            if (recipe != null)
            {
                return new InventoryItem
                              {
                                  Name = recipe.ResultItem,
                                  Quantity = recipe.ResultItemQuantity
                              };
            }

            return null;
        }
    }
}