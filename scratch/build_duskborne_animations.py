import os
import re
import uuid

def generate_guid():
    return uuid.uuid4().hex

def load_slices_from_meta(meta_path):
    with open(meta_path, 'r', encoding='utf-8') as f:
        content = f.read()

    guid_m = re.search(r'guid:\s*([a-f0-9]+)', content)
    guid = guid_m.group(1) if guid_m else None

    entries = re.findall(r'213:\s*(-?\d+)\s+second:\s*([^\r\n]+)', content)
    
    def sort_key(item):
        name = item[1]
        m = re.search(r'_(\d+)$', name)
        return int(m.group(1)) if m else 0

    sorted_entries = sorted(entries, key=sort_key)
    return guid, sorted_entries

def create_anim_file(clip_name, guid, slices, sample_rate, loop_time, events, out_anim_path):
    curve_lines = []
    mapping_lines = []

    dt = 1.0 / sample_rate
    total_frames = len(slices)
    stop_time = total_frames * dt

    for i, (internal_id, name) in enumerate(slices):
        t = i * dt
        curve_lines.append(f"    - time: {t:.8f}")
        curve_lines.append(f"      value: {{fileID: {internal_id}, guid: {guid}, type: 3}}")
        mapping_lines.append(f"    - {{fileID: {internal_id}, guid: {guid}, type: 3}}")

    events_lines = []
    if events:
        events_lines.append("  m_Events:")
        for ev_time, func_name in events:
            events_lines.append(f"  - time: {ev_time:.8f}")
            events_lines.append(f"    functionName: {func_name}")
            events_lines.append("    data: ")
            events_lines.append("    objectReferenceParameter: {fileID: 0}")
            events_lines.append("    floatParameter: 0")
            events_lines.append("    intParameter: 0")
            events_lines.append("    messageOptions: 0")
    else:
        events_lines.append("  m_Events: []")

    curve_str = "\n".join(curve_lines)
    mapping_str = "\n".join(mapping_lines)
    events_str = "\n".join(events_lines)
    loop_val = 1 if loop_time else 0

    content = f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!74 &7400000
AnimationClip:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: {clip_name}
  serializedVersion: 7
  m_Legacy: 0
  m_Compressed: 0
  m_UseHighQualityCurve: 1
  m_RotationCurves: []
  m_CompressedRotationCurves: []
  m_EulerCurves: []
  m_PositionCurves: []
  m_ScaleCurves: []
  m_FloatCurves: []
  m_PPtrCurves:
  - serializedVersion: 2
    curve:
{curve_str}
    attribute: m_Sprite
    path: 
    classID: 212
    script: {{fileID: 0}}
    flags: 2
  m_SampleRate: {sample_rate}
  m_WrapMode: 0
  m_Bounds:
    m_Center: {{x: 0, y: 0, z: 0}}
    m_Extent: {{x: 0, y: 0, z: 0}}
  m_ClipBindingConstant:
    genericBindings:
    - serializedVersion: 2
      path: 0
      attribute: 0
      script: {{fileID: 0}}
      typeID: 212
      customType: 23
      isPPtrCurve: 1
      isIntCurve: 0
      isSerializeReferenceCurve: 0
    pptrCurveMapping:
{mapping_str}
  m_AnimationClipSettings:
    serializedVersion: 2
    m_AdditiveReferencePoseClip: {{fileID: 0}}
    m_AdditiveReferencePoseTime: 0
    m_StartTime: 0
    m_StopTime: {stop_time:.7f}
    m_OrientationOffsetY: 0
    m_Level: 0
    m_CycleOffset: 0
    m_HasAdditiveReferencePose: 0
    m_LoopTime: {loop_val}
    m_LoopBlend: 0
    m_LoopBlendOrientation: 0
    m_LoopBlendPositionY: 0
    m_LoopBlendPositionXZ: 0
    m_KeepOriginalOrientation: 0
    m_KeepOriginalPositionY: 1
    m_KeepOriginalPositionXZ: 0
    m_HeightFromFeet: 0
    m_Mirror: 0
  m_EditorCurves: []
  m_EulerEditorCurves: []
  m_HasGenericRootTransform: 0
  m_HasMotionFloatCurves: 0
{events_str}
"""
    with open(out_anim_path, 'w', encoding='utf-8') as f:
        f.write(content)

    meta_path = out_anim_path + ".meta"
    anim_guid = generate_guid()
    meta_content = f"""fileFormatVersion: 2
guid: {anim_guid}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 7400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    with open(meta_path, 'w', encoding='utf-8') as f:
        f.write(meta_content)

    print(f"Generated anim: {os.path.basename(out_anim_path)} ({len(slices)} frames, guid={anim_guid})")
    return anim_guid

def generate_animator_controller(controller_name, state_dict, out_controller_path):
    """
    state_dict is a list of tuples: (state_name, anim_guid, bool_param_name, is_default, speed_param)
    """
    # Build AnimatorParameters
    # We will include standard parameters:
    # idle, run, attack, sheathe (if present), hurt, death, xVelocity, attackSpeedMultiplier, moveAnimSpeedMultiplier, battleAnimSpeedMultiplier
    param_names = ["idle", "run", "attack"]
    if any(param == "sheathe" for _, _, param, _, _ in state_dict):
        param_names.append("sheathe")
    param_names.extend(["hurt", "death", "xVelocity", "attackSpeedMultiplier", "moveAnimSpeedMultiplier", "battleAnimSpeedMultiplier"])

    params_yaml = []
    for p in param_names:
        is_float = (p in ["xVelocity", "attackSpeedMultiplier", "moveAnimSpeedMultiplier", "battleAnimSpeedMultiplier"])
        p_type = 1 if is_float else 4
        def_float = 1.0 if "Multiplier" in p else 0.0
        params_yaml.append(f"""  - m_Name: {p}
    m_Type: {p_type}
    m_DefaultFloat: {def_float}
    m_DefaultInt: 0
    m_DefaultBool: 0
    m_Controller: {{fileID: 9100000}}""")

    params_str = "\n".join(params_yaml)

    # Assign IDs
    # StateMachine ID: 110700001
    # For each state i (1..N):
    # State ID: 110200000 + i
    # Exit Transition ID: 110100000 + i
    # Entry Transition ID: 110900000 + i
    states_yaml = []
    transitions_yaml = []
    entry_transitions_yaml = []
    state_machine_child_states = []

    default_state_id = None

    for i, (s_name, a_guid, b_param, is_def, spd_param) in enumerate(state_dict, start=1):
        state_id = 110200000 + i
        exit_trans_id = 110100000 + i
        entry_trans_id = 110900000 + i

        if is_def:
            default_state_id = state_id

        # Exit transition when b_param becomes false (m_ConditionMode: 2)
        transitions_yaml.append(f"""--- !u!1101 &{exit_trans_id}
AnimatorStateTransition:
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: 
  m_Conditions:
  - m_ConditionMode: 2
    m_ConditionEvent: {b_param}
    m_EventTreshold: 0
  m_DstStateMachine: {{fileID: 0}}
  m_DstState: {{fileID: 0}}
  m_Solo: 0
  m_Mute: 0
  m_IsExit: 1
  serializedVersion: 3
  m_TransitionDuration: 0
  m_TransitionOffset: 0
  m_ExitTime: 0.75
  m_HasExitTime: 0
  m_HasFixedDuration: 1
  m_InterruptionSource: 0
  m_OrderedInterruption: 1
  m_CanTransitionToSelf: 1""")

        # Entry transition to this state when b_param is true (m_ConditionMode: 1)
        if not is_def:
            entry_transitions_yaml.append(f"""--- !u!1109 &{entry_trans_id}
AnimatorTransition:
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: 
  m_Conditions:
  - m_ConditionMode: 1
    m_ConditionEvent: {b_param}
    m_EventTreshold: 0
  m_DstStateMachine: {{fileID: 0}}
  m_DstState: {{fileID: {state_id}}}
  m_Solo: 0
  m_Mute: 0
  m_IsExit: 0
  serializedVersion: 1""")

        # State YAML
        speed_param_str = spd_param if spd_param else ""
        speed_param_active = 1 if spd_param else 0

        states_yaml.append(f"""--- !u!1102 &{state_id}
AnimatorState:
  serializedVersion: 6
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: {s_name}
  m_Speed: 1
  m_CycleOffset: 0
  m_Transitions:
  - {{fileID: {exit_trans_id}}}
  m_StateMachineBehaviours: []
  m_Position: {{x: 50, y: 50, z: 0}}
  m_IKOnFeet: 0
  m_WriteDefaultValues: 1
  m_Mirror: 0
  m_SpeedParameterActive: {speed_param_active}
  m_MirrorParameterActive: 0
  m_CycleOffsetParameterActive: 0
  m_TimeParameterActive: 0
  m_Motion: {{fileID: 7400000, guid: {a_guid}, type: 2}}
  m_Tag: 
  m_SpeedParameter: {speed_param_str}
  m_MirrorParameter: 
  m_CycleOffsetParameter: 
  m_TimeParameter: """)

        state_machine_child_states.append(f"""  - serializedVersion: 1
    m_State: {{fileID: {state_id}}}
    m_Position: {{x: {200 + (i-1)*150}, y: {100 + (i%2)*80}, z: 0}}""")

    if default_state_id is None and len(state_dict) > 0:
        default_state_id = 110200001

    child_states_str = "\n".join(state_machine_child_states)
    entry_trans_refs = "\n".join([f"  - {{fileID: {110900000 + i}}}" for i, (_, _, _, is_def, _) in enumerate(state_dict, start=1) if not is_def])

    # State Machine definition
    state_machine_yaml = f"""--- !u!1107 &110700001
AnimatorStateMachine:
  serializedVersion: 6
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: Base Layer
  m_ChildStates:
{child_states_str}
  m_ChildStateMachines: []
  m_AnyStateTransitions: []
  m_EntryTransitions:
{entry_trans_refs}
  m_StateMachineTransitions: {{}}
  m_StateMachineBehaviours: []
  m_AnyStatePosition: {{x: 50, y: 20, z: 0}}
  m_EntryPosition: {{x: 50, y: 120, z: 0}}
  m_ExitPosition: {{x: 800, y: 120, z: 0}}
  m_ParentStateMachinePosition: {{x: 800, y: 20, z: 0}}
  m_DefaultState: {{fileID: {default_state_id}}}"""

    header_yaml = f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!91 &9100000
AnimatorController:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: {controller_name}
  serializedVersion: 5
  m_AnimatorParameters:
{params_str}
  m_AnimatorLayers:
  - serializedVersion: 5
    m_Name: Base Layer
    m_StateMachine: {{fileID: 110700001}}
    m_Mask: {{fileID: 0}}
    m_Motions: []
    m_Behaviours: []
    m_BlendingMode: 0
    m_SyncedLayerIndex: -1
    m_DefaultWeight: 0
    m_IKPass: 0
    m_SyncedLayerAffectsTiming: 0
    m_Controller: {{fileID: 9100000}}"""

    full_content = "\n".join([header_yaml] + transitions_yaml + states_yaml + [state_machine_yaml] + entry_transitions_yaml) + "\n"

    with open(out_controller_path, 'w', encoding='utf-8') as f:
        f.write(full_content)

    c_meta_path = out_controller_path + ".meta"
    controller_guid = generate_guid()
    c_meta_content = f"""fileFormatVersion: 2
guid: {controller_guid}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 9100000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    with open(c_meta_path, 'w', encoding='utf-8') as f:
        f.write(c_meta_content)

    print(f"Generated Controller: {os.path.basename(out_controller_path)} (guid={controller_guid})")
    return controller_guid

print("Controller generator ready.")

def build_all():
    base_gfx = 'Assets/Graphics/EnemyToGameRpg/ModeNHanVatDo'
    base_anim = 'Assets/Animations/Characters_Animation/Enemy_DuskBorne'
    base_ctrl = 'Assets/Animations/AnimatorControllers/Characters/Enemy_DuskBorne'

    # 1. SeveredFang
    print('=== Building SeveredFang ===')
    sf_gfx = os.path.join(base_gfx, 'FallenOrder-SeveredFang/SpriteSheet')
    sf_anim_dir = os.path.join(base_anim, 'SeveredFang')
    os.makedirs(sf_anim_dir, exist_ok=True)

    g, sl = load_slices_from_meta(os.path.join(sf_gfx, 'SeveredFangIdle001-Sheet.png.meta'))
    sf_idle_guid = create_anim_file('SeveredFang_Idle', g, sl, 8, True, [], os.path.join(sf_anim_dir, 'SeveredFang_Idle.anim'))

    g, sl = load_slices_from_meta(os.path.join(sf_gfx, 'SeveredFangWalk001-Sheet.png.meta'))
    sf_run_guid = create_anim_file('SeveredFang_Run', g, sl, 10, True, [], os.path.join(sf_anim_dir, 'SeveredFang_Run.anim'))

    g, sl = load_slices_from_meta(os.path.join(sf_gfx, 'SeveredFangBasicAtk001-Sheet.png.meta'))
    sf_atk_events = [(0.35, 'SlashDamageTrigger'), (0.75, 'SlashDamageTrigger'), (1.15, 'SlashDamageTrigger'), (1.50, 'CurrentStateTrigger')]
    sf_atk_guid = create_anim_file('SeveredFang_Attack', g, sl, 16, False, sf_atk_events, os.path.join(sf_anim_dir, 'SeveredFang_Attack.anim'))

    g, sl = load_slices_from_meta(os.path.join(sf_gfx, 'SeveredFangSheathe001-Sheet.png.meta'))
    sf_sheathe_events = [(0.15, 'OpenCounterWindowTrigger'), (0.85, 'CloseCounterWindowTrigger'), (1.10, 'CurrentStateTrigger')]
    sf_sheathe_guid = create_anim_file('SeveredFang_Sheathe', g, sl, 12, False, sf_sheathe_events, os.path.join(sf_anim_dir, 'SeveredFang_Sheathe.anim'))

    g, sl = load_slices_from_meta(os.path.join(sf_gfx, 'SeveredFangHurt001-Sheet.png.meta'))
    sf_hurt_events = [(0.35, 'CurrentStateTrigger')]
    sf_hurt_guid = create_anim_file('SeveredFang_Hurt', g, sl, 10, False, sf_hurt_events, os.path.join(sf_anim_dir, 'SeveredFang_Hurt.anim'))

    g, sl = load_slices_from_meta(os.path.join(sf_gfx, 'SeveredFangDeath001-Sheet.png.meta'))
    sf_death_guid = create_anim_file('SeveredFang_Death', g, sl, 8, False, [], os.path.join(sf_anim_dir, 'SeveredFang_Death.anim'))

    sf_states = [
        ('Idle', sf_idle_guid, 'idle', True, None),
        ('Run', sf_run_guid, 'run', False, 'moveAnimSpeedMultiplier'),
        ('Attack', sf_atk_guid, 'attack', False, 'attackSpeedMultiplier'),
        ('Sheathe', sf_sheathe_guid, 'sheathe', False, None),
        ('Hurt', sf_hurt_guid, 'hurt', False, None),
        ('Death', sf_death_guid, 'death', False, None)
    ]
    generate_animator_controller('Enemy_SeveredFang', sf_states, os.path.join(base_ctrl, 'Enemy_SeveredFang.controller'))

    # 2. DemonKin
    print('=== Building DemonKin ===')
    dk_gfx = os.path.join(base_gfx, 'DuskBorne-DemonKin/SpriteSheet')
    dk_anim_dir = os.path.join(base_anim, 'DemonKin')
    os.makedirs(dk_anim_dir, exist_ok=True)

    g, sl = load_slices_from_meta(os.path.join(dk_gfx, 'DemonKinIdle001-Sheet.png.meta'))
    dk_idle_guid = create_anim_file('DemonKin_Idle', g, sl, 8, True, [], os.path.join(dk_anim_dir, 'DemonKin_Idle.anim'))

    g, sl = load_slices_from_meta(os.path.join(dk_gfx, 'DemonKinWalk001-Sheet.png.meta'))
    dk_run_guid = create_anim_file('DemonKin_Run', g, sl, 10, True, [], os.path.join(dk_anim_dir, 'DemonKin_Run.anim'))

    g, sl = load_slices_from_meta(os.path.join(dk_gfx, 'DemonKinBasicAtk001-Sheet.png.meta'))
    dk_atk_events = [(0.70, 'SmashDamageTrigger'), (1.15, 'CurrentStateTrigger')]
    dk_atk_guid = create_anim_file('DemonKin_Attack', g, sl, 10, False, dk_atk_events, os.path.join(dk_anim_dir, 'DemonKin_Attack.anim'))

    g, sl = load_slices_from_meta(os.path.join(dk_gfx, 'DemonKinHurt001-Sheet.png.meta'))
    dk_hurt_events = [(0.35, 'CurrentStateTrigger')]
    dk_hurt_guid = create_anim_file('DemonKin_Hurt', g, sl, 10, False, dk_hurt_events, os.path.join(dk_anim_dir, 'DemonKin_Hurt.anim'))

    g, sl = load_slices_from_meta(os.path.join(dk_gfx, 'DemonKinDeath001-Sheet.png.meta'))
    dk_death_guid = create_anim_file('DemonKin_Death', g, sl, 8, False, [], os.path.join(dk_anim_dir, 'DemonKin_Death.anim'))

    dk_states = [
        ('Idle', dk_idle_guid, 'idle', True, None),
        ('Run', dk_run_guid, 'run', False, 'moveAnimSpeedMultiplier'),
        ('Attack', dk_atk_guid, 'attack', False, 'attackSpeedMultiplier'),
        ('Hurt', dk_hurt_guid, 'hurt', False, None),
        ('Death', dk_death_guid, 'death', False, None)
    ]
    generate_animator_controller('Enemy_DemonKin', dk_states, os.path.join(base_ctrl, 'Enemy_DemonKin.controller'))

    # 3. Druid
    print('=== Building Druid ===')
    dr_gfx = os.path.join(base_gfx, 'DuskBorne-Druid/SpriteSheet')
    dr_anim_dir = os.path.join(base_anim, 'Druid')
    os.makedirs(dr_anim_dir, exist_ok=True)

    g, sl = load_slices_from_meta(os.path.join(dr_gfx, 'DruidIdle001-Sheet.png.meta'))
    dr_idle_guid = create_anim_file('Druid_Idle', g, sl, 8, True, [], os.path.join(dr_anim_dir, 'Druid_Idle.anim'))

    g, sl = load_slices_from_meta(os.path.join(dr_gfx, 'DruidWalk001-Sheet.png.meta'))
    dr_run_guid = create_anim_file('Druid_Run', g, sl, 10, True, [], os.path.join(dr_anim_dir, 'Druid_Run.anim'))

    g, sl = load_slices_from_meta(os.path.join(dr_gfx, 'DruidBasicAtk1-Sheet.png.meta'))
    dr_atk_events = [(0.60, 'CastSpikeTrigger'), (1.25, 'CurrentStateTrigger')]
    dr_atk_guid = create_anim_file('Druid_Attack', g, sl, 10, False, dr_atk_events, os.path.join(dr_anim_dir, 'Druid_Attack.anim'))

    g, sl = load_slices_from_meta(os.path.join(dr_gfx, 'DruidEarthVFX001-Sheet.png.meta'))
    dr_spike_guid = create_anim_file('Druid_EarthSpike', g, sl, 12, False, [], os.path.join(dr_anim_dir, 'Druid_EarthSpike.anim'))

    g, sl = load_slices_from_meta(os.path.join(dr_gfx, 'DruidHurt001-Sheet.png.meta'))
    dr_hurt_events = [(0.35, 'CurrentStateTrigger')]
    dr_hurt_guid = create_anim_file('Druid_Hurt', g, sl, 10, False, dr_hurt_events, os.path.join(dr_anim_dir, 'Druid_Hurt.anim'))

    g, sl = load_slices_from_meta(os.path.join(dr_gfx, 'DruidDeath001-Sheet.png.meta'))
    dr_death_guid = create_anim_file('Druid_Death', g, sl, 8, False, [], os.path.join(dr_anim_dir, 'Druid_Death.anim'))

    dr_states = [
        ('Idle', dr_idle_guid, 'idle', True, None),
        ('Run', dr_run_guid, 'run', False, 'moveAnimSpeedMultiplier'),
        ('Attack', dr_atk_guid, 'attack', False, 'attackSpeedMultiplier'),
        ('Hurt', dr_hurt_guid, 'hurt', False, None),
        ('Death', dr_death_guid, 'death', False, None)
    ]
    generate_animator_controller('Enemy_Druid', dr_states, os.path.join(base_ctrl, 'Enemy_Druid.controller'))

    # 4. ArchDemon
    print('=== Building ArchDemon ===')
    ad_gfx = os.path.join(base_gfx, 'DuskBorne-ArchDemon/SpriteSheets')
    ad_anim_dir = os.path.join(base_anim, 'ArchDemon')
    os.makedirs(ad_anim_dir, exist_ok=True)

    g, sl = load_slices_from_meta(os.path.join(ad_gfx, 'ArchDemonIdle001-Sheet.png.meta'))
    ad_idle_guid = create_anim_file('ArchDemon_Idle', g, sl, 8, True, [], os.path.join(ad_anim_dir, 'ArchDemon_Idle.anim'))

    g, sl = load_slices_from_meta(os.path.join(ad_gfx, 'ArchDemonBasicAtk001-Sheet.png.meta'))
    ad_atk_events = [(0.90, 'FireDarkOrbTrigger'), (1.30, 'FireDarkOrbTrigger'), (1.95, 'CurrentStateTrigger')]
    ad_atk_guid = create_anim_file('ArchDemon_Attack', g, sl, 15, False, ad_atk_events, os.path.join(ad_anim_dir, 'ArchDemon_Attack.anim'))

    g, sl = load_slices_from_meta(os.path.join(ad_gfx, 'ArchDemonHurt001-Sheet.png.meta'))
    ad_hurt_events = [(0.35, 'CurrentStateTrigger')]
    ad_hurt_guid = create_anim_file('ArchDemon_Hurt', g, sl, 10, False, ad_hurt_events, os.path.join(ad_anim_dir, 'ArchDemon_Hurt.anim'))

    g, sl = load_slices_from_meta(os.path.join(ad_gfx, 'ArchDemonDeath001-Sheet.png.meta'))
    ad_death_guid = create_anim_file('ArchDemon_Death', g, sl, 8, False, [], os.path.join(ad_anim_dir, 'ArchDemon_Death.anim'))

    ad_states = [
        ('Idle', ad_idle_guid, 'idle', True, None),
        ('Attack', ad_atk_guid, 'attack', False, 'attackSpeedMultiplier'),
        ('Hurt', ad_hurt_guid, 'hurt', False, None),
        ('Death', ad_death_guid, 'death', False, None)
    ]
    generate_animator_controller('Enemy_ArchDemon', ad_states, os.path.join(base_ctrl, 'Enemy_ArchDemon.controller'))

    print('=== ALL ANIMATIONS AND CONTROLLERS CREATED SUCCESSFULLY! ===')

build_all()
