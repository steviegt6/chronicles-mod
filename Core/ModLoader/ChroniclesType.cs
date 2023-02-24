using System.Collections.Generic;

namespace Chronicles.Core.ModLoader; 

/// <summary>
///     Defines base members that all Chronicles abstractions over existing
///     ModTypes should have.
/// </summary>
/// <typeparam name="TModType">The original ModType being abstracted.</typeparam>
public interface IChroniclesType<in TModType> {
}
