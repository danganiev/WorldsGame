using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NLog.Config;
using NLog.Targets;
using WorldsGame;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Saving.World;
using WorldsGame.Utils;
using WorldsGame.Utils.Exceptions;
using FormsMessageBox = System.Windows.Forms.MessageBox;
using WGame = WorldsGame.WorldsGame;

namespace WorldsServer
{
    public partial class ServerForm : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint MessageBox(IntPtr hWnd, String text, String caption, uint type);

        public static string appGuid = "CB82E719-37AE-4E42-B9E5-FED7DCD4FF71";

        //Because THREADING
        private WorldSave _worldSave;

        private string SelectedWorldType { get; set; }

        private string WorldName { get; set; }

        private bool IsNewGame { get; set; }

        private string Seed { get; set; }

        private string Port { get; set; }

        private WorldSave SelectedWorldSave
        {
            get { return (WorldSave)loadGameList.SelectedItems[0]; }
        }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private PleaseWaitForm _pleaseWait;

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

        public ServerForm()
        {
            InitializeComponent();
            MoveControls();
            LoadData();
            newGameButton.Checked = true;
        }

        private static SaverHelper<WorldSettings> WorldsSaverHelper
        {
            get { return new SaverHelper<WorldSettings>(WorldSettings.StaticContainerName); }
        }

        private void GoButton_Click(object sender, EventArgs e)
        {
            IsNewGame = newGameButton.Checked;

            if (IsPortOK())
            {
                if (newGameButton.Checked)
                {
                    NewGame();
                }
                else if (loadGameButton.Checked)
                {
                    LoadGame();
                }
            }
        }

        private void GameLoop()
        {
            using (var mutex = new Mutex(false, appGuid))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox(new IntPtr(0), "Another instance of Worlds is already running", "Sorry!", 0);
                    return;
                }

                SetupLog();

                bool stillRunning = true;

                while (stillRunning)
                {
                    var game = new WGame(new ServerSettings
                    {
                        MaxPlayers = 16,
                        WorldTypeName = SelectedWorldType,
                        WorldName = WorldName,
                        IsNewGame = IsNewGame,
                        Seed = Seed,
                        WorldSave = _worldSave,
                        Port = Convert.ToInt32(Port)
                    });

                    try
                    {
                        stillRunning = false;

                        game.Run();
                    }
                    catch (GameRestartException)
                    {
                        stillRunning = true;
                    }
                    catch (Exception e)
                    {
                        logger.ErrorException("Server exception:", e);
                        logger.Error(e.ToString());
                    }
                    finally
                    {
                        game.Dispose();
                    }
                }
            }
        }

        private void MoveControls()
        {
            loadGamePanel.Top = newGamePanel.Top;
        }

        private void LoadData()
        {
            LoadWorldTypes();
            LoadSavedGames();
        }

        private void LoadWorldTypes()
        {
            ListLoader.LoadList(worldTypesList, WorldsSaverHelper);
        }

        private void LoadSavedGames()
        {
            loadGameList.Items.Clear();

            foreach (WorldSave worldSave in WorldSave.StaticSaverHelper().LoadList())
            {
                loadGameList.Items.Add(worldSave);
            }
        }

        private void NewGame()
        {
            string message = "";
            if (nameBox.TextLength == 0)
            {
                message = "Please name your world.";
            }

            if (worldTypesList.SelectedItems.Count != 1)
            {
                message = "Please select a world type";
            }

            if (message != "")
            {
                FormsMessageBox.Show(message, "Server can't be started", MessageBoxButtons.OK);
                return;
            }

            SelectedWorldType = worldTypesList.SelectedItem.ToString();
            WorldName = nameBox.Text;
            Seed = seedLabel.Text;
            Port = portTextBox.Text;

            var gameThread = new Thread(GameLoop) { IsBackground = false };
            gameThread.Start();
        }

        private void LoadGame()
        {
            string message = "";
            if (loadGameList.SelectedItems.Count != 1)
            {
                message = "Please select a world to load";
            }

            if (message != "")
            {
                FormsMessageBox.Show(message, "Server can't be started", MessageBoxButtons.OK);
                return;
            }

            _worldSave = SelectedWorldSave;
            IsNewGame = false;
            Port = portTextBox.Text;

            var gameThread = new Thread(GameLoop) { IsBackground = false };
            gameThread.Start();
        }

        private bool IsPortOK()
        {
            bool isOK = true;
            if (portTextBox.Text == "")
            {
                isOK = false;
            }
            else
            {
                int port = 0;
                try
                {
                    port = Convert.ToInt32(portTextBox.Text);
                }
                catch (OverflowException)
                {
                    isOK = false;
                }
                catch (FormatException)
                {
                    isOK = false;
                }

                if (port <= 1024 || port > 49151)
                {
                    isOK = false;
                }
            }

            if (!isOK)
            {
                FormsMessageBox.Show("Please enter a valid port. Port must be a number from 1024 to 49151", "Server can't be started", MessageBoxButtons.OK);

                return false;
            }

            return true;
        }

        private void newGameButton_CheckedChanged(object sender, EventArgs e)
        {
            bool value = newGameButton.Checked;

            loadGameButton.Checked = !value;

            newGamePanel.Visible = true;
            loadGamePanel.Visible = false;
        }

        private void loadGameButton_CheckedChanged(object sender, EventArgs e)
        {
            bool value = loadGameButton.Checked;

            newGameButton.Checked = !value;

            loadGamePanel.Visible = true;
            newGamePanel.Visible = false;
        }

        private void loadSinglePlayerButton_Click(object sender, EventArgs e)
        {
            DialogResult dlgRes;

            dlgRes = FormsMessageBox.Show(
            "This will load world settings from singleplayer, but not savegames. It will overwrite current server data. Do you wish to continue?",
            "Load singleplayer data",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Question);

            if (dlgRes == DialogResult.OK)
            {
                var bw = new BackgroundWorker { WorkerSupportsCancellation = true, WorkerReportsProgress = false };
                bw.DoWork += LoadSinglePlayerData;
                bw.RunWorkerCompleted += (o, args) =>
                                         {
                                             _pleaseWait.Hide();
                                             LoadData();
                                         };

                bw.RunWorkerAsync();
                _pleaseWait = new PleaseWaitForm();
                _pleaseWait.SetText("Please wait. Copying from singleplayerdata...");
                _pleaseWait.Show();
            }
        }

        private void LoadSinglePlayerData(object sender, DoWorkEventArgs e)
        {
            string worldsSavesPath = Path.Combine(SavingUtils.FullSavingPath, Constants.SINGLEPLAYER_SAVE_FOLDER_NAME);
            string serverSavesPath = Path.Combine(SavingUtils.FullSavingPath, Constants.SERVER_SAVE_FOLDER_NAME);

            string compiledGameBundleName = CompiledGameBundleSave.StaticContainerName;
            string worldsName = WorldSave.StaticContainerName;

            foreach (string dirPath in Directory.GetDirectories(worldsSavesPath, "*",
                SearchOption.AllDirectories))
            {
                if (e.Cancel)
                {
                    return;
                }

                string[] newPathArray = dirPath.Split(Path.DirectorySeparatorChar);
                int savedGamesIndex = 0;

                for (int index = 0; index < newPathArray.Length; index++)
                {
                    string dir = newPathArray[index];
                    if (dir == "SavedGames")
                    {
                        savedGamesIndex = index;
                    }
                }

                if (savedGamesIndex == 0)
                {
                    // Something is wrong
                    return;
                }

                int nameIndex = savedGamesIndex + 2;
                string lastDirName = newPathArray[nameIndex];

                if (lastDirName == compiledGameBundleName || lastDirName == worldsName)
                {
                    continue;
                }

                Directory.CreateDirectory(dirPath.Replace(worldsSavesPath, serverSavesPath));
            }

            foreach (string newPath in Directory.GetFiles(worldsSavesPath, "*.*", SearchOption.AllDirectories))
            {
                if (e.Cancel)
                {
                    return;
                }

                string[] newPathArray = newPath.Split(Path.DirectorySeparatorChar);
                int savedGamesIndex = 0;

                for (int index = 0; index < newPathArray.Length; index++)
                {
                    string dir = newPathArray[index];
                    if (dir == "SavedGames")
                    {
                        savedGamesIndex = index;
                    }
                }

                if (savedGamesIndex == 0)
                {
                    // Something is wrong
                    return;
                }

                int nameIndex = savedGamesIndex + 2;
                string containerName = newPathArray[nameIndex];

                if (containerName == compiledGameBundleName || containerName == worldsName)
                {
                    continue;
                }

                File.Copy(newPath, newPath.Replace(worldsSavesPath, serverSavesPath), true);
            }
        }
    }
}