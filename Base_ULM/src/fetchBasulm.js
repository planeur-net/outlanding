/**
 * Fetch all ULM terrains from basulm.ffplum.fr API and write to a .cup file.
 * Usage: node src/fetchBasulm.js [output.cup]
 *
 * API doc: https://www.basulm.ffplum.fr/clefs-de-l-api.html
 */

'use strict';

const https = require('https');
const http  = require('http');
const fs = require('fs');
const path = require('path');
const zlib = require('zlib');
const poppler = require('pdf-poppler');

const PDF_BASE_URL = 'http://basulm.ffplum.info/PDF';
const WORKSPACE_DIR = path.join(__dirname, '..', '..');
const BASE_ULM_DIR = path.join(WORKSPACE_DIR, 'Base_ULM');
const PDF_DIR = path.join(BASE_ULM_DIR, 'pdf');
const PICS_DIR = path.join(BASE_ULM_DIR, 'Pics');
const CUPX_FILE = path.join(WORKSPACE_DIR, 'basulm_terrains.cupx');
const GUIDE_CUP_FILE = path.join(WORKSPACE_DIR, 'guide_aires_securite.cup');

const API_URL = 'https://basulm.ffplum.fr/getbasulm/get/basulm/listall';
const API_KEY = 'VCAHQ8XSZB5FWG7Z0TW2';
const OUTPUT_FILE = process.argv[2] || path.join(WORKSPACE_DIR, 'basulm_terrains.cup');

const CRC32_TABLE = new Uint32Array(256).map((_, index) => {
  let value = index;
  for (let bit = 0; bit < 8; bit++) {
    value = (value & 1) !== 0 ? (0xEDB88320 ^ (value >>> 1)) : (value >>> 1);
  }
  return value >>> 0;
});

// Convert API lat/lon string (e.g. "N 48 56 10") to CUP format (e.g. "4856.167N")
function convertCoord(str, isLon) {
  // Format: "[N|S|E|W] DDD MM SS"
  const parts = str.trim().split(/\s+/);
  const hemi = parts[0];         // N, S, E, W
  const deg  = parseInt(parts[1], 10);
  const min  = parseInt(parts[2], 10);
  const sec  = parseInt(parts[3], 10);

  const decMin = (min + sec / 60).toFixed(3);
  // Pad degrees: 2 digits for lat, 3 for lon
  const degStr = isLon
    ? String(deg).padStart(3, '0')
    : String(deg).padStart(2, '0');
  // Pad minutes to MM.mmm
  const minStr = decMin.padStart(6, '0');

  return `${degStr}${minStr}${hemi}`;
}

// Convert altitude string "335 ft" or "102 m" to metres string "102.1m"
function convertAltitude(altStr) {
  const match = altStr.trim().match(/^([\d.]+)\s*(ft|m)$/i);
  if (!match) return '';
  const value = parseFloat(match[1]);
  const unit  = match[2].toLowerCase();
  const metres = unit === 'ft' ? value * 0.3048 : value;
  return `${Math.round(metres)}m`;
}

// Map nature_piste to CUP style value
// CUP style: 1=unknown, 2=grass, 3=outlanding, 4=glidersite, 5=airfield (hard)
function surfaceStyle(nature) {
  if (!nature) return '2';
  const n = nature.toLowerCase();
  if (n === 'dur' || n === 'bitume' || n === 'beton') return '5'; // hard surface
  if (n === 'eau')   return '1';
  return '2'; // herbe, terre, neige, default → grass
}

// Convert runway length/width in metres to CUP format (e.g. "800.0m")
function rwMetres(val) {
  if (!val || val === '') return '';
  const n = parseFloat(val);
  if (isNaN(n) || n === 0) return '';
  return `${Math.round(n)}m`;
}

function asNumber(val) {
  if (val === null || val === undefined || val === '') return null;
  const n = parseFloat(val);
  return Number.isNaN(n) ? null : n;
}

// Derive runway direction (degrees) from orientation fields
// Perl logic: prefer orientation_pref_1 if it's a QFU (≤99), else use orientation_piste_1
// QFU is in tens (e.g. 27 = 270°), degrees as-is if >99
function rwDir(prefStr, pisteStr) {
  const clean = s => (s || '').trim().replace(/^0+(\d)/, '$1').split('-')[0].split('/')[0];
  let v = clean(prefStr);
  if (v !== '' && v !== 'omnidir' && v !== 'Inconnue') {
    const n = parseInt(v, 10);
    if (!isNaN(n)) return n <= 99 ? String(n * 10) : String(n);
  }
  v = clean(pisteStr);
  if (v !== '' && v !== 'omnidir' && v !== 'Inconnue') {
    const n = parseInt(v, 10);
    if (!isNaN(n)) return n <= 99 ? String(n * 10) : String(n);
  }
  return '';
}

// Map pays (French country name) to ISO 2-letter code
const PAYS_MAP = {
  'france':         'FR',
  'espagne':        'ES',
  'italie':         'IT',
  'suisse':         'CH',
  'allemagne':      'DE',
  'belgique':       'BE',
  'luxembourg':     'LU',
  'andorre':        'AD',
  'monaco':         'MC',
  'portugal':       'PT',
  'royaume-uni':    'GB',
  'pays-bas':       'NL',
  'autriche':       'AT',
};

function countryCode(pays) {
  return PAYS_MAP[pays.toLowerCase()] || 'FR';
}

function isOaciCode(code) {
  return /^[A-Za-z]{4}$/.test((code || '').trim());
}

function isAltisurface(desc) {
  return /altisurface/i.test((desc || '').trim());
}

function normalizeText(value) {
  return (value || '')
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '');
}

function isPermanentlyClosed(desc) {
  const normalized = normalizeText(desc);
  return normalized.includes('ferme definitivement')
    || normalized.includes('fermee definitivement')
    || normalized.includes('fermee def');
}

function hasRunwayDimensionInfo(rwlenNum, rwwidthNum) {
  return rwlenNum !== null || rwwidthNum !== null;
}

function isSurfaceAllowedForOutlanding(nature) {
  const normalized = normalizeText(nature).trim();
  return normalized === 'dur'
    || normalized === 'bitume'
    || normalized === 'beton'
    || normalized === 'herbe'
    || normalized === 'terre';
}

function isMainlandCodePattern(code) {
  return /^LF[0-9A-Z]+$/i.test((code || '').trim());
}

function normalizeForMatch(value) {
  return normalizeText(value)
    .replace(/[^a-z0-9 ]/g, ' ')
    .replace(/\s+/g, ' ')
    .trim();
}

function parseCsvLine(line) {
  const fields = [];
  let current = '';
  let inQuotes = false;

  for (let i = 0; i < line.length; i++) {
    const ch = line[i];
    if (ch === '"') {
      if (inQuotes && line[i + 1] === '"') {
        current += '"';
        i++;
      } else {
        inQuotes = !inQuotes;
      }
    } else if (ch === ',' && !inQuotes) {
      fields.push(current);
      current = '';
    } else {
      current += ch;
    }
  }
  fields.push(current);
  return fields;
}

function loadGuideIndex(filePath) {
  if (!fs.existsSync(filePath)) {
    console.warn(`Guide file not found: ${filePath}. Duplicate filter against guide will be skipped.`);
    return [];
  }

  const lines = fs.readFileSync(filePath, 'utf8').split(/\r?\n/);
  const entries = [];

  for (const line of lines) {
    if (!line || line.startsWith('name,code,')) continue;
    if (line.startsWith('"version="')) continue;

    const fields = parseCsvLine(line);
    if (fields.length < 12) continue;

    const name = (fields[0] || '').trim();
    const code = (fields[1] || '').trim();
    const desc = (fields[11] || '').trim();
    if (!name && !code && !desc) continue;

    entries.push({
      nameNorm: normalizeForMatch(name),
      codeNorm: normalizeForMatch(code),
      descNorm: normalizeForMatch(desc),
    });
  }

  console.log(`Loaded ${entries.length} guide entries from ${path.basename(filePath)}.`);
  return entries;
}

function isAlreadyInGuide(name, code, guideEntries) {
  if (!guideEntries.length) return false;

  const basulmNameNorm = normalizeForMatch(name);
  const basulmCodeNorm = normalizeForMatch(code);

  for (const guide of guideEntries) {
    if (guide.descNorm && basulmNameNorm && guide.descNorm.includes(basulmNameNorm)) return true;
    if (guide.descNorm && basulmCodeNorm && guide.descNorm.includes(basulmCodeNorm)) return true;
    if (guide.nameNorm && basulmNameNorm && guide.nameNorm.includes(basulmNameNorm)) return true;
    if (guide.nameNorm && basulmCodeNorm && guide.nameNorm.includes(basulmCodeNorm)) return true;
  }

  return false;
}

// Escape a field for CSV (quote if it contains comma, quote, or newline)
function csvField(val) {
  if (val === null || val === undefined) return '';
  const s = String(val);
  if (s.includes(',') || s.includes('"') || s.includes('\n')) {
    return '"' + s.replace(/"/g, '""') + '"';
  }
  return s;
}

function quotedCsvField(val) {
  if (val === null || val === undefined || val === '') return '';
  return '"' + String(val).replace(/"/g, '""') + '"';
}

function crc32(buffer) {
  let value = 0xFFFFFFFF;
  for (const byte of buffer) {
    value = CRC32_TABLE[(value ^ byte) & 0xFF] ^ (value >>> 8);
  }
  return (value ^ 0xFFFFFFFF) >>> 0;
}

function dosDateTime(date) {
  const year = Math.max(date.getFullYear(), 1980);
  const month = date.getMonth() + 1;
  const day = date.getDate();
  const hours = date.getHours();
  const minutes = date.getMinutes();
  const seconds = Math.floor(date.getSeconds() / 2);

  return {
    time: (hours << 11) | (minutes << 5) | seconds,
    date: ((year - 1980) << 9) | (month << 5) | day,
  };
}

function zipExtraFields(mtime, atime) {
  const modified = Math.floor(mtime.getTime() / 1000);
  const accessed = Math.floor((atime || mtime).getTime() / 1000);

  const localTimestamp = Buffer.alloc(13);
  localTimestamp.writeUInt16LE(0x5455, 0);
  localTimestamp.writeUInt16LE(9, 2);
  localTimestamp.writeUInt8(0x03, 4);
  localTimestamp.writeUInt32LE(modified, 5);
  localTimestamp.writeUInt32LE(accessed, 9);

  const centralTimestamp = Buffer.alloc(9);
  centralTimestamp.writeUInt16LE(0x5455, 0);
  centralTimestamp.writeUInt16LE(5, 2);
  centralTimestamp.writeUInt8(0x03, 4);
  centralTimestamp.writeUInt32LE(modified, 5);

  const unixData = Buffer.alloc(15);
  unixData.writeUInt16LE(0x7875, 0);
  unixData.writeUInt16LE(11, 2);
  unixData.writeUInt8(1, 4);
  unixData.writeUInt8(4, 5);
  unixData.writeUInt32LE(1000, 6);
  unixData.writeUInt8(4, 10);
  unixData.writeUInt32LE(1000, 11);

  return {
    local: Buffer.concat([localTimestamp, unixData]),
    central: Buffer.concat([centralTimestamp, unixData]),
  };
}

function buildZip(entries) {
  const localParts = [];
  const centralParts = [];
  let offset = 0;

  for (const entry of entries) {
    const name = entry.name.replace(/\\/g, '/');
    const nameBuffer = Buffer.from(name, 'utf8');
    const timestamp = dosDateTime(entry.mtime || new Date());
    const isDirectory = Boolean(entry.directory);
    const rawData = isDirectory ? Buffer.alloc(0) : entry.data;
    const compressedData = isDirectory ? Buffer.alloc(0) : zlib.deflateRawSync(rawData);
    const checksum = isDirectory ? 0 : crc32(rawData);
    const compressedSize = compressedData.length;
    const uncompressedSize = rawData.length;
    const versionNeeded = isDirectory ? 10 : 20;
    const externalAttributes = isDirectory ? 0x41FF0010 : 0x81FF0000;
    const extraFields = zipExtraFields(entry.mtime || new Date(), entry.atime || entry.mtime || new Date());

    const localHeader = Buffer.alloc(30);
    localHeader.writeUInt32LE(0x04034b50, 0);
    localHeader.writeUInt16LE(versionNeeded, 4);
    localHeader.writeUInt16LE(0, 6);
    localHeader.writeUInt16LE(isDirectory ? 0 : 8, 8);
    localHeader.writeUInt16LE(timestamp.time, 10);
    localHeader.writeUInt16LE(timestamp.date, 12);
    localHeader.writeUInt32LE(checksum, 14);
    localHeader.writeUInt32LE(compressedSize, 18);
    localHeader.writeUInt32LE(uncompressedSize, 22);
    localHeader.writeUInt16LE(nameBuffer.length, 26);
    localHeader.writeUInt16LE(extraFields.local.length, 28);

    localParts.push(localHeader, nameBuffer, extraFields.local, compressedData);

    const centralHeader = Buffer.alloc(46);
    centralHeader.writeUInt32LE(0x02014b50, 0);
    centralHeader.writeUInt16LE(798, 4);
    centralHeader.writeUInt16LE(versionNeeded, 6);
    centralHeader.writeUInt16LE(0, 8);
    centralHeader.writeUInt16LE(isDirectory ? 0 : 8, 10);
    centralHeader.writeUInt16LE(timestamp.time, 12);
    centralHeader.writeUInt16LE(timestamp.date, 14);
    centralHeader.writeUInt32LE(checksum, 16);
    centralHeader.writeUInt32LE(compressedSize, 20);
    centralHeader.writeUInt32LE(uncompressedSize, 24);
    centralHeader.writeUInt16LE(nameBuffer.length, 28);
    centralHeader.writeUInt16LE(extraFields.central.length, 30);
    centralHeader.writeUInt16LE(0, 32);
    centralHeader.writeUInt16LE(0, 34);
    centralHeader.writeUInt16LE(0, 36);
    centralHeader.writeUInt32LE(externalAttributes, 38);
    centralHeader.writeUInt32LE(offset, 42);

    centralParts.push(centralHeader, nameBuffer, extraFields.central);
    offset += localHeader.length + nameBuffer.length + extraFields.local.length + compressedData.length;
  }

  const centralOffset = offset;
  const centralSize = centralParts.reduce((total, part) => total + part.length, 0);
  const eocd = Buffer.alloc(22);
  eocd.writeUInt32LE(0x06054b50, 0);
  eocd.writeUInt16LE(0, 4);
  eocd.writeUInt16LE(0, 6);
  eocd.writeUInt16LE(entries.length, 8);
  eocd.writeUInt16LE(entries.length, 10);
  eocd.writeUInt32LE(centralSize, 12);
  eocd.writeUInt32LE(centralOffset, 16);
  eocd.writeUInt16LE(0, 20);

  return Buffer.concat([...localParts, ...centralParts, eocd]);
}

function fetchJSON(url, apiKey) {
  return new Promise((resolve, reject) => {
    const options = {
      headers: { 'Authorization': `api_key ${apiKey}` },
    };
    https.get(url, options, (res) => {
      if (res.statusCode !== 200) {
        reject(new Error(`HTTP ${res.statusCode}`));
        res.resume();
        return;
      }
      let data = '';
      res.on('data', chunk => { data += chunk; });
      res.on('end', () => {
        try { resolve(JSON.parse(data)); }
        catch (e) { reject(e); }
      });
    }).on('error', reject);
  });
}

function writeCupFile(versionLabel, waypoints) {
  const lines = [];

  lines.push('name,code,country,lat,lon,elev,style,rwdir,rwlen,rwwidth,freq,desc,userdata,pics');
  lines.push(`"version=",4.2,,,,,,,,,,"${versionLabel}",,`);

  for (const waypoint of waypoints) {
    const picsFile = fs.existsSync(path.join(PICS_DIR, `${waypoint.code}.jpg`))
      ? `${waypoint.code}.jpg`
      : '';

    const row = [
      csvField(waypoint.name),
      csvField(waypoint.code),
      waypoint.country,
      waypoint.lat,
      waypoint.lon,
      waypoint.elev,
      waypoint.style,
      waypoint.rwdir,
      waypoint.rwlen,
      waypoint.rwwidth,
      waypoint.freq,
      csvField(waypoint.desc),
      '',
      quotedCsvField(picsFile),
    ].join(',');

    lines.push(row);
  }

  fs.writeFileSync(OUTPUT_FILE, lines.join('\r\n'), 'utf8');
  console.log(`Written ${waypoints.length} waypoints to: ${OUTPUT_FILE}`);
}

async function writeJpgFiles(codes) {
  fs.mkdirSync(PICS_DIR, { recursive: true });

  const keptCodeSet = new Set(codes);
  let removed = 0;
  for (const fileName of fs.readdirSync(PICS_DIR)) {
    if (path.extname(fileName).toLowerCase() !== '.jpg') continue;
    const code = path.basename(fileName, '.jpg');
    if (!keptCodeSet.has(code)) {
      fs.unlinkSync(path.join(PICS_DIR, fileName));
      removed++;
    }
  }
  console.log(`Removed ${removed} stale JPG files.`);

  let converted = 0;
  let skipped = 0;
  let failed = 0;

  for (const code of codes) {
    const pdfPath = path.join(PDF_DIR, `${code}.pdf`);
    if (!fs.existsSync(pdfPath)) {
      continue;
    }

    const jpgPath = path.join(PICS_DIR, `${code}.jpg`);
    if (fs.existsSync(jpgPath)) {
      const pdfStat = fs.statSync(pdfPath);
      const jpgStat = fs.statSync(jpgPath);
      if (jpgStat.mtimeMs >= pdfStat.mtimeMs) {
        skipped++;
        continue;
      }
    }

    const tempJpgPath = path.join(PICS_DIR, `${code}-1.jpg`);
    if (fs.existsSync(tempJpgPath)) {
      fs.unlinkSync(tempJpgPath);
    }

    try {
      await poppler.convert(pdfPath, {
        format: 'jpeg',
        out_dir: PICS_DIR,
        out_prefix: code,
        page: 1,
      });

      if (fs.existsSync(jpgPath)) {
        fs.unlinkSync(jpgPath);
      }
      fs.renameSync(tempJpgPath, jpgPath);
      converted++;
      process.stdout.write(`\rJPGs: ${converted} converted, ${skipped} skipped, ${failed} failed   `);
    } catch (e) {
      if (fs.existsSync(tempJpgPath)) {
        fs.unlinkSync(tempJpgPath);
      }
      failed++;
    }
  }

  console.log(`\nJPGs complete: ${converted} converted, ${skipped} already existed, ${failed} failed.`);
}

async function writeCupxFile() {
  const fileNames = fs.readdirSync(PICS_DIR)
    .filter(fileName => path.extname(fileName).toLowerCase() === '.jpg')
    .sort((left, right) => left.localeCompare(right));

  const picsZip = buildZip([
    {
      name: 'pics/',
      directory: true,
      mtime: fs.statSync(PICS_DIR).mtime,
      atime: fs.statSync(PICS_DIR).atime,
    },
    ...fileNames.map(fileName => ({
      name: path.posix.join('pics', fileName),
      data: fs.readFileSync(path.join(PICS_DIR, fileName)),
      mtime: fs.statSync(path.join(PICS_DIR, fileName)).mtime,
      atime: fs.statSync(path.join(PICS_DIR, fileName)).atime,
    })),
  ]);

  const pointsZip = buildZip([
    {
      name: 'POINTS.cup',
      data: fs.readFileSync(OUTPUT_FILE),
      mtime: fs.statSync(OUTPUT_FILE).mtime,
      atime: fs.statSync(OUTPUT_FILE).atime,
    },
  ]);

  fs.writeFileSync(CUPX_FILE, Buffer.concat([picsZip, pointsZip]));
  console.log(`Written CUPX file to: ${CUPX_FILE}`);
}
async function main() {
  console.log(`Fetching terrain list from basulm API...`);
  const json = await fetchJSON(API_URL, API_KEY);

  if (json.status !== 'ok') {
    console.error('API returned status:', json.status);
    process.exit(1);
  }

  const terrains = json.liste;
  console.log(`Received ${terrains.length} terrains (API count: ${json.count})`);

  const now = new Date().toISOString().slice(0, 19).replace('T', ' ');
  const versionLabel = `basulm ${now}`;
  const guideEntries = loadGuideIndex(GUIDE_CUP_FILE);
  const waypoints = [];
  const keptCodes = [];

  let kept = 0;

  for (const t of terrains) {
    const name    = t.toponyme || '';
    const code    = t.code_terrain || '';
    const desc    = t.type_terrain || '';
    const nature  = t.nature_piste_1 || '';
    if (!isMainlandCodePattern(code)) {
      continue;
    }
    if (!isSurfaceAllowedForOutlanding(nature)) {
      continue;
    }
    if (isOaciCode(code)) {
      continue;
    }
    if (isAltisurface(desc)) {
      continue;
    }
    if (isPermanentlyClosed(desc)) {
      continue;
    }
    if (isAlreadyInGuide(name, code, guideEntries)) {
      continue;
    }
    const country = countryCode(t.pays || 'France');
    const lat     = convertCoord(t.latitude, false);
    const lon     = convertCoord(t.longitude, true);
    const elev    = convertAltitude(t.altitude);
    const style   = surfaceStyle(t.nature_piste_1);
    const rwdir   = rwDir(t.orientation_pref_1, t.orientation_piste_1);
    const rwlenNum = asNumber(t.longueur_piste_1);
    const rwwidthNum = asNumber(t.largeur_piste_1);
    if (!hasRunwayDimensionInfo(rwlenNum, rwwidthNum)) {
      continue;
    }
    if ((rwlenNum !== null && rwlenNum < 300) || (rwwidthNum !== null && rwwidthNum < 30)) {
      continue;
    }
    const rwlen   = rwMetres(t.longueur_piste_1);
    const rwwidth = rwMetres(t.largeur_piste_1);
    const freqMatch = (t.radio || '').replace(',', '.').match(/\d{2,3}\.\d+/);
    const freq    = freqMatch ? freqMatch[0] : '';
    waypoints.push({
      name,
      code,
      country,
      lat,
      lon,
      elev,
      style,
      rwdir,
      rwlen,
      rwwidth,
      freq,
      desc,
    });
    keptCodes.push(code);
    kept++;
  }

  writeCupFile(versionLabel, waypoints);

  // Download PDFs for each terrain
  fs.mkdirSync(PDF_DIR, { recursive: true });
  const keptCodeSet = new Set(keptCodes);
  let removed = 0;
  for (const fileName of fs.readdirSync(PDF_DIR)) {
    if (path.extname(fileName).toLowerCase() !== '.pdf') continue;
    const code = path.basename(fileName, '.pdf');
    if (!keptCodeSet.has(code)) {
      fs.unlinkSync(path.join(PDF_DIR, fileName));
      removed++;
    }
  }
  console.log(`Removed ${removed} stale PDF files.`);

  let downloaded = 0, skipped = 0, failed = 0;

  for (const code of keptCodes) {
    const dest = path.join(PDF_DIR, `${code}.pdf`);
    if (fs.existsSync(dest)) {
      skipped++;
      continue;
    }
    const url = `${PDF_BASE_URL}/${code}.pdf`;
    try {
      await downloadFile(url, dest);
      downloaded++;
      process.stdout.write(`\rPDFs: ${downloaded} downloaded, ${skipped} skipped, ${failed} not found   `);
    } catch (e) {
      if (fs.existsSync(dest)) fs.unlinkSync(dest); // remove partial/non-pdf file
      if (e.message !== 'not-pdf') failed++;
    }
  }
  console.log(`\nPDFs complete: ${downloaded} downloaded, ${skipped} already existed, ${failed} not found.`);

  await writeJpgFiles(keptCodes);
  writeCupFile(versionLabel, waypoints);
  await writeCupxFile();
}

function downloadFile(url, dest) {
  return new Promise((resolve, reject) => {
    const mod = url.startsWith('https') ? https : http;
    const file = fs.createWriteStream(dest);
    mod.get(url, res => {
      if (res.statusCode === 301 || res.statusCode === 302) {
        file.destroy();
        return downloadFile(res.headers.location, dest).then(resolve).catch(reject);
      }
      if (res.statusCode !== 200) {
        file.destroy();
        return reject(new Error(`HTTP ${res.statusCode}`));
      }
      const ct = res.headers['content-type'] || '';
      if (!ct.includes('application/pdf')) {
        file.destroy();
        res.resume(); // drain to free socket
        return reject(new Error('not-pdf'));
      }
      res.pipe(file);
      file.on('finish', () => file.close(resolve));
      file.on('error', reject);
    }).on('error', reject);
  });
}

main().catch(err => {
  console.error('Error:', err.message);
  process.exit(1);
});
