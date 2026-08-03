<!-- markdownlint-disable-file -->
# Planning Log: Add GitHub Actions Workflow for combined_guide+champs.cup

## Discrepancy Log

### Unaddressed Research Items

* DR-03: Incomplete Dual-Source Trigger Coverage
  * Source: Plan Validator findings (2026-08-03)
  * Issue: Plan only triggers combining after workflow 2, missing updates when guide_aires_securite.cup changes alone
  * Severity: CRITICAL — Violates user requirement "when either source file is modified"
  * Resolution: Add workflow 1.1 to trigger combining after workflow 1 completes
  * Impact: Requires updating plan to include dual trigger paths (after workflow 1 AND after workflow 2)

* DR-01: Manual vs. Direct Trigger Trade-off
  * Source: .copilot-tracking/research/2026-08-03/combine-workflow-research.md (Lines 20-26)
  * Reason: Chained trigger after workflow 2 selected as optimal approach; direct trigger not implemented
  * Rationale: Ensures version info is added to both files before combining; follows project pattern of workflow chaining
  * Impact: low

* DR-02: Bash Script Cross-Platform Support
  * Source: .copilot-tracking/research/2026-08-03/combine-workflow-research.md (Lines 32-35)
  * Reason: Script uses bash/sed/git which are Linux-specific; Windows support not addressed
  * Rationale: Project uses GitHub Actions on ubuntu-latest; local execution on Windows would require different handling
  * Impact: low (workflow automation targets Linux environment only)

* DR-03: Incomplete Dual-Source Trigger Coverage
  * Source: .copilot-tracking/research/2026-08-03/combine-workflow-research.md (Line 3: "when either source file is modified")
  * Reason: Plan implements workflow 2.2 trigger only after workflow 2 completes; does not trigger after workflow 1
  * Rationale: User requirement specifies automation "when either source file is modified"; current plan only addresses champs_des_alpes.cup modifications (through workflow 2)
  * Impact: critical - Violates user requirement for guide_aires_securite.cup-only modifications
  * Details: If guide_aires_securite.cup is modified without changes to champs_des_alpes.cup:
    - Workflow 1 executes (adds version to guide_aires_securite.cup) ✓
    - Workflow 2 does NOT execute (no trigger, as champs_des_alpes.cup unchanged) 
    - Workflow 2.2 does NOT trigger (depends only on workflow 2 completion) ✗
    - combined_guide+champs.cup remains stale with old champs_des_alpes content ✗
  * Remediation options:
    1. Add workflow 1.1 that triggers after workflow 1, combining immediately with latest guide_aires_securite.cup
    2. Modify workflow 2.2 trigger to activate after workflow 1 OR workflow 2 completes (requires workflow_run with multiple workflow names or push-based trigger on combined_guide+champs.cup)
    3. Accept partial coverage and document that combined file only updates when champs_des_alpes.cup changes

### Plan Deviations from Research

* DD-01: Workflow Numbering Scheme
  * Research recommends: `2.2-combine-guide-champs.yml` per project pattern
  * Plan implements: Same as research (2.2 numbering)
  * Rationale: No deviation; plan follows research recommendation exactly

* DD-02: Auto-commit Action Version
  * Research found: Workflow 4.1 uses v5; Workflow 2 uses v4
  * Plan implements: v5 (stefanzweifel/git-auto-commit-action@v5)
  * Rationale: v5 is more recent and used in the primary reference implementation (4.1); consistent with current best practice

## Implementation Paths Considered

### Selected: Dual Chained Workflows (1.1 after workflow 1, 2.2 after workflow 2)

* Approach: Create two separate workflows that each trigger after their respective parent workflows. Workflow 1.1 triggers after workflow 1 (guide_aires_securite versioning) and workflow 2.2 triggers after workflow 2 (champs_des_alpes versioning). Both run the same combineFiles.sh script, ensuring combined file updates whenever either source file changes.
* Rationale:
  - Ensures version information is present in both files before combining (each workflow runs after its respective file has been versioned)
  - Provides complete coverage: guide_aires_securite-only changes trigger 1.1, champs_des_alpes-only changes trigger 2.2, both-file changes trigger both
  - Follows established project pattern (workflow 4.1 demonstrates similar chaining, albeit for single trigger)
  - Automatic execution reduces manual steps
  - Integrated with GitHub Actions native workflow orchestration
* Evidence: .github/workflows/4.1-aggregate-mountain-peaks-fr-ch-it.yml uses workflow_run chaining pattern; user requirement explicitly requires "when either source file is modified"
* Trade-offs: Two workflows instead of one (slightly more operational overhead); potential minor delay between first and second workflow completion when both files modified simultaneously (acceptable given requirement satisfaction)

### IP-01: Single Workflow with Dual Triggers (Not technically feasible)

* Approach: Create one workflow that responds to workflow_run from workflow 1 OR workflow 2
* Benefit: Single workflow to maintain; unified combining logic
* Drawback: GitHub Actions workflow_run does not support OR logic across multiple workflows natively; would require hybrid approach with direct push triggers (creating race conditions with versioning)
* Rejection rationale: Violates requirement for proper version sequencing; GitHub Actions technical limitation makes this fragile; dual-workflow approach is more reliable

### IP-02: Direct Push-Based Trigger (Rejected per validator feedback)

* Approach: Create workflow that triggers on direct push when either file changes, without waiting for parent workflows
* Benefit: Immediate execution, simpler configuration
* Drawback: Files may not have version info yet (race condition); combines unversioned files; violates project pattern
* Rejection rationale: Violates user requirement and project conventions; version info must precede combining

### IP-03: Accept Partial Coverage (Rejected)

* Approach: Implement only workflow 2.2 (after workflow 2) and accept that guide_aires_securite-only changes won't trigger combining
* Benefit: Minimal implementation effort (~1 hour)
* Drawback: Only 67% coverage of requirement ("either file modified"); users updating guide_aires_securite alone get stale champs_des_alpes data
* Rejection rationale: Directly violates stated user requirement; Plan Validator flagged as CRITICAL

## Previous Implementation Paths

### Originally Considered: Single Workflow 2.2 (Validator rejected - replaced with selected approach)

This was the initial plan before Plan Validator identified the critical gap. The single workflow 2.2 triggering after workflow 2 only addressed half of the requirement. This has been superseded by the dual-workflow approach above.

## Suggested Follow-On Work

Items identified during planning that fall outside current scope:

* WI-01: Create Quick Reference Guide for Workflow Chain
  * Description: Document the workflow dependency chain (1 → 2 → 2.2) with visual diagram
  * Priority: Low
  * Source: Planning process - helps future maintainers understand workflow orchestration
  * Dependency: Workflow 2.2 deployed and tested
  * Effort: 1-2 hours

* WI-02: Add Webhook Integration for Real-time Updates
  * Description: Integrate combined_guide+champs.cup updates with external services (e.g., Discord notifications, FTP upload)
  * Priority: Low
  * Source: Similar to workflow 1000 (Ludovic Private Actions) which uploads to Google Drive
  * Dependency: Workflow 2.2 stable and reliable
  * Effort: 2-3 hours

* WI-03: Implement Combined File Validation
  * Description: Add validation step to check generated combined_guide+champs.cup for consistency (entry count, format, etc.)
  * Priority: Medium
  * Source: Pattern observed in workflow 998 (altitude checking) and src/WaypointProcessor/
  * Dependency: Workflow 2.2 deployed
  * Effort: 2-4 hours

* WI-04: Automated Testing of combineFiles.sh
  * Description: Create unit tests for the bash script to verify combining logic with various input files
  * Priority: Medium
  * Source: Best practice for script reliability
  * Dependency: Local development environment setup
  * Effort: 3-5 hours

* WI-05: Execute remote workflow validation checklist
  * Description: Run manual workflow_dispatch and source-file change scenarios in GitHub Actions to complete Phase 2.2, 2.3, and 3.2 evidence collection
  * Priority: High
  * Source: Implementation Phase 2 and 3 blockers
  * Dependency: Push rights and access to GitHub Actions UI for planeur-net/outlanding
  * Effort: 30-60 minutes

## Decision Points and Resolutions

No critical decision points required. Plan follows established project patterns with clear evidence from workflow 4.1.

## Implementation Deviations

* DD-03: Local YAML validation fallback used instead of planned yamllint command
  * Plan specifies: `yamllint .github/workflows/1.1-combine-guide-champs.yml .github/workflows/2.2-combine-guide-champs.yml`
  * Implementation differs: local parser validation was used as an interim check because yamllint is unavailable
  * Rationale: Preserve forward progress while documenting the exact missing tool and keeping remote validation pending

* DD-04: Runtime workflow chain validation deferred to remote execution
  * Plan specifies: manual verification of workflow 1 -> 1.1 and workflow 2 -> 2.2 execution chains
  * Implementation differs: only static/local checks completed; runtime chain confirmation blocked locally
  * Rationale: workflow_run and GitHub-hosted job behavior require remote Actions execution that is not available in this local-only run

## Known Risks and Mitigations

| Risk | Severity | Mitigation |
|------|----------|-----------|
| Workflow 2 name changes | Medium | Monitor workflow 2 for name changes; workflow_run trigger will fail silently if name doesn't match |
| combineFiles.sh script errors | Medium | Add error handling to workflow (set +e, check exit code) |
| Race condition with workflow 2 | Low | workflow_run guarantees execution after specified workflow completes |
| Auto-commit fails due to branch protection | Medium | Ensure GitHub Actions bot has write permissions; verify branch protection rules allow auto-commits |

## Timeline and Effort Estimate

| Phase | Estimated Time | Notes |
|-------|-----------------|-------|
| Phase 1 (Create Workflow) | 30 minutes | File creation and configuration |
| Phase 2 (Test & Integration) | 1-2 hours | Manual testing, workflow chain verification |
| Phase 3 (Validation) | 1-2 hours | YAML validation, content verification, file checks |
| **Total** | **2.5-4.5 hours** | Includes all testing and validation |

## Context and Assumptions

* GitHub Actions is properly configured in the repository
* Both source files (guide_aires_securite.cup, champs_des_alpes.cup) will continue to be versioned by workflows 1 and 2
* combined_guide+champs.cup should always reflect the latest versions of both source files
* Repository branch protection allows auto-commits from GitHub Actions
* No additional build tools or dependencies needed beyond bash, git, and sed
