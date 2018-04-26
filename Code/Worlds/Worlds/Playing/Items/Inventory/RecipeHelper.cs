using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Utils;

namespace WorldsGame.Playing.Items.Inventory
{
    internal enum RecipeSize
    {
        OneByOne,
        OneByTwo,
        TwoByOne,
        TwoByTwo,
        OneByThree,
        ThreeByOne,
        TwoByThree,
        ThreeByTwo,
        ThreeByThree
    }

    internal static class RecipeHelper
    {
        internal static Dictionary<string, CompiledRecipe> Recipes { get; private set; }

        internal static Dictionary<RecipeSize, HashSet<CompiledRecipe>> RecipeMap { get; private set; }

        // NOTE: We can store Dictionaries of sets of indices instead of whole CompiledRecipes, and Union/Intersect them on search
        // but if something more simple (LINQ) works than it could be an overkill.

        // We'll need indices just for multiplayer actually

        static RecipeHelper()
        {
            Recipes = new Dictionary<string, CompiledRecipe>();
            RecipeMap = new Dictionary<RecipeSize, HashSet<CompiledRecipe>>();

            foreach (RecipeSize recipeSize in (RecipeSize[])Enum.GetValues(typeof(RecipeSize)))
            {
                RecipeMap[recipeSize] = new HashSet<CompiledRecipe>();
            }
        }

        internal static void Clear()
        {
            Recipes.Clear();

            foreach (RecipeSize recipeSize in (RecipeSize[])Enum.GetValues(typeof(RecipeSize)))
            {
                RecipeMap[recipeSize].Clear();
            }
        }

        internal static CompiledRecipe Get(string name)
        {
            name = name.ToLower();

            if (Recipes.ContainsKey(name))
            {
                return Recipes[name];
            }

            return null;
        }

        internal static RecipeSize GetRecipeSizeEnum(int xSize, int ySize)
        {
            if (xSize == 1 && ySize == 1) return RecipeSize.OneByOne;
            if (xSize == 1 && ySize == 2) return RecipeSize.OneByTwo;
            if (xSize == 2 && ySize == 1) return RecipeSize.TwoByOne;
            if (xSize == 2 && ySize == 2) return RecipeSize.TwoByTwo;
            if (xSize == 1 && ySize == 3) return RecipeSize.OneByThree;
            if (xSize == 3 && ySize == 1) return RecipeSize.ThreeByOne;
            if (xSize == 2 && ySize == 3) return RecipeSize.TwoByThree;
            if (xSize == 3 && ySize == 2) return RecipeSize.ThreeByTwo;
            if (xSize == 3 && ySize == 3) return RecipeSize.ThreeByThree;

            return RecipeSize.OneByOne;
        }

        internal static CompiledRecipe FindByItems(ICollection<string> itemNames, int xSize, int ySize)
        {
            RecipeSize recipeSize = GetRecipeSizeEnum(xSize, ySize);

            HashSet<CompiledRecipe> recipes = RecipeMap[recipeSize];

            int index = 0;
            Func<CompiledRecipe, bool> predicate = LINQPredicateBuilder.True<CompiledRecipe>();

            foreach (string itemName in itemNames)
            {
                string name = itemName;
                int index1 = index;
                predicate = predicate.And(r => r.Items[index1] == name);
                index++;
            }

            List<CompiledRecipe> resultRecipes = recipes.Where(predicate).ToList();

            if (resultRecipes.Count > 0)
            {
                return resultRecipes.First();
            }

            return null;
        }

        //        internal static BlockType Get(int key)
        //        {
        //            return Get(BlockTypeCache[key]);
        //        }

        internal static void Initialize(CompiledGameBundle compiledGameBundle)
        {
            Clear();
            //            AddToCache(AIR_BLOCK_INDEX, AIR_BLOCK_TYPE.Name);

            for (int index = 0; index < compiledGameBundle.Recipes.Count; index++)
            {
                CompiledRecipe recipe = compiledGameBundle.Recipes[index];

                Recipes.Add(recipe.Name.ToLower(), recipe);

                RecipeSize recipeSize = GetRecipeSizeEnum(recipe.XSize, recipe.YSize);

                RecipeMap[recipeSize].Add(recipe);
            }
        }
    }
}