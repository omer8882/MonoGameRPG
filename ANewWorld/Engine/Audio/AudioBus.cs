using System.Collections.Generic;

namespace ANewWorld.Engine.Audio
{
    public sealed class AudioBus
    {
        public readonly List<PlaySfx> Sfx = new(64);
        public readonly List<PlaySfxAt> SfxAt = new(16);
        public readonly List<StartLoop> LoopStarts = new(8);
        public readonly List<StopLoop> LoopStops = new(8);

        public void Publish(PlaySfx evt) => Sfx.Add(evt);
        public void Publish(PlaySfxAt evt) => SfxAt.Add(evt);
        public void Publish(StartLoop evt) => LoopStarts.Add(evt);
        public void Publish(StopLoop evt) => LoopStops.Add(evt);

        public void Clear()
        {
            Sfx.Clear();
            SfxAt.Clear();
            LoopStarts.Clear();
            LoopStops.Clear();
        }
    }
}
