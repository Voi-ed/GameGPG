using UnityEngine;

public class ItemEffect_DataSO : ScriptableObject
{
    [TextArea]
    public string effectDescription;
    protected Player player;
    protected ItemDataSO sourceItem;
    protected ItemSetDataSO sourceSet;

    public virtual bool CanBeUsed(Player player)
    {
        return true;
    }

    public virtual void ExecuteEffect()
    {

    }

    public virtual void BindSource(Player player, ItemDataSO sourceItem = null, ItemSetDataSO sourceSet = null)
    {
        this.player = player;
        this.sourceItem = sourceItem;
        this.sourceSet = sourceSet != null ? sourceSet : (sourceItem as EquipmentDataSO)?.itemSet;
        Subscribe(player);
    }

    public virtual void Subscribe(Player player)
    {
        this.player = player;
    }

    public virtual void Unsubscribe()
    {
        this.player = null;
        this.sourceItem = null;
        this.sourceSet = null;
    }

    /// <summary>
    /// Checks if this effect belongs to an active Set with color override enabled.
    /// Returns true if a custom theme color should be applied, false if original VFX color should be kept.
    /// </summary>
    public virtual bool TryGetVfxThemeColor(out Color themeColor)
    {
        themeColor = Color.white;
        return false;
    }

    /// <summary>
    /// Spawns a VFX at its original color without Set theme tint override.
    /// </summary>
    protected GameObject InstantiateVfx(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        GameObject instance = Instantiate(prefab, position, rotation);
        return instance;
    }

    /// <summary>
    /// Preserves original VFX colors.
    /// </summary>
    protected void ApplyVfxTheme(GameObject vfxInstance)
    {
        // Set VFX color override disabled: preserve original VFX colors
    }

}
