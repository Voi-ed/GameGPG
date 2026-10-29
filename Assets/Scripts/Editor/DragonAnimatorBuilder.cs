#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[InitializeOnLoad]
public static class DragonAnimatorBuilder
{
    private const string ControllerPath = "Assets/Animations/AnimatorControllers/Characters/Dragon_Controller.controller";
    private const string AnimFolder = "Assets/Animations/Characters_Animation/Dragon";
    private const string WalkSheetPath = "Assets/Audio/VS2/Shadow_Demon_Dragon_Asset_Pack/Shadow_Demon_Dragon_Asset_Pack/Animations/Walk_Right/Walk_Right.png";
    private const string DeathSheetPath = "Assets/Audio/VS2/Shadow_Demon_Dragon_Asset_Pack/Shadow_Demon_Dragon_Asset_Pack/Animations/Death_Right/Death_Right.png";
    private const string FireballSheetPath = "Assets/Audio/VS2/Shadow_Demon_Dragon_Asset_Pack/Shadow_Demon_Dragon_Asset_Pack/Animations/Fireball_Right/Fireball_Right.png";

    private const string SourceVfxDir = "Assets/VFX_Import/Hun0FX/FX/FireFX_vol1/Prefabs";
    private const string TargetVfxDir = "Assets/Prefab/Vfx_UseItem";
    private const string PillarVfxPath = "Assets/Prefab/Vfx_UseItem/Vfx_Dragon_HellfirePillar.prefab";
    private const string FireballVfxPath = "Assets/Prefab/Vfx_UseItem/Vfx_Dragon_Fireball.prefab";
    private const string ExplosionVfxPath = "Assets/Prefab/Vfx_UseItem/Vfx_Dragon_FireExplosion.prefab";
    private const string DragonPrefabPath = "Assets/Prefab/Dragon_Form.prefab";

    private const string SessionKey = "DragonAnimatorRebuilt_v13";

    static DragonAnimatorBuilder()
    {
        EditorApplication.delayCall += EnsureDragonAnimatorBuilt;
    }

    private static void EnsureDragonAnimatorBuilt()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
            return;

        bool forceOnce = !SessionState.GetBool(SessionKey, false);

        bool hasSkillParam = controller.parameters.Any(p => p.name == "skill" && p.type == AnimatorControllerParameterType.Bool);
        bool hasFireballParam = controller.parameters.Any(p => p.name == "fireball" && p.type == AnimatorControllerParameterType.Bool);
        bool hasAttackBoolParam = controller.parameters.Any(p => p.name == "attack" && p.type == AnimatorControllerParameterType.Bool);
        AnimatorStateMachine sm = controller.layers[0].stateMachine;
        bool hasAnyStateTransitions = sm.anyStateTransitions != null && sm.anyStateTransitions.Length > 0;
        bool hasRoarState = sm.states.Any(s => s.state != null && s.state.name == "Dragon_Roar");
        bool hasFireballState = sm.states.Any(s => s.state != null && s.state.name == "Dragon_Fireball");

        if (forceOnce || !hasSkillParam || !hasFireballParam || !hasAttackBoolParam || hasAnyStateTransitions || !hasRoarState || !hasFireballState)
        {
            SessionState.SetBool(SessionKey, true);
            RebuildDragonAnimator();
        }
    }

    [MenuItem("Tools/Dragon/Rebuild Dragon Animator Controller")]
    public static void RebuildDragonAnimator()
    {
        // 0. Tạo các Prefab VFX chuẩn trong Assets/Prefab/Vfx_UseItem/ (Copy từ FireFX_vol1) & gán vào Dragon_Form.prefab
        SetupDragonVfxPrefabsAndAssign();

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            Debug.LogError("[DragonAnimatorBuilder] Không tìm thấy Dragon_Controller.controller tại: " + ControllerPath);
            return;
        }

        // 1. Load & chuẩn hóa các AnimationClip
        AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimFolder}/Dragon_Idle.anim");
        AnimationClip walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimFolder}/Dragon_Walk.anim");
        AnimationClip attackClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimFolder}/Dragon_Attack.anim");
        AnimationClip roarClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimFolder}/Dragon_Roar.anim");
        AnimationClip fireballClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimFolder}/Dragon_Fireball.anim");
        AnimationClip jumpClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimFolder}/Dragon_Jump.anim");
        AnimationClip fallClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimFolder}/Dragon_Fall.anim");
        AnimationClip deathClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimFolder}/Dragon_Death.anim");

        FixJumpAndFallClips(jumpClip, fallClip);
        FixAttackRoarAndFireballEvents(attackClip, roarClip, fireballClip);

        // 2. Xóa toàn bộ states, transitions cũ trên Base Layer
        AnimatorControllerLayer baseLayer = controller.layers[0];
        AnimatorStateMachine rootSm = baseLayer.stateMachine;

        foreach (var anyTrans in rootSm.anyStateTransitions.ToArray())
            rootSm.RemoveAnyStateTransition(anyTrans);

        foreach (var entryTrans in rootSm.entryTransitions.ToArray())
            rootSm.RemoveEntryTransition(entryTrans);

        foreach (var childState in rootSm.states.ToArray())
            rootSm.RemoveState(childState.state);

        // Dọn dẹp các sub-asset mồ côi trong file .controller
        Object[] allSubAssets = AssetDatabase.LoadAllAssetsAtPath(ControllerPath);
        foreach (Object sub in allSubAssets)
        {
            if (sub == null || sub == controller || sub == rootSm)
                continue;

            if (sub is AnimatorState || sub is AnimatorStateTransition || sub is AnimatorTransition || sub is BlendTree)
            {
                Object.DestroyImmediate(sub, true);
            }
        }

        // 3. Làm mới toàn bộ Parameters chuẩn theo kiến trúc State Machine giống Player
        foreach (var param in controller.parameters.ToArray())
        {
            controller.RemoveParameter(param);
        }

        AddBoolParam(controller, "idle", false);
        AddBoolParam(controller, "move", false);
        AddBoolParam(controller, "attack", false);
        AddBoolParam(controller, "skill", false);
        AddBoolParam(controller, "fireball", false);
        AddBoolParam(controller, "jump", false);
        AddBoolParam(controller, "dead", false);
        controller.AddParameter("yVelocity", AnimatorControllerParameterType.Float);
        AddBoolParam(controller, "isGrounded", true);
        AddIntParam(controller, "comboIndex", 1);
        AddBoolParam(controller, "isMoving", false);
        AddBoolParam(controller, "isAttacking", false);
        AddBoolParam(controller, "isCastingSkill", false);
        AddBoolParam(controller, "isDead", false);

        // 4. Bố cục vị trí Entry / AnyState / Exit giống hệt Player.controller
        rootSm.anyStatePosition = new Vector3(50f, 20f, 0f);
        rootSm.entryPosition = new Vector3(50f, 150f, 0f);
        rootSm.exitPosition = new Vector3(770f, 150f, 0f);
        rootSm.parentStateMachinePosition = new Vector3(800f, 20f, 0f);

        // 5. Tạo các State xếp thẳng hàng dọc (x = 390) nối Entry -> State -> Exit chuẩn Player
        // (1) Dragon_Idle (Default State)
        AnimatorState idleState = rootSm.AddState("Dragon_Idle", new Vector3(390f, 20f, 0f));
        idleState.motion = idleClip;
        rootSm.defaultState = idleState;
        AddExitTransition(idleState, "idle");

        // (2) Dragon_Walk (move)
        AnimatorState walkState = rootSm.AddState("Dragon_Walk", new Vector3(390f, 80f, 0f));
        walkState.motion = walkClip;
        AddEntryTransition(rootSm, walkState, "move");
        AddExitTransition(walkState, "move");

        // (3) Dragon_Attack (attack - Chuột Trái)
        AnimatorState attackState = rootSm.AddState("Dragon_Attack", new Vector3(390f, 140f, 0f));
        attackState.motion = attackClip;
        AddEntryTransition(rootSm, attackState, "attack");
        AddExitTransition(attackState, "attack");

        // (4) Dragon_Roar (skill - Chuột Phải: Long Nộ Địa Ngục)
        AnimatorState roarState = rootSm.AddState("Dragon_Roar", new Vector3(390f, 200f, 0f));
        roarState.motion = roarClip;
        AddEntryTransition(rootSm, roarState, "skill");
        AddExitTransition(roarState, "skill");

        // (5) Dragon_Fireball (fireball - Left Shift: Hạ thân tụ khí bắn Hắc Hỏa Cầu FX_Fire_03_2)
        AnimatorState fireballState = rootSm.AddState("Dragon_Fireball", new Vector3(390f, 260f, 0f));
        fireballState.motion = fireballClip;
        AddEntryTransition(rootSm, fireballState, "fireball");
        AddExitTransition(fireballState, "fireball");

        // (6) Dragon_JumpFall (jump - BlendTree kết hợp Jump & Fall theo yVelocity giống Player)
        AnimatorState jumpFallState = rootSm.AddState("Dragon_JumpFall", new Vector3(390f, 320f, 0f));
        BlendTree jumpFallTree = new BlendTree
        {
            name = "JumpFall_BlendTree",
            blendType = BlendTreeType.Simple1D,
            blendParameter = "yVelocity",
            useAutomaticThresholds = false
        };
        AssetDatabase.AddObjectToAsset(jumpFallTree, controller);
        jumpFallTree.AddChild(fallClip, -0.1f);
        jumpFallTree.AddChild(jumpClip, 0.1f);
        jumpFallState.motion = jumpFallTree;
        AddEntryTransition(rootSm, jumpFallState, "jump");
        AddExitTransition(jumpFallState, "jump");

        // (7) Dragon_Death (dead)
        AnimatorState deathState = rootSm.AddState("Dragon_Death", new Vector3(390f, 380f, 0f));
        deathState.motion = deathClip;
        AddEntryTransition(rootSm, deathState, "dead");
        AddExitTransition(deathState, "dead");

        EditorUtility.SetDirty(rootSm);
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

        Debug.Log("[DragonAnimatorBuilder] Đã dựng lại Dragon_Controller.controller (kèm Dragon_Roar & Dragon_Fireball) và chuẩn hóa VFX trong Vfx_UseItem!");
    }

    private static void SetupDragonVfxPrefabsAndAssign()
    {
        if (!AssetDatabase.IsValidFolder(TargetVfxDir))
        {
            Directory.CreateDirectory(TargetVfxDir);
            AssetDatabase.Refresh();
        }

        // 1. Copy FX_Fire_01_2 -> Vfx_Dragon_HellfirePillar.prefab
        EnsureVfxPrefabCopied($"{SourceVfxDir}/FX_Fire_01_2.prefab", PillarVfxPath, root =>
        {
            DisableParticleLooping(root);
            if (root.GetComponent<Dragon_HellfirePillar>() == null)
                root.AddComponent<Dragon_HellfirePillar>();
        });

        // 2. Copy FX_Fire_05 _2 -> Vfx_Dragon_FireExplosion.prefab
        string explosionSource = File.Exists($"{SourceVfxDir}/FX_Fire_05 _2.prefab")
            ? $"{SourceVfxDir}/FX_Fire_05 _2.prefab"
            : $"{SourceVfxDir}/FX_Fire_05.prefab";

        EnsureVfxPrefabCopied(explosionSource, ExplosionVfxPath, root =>
        {
            DisableParticleLooping(root);
        });

        // 3. Copy FX_Fire_03_2 -> Vfx_Dragon_Fireball.prefab và liên kết explosionPrefab
        EnsureVfxPrefabCopied($"{SourceVfxDir}/FX_Fire_03_2.prefab", FireballVfxPath, root =>
        {
            var proj = root.GetComponent<Dragon_FireballProjectile>();
            if (proj == null)
                proj = root.AddComponent<Dragon_FireballProjectile>();

            GameObject expAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ExplosionVfxPath);
            if (expAsset != null)
            {
                proj.ExplosionPrefab = expAsset;
                SerializedObject soProj = new SerializedObject(proj);
                SerializedProperty p = soProj.FindProperty("explosionPrefab");
                if (p != null)
                {
                    p.objectReferenceValue = expAsset;
                    soProj.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        });

        // 4. Gán Direct Serialization vào Dragon_Form.prefab
        GameObject dragonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DragonPrefabPath);
        if (dragonPrefab != null)
        {
            Dragon_Controller dc = dragonPrefab.GetComponent<Dragon_Controller>();
            if (dc != null)
            {
                SerializedObject so = new SerializedObject(dc);
                AssignPrefabRef(so, "hellfirePillarPrefab", PillarVfxPath);
                AssignPrefabRef(so, "fireballPrefab", FireballVfxPath);
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(dc);
                EditorUtility.SetDirty(dragonPrefab);
                PrefabUtility.SavePrefabAsset(dragonPrefab);
            }
        }
    }

    private static void EnsureVfxPrefabCopied(string sourcePath, string targetPath, System.Action<GameObject> configureAction)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath) == null)
            return;

        if (AssetDatabase.LoadAssetAtPath<GameObject>(targetPath) == null)
        {
            AssetDatabase.CopyAsset(sourcePath, targetPath);
            AssetDatabase.ImportAsset(targetPath);
        }

        GameObject contents = PrefabUtility.LoadPrefabContents(targetPath);
        if (contents != null)
        {
            configureAction?.Invoke(contents);
            PrefabUtility.SaveAsPrefabAsset(contents, targetPath);
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    private static void DisableParticleLooping(GameObject root)
    {
        ParticleSystem[] systems = root.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in systems)
        {
            var main = ps.main;
            main.loop = false;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            r.sortingLayerName = "Default";
            r.sortingOrder = 25;
        }
    }

    private static void AssignPrefabRef(SerializedObject so, string propName, string assetPath)
    {
        SerializedProperty prop = so.FindProperty(propName);
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prop != null && asset != null)
        {
            prop.objectReferenceValue = asset;
        }
    }

    private static void AddBoolParam(AnimatorController controller, string name, bool defaultVal)
    {
        var param = new AnimatorControllerParameter
        {
            name = name,
            type = AnimatorControllerParameterType.Bool,
            defaultBool = defaultVal
        };
        controller.AddParameter(param);
    }

    private static void AddIntParam(AnimatorController controller, string name, int defaultVal)
    {
        var param = new AnimatorControllerParameter
        {
            name = name,
            type = AnimatorControllerParameterType.Int,
            defaultInt = defaultVal
        };
        controller.AddParameter(param);
    }

    private static void AddEntryTransition(AnimatorStateMachine sm, AnimatorState targetState, string boolParam)
    {
        AnimatorTransition entryTrans = sm.AddEntryTransition(targetState);
        entryTrans.AddCondition(AnimatorConditionMode.If, 0f, boolParam);
    }

    private static void AddExitTransition(AnimatorState state, string boolParam)
    {
        AnimatorStateTransition exitTrans = state.AddExitTransition();
        exitTrans.hasExitTime = false;
        exitTrans.exitTime = 0.75f;
        exitTrans.duration = 0f;
        exitTrans.AddCondition(AnimatorConditionMode.IfNot, 0f, boolParam);
    }

    private static void FixJumpAndFallClips(AnimationClip jumpClip, AnimationClip fallClip)
    {
        Sprite[] walkSprites = AssetDatabase.LoadAllAssetsAtPath(WalkSheetPath).OfType<Sprite>().ToArray();
        if (walkSprites.Length < 4)
            return;

        EditorCurveBinding spriteBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");

        if (jumpClip != null)
        {
            var existing = AnimationUtility.GetObjectReferenceCurve(jumpClip, spriteBinding);
            if (existing == null || existing.Length == 0)
            {
                jumpClip.frameRate = 8f;
                ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[]
                {
                    new ObjectReferenceKeyframe { time = 0f, value = walkSprites[Mathf.Min(3, walkSprites.Length - 1)] },
                    new ObjectReferenceKeyframe { time = 0.125f, value = walkSprites[Mathf.Min(4, walkSprites.Length - 1)] }
                };
                AnimationUtility.SetObjectReferenceCurve(jumpClip, spriteBinding, keys);
                EditorUtility.SetDirty(jumpClip);
            }
        }

        if (fallClip != null)
        {
            var existing = AnimationUtility.GetObjectReferenceCurve(fallClip, spriteBinding);
            if (existing == null || existing.Length == 0)
            {
                fallClip.frameRate = 8f;
                ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[]
                {
                    new ObjectReferenceKeyframe { time = 0f, value = walkSprites[Mathf.Min(5, walkSprites.Length - 1)] },
                    new ObjectReferenceKeyframe { time = 0.125f, value = walkSprites[Mathf.Min(6, walkSprites.Length - 1)] }
                };
                AnimationUtility.SetObjectReferenceCurve(fallClip, spriteBinding, keys);
                EditorUtility.SetDirty(fallClip);
            }
        }
    }

    private static void FixAttackRoarAndFireballEvents(AnimationClip attackClip, AnimationClip roarClip, AnimationClip fireballClip)
    {
        if (attackClip != null)
        {
            AnimationEvent[] attackEvents = new AnimationEvent[]
            {
                new AnimationEvent
                {
                    time = 5f / 14f,
                    functionName = "AttackTrigger"
                },
                new AnimationEvent
                {
                    time = 9f / 14f,
                    functionName = "CurrentStateTrigger"
                }
            };
            AnimationUtility.SetAnimationEvents(attackClip, attackEvents);
            EditorUtility.SetDirty(attackClip);
        }

        if (roarClip != null)
        {
            AnimationEvent[] roarEvents = new AnimationEvent[]
            {
                new AnimationEvent
                {
                    time = 9f / 16f,
                    functionName = "AttackTrigger"
                },
                new AnimationEvent
                {
                    time = 21f / 16f,
                    functionName = "CurrentStateTrigger"
                }
            };
            AnimationUtility.SetAnimationEvents(roarClip, roarEvents);
            EditorUtility.SetDirty(roarClip);
        }

        if (fireballClip != null)
        {
            Sprite[] fbSprites = AssetDatabase.LoadAllAssetsAtPath(FireballSheetPath).OfType<Sprite>()
                .OrderBy(s =>
                {
                    int idx = s.name.LastIndexOf('_');
                    if (idx >= 0 && int.TryParse(s.name.Substring(idx + 1), out int n))
                        return n;
                    return 0;
                }).ToArray();

            if (fbSprites.Length >= 10)
            {
                fireballClip.frameRate = 50f;
                EditorCurveBinding spriteBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");

                (float time, int spriteIdx)[] keyframeDefs = new (float, int)[]
                {
                    (0.00f, 0), // Bắt đầu hít sâu tụ khí
                    (0.08f, 1), // Ngửa cổ lấy đà, cánh giương cao
                    (0.18f, 2), // Tụ cầu Hắc Hỏa trong vòm họng
                    (0.30f, 3), // Cực điểm nén Hắc Hỏa giữa 2 hàm răng
                    (0.42f, 4), // Nhịp gập cổ chuẩn bị phóng
                    (0.48f, 5), // AttackTrigger: Khạc tung cầu lửa + hiệu ứng nổ đầu nòng!
                    (0.58f, 6), // Dư chấn luồng lửa vươn dài
                    (0.68f, 7), // Giật lùi phản chấn & tàn lửa bay
                    (0.78f, 8), // Thu đầu & cánh về tư thế chuẩn
                    (0.88f, 9)  // CurrentStateTrigger: Chuyển mượt 100% về Idle_0
                };

                ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[keyframeDefs.Length];
                for (int i = 0; i < keyframeDefs.Length; i++)
                {
                    keys[i] = new ObjectReferenceKeyframe
                    {
                        time = keyframeDefs[i].time,
                        value = fbSprites[keyframeDefs[i].spriteIdx]
                    };
                }
                AnimationUtility.SetObjectReferenceCurve(fireballClip, spriteBinding, keys);
            }

            AnimationEvent[] fbEvents = new AnimationEvent[]
            {
                new AnimationEvent
                {
                    time = 0.48f, // Frame 5: Khạc đạn & bật giật lùi
                    functionName = "AttackTrigger"
                },
                new AnimationEvent
                {
                    time = 0.88f, // Frame 9: Hoàn tất -> về Idle
                    functionName = "CurrentStateTrigger"
                }
            };
            AnimationUtility.SetAnimationEvents(fireballClip, fbEvents);
            EditorUtility.SetDirty(fireballClip);
        }
    }
}
#endif
