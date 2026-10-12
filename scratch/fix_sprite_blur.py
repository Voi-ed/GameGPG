import os
import re

folder = "Assets/Graphics/EnemyToGameRpg/ModeNHanVatDo"

# Target files that need maxTextureSize: 4096 because width > 2048
large_sheets = {
    "SeveredFangBasicAtk001-Sheet.png.meta",
    "ArchDemonBasicAtk001-Sheet.png.meta"
}

updated_files = []

for root, dirs, files in os.walk(folder):
    for f in files:
        if not f.endswith(".png.meta"):
            continue

        p = os.path.join(root, f)
        with open(p, "r", encoding="utf-8") as fp:
            content = fp.read()

        orig = content

        # If it is one of the large sheets, set maxTextureSize to 4096
        if f in large_sheets:
            content = re.sub(r"maxTextureSize:\s*\d+", "maxTextureSize: 4096", content)

        # Set textureCompression: 0 everywhere (for all DuskBorne pixel art sheets)
        content = re.sub(r"textureCompression:\s*[12]", "textureCompression: 0", content)

        if content != orig:
            with open(p, "w", encoding="utf-8") as fp:
                fp.write(content)
            updated_files.append(f)

print(f"Updated {len(updated_files)} meta files:")
for uf in updated_files:
    print(f"  - {uf}")
