using DefaultEcs;
using DefaultEcs.System;
using ANewWorld.Engine.Audio;
using Microsoft.Xna.Framework;

namespace ANewWorld.Engine.Systems
{
    public sealed class AudioSystem : ISystem<float>, System.IDisposable
    {
        private readonly World _world;
        private readonly SoundService _sounds;
        private readonly AudioBus _bus;
        private readonly EntitySet _sfxSet;
        private readonly EntitySet _sfxAtSet;
        private readonly EntitySet _startLoopSet;
        private readonly EntitySet _stopLoopSet;

        public bool IsEnabled { get; set; } = true;

        public AudioSystem(World world, SoundService sounds, AudioBus bus)
        {
            _world = world;
            _sounds = sounds;
            _bus = bus;
            _sfxSet = world.GetEntities().With<PlaySfx>().AsSet();
            _sfxAtSet = world.GetEntities().With<PlaySfxAt>().AsSet();
            _startLoopSet = world.GetEntities().With<StartLoop>().AsSet();
            _stopLoopSet = world.GetEntities().With<StopLoop>().AsSet();
        }

        public void Update(float dt)
        {
            if (!IsEnabled) return;

            // Drain bus first
            for (int i = 0; i < _bus.Sfx.Count; i++)
            {
                var evt = _bus.Sfx[i];
                _sounds.Get(evt.Asset).Play(evt.Volume <= 0 ? 1f : evt.Volume, evt.Pitch, evt.Pan);
            }
            for (int i = 0; i < _bus.SfxAt.Count; i++)
            {
                var evt = _bus.SfxAt[i];
                _sounds.Get(evt.Asset).Play(evt.Volume <= 0 ? 1f : evt.Volume, evt.Pitch, 0f);
            }
            for (int i = 0; i < _bus.LoopStarts.Count; i++)
            {
                var evt = _bus.LoopStarts[i];
                var key = string.IsNullOrEmpty(evt.Key) ? evt.Asset : evt.Key!;
                _sounds.StartLoop(evt.Asset, key, evt.Volume <= 0 ? 1f : evt.Volume, evt.Pitch, evt.Pan);
            }
            for (int i = 0; i < _bus.LoopStops.Count; i++)
            {
                var evt = _bus.LoopStops[i];
                var key = string.IsNullOrEmpty(evt.Key) ? evt.Asset : evt.Key!;
                _sounds.StopLoop(key);
            }
            _bus.Clear();

            // Entity-based one-shots
            foreach (ref readonly var e in _sfxSet.GetEntities())
            {
                var evt = e.Get<PlaySfx>();
                _sounds.Get(evt.Asset).Play(evt.Volume <= 0 ? 1f : evt.Volume, evt.Pitch, evt.Pan);
                e.Dispose();
            }
            foreach (ref readonly var e in _sfxAtSet.GetEntities())
            {
                var evt = e.Get<PlaySfxAt>();
                _sounds.Get(evt.Asset).Play(evt.Volume <= 0 ? 1f : evt.Volume, evt.Pitch, 0f);
                e.Dispose();
            }

            // Loop control events
            foreach (ref readonly var e in _startLoopSet.GetEntities())
            {
                var evt = e.Get<StartLoop>();
                var key = string.IsNullOrEmpty(evt.Key) ? evt.Asset : evt.Key!;
                _sounds.StartLoop(evt.Asset, key, evt.Volume <= 0 ? 1f : evt.Volume, evt.Pitch, evt.Pan);
                e.Dispose();
            }
            foreach (ref readonly var e in _stopLoopSet.GetEntities())
            {
                var evt = e.Get<StopLoop>();
                var key = string.IsNullOrEmpty(evt.Key) ? evt.Asset : evt.Key!;
                _sounds.StopLoop(key);
                e.Dispose();
            }
        }

        public void Dispose()
        {
            _sfxSet?.Dispose();
            _sfxAtSet?.Dispose();
            _startLoopSet?.Dispose();
            _stopLoopSet?.Dispose();
        }
    }
}
