import re

# 1. Clean meta internalIDToNameTable in ArchDemonBasicAtk001-Sheet.png.meta
meta_path = "Assets/Graphics/EnemyToGameRpg/ModeNHanVatDo/DuskBorne-ArchDemon/SpriteSheets/ArchDemonBasicAtk001-Sheet.png.meta"
with open(meta_path, "r", encoding="utf-8") as f:
    meta_content = f.read()

# Sprite IDs in order 0 to 14
sprites_data = [
    ("-3970589427392028295", "ArchDemonBasicAtk001-Sheet_0"),
    ("1188886742568398702", "ArchDemonBasicAtk001-Sheet_1"),
    ("482797791953838701", "ArchDemonBasicAtk001-Sheet_2"),
    ("-5744554377020702508", "ArchDemonBasicAtk001-Sheet_3"),
    ("-6006621870850466", "ArchDemonBasicAtk001-Sheet_4"),
    ("-7544396562254309787", "ArchDemonBasicAtk001-Sheet_5"),
    ("8528477237256036597", "ArchDemonBasicAtk001-Sheet_6"),
    ("-8376795314478499838", "ArchDemonBasicAtk001-Sheet_7"),
    ("-811898465591105187", "ArchDemonBasicAtk001-Sheet_8"),
    ("-6361741082343469371", "ArchDemonBasicAtk001-Sheet_9"),
    ("-5739081026707015955", "ArchDemonBasicAtk001-Sheet_10"),
    ("4231244313269118369", "ArchDemonBasicAtk001-Sheet_11"),
    ("4344151244158979492", "ArchDemonBasicAtk001-Sheet_12"),
    ("7483552688837789658", "ArchDemonBasicAtk001-Sheet_13"),
    ("-2630097841353598230", "ArchDemonBasicAtk001-Sheet_14")
]

new_table = "  internalIDToNameTable:\n"
for fid, name in sprites_data:
    new_table += f"  - first:\n      213: {fid}\n    second: {name}\n"

meta_content = re.sub(
    r"  internalIDToNameTable:\n(?:  - first:\n      213: [^\n]+\n    second: [^\n]+\n)+",
    new_table,
    meta_content
)

with open(meta_path, "w", encoding="utf-8") as f:
    f.write(meta_content)
print("Updated ArchDemonBasicAtk001-Sheet.png.meta internalIDToNameTable to 15 frames!")

# 2. Rebuild ArchDemon_Attack.anim
anim_path = "Assets/Animations/Characters_Animation/Enemy_DuskBorne/ArchDemon/ArchDemon_Attack.anim"
guid = "9dda2a4994da240f98b180c46ccdc0bb"

curve_lines = []
pptr_lines = []

sample_rate = 12
for i, (fid, _) in enumerate(sprites_data):
    t = i / sample_rate
    t_str = f"{t:.8f}"
    curve_lines.append(f"    - time: {t_str}\n      value: {{fileID: {fid}, guid: {guid}, type: 3}}")
    pptr_lines.append(f"    - {{fileID: {fid}, guid: {guid}, type: 3}}")

curve_block = "\n".join(curve_lines)
pptr_block = "\n".join(pptr_lines)

stop_time = 15 / sample_rate # 1.25s

anim_content = f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!74 &7400000
AnimationClip:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: ArchDemon_Attack
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
{curve_block}
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
{pptr_block}
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
    m_LoopTime: 0
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
  m_Events:
  - time: 0.58333333
    functionName: FireDarkOrbTrigger
    data: 
    objectReferenceParameter: {{fileID: 0}}
    floatParameter: 0
    intParameter: 0
    messageOptions: 0
  - time: 0.83333333
    functionName: FireDarkOrbTrigger
    data: 
    objectReferenceParameter: {{fileID: 0}}
    floatParameter: 0
    intParameter: 0
    messageOptions: 0
  - time: 1.20000000
    functionName: CurrentStateTrigger
    data: 
    objectReferenceParameter: {{fileID: 0}}
    floatParameter: 0
    intParameter: 0
    messageOptions: 0
"""

with open(anim_path, "w", encoding="utf-8") as f:
    f.write(anim_content)
print("Rebuilt ArchDemon_Attack.anim cleanly with 15 frames!")
