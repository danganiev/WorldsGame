using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WorldsGame.Network.Manager;
using WorldsGame.Network.Message;
using WorldsGame.Network.Message.Players;
using WorldsGame.Playing.Players;
using WorldsGame.Utils;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Network.Chat
{
    internal class Chat : IDisposable
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteFont _font;
        private Dictionary<string, Color> _playerColors;

        // Timestamp and chatline
        private readonly SortedDictionary<double, ChatLine> _chatHistory;

        private SayLine _sayLine;

        //        private SpriteBatch _spriteBatch;
        private bool _isFirstChar;

        internal bool IsSayLineOn { get; private set; }

        internal ClientNetworkManager NetworkManager { get; set; }

        private ClientMessageProcessor _clientMessageProcessor;

        internal ClientMessageProcessor ClientMessageProcessor
        {
            get { return _clientMessageProcessor; }
            set
            {
                _clientMessageProcessor = value;
                _clientMessageProcessor.OnChatMessage += OnChatMessage;
            }
        }

        internal ClientPlayerManager ClientPlayerManager { get; set; }

        private ClientPlayerActionManager _playerActionManager;

        internal ClientPlayerActionManager PlayerActionManager
        {
            get { return _playerActionManager; }
            set
            {
                _playerActionManager = value;

                if (value != null)
                {
                    _playerActionManager.OnPlayerWroteInChat += ToggleSayLineOn;
                }
            }
        }

        internal event Action SayLineIsOn = () => { };

        internal event Action SayLineIsOff = () => { };

        internal Chat(GraphicsDeviceManager graphics)
        {
            _graphics = graphics;
            _playerColors = new Dictionary<string, Color>();
            _chatHistory = new SortedDictionary<double, ChatLine>();
            IsSayLineOn = false;

            Initialize();
        }

        private void Initialize()
        {
            //            Messenger.On("TKeyPressed", ToggleSayLineOn);
        }

        private void OnChatMessage(ChatMessage chatMessage)
        {
            var chatLine = new ChatLine(ClientPlayerManager.GetPlayerName(chatMessage.PlayerSlot), chatMessage.Text, _font);
            if (_chatHistory.Count > 5)
            {
                KeyValuePair<double, ChatLine> firstChatLine = _chatHistory.First();
                _chatHistory.Remove(firstChatLine.Key);
            }
            _chatHistory.Add(NetworkManager.Connection.GetLocalTime(chatMessage.Timestamp), chatLine);
        }

        internal void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("Fonts/DefaultFont");

            //            _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);
            _sayLine = new SayLine(_font);
        }

        internal void Update(GameTime gameTime)
        {
            double timestamp = NetTime.Now;
            var expiredKeys = (from line in _chatHistory
                               where line.Key + 10 < timestamp
                               select line.Key).ToList();

            foreach (double expiredKey in expiredKeys)
            {
                _chatHistory.Remove(expiredKey);
            }
        }

        internal void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //            _spriteBatch.Begin();
            if (IsSayLineOn)
            {
                _sayLine.Draw(spriteBatch);
            }

            var i = 1;
            foreach (KeyValuePair<double, ChatLine> chatLine in _chatHistory)
            {
                int height = SayLine.Y - i * 20;
                i++;
                chatLine.Value.Draw(spriteBatch, height);
            }
            //            _spriteBatch.End();
        }

        internal void ToggleSayLineOff()
        {
            IsSayLineOn = false;
            //            Messenger.On("TKeyPressed", ToggleSayLineOn);
            if (PlayerActionManager != null)
            {
                PlayerActionManager.OnPlayerWroteInChat += ToggleSayLineOn;
            }

            Messenger.Off<char>("CharacterEntered", OnCharacterEntered);
            SayLineIsOff();
        }

        private void ToggleSayLineOn()
        {
            IsSayLineOn = true;
            _isFirstChar = true;
            //            Messenger.Off("TKeyPressed", ToggleSayLineOn);

            if (PlayerActionManager != null)
            {
                PlayerActionManager.OnPlayerWroteInChat -= ToggleSayLineOn;
            }

            Messenger.On<char>("CharacterEntered", OnCharacterEntered);
            SayLineIsOn();
            _sayLine.Text = "";
        }

        private void OnCharacterEntered(char character)
        {
            if (_isFirstChar)
            {
                _sayLine.Text = "";
                _isFirstChar = false;
                return;
            }

            switch ((int)character)
            {
                case InputController.ENTER:
                    Say();
                    break;

                case InputController.BACKSPACE:
                    if (_sayLine.Text.Length > 0)
                    {
                        _sayLine.Text = _sayLine.Text.Substring(0, _sayLine.Text.Length - 1);
                    }
                    break;

                default:
                    _sayLine.Text += character;
                    break;
            }
        }

        private void Say()
        {
            if (NetworkManager == null)
            {
                ToggleSayLineOff();
                return;
            }

            var message = new ChatMessage
            {
                Text = _sayLine.Text
            };

            message.Send(NetworkManager);

            ToggleSayLineOff();
        }

        public void Dispose()
        {
            SayLineIsOn = null;
            SayLineIsOff = null;

            if (_clientMessageProcessor != null)
            {
                _clientMessageProcessor.OnChatMessage -= OnChatMessage;
            }

            //            spriteBatch.Dispose();
        }
    }
}