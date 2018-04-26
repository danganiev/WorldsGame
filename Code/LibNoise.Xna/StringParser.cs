using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

using LibNoise.Xna.Generator;
using LibNoise.Xna.Operator;
using Random = LibNoise.Xna.Generator.Random;

namespace LibNoise.Xna
{
    public static class StringParser
    {
        private static void SimpleValidate(string input)
        {
            if (input == "")
            {
                throw new LibNoiseStringParserException("Input is empty.");
            }

            int openingBracketCount = input.Count(c => c == '(');
            int closingBracketCount = input.Count(c => c == ')');

            if (openingBracketCount == 0 || closingBracketCount == 0 || closingBracketCount != openingBracketCount)
            {
                throw new LibNoiseStringParserException("Opening and closing bracket count doesn't match.");
            }

            if (input.Last() != ')')
            {
                throw new LibNoiseStringParserException("Input doesn't end with a closing bracket.");
            }
        }

        public static ModuleBase Parse(string input)
        {
            // Clear whitespace for easier prey!
            string lowerInput = input.Replace(" ", string.Empty).ToLower();

            SimpleValidate(lowerInput);

            string[] openBracketSplit = lowerInput.Split('(');

            string moduleName = openBracketSplit.First();

            string remainder = "";

            for (int i = 1; i < openBracketSplit.Length; i++)
            {
                if (i != 1)
                {
                    remainder += '(';
                }

                remainder += openBracketSplit[i];
            }
            
            // The last character must always be a closing bracket
            remainder = remainder.Remove(remainder.Length - 1);

            TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;

            switch (moduleName)
            {
                // Generators
                case "billow":
                    return GetBillowModule(remainder);
                case "checker":
                    return new Checker();
                case "const":
                    return GetConstModule(remainder);
                case "cylinders":
                    return GetCylindersModule(remainder);
                case "noise":
                    return GetCachedNoiseModule(remainder);
                case "perlin":
                    return GetPerlinModule(remainder);
                case "random":
                    return GetRandomModule(remainder);
                case "riggedmultifractal":
                case "rigged":
                    return GetRiggedMultifractalModule(remainder);
                case "simplex":
                case "simplex3d":
                    return GetSimplexModule(remainder);
                case "simplex2d":
                    return GetSimplexModule2D(remainder);
                case "spheres":
                case "sphere":
                    return GetSpheresModule(remainder);
                case "voronoi":
                    return GetVoronoiModule(remainder);
                case "x":
                    return GetXYZModule('x');
                case "y":
                    return GetXYZModule('y');
                case "z":
                    return GetXYZModule('z');
                    
                // Operators
                case "abs":
                    return GetAbsModule(remainder);
                case "add":
                    return GetMathModule(remainder, MathModuleType.Add);
                case "invert":
                case "negate":
                    return GetInvertModule(remainder);
                case "max":
                    return GetMathModule(remainder, MathModuleType.Max);
                case "min":
                    return GetMathModule(remainder, MathModuleType.Min);
                case "multiply":
                    return GetMathModule(remainder, MathModuleType.Multiply);
                case "rotate":
                    return GetSpaceTransformationModule(remainder, SpaceModuleType.Rotate);
                case "scale":
                    return GetSpaceTransformationModule(remainder, SpaceModuleType.Scale);
                case "subtract":
                    return GetMathModule(remainder, MathModuleType.Subtract);
                case "translate":
                    return GetSpaceTransformationModule(remainder, SpaceModuleType.Translate);
                default:
                    throw new LibNoiseStringParserException(string.Format("There is no '{0}' module, sorry.", textInfo.ToTitleCase(moduleName)));
            }
        }

        // Generators
        private static ModuleBase GetBillowModule(string input)
        {
            string[] inputArray = input.Split(',');

            if (inputArray.Length != 5)
            {
                throw new LibNoiseStringParserException("Cannot create 'Billow' module: input has wrong parameters count, \n" +
                                                        " must be 5.");
            }

            int octaves;
            int quality;
            double persistence;
            double frequency;
            double lacunarity;

            ParsePerlinLikeInput(inputArray, out octaves, out quality, out persistence, out frequency, out lacunarity, moduleName: "Billow");

            return new Billow(frequency: frequency, lacunarity: lacunarity, persistence: persistence, octaves: octaves,
                quality: (QualityMode)quality);
        }

        private static void ParsePerlinLikeInput(string[] inputArray, out int octaves, out int quality, out double persistence,
                                                 out double frequency, out double lacunarity, string moduleName = "Perlin", string[] paramsList = null)
        {
            octaves = 0;
            quality = 0;
            persistence = 0;
            frequency = 0;
            lacunarity = 0;

            if (paramsList == null)
            {
                paramsList = new[] { "frequency", "lacunarity", "persistence", "octaves", "quality" };    
            }

            for (int index = 0; index < paramsList.Length; index++)
            {
                string param = paramsList[index];
                switch (param)
                {
                    case "frequency":
                        ParseDouble(inputArray[index], out frequency, moduleName, "frequency");
                        break;
                    case "lacunarity":
                        ParseDouble(inputArray[index], out lacunarity, moduleName, "lacunarity");
                        break;
                    case "persistence":
                        ParseDouble(inputArray[index], out persistence, moduleName, "persistence");
                        break;
                    case "octaves":
                        ParseInteger(inputArray[index], out octaves, moduleName, "octaves");
                        break;
                    case "quality":
                        ParseInteger(inputArray[index], out quality, moduleName, "quality");

                        if (quality < 0 || quality > 2)
                        {
                            throw new LibNoiseStringParserException(string.Format("Cannot create '{0}' module: quality is not between 0 and 2", moduleName));
                        }
                        break;
                    default:
                        return;
                }
            }         
        }

        private static void ParseDouble(string input, out double result, string moduleName, string paramName)
        {
            bool parseResult = double.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out result);

            if (!parseResult)
            {
                throw new LibNoiseStringParserException(
                    string.Format("Cannot create '{0}' module: {1} is not a double number", moduleName, paramName));
            }
        }

        private static void ParseInteger(string input, out int result, string moduleName, string paramName)
        {
            bool parseResult = int.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out result);

            if (!parseResult)
            {
                throw new LibNoiseStringParserException(
                    string.Format("Cannot create '{0}' module: {1} is not a integer number", moduleName, paramName));
            }
        }

        private static void ParseBool(string input, out bool result, string moduleName, string paramName)
        {
            bool parseResult = bool.TryParse(input, out result);

            if (!parseResult)
            {
                throw new LibNoiseStringParserException(
                    string.Format("Cannot create '{0}' module: {1} is not a boolean", moduleName, paramName));
            }
        }

        private static ModuleBase GetPerlinModule(string input)
        {
            string[] inputArray = input.Split(',');

            if (inputArray.Length != 5)
            {
                throw new LibNoiseStringParserException("Cannot create 'Perlin' module: input has wrong parameters count, \n" +
                                                        "must be 5.");
            }

            double frequency;
            double lacunarity;
            double persistence;
            int octaves;
            int quality;

            ParsePerlinLikeInput(inputArray, out octaves, out quality, out persistence, out frequency, out lacunarity, moduleName: "Perlin");
            
            return new Perlin(frequency: frequency, lacunarity: lacunarity, persistence: persistence, octaves: octaves, 
                quality: (QualityMode)quality);
        }

        private static ModuleBase GetRandomModule(string input)
        {
            string[] inputArray = input.Split(',');

            if (inputArray.Length != 1)
            {
                throw new LibNoiseStringParserException("Cannot create 'Random' module: input has wrong parameters count, \n" +
                                                        "must be 1.");
            }
            
            int maxValue;

            ParseInteger(inputArray[0], out maxValue, "Random", "Max value");

            return new Random(maxValue);
        }

        private static ModuleBase GetConstModule(string input)
        {
            string[] inputArray = input.Split(',');

            if (inputArray.Length != 1)
            {
                throw new LibNoiseStringParserException("Cannot create 'Const' module: input has wrong parameters count, \n" +
                                                        "must be 1.");
            }

            double value;

            try
            {
                value = Convert.ToDouble(input, CultureInfo.InvariantCulture);
            }
            catch(FormatException e)
            {
                throw new LibNoiseStringParserException("Cannot create 'Const' module: input is not a number.", e);
            }

            return new Const(value);
        }

        private static ModuleBase GetCylindersModule(string input)
        {
            string[] inputArray = input.Split(',');

            if (inputArray.Length != 1)
            {
                throw new LibNoiseStringParserException("Cannot create 'Cylinders' module: input has wrong parameters count, \n" +
                                                        "must be 1.");
            }

            double frequency;

            try
            {
                frequency = Convert.ToDouble(input, CultureInfo.InvariantCulture);
            }
            catch (FormatException e)
            {
                throw new LibNoiseStringParserException("Cannot create 'Cylinders' module: frequency is not a number.", e);
            }

            return new Cylinders(frequency); 
        }

        private static ModuleBase GetCachedNoiseModule(string input)
        {
            string[] inputArray = input.Split(',');

            if (inputArray.Length != 1)
            {
                throw new LibNoiseStringParserException("Cannot create 'Noise' module: input has wrong parameters count, \n" +
                                                        "must be 1.");
            }

            return new ValueStorage(input);
        }

        private static ModuleBase GetRiggedMultifractalModule(string input)
        {
            string[] inputArray = input.Split(',');

            if (inputArray.Length != 4)
            {
                throw new LibNoiseStringParserException("Cannot create 'RiggedMultifractal' module: input has wrong parameters count, \n" +
                                                        "must be 4.");
            }

            int octaves;
            int quality;
            double persistence;
            double frequency;
            double lacunarity;

            ParsePerlinLikeInput(inputArray, out octaves, out quality, out persistence, out frequency, out lacunarity, moduleName: "RiggedMultifractal",
                paramsList: new[] { "frequency", "lacunarity", "octaves", "quality" });

            return new RiggedMultifractal(frequency: frequency, lacunarity: lacunarity, octaves: octaves,
                quality: (QualityMode)quality);
        }

        private static ModuleBase GetSimplexModule(string input)
        {
            string[] inputArray = input.Split(',');

            if (inputArray.Length != 1 && inputArray.Length != 2)
            {
                throw new LibNoiseStringParserException("Cannot create 'Simplex' module: input has wrong parameters count, \n" +
                                                        "must be 1 or 2.");
            }

            double scale;
            ParseDouble(inputArray[0], out scale, "Simplex", "scale");

            return inputArray.Length == 1 ? new Simplex(scale: scale) : new Simplex(scale: scale, paramz: inputArray[1]);
        }

        private static ModuleBase GetSimplexModule2D(string input)
        {
            string[] inputArray = input.Split(',');

            if (inputArray.Length != 2 && inputArray.Length != 1)
            {
                throw new LibNoiseStringParserException("Cannot create 'Simplex2D' module: input has wrong parameters count, \n" +
                                                        "must be 1 or 2.");
            }

            double scale;
            ParseDouble(inputArray[0], out scale, "Simplex2D", "Scale");

            return inputArray.Length == 1 ? new Simplex2D(scale: scale) : new Simplex2D(scale: scale, paramz: inputArray[1]);
        }

        private static ModuleBase GetSpheresModule(string input)
        {
            string[] inputArray = input.Split(',');

            if (inputArray.Length != 1)
            {
                throw new LibNoiseStringParserException("Cannot create 'Spheres' module: input has wrong parameters count, \n" +
                                                        "must be 1.");
            }

            double frequency;
            ParseDouble(inputArray[0], out frequency, "Spheres", "Frequency");

            return new Spheres(frequency);
        }

        private static ModuleBase GetVoronoiModule(string input)
        {
            string[] inputArray = input.Split(',');

            if (inputArray.Length != 3)
            {
                throw new LibNoiseStringParserException("Cannot create 'Voronoi' module: input has wrong parameters count, \n" +
                                                        "must be 3.");
            }

            double frequency;
            ParseDouble(inputArray[0], out frequency, "Voronoi", "Frequency");

            double displacement;
            ParseDouble(inputArray[1], out displacement, "Voronoi", "Displacement");

            bool useDistance;
            ParseBool(inputArray[2], out useDistance, "Voronoi", "Use Distance");

            return new Voronoi(frequency: frequency, displacement: displacement, distance: useDistance);
        }

        private static ModuleBase GetXYZModule(char param)
        {
            switch (param)
            {
                case 'x':
                    return new XYZ("x");
                case 'y':
                    return new XYZ("y");
                case 'z':
                    return new XYZ("z");
            }

            return new XYZ();
        }

        private enum MathModuleType
        {            
            Add,
            Subtract,
            Multiply,
            Min,
            Max
        };

        // Operators
        private static ModuleBase GetMathModule(string input, MathModuleType type)
        {
            string moduleName = "";

            switch (type)
            {
                case MathModuleType.Add:
                    moduleName = "Add";
                    break;
                case MathModuleType.Subtract:
                    moduleName = "Subtract";
                    break;
                case MathModuleType.Multiply:
                    moduleName = "Multiply";
                    break;
                case MathModuleType.Min:
                    moduleName = "Min";
                    break;
                case MathModuleType.Max:
                    moduleName = "Max";
                    break;
            }

            int dividingCommaIndex = FindFirstLevelClosingBracket(input);

            if (dividingCommaIndex == -1)
            {
                throw new LibNoiseStringParserException(string.Format("Cannot create '{0}' module: this is the operator module, but input doesn't provide any interior modules.", moduleName));                
            }
            
            if (dividingCommaIndex == input.Length - 1 || input[dividingCommaIndex + 1] != ',')
            {
                throw new LibNoiseStringParserException(string.Format("Cannot create '{0}' module: input doesn't have a dividing comma.", moduleName));
            }

            string firstArgument = input.Substring(0, dividingCommaIndex + 1);
            string lastArgument = input.Substring(dividingCommaIndex + 2, input.Length - (dividingCommaIndex + 2));
            
            switch (type)
            {
                case MathModuleType.Add:
                    return new Add(Parse(firstArgument), Parse(lastArgument));
                case MathModuleType.Subtract:
                    return new Subtract(Parse(firstArgument), Parse(lastArgument));
                case MathModuleType.Multiply:
                    return new Multiply(Parse(firstArgument), Parse(lastArgument));
                case MathModuleType.Min:
                    return new Min(Parse(firstArgument), Parse(lastArgument));
                case MathModuleType.Max:
                    return new Max(Parse(firstArgument), Parse(lastArgument));
            }

            // Default
            return new Add(Parse(firstArgument), Parse(lastArgument));
        }

        private enum SpaceModuleType
        {            
            Rotate,
            Scale,
            Translate
        };

        private static ModuleBase GetSpaceTransformationModule(string input, SpaceModuleType type)
        {
            string moduleName = "";

            switch (type)
            {
                case SpaceModuleType.Rotate:
                    moduleName = "Rotate";
                    break;
                case SpaceModuleType.Scale:
                    moduleName = "Scale";
                    break;
                case SpaceModuleType.Translate:
                    moduleName = "Translate";
                    break;
            }

            string[] inputArray = input.Split(',');

            if (inputArray.Length < 4)
            {
                throw new LibNoiseStringParserException(string.Format("Cannot create '{0}' module: input has wrong parameters count, \n" +
                                                        "must be 4.", moduleName));
            }

            double x;
            ParseDouble(inputArray[0], out x, moduleName, "X");

            double y;
            ParseDouble(inputArray[1], out y, moduleName, "Y");

            double z;
            ParseDouble(inputArray[2], out z, moduleName, "Z");

            var moduleInputList = new List<string>();

            for (int i = 3; i < inputArray.Length; i++)
            {
                moduleInputList.Add(inputArray[i]);
            }

            string moduleInput = string.Join(",", moduleInputList);
            ModuleBase interiorModule = Parse(moduleInput);

            switch (type)
            {
                case SpaceModuleType.Rotate:
                    return new Rotate(x, y, z, interiorModule);
                case SpaceModuleType.Scale:
                    return new Scale(x, y, z, interiorModule);
                case SpaceModuleType.Translate:
                    return new Translate(x, y, z, interiorModule);
            }

            return new Rotate(x, y, z, interiorModule);
        }

        private static ModuleBase GetAbsModule(string input)
        {
            int dividingCommaIndex = FindFirstLevelClosingBracket(input);

            if (dividingCommaIndex == -1)
            {
                throw new LibNoiseStringParserException("Cannot create 'Abs' module: this is the operator module, \n" +
                                                        "but input doesn't provide the interior module.");
            }

            if (dividingCommaIndex != input.Length - 1)
            {
                throw new LibNoiseStringParserException("Cannot create 'Abs' module: input doesn't have a closing bracket \n" +
                                                                      "at the end.");
            }

            return new Abs(Parse(input));
        }

        private static ModuleBase GetInvertModule(string input)
        {
            int dividingCommaIndex = FindFirstLevelClosingBracket(input);

            if (dividingCommaIndex == -1)
            {
                throw new LibNoiseStringParserException("Cannot create 'Invert' module: this is the operator module, \n" +
                                                        "but input doesn't provide the interior module.");
            }

            if (dividingCommaIndex != input.Length - 1)
            {
                throw new LibNoiseStringParserException("Cannot create 'Invert' module: input doesn't have a closing bracket \n" +
                                                                      "at the end.");
            }

            return new Invert(Parse(input));
        }

        private static int FindFirstLevelClosingBracket(string input)
        {
            char[] inputArray = input.ToCharArray();

            int bracketDepthLevel = 0;
            bool firstBracketFound = false;

            for (int index = 0; index < inputArray.Length; index++)
            {
                char t = inputArray[index];
                if (t == '(')
                {
                    firstBracketFound = true;
                    bracketDepthLevel++;
                }

                if (t == ')')
                {
                    bracketDepthLevel--;
                }

                if (firstBracketFound && bracketDepthLevel == 0)
                {
                    return index;
                }
            }

            return -1;
        }
    }

    public class LibNoiseStringParserException : Exception
    {
        public LibNoiseStringParserException()
        {
        }

        public LibNoiseStringParserException(string message) : base(message)
        {
        }

        public LibNoiseStringParserException(string message, Exception inner) : base(message, inner)
        {
        }

        protected LibNoiseStringParserException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
