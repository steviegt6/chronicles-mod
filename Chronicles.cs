using System;
using JetBrains.Annotations;
using Terraria.ModLoader;

namespace Chronicles;

/// <summary>
///     The central <see cref="Mod"/> class for Chronicles. Manages very little
///     information by itself, but is employed heavily by the tModLoader API.
/// </summary>
/// <remarks>
///     This class should be responsible for very little. Information and logic
///     should be delegated to various <see cref="ModSystem"/>s instead (or
///     elsewhere when appropriate).
/// </remarks>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public sealed class ChroniclesMod : Mod {
    private static bool instantiated;

    public ChroniclesMod() {
        // Shouldn't generally ever happen, but I'm paranoid - especially about
        // the logic behind assembly loading and unloaded. Better safe than
        // sorry, yeah?
        if (instantiated)
            throw new InvalidOperationException($"Attempted to instantiate a second instance of {nameof(ChroniclesMod)} in the same AssemblyLoadContext.");

        instantiated = true;
    }
}
