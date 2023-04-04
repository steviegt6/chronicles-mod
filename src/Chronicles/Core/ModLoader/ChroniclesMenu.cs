using JetBrains.Annotations;
using Terraria.ModLoader;

namespace Chronicles.Core.ModLoader;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public abstract class ChroniclesMenu : ModMenu,
                                       IChroniclesType<ModMenu> { }
