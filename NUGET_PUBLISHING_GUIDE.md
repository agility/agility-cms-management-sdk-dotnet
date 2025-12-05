# NuGet Package Publishing Guide

## Overview

This guide walks you through versioning and publishing the Agility.Management.SDK NuGet package.

## Versioning

### Where to Set Version

**The version is set in ONE place only:** `management.api.sdk/management.api.sdk.csproj`

```xml
<Version>1.0.9-beta</Version>
```

### NuGet Version Format

NuGet uses [Semantic Versioning (SemVer)](https://semver.org/) format: `MAJOR.MINOR.PATCH[-PRERELEASE][+BUILD]`

Examples:
- `1.0.0` - Stable release
- `1.0.9-beta` - Pre-release (beta)
- `1.0.9-beta.1` - Pre-release with build number
- `1.1.0-alpha` - Pre-release (alpha)
- `2.0.0` - Major version bump

### Version Bump Guidelines

- **MAJOR** (1.0.0 → 2.0.0): Breaking changes
- **MINOR** (1.0.0 → 1.1.0): New features, backward compatible
- **PATCH** (1.0.0 → 1.0.1): Bug fixes, backward compatible
- **PRERELEASE** (-beta, -alpha): Development/testing releases

### How to Update Version

1. Open `management.api.sdk/management.api.sdk.csproj`
2. Update the `<Version>` tag:
   ```xml
   <Version>1.0.10</Version>  <!-- Example: bumping patch version -->
   ```
3. Save the file
4. **That's it!** No code changes needed.

## Prerequisites

1. **NuGet.org Account**
   - Sign up at https://www.nuget.org/users/account/LogOn
   - Verify your email address

2. **API Key**
   - Go to https://www.nuget.org/account/apikeys
   - Click "Create"
   - Name: e.g., "Agility SDK Publishing"
   - Select "Select scopes" → "Push new packages and package versions"
   - Set expiration (or leave blank for no expiration)
   - Click "Create"
   - **Copy the API key** (you'll only see it once!)

3. **.NET SDK**
   - Ensure you have .NET SDK 6.0 or later installed
   - Verify: `dotnet --version`

## Publishing Steps

### Step 1: Update Version (if needed)

Edit `management.api.sdk/management.api.sdk.csproj` and update the version:

```xml
<Version>1.0.10</Version>
```

### Step 2: Build the Project

```powershell
cd management.api.sdk
dotnet build --configuration Release
```

### Step 3: Create the NuGet Package

```powershell
dotnet pack --configuration Release --no-build
```

This creates a `.nupkg` file in `bin/Release/` directory.

**Alternative:** If you want to build and pack in one step:

```powershell
dotnet pack --configuration Release
```

### Step 4: Verify the Package

Check the generated `.nupkg` file:

```powershell
# List the package file
dir bin\Release\*.nupkg
```

You can also inspect the package contents:

```powershell
# Extract and view (optional)
dotnet nuget locals all --list
```

### Step 5: Publish to NuGet.org

**Option A: Using API Key (Recommended)**

```powershell
dotnet nuget push bin\Release\Agility.Management.SDK.1.0.10.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

Replace:
- `YOUR_API_KEY` with your actual API key from NuGet.org
- `1.0.10` with your actual version number

**Option B: Using Interactive Login**

```powershell
dotnet nuget push bin\Release\Agility.Management.SDK.1.0.10.nupkg --source https://api.nuget.org/v3/index.json --interactive
```

This will prompt you to authenticate via browser.

### Step 6: Verify Publication

1. Go to https://www.nuget.org/packages/Agility.Management.SDK
2. Your new version should appear (may take a few minutes)
3. Users can install with: `dotnet add package Agility.Management.SDK`

## Quick Reference Commands

### Full Publishing Workflow

```powershell
# 1. Navigate to project directory
cd management.api.sdk

# 2. Update version in .csproj file (manually edit)

# 3. Build and pack
dotnet pack --configuration Release

# 4. Publish (replace API_KEY and VERSION)
dotnet nuget push bin\Release\Agility.Management.SDK.VERSION.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

### Check Current Version

```powershell
# View the .csproj file
Get-Content management.api.sdk.csproj | Select-String "Version"
```

### List All Generated Packages

```powershell
dir bin\Release\*.nupkg
```

## Pre-release Packages

If you're publishing a pre-release version (e.g., `1.0.10-beta`):

- The package will be marked as pre-release on NuGet.org
- Users need to explicitly opt-in: `dotnet add package Agility.Management.SDK --version 1.0.10-beta`
- Or use: `dotnet add package Agility.Management.SDK --prerelease`

## Troubleshooting

### Error: "Package already exists"

- You cannot republish the same version
- Bump the version number and try again

### Error: "API key is invalid"

- Verify your API key at https://www.nuget.org/account/apikeys
- Ensure the key hasn't expired
- Check that the key has "Push" permissions

### Error: "Package validation failed"

- Check that all required metadata is set in `.csproj`
- Verify the package ID matches your NuGet.org account permissions

### Package Not Appearing

- NuGet.org indexing can take 5-10 minutes
- Check your package page: https://www.nuget.org/packages/Agility.Management.SDK
- Verify the push command succeeded (check for success message)

## Best Practices

1. **Always test locally first**
   - Create a test project
   - Install your local package: `dotnet add package Agility.Management.SDK --source ./bin/Release`
   - Verify it works before publishing

2. **Use semantic versioning**
   - Follow SemVer guidelines
   - Document breaking changes in release notes

3. **Tag releases in Git**
   ```powershell
   git tag v1.0.10
   git push origin v1.0.10
   ```

4. **Keep API keys secure**
   - Never commit API keys to Git
   - Use environment variables or secure storage
   - Consider using Azure DevOps or GitHub Actions for automated publishing

5. **Document changes**
   - Update README.md with new features/changes
   - Consider adding a CHANGELOG.md file

## Automated Publishing (Future)

Consider setting up CI/CD for automated publishing:

- **GitHub Actions**: Automatically publish on tag creation
- **Azure DevOps**: Pipeline-based publishing
- **GitLab CI**: Similar automation options

Example GitHub Actions workflow would:
1. Trigger on version tag (e.g., `v1.0.10`)
2. Build and pack the project
3. Publish to NuGet.org using stored secrets

## Additional Resources

- [NuGet Package Creation](https://docs.microsoft.com/en-us/nuget/create-packages/creating-a-package-dotnet-cli)
- [NuGet Publishing Guide](https://docs.microsoft.com/en-us/nuget/nuget-org/publish-a-package)
- [Semantic Versioning](https://semver.org/)
- [NuGet.org Package Management](https://www.nuget.org/packages/manage/upload)

