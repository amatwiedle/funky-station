using Content.Shared._Funkystation.Cybernetics.Components;
using Content.Server.DoAfter;
using Content.Server.Electrocution;
using Content.Shared._Shitmed.Cybernetics;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Cybernetics;

/// <summary>
/// Handles effects caused by cybernetic implants.
/// </summary>
public partial class CyberneticEffectsSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly ElectrocutionSystem _electrocution = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AutoDefibComponent, MobStateChangedEvent>(OnDeath);
        SubscribeLocalEvent<AutoDefibComponent, AutoDefibDoAfterEvent>(OnAutoDefibDoAfter);
    }

    private void OnDeath(Entity<AutoDefibComponent> ent, ref MobStateChangedEvent args)
    {
        TryStartAutoDefibProcedure(ent);
    }

    private void OnAutoDefibDoAfter(Entity<AutoDefibComponent> ent, ref AutoDefibDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled) return;

        if (!CanAutoDefib(ent)) return;

        args.Handled = true;
        StartAutoDefibProcedure(ent);
    }

    /// <summary>
    ///     Checks if the entity can begin the auto defib procedure.
    /// </summary>
    /// <param name="ent">The Entity and AutoDefibComponent attached to it.</param>
    /// <returns>
    ///     Returns true if the entity is a valid candidate for the auto defib procedure, false otherwise.
    /// </returns>
    public bool CanAutoDefib(Entity<AutoDefibComponent> ent)
    {
        if (!TryComp<MobStateComponent>(ent, out var mobState)) return false;

        if (!_mobState.IsDead(ent, mobState)) return false; // Dont shock the living person.

        if (ent.Comp.Charges <= 0) return false; // No charges remaining, cant shock.

        if (TryComp<CyberneticsComponent>(ent, out var cybernetics)) // Check if the auto defib comes from a cybernetic part.
        {
            if (cybernetics.Disabled) return false; // The cybernetic part the auto defib is attached to is disabled.
        }

        return true;
    }

    /// <summary>
    ///     Tries to begin the auto defib procedure. On a valid entity, it starts the doAfter.
    /// </summary>
    /// <param name="ent">The Entity and AutoDefibComponent attached to it.</param>
    /// <returns>
    ///     Returns true if the auto defib procedure do-after started, otherwise false.
    /// </returns>
    public bool TryStartAutoDefibProcedure(Entity<AutoDefibComponent> ent)
    {
        if (!CanAutoDefib(ent)) return false;

        _audio.PlayPvs(ent.Comp.ChargeSound, ent);

        return _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, ent.Comp.DoAfterDuration, new AutoDefibDoAfterEvent(),
            ent)
        {
            NeedHand = false,
            BreakOnMove = false,
            BreakOnDamage = false,
            RequireCanInteract = false
        });
    }

    public bool StartAutoDefibProcedure(Entity<AutoDefibComponent> ent)
    {
        if (ent.Comp.Charges <= 0) return false;
        if (!TryComp<MobStateComponent>(ent, out var mobState)) return false;

        if (_mobState.IsAlive(ent, mobState)) return false;

        // kickstart my heart plays in the distance
        _audio.PlayPvs(ent.Comp.AutoDefibSound, ent);
        _electrocution.TryDoElectrocution(ent, null, ent.Comp.AutoDefibDamage, ent.Comp.WritheDuration, true, ignoreInsulation: true);
        _damageable.TryChangeDamage(ent, ent.Comp.AutoDefibHeal, true, origin: ent);

        if (_mobThreshold.TryGetThresholdForState(ent, MobState.Dead, out var threshold) &&
            TryComp<DamageableComponent>(ent, out var damageableComponent) &&
            damageableComponent.TotalDamage < threshold)
        {
            _mobState.ChangeMobState(ent, MobState.Critical);
            var s = new Solution("Epinephrine", FixedPoint2.New(20));
            if (!_solutionContainer.TryGetInjectableSolution(ent.Owner, out var targetSoln, out var targetSolution)) return false;
            if (!targetSolution.CanAddSolution(s)) return false;

            _solutionContainer.TryAddSolution(targetSoln.Value, s);
            ent.Comp.Charges -= 1;
        }
        return true;
    }

}
