const fs = require('fs');
const path = require('path');

function fixMetaJson(metaPath) {
    const data = JSON.parse(fs.readFileSync(metaPath, 'utf-8'));
    let changed = false;

    for (const state of data.states) {
        if (!state.delays) continue;

        // Remove empty delays arrays
        if (state.delays.length === 0) {
            delete state.delays;
            changed = true;
            continue;
        }

        // Fix: DMI delays format may have wrong dimension
        // RSI needs: delays[i] = delays for direction i (length = frames per direction)
        const dirs = state.directions || 1;

        if (state.delays.length !== dirs) {
            // DMI stores delays as one entry per animation frame (shared across all directions)
            // RSI needs delays as one entry per direction, each with frames_per_dir entries
            if (state.delays.length === 1 && state.delays[0].length > 0) {
                // Replicate the same delays for each direction
                const allDelays = state.delays[0];
                state.delays = Array.from({ length: dirs }, () => [...allDelays]);
                changed = true;
            } else {
                // Unknown format, collapse to 1 frame per direction
                state.delays = Array.from({ length: dirs }, () => [1]);
                changed = true;
            }
        }
    }

    if (changed) {
        // Remove delays field from single-frame states
        for (const state of data.states) {
            if (state.delays && state.delays.every(d => d.length === 1)) {
                delete state.delays;
            }
        }
        fs.writeFileSync(metaPath, JSON.stringify(data, null, 2));
        console.log(`  Fixed: ${path.basename(path.dirname(metaPath))}/${path.basename(metaPath)}`);
    }
}

function walkDir(dir) {
    const entries = fs.readdirSync(dir, { withFileTypes: true });
    for (const entry of entries) {
        const fullPath = path.join(dir, entry.name);
        if (entry.isDirectory()) {
            walkDir(fullPath);
        } else if (entry.name === 'meta.json' && fullPath.includes('Borg_subtype')) {
            fixMetaJson(fullPath);
        }
    }
}

console.log('Fixing Borg_subtype RSI meta.json files...');
walkDir('Resources/Textures/ADT/Mobs/Silicon/Borgs/Borg_subtype');
console.log('Done.');
