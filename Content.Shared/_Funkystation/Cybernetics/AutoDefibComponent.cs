using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Robust.Shared.Audio;
using Content.Shared.Damage;

namespace Content.Shared._Funkystation.Cybernetics.Components;

/// <summary>
/// Enables automatic defibrillation of the entity it is attached to when it dies.
/// </summary>
[RegisterComponent]
public sealed partial class AutoDefibComponent : Component
{
    /// <summary>
    /// The amount of times the auto-defib procedure can be used.
    /// </summary>
    [DataField]
    public int Charges = 2;

    /// <summary>
    /// How much damage is healed from getting auto-defibbed.
    /// </summary>
    [DataField("autoDefibHeal", required: true), ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier AutoDefibHeal = default!;

    /// <summary>
    /// The electrical damage from getting auto-defibbed.
    /// </summary>
    [DataField("autoDefibDamage"), ViewVariables(VVAccess.ReadWrite)]
    public int AutoDefibDamage = 5;

    /// <summary>
    /// How long the entity will be electrocuted after getting auto-defibbed.
    /// </summary>
    [DataField("writheDuration"), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan WritheDuration = TimeSpan.FromSeconds(3);

    /// <summary>
    /// How long the doafter for beginning the auto-defib procedure takes.
    /// </summary>
    /// <remarks>
    /// This is synced to be as long as the audio; do not change one but not the other.
    /// </remarks>
    [DataField("doAfterDuration"), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(3);

    /// <summary>
    /// The sound when someone is auto-defibbed.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("autoDefibSound")]
    public SoundSpecifier? AutoDefibSound = new SoundPathSpecifier("/Audio/Items/Defib/defib_zap.ogg");

    /// <summary>
    /// The sound when charging the auto-defib.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("chargeSound")]
    public SoundSpecifier? ChargeSound = new SoundPathSpecifier("/Audio/Items/Defib/defib_charge.ogg");

    /// <summary>
    /// The sound when the auto-defib fails.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("failureSound")]
    public SoundSpecifier? FailureSound = new SoundPathSpecifier("/Audio/Items/Defib/defib_failed.ogg");

    /// <summary>
    /// The sound when the auto-defib succeeds.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("successSound")]
    public SoundSpecifier? SuccessSound = new SoundPathSpecifier("/Audio/Items/Defib/defib_success.ogg");
}

[Serializable, NetSerializable]
public sealed partial class AutoDefibDoAfterEvent : SimpleDoAfterEvent
{

}