using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace ANewWorld.Engine.Audio
{
    public sealed class SoundService
    {
        private readonly ContentManager _content;
        private readonly Dictionary<string, SoundEffect> _cache = [];
        private readonly Dictionary<string, SoundEffectInstance> _loops = [];

        public SoundService(ContentManager content)
        {
            _content = content;
        }

        public SoundEffect Get(string asset)
        {
            if (!_cache.TryGetValue(asset, out var sfx))
            {
                sfx = _content.Load<SoundEffect>(asset);
                _cache[asset] = sfx;
            }
            return sfx;
        }

        public void StartLoop(string asset, string key, float volume, float pitch, float pan)
        {
            if (_loops.TryGetValue(key, out var existing))
            {
                if (existing.State != SoundState.Playing)
                {
                    existing.Volume = volume;
                    existing.Pitch = pitch;
                    existing.Pan = pan;
                    existing.IsLooped = true;
                    existing.Play();
                }
                return;
            }
            var inst = Get(asset).CreateInstance();
            inst.IsLooped = true;
            inst.Volume = volume;
            inst.Pitch = pitch;
            inst.Pan = pan;
            inst.Play();
            _loops[key] = inst;
        }

        public void StopLoop(string key)
        {
            if (_loops.TryGetValue(key, out var inst))
            {
                inst.Stop();
                inst.Dispose();
                _loops.Remove(key);
            }
        }
    }
}
