using System;
using LibNoise.Xna;
using WorldsGame.Saving;

namespace WorldsGame.Playing.DataClasses
{
    [Serializable]
    public class CompiledNoise
    {
        //        [NonSerialized]
        //        private CompiledGameBundle _gameBundle;
        //
        //        public CompiledGameBundle GameBundle
        //        {
        //            get { return _gameBundle; }
        //            set { _gameBundle = value; }
        //        }

        private string _noiseFunctionText;

        public string NoiseFunctionText
        {
            get { return _noiseFunctionText; }
            set
            {
                _noiseFunctionText = value;
                NoiseFunction = StringParser.Parse(value);
            }
        }

        public string Name { get; private set; }

        [NonSerialized]
        private ModuleBase _noiseFunction;

        public ModuleBase NoiseFunction
        {
            get
            {
                if (_noiseFunction == null)
                {
                    NoiseFunction = StringParser.Parse(NoiseFunctionText);
                }
                return _noiseFunction;
            }
            private set { _noiseFunction = value; }
        }

        //For serialization only!
        public CompiledNoise()
        {
        }

        public CompiledNoise(Noise noise)
        {
            //            GameBundle = bundle;
            Name = noise.Name;
            NoiseFunctionText = noise.NoiseFunction;
        }
    }
}