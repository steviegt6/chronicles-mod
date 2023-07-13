namespace Chronicles.Core;

internal interface IPackNPC {
    /// <summary>
    /// Determines the total number of NPCs that will spawn in a pack.
    /// </summary>
    /// <returns></returns>
    int PackSize();
}