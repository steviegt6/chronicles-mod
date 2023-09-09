namespace Chronicles.Core;

/// <summary>
/// Allows NPCs to spawn in 'packs' of a number determined by <see cref="PackSize"/>
/// </summary>
internal interface IPackNPC {
    /// <summary>
    /// Determines the total number of NPCs that will spawn in a pack.
    /// </summary>
    /// <returns></returns>
    int PackSize();
}