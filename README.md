# Markdown Viewer

A simple Windows application for viewing Markdown (.md) files with WPF.

**Developed by SmartArt Tech**

## Features

- ✅ **Open Markdown Files**: Use File → Open or Ctrl+O to browse and open .md files
- ✅ **Command Line Support**: Pass .md files as arguments: `MarkdownViewer.exe file.md`
- ✅ **HTML Rendering**: Converts Markdown to styled HTML for display
- ✅ **Default File Handler**: Optional integration to set as default .md file viewer
- ✅ **External Link Support**: Opens external links in default browser
- ✅ **Basic Markdown Support**: Headers, bold, italic, links, code blocks, lists, blockquotes

## Building and Running

### Prerequisites
- .NET 8.0 SDK or later
- Windows 10/11

### Build Instructions
```bash
# Clone/download the project
cd MarkdownViewer

# Build the application
dotnet build

# Run the application
dotnet run

# Run with a specific file
dotnet run sample.md
```

### Creating an Executable
```bash
# Publish as self-contained executable (Windows x64)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true

# Publish for Windows x86
dotnet publish -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true

# The executable will be in: bin/Release/net8.0-windows/[runtime]/publish/
```

### Download Pre-built Releases

**Latest releases are available on GitHub:**
- Go to the [Releases page](https://github.com/smartarttech/markdown-viewer/releases)
- Download the appropriate zip file for your system:
  - `MarkdownViewer-vX.X.X-win-x64.zip` (64-bit Windows)
  - `MarkdownViewer-vX.X.X-win-x86.zip` (32-bit Windows)
- Extract and run `MarkdownViewer.exe`

## Usage

### Opening Files
1. **File Menu**: Click File → Open or press Ctrl+O
2. **Command Line**: `MarkdownViewer.exe path/to/file.md`
3. **Drag & Drop**: When set as default handler, double-click .md files

### Setting as Default Handler
1. Open the application
2. Go to Settings → "Set as Default MD Viewer"
3. Grant administrator permissions if prompted
4. All .md files will now open with Markdown Viewer

### Keyboard Shortcuts
- `Ctrl+O`: Open file dialog
- `Alt+F4`: Exit application

## Supported Markdown Elements

- **Headers**: # H1, ## H2, ### H3, etc.
- **Text Formatting**: **bold**, *italic*, `code`
- **Links**: [link text](http://example.com)
- **Code Blocks**: ```code```
- **Lists**: - bullets and 1. numbered
- **Blockquotes**: > quoted text
- **Line Breaks**: Automatic paragraph and line break conversion

## File Structure

```
MarkdownViewer/
├── App.xaml              # Application definition
├── App.xaml.cs           # Application code-behind
├── MainWindow.xaml       # Main window UI
├── MainWindow.xaml.cs    # Main window logic
├── MarkdownViewer.csproj # Project file
├── sample.md             # Sample markdown file
└── README.md             # This file
```

## Architecture

- **UI Framework**: WPF (Windows Presentation Foundation)
- **Web Rendering**: WebBrowser control (Internet Explorer engine)
- **Markdown Parser**: Custom regex-based converter
- **File Associations**: Windows Registry integration

## Limitations

- Uses WebBrowser control (IE engine) - may have limited CSS support
- Simple markdown parser - doesn't support all advanced features
- Windows-only application
- Requires administrator rights to set as default file handler

## GitHub Auto-Releases

This project includes automated GitHub Actions for creating releases:

### For Developers/Contributors

1. **Setup**: Follow instructions in `GITHUB_SETUP.md`
2. **Release Process**: Create version tags to trigger automatic builds
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```
3. **Automated Build**: GitHub Actions automatically:
   - Builds for Windows x64 and x86
   - Creates self-contained executables
   - Packages into zip files
   - Publishes to GitHub Releases

### For Users

- **Download**: Get the latest version from [GitHub Releases](https://github.com/smartarttech/markdown-viewer/releases)
- **No Installation Required**: Self-contained executables include all dependencies
- **Automatic Updates**: Check releases page for new versions

## About

**Markdown Viewer** is developed by **SmartArt Tech**, a technology company focused on creating simple, effective tools for productivity and content management.

© 2024 SmartArt Tech. All rights reserved.

## License

This project is provided as-is for educational and demonstration purposes.