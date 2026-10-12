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

    # Find internalIDToNameTable entries: 213: <id> \n second: <name>
    entries = re.findall(r'213:\s*(-?\d+)\s+second:\s*([^\r\n]+)', content)
    
    # Sort entries by the trailing number in name (e.g. Sheet_0, Sheet_1)
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

    return anim_guid

print("Script template ready.")
