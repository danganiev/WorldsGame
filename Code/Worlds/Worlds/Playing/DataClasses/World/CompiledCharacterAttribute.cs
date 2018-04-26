using System;
using WorldsGame.Saving;

namespace WorldsGame.Playing.DataClasses
{
    [Serializable]
    public class CompiledCharacterAttribute
    {
        public const string AtlasName = "attributesIconAtlas.png";

        public string Name { get; private set; }

        // These are just attribute definers (think classes). Actual values (think instances) are stored in entity attributes
        public float DefaultMinValue { get; private set; }

        public float DefaultValue { get; private set; }

        public float DefaultMaxValue { get; private set; }

        public float MinMaxDiff { get; private set; }

        public int FullTextureIndex { get; set; }

        public int HalfTextureIndex { get { return FullTextureIndex + 1; } }

        //For serialization only!
        public CompiledCharacterAttribute()
        {
        }

        public CompiledCharacterAttribute(CharacterAttribute characterAttribute)
        {
            Name = characterAttribute.Name;
            DefaultMinValue = CharacterAttribute.DefaultMinValue;
            DefaultMaxValue = characterAttribute.DefaultMaxValue;
            MinMaxDiff = DefaultMaxValue - DefaultMinValue;
            DefaultValue = characterAttribute.DefaultValue;
        }
    }
}