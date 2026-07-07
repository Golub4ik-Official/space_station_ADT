#!/bin/bash

OLD_CS_HEADER='// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter'\''s Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝
'

NEW_CS_HEADER='// 888       888 d8b 888      d8b      888    888                                 888                    
// 888   o   888 Y8P 888      Y8P      888    888                                 888                    
// 888  d8b  888     888               888    888                                 888                    
// 888 d888b 888 888 888  888 888      8888888888  8888b.  88888b.d88b.  88888b.  888888 .d88b.  888d888 
// 888d88888b888 888 888 .88P 888      888    888     "88b 888 "888 "88b 888 "88b 888   d8P  Y8b 888P"   
// 88888P Y88888 888 888888K  888      888    888 .d888888 888  888  888 888  888 888   88888888 888     
// 8888P   Y8888 888 888 "88b 888      888    888 888  888 888  888  888 888 d88P Y88b. Y8b.     888     
// 888P     Y888 888 888  888 888      888    888 "Y888888 888  888  888 88888P"   "Y888 "Y8888  888     
//                                                                      888                             
//                                                                      888                             
//                                                                      888
'

OLD_YAML_HEADER='# ╔══════════════════════════════════════════════════════╗
# ║  WikiHampter'\''s Cyborg Rework                        ║
# ║  Author: WikiHampter | MIT License                   ║
# ╚══════════════════════════════════════════════════════╝
'

NEW_YAML_HEADER='# 888       888 d8b 888      d8b      888    888                                 888                    
# 888   o   888 Y8P 888      Y8P      888    888                                 888                    
# 888  d8b  888     888               888    888                                 888                    
# 888 d888b 888 888 888  888 888      8888888888  8888b.  88888b.d88b.  88888b.  888888 .d88b.  888d888 
# 888d88888b888 888 888 .88P 888      888    888     "88b 888 "888 "88b 888 "88b 888   d8P  Y8b 888P"   
# 88888P Y88888 888 888888K  888      888    888 .d888888 888  888  888 888  888 888   88888888 888     
# 8888P   Y8888 888 888 "88b 888      888    888 888  888 888  888  888 888 d88P Y88b. Y8b.     888     
# 888P     Y888 888 888  888 888      888    888 "Y888888 888  888  888 88888P"   "Y888 "Y8888  888     
#                                                                      888                             
#                                                                      888                             
#                                                                      888
'

find Content.Shared/ADT/Silicons/Borgs/Core -name "*.cs" -type f | while read f; do
    if grep -q "$OLD_CS_HEADER" "$f" 2>/dev/null; then
        # Use python for multiline replacement to avoid sed escaping hell
        python3 -c "
import sys
with open('$f', 'r', encoding='utf-8') as file:
    content = file.read()
old = '''$OLD_CS_HEADER'''
new = '''$NEW_CS_HEADER'''
if old in content:
    content = content.replace(old, new)
    with open('$f', 'w', encoding='utf-8') as file:
        file.write(content)
    print('Updated: $f')
"
    fi
done

find Content.Server/ADT/Silicons/Borgs/Core -name "*.cs" -type f | while read f; do
    if grep -q "$OLD_CS_HEADER" "$f" 2>/dev/null; then
        python3 -c "
import sys
with open('$f', 'r', encoding='utf-8') as file:
    content = file.read()
old = '''$OLD_CS_HEADER'''
new = '''$NEW_CS_HEADER'''
if old in content:
    content = content.replace(old, new)
    with open('$f', 'w', encoding='utf-8') as file:
        file.write(content)
    print('Updated: $f')
"
    fi
done

find Content.Client/ADT/Silicons/Borgs/Core -name "*.cs" -type f | while read f; do
    if grep -q "$OLD_CS_HEADER" "$f" 2>/dev/null; then
        python3 -c "
import sys
with open('$f', 'r', encoding='utf-8') as file:
    content = file.read()
old = '''$OLD_CS_HEADER'''
new = '''$NEW_CS_HEADER'''
if old in content:
    content = content.replace(old, new)
    with open('$f', 'w', encoding='utf-8') as file:
        file.write(content)
    print('Updated: $f')
"
    fi
done

find Resources/Prototypes/ADT/Silicons/Borgs -name "*.yml" -type f | while read f; do
    if grep -q "WikiHampter" "$f" 2>/dev/null; then
        python3 -c "
import sys
with open('$f', 'r', encoding='utf-8') as file:
    content = file.read()
old = '''$OLD_YAML_HEADER'''
new = '''$NEW_YAML_HEADER'''
if old in content:
    content = content.replace(old, new)
    with open('$f', 'w', encoding='utf-8') as file:
        file.write(content)
    print('Updated: $f')
"
    fi
done

find Resources/Prototypes/ADT/Entities/Mobs/Cyborgs/Core -name "*.yml" -type f | while read f; do
    if grep -q "WikiHampter" "$f" 2>/dev/null; then
        python3 -c "
import sys
with open('$f', 'r', encoding='utf-8') as file:
    content = file.read()
old = '''$OLD_YAML_HEADER'''
new = '''$NEW_YAML_HEADER'''
if old in content:
    content = content.replace(old, new)
    with open('$f', 'w', encoding='utf-8') as file:
        file.write(content)
    print('Updated: $f')
"
    fi
done

echo "All headers updated!"
