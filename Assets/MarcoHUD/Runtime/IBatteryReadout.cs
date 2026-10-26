namespace LoQueCallaste.Hud
{
    /// <summary>Optional contract for the flashlight from issue #9. Values are normalized 0..1.</summary>
    public interface IBatteryReadout
    {
        float Charge01 { get; }
    }
}
