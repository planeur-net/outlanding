import json
import shutil
import subprocess
from pathlib import Path

root = Path.cwd()
val_dir = root / '.copilot-tracking' / 'tmp' / 'phase3-validation'
if val_dir.exists():
    shutil.rmtree(val_dir)
(val_dir / 'bin').mkdir(parents=True, exist_ok=True)
shutil.copy2(root / 'guide_aires_securite.cup', val_dir / 'guide_aires_securite.cup')
shutil.copy2(root / 'champs_des_alpes.cup', val_dir / 'champs_des_alpes.cup')
shutil.copy2(root / 'bin' / 'combineFiles.sh', val_dir / 'bin' / 'combineFiles.sh')

# Ensure shell script has LF endings for WSL/bash execution.
script_path = val_dir / 'bin' / 'combineFiles.sh'
script_text = script_path.read_text(encoding='utf-8', errors='replace')
with script_path.open('w', encoding='utf-8', newline='\n') as script_file:
    script_file.write(script_text.replace('\r\n', '\n'))


def run(cmd: str, cwd: Path):
    return subprocess.run(cmd, cwd=cwd, shell=True, text=True, capture_output=True, check=True)

run('git init', val_dir)
run('git config user.email phase3-validation@example.local', val_dir)
run('git config user.name "Phase3 Validator"', val_dir)
run('git config commit.gpgsign false', val_dir)
run('git add .', val_dir)
run('git commit -m "baseline for phase3 validation"', val_dir)

wsl_dir = str(val_dir).replace('\\', '/').replace('C:', '/mnt/c')


def append_line(path: Path, line: str):
    with path.open('a', encoding='utf-8', newline='') as f:
        f.write('\n' + line)


def run_scenario(name: str, mod_guide: bool, mod_champs: bool):
    guide = val_dir / 'guide_aires_securite.cup'
    champs = val_dir / 'champs_des_alpes.cup'

    if mod_guide:
        append_line(guide, f'PHASE3_{name}_GUIDE,PHASE3_{name}_GUIDE,UT,45:00:00N,007:00:00E,1000m,1,1000m,0m,0m,0m,0m,,')
        append_line(guide, '-----Related Tasks-----')
    if mod_champs:
        append_line(champs, f'PHASE3_{name}_CHAMPS,PHASE3_{name}_CHAMPS,UT,46:00:00N,008:00:00E,1000m,1,1000m,0m,0m,0m,0m,,')
        append_line(champs, '-----Related Tasks-----')

    run('git add guide_aires_securite.cup champs_des_alpes.cup', val_dir)
    run(f'git commit -m "scenario {name} source changes"', val_dir)

    run(f'bash -lc "cd {wsl_dir} && bash ./bin/combineFiles.sh"', val_dir)

    guide_version = run('git log --pretty="%h %cI" -n1 -- guide_aires_securite.cup', val_dir).stdout.strip()
    champs_version = run('git log --pretty="%h %cI" -n1 -- champs_des_alpes.cup', val_dir).stdout.strip()

    combined = (val_dir / 'combined_guide+champs.cup').read_text(encoding='utf-8', errors='replace')
    lines = combined.splitlines()
    version_line = lines[1] if len(lines) >= 2 else ''

    return {
        'scenario': name,
        'combinedExists': (val_dir / 'combined_guide+champs.cup').exists(),
        'versionLine': version_line,
        'versionHasGuide': f'[guide_aires_securite]{guide_version}' in version_line,
        'versionHasChamps': f'[champs_des_alpes]{champs_version}' in version_line,
        'containsGuideMarker': (f'PHASE3_{name}_GUIDE' in combined),
        'containsChampsMarker': (f'PHASE3_{name}_CHAMPS' in combined),
        'containsRelatedTasks': ('-----Related Tasks-----' in combined),
        'lineCount': len(lines)
    }

results = [
    run_scenario('A', True, False),
    run_scenario('B', False, True),
    run_scenario('C', True, True),
]

out = {
    'validationRepo': str(val_dir),
    'results': results,
}

print(json.dumps(out, ensure_ascii=True, indent=2))
