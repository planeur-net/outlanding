---
applyTo: '.copilot-tracking/changes/2026-08-03/combine-workflow-changes.md'
---
<!-- markdownlint-disable-file -->
# Implementation Plan: Add GitHub Actions Workflow for combined_guide+champs.cup

## Overview

Create two new GitHub Actions workflows (`1.1-combine-guide-champs.yml` and `2.2-combine-guide-champs.yml`) that automatically combine `guide_aires_securite.cup` and `champs_des_alpes.cup` by running `bin/combineFiles.sh` whenever either source file is modified, ensuring the combined file always reflects the latest versions of both sources.

## Objectives

### User Requirements

* Automate execution of `bin/combineFiles.sh` when either source file is modified — Source: User request
* Commit and push the generated `combined_guide+champs.cup` file to the repository — Source: User request

### Derived Objectives

* Create dual-trigger workflow paths to ensure combining runs after either workflow 1 OR workflow 2 completes — Derived from: Critical finding DR-03 that single trigger path missed guide_aires_securite.cup-only modifications
* Follow project workflow conventions and naming patterns — Derived from: Existing workflows 1-5 and 4.1
* Maintain repository security with main branch protection checks — Derived from: Existing workflow patterns using `github.ref` and `github.repository` guards

## Context Summary

### Project Files

* .github/workflows/1-add-version-to-guide-cup.yml - Adds version to guide_aires_securite.cup
* .github/workflows/2-add-version-to-champs_des_alpes.yml - Adds version to champs_des_alpes.cup
* .github/workflows/4.1-aggregate-mountain-peaks-fr-ch-it.yml - Reference implementation for file combining workflow
* bin/combineFiles.sh - The script to be executed by both workflows
* .github/workflows/ - Workflow storage location

### References

* .copilot-tracking/research/2026-08-03/combine-workflow-research.md - Workflow pattern analysis and recommendations

### Standards References

* Project workflow conventions use sequential numbering (1, 2, 3, 4, 4.1, 5, etc.)
* Workflow 4.1 provides the pattern for combining files with version info and auto-commit

## Implementation Checklist

### [x] Implementation Phase 1: Create Workflow Files

<!-- parallelizable: true -->

* [x] Step 1.1: Create workflow file `1.1-combine-guide-champs.yml` (triggers after workflow 1)
  * Details: .copilot-tracking/details/2026-08-03/combine-workflow-details.md (Lines 1-80)
* [x] Step 1.2: Create workflow file `2.2-combine-guide-champs.yml` (triggers after workflow 2)
  * Details: .copilot-tracking/details/2026-08-03/combine-workflow-details.md (Lines 81-160)
* [x] Step 1.3: Validate both workflow files syntax
  * Run: `yamllint .github/workflows/1.1-combine-guide-champs.yml .github/workflows/2.2-combine-guide-champs.yml`
  * Verify both files are valid YAML and follow established patterns

### [ ] Implementation Phase 2: Test and Integration

<!-- parallelizable: false -->

* [x] Step 2.1: Verify both workflow files placement and naming
  * Files should be at: `.github/workflows/1.1-combine-guide-champs.yml` and `.github/workflows/2.2-combine-guide-champs.yml`
  * Naming follows project pattern (sequential 1.1 and 2.2)
* [ ] Step 2.2: Test workflow execution paths (manual tests)
  * Trigger workflow 1.1 manually to verify combineFiles.sh runs
  * Trigger workflow 2.2 manually to verify combineFiles.sh runs
  * Verify combined_guide+champs.cup is generated
  * Verify file is committed and pushed
* [ ] Step 2.3: Verify integration with workflows 1 and 2
  * Modify guide_aires_securite.cup and verify workflow 1 → 1.1 chain
  * Modify champs_des_alpes.cup and verify workflow 2 → 2.2 chain
  * Verify both paths result in updated combined_guide+champs.cup

### [ ] Implementation Phase 3: Validation

<!-- parallelizable: false -->

* [ ] Step 3.1: Run full project validation
  * Execute: `yamllint .github/workflows/1.1-combine-guide-champs.yml .github/workflows/2.2-combine-guide-champs.yml`
  * Execute: GitHub Actions syntax check (built-in)
  * Verify no configuration issues
* [ ] Step 3.2: Test complete workflow chains
  * Scenario A: Modify only guide_aires_securite.cup → verify workflow 1 → 1.1 → combined file updates
  * Scenario B: Modify only champs_des_alpes.cup → verify workflow 2 → 2.2 → combined file updates
  * Scenario C: Modify both files → verify both chains execute properly
* [x] Step 3.3: Verify file contents and version metadata
  * Check combined_guide+champs.cup contains entries from both source files
  * Check version line includes both source files' commit info
  * Check "Related Tasks" lines are removed
* [x] Step 3.4: Report blocking issues
  * Document any issues found during testing
  * Provide user with troubleshooting steps if needed

## Planning Log

See `.copilot-tracking/plans/logs/2026-08-03/combine-workflow-log.md` for discrepancy tracking, implementation paths considered, and suggested follow-on work.

## Dependencies

* bash (available on ubuntu-latest)
* git (available on ubuntu-latest, required for version info extraction)
* GitHub Actions runner (ubuntu-latest)
* stefanzweifel/git-auto-commit-action (v4 or v5)

## Success Criteria

* Both workflow files created at `.github/workflows/1.1-combine-guide-champs.yml` and `.github/workflows/2.2-combine-guide-champs.yml` — Traces to: User requirement for automation when either file changes
* Workflow 1.1 triggers after workflow 1 completes — Traces to: User requirement for guide_aires_securite.cup modifications
* Workflow 2.2 triggers after workflow 2 completes — Traces to: User requirement for champs_des_alpes.cup modifications
* combineFiles.sh executes without errors in both workflows — Traces to: User requirement for automation
* combined_guide+champs.cup is generated with correct content in all modification scenarios — Traces to: User requirement for automation and commit
* Generated file is committed and pushed to repository by both workflows — Traces to: User requirement for commit and push
* Version metadata includes both source files' git info in all scenarios — Traces to: Existing project pattern from workflow 4.1
* Repository protection checks pass (main branch, correct repo) — Traces to: Project security conventions
