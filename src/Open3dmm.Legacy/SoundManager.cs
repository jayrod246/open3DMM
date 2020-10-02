using ManagedBass;
using ManagedBass.Midi;
using Open3dmm.Core;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Open3dmm
{
    public class DummySoundManager : SoundManager
    {
        public override void PlaySound(IResolver resolver, ChunkIdentifier identifier, int track = -1, float volume = 1, int playCount = 1)
        {
        }

        public override void StopSound(int track)
        {
        }

        public override void StopAll()
        {
        }
    }

    public class BassSoundManager : SoundManager
    {
        private readonly Dictionary<int, Sound> tracks;
        private readonly List<Sound> other = new List<Sound>();

        static BassSoundManager()
        {
            if (!Bass.Init())
                throw new InvalidOperationException(Bass.LastError.ToString());
        }

        public BassSoundManager()
        {
            tracks = new Dictionary<int, Sound>();
        }

        public override void PlaySound(IResolver resolver, ChunkIdentifier identifier, int track = -1, float volume = 1f, int playCount = 1)
        {
            if (identifier.Tag == Tags.MSND)
            {
                if (resolver.TryGetIdentifier(identifier, Tags.WAVE, 0, out var item) || resolver.TryGetIdentifier(identifier, Tags.MIDS, 0, out item))
                    PlaySound(resolver, item, track, volume, playCount);
            }
            else
            {
                BassSound sound;
                if (identifier.Tag == Tags.MIDS)
                    sound = new MidiSound(resolver, identifier, volume, playCount, TimeSpan.Zero);
                else if (identifier.Tag == Tags.WAVE)
                    sound = new WaveSound(resolver, identifier, volume, playCount, TimeSpan.Zero);
                else return;

                tracks[track] = sound;
                if (track == -1)
                    other.Add(tracks[-1]);
            }
        }

        public override void StopSound(int track)
        {
            if (track == -1) return;
            if (tracks.ContainsKey(track))
            {
                tracks[track].Stop();
                tracks[track].Dispose();
            }
        }

        public override void StopAll()
        {
            foreach (var track in tracks.Keys)
                StopSound(track);
            foreach (var sound in other)
            {
                sound.Stop();
                sound.Dispose();
            }
        }

        private abstract class Sound : IDisposable
        {
            public Sound(IResolver resolver, ChunkIdentifier identifier, float volume, int playCount, TimeSpan time)
            {
                var rf = resolver.ScopeOf(identifier);
                if (rf != null)
                {
                    using var block = BinaryStream.Create(rf.File.GetChunk(identifier).Section.Memory.Span).Decompress();
                    Initialize(block);
                }
            }

            protected abstract void Initialize(IReadOnlyStream block);

            public abstract void Stop();

            public abstract void Dispose();
        }

        private abstract class BassSound : Sound
        {
            private int sync;
            protected int Channel;

            protected BassSound(IResolver resolver, ChunkIdentifier identifier, float volume, int playCount, TimeSpan time) : base(resolver, identifier, volume, playCount, time)
            {
                if (playCount == -1)
                    Bass.ChannelAddFlag(Channel, BassFlags.Loop);
                else
                {
                    if (playCount > 0)
                        sync = Bass.ChannelSetSync(Channel, SyncFlags.End, default, (a, b, c, d) =>
                        {
                            if (--playCount == 0)
                                Stop();
                            else
                                Play(volume, TimeSpan.Zero);
                        });
                    Bass.ChannelRemoveFlag(Channel, BassFlags.Loop);
                }
                Play(volume, time);
            }

            protected abstract void Play(float volume, TimeSpan time);

            public override void Stop()
            {
                if (Channel == 0) return;
                ClearSync();
                Bass.ChannelStop(Channel);
            }

            public override void Dispose()
            {
                if (Channel == 0) return;
                Bass.StreamFree(Channel);
                Channel = 0;
            }

            private void ClearSync()
            {
                if (sync == 0) return;
                Bass.ChannelRemoveSync(Channel, sync);
                sync = 0;
            }
        }

        private class WaveSound : BassSound
        {
            public MemoryHandle? MemoryHandle { get; set; }

            public WaveSound(IResolver resolver, ChunkIdentifier identifier, float volume, int playCount, TimeSpan time) : base(resolver, identifier, volume, playCount, time)
            {
            }

            protected unsafe override void Initialize(IReadOnlyStream block)
            {
                Memory<byte> memory = new byte[block.Remainder];
                MemoryHandle?.Dispose();
                MemoryHandle = memory.Pin();
                block.ReadTo(memory.Span);
                Channel = Bass.CreateStream(new IntPtr(MemoryHandle.Value.Pointer), 0, memory.Length, BassFlags.Default);
            }

            protected override void Play(float volume, TimeSpan time)
            {
                if (Channel == 0) return;
                Bass.ChannelSetPosition(Channel, Bass.ChannelSeconds2Bytes(Channel, time.TotalSeconds), PositionFlags.Bytes);
                Bass.ChannelSetAttribute(Channel, ChannelAttribute.Volume, volume);
                Bass.ChannelPlay(Channel);
            }

            public override void Dispose()
            {
                base.Dispose();
                MemoryHandle?.Dispose();
            }
        }

        private class MidiSound : BassSound
        {
            private const uint MIDI_ENDER = 0x002FFF00;
            static readonly MidiFont[] fonts = new MidiFont[1];
            public MemoryHandle? MemoryHandle { get; set; }

            public MidiSound(IResolver resolver, ChunkIdentifier identifier, float volume, int playCount, TimeSpan time) : base(resolver, identifier, volume, playCount, time)
            {
            }

            private void EnsureHasFont(int stream)
            {
                if (fonts[0].Handle == 0)
                {
                    var handle = BassMidi.FontInit("ChoriumRevA.SF2", FontInitFlags.Unicode);
                    fonts[0] = new MidiFont
                    {
                        Handle = handle,
                        Preset = -1,
                        Bank = 0
                    };
                    BassMidi.StreamSetFonts(0, fonts, 1);
                }
                BassMidi.StreamSetFonts(stream, fonts, 1);
            }

            protected override unsafe void Initialize(IReadOnlyStream block)
            {
                var len = checked((int)block.Remainder + 26);
                Memory<byte> memory = new byte[len];
                MemoryHandle?.Dispose();
                MemoryHandle = memory.Pin();
                using IStream w = BinaryStream.Create(memory);
                w.Write(0x6468544D);
                WriteBigEndian(w, 6);
                w.Write<short>(0);
                WriteBigEndian<short>(w, 1);
                WriteBigEndian<short>(w, 512);
                w.Write(0x6B72544D);
                WriteBigEndian(w, len - 22);
                int rem = checked((int)block.Remainder);
                using var mo = MemoryPool<byte>.Shared.Rent(rem);
                w.Write(block.ReadTo(mo.Memory.Span.Slice(0, rem)));
                w.Write(MIDI_ENDER);
                Channel = BassMidi.CreateStream(new IntPtr(MemoryHandle.Value.Pointer), 0, len, BassFlags.MidiNoFx);
                EnsureHasFont(Channel);

                void WriteBigEndian<T>(IStream b, T value) where T : unmanaged
                {
                    IOHelper.GetBytes(ref value).Reverse();
                    b.Write(value);
                }
            }

            protected override void Play(float volume, TimeSpan time)
            {
                if (Channel == 0) return;
                Bass.ChannelSetPosition(Channel, time.Ticks, PositionFlags.MIDITick);
                Bass.ChannelSetAttribute(Channel, ChannelAttribute.MidiTrackVolume, volume);
                Bass.ChannelPlay(Channel);
            }

            public override void Dispose()
            {
                base.Dispose();
                MemoryHandle?.Dispose();
            }
        }
    }

    public abstract class SoundManager
    {
        public float MasterVolume { get; set; }

        public abstract void PlaySound(IResolver resolver, ChunkIdentifier identifier, int track = -1, float volume = 1f, int playCount = 1);

        public abstract void StopSound(int track);

        public abstract void StopAll();
    }
}
