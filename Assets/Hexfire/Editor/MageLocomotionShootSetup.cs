#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hexfire.EditorTools
{
  public static class MageLocomotionShootSetup
  {
    const string ControllerPath = "Assets/Hexfire/Player/Animations/MageLocomotion.controller";
    const string AttackClipPath = "Assets/WizardPBR/Animations/Attack01.fbx";
    const string DefendClipPath = "Assets/WizardPBR/Animations/DefendStart.fbx";

    [MenuItem("Hexfire/Setup Mage Shoot Animation (Attack01)")]
    public static void Setup()
    {
      var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
      if (controller == null)
      {
        EditorUtility.DisplayDialog("Hexfire", "Brak MageLocomotion.controller.", "OK");
        return;
      }

      AnimationClip attackClip = FindAnimationClip(AttackClipPath);
      if (attackClip == null)
      {
        EditorUtility.DisplayDialog("Hexfire", "Brak Attack01.fbx w WizardPBR.", "OK");
        return;
      }

      if (!HasParameter(controller, "Shoot"))
        controller.AddParameter("Shoot", AnimatorControllerParameterType.Trigger);

      AnimatorStateMachine root = controller.layers[0].stateMachine;
      AnimatorState attackState = FindState(root, "Attack01");
      if (attackState == null)
      {
        attackState = root.AddState("Attack01", new Vector3(300f, 180f, 0f));
        attackState.motion = attackClip;
        attackState.speed = 1.4f;
      }
      else if (attackState.motion == null)
      {
        attackState.motion = attackClip;
        attackState.speed = 1.4f;
      }

      AnimatorState idleState = FindState(root, "Idle");
      if (idleState == null)
      {
        EditorUtility.DisplayDialog("Hexfire", "Brak stanu Idle w MageLocomotion.", "OK");
        return;
      }

      if (!HasAnyStateTransition(root, attackState, "Shoot", true))
      {
        AnimatorStateTransition anyToAttack = root.AddAnyStateTransition(attackState);
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0f, "Shoot");
        anyToAttack.duration = 0.1f;
        anyToAttack.hasExitTime = false;
        anyToAttack.canTransitionToSelf = false;
      }

      AnimatorState walkState = FindState(root, "Walk");
      if (!HasExitTimeTransition(attackState, idleState))
      {
        AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.duration = 0.1f;
        attackToIdle.hasExitTime = true;
        attackToIdle.exitTime = 0.85f;
      }

      if (walkState != null && !HasSpeedTransition(attackState, walkState))
      {
        AnimatorStateTransition attackToWalk = attackState.AddTransition(walkState);
        attackToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        attackToWalk.duration = 0.1f;
        attackToWalk.hasExitTime = true;
        attackToWalk.exitTime = 0.85f;
      }

      EditorUtility.SetDirty(controller);
      AssetDatabase.SaveAssets();

      EditorUtility.DisplayDialog(
        "Hexfire",
        "Dodano parametr Shoot + stan Attack01.\n" +
        "Dodaj PlayerShootAnimator na Player_Mage (menu Setup Player Mage).",
        "OK");
    }

    [MenuItem("Hexfire/Setup Mage Shield Animation (DefendStart)")]
    public static void SetupShield()
    {
      var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
      if (controller == null)
      {
        EditorUtility.DisplayDialog("Hexfire", "Brak MageLocomotion.controller.", "OK");
        return;
      }

      AnimationClip defendClip = FindAnimationClip(DefendClipPath);
      if (defendClip == null)
      {
        EditorUtility.DisplayDialog("Hexfire", "Brak DefendStart.fbx w WizardPBR.", "OK");
        return;
      }

      if (!HasParameter(controller, "Shield"))
        controller.AddParameter("Shield", AnimatorControllerParameterType.Trigger);

      AnimatorStateMachine root = controller.layers[0].stateMachine;
      AnimatorState defendState = FindState(root, "DefendStart");
      if (defendState == null)
      {
        defendState = root.AddState("DefendStart", new Vector3(520f, 180f, 0f));
        defendState.motion = defendClip;
      }
      else if (defendState.motion == null)
      {
        defendState.motion = defendClip;
      }

      AnimatorState idleState = FindState(root, "Idle");
      if (idleState == null)
      {
        EditorUtility.DisplayDialog("Hexfire", "Brak stanu Idle w MageLocomotion.", "OK");
        return;
      }

      if (!HasAnyStateTransition(root, defendState, "Shield", true))
      {
        AnimatorStateTransition anyToDefend = root.AddAnyStateTransition(defendState);
        anyToDefend.AddCondition(AnimatorConditionMode.If, 0f, "Shield");
        anyToDefend.duration = 0.1f;
        anyToDefend.hasExitTime = false;
        anyToDefend.canTransitionToSelf = false;
      }

      if (!HasExitTimeTransition(defendState, idleState))
      {
        AnimatorStateTransition defendToIdle = defendState.AddTransition(idleState);
        defendToIdle.duration = 0.1f;
        defendToIdle.hasExitTime = true;
        defendToIdle.exitTime = 0.9f;
      }

      EditorUtility.SetDirty(controller);
      AssetDatabase.SaveAssets();

      EditorUtility.DisplayDialog(
        "Hexfire",
        "Dodano parametr Shield + stan DefendStart (animacja tarczy miecza).",
        "OK");
    }

    static AnimationClip FindAnimationClip(string assetPath)
    {
      Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
      foreach (Object asset in assets)
      {
        if (asset is AnimationClip clip && !clip.name.StartsWith("__"))
          return clip;
      }

      return null;
    }

    static bool HasParameter(AnimatorController controller, string name)
    {
      foreach (AnimatorControllerParameter parameter in controller.parameters)
      {
        if (parameter.name == name)
          return true;
      }

      return false;
    }

    static AnimatorState FindState(AnimatorStateMachine machine, string stateName)
    {
      foreach (ChildAnimatorState child in machine.states)
      {
        if (child.state.name == stateName)
          return child.state;
      }

      return null;
    }

    static bool HasAnyStateTransition(
      AnimatorStateMachine machine,
      AnimatorState destination,
      string parameter,
      bool whenTrue)
    {
      foreach (AnimatorStateTransition transition in machine.anyStateTransitions)
      {
        if (transition.destinationState != destination)
          continue;

        foreach (AnimatorCondition condition in transition.conditions)
        {
          if (condition.parameter == parameter &&
              condition.mode == (whenTrue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot))
            return true;
        }
      }

      return false;
    }

    static bool HasExitTimeTransition(AnimatorState source, AnimatorState destination)
    {
      foreach (AnimatorStateTransition transition in source.transitions)
      {
        if (transition.destinationState == destination && transition.hasExitTime)
          return true;
      }

      return false;
    }

    static bool HasSpeedTransition(AnimatorState source, AnimatorState destination)
    {
      foreach (AnimatorStateTransition transition in source.transitions)
      {
        if (transition.destinationState != destination)
          continue;

        foreach (AnimatorCondition condition in transition.conditions)
        {
          if (condition.parameter == "Speed" &&
              condition.mode == AnimatorConditionMode.Greater)
            return true;
        }
      }

      return false;
    }

    static bool HasTransition(
      AnimatorState source,
      AnimatorState destination,
      string parameter,
      bool whenTrue)
    {
      foreach (AnimatorStateTransition transition in source.transitions)
      {
        if (transition.destinationState != destination)
          continue;

        foreach (AnimatorCondition condition in transition.conditions)
        {
          if (condition.parameter == parameter &&
              condition.mode == (whenTrue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot))
            return true;
        }
      }

      return false;
    }
  }
}
#endif
