const fs = require('fs');
const path = require('path');
const zlib = require('zlib');
const { PNG } = require('pngjs');

// ─── DMI Parsing ───────────────────────────────────────────────────────────

function readDmiMetadata(buffer) {
    let offset = 8;
    while (offset + 8 <= buffer.length) {
        const length = buffer.readUInt32BE(offset);
        const type = buffer.toString('ascii', offset + 4, offset + 8);
        if (type === 'zTXt' || type === 'tEXt') {
            const data = buffer.slice(offset + 8, offset + 8 + length);
            const nullIdx = data.indexOf(0);
            const keyword = data.slice(0, nullIdx).toString('ascii');
            if (keyword === 'Description') {
                let raw;
                if (type === 'zTXt') {
                    const compressed = data.slice(nullIdx + 2);
                    raw = zlib.inflateSync(compressed).toString('utf8');
                } else {
                    raw = data.slice(nullIdx + 1).toString('utf8');
                }
                return parseDmiText(raw);
            }
        }
        offset += 12 + length;
        if (offset >= buffer.length) break;
    }
    return null;
}

function parseDmiText(text) {
    const states = [];
    const lines = text.split('\n');
    let current = null;
    let inDmi = false;
    let width = 32, height = 32;

    for (let line of lines) {
        line = line.trim();
        if (line.startsWith('#')) {
            if (line.includes('BEGIN DMI')) inDmi = true;
            if (line.includes('END DMI')) { inDmi = false; if (current) { states.push(current); current = null; } }
            continue;
        }
        if (!inDmi) continue;

        const eq = line.indexOf('=');
        if (eq === -1) continue;
        const key = line.substring(0, eq).trim();
        const val = line.substring(eq + 1).trim();

        if (key === 'width') width = parseInt(val);
        else if (key === 'height') height = parseInt(val);
        else if (key === 'state') {
            if (current) states.push(current);
            const m = val.match(/"([^"]*)"/);
            current = { name: m ? m[1] : val, dirs: 1, frames: 1, delays: [] };
        } else if (current) {
            if (key === 'dirs') current.dirs = parseInt(val) || 1;
            else if (key === 'frames') current.frames = parseInt(val) || 1;
            else if (key === 'delay') {
                const delays = val.replace(/[{} ]/g, '').split(',').filter(x => x).map(parseFloat);
                if (delays.length > 0) current.delays.push(delays);
            }
        }
    }
    if (current) states.push(current);
    return { width, height, states };
}

// Extract a single frame from the DMI spritesheet
function extractFrame(pngData, pngWidth, iconW, iconH, frameIdx, cols) {
    const sx = (frameIdx % cols) * iconW;
    const sy = Math.floor(frameIdx / cols) * iconH;
    const frame = new PNG({ width: iconW, height: iconH });
    for (let y = 0; y < iconH; y++) {
        for (let x = 0; x < iconW; x++) {
            const si = ((sy + y) * pngWidth + (sx + x)) * 4;
            const di = (y * iconW + x) * 4;
            for (let c = 0; c < 4; c++) frame.data[di + c] = pngData[si + c];
        }
    }
    return frame;
}

// DMI direction order (BYOND): South, North, East, West
// SS14 RSI direction enum: South=0, North=1, East=2, West=3
// They match! dir index = RsiDirection value

// ─── Flat RSI Creation ─────────────────────────────────────────────────────

function fixDelays(delays, dirs) {
    if (!delays || delays.length === 0) return null;
    if (delays.length === dirs) {
        // Already correct format, but check for single-frame case
        return delays.every(d => d.length === 1) ? null : delays;
    }
    if (delays.length === 1 && delays[0].length > 0) {
        // DMI format: single array of delays (shared by all directions)
        // Replicate for each direction
        return Array.from({ length: dirs }, () => [...delays[0]]);
    }
    return null; // Can't determine, omit
}

function createFlatRsi(rsiDir, iconSize, states) {
    // states = [{ name, directions: 4|1, frames: [[Array of delays per dir]], images: [[Array of frame PNGs per dir]] }]
    fs.mkdirSync(rsiDir, { recursive: true });

    const metaStates = [];

    for (const state of states) {
        const dirs = state.directions || 1;
        // Compose frames into one PNG
        // Frame order: all frames of South, then all frames of North, then East, then West
        let allFrames = [];
        for (let d = 0; d < dirs; d++) {
            for (const frame of state.images[d]) {
                allFrames.push(frame);
            }
        }

        const totalFrames = allFrames.length;
        const srcWidth = Math.ceil(Math.sqrt(totalFrames * iconSize.x / iconSize.y));
        // Make sure width divides evenly by iconSize.x
        let cols = srcWidth;
        let rows = Math.ceil(totalFrames / cols);
        // Adjust to minimize wasted space
        while (cols * rows < totalFrames) {
            if (cols <= rows) cols++;
            else rows++;
        }
        // Final PNG dimensions
        const pngW = cols * iconSize.x;
        const pngH = rows * iconSize.y;

        const outPng = new PNG({ width: pngW, height: pngH });
        for (let i = 0; i < totalFrames; i++) {
            const col = i % cols;
            const row = Math.floor(i / cols);
            const frame = allFrames[i];
            // Blit frame at (col * iconW, row * iconH)
            for (let y = 0; y < iconSize.y; y++) {
                for (let x = 0; x < iconSize.x; x++) {
                    const si = (y * iconSize.x + x) * 4;
                    const di = ((row * iconSize.y + y) * pngW + (col * iconSize.x + x)) * 4;
                    for (let c = 0; c < 4; c++) outPng.data[di + c] = frame.data[si + c];
                }
            }
        }

        fs.writeFileSync(path.join(rsiDir, `${state.name}.png`), PNG.sync.write(outPng));

        const metaEntry = { name: state.name, directions: dirs };
        if (state.delays) {
            metaEntry.delays = state.delays;
        }
        metaStates.push(metaEntry);
    }

    const meta = {
        version: 1,
        size: { x: iconSize.x, y: iconSize.y },
        license: 'CC-BY-SA-3.0',
        copyright: 'Converted from SS13 robots.dmi. Original: tgstation. ADT borg rework subtype.',
        states: metaStates
    };

    fs.writeFileSync(path.join(rsiDir, 'meta.json'), JSON.stringify(meta, null, 2));
    console.log(`  Created RSI: ${rsiDir} (${states.length} states)`);
}

// ─── DMI Frame Extraction (properly handles 4-dir) ─────────────────────────

function extractStateFrames(dmiPng, meta, stateName) {
    const stateMeta = meta.states.find(s => s.name === stateName);
    if (!stateMeta) return null;

    const iconW = meta.width;
    const iconH = meta.height;
    const cols = Math.floor(dmiPng.width / iconW);

    // Calculate frame index offset for this state in the spritesheet
    let frameIdx = 0;
    for (const s of meta.states) {
        if (s.name === stateName) break;
        frameIdx += (s.dirs || 1) * (s.frames || 1);
    }

    const dirs = stateMeta.dirs || 1;
    const frames = stateMeta.frames || 1;

    // DMI frame order within a state: all directions for frame 0, then all directions for frame 1, etc.
    // Actually, BYOND DMI order is: dir 0 frame 0, dir 1 frame 0, ..., dir N frame 0, dir 0 frame 1, ...
    // This is direction-major interleaved.
    // But we need SS14 RSI order: all frames of dir 0 (South), then all frames of dir 1 (North), etc.

    // For DMI with dirs=4, frames=1:
    //   Frame 0: dir 0 (South), Frame 1: dir 1 (North), Frame 2: dir 2 (East), Frame 3: dir 3 (West)
    // This is already in the right order!

    // For DMI with dirs=4, frames=2:
    //   Frame 0: dir 0 f0 (South f0), Frame 1: dir 1 f0 (North f0), Frame 2: dir 2 f0 (East f0), Frame 3: dir 3 f0 (West f0)
    //   Frame 4: dir 0 f1 (South f1), Frame 5: dir 1 f1 (North f1), ...
    // We need to reorder to: South f0, South f1, North f0, North f1, East f0, East f1, West f0, West f1

    // Extract all raw frames
    const rawFrames = [];
    for (let i = 0; i < dirs * frames; i++) {
        rawFrames.push(extractFrame(dmiPng.data, dmiPng.width, iconW, iconH, frameIdx + i, cols));
    }

    // Reorder from DMI (direction-major) to SS14 RSI (direction-grouped)
    // DMI: [d0f0, d1f0, d2f0, d3f0, d0f1, d1f1, d2f1, d3f1, ...]
    // RSI: [d0f0, d0f1, ..., d1f0, d1f1, ..., d2f0, d2f1, ..., d3f0, d3f1, ...]
    const result = { dirs, frames, delays: stateMeta.delays, images: [] };

    for (let d = 0; d < dirs; d++) {
        result.images[d] = [];
        for (let f = 0; f < frames; f++) {
            const dmIdx = f * dirs + d; // DMI index
            result.images[d].push(rawFrames[dmIdx]);
        }
    }

    return result;
}

// ─── Mapping: DMI state → Borg Type ───────────────────────────────────────

const BORG_TYPES = {
    generic:     { prefix: 'robot',    rsiDir: 'Generic' },
    security:    { prefix: 'sec',      rsiDir: 'Security' },
    medical:     { prefix: 'medical',  rsiDir: 'Medical' },
    engineering: { prefix: 'engineer', rsiDir: 'Engineering' },
    mining:      { prefix: 'miner',    rsiDir: 'Mining' },
    janitor:     { prefix: 'janitor',  rsiDir: 'Janitor' },
    service:     { prefix: 'service',  rsiDir: 'Service' },
};

// Each entry: { dmiBody, dmiEyes, type, id, name }
const SUBTYPE_MAP = [
    // ── Standard Series (uses eyes-Standard) ──
    { dmiBody: 'Standard', dmiEyes: 'eyes-Standard', type: 'generic',     id: 'standard',     name: 'Standard' },
    { dmiBody: 'Standard-Security',   dmiEyes: 'eyes-Standard',        type: 'security',    id: 'standard_security',    name: 'Standard Security' },
    { dmiBody: 'Standard-Medical',    dmiEyes: 'eyes-Standard',        type: 'medical',     id: 'standard_medical',     name: 'Standard Medical' },
    { dmiBody: 'Standard-Engineering', dmiEyes: 'eyes-Standard',       type: 'engineering', id: 'standard_engineering', name: 'Standard Engineering' },
    { dmiBody: 'Standard-Mining',     dmiEyes: 'eyes-Standard',        type: 'mining',      id: 'standard_mining',      name: 'Standard Mining' },
    { dmiBody: 'Standard-Janitor',    dmiEyes: 'eyes-Standard',        type: 'janitor',     id: 'standard_janitor',     name: 'Standard Janitor' },
    { dmiBody: 'Standard-Service',    dmiEyes: 'eyes-Standard',        type: 'service',     id: 'standard_service',     name: 'Standard Service' },

    // ── Noble Series (uses eyes-Standard) ──
    { dmiBody: 'Noble', dmiEyes: 'eyes-Standard', type: 'generic',     id: 'noble',         name: 'Noble' },
    { dmiBody: 'Noble-Security',   dmiEyes: 'eyes-Standard', type: 'security',    id: 'noble_security',    name: 'Noble Security' },
    { dmiBody: 'Noble-Medical',    dmiEyes: 'eyes-Standard', type: 'medical',     id: 'noble_medical',     name: 'Noble Medical' },
    { dmiBody: 'Noble-Engineering', dmiEyes: 'eyes-Standard', type: 'engineering', id: 'noble_engineering', name: 'Noble Engineering' },
    { dmiBody: 'Noble-Mining',     dmiEyes: 'eyes-Standard', type: 'mining',      id: 'noble_mining',      name: 'Noble Mining' },
    { dmiBody: 'Noble-Janitor',    dmiEyes: 'eyes-Standard', type: 'janitor',     id: 'noble_janitor',     name: 'Noble Janitor' },
    { dmiBody: 'Noble-Service',    dmiEyes: 'eyes-Standard', type: 'service',     id: 'noble_service',     name: 'Noble Service' },

    // ── Cricket Series (uses eyes-Cricket) ──
    { dmiBody: 'Cricket-Security',   dmiEyes: 'eyes-Cricket', type: 'security',    id: 'cricket_security',    name: 'Cricket Security' },
    { dmiBody: 'Cricket-Medical',    dmiEyes: 'eyes-Cricket', type: 'medical',     id: 'cricket_medical',     name: 'Cricket Medical' },
    { dmiBody: 'Cricket-Engineering', dmiEyes: 'eyes-Cricket', type: 'engineering', id: 'cricket_engineering', name: 'Cricket Engineering' },
    { dmiBody: 'Cricket-Mining',     dmiEyes: 'eyes-Cricket', type: 'mining',      id: 'cricket_mining',      name: 'Cricket Mining' },
    { dmiBody: 'Cricket-Janitor',    dmiEyes: 'eyes-Cricket', type: 'janitor',     id: 'cricket_janitor',     name: 'Cricket Janitor' },
    { dmiBody: 'Cricket-Service',    dmiEyes: 'eyes-Cricket', type: 'service',     id: 'cricket_service',     name: 'Cricket Service' },

    // ── Rover Series (uses eyes-Rover-*) ──
    { dmiBody: 'Rover-Engineering', dmiEyes: 'eyes-Rover-Engineering', type: 'engineering', id: 'rover_engineering', name: 'Rover Engineer' },
    { dmiBody: 'Rover-Janitor',     dmiEyes: 'eyes-Rover-Janitor',     type: 'janitor',     id: 'rover_janitor',     name: 'Rover Janitor' },
    { dmiBody: 'Rover-Medical',     dmiEyes: 'eyes-Rover-Medical',     type: 'medical',     id: 'rover_medical',     name: 'Rover Medical' },
    { dmiBody: 'Rover-Service',     dmiEyes: 'eyes-Rover-Service',     type: 'service',     id: 'rover_service',     name: 'Rover Service' },

    // ── Individual Types ──
    { dmiBody: 'Bloodhound',       dmiEyes: 'eyes-Bloodhound',       type: 'security',    id: 'bloodhound',         name: 'Bloodhound' },
    { dmiBody: 'Droid',            dmiEyes: 'eyes-Droid',            type: 'generic',     id: 'droid',              name: 'Droid' },
    { dmiBody: 'Droid_Medical',    dmiEyes: 'eyes-Droid_Medical',    type: 'medical',     id: 'droid_medical',      name: 'Droid Medical' },
    { dmiBody: 'Droid_Mining',     dmiEyes: 'eyes-Droid_Mining',     type: 'mining',      id: 'droid_mining',       name: 'Droid Mining' },
    { dmiBody: 'Heavy_Sec',        dmiEyes: 'eyes-Heavy_Sec',        type: 'security',    id: 'heavy_sec',          name: 'Heavy Security' },
    { dmiBody: 'Qualified_Doctor', dmiEyes: 'eyes-Qualified_Doctor', type: 'medical',     id: 'qualified_doctor_dmi', name: 'Qualified Doctor' },
    { dmiBody: 'Squat_Miner',      dmiEyes: 'eyes-Squat_Miner',      type: 'mining',      id: 'squat_miner',        name: 'Squat Miner' },
    { dmiBody: 'repairbot',        dmiEyes: 'eyes-repairbot',        type: 'generic',     id: 'repairbot',          name: 'Repairbot' },
    { dmiBody: 'Xenoborg',         dmiEyes: null,                    type: 'generic',     id: 'xenoborg',           name: 'Xenoborg' },
    { dmiBody: 'Landmate',         dmiEyes: 'eyes-Landmate',         type: 'engineering', id: 'landmate',           name: 'Landmate' },
];

// ─── Main ──────────────────────────────────────────────────────────────────

function main() {
    const dmiPath = '_SS14_Borg_Port/SS13_Reference/icons/mob/robots.dmi';
    const baseOutputDir = 'Resources/Textures/ADT/Mobs/Silicon/Borgs/Borg_subtype';

    if (!fs.existsSync(dmiPath)) {
        console.error('DMI not found:', dmiPath);
        process.exit(1);
    }

    console.log('Reading DMI:', dmiPath);
    const buffer = fs.readFileSync(dmiPath);
    const dmiPng = PNG.sync.read(buffer, { filterType: -1 });
    const meta = readDmiMetadata(buffer);

    if (!meta) {
        console.error('No DMI metadata found');
        process.exit(1);
    }

    console.log(`  States: ${meta.states.length}, Size: ${meta.width}x${meta.height}, Sheet: ${dmiPng.width}x${dmiPng.height}`);
    console.log('');

    let createdCount = 0;

    for (const entry of SUBTYPE_MAP) {
        const borgType = BORG_TYPES[entry.type];
        if (!borgType) {
            console.warn(`  Unknown borg type: ${entry.type}`);
            continue;
        }

        const prefix = borgType.prefix;
        const category = borgType.rsiDir;
        const rsiName = `${entry.id}.rsi`;
        const rsiDir = path.join(baseOutputDir, category, rsiName);

        console.log(`\nCreating subtype: ${entry.id} (${entry.name} → ${entry.type})`);

        // Extract body frames
        const body = extractStateFrames(dmiPng, meta, entry.dmiBody);
        if (!body) {
            console.warn(`  SKIP: body state "${entry.dmiBody}" not found in DMI`);
            continue;
        }

        // Extract eyes frames (or use body as fallback)
        let eyes = null;
        if (entry.dmiEyes) {
            eyes = extractStateFrames(dmiPng, meta, entry.dmiEyes);
        }
        if (!eyes) {
            // Use body frames for eyes too (no visible distinction)
            eyes = body;
        }

        const iconSize = { x: meta.width, y: meta.height };

        // Build states for the RSI
        const rsiStates = [];

        // State 1: Body (prefix)
        rsiStates.push({
            name: prefix,
            directions: body.dirs,
            delays: fixDelays(body.delays, body.dirs),
            images: body.images
        });

        // State 2: Has-mind eyes (prefix_e)
        rsiStates.push({
            name: `${prefix}_e`,
            directions: eyes.dirs,
            delays: fixDelays(eyes.delays, eyes.dirs),
            images: eyes.images
        });

        // State 3: No-mind eyes (prefix_e_r) - same as eyes for now
        rsiStates.push({
            name: `${prefix}_e_r`,
            directions: eyes.dirs,
            delays: null, // single frame
            images: eyes.images
        });

        // State 4: Toggle light (prefix_l) - same as eyes for now
        rsiStates.push({
            name: `${prefix}_l`,
            directions: eyes.dirs,
            delays: fixDelays(eyes.delays, eyes.dirs),
            images: eyes.images
        });

        createFlatRsi(rsiDir, iconSize, rsiStates);
        createdCount++;

        // Generate YAML content
        const subtypeId = `ss13_${entry.id}`;
        const dummyId = `ADTSs13${entry.id.charAt(0).toUpperCase() + entry.id.slice(1)}Chassis`;
        const parentDummy = entry.type === 'security' ? 'ADTBorgChassisSec'
            : `BorgChassis${entry.type.charAt(0).toUpperCase() + entry.type.slice(1)}`;

        console.log(`  YAML subtype: ${subtypeId}`);
        console.log(`  YAML entity: ${dummyId}`);
    }

    console.log(`\nDone! Created ${createdCount} subtypes.`);
}

main();
