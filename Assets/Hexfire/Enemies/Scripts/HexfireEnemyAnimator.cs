using UnityEngine;

namespace Hexfire.Enemies
{
  /// <summary>
  /// Opcjonalny driver animacji Haon (Ghost / Mimic / Chest Mimic).
  /// Ustaw nazwy stanow z Animatora prefabu — np. Ghost_walk / Ghost_Stand / Ghost_Attack.
  /// </summary>
  [DisallowMultipleComponent]
  public class HexfireEnemyAnimator : MonoBehaviour
  {
    const int BaseLayer = 0;

    [Header("Animator (zwykle na dziecku-visual)")]
    public Animator animator;

    [Header("Stany — Ghost")]
    public string idleState = "Ghost_Stand";
    public string walkState = "Ghost_walk";
    public string attackState = "Ghost_Attack";

    [Header("Stany — Mimic (chodzacy)")]
    public bool useMimicStates;
    public string mimicIdleState = "anim_Mimic_Idle";
    public string mimicWalkState = "Anim_Mimic_Walk 0";
    public string mimicAttackState = "anim_Mimic_Attack";

    [Header("Stany — Chest Mimic (skrzynia, np. prf_Mimic-Chest6)")]
    public bool useChestMimicStates;
    public string chestIdleState = "ChestMimic_Stand";
    public string chestWalkState = "ChestMimic_Stand";
    public string chestAttackState = "Chest_Open-Epic";

    [Header("Progi")]
    public float walkSpeedThreshold = 0.15f;
    public float attackLockDuration = 0.35f;

    CharacterController controller;
    float attackTimer;

    void Awake()
    {
      controller = GetComponentInParent<CharacterController>();
      if (animator == null)
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
      if (animator == null)
        return;

      if (attackTimer > 0f)
      {
        attackTimer -= Time.deltaTime;
        return;
      }

      float speed = controller != null
        ? new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude
        : 0f;

      string idle = ResolveIdleState();
      string walk = ResolveWalkState();
      TryCrossFade(speed > walkSpeedThreshold ? walk : idle, 0.12f);
    }

    public void PlayAttack()
    {
      if (animator == null)
        return;

      if (TryCrossFade(ResolveAttackState(), 0.05f))
        attackTimer = attackLockDuration;
    }

    string ResolveIdleState()
    {
      if (useChestMimicStates)
        return chestIdleState;
      if (useMimicStates)
        return mimicIdleState;
      return idleState;
    }

    string ResolveWalkState()
    {
      if (useChestMimicStates)
        return chestWalkState;
      if (useMimicStates)
        return mimicWalkState;
      return walkState;
    }

    string ResolveAttackState()
    {
      if (useChestMimicStates)
        return chestAttackState;
      if (useMimicStates)
        return mimicAttackState;
      return attackState;
    }

    bool TryCrossFade(string stateName, float duration)
    {
      if (string.IsNullOrEmpty(stateName))
        return false;
      if (animator.runtimeAnimatorController == null)
        return false;
      if (animator.layerCount <= BaseLayer)
        return false;

      int hash = Animator.StringToHash(stateName);
      if (!animator.HasState(BaseLayer, hash))
        return false;

      animator.CrossFade(hash, duration, BaseLayer);
      return true;
    }
  }
}
