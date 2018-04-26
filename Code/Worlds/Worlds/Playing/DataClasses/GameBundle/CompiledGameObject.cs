using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using WorldsGame.Saving;

using WorldsLib;

namespace WorldsGame.Playing.DataClasses
{
    [Serializable]
    public class CompiledGameObject
    {
        [NonSerialized]
        private CompiledGameBundle _gameBundle;

        public CompiledGameBundle GameBundle
        {
            get { return _gameBundle; }
            set { _gameBundle = value; }
        }

        public string Name { get; set; }

        public Dictionary<Vector3i, string> Blocks { get; set; }

        public Vector3i ForwardNormal { get { return new Vector3i(Vector3.Forward); } }

        //For serialization only!
        public CompiledGameObject()
        {
        }

        public CompiledGameObject(CompiledGameBundle gameBundle, GameObject gameObject)
        {
            GameBundle = gameBundle;
            Name = gameObject.Name;
            Blocks = gameObject.Blocks;
        }
    }
}