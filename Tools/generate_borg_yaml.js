const fs = require('fs');
const path = require('path');

// ─── Mapping ───────────────────────────────────────────────────────────────

const BORG_TYPES = {
    generic:     { parent: 'BorgChassisGeneric',    name: 'cyborg' },
    security:    { parent: 'ADTBorgChassisSec',      name: 'security cyborg' },
    medical:     { parent: 'BorgChassisMedical',     name: 'medical cyborg' },
    engineering: { parent: 'BorgChassisEngineer',    name: 'engineering cyborg' },
    mining:      { parent: 'BorgChassisMining',      name: 'mining cyborg' },
    janitor:     { parent: 'BorgChassisJanitor',     name: 'janitor cyborg' },
    service:     { parent: 'BorgChassisService',     name: 'service cyborg' },
};

const ENTRIES = [
    { id: 'standard', type: 'generic', name: 'Standard Cyborg' },
    { id: 'standard_security', type: 'security', name: 'Standard Security Cyborg' },
    { id: 'standard_medical', type: 'medical', name: 'Standard Medical Cyborg' },
    { id: 'standard_engineering', type: 'engineering', name: 'Standard Engineering Cyborg' },
    { id: 'standard_mining', type: 'mining', name: 'Standard Mining Cyborg' },
    { id: 'standard_janitor', type: 'janitor', name: 'Standard Janitor Cyborg' },
    { id: 'standard_service', type: 'service', name: 'Standard Service Cyborg' },
    { id: 'noble', type: 'generic', name: 'Noble Cyborg' },
    { id: 'noble_security', type: 'security', name: 'Noble Security Cyborg' },
    { id: 'noble_medical', type: 'medical', name: 'Noble Medical Cyborg' },
    { id: 'noble_engineering', type: 'engineering', name: 'Noble Engineering Cyborg' },
    { id: 'noble_mining', type: 'mining', name: 'Noble Mining Cyborg' },
    { id: 'noble_janitor', type: 'janitor', name: 'Noble Janitor Cyborg' },
    { id: 'noble_service', type: 'service', name: 'Noble Service Cyborg' },
    { id: 'cricket_security', type: 'security', name: 'Cricket Security Cyborg' },
    { id: 'cricket_medical', type: 'medical', name: 'Cricket Medical Cyborg' },
    { id: 'cricket_engineering', type: 'engineering', name: 'Cricket Engineering Cyborg' },
    { id: 'cricket_mining', type: 'mining', name: 'Cricket Mining Cyborg' },
    { id: 'cricket_janitor', type: 'janitor', name: 'Cricket Janitor Cyborg' },
    { id: 'cricket_service', type: 'service', name: 'Cricket Service Cyborg' },
    { id: 'rover_engineering', type: 'engineering', name: 'Rover Engineering Cyborg' },
    { id: 'rover_janitor', type: 'janitor', name: 'Rover Janitor Cyborg' },
    { id: 'rover_medical', type: 'medical', name: 'Rover Medical Cyborg' },
    { id: 'rover_service', type: 'service', name: 'Rover Service Cyborg' },
    { id: 'bloodhound', type: 'security', name: 'Bloodhound Cyborg' },
    { id: 'droid', type: 'generic', name: 'Droid Cyborg' },
    { id: 'droid_medical', type: 'medical', name: 'Droid Medical Cyborg' },
    { id: 'droid_mining', type: 'mining', name: 'Droid Mining Cyborg' },
    { id: 'heavy_sec', type: 'security', name: 'Heavy Security Cyborg' },
    { id: 'qualified_doctor_dmi', type: 'medical', name: 'Qualified Doctor Cyborg' },
    { id: 'squat_miner', type: 'mining', name: 'Squat Miner Cyborg' },
    { id: 'repairbot', type: 'generic', name: 'Repairbot Cyborg' },
    { id: 'xenoborg', type: 'generic', name: 'Xenoborg Cyborg' },
    { id: 'landmate', type: 'engineering', name: 'Landmate Cyborg' },
];

// Map category to RSI path prefix
const CATEGORY_RSI = {
    generic: 'Generic',
    security: 'Security',
    medical: 'Medical',
    engineering: 'Engineering',
    mining: 'Mining',
    janitor: 'Janitor',
    service: 'Service',
};

// ─── Helpers ───────────────────────────────────────────────────────────────

function toPascalCase(str) {
    return str.split('_').map(s => s.charAt(0).toUpperCase() + s.slice(1)).join('');
}

// ─── Generate YAML ─────────────────────────────────────────────────────────

function generateYaml() {
    const subtypeLines = [];
    const entityLines = [];

    for (const entry of ENTRIES) {
        const borgType = BORG_TYPES[entry.type];
        const category = CATEGORY_RSI[entry.type];
        const rsiPath = `ADT/Mobs/Silicon/Borgs/Borg_subtype/${category}/${entry.id}.rsi`;
        const subtypeId = `ss13_${entry.id}`;
        const chassisId = `ADTSs13${toPascalCase(entry.id)}Chassis`;

        // BorgSubtype entry
        subtypeLines.push(`- type: borgSubtype`);
        subtypeLines.push(`  parentBorgType: ${entry.type}`);
        subtypeLines.push(`  id: ${subtypeId}`);
        subtypeLines.push(`  sprite: ${rsiPath}`);
        subtypeLines.push(`  dummyPrototype: ${chassisId}`);
        subtypeLines.push('');

        // Entity entry
        entityLines.push(`- type: entity`);
        entityLines.push(`  id: ${chassisId}`);
        entityLines.push(`  parent: ${borgType.parent}`);
        entityLines.push(`  name: ${entry.name}`);
        entityLines.push(`  components:`);
        entityLines.push(`  - type: BorgSwitchableType`);
        entityLines.push(`    selectedBorgType: ${entry.type}`);
        entityLines.push(`  - type: BorgSwitchableSubtype`);
        entityLines.push(`    borgSubtype: ${subtypeId}`);
        entityLines.push('');
    }

    // Write borg_subtype.yml additions
    const subtypeYaml =
`# SS13 DMI Converted Borg Subtypes
# Auto-generated from SS13 robots.dmi
${subtypeLines.join('\n')}`;

    // Write borg_subtypes.yml additions
    const entityYaml =
`# SS13 DMI Converted Borg Chassis
# Auto-generated from SS13 robots.dmi
${entityLines.join('\n')}`;

    const outDir = 'Resources/Prototypes/ADT/Entities/Mobs/Cyborgs';
    fs.writeFileSync(path.join(outDir, 'borg_subtype_ss13.yml'), subtypeYaml);
    fs.writeFileSync(path.join(outDir, 'borg_subtypes_ss13.yml'), entityYaml);

    console.log('Written:');
    console.log(`  ${outDir}/borg_subtype_ss13.yml`);
    console.log(`  ${outDir}/borg_subtypes_ss13.yml`);
    console.log(`  Total: ${ENTRIES.length} subtypes`);
}

generateYaml();
