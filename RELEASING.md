# Releasing and Versioning Guide

This project follows [Semantic Versioning (SemVer)](https://semver.org/) and uses an automated CI/CD pipeline for releases.

## Versioning Strategy

The version number follows the `MAJOR.MINOR.PATCH` format:

- **MAJOR**: Incompatible API changes.
- **MINOR**: Add functionality in a backwards-compatible manner.
- **PATCH**: Backwards-compatible bug fixes.

The **Source of Truth** for the current version is the `<Version>` tag in the project file:
`src/Touchstone.Parser/Touchstone.Parser.csproj`

## Automated Release Process

The release process is fully automated via GitHub Actions.

### 1. Automatic Release (On Push to Main)
Whenever a push occurs on the `main` branch, the [Publish to NuGet](.github/workflows/publish.yml) workflow triggers:
1. **Version Detection**: It reads the version from the `.csproj` file.
2. **Tag Check**: It checks if a Git tag for that version (e.g., `v0.1.0`) already exists.
3. **Release Execution**: If the version is new:
   - A Git tag is created and pushed.
   - A GitHub Release is created with auto-generated release notes.
   - The NuGet package is built, packed, and published to **NuGet.org** and **GitHub Packages**.

### 2. Manual Release (Workflow Dispatch)
You can manually trigger a release or override the versioning from the GitHub UI:
1. Go to the **Actions** tab.
2. Select **Publish to NuGet**.
3. Click **Run workflow**.
4. (Optional) Provide a **Manual version override** (e.g., `1.0.0`) to force a specific version regardless of the project file.

## Pre-releases and Branch Runs
- **Tags**: Any tag pushed starting with `v*` will trigger a release/publish flow using that tag name as the version.
- **Non-Main Branches**: Pushes to other branches do not trigger automatic releases.

## Best Practices
1. **Always bump the version**: Before merging a feature or fix to `main`, ensure the `<Version>` in the `.csproj` has been updated if you want a new release.
2. **Review Release Notes**: After an automated release, review the auto-generated notes on the GitHub Releases page to ensure they accurately reflect the changes.
3. **NuGet Secrets**: Ensure NuGet API Key is correctly set in the repository secrets.
