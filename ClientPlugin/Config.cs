using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using ClientPlugin.Settings.Elements;
using ClientPlugin.Settings.Tools;

namespace ClientPlugin;

public class Config : INotifyPropertyChanged
{
    #region Options

    private bool enabled = true;
    private float gravityMultiplier = 1f;

    #endregion

    #region User interface

    [XmlIgnore]
    public readonly string Title = "True Gravity";

    [Separator("True Gravity")]

    [Checkbox(description: "Turn the plugin on or off. When off, the game's own gravity multiplier (2) is used. Takes effect the next time a world is loaded.")]
    public bool Enabled
    {
        get => enabled;
        set => SetField(ref enabled, value);
    }

    [Slider(0.1f, 10f, 0.05f, SliderAttribute.SliderType.Float,
        description: "Gravity multiplier for ships, floating objects and debris (not characters). 1 = gravity matches what the HUD and gravity generators show. The game's default is 2. Takes effect the next time a world is loaded.")]
    public float GravityMultiplier
    {
        get => gravityMultiplier;
        // Also applied when Config.xml is loaded, so a hand-edited file cannot feed 0, negative or NaN into the physics.
        set => SetField(ref gravityMultiplier, float.IsFinite(value) ? Math.Clamp(value, 0.1f, 10f) : 1f);
    }

    #endregion

    #region Property change notification boilerplate

    public static readonly Config Default = new Config();
    public static Config Current = ConfigStorage.Load();

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}
