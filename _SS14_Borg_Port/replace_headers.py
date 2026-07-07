import os

old_cs = (
    '// ╔══════════════════════════════════════════════════════╗\n'
    '// ║  WikiHampter\'s Cyborg Rework                        ║\n'
    '// ║  Author: WikiHampter | MIT License                   ║\n'
    '// ╚══════════════════════════════════════════════════════╝'
)

new_cs = (
    '// 888       888 d8b 888      d8b      888    888                                 888                    \n'
    '// 888   o   888 Y8P 888      Y8P      888    888                                 888                    \n'
    '// 888  d8b  888     888               888    888                                 888                    \n'
    '// 888 d888b 888 888 888  888 888      8888888888  8888b.  88888b.d88b.  88888b.  888888 .d88b.  888d888 \n'
    '// 888d88888b888 888 888 .88P 888      888    888     "88b 888 "888 "88b 888 "88b 888   d8P  Y8b 888P"   \n'
    '// 88888P Y88888 888 888888K  888      888    888 .d888888 888  888  888 888  888 888   88888888 888     \n'
    '// 8888P   Y8888 888 888 "88b 888      888    888 888  888 888  888  888 888 d88P Y88b. Y8b.     888     \n'
    '// 888P     Y888 888 888  888 888      888    888 "Y888888 888  888  888 88888P"   "Y888 "Y8888  888     \n'
    '//                                                                      888                             \n'
    '//                                                                      888                             \n'
    '//                                                                      888'
)

old_yml = (
    '# ╔══════════════════════════════════════════════════════╗\n'
    '# ║  WikiHampter\'s Cyborg Rework                        ║\n'
    '# ║  Author: WikiHampter | MIT License                   ║\n'
    '# ╚══════════════════════════════════════════════════════╝'
)

new_yml = (
    '# 888       888 d8b 888      d8b      888    888                                 888                    \n'
    '# 888   o   888 Y8P 888      Y8P      888    888                                 888                    \n'
    '# 888  d8b  888     888               888    888                                 888                    \n'
    '# 888 d888b 888 888 888  888 888      8888888888  8888b.  88888b.d88b.  88888b.  888888 .d88b.  888d888 \n'
    '# 888d88888b888 888 888 .88P 888      888    888     "88b 888 "888 "88b 888 "88b 888   d8P  Y8b 888P"   \n'
    '# 88888P Y88888 888 888888K  888      888    888 .d888888 888  888  888 888  888 888   88888888 888     \n'
    '# 8888P   Y8888 888 888 "88b 888      888    888 888  888 888  888  888 888 d88P Y88b. Y8b.     888     \n'
    '# 888P     Y888 888 888  888 888      888    888 "Y888888 888  888  888 88888P"   "Y888 "Y8888  888     \n'
    '#                                                                      888                             \n'
    '#                                                                      888                             \n'
    '#                                                                      888'
)

count = 0
dirs_to_check = [
    'Content.Shared/ADT/Silicons/Borgs/Core',
    'Content.Server/ADT/Silicons/Borgs/Core',
    'Content.Client/ADT/Silicons/Borgs/Core',
]

for d in dirs_to_check:
    for root, dirs, files in os.walk(d):
        for f in files:
            if f.endswith('.cs'):
                path = os.path.join(root, f)
                with open(path, 'r', encoding='utf-8') as fh:
                    content = fh.read()
                if old_cs in content:
                    content = content.replace(old_cs, new_cs)
                    with open(path, 'w', encoding='utf-8') as fh:
                        fh.write(content)
                    count += 1
                    print(f'CS: {path}')

for root, dirs, files in os.walk('Resources/Prototypes/ADT'):
    for f in files:
        if not f.endswith('.yml'):
            continue
        path = os.path.join(root, f)
        with open(path, 'r', encoding='utf-8') as fh:
            content = fh.read()
        if 'WikiHampter' in content and old_yml in content:
            content = content.replace(old_yml, new_yml)
            with open(path, 'w', encoding='utf-8') as fh:
                fh.write(content)
            count += 1
            print(f'YML: {path}')

print(f'\nTotal: {count} files updated')
