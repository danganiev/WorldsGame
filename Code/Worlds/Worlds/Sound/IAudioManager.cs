using Microsoft.Xna.Framework.Audio;

namespace WorldsGame.Sound
{
    public interface IAudioManager
    {
        /// <summary>
        /// Gets or sets the volume to play songs. 1.0f is max volume.
        /// </summary>
        float MusicVolume { get; set; }

        /// <summary>
        /// Gets or sets the master volume for all sounds. 1.0f is max volume.
        /// </summary>
        float SoundVolume { get; set; }

        /// <summary>
        /// Gets whether a song is playing or paused (i.e. not stopped).
        /// </summary>
        bool IsSongActive { get; }

        /// <summary>
        /// Gets whether the current song is paused.
        /// </summary>
        bool IsSongPaused { get; }

        bool SongsEnable { get; set; }

        bool SoundsEnable { get; set; }

        bool Enabled { get; set; }

        /// <summary>
        /// Loads the ContentManager in AudioManager. Should not be called more thatn once.
        /// </summary>
        //        void LoadContent();

        /// <summary>
        /// Loads a Song into the AudioManager.
        /// </summary>
        /// <param name="songName">Name of the song to load</param>
        /// <param name="songPath">Path to the song asset file</param>
        void LoadSong(string songName, string songPath);

        /// <summary>
        /// Loads a SoundEffect into the AudioManager.
        /// </summary>
        /// <param name="soundName">Name of the sound to load</param>
        /// <param name="soundPath">Path to the song asset file</param>
        void LoadSound(string soundName, string soundPath);

        /// <summary>
        /// Unloads all loaded songs and sounds.
        /// </summary>
        void UnloadContent();

        /// <summary>
        /// Starts playing the song with the given name. If it is already playing, this method
        /// does nothing. If another song is currently playing, it is stopped first.
        /// </summary>
        /// <param name="songName">Name of the song to play</param>
        /// <param name="loop">True if song should loop, false otherwise</param>
        void PlaySong(string songName, bool loop);

        /// <summary>
        /// Pauses the currently playing song. This is a no-op if the song is already paused,
        /// or if no song is currently playing.
        /// </summary>
        void PauseSong();

        /// <summary>
        /// Resumes the currently paused song. This is a no-op if the song is not paused,
        /// or if no song is currently playing.
        /// </summary>
        void ResumeSong();

        /// <summary>
        /// Stops the currently playing song. This is a no-op if no song is currently playing.
        /// </summary>
        void StopSong();

        /// <summary>
        /// Plays the sound of the given name with the given parameters.
        /// </summary>
        /// <param name="soundName">Name of the sound</param>
        /// <param name="volume">Volume, 0.0f to 1.0f</param>
        /// <param name="pitch">Pitch, -1.0f (down one octave) to 1.0f (up one octave)</param>
        /// <param name="pan">Pan, -1.0f (full left) to 1.0f (full right)</param>
        /// <return>Instance of the current sound playing</return>
        //        SoundEffectInstance PlaySound(string soundName/*, float volume, float pitch, float pan*/);
        void PlaySound(string soundName, /*float volume,*/ AudioListener listener, AudioEmitter emitter
            /*, float pitch, float pan*/);

        /// <summary>
        /// Stops all currently playing sounds.
        /// </summary>
        void StopAllSounds();
    }
}