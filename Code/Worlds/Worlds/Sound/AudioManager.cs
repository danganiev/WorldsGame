using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Sound
{
    /// <summary>
    /// Manages playback of sounds and music.
    /// </summary>
    public class AudioManager : GameComponent, IAudioManager
    {
        // Change MaxSounds to set the maximum number of simultaneous sounds that can be playing.
        private const int MaxSounds = 32;

        private const int MAX_HEARABLE_DISTANCE = 32;

        private ContentManager _content;

        private readonly Dictionary<string, Song> _songs = new Dictionary<string, Song>();
        private readonly Dictionary<string, SoundEffect> _sounds = new Dictionary<string, SoundEffect>();
        private readonly Dictionary<string, SoundEffectInstance> _soundInstances = new Dictionary<string, SoundEffectInstance>();

        private Song _currentSong;
        //        private readonly SoundEffectInstance[] _playingSounds = new SoundEffectInstance[MaxSounds];

        private bool _isMusicPaused;

        private bool _songsEnable;
        private bool _soundsEnable;

        //        private bool _needsLoadContent;

        /// <summary>
        /// Gets the name of the currently playing song, or null if no song is playing.
        /// </summary>
        public string CurrentSong { get; private set; }

        /// <summary>
        /// Gets or sets the volume to play songs. 1.0f is max volume.
        /// </summary>
        public float MusicVolume
        {
            get { return MediaPlayer.Volume; }
            set { MediaPlayer.Volume = value; }
        }

        /// <summary>
        /// Gets or sets the master volume for all sounds. 1.0f is max volume.
        /// </summary>
        public float SoundVolume
        {
            get { return SoundEffect.MasterVolume; }
            set { SoundEffect.MasterVolume = value; }
        }

        /// <summary>
        /// Gets whether a song is playing or paused (i.e. not stopped).
        /// </summary>
        public bool IsSongActive { get { return _currentSong != null && MediaPlayer.State != MediaState.Stopped; } }

        /// <summary>
        /// Gets whether the current song is paused.
        /// </summary>
        public bool IsSongPaused { get { return _currentSong != null && _isMusicPaused; } }

        public bool SongsEnable
        {
            get { return _songsEnable; }
            set
            {
                _songsEnable = value;
                if (!_songsEnable) StopSong();
            }
        }

        public bool SoundsEnable
        {
            get { return _soundsEnable; }
            set
            {
                _soundsEnable = value;
                if (!_soundsEnable) StopAllSounds();
            }
        }

        /// <summary>
        /// Creates a new Audio Manager. Add this to the Components collection of your Game.
        /// </summary>
        /// <param name="game">The Game</param>
        public AudioManager(Game game)
            : base(game)
        {
            _songsEnable = MediaPlayer.GameHasControl;
            _soundsEnable = true;

            _isMusicPaused = false;
            _currentSong = null;

            //            _needsLoadContent = true;

            MusicVolume = 1f;
            SoundVolume = 1f;
        }

        public override void Initialize()
        {
            Subscribe();
        }

        public void Subscribe()
        {
            Messenger.On<string, AudioListener, AudioEmitter>("PlaySound", PlaySound);
        }

        /// <summary>
        /// Loads the ContentManager in AudioManager. Should not be called more than once.
        /// </summary>
        //        public void LoadContent()
        //        {
        //            LoadContent(String.Empty);
        //        }

        /// <summary>
        /// Loads the ContentManager in AudioManager. Should not be called more thatn once.
        /// </summary>
        /// <param name="contentFolder">Folder where the audio files are located</param>
        //        public void LoadContent(string contentFolder)
        //        {
        //            _content = contentFolder != String.Empty ? new ContentManager(Game.Content.ServiceProvider, contentFolder) : new ContentManager(Game.Content.ServiceProvider, Game.Content.RootDirectory);
        //            _needsLoadContent = false;
        //        }

        /// <summary>
        /// Loads a Song into the AudioManager.
        /// </summary>
        /// <param name="songName">Name of the song to load</param>
        /// <param name="songPath">Path to the song asset file</param>
        public void LoadSong(string songName, string songPath)
        {
            //            if (_needsLoadContent)
            //            {
            //                throw new InvalidOperationException("Content has not yet been Loaded. Use instruction LoadContent");
            //            }

            if (_songs.ContainsKey(songName))
            {
                throw new InvalidOperationException(string.Format("Song '{0}' has already been loaded", songName));
            }

            _songs.Add(songName, _content.Load<Song>(songPath));
        }

        /// <summary>
        /// Loads a SoundEffect into the AudioManager.
        /// </summary>
        /// <param name="soundName">Name of the sound to load</param>
        /// <param name="soundPath">Path to the song asset file</param>
        public void LoadSound(string soundName, string soundPath)
        {
            //            if (_needsLoadContent)
            //            {
            //                throw new InvalidOperationException("Content has not yet been Loaded. Use instruction LoadContent");
            //            }

            if (_sounds.ContainsKey(soundName))
            {
                throw new InvalidOperationException(string.Format("Sound '{0}' has already been loaded", soundName));
            }

            //            _sounds.Add(soundName, _content.Load<SoundEffect>(soundPath));
            using (FileStream fs = File.OpenRead(soundPath))
            {
                _sounds.Add(soundName, SoundEffect.FromStream(fs));
            }
        }

        /// <summary>
        /// Unloads all loaded songs and sounds.
        /// </summary>
        public void UnloadContent()
        {
            _content.Unload();
        }

        /// <summary>
        /// Starts playing the song with the given name. If it is already playing, this method
        /// does nothing. If another song is currently playing, it is stopped first.
        /// </summary>
        /// <param name="songName">Name of the song to play</param>
        /// <param name="loop">True if song should loop, false otherwise</param>
        public void PlaySong(string songName, bool loop)
        {
            if (CurrentSong == songName || !_songsEnable) return;

            if (_currentSong != null)
                MediaPlayer.Stop();

            if (!_songs.TryGetValue(songName, out _currentSong))
                throw new ArgumentException(string.Format("Song '{0}' not found", songName));

            CurrentSong = songName;

            _isMusicPaused = false;
            MediaPlayer.IsRepeating = loop;
            MediaPlayer.Play(_currentSong);

            if (!Enabled)
                MediaPlayer.Pause();
        }

        /// <summary>
        /// Pauses the currently playing song. This is a no-op if the song is already paused,
        /// or if no song is currently playing.
        /// </summary>
        public void PauseSong()
        {
            if (_currentSong != null && !_isMusicPaused)
            {
                if (Enabled) MediaPlayer.Pause();
                _isMusicPaused = true;
            }
        }

        /// <summary>
        /// Resumes the currently paused song. This is a no-op if the song is not paused,
        /// or if no song is currently playing.
        /// </summary>
        public void ResumeSong()
        {
            if (_currentSong != null && _isMusicPaused)
            {
                if (Enabled) MediaPlayer.Resume();
                _isMusicPaused = false;
            }
        }

        /// <summary>
        /// Stops the currently playing song. This is a no-op if no song is currently playing.
        /// </summary>
        public void StopSong()
        {
            if (_currentSong != null && MediaPlayer.State != MediaState.Stopped)
            {
                MediaPlayer.Stop();
                _isMusicPaused = false;
            }
        }

        /// <summary>
        /// Plays the sound of the given name with the given parameters.
        /// </summary>
        /// <param name="soundName">Name of the sound</param>
        /// <param name="volume">Volume, 0.0f to 1.0f</param>
        /// <param name="pitch">Pitch, -1.0f (down one octave) to 1.0f (up one octave)</param>
        /// <param name="pan">Pan, -1.0f (full left) to 1.0f (full right)</param>
        public void PlaySound(string soundName, /*float volume,*/ AudioListener listener, AudioEmitter emitter/*, float pitch, float pan*/)
        {
            if (_soundsEnable)
            {
                SoundEffect sound;

                if (!_sounds.TryGetValue(soundName, out sound))
                {
                    throw new ArgumentException(string.Format("Sound '{0}' not found", soundName));
                }

                float volume = ComputeVolume(listener, emitter);

                if (volume == 0)
                {
                    return;
                }

                //                int index = GetAvailableSoundIndex();

                //                if (index != -1)
                //                {
                if (_soundInstances.ContainsKey(soundName))
                {
                    _soundInstances[soundName].Dispose();
                }
                _soundInstances[soundName] = sound.CreateInstance();
                _soundInstances[soundName].Apply3D(listener, emitter);
                _soundInstances[soundName].Volume = volume * SoundVolume;
                _soundInstances[soundName].Pitch = 0;// pitch;
                _soundInstances[soundName].Pan = 0;// pan;
                _soundInstances[soundName].Play();

                if (!Enabled)
                {
                    _soundInstances[soundName].Pause();

                    //                        _playingSounds[index].Pause();
                }

                //                    return _playingSounds[index];
                //                }
            }
        }

        private float ComputeVolume(AudioListener listener, AudioEmitter emitter)
        {
            float positionDiff = Vector3.DistanceSquared(listener.Position, emitter.Position);

            if (positionDiff > 1024)
            {
                return 0;
            }

            return (1024 - positionDiff) / 1024;
        }

        /// <summary>
        /// Stops all currently playing sounds.
        /// </summary>
        public void StopAllSounds()
        {
            foreach (var soundEffectInstance in _soundInstances)
            {
                soundEffectInstance.Value.Stop();
                soundEffectInstance.Value.Dispose();
            }
            _soundInstances.Clear();
            //            for (int i = 0; i < _playingSounds.Length; ++i)
            //            {
            //                if (_playingSounds[i] != null)
            //                {
            //                    _playingSounds[i].Stop();
            //                    _playingSounds[i].Dispose();
            //                    _playingSounds[i] = null;
            //                }
            //            }
        }

        /// <summary>
        /// Called per loop unless Enabled is set to false.
        /// </summary>
        /// <param name="gameTime">Time elapsed since last frame</param>
        public override void Update(GameTime gameTime)
        {
            //            for (int i = 0; i < _playingSounds.Length; ++i)
            //            {
            //                if (_playingSounds[i] != null && _playingSounds[i].State == SoundState.Stopped)
            //                {
            //                    _playingSounds[i].Dispose();
            //                    _playingSounds[i] = null;
            //                }
            //            }

            if (_currentSong != null && MediaPlayer.State == MediaState.Stopped)
            {
                _currentSong = null;
                CurrentSong = null;
                _isMusicPaused = false;
            }

            base.Update(gameTime);
        }

        // Pauses all music and sound if disabled, resumes if enabled.
        protected override void OnEnabledChanged(object sender, EventArgs args)
        {
            if (Enabled)
            {
                //                for (int i = 0; i < _playingSounds.Length; ++i)
                //                {
                //                    if (_playingSounds[i] != null && _playingSounds[i].State == SoundState.Paused)
                //                    {
                //                        _playingSounds[i].Resume();
                //                    }
                //                }

                foreach (var soundEffectInstance in _soundInstances)
                {
                    if (soundEffectInstance.Value.State == SoundState.Paused)
                    {
                        soundEffectInstance.Value.Resume();
                    }
                }

                if (!_isMusicPaused)
                {
                    MediaPlayer.Resume();
                }
            }
            else
            {
                //                for (int i = 0; i < _playingSounds.Length; ++i)
                //                {
                //                    if (_playingSounds[i] != null && _playingSounds[i].State == SoundState.Playing)
                //                    {
                //                        _playingSounds[i].Pause();
                //                    }
                //                }

                foreach (var soundEffectInstance in _soundInstances)
                {
                    if (soundEffectInstance.Value.State == SoundState.Playing)
                    {
                        soundEffectInstance.Value.Pause();
                    }
                }

                MediaPlayer.Pause();
            }
            base.OnEnabledChanged(sender, args);
        }

        // Acquires an open sound slot.
        //        private int GetAvailableSoundIndex()
        //        {
        //            for (int i = 0; i < _playingSounds.Length; ++i)
        //            {
        //                if (_playingSounds[i] == null)
        //                {
        //                    return i;
        //                }
        //            }
        //            return -1;
        //        }

        //
        //        public VisualizationData GetVisualizationData()
        //        {
        //            return MediaPlayer.GetVisualizationData();
        //        }

        private void Unsubscribe()
        {
            Messenger.Off<string, AudioListener, AudioEmitter>("PlaySound", PlaySound);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Unsubscribe();
        }
    }
}