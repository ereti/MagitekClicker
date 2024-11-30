using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using System.Threading.Tasks;
using NAudio.Wave.SampleProviders;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Dalamud.Plugin;
using MagitekClicker.Classes;

public class SoundPlayer : IDisposable
{
    public static SoundPlayer Instance { get; } = new();

    private readonly IWavePlayer wavOut;
    private readonly MixingSampleProvider mixer;
    private readonly VolumeSampleProvider sampleProvider;

    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    public SoundPlayer()
    {
        wavOut = new WaveOutEvent();
        mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
        mixer.ReadFully = true;
        sampleProvider = new VolumeSampleProvider(mixer);

        mixer.MixerInputEnded += OnMixerInputEnded;

        wavOut.Init(sampleProvider);
        wavOut.Play();
    }

    public void SetVolume(float volume)
    {
        sampleProvider.Volume = volume;
    }

    public void PlaySound(string path)
    {
        try
        {
            using AudioFileReader sound = new(path);
            mixer.AddMixerInput((ISampleProvider) sound);
        }
        catch (Exception e)
        {
            Log.Error(e, "failed to play audio " + path);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private void OnMixerInputEnded(object? sender, SampleProviderEventArgs e)
    {

    }
}
