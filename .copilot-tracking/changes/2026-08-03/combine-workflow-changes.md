<!-- markdownlint-disable-file -->
# Release Changes: Add GitHub Actions Workflow for combined_guide+champs.cup

**Related Plan**: combine-workflow-plan.instructions.md
**Implementation Date**: 2026-08-03

## Summary

Implemented dual combine workflows so the combined file can be regenerated after either parent versioning workflow. Local validation and static integration checks are complete; GitHub-hosted runtime chain validation is pending remote execution.

## Changes

### Added

* .github/workflows/1.1-combine-guide-champs.yml - New workflow triggered after workflow 1 completion to run combineFiles.sh, auto-commit combined_guide+champs.cup, and dispatch pages update event
* .github/workflows/2.2-combine-guide-champs.yml - New workflow triggered after workflow 2 completion to run combineFiles.sh, auto-commit combined_guide+champs.cup, and dispatch pages update event

### Modified

* combined_guide+champs.cup - Regenerated during local execution-path verification of combineFiles.sh

### Removed

* Temporary local probe artifacts removed after validation: part1.cup and combined_guide+champs.cup

## Additional or Deviating Changes

* Local YAML validation used fallback parser checks instead of yamllint
	* yamllint is not installed in the current local environment
* Full runtime validation remains blocked for remote-only steps
	* GitHub Actions workflow_run and workflow_dispatch execution cannot be confirmed from this local environment alone
* Local combine script verification surfaced line-ending sensitivity on Windows
	* bin/combineFiles.sh is designed for bash/Linux execution and showed CRLF-related behavior during local-only simulation

## Release Summary

Phase completion status:

* Phase 1: Complete
* Phase 2: Partial (runtime workflow chain execution requires GitHub-hosted runs)
* Phase 3: Partial (remote validation still required)

Files affected:

* Added: .github/workflows/1.1-combine-guide-champs.yml, .github/workflows/2.2-combine-guide-champs.yml
* Modified: combined_guide+champs.cup
* Removed: temporary local probe artifacts created during validation

Validation summary:

* Workflow files are present at expected paths and parse successfully with local YAML parser checks
* Static trigger wiring between workflows 1/2 and 1.1/2.2 is aligned by exact workflow names
* Remaining required checks (manual dispatch, workflow_run chain execution, GitHub built-in syntax/result checks) must be completed in GitHub Actions
