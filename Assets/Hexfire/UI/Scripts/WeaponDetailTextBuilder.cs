using System.Text;
using Hexfire.Weapons;

namespace Hexfire.UI
{
  public static class WeaponDetailTextBuilder
  {
    public static string Build(WeaponData weapon)
    {
      if (weapon == null)
        return "Brak aktywnej broni.";

      var text = new StringBuilder();
      text.AppendLine($"<b>{weapon.weaponName}</b>");

      if (!string.IsNullOrWhiteSpace(weapon.description))
        text.AppendLine(weapon.description.Trim());

      text.AppendLine();

      if (weapon is GreenFireOrbWeaponData greenOrb)
      {
        AppendLmbHeader(text);
        text.AppendLine($"  Koszt: 0 MP");
        text.AppendLine($"  Obrażenia: {greenOrb.damage:0}");
        text.AppendLine($"  Prędkość: {greenOrb.projectileSpeed:0.##}");
        text.AppendLine();
        text.AppendLine("<b>PPM — Umiejętność: Leczenie</b>");
        text.AppendLine($"  Koszt: {greenOrb.HealManaCost:0} MP");
        text.AppendLine($"  Leczy: +{greenOrb.HealPerCast} HP");
        text.AppendLine($"  Cooldown: {greenOrb.healRate:0.##} s");
        return text.ToString().TrimEnd();
      }

      if (weapon is MeleeSwordWeaponData sword)
      {
        text.AppendLine("<b>LPM — Cios mieczem</b>");
        text.AppendLine("  Koszt: 0 MP");
        text.AppendLine($"  Obrażenia: {sword.damage:0}");
        text.AppendLine($"  Zasięg: {sword.meleeRange:0.##} m");
        text.AppendLine($"  Cooldown ciosu: {sword.swingInterval:0.##} s");
        text.AppendLine();
        text.AppendLine("<b>PPM — Magic Shield</b>");
        text.AppendLine($"  Koszt: {sword.shieldManaCost:0} MP");
        text.AppendLine($"  Iframe: {sword.shieldDuration:0.##} s");
        text.AppendLine($"  Cooldown: {sword.shieldCooldown:0.##} s");
        return text.ToString().TrimEnd();
      }

      if (weapon is StaffWeaponData staff)
      {
        AppendLmbHeader(text);
        text.AppendLine($"  Koszt: {FormatManaCost(staff)} (za cały strzał)");
        text.AppendLine($"  Obrażenia: {staff.damage:0}");
        text.AppendLine($"  Prędkość: {staff.projectileSpeed:0.##}");
        text.AppendLine($"  Tryb: {DescribeFirePattern(staff.firePattern)}");

        if (staff.firePattern == WeaponFirePattern.Spread && staff.spreadAngles != null)
          text.AppendLine($"  Kąty: {string.Join(", ", staff.spreadAngles)}°");

        if (staff.firePattern == WeaponFirePattern.Chaos)
        {
          text.AppendLine($"  Pociski w serii: {staff.chaosProjectileCount}");
          text.AppendLine($"  Kąt losowy: {staff.chaosAngleMin:0}° … {staff.chaosAngleMax:0}°");
        }

        text.AppendLine($"  Cooldown: {staff.FireInterval:0.##} s");

        if (staff.abilityType != StaffAbilityType.None)
        {
          text.AppendLine();
          text.AppendLine($"<b>PPM — Umiejętność: {DescribeStaffAbility(staff.abilityType)}</b>");
          text.AppendLine($"  Koszt: {staff.abilityManaCost:0} MP");
          text.AppendLine($"  Cooldown: {staff.abilityCooldown:0.##} s");

          if (staff.abilityType == StaffAbilityType.ManaRestore)
            text.AppendLine($"  Przywraca: +{staff.abilityManaRestore:0} MP");
          else if (staff.abilityType == StaffAbilityType.ShotgunChaos)
          {
            float abilitySpeed = staff.abilityProjectileSpeed > 0f
              ? staff.abilityProjectileSpeed
              : staff.projectileSpeed;
            text.AppendLine($"  Pociski: {staff.abilityShotgunCount}");
            text.AppendLine($"  Prędkość PPM: {abilitySpeed:0.##}");
          }
          else if (staff.abilityType == StaffAbilityType.RingNova)
          {
            float abilitySpeed = staff.abilityProjectileSpeed > 0f
              ? staff.abilityProjectileSpeed
              : staff.projectileSpeed;
            text.AppendLine($"  Pociski: {staff.abilityRingBulletCount} (360°)");
            text.AppendLine($"  Prędkość PPM: {abilitySpeed:0.##}");
          }
        }

        return text.ToString().TrimEnd();
      }

      if (weapon is ProjectileWeaponData projectile)
      {
        AppendLmbHeader(text);
        text.AppendLine($"  Koszt: {FormatManaCost(projectile)}");
        text.AppendLine($"  Obrażenia: {projectile.damage:0}");
        text.AppendLine($"  Prędkość: {projectile.projectileSpeed:0.##}");
        text.AppendLine($"  Tryb: {DescribeFirePattern(projectile.firePattern)}");
        text.AppendLine($"  Cooldown: {projectile.FireInterval:0.##} s");
        return text.ToString().TrimEnd();
      }

      text.AppendLine("<b>LPM — Atak</b>");
      text.AppendLine($"  Koszt: {FormatManaCost(weapon)}");
      text.AppendLine($"  Obrażenia: {weapon.damage:0}");
      return text.ToString().TrimEnd();
    }

    static void AppendLmbHeader(StringBuilder text) => text.AppendLine("<b>LPM — Strzał</b>");

    static string FormatManaCost(WeaponData weapon)
    {
      if (weapon == null || !weapon.usesMana || weapon.manaCost <= 0f)
        return "0 MP";

      return $"{weapon.manaCost:0} MP";
    }

    static string DescribeStaffAbility(StaffAbilityType ability)
    {
      return ability switch
      {
        StaffAbilityType.ManaRestore => "Odnowa many",
        StaffAbilityType.ShotgunChaos => "Shotgun chaos",
        StaffAbilityType.RingNova => "Nova (pierścień)",
        _ => "—"
      };
    }

    static string DescribeFirePattern(WeaponFirePattern pattern)
    {
      return pattern switch
      {
        WeaponFirePattern.Spread => "Rozproszenie (3 kule naraz)",
        WeaponFirePattern.Chaos => "Chaos (seria losowych)",
        _ => "Pojedynczy"
      };
    }
  }
}
