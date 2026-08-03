<!-- markdownlint-disable-file -->
# Implementation Details: Add GitHub Actions Workflow for combined_guide+champs.cup

## Context Reference

Sources: .copilot-tracking/research/2026-08-03/combine-workflow-research.md, Workflow 4.1 pattern analysis, existing workflows 1 and 2

## Implementation Phase 1: Create Workflow Files

<!-- parallelizable: true -->

### Step 1.1: Create workflow file 1.1-combine-guide-champs.yml (triggers after workflow 1)

Create a new GitHub Actions workflow file that combines the two waypoint files after the guide_aires_securite versioning workflow completes.

Files:
* .github/workflows/1.1-combine-guide-champs.yml - New workflow file to create

Workflow template structure:
```yaml
name: 1.1 - Combine guide_aires_securite + champs_des_alpes

on:
  workflow_run:
    workflows: ["1 - Add version to guide_aires_securite.cup"]
    types:
      - completed
  
  workflow_dispatch:

jobs:
  combine_guide_champs:
    if: github.ref == 'refs/heads/main' && github.repository == 'planeur-net/outlanding'
    name: Combine_guide_champs
    runs-on: ubuntu-latest
    steps:
    
    - name: 🚚 Get latest code for outlanding
      uses: actions/checkout@v3
      with:
        path: ./outlanding

    - name: ⚙️ Combine guide_aires_securite + champs_des_alpes
      id: combine_files
      run: |
        cd ./outlanding
        echo "Combining guide_aires_securite.cup + champs_des_alpes.cup"
        bash ./bin/combineFiles.sh
        
        # Save commit message for future push
        export sourceCommit=$(git log --format=%s -n 1 -- guide_aires_securite.cup)
        echo "LAST_COMMIT_MESSAGE=$sourceCommit" >> $GITHUB_OUTPUT

    - name: ⚙️ Push to repo
      uses: stefanzweifel/git-auto-commit-action@v5
      with:
        repository: ./outlanding
        commit_message: ${{ steps.combine_files.outputs.LAST_COMMIT_MESSAGE }}
        file_pattern: 'combined_guide+champs.cup'

    - name: Trigger event for github pages update
      uses: peter-evans/repository-dispatch@v2
      with:
        event-type: update-pages-event
```

Success criteria:
* File is valid YAML with correct indentation
* Trigger configuration references workflow 1 name: "1 - Add version to guide_aires_securite.cup"
* Job name follows project naming convention
* All steps include descriptive names with emoji prefix
* Auto-commit action targets the correct file pattern
* Dispatch trigger for pages update is configured

Context references:
* .github/workflows/4.1-aggregate-mountain-peaks-fr-ch-it.yml (Lines 1-76) - Reference implementation
* .github/workflows/1-add-version-to-guide-cup.yml (Lines 1-50) - Workflow to trigger after
* bin/combineFiles.sh (Lines 1-25) - Script to be executed

Dependencies:
* Workflow 1 must complete first (established via workflow_run trigger)
* Repository checkout with git history available

### Step 1.2: Create workflow file 2.2-combine-guide-champs.yml (triggers after workflow 2)

Create a new GitHub Actions workflow file that combines the two waypoint files after the champs_des_alpes versioning workflow completes.

Files:
* .github/workflows/2.2-combine-guide-champs.yml - New workflow file to create

Workflow template structure:
```yaml
name: 2.2 - Combine guide_aires_securite + champs_des_alpes

on:
  workflow_run:
    workflows: ["2 - Add version to champs_des_alpes"]
    types:
      - completed
  
  workflow_dispatch:

jobs:
  combine_guide_champs:
    if: github.ref == 'refs/heads/main' && github.repository == 'planeur-net/outlanding'
    name: Combine_guide_champs
    runs-on: ubuntu-latest
    steps:
    
    - name: 🚚 Get latest code for outlanding
      uses: actions/checkout@v3
      with:
        path: ./outlanding

    - name: ⚙️ Combine guide_aires_securite + champs_des_alpes
      id: combine_files
      run: |
        cd ./outlanding
        echo "Combining guide_aires_securite.cup + champs_des_alpes.cup"
        bash ./bin/combineFiles.sh
        
        # Save commit message for future push
        export sourceCommit=$(git log --format=%s -n 1 -- champs_des_alpes.cup)
        echo "LAST_COMMIT_MESSAGE=$sourceCommit" >> $GITHUB_OUTPUT

    - name: ⚙️ Push to repo
      uses: stefanzweifel/git-auto-commit-action@v5
      with:
        repository: ./outlanding
        commit_message: ${{ steps.combine_files.outputs.LAST_COMMIT_MESSAGE }}
        file_pattern: 'combined_guide+champs.cup'

    - name: Trigger event for github pages update
      uses: peter-evans/repository-dispatch@v2
      with:
        event-type: update-pages-event
```

Success criteria:
* File is valid YAML with correct indentation
* Trigger configuration references workflow 2 name: "2 - Add version to champs_des_alpes"
* Job name follows project naming convention
* All steps include descriptive names with emoji prefix
* Auto-commit action targets the correct file pattern
* Dispatch trigger for pages update is configured

Context references:
* .github/workflows/4.1-aggregate-mountain-peaks-fr-ch-it.yml (Lines 1-76) - Reference implementation
* .github/workflows/2-add-version-to-champs_des_alpes.yml (Lines 1-50) - Workflow to trigger after
* bin/combineFiles.sh (Lines 1-25) - Script to be executed

Dependencies:
* Workflow 2 must complete first (established via workflow_run trigger)
* Repository checkout with git history available

### Step 1.3: Validate both workflow files syntax

Validate YAML syntax and GitHub Actions configuration for both workflow files.

Validation method:
* Use yamllint: `yamllint .github/workflows/1.1-combine-guide-champs.yml .github/workflows/2.2-combine-guide-champs.yml`
* Alternatively, validate through GitHub Actions' built-in syntax checker on push

Success criteria:
* YAML is valid with correct indentation (2 spaces per GitHub Actions convention)
* All required fields are present in both files (name, on, jobs)
* Job configurations follow established patterns in both files
* Trigger references existing workflow names (exact match)
* Both files ready for deployment
Context references:
* Project workflow directory: .github/workflows/

Dependencies:
* yamllint tool (optional, for local validation)
* GitHub Actions syntax validation (automatic on push)

## Implementation Phase 2: Test and Integration

<!-- parallelizable: false -->

### Step 2.1: Verify both workflow files placement and naming

Confirm the workflow files are created in the correct locations with correct naming.

Files:
* .github/workflows/1.1-combine-guide-champs.yml - Target location for first workflow
* .github/workflows/2.2-combine-guide-champs.yml - Target location for second workflow

Success criteria:
* Both files exist at correct locations in `.github/workflows/` directory
* File names follow project naming: sequential number prefix (1.1 and 2.2), descriptive name, .yml extension
* Files are placed with other workflow files

Context references:
* Project workflow directory structure: .github/workflows/

Dependencies:
* File creation completed in Step 1.1 and 1.2

### Step 2.2: Test workflow execution paths (manual tests)

Manually trigger both workflows to verify execution and output.

Execution steps:
1. Push both workflow files to main branch (or feature branch for initial testing)
2. In GitHub Actions UI, navigate to Actions tab
3. For workflow 1.1:
   - Select "1.1 - Combine guide_aires_securite + champs_des_alpes"
   - Click "Run workflow" to trigger manual execution
   - Monitor execution for errors
4. For workflow 2.2:
   - Select "2.2 - Combine guide_aires_securite + champs_des_alpes"
   - Click "Run workflow" to trigger manual execution
   - Monitor execution for errors
5. Verify combined_guide+champs.cup is generated and updated in both cases

Success criteria:
* Both workflows run without errors
* All steps complete in both workflows (checkout, combine_files, push to repo, dispatch)
* combined_guide+champs.cup is generated and shows modified status
* Commit message appears in git history for both executions
* Repository dispatch trigger succeeds in both workflows
* File content contains entries from both source files

Context references:
* Workflow files: .github/workflows/1.1-combine-guide-champs.yml, .github/workflows/2.2-combine-guide-champs.yml
* Generated file: combined_guide+champs.cup
* Script execution: bin/combineFiles.sh

Dependencies:
* Both workflow files must be in repository
* Both source files (guide_aires_securite.cup, champs_des_alpes.cup) must exist with valid content
* Git repository must have history for both files

### Step 2.3: Verify integration with workflows 1 and 2

Confirm the workflows trigger automatically after their respective parent workflows complete.

Verification steps for workflow 1 → 1.1 chain:
1. Modify guide_aires_securite.cup file (add or change a waypoint)
2. Commit and push to main branch
3. Monitor workflow 1 execution
4. After workflow 1 completes, verify workflow 1.1 triggers automatically
5. Monitor workflow 1.1 for successful completion
6. Verify combined_guide+champs.cup is updated

Verification steps for workflow 2 → 2.2 chain:
1. Modify champs_des_alpes.cup file (add or change a waypoint)
2. Commit and push to main branch
3. Monitor workflow 2 execution
4. After workflow 2 completes, verify workflow 2.2 triggers automatically
5. Monitor workflow 2.2 for successful completion
6. Verify combined_guide+champs.cup is updated

Success criteria:
* Workflow 1 completes with success
* Workflow 1.1 automatically triggers within seconds of workflow 1 completion
* combined_guide+champs.cup is committed with new content after 1.1 completes
* Workflow 2 completes with success
* Workflow 2.2 automatically triggers within seconds of workflow 2 completion
* combined_guide+champs.cup is committed with new content after 2.2 completes
* Version line in combined_guide+champs.cup includes both source files' latest commits in all cases

Context references:
* Workflow 1: .github/workflows/1-add-version-to-guide-cup.yml
* Workflow 1.1: .github/workflows/1.1-combine-guide-champs.yml
* Workflow 2: .github/workflows/2-add-version-to-champs_des_alpes.yml
* Workflow 2.2: .github/workflows/2.2-combine-guide-champs.yml

Dependencies:
* Both parent workflows must complete before child workflows trigger
* Repository branch protection must allow auto-commit action

## Implementation Phase 3: Validation

<!-- parallelizable: false -->

### Step 3.1: Run full project validation

Execute YAML linting and GitHub Actions syntax validation for both workflows.

Validation commands:
* YAML Lint: `yamllint .github/workflows/1.1-combine-guide-champs.yml .github/workflows/2.2-combine-guide-champs.yml`
* GitHub Actions Syntax: Validated automatically on push to repository

Success criteria:
* YAML lint reports no errors or warnings for both files
* GitHub Actions interface shows no workflow configuration errors
* Both workflows appear in Actions tab with correct names
* Both workflows are ready for production use

Context references:
* Workflow files: .github/workflows/1.1-combine-guide-champs.yml and .github/workflows/2.2-combine-guide-champs.yml

Dependencies:
* yamllint tool installed (optional for local validation)

### Step 3.2: Test complete workflow chains (all scenarios)

Execute comprehensive workflow testing covering all modification scenarios to verify the "either source file" requirement is fully met.

**Scenario A: guide_aires_securite.cup only modification**
1. Modify guide_aires_securite.cup (add or edit a waypoint entry)
2. Commit and push to main branch
3. Monitor workflow 1 (add version to guide_aires_securite.cup) - should complete
4. Verify workflow 1.1 (combine) automatically triggers - should complete
5. Verify combined_guide+champs.cup is updated with new content
6. Check that version line includes latest guide_aires_securite.cup commit with current champs_des_alpes.cup commit

**Scenario B: champs_des_alpes.cup only modification**
1. Modify champs_des_alpes.cup (add or edit a waypoint entry)
2. Commit and push to main branch
3. Monitor workflow 2 (add version to champs_des_alpes.cup) - should complete
4. Verify workflow 2.2 (combine) automatically triggers - should complete
5. Verify combined_guide+champs.cup is updated with new content
6. Check that version line includes latest champs_des_alpes.cup commit with current guide_aires_securite.cup commit

**Scenario C: Both files modified**
1. Modify both guide_aires_securite.cup and champs_des_alpes.cup
2. Commit and push to main branch
3. Monitor both workflow 1 and workflow 2 executions - should both complete
4. Verify both workflow 1.1 and workflow 2.2 trigger - both should complete
5. Verify combined_guide+champs.cup is updated correctly in both execution paths

Success criteria for all scenarios:
* Workflow 1 completes with success (when guide_aires_securite.cup modified)
* Workflow 1.1 triggers automatically and completes (when workflow 1 completes)
* Workflow 2 completes with success (when champs_des_alpes.cup modified)
* Workflow 2.2 triggers automatically and completes (when workflow 2 completes)
* combined_guide+champs.cup is generated and committed in all scenarios
* Version line is correctly formatted in all scenarios
* File content correctly reflects both source files

Context references:
* Workflow 1: .github/workflows/1-add-version-to-guide-cup.yml
* Workflow 1.1: .github/workflows/1.1-combine-guide-champs.yml
* Workflow 2: .github/workflows/2-add-version-to-champs_des_alpes.yml
* Workflow 2.2: .github/workflows/2.2-combine-guide-champs.yml
* Source files: guide_aires_securite.cup, champs_des_alpes.cup
* Generated file: combined_guide+champs.cup

Dependencies:
* All four workflows must be deployed and functional
* Both source files must have git history
* Main branch must allow auto-commits from GitHub Actions

### Step 3.3: Verify file contents and version metadata

Inspect the generated combined_guide+champs.cup file to ensure correct content in all scenarios.

Verification steps:
1. After each scenario execution, download or view combined_guide+champs.cup from repository
2. Verify file contains waypoint entries from guide_aires_securite.cup
3. Verify file contains waypoint entries from champs_des_alpes.cup
4. Verify version line is present (second line after header)
5. Verify version line format matches: `"version=",,,,,,,,,,,,"[guide_aires_securite]<commit> + [champs_des_alpes]<commit>",,`
6. Verify "Related Tasks" sections are removed from both source files
7. Verify file is syntactically valid SeeYou CUP format (can be imported into flight planning software)
8. Verify that version timestamps are recent (within seconds of workflow execution)

Success criteria:
* File contains entries from both source files in all scenarios
* No duplicate entries or formatting errors
* Version line present with correct format in all scenarios
* "Related Tasks" lines removed
* File is syntactically valid CUP format
* Git commit hashes and timestamps are correctly included and recent
* File size is approximately sum of both source files (accounting for removed headers/tasks)

Context references:
* Generated file: combined_guide+champs.cup
* combineFiles.sh: bin/combineFiles.sh (Lines 1-25) - Documents expected output format
* Source files: guide_aires_securite.cup, champs_des_alpes.cup

Dependencies:
* Both workflows 1.1 and 2.2 must complete successfully
* combineFiles.sh must execute without errors in both workflows
* Git commit hashes and timestamps are correctly included

Context references:
* Generated file: combined_guide+champs.cup
* combineFiles.sh: bin/combineFiles.sh (Lines 1-25) - Documents expected output format

Dependencies:
* Workflow 2.2 must complete successfully
* combineFiles.sh must execute without errors

### Step 3.4: Report blocking issues

Document any issues discovered during validation and provide troubleshooting guidance.

Reporting steps:
* If validation fails, document the specific error message and context
* If file contents are incorrect, document what is missing or malformed
* If workflow doesn't trigger, check GitHub Actions logs for reason
* Provide user with next steps for resolution

Success criteria:
* All validation checks pass
### Step 3.4: Report blocking issues

Document any issues discovered during validation and provide troubleshooting guidance.

Reporting steps:
* If validation fails, document the specific error message and context
* If file contents are incorrect, document what is missing or malformed
* If either workflow doesn't trigger, check GitHub Actions logs for reason
* If any modification scenario doesn't result in combined file update, document which scenario failed
* Provide user with next steps for resolution

Success criteria:
* All validation checks pass
* All three modification scenarios result in updated combined_guide+champs.cup
* No blocking issues identified
* If issues are found, they are documented with sufficient detail for resolution
* User has clear guidance on next steps

Context references:
* Workflow logs: GitHub Actions UI for workflows 1.1 and 2.2
* Generated file: combined_guide+champs.cup
* Script output: .github/workflows/1.1-combine-guide-champs.yml and .github/workflows/2.2-combine-guide-champs.yml step logs

Dependencies:
* Previous validation steps (3.1-3.3) completed

## Dependencies

* GitHub Actions runner (ubuntu-latest with bash and git)
* Actions: actions/checkout@v3, stefanzweifel/git-auto-commit-action@v5, peter-evans/repository-dispatch@v2
* Existing workflows: 1-add-version-to-guide-cup.yml, 2-add-version-to-champs_des_alpes.yml
* Source files: guide_aires_securite.cup, champs_des_alpes.cup
* Script: bin/combineFiles.sh

## Success Criteria

* Both workflow files created and deployed (.github/workflows/1.1-combine-guide-champs.yml and .github/workflows/2.2-combine-guide-champs.yml)
* Workflow 1.1 triggers automatically after workflow 1 completes
* Workflow 2.2 triggers automatically after workflow 2 completes
* combineFiles.sh executes without errors in both workflows
* combined_guide+champs.cup is generated with correct content in all modification scenarios
* Generated file is committed and pushed to main branch in all scenarios
* Version metadata includes both source files' git information
* Repository dispatch trigger initiates downstream workflows in both workflows
* All validation steps pass with no critical errors
* All three modification scenarios (guide_aires only, champs only, both) result in updated combined_guide+champs.cup
