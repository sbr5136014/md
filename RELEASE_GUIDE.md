# Quick Release Guide

## 🚀 Creating a New Release

### 1. Prepare for Release
```bash
# Make sure all changes are committed
git status
git add .
git commit -m "Prepare for release v1.0.0"
git push origin main
```

### 2. Create Version Tag
```bash
# Create and push the version tag
git tag v1.0.0
git push origin v1.0.0
```

### 3. Automatic Process Begins
Once the tag is pushed, GitHub Actions will automatically:
- ✅ Build the application
- ✅ Create Windows executables (x64 and x86)
- ✅ Package into zip files
- ✅ Create GitHub release with downloads

### 4. Monitor Progress
- Go to **Actions** tab in GitHub
- Watch the "Release" workflow
- Check for any build errors

### 5. Verify Release
- Go to **Releases** tab
- Confirm new release is published
- Test download links work

## 📋 Release Checklist

Before creating a release:

- [ ] All features tested and working
- [ ] Version number updated in code (if needed)
- [ ] README.md updated with new features
- [ ] Commit all changes to main branch
- [ ] Choose appropriate version number:
  - `v1.0.0` - Major release
  - `v1.1.0` - New features
  - `v1.0.1` - Bug fixes

## 🏷️ Version Tags

Use semantic versioning with 'v' prefix:
- `v1.0.0` - First stable release
- `v1.1.0` - Added new markdown features
- `v1.0.1` - Fixed file opening bug
- `v2.0.0` - Major UI overhaul

## 📦 What Gets Built

Each release creates:
- `MarkdownViewer-v1.0.0-win-x64.zip` - 64-bit Windows
- `MarkdownViewer-v1.0.0-win-x86.zip` - 32-bit Windows

Each zip contains:
- `MarkdownViewer.exe` - Self-contained executable
- All required .NET dependencies
- No installation required

## 🐛 Troubleshooting

### Build Fails
1. Check Actions logs for errors
2. Verify project builds locally:
   ```bash
   dotnet build -c Release
   ```
3. Fix issues and create new tag

### Wrong Version in Release
1. Delete the tag:
   ```bash
   git tag -d v1.0.0
   git push origin --delete v1.0.0
   ```
2. Create correct tag:
   ```bash
   git tag v1.0.1
   git push origin v1.0.1
   ```

### Missing Files
- Check `.github/workflows/release.yml`
- Verify file paths are correct
- Test publish command locally

## 🔄 Release Schedule

**Recommended schedule:**
- **Major releases** (v2.0.0): Every 6-12 months
- **Minor releases** (v1.1.0): Monthly or when new features ready
- **Patch releases** (v1.0.1): As needed for bug fixes

---

**SmartArt Tech** - Streamlined release process for quality software delivery.