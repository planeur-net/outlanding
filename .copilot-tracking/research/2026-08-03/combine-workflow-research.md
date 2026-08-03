<!-- markdownlint-disable-file -->
# Research: Combine Workflow for combined_guide+champs.cup

## Scope
Create a GitHub Actions workflow to automate running `bin/combineFiles.sh` when either source file (`guide_aires_securite.cup` or `champs_des_alpes.cup`) is modified, and commit the generated file back to the repository.

## Key Findings

### Existing Workflow Patterns
- **Workflow 1**: Adds version to `guide_aires_securite.cup` on push (workflow: `1-add-version-to-guide-cup.yml`)
- **Workflow 2**: Adds version to `champs_des_alpes.cup` on push (workflow: `2-add-version-to-champs_des_alpes.yml`)
- **Workflow 4.1**: Aggregates three mountain peak files by combining them (workflow: `4.1-aggregate-mountain-peaks-fr-ch-it.yml`)
  - Triggered by `workflow_run` when workflow 4 completes
  - Uses `sed` for file manipulation
  - Adds version info combining git commit data
  - Commits and pushes results using `stefanzweifel/git-auto-commit-action@v5`

### combineFiles.sh Script
- Located: `bin/combineFiles.sh`
- Purpose: Combines `guide_aires_securite.cup` and `champs_des_alpes.cup` into `combined_guide+champs.cup`
- Operations:
  - Removes "Related Tasks" lines from both files
  - Removes existing version info
  - Removes header from second file
  - Concatenates files
  - Adds combined version metadata with git commit info
  - Cleans up temporary files

### Trigger Points
Two approaches possible:
1. **Direct trigger**: On push when either `guide_aires_securite.cup` or `champs_des_alpes.cup` changes
2. **Chained trigger**: After workflow 2 (champs_des_alpes versioning) completes

### Commit Strategy
- Use `stefanzweifel/git-auto-commit-action` (v4 or v5) for committing results
- Commit file pattern: `combined_guide+champs.cup`
- Commit message: Should reference the source files modified

### Repository Conventions
- Workflow files numbered sequentially and stored in `.github/workflows/`
- Main branch protection: `if: github.ref == 'refs/heads/main' && github.repository == 'planeur-net/outlanding'`
- Workflow dispatch enabled for manual testing
- Cleanup step after combining files

## Known Constraints
- The script requires bash (uses sed, git, cat commands)
- Must run on `ubuntu-latest` (bash/sed availability)
- Git operations require full checkout history (already done by default)
- Version info extraction depends on git log history

## Recommended Approach
Chain the workflow after workflow 2 (champs_des_alpes versioning) using `workflow_run` trigger. This ensures both source files have their version info added before combining. Workflow numbering: `2.2-combine-guide-champs.yml`.
