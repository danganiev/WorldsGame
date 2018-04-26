#region License

//  TechCraft - http://techcraft.codeplex.com
//  This source code is offered under the Microsoft Public License (Ms-PL) which is outlined as follows:

//  Microsoft Public License (Ms-PL)
//  This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

//  1. Definitions
//  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
//  A "contribution" is the original software, or any additions or changes to the software.
//  A "contributor" is any person that distributes its contribution under this license.
//  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

//  2. Grant of Rights
//  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
//  (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//  3. Conditions and Limitations
//  (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
//  (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
//  (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
//  (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
//  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

#endregion License

using System;
using System.Runtime.InteropServices;
using System.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using WorldsGame.Utils.Exceptions;
using LogLevel = NLog.LogLevel;

namespace WorldsGame
{
#if WINDOWS || XBOX

    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        //Super incompatible with XBOX thing. Disallows more than one instance of the game
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint MessageBox(IntPtr hWnd, String text, String caption, uint type);

        public static string appGuid = "CB82E719-37AE-4E42-B9E4-FED7DCD4FF70";

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static void SetupLog()
        {
            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            fileTarget.FileName = "${basedir}/errorlog.txt";
            fileTarget.Layout = "${message} ${exception}";

            var rule = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
        }

        private static void Main(string[] args)
        {
            using (var mutex = new Mutex(false, appGuid))
            {
                //                if (!mutex.WaitOne(0, false))
                //                {
                //                    MessageBox(new IntPtr(0), "Another instance of Worlds is already running", "Sorry!", 0);
                //                    return;
                //                }

                SetupLog();

                bool stillRunning = true;
                bool isRestarted = false;

                while (stillRunning)
                {
                    var game = new WorldsGame(isRestarted: isRestarted);
                    try
                    {
                        stillRunning = false;
                        isRestarted = false;

                        game.Run();
                    }
                    catch (GameRestartException)
                    {
                        stillRunning = true;
                        isRestarted = true;
                    }
#if !DEBUG
                    catch (Exception e)
                    {
                        logger.FatalException("Fatal exception", e);
                        logger.Error(e.ToString());
                        throw;
                    }
#endif
                    finally
                    {
                        game.Dispose();
                    }
                }
            }
        }
    }

#endif
}