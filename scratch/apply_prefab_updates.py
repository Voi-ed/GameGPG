import re
import shutil

src = "Assets/Prefab/Enemy/Enemy_Skeleton.prefab"

damage_scale_yaml = """  slashDamageScale:
    phyiscal: 1
    elemental: 0
    chillDuration: 3
    chillSlowMulitplier: 0.2
    burnDuratin: 3
    burnDamageScale: 1
    shockDuration: 3
    shockDamageScale: 1
    shockCharge: 0.4
  iaidoCooldown: 4.5
  iaidoMinDistance: 2.5
  iaidoMaxDistance: 7
  sheatheDuration: 0.85
  dashSpeed: 14
  dashDuration: 0.22
  iaidoDamageScale:
    phyiscal: 2
    elemental: 0
    chillDuration: 3
    chillSlowMulitplier: 0.2
    burnDuratin: 3
    burnDamageScale: 1
    shockDuration: 3
    shockDamageScale: 1
    shockCharge: 0.4
  isCounterWindowOpen: 0"""

severed_fang_script_body = f"""  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 6409625887082432548}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 008f89a57904a49dabef038ba0490b75, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  whatIsGround:
    serializedVersion: 2
    m_Bits: 64
  groundCheckDistance: 0.5
  wallCheckDistance: 0.5
  groundCheck: {{fileID: 8737565620325827956}}
  primaryWallCheck: {{fileID: 8227408368413302491}}
  secondaryWallCheck: {{fileID: 0}}
  questTargetId: enemy_severed_fang
  battleMoveSpeed: 4.2
  attackDistance: 1.8
  attackCooldown: 0.5
  canChasePlayer: 1
  battleTimeDuration: 5
  minRetreatDistance: 1
  retreatVelocity: {{x: 5, y: 3}}
  stunnedDuration: 1
  stunnedVelocity: {{x: 7, y: 7}}
  canBeStunned: 1
  idleTime: 1.5
  moveSpeed: 2.2
  moveAnimSpeedMultiplier: 1
  whatIsPlayer:
    serializedVersion: 2
    m_Bits: 128
  playerCheck: {{fileID: 8227408368413302491}}
  playerCheckDistance: 10
  attackRange: 1.8
{damage_scale_yaml}"""

demon_kin_script_body = """  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 6409625887082432548}
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
  groundCheck: {fileID: 8737565620325827956}
  primaryWallCheck: {fileID: 8227408368413302491}
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
  playerCheck: {fileID: 8227408368413302491}
  playerCheckDistance: 10
  attackRange: 2
  smashDamageScale:
    phyiscal: 1.5
    elemental: 0
    chillDuration: 3
    chillSlowMulitplier: 0.2
    burnDuratin: 3
    burnDamageScale: 1
    shockDuration: 3
    shockDamageScale: 1
    shockCharge: 0.4
  chargeSpeed: 7.5
  chargeDuration: 0.5
  chargeCooldown: 5
  chargeMinDistance: 3
  chargeMaxDistance: 6.5
  isEnraged: 0"""

druid_script_body = """  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 6409625887082432548}
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
  groundCheck: {fileID: 8737565620325827956}
  primaryWallCheck: {fileID: 8227408368413302491}
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
  playerCheck: {fileID: 8227408368413302491}
  playerCheckDistance: 10
  earthSpikePrefab: {fileID: 1661612929979200762, guid: 7a82b4039cd84b4cb7df512a83210401, type: 3}
  castCooldown: 3.5
  minCastDistance: 2.5
  maxCastDistance: 9
  retreatDistance: 2.8
  spikeDamageScale:
    phyiscal: 0.5
    elemental: 1.5
    chillDuration: 3
    chillSlowMulitplier: 0.2
    burnDuratin: 3
    burnDamageScale: 1
    shockDuration: 3
    shockDamageScale: 1
    shockCharge: 0.4"""

arch_demon_script_body = """  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 6409625887082432548}
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
  groundCheck: {fileID: 8737565620325827956}
  primaryWallCheck: {fileID: 8227408368413302491}
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
  playerCheck: {fileID: 8227408368413302491}
  playerCheckDistance: 12
  darkOrbPrefab: {fileID: 8283638665200491288, guid: 7a82b4039cd84b4cb7df512a83210402, type: 3}
  firePoint: {fileID: 4484963058172476661}
  hoverHeight: 2.2
  spellcastCooldown: 4.2
  closeDangerDistance: 2.2
  teleportCooldown: 5.5
  spellDamageScale:
    phyiscal: 0.2
    elemental: 2
    chillDuration: 3
    chillSlowMulitplier: 0.2
    burnDuratin: 3
    burnDamageScale: 1
    shockDuration: 3
    shockDamageScale: 1
    shockCharge: 0.4"""

prefabs_data = [
    {
        "name": "SeveredFang",
        "file": "Assets/Prefab/Enemy/Enemy_DuskBorne/SeveredFang/SeveredFang.prefab",
        "script_body": severed_fang_script_body,
        "triggers_guid": "de447dca22279437a98645e2fe3bbddf",
        "controller_guid": "a5437d05854e4171abb20d612f0e478f",
        "sprite_fid": "1810033441652940789",
        "sprite_guid": "3fd1520b46e9e4c50b4358234774ac68",
        "col_size": ("0.7", "1.4"),
        "col_offset": ("0", "0.05"),
        "gravity": "3.5",
        "health": "120"
    },
    {
        "name": "DemonKin",
        "file": "Assets/Prefab/Enemy/Enemy_DuskBorne/DemonKin/DemonKin.prefab",
        "script_body": demon_kin_script_body,
        "triggers_guid": "352646e6dc24f4c65a6f82b10691e630",
        "controller_guid": "60ea5ff876d3427591e2fc8c7d61d302",
        "sprite_fid": "-2062054304277209012",
        "sprite_guid": "0fa66c4f8aee14e879165316e9f94f0d",
        "col_size": ("0.9", "1.5"),
        "col_offset": ("0", "0.05"),
        "gravity": "3.5",
        "health": "250"
    },
    {
        "name": "Druid",
        "file": "Assets/Prefab/Enemy/Enemy_DuskBorne/Druid/Druid.prefab",
        "script_body": druid_script_body,
        "triggers_guid": "aea1a479e97ba40ecbba11413fd47ee1",
        "controller_guid": "e0b9373a22f54e9e846f5469ac29f770",
        "sprite_fid": "-722312802967330248",
        "sprite_guid": "098a48ea66f18438ebcfa3b908d1ed80",
        "col_size": ("0.7", "1.4"),
        "col_offset": ("0", "0.05"),
        "gravity": "3.5",
        "health": "90"
    },
    {
        "name": "ArchDemon",
        "file": "Assets/Prefab/Enemy/Enemy_DuskBorne/ArchDemon/ArchDemon.prefab",
        "script_body": arch_demon_script_body,
        "triggers_guid": "03e0fec09602a435db2ebbda2ccafcc8",
        "controller_guid": "b3deeb41a639487cb745782bbb74494d",
        "sprite_fid": "2764273851807834643",
        "sprite_guid": "e672c0b1b679e465981a334a5e0af1d0",
        "col_size": ("0.8", "1.5"),
        "col_offset": ("0", "0.05"),
        "gravity": "0",
        "health": "400"
    }
]

for p in prefabs_data:
    # 1. Fresh copy from clean skeleton template
    shutil.copyfile(src, p["file"])
    with open(p["file"], "r", encoding="utf-8") as f:
        content = f.read()

    # 2. Rename root GameObject and ensure m_IsActive: 1
    content = content.replace("m_Name: Enemy_Skeleton", f"m_Name: {p['name']}")
    content = re.sub(
        r"(--- !u!1 &6409625887082432548\nGameObject:.*?m_IsActive: )\d+",
        r"\g<1>1",
        content,
        flags=re.DOTALL
    )

    # 3. Replace Main Enemy MonoBehaviour
    content = re.sub(
        r"(--- !u!114 &4713152696882292745\nMonoBehaviour:\n)(.*?)(?=\n--- !u!|\Z)",
        rf"\g<1>{p['script_body']}",
        content,
        flags=re.DOTALL
    )

    # 4. Replace AnimationTriggers script guid
    content = re.sub(
        r"(--- !u!114 &2944810346713108217\nMonoBehaviour:.*?m_Script: \{fileID: 11500000, guid: )[a-f0-9]+(, type: 3\})",
        rf"\g<1>{p['triggers_guid']}\g<2>",
        content,
        flags=re.DOTALL
    )

    # 5. Replace Animator Controller guid
    content = re.sub(
        r"(--- !u!95 &3630878707846590570\nAnimator:.*?m_Controller: \{fileID: 9100000, guid: )[a-f0-9]+(, type: 2\})",
        rf"\g<1>{p['controller_guid']}\g<2>",
        content,
        flags=re.DOTALL
    )

    # 6. Replace SpriteRenderer sprite
    content = re.sub(
        r"(--- !u!212 &5498923167564123464\nSpriteRenderer:.*?m_Sprite: \{fileID: )-?\d+, guid: [a-f0-9]+(, type: 3\})",
        rf"\g<1>{p['sprite_fid']}, guid: {p['sprite_guid']}\g<2>",
        content,
        flags=re.DOTALL
    )

    # 7. Update CapsuleCollider2D
    content = re.sub(
        r"(--- !u!70 &6607570325663642348\nCapsuleCollider2D:.*?m_Offset: )\{[^\}]+\}",
        rf"\g<1>{{x: {p['col_offset'][0]}, y: {p['col_offset'][1]}}}",
        content,
        flags=re.DOTALL
    )
    content = re.sub(
        r"(--- !u!70 &6607570325663642348\nCapsuleCollider2D:.*?m_Size: )\{[^\}]+\}",
        rf"\g<1>{{x: {p['col_size'][0]}, y: {p['col_size'][1]}}}",
        content,
        flags=re.DOTALL
    )

    # 8. Update Rigidbody2D gravity
    content = re.sub(
        r"(--- !u!50 &1326299994285886784\nRigidbody2D:.*?m_GravityScale: )[\d\.]+",
        rf"\g<1>{p['gravity']}",
        content,
        flags=re.DOTALL
    )

    # 9. Update Entity_Stats maxHealth
    content = re.sub(
        r"(--- !u!114 &4284223986879445403\nMonoBehaviour:.*?maxHealth:\n.*?baseValue: )\d+",
        rf"\g<1>{p['health']}",
        content,
        flags=re.DOTALL
    )

    with open(p["file"], "w", encoding="utf-8") as f:
        f.write(content)

    print(f"Applied updates to {p['file']}")

print("All updates applied cleanly!")
