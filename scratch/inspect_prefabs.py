import re
import os

prefabs = {
    "SeveredFang": "Assets/Prefab/Enemy/Enemy_DuskBorne/SeveredFang/SeveredFang.prefab",
    "DemonKin": "Assets/Prefab/Enemy/Enemy_DuskBorne/DemonKin/DemonKin.prefab",
    "Druid": "Assets/Prefab/Enemy/Enemy_DuskBorne/Druid/Druid.prefab",
    "ArchDemon": "Assets/Prefab/Enemy/Enemy_DuskBorne/ArchDemon/ArchDemon.prefab"
}

def inspect_prefab(name, path):
    print(f"\n==================== {name} ====================")
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()

    blocks = content.split("--- !u!")
    header = blocks[0]
    items = blocks[1:]

    root_go = None
    components = {}

    for item in items:
        lines = item.splitlines()
        first_line = lines[0]
        type_id, file_id = first_line.split(" &")
        comp_type = lines[1].split(":")[0]

        # Check GameObject name
        if comp_type == "GameObject":
            for l in lines:
                if l.startswith("  m_Name:"):
                    go_name = l.split(":", 1)[1].strip()
                    if go_name == name:
                        root_go = (file_id, lines)
                    elif go_name == "Animator":
                        components["Animator_GO"] = file_id
                    elif go_name == "TargetCheck":
                        components["TargetCheck_GO"] = file_id
                    elif go_name == "GroundCheck":
                        components["GroundCheck_GO"] = file_id
                    elif go_name == "AttackAlert_Simple":
                        components["AttackAlert_Simple_GO"] = file_id

        # Check Scripts
        if comp_type == "MonoBehaviour":
            script_guid = None
            for l in lines:
                if "m_Script:" in l:
                    m = re.search(r"guid:\s*([a-f0-9]+)", l)
                    if m:
                        script_guid = m.group(1)
            if script_guid:
                if script_guid == "6fba1b5761bcfe341b0e611c8aecc498":
                    components["MainEnemyScript"] = (file_id, lines)
                elif script_guid == "8a8e2d35cbc9148469cb6f031622775b":
                    components["AnimTriggersScript"] = (file_id, lines)
                elif script_guid == "827985cac5dcbe14da108be2e9ba9e04":
                    components["Entity_Stats"] = (file_id, lines)
                elif script_guid == "2deb6c68399f5dd40a3049f988ed38ea":
                    components["Enemy_Health"] = (file_id, lines)

        if comp_type == "Animator":
            components["Animator"] = (file_id, lines)

        if comp_type == "SpriteRenderer":
            go_id = None
            for l in lines:
                if "m_GameObject:" in l:
                    m = re.search(r"fileID:\s*(\d+)", l)
                    if m:
                        go_id = m.group(1)
            components[f"SpriteRenderer_{go_id}"] = (file_id, lines)

        if comp_type == "Rigidbody2D":
            components["Rigidbody2D"] = (file_id, lines)

        if comp_type == "CapsuleCollider2D":
            components["CapsuleCollider2D"] = (file_id, lines)

    print(f"Root GO: {root_go[0] if root_go else 'NOT FOUND'}")
    for k in sorted(components.keys()):
        val = components[k]
        if isinstance(val, tuple):
            print(f"  {k}: fileID {val[0]}")
        else:
            print(f"  {k}: fileID {val}")

for name, path in prefabs.items():
    inspect_prefab(name, path)
