using System;
using System.Collections.Generic;
using WorldsGame.Saving;

namespace WorldsGame.Playing.DataClasses
{
    [Serializable]
    public class CompiledRecipe
    {
        [NonSerialized]
        private CompiledGameBundle _gameBundle;

        public CompiledGameBundle GameBundle
        {
            get { return _gameBundle; }
            set { _gameBundle = value; }
        }

        public string Name { get; set; }

        public int XSize { get; set; }

        public int YSize { get; set; }

        // This should always have XSize * YSize size.
        public Dictionary<int, string> Items { get; set; }

        public string ResultItem { get; set; }

        public int ResultItemQuantity { get; set; }

        //For serialization only!
        public CompiledRecipe()
        {
        }

        public CompiledRecipe(CompiledGameBundle gameBundle, Recipe recipe)
        {
            GameBundle = gameBundle;
            Name = recipe.Name;
            XSize = recipe.XSize;
            YSize = recipe.YSize;
            Items = recipe.Items;
            ResultItem = recipe.ResultItem;
            ResultItemQuantity = recipe.ResultItemQuantity;
        }
    }
}