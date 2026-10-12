import os
import re

# =========================================================================
# STEP 1: CREATE SUB-PREFABS
# =========================================================================

DRUID_EARTH_SPIKE_PREFAB = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &1661612929979200762
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 903074931570715225}
  - component: {fileID: 8887241165507466676}
  - component: {fileID: 1253651293912607394}
  - component: {fileID: 8667368846469882883}
  - component: {fileID: 8082858111485819837}
  m_Layer: 0
  m_Name: Druid_EarthSpike
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &903074931570715225
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1661612929979200762}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &8887241165507466676
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1661612929979200762}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 584b8d3b0f8c04bd68bb3cd7b1bac80f, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  damageRadius: 1.2
  whatIsTarget:
    serializedVersion: 2
    m_Bits: 128
  eruptionDelay: 0.45
  lifetime: 1
--- !u!212 &1253651293912607394
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1661612929979200762}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RayTracingAccelStructBuildFlagsOverride: 0
  m_RayTracingAccelStructBuildFlags: 1
  m_SmallMeshCulling: 1
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {fileID: 2100000, guid: a97c105638bdf8b4a8650670310a4cd3, type: 2}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {fileID: 0}
  m_ProbeAnchor: {fileID: 0}
  m_LightProbeVolumeOverride: {fileID: 0}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {fileID: 0}
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 1
  m_Sprite: {fileID: 192881885319043410, guid: 94645ff1bbf8742bab09185f9cb62e84, type: 3}
  m_Color: {r: 1, g: 1, b: 1, a: 1}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {x: 1, y: 1}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0
--- !u!61 &8667368846469882883
BoxCollider2D:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1661612929979200762}
  m_Enabled: 1
  serializedVersion: 3
  m_Density: 1
  m_Material: {fileID: 0}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_ForceSendLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_ForceReceiveLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_ContactCaptureLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_CallbackLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_IsTrigger: 1
  m_UsedByEffector: 0
  m_CompositeOperation: 0
  m_CompositeOrder: 0
  m_Offset: {x: 0, y: 0.75}
  m_SpriteTilingProperty:
    border: {x: 0, y: 0, z: 0, w: 0}
    pivot: {x: 0.5, y: 0.5}
    oldSize: {x: 1, y: 1}
    newSize: {x: 1, y: 1}
    adaptiveTilingThreshold: 0.5
    drawMode: 0
    adaptiveTiling: 0
  m_AutoTiling: 0
  m_Size: {x: 1.2, y: 1.5}
  m_EdgeRadius: 0
--- !u!95 &8082858111485819837
Animator:
  serializedVersion: 7
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1661612929979200762}
  m_Enabled: 1
  m_Avatar: {fileID: 0}
  m_Controller: {fileID: 9100000, guid: ad8fa76975604887a9998f7aed16948f, type: 2}
  m_CullingMode: 0
  m_UpdateMode: 0
  m_ApplyRootMotion: 0
  m_LinearVelocityBlending: 0
  m_StabilizeFeet: 0
  m_AnimatePhysics: 0
  m_WarningMessage: 
  m_HasTransformHierarchy: 1
  m_AllowConstantClipSamplingOptimization: 1
  m_KeepAnimatorStateOnDisable: 0
  m_WriteDefaultValuesOnDisable: 0
"""

ARCHDEMON_DARK_ORB_PREFAB = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &8283638665200491288
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 5285510823393787667}
  - component: {fileID: 6185998959358163839}
  - component: {fileID: 6910203080990270239}
  - component: {fileID: 4418727717915040996}
  - component: {fileID: 3847697577484056375}
  m_Layer: 0
  m_Name: ArchDemon_DarkOrb
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &5285510823393787667
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 8283638665200491288}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &6185998959358163839
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 8283638665200491288}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a3a1379b6bffa441685d74b2196eeb1a, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  speed: 11
  lifetime: 3.5
  whatIsTarget:
    serializedVersion: 2
    m_Bits: 128
  whatIsGround:
    serializedVersion: 2
    m_Bits: 64
  damageRadius: 1
--- !u!58 &6910203080990270239
CircleCollider2D:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 8283638665200491288}
  m_Enabled: 1
  serializedVersion: 3
  m_Density: 1
  m_Material: {fileID: 0}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_ForceSendLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_ForceReceiveLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_ContactCaptureLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_CallbackLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_IsTrigger: 1
  m_UsedByEffector: 0
  m_CompositeOperation: 0
  m_CompositeOrder: 0
  m_Offset: {x: 0, y: 0}
  m_Radius: 0.4
--- !u!50 &4418727717915040996
Rigidbody2D:
  serializedVersion: 5
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 8283638665200491288}
  m_BodyType: 1
  m_Simulated: 1
  m_UseFullKinematicContacts: 0
  m_UseAutoMass: 0
  m_Mass: 1
  m_LinearDamping: 0
  m_AngularDamping: 0.05
  m_GravityScale: 0
  m_Material: {fileID: 0}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_Interpolate: 0
  m_SleepingMode: 1
  m_CollisionDetection: 1
  m_Constraints: 4
--- !u!212 &3847697577484056375
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 8283638665200491288}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RayTracingAccelStructBuildFlagsOverride: 0
  m_RayTracingAccelStructBuildFlags: 1
  m_SmallMeshCulling: 1
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {fileID: 2100000, guid: a97c105638bdf8b4a8650670310a4cd3, type: 2}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {fileID: 0}
  m_ProbeAnchor: {fileID: 0}
  m_LightProbeVolumeOverride: {fileID: 0}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {fileID: 0}
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 2
  m_Sprite: {fileID: -269334206, guid: 54033911a3e1d994da3f0c1bc2014ed0, type: 3}
  m_Color: {r: 1, g: 1, b: 1, a: 1}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {x: 1, y: 1}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0
"""

def make_meta(guid):
    return f"""fileFormatVersion: 2
guid: {guid}
PrefabImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

os.makedirs("Assets/Prefab/Enemy/Enemy_DuskBorne/Druid", exist_ok=True)
os.makedirs("Assets/Prefab/Enemy/Enemy_DuskBorne/ArchDemon", exist_ok=True)

with open("Assets/Prefab/Enemy/Enemy_DuskBorne/Druid/Druid_EarthSpike.prefab", "w", encoding="utf-8") as f:
    f.write(DRUID_EARTH_SPIKE_PREFAB)
with open("Assets/Prefab/Enemy/Enemy_DuskBorne/Druid/Druid_EarthSpike.prefab.meta", "w", encoding="utf-8") as f:
    f.write(make_meta("7a82b4039cd84b4cb7df512a83210401"))

with open("Assets/Prefab/Enemy/Enemy_DuskBorne/ArchDemon/ArchDemon_DarkOrb.prefab", "w", encoding="utf-8") as f:
    f.write(ARCHDEMON_DARK_ORB_PREFAB)
with open("Assets/Prefab/Enemy/Enemy_DuskBorne/ArchDemon/ArchDemon_DarkOrb.prefab.meta", "w", encoding="utf-8") as f:
    f.write(make_meta("7a82b4039cd84b4cb7df512a83210402"))

print("Sub-prefabs created successfully!")

# =========================================================================
# STEP 2: EDIT 4 MAIN PREFABS
# =========================================================================

damage_scale_yaml = """    phyiscal: {phys}
    elemental: {elem}
    chillDuration: 3
    chillSlowMulitplier: 0.2
    burnDuratin: 3
    burnDamageScale: 1
    shockDuration: 3
    shockDamageScale: 1
    shockCharge: 0.4"""

def update_prefab(filepath, config):
    with open(filepath, "r", encoding="utf-8") as f:
        content = f.read()

    blocks = content.split("--- !u!")
    header = blocks[0]
    items = blocks[1:]

    new_items = []
    for item in items:
        lines = item.splitlines()
        first_line = lines[0]
        type_id, file_id = first_line.split(" &")
        comp_type = lines[1].split(":")[0]

        # 1. Root GameObject -> Ensure active
        if file_id == config["root_go"]:
            new_lines = []
            for l in lines:
                if l.startswith("  m_IsActive:"):
                    new_lines.append("  m_IsActive: 1")
                else:
                    new_lines.append(l)
            new_items.append("\n".join(new_lines))
            continue

        # 2. Main Enemy Script -> replace with DuskBorne class and fields
        if file_id == config["main_script_id"]:
            new_script = config["main_script_body"].strip()
            new_items.append(new_script)
            continue

        # 3. AnimationTriggers on child Animator -> replace script guid
        if file_id == config["triggers_script_id"]:
            new_lines = []
            for l in lines:
                if "m_Script:" in l:
                    new_lines.append(f"  m_Script: {{fileID: 11500000, guid: {config['triggers_script_guid']}, type: 3}}")
                else:
                    new_lines.append(l)
            new_items.append("\n".join(new_lines))
            continue

        # 4. Animator Component -> replace controller guid
        if file_id == config["animator_id"]:
            new_lines = []
            for l in lines:
                if "m_Controller:" in l:
                    new_lines.append(f"  m_Controller: {{fileID: 9100000, guid: {config['controller_guid']}, type: 2}}")
                else:
                    new_lines.append(l)
            new_items.append("\n".join(new_lines))
            continue

        # 5. SpriteRenderer on Animator child -> replace sprite
        if file_id == config["sr_id"]:
            new_lines = []
            for l in lines:
                if "m_Sprite:" in l:
                    new_lines.append(f"  m_Sprite: {{fileID: {config['sprite_file_id']}, guid: {config['sprite_guid']}, type: 3}}")
                else:
                    new_lines.append(l)
            new_items.append("\n".join(new_lines))
            continue

        # 6. CapsuleCollider2D -> update size and offset
        if file_id == config["collider_id"]:
            new_lines = []
            for l in lines:
                if l.startswith("  m_Size:"):
                    new_lines.append(f"  m_Size: {{x: {config['col_size'][0]}, y: {config['col_size'][1]}}}")
                elif l.startswith("  m_Offset:"):
                    new_lines.append(f"  m_Offset: {{x: {config['col_offset'][0]}, y: {config['col_offset'][1]}}}")
                else:
                    new_lines.append(l)
            new_items.append("\n".join(new_lines))
            continue

        # 7. Rigidbody2D -> update gravity if specified
        if file_id == config["rb_id"]:
            new_lines = []
            for l in lines:
                if l.startswith("  m_GravityScale:"):
                    new_lines.append(f"  m_GravityScale: {config['gravity_scale']}")
                else:
                    new_lines.append(l)
            new_items.append("\n".join(new_lines))
            continue

        # 8. Entity_Stats -> update maxHealth
        if file_id == config["stats_id"]:
            new_lines = []
            in_max_health = False
            for l in lines:
                if "maxHealth:" in l:
                    in_max_health = True
                    new_lines.append(l)
                elif in_max_health and "baseValue:" in l:
                    new_lines.append(f"      baseValue: {config['max_health']}")
                    in_max_health = False
                else:
                    new_lines.append(l)
            new_items.append("\n".join(new_lines))
            continue

        # Keep unchanged
        new_items.append(item)

    new_content = header + "--- !u!" + "--- !u!".join(new_items)
    with open(filepath, "w", encoding="utf-8") as f:
        f.write(new_content)
    print(f"Updated {filepath} successfully!")

# Configurations
severed_fang_cfg = {
    "root_go": "5264577701942099946",
    "main_script_id": "1321912545747341818",
    "triggers_script_id": "551965286187717739",
    "triggers_script_guid": "de447dca22279437a98645e2fe3bbddf",
    "animator_id": "3013544042889634119",
    "controller_guid": "a5437d05854e4171abb20d612f0e478f",
    "sr_id": "254388100759012535",
    "sprite_file_id": "1810033441652940789",
    "sprite_guid": "3fd1520b46e9e4c50b4358234774ac68",
    "collider_id": "5645034173617274973",
    "col_size": (0.7, 1.4),
    "col_offset": (0, 0.05),
    "rb_id": "5500465976147032307",
    "gravity_scale": 3.5,
    "stats_id": "5211453270537126983",
    "max_health": 120,
    "main_script_body": """114 &1321912545747341818
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 5264577701942099946}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 008f89a57904a49dabef038ba0490b75, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  whatIsGround:
    serializedVersion: 2
    m_Bits: 64
  groundCheckDistance: 0.5
  wallCheckDistance: 0.5
  groundCheck: {fileID: 360185221042212340}
  primaryWallCheck: {fileID: 8027753167807487678}
  secondaryWallCheck: {fileID: 0}
  questTargetId: enemy_severed_fang
  battleMoveSpeed: 4.2
  attackDistance: 1.8
  attackCooldown: 0.5
  canChasePlayer: 1
  battleTimeDuration: 5
  minRetreatDistance: 1
  retreatVelocity: {x: 5, y: 3}
  stunnedDuration: 1
  stunnedVelocity: {x: 7, y: 7}
  canBeStunned: 1
  idleTime: 1.5
  moveSpeed: 2.2
  moveAnimSpeedMultiplier: 1
  whatIsPlayer:
    serializedVersion: 2
    m_Bits: 128
  playerCheck: {fileID: 8027753167807487678}
  playerCheckDistance: 10
  attackRange: 1.8
  slashDamageScale:
""" + damage_scale_yaml.format(phys=1, elem=0) + """
  iaidoCooldown: 4.5
  iaidoMinDistance: 2.5
  iaidoMaxDistance: 7
  sheatheDuration: 0.85
  dashSpeed: 14
  dashDuration: 0.22
  iaidoDamageScale:
""" + damage_scale_yaml.format(phys=2, elem=0) + """
  isCounterWindowOpen: 0"""
}

demon_kin_cfg = {
    "root_go": "8145076301810619660",
    "main_script_id": "8006803563896450609",
    "triggers_script_id": "1898367668997492343",
    "triggers_script_guid": "352646e6dc24f4c65a6f82b10691e630",
    "animator_id": "8810960660426125699",
    "controller_guid": "60ea5ff876d3427591e2fc8c7d61d302",
    "sr_id": "9001546686245506333",
    "sprite_file_id": "-2062054304277209012",
    "sprite_guid": "0fa66c4f8aee14e879165316e9f94f0d",
    "collider_id": "8345090466306465662",
    "col_size": (0.9, 1.5),
    "col_offset": (0, 0.05),
    "rb_id": "2494753612905387561",
    "gravity_scale": 3.5,
    "stats_id": "2859603942038665876",
    "max_health": 250,
    "main_script_body": """114 &8006803563896450609
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 8145076301810619660}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: c5a1282214782481abc138c21840b50a, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  whatIsGround:
    serializedVersion: 2
    m_Bits: 64
  groundCheckDistance: 0.5
  wallCheckDistance: 0.5
  groundCheck: {fileID: 9117748851575502037}
  primaryWallCheck: {fileID: 5728903304584427755}
  secondaryWallCheck: {fileID: 0}
  questTargetId: enemy_demon_kin
  battleMoveSpeed: 3.2
  attackDistance: 2
  attackCooldown: 0.5
  canChasePlayer: 1
  battleTimeDuration: 5
  minRetreatDistance: 1
  retreatVelocity: {x: 4, y: 2}
  stunnedDuration: 1
  stunnedVelocity: {x: 7, y: 7}
  canBeStunned: 1
  idleTime: 2
  moveSpeed: 1.8
  moveAnimSpeedMultiplier: 1
  whatIsPlayer:
    serializedVersion: 2
    m_Bits: 128
  playerCheck: {fileID: 5728903304584427755}
  playerCheckDistance: 10
  attackRange: 2
  smashDamageScale:
""" + damage_scale_yaml.format(phys=1.5, elem=0) + """
  chargeSpeed: 7.5
  chargeDuration: 0.5
  chargeCooldown: 5
  chargeMinDistance: 3
  chargeMaxDistance: 6.5
  isEnraged: 0"""
}

druid_cfg = {
    "root_go": "5926138452479376247",
    "main_script_id": "2875815137076424296",
    "triggers_script_id": "8939448840123359118",
    "triggers_script_guid": "aea1a479e97ba40ecbba11413fd47ee1",
    "animator_id": "4282343690245179893",
    "controller_guid": "e0b9373a22f54e9e846f5469ac29f770",
    "sr_id": "5817307605662530160",
    "sprite_file_id": "-722312802967330248",
    "sprite_guid": "098a48ea66f18438ebcfa3b908d1ed80",
    "collider_id": "8636389197179010122",
    "col_size": (0.7, 1.4),
    "col_offset": (0, 0.05),
    "rb_id": "2493420114209610628",
    "gravity_scale": 3.5,
    "stats_id": "3836301624772371999",
    "max_health": 90,
    "main_script_body": """114 &2875815137076424296
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 5926138452479376247}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: d5f17931ebd084187b78dc5ae9be00e2, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  whatIsGround:
    serializedVersion: 2
    m_Bits: 64
  groundCheckDistance: 0.5
  wallCheckDistance: 0.5
  groundCheck: {fileID: 8581373796752057219}
  primaryWallCheck: {fileID: 2651624126528479162}
  secondaryWallCheck: {fileID: 0}
  questTargetId: enemy_druid
  battleMoveSpeed: 2.8
  attackDistance: 6
  attackCooldown: 0.5
  canChasePlayer: 1
  battleTimeDuration: 5
  minRetreatDistance: 2.8
  retreatVelocity: {x: 4, y: 2}
  stunnedDuration: 1
  stunnedVelocity: {x: 7, y: 7}
  canBeStunned: 1
  idleTime: 2
  moveSpeed: 1.6
  moveAnimSpeedMultiplier: 1
  whatIsPlayer:
    serializedVersion: 2
    m_Bits: 128
  playerCheck: {fileID: 2651624126528479162}
  playerCheckDistance: 10
  earthSpikePrefab: {fileID: 1661612929979200762, guid: 7a82b4039cd84b4cb7df512a83210401, type: 3}
  castCooldown: 3.5
  minCastDistance: 2.5
  maxCastDistance: 9
  retreatDistance: 2.8
  spikeDamageScale:
""" + damage_scale_yaml.format(phys=0.5, elem=1.5)
}

arch_demon_cfg = {
    "root_go": "727868592545740921",
    "main_script_id": "3099722170568900621",
    "triggers_script_id": "9222553567743972692",
    "triggers_script_guid": "03e0fec09602a435db2ebbda2ccafcc8",
    "animator_id": "5045216443747603460",
    "controller_guid": "b3deeb41a639487cb745782bbb74494d",
    "sr_id": "5829133510080885215",
    "sprite_file_id": "2764273851807834643",
    "sprite_guid": "e672c0b1b679e465981a334a5e0af1d0",
    "collider_id": "1564334986412126514",
    "col_size": (0.8, 1.5),
    "col_offset": (0, 0.05),
    "rb_id": "2155119814738091634",
    "gravity_scale": 0,
    "stats_id": "6265136132708382245",
    "max_health": 400,
    "main_script_body": """114 &3099722170568900621
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 727868592545740921}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 4bfa8a5fdc3384525adf9db25f5852ed, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  whatIsGround:
    serializedVersion: 2
    m_Bits: 64
  groundCheckDistance: 0.5
  wallCheckDistance: 0.5
  groundCheck: {fileID: 3092079180268448541}
  primaryWallCheck: {fileID: 1455469873346762095}
  secondaryWallCheck: {fileID: 0}
  questTargetId: enemy_arch_demon
  battleMoveSpeed: 3.5
  attackDistance: 7
  attackCooldown: 0.5
  canChasePlayer: 1
  battleTimeDuration: 6
  minRetreatDistance: 2.2
  retreatVelocity: {x: 4, y: 2}
  stunnedDuration: 1
  stunnedVelocity: {x: 6, y: 6}
  canBeStunned: 1
  idleTime: 1.5
  moveSpeed: 2
  moveAnimSpeedMultiplier: 1
  whatIsPlayer:
    serializedVersion: 2
    m_Bits: 128
  playerCheck: {fileID: 1455469873346762095}
  playerCheckDistance: 12
  darkOrbPrefab: {fileID: 8283638665200491288, guid: 7a82b4039cd84b4cb7df512a83210402, type: 3}
  firePoint: {fileID: 1401062207594513175}
  hoverHeight: 2.2
  spellcastCooldown: 4.2
  closeDangerDistance: 2.2
  teleportCooldown: 5.5
  spellDamageScale:
""" + damage_scale_yaml.format(phys=0.2, elem=2)
}

update_prefab("Assets/Prefab/Enemy/Enemy_DuskBorne/SeveredFang/SeveredFang.prefab", severed_fang_cfg)
update_prefab("Assets/Prefab/Enemy/Enemy_DuskBorne/DemonKin/DemonKin.prefab", demon_kin_cfg)
update_prefab("Assets/Prefab/Enemy/Enemy_DuskBorne/Druid/Druid.prefab", druid_cfg)
update_prefab("Assets/Prefab/Enemy/Enemy_DuskBorne/ArchDemon/ArchDemon.prefab", arch_demon_cfg)

print("All 4 prefabs updated successfully!")
