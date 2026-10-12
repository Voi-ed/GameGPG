import re
import os

configs = {
    "Assets/Graphics/EnemyToGameRpg/ModeNHanVatDo/FallenOrder-SeveredFang/SpriteSheet/SeveredFangBasicAtk001-Sheet.png.meta": {
        "pivot_x": 0.375, # 48 / 128, offsets -16px back to idle center at x=60
        "pivot_y": 0.5
    },
    "Assets/Graphics/EnemyToGameRpg/ModeNHanVatDo/DuskBorne-Druid/SpriteSheet/DruidBasicAtk1-Sheet.png.meta": {
        "pivot_x": 0.3203125, # 41 / 128, offsets -23px back to idle center at x=64
        "pivot_y": 0.5
    },
    "Assets/Graphics/EnemyToGameRpg/ModeNHanVatDo/DuskBorne-ArchDemon/SpriteSheets/ArchDemonBasicAtk001-Sheet.png.meta": {
        "pivot_x": 0.2578125, # 66 / 256, offsets -62px back to idle center at x=64 in 256px frame
        "pivot_y": 0.5
    },
    "Assets/Graphics/EnemyToGameRpg/ModeNHanVatDo/DuskBorne-DemonKin/SpriteSheet/DemonKinBasicAtk001-Sheet.png.meta": {
        "pivot_x": 0.4921875, # 63 / 128, offsets -1px back to idle center at x=62
        "pivot_y": 0.5
    }
}

for meta_path, cfg in configs.items():
    if not os.path.exists(meta_path):
        print(f"File not found: {meta_path}")
        continue
    
    with open(meta_path, "r", encoding="utf-8") as f:
        content = f.read()

    px = cfg["pivot_x"]
    py = cfg["pivot_y"]

    # 1. Update TextureImporter header alignment and spritePivot
    content = re.sub(
        r"(\s+alignment:\s*)\d+",
        r"\g<1>9",
        content,
        count=1
    )
    content = re.sub(
        r"(\s+spritePivot:\s*\{x:\s*)[^,]+(,\s*y:\s*)[^\}]+(\})",
        rf"\g<1>{px}\g<2>{py}\g<3>",
        content,
        count=1
    )

    # 2. Update all spriteSheet sprites entries: alignment: 9, pivot: {x: px, y: py}
    # Pattern for each sprite slice:
    #       alignment: 0
    #       pivot: {x: 0, y: 0}
    content = re.sub(
        r"(\s+alignment:\s*)\d+(\s*\n\s+pivot:\s*\{x:\s*)[^,]+(,\s*y:\s*)[^\}]+(\})",
        rf"\g<1>9\g<2>{px}\g<3>{py}\g<4>",
        content
    )

    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(content)

    print(f"Updated {os.path.basename(meta_path)} -> Custom Pivot ({px}, {py})")

print("All attack sprite sheet meta files successfully updated!")
