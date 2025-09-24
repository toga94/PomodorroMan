using System;
using System.Collections.Generic;
using System.Media;
using System.IO;

namespace PomodorroMan
{
    public class AmbientSound
    {
        public string Name { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public SoundCategory Category { get; set; }
        public bool IsLooping { get; set; }
        public float Volume { get; set; } = 0.5f;
    }

    public enum SoundCategory
    {
        Nature,
        WhiteNoise,
        Ambient,
        Focus,
        Relaxation
    }

    public class AmbientSoundManager
    {
        private readonly Dictionary<string, AmbientSound> _sounds = new();
        private SoundPlayer? _currentPlayer;
        private bool _isPlaying = false;
        private AmbientSound? _currentSound;

        public event EventHandler<SoundEventArgs>? SoundStarted;
        public event EventHandler<SoundEventArgs>? SoundStopped;

        public AmbientSoundManager()
        {
            InitializeDefaultSounds();
        }

        private void InitializeDefaultSounds()
        {
            // Add default ambient sounds (these would be actual sound files in a real implementation)
            AddSound(new AmbientSound
            {
                Name = "Rain",
                FilePath = "Assets/Sounds/rain.wav",
                Category = SoundCategory.Nature,
                IsLooping = true,
                Volume = 0.6f
            });

            AddSound(new AmbientSound
            {
                Name = "Forest",
                FilePath = "Assets/Sounds/forest.wav",
                Category = SoundCategory.Nature,
                IsLooping = true,
                Volume = 0.5f
            });

            AddSound(new AmbientSound
            {
                Name = "White Noise",
                FilePath = "Assets/Sounds/whitenoise.wav",
                Category = SoundCategory.WhiteNoise,
                IsLooping = true,
                Volume = 0.4f
            });

            AddSound(new AmbientSound
            {
                Name = "Ocean Waves",
                FilePath = "Assets/Sounds/ocean.wav",
                Category = SoundCategory.Nature,
                IsLooping = true,
                Volume = 0.5f
            });

            AddSound(new AmbientSound
            {
                Name = "Cafe Ambience",
                FilePath = "Assets/Sounds/cafe.wav",
                Category = SoundCategory.Ambient,
                IsLooping = true,
                Volume = 0.3f
            });

            AddSound(new AmbientSound
            {
                Name = "Focus Music",
                FilePath = "Assets/Sounds/focus.wav",
                Category = SoundCategory.Focus,
                IsLooping = true,
                Volume = 0.4f
            });
        }

        public void AddSound(AmbientSound sound)
        {
            _sounds[sound.Name] = sound;
        }

        public void PlaySound(string soundName)
        {
            if (_sounds.TryGetValue(soundName, out var sound))
            {
                StopCurrentSound();
                _currentSound = sound;
                _isPlaying = true;
                
                try
                {
                    if (File.Exists(sound.FilePath))
                    {
                        _currentPlayer = new SoundPlayer(sound.FilePath);
                        _currentPlayer.PlayLooping();
                        SoundStarted?.Invoke(this, new SoundEventArgs(sound));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error playing sound {soundName}: {ex.Message}");
                }
            }
        }

        public void StopCurrentSound()
        {
            if (_currentPlayer != null)
            {
                _currentPlayer.Stop();
                _currentPlayer.Dispose();
                _currentPlayer = null;
            }

            if (_currentSound != null)
            {
                var stoppedSound = _currentSound;
                _currentSound = null;
                _isPlaying = false;
                SoundStopped?.Invoke(this, new SoundEventArgs(stoppedSound));
            }
        }

        public void SetVolume(float volume)
        {
            if (_currentPlayer != null && _currentSound != null)
            {
                _currentSound.Volume = Math.Max(0f, Math.Min(1f, volume));
                // Note: SoundPlayer doesn't support volume control directly
                // In a real implementation, you'd use a more advanced audio library
            }
        }

        public List<AmbientSound> GetSoundsByCategory(SoundCategory category)
        {
            var sounds = new List<AmbientSound>();
            foreach (var sound in _sounds.Values)
            {
                if (sound.Category == category)
                {
                    sounds.Add(sound);
                }
            }
            return sounds;
        }

        public List<AmbientSound> GetAllSounds()
        {
            return new List<AmbientSound>(_sounds.Values);
        }

        public AmbientSound? GetCurrentSound()
        {
            return _currentSound;
        }

        public bool IsPlaying => _isPlaying;

        public void Dispose()
        {
            StopCurrentSound();
        }
    }

    public class SoundEventArgs : EventArgs
    {
        public AmbientSound Sound { get; }

        public SoundEventArgs(AmbientSound sound)
        {
            Sound = sound;
        }
    }
}
