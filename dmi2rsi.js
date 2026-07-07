const fs = require('fs');
const path = require('path');
const zlib = require('zlib');
const { PNG } = require('pngjs');

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
    let version = 0;
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

        if (key === 'version') version = parseFloat(val);
        else if (key === 'width') width = parseInt(val);
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
    return { version, width, height, states };
}

function convertDmiToRsi(dmiPath, outputDir, copyright) {
    const buffer = fs.readFileSync(dmiPath);
    const png = PNG.sync.read(buffer, { filterType: -1 });
    const meta = readDmiMetadata(buffer);
    if (!meta) { console.error('No DMI metadata in', dmiPath); return; }

    const rsiName = path.basename(dmiPath, '.dmi') + '.rsi';
    const rsiDir = path.join(outputDir, rsiName);
    fs.mkdirSync(rsiDir, { recursive: true });

    const iconW = meta.width;
    const iconH = meta.height;
    const cols = Math.floor(png.width / iconW);

    let frameIdx = 0;
    const rsiStates = [];
    const stateDirs = new Map();

    // Pass 1: calculate frame index for each state
    for (const state of meta.states) {
        const name = state.name;
        const dirs = state.dirs || 1;
        const frames = state.frames || 1;
        const totalCells = dirs * frames;

        const stateDir = path.join(rsiDir, name);
        fs.mkdirSync(stateDir, { recursive: true });

        const dirNames = ['', '_r', '_l'];
        const pngFiles = [];

        for (let cell = 0; cell < totalCells; cell++) {
            const sx = (frameIdx % cols) * iconW;
            const sy = Math.floor(frameIdx / cols) * iconH;

            const frame = new PNG({ width: iconW, height: iconH });
            for (let y = 0; y < iconH; y++) {
                for (let x = 0; x < iconW; x++) {
                    const si = ((sy + y) * png.width + (sx + x)) * 4;
                    const di = (y * iconW + x) * 4;
                    for (let c = 0; c < 4; c++) frame.data[di + c] = png.data[si + c];
                }
            }

            const dir = cell % dirs;
            const frameNum = Math.floor(cell / dirs);
            const dirSuffix = dirs > 1 ? (dirNames[dir] || '') : '';
            const animSuffix = frames > 1 ? `_f${frameNum}` : '';

            // For multi-direction states: north = "", east = "_r", south/west = "_l"  
            // But BYOND uses cardinal dirs: 1=N, 2=S, 3=E, 4=W
            // For 4 dirs: N="", S="_s", E="_e", W="_w" (SS14 convention)
            // For _r/_l: BYOND dir order is N->S->E->W, SS14 expects N->E->S->W
            // Actually in the SS14 chassis.rsi: robot, robot_e, robot_e_r, robot_l
            // Where: _e = east/right, _e_r = ??? 

            // SS14 RSI conventions:
            // 1 direction: just the state name
            // 4 directions: "", "_e", "_l", "_r" (or "", "_r", "_l" for 3-dir)
            // Actually in SS14 the convention is: 4 dirs = North, East, South, West
            // The suffixes: ""=N, "_e"=E, "_l"=S, "_r"=W (looking at chassis.rsi: robot=North, robot_e=East?, sec_e_r=...)
            // Actually in chassis.rsi: robot(4dir), robot_e(4dir), robot_e_r(4dir), robot_l(4dir)
            // Looking at the SS14 borg sprites convention from Mobs/Silicon/chassis.rsi:
            // - "robot" = body (4 dirs)
            // - "robot_e" = has-mind eyes (4 dirs)
            // - "robot_e_r" = no-mind eyes (4 dirs)  
            // - "robot_l" = light (4 dirs)
            // The 4 directions in RSI are inferred from frames = 4 with delays
            // For RSI, each state with directions=N has N frames in a single PNG
            // The frame order is: North, East, South, West

            const filename = frames > 1
                ? `${name}${dirSuffix}.f${frameNum}.png`
                : `${name}${dirSuffix}.png`;

            fs.writeFileSync(path.join(stateDir, filename), PNG.sync.write(frame));
            pngFiles.push(filename);
            frameIdx++;
        }

        // Rename files to proper multi-frame RSI format
        // In SS14 RSI format: each state with directions==N is stored as N frames in one PNG? 
        // No - in SS14 RSI, each direction is a SEPARATE PNG file
        // Format: {statename}.png, {statename}_e.png, {statename}_l.png, {statename}_r.png
        // For animated: {statename}.f0.png, {statename}.f1.png etc.
        
        // Actually let's use proper RSI naming:
        // For states with frames:
        //   {name}.png - single frame for all directions
        // For animated:
        //   {name}.f{frame}.png

        stateDirs.set(name, { dirs, frames, delays: state.delays });
        frameIdx = frameIdx; // already tracked
    }

    // Create meta.json
    for (const [name, info] of stateDirs) {
        const entry = { name };
        if (info.dirs > 1) entry.directions = info.dirs;
        if (info.frames > 1 || info.delays.length > 0) {
            if (info.delays.length > 0) {
                entry.delays = info.delays;
            } else {
                entry.delays = Array.from({ length: info.dirs }, () => 
                    Array(info.frames).fill(0.1)
                );
            }
        }
        rsiStates.push(entry);
    }

    const rsiMeta = {
        version: 1,
        size: { x: iconW, y: iconH },
        license: 'CC-BY-SA-3.0',
        copyright: copyright || 'Converted from SS13 DMI',
        states: rsiStates
    };

    fs.writeFileSync(path.join(rsiDir, 'meta.json'), JSON.stringify(rsiMeta, null, 2));
    console.log(`✓ ${rsiName} (${rsiStates.length} states, ${png.width}x${png.height})`);
}

// Convert specific DMI files needed for the borg rework
const files = [
    {
        src: '_SS14_Borg_Port/SS13_Reference/icons/mob/robots.dmi',
        copyright: 'Converted from SS13 robots.dmi. Original: tgstation.'
    },
    {
        src: '_SS14_Borg_Port/SS13_Reference/icons/mob/robot_items.dmi',
        copyright: 'Converted from SS13 robot_items.dmi. Original: tgstation.'
    },
    {
        src: '_SS14_Borg_Port/SS13_Reference/icons/mob/screen_robot.dmi',
        copyright: 'Converted from SS13 screen_robot.dmi. Original: tgstation.'
    },
];

const outDir = 'Resources/Textures/_DMI_Converted';
fs.mkdirSync(outDir, { recursive: true });

for (const f of files) {
    if (fs.existsSync(f.src)) {
        convertDmiToRsi(f.src, outDir, f.copyright);
    } else {
        console.error('Not found:', f.src);
    }
}

console.log('\nDone! Converted DMI files to RSI format in', outDir);
