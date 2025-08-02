# Code Formatting Setup

To ensure consistent code formatting across all team members' machines:

## 1. Required VS Code Extensions

Install these extensions (they should auto-install from `.vscode/extensions.json`):
- C# Dev Kit
- Prettier - Code formatter
- EditorConfig for VS Code
- Tailwind CSS IntelliSense

## 2. VS Code Settings

The project includes shared VS Code settings in `.vscode/settings.json` that will:
- Format code on save
- Use Prettier for frontend files
- Use C# formatter for backend files
- Organize imports automatically
- Enforce consistent line endings

## 3. EditorConfig

The `.editorconfig` file defines formatting rules that work across different IDEs:
- 4 spaces for C# files
- 2 spaces for JS/TS/JSON files
- UTF-8 encoding
- LF line endings
- Trim trailing whitespace

## 4. Frontend Formatting

- **Prettier**: Configured in `.prettierrc` for consistent JS/TS formatting
- **ESLint**: Extended rules in `eslint.config.js` for code quality and formatting

## 5. Backend Formatting

- **EditorConfig**: Handles C# formatting rules
- **Built-in C# formatter**: Uses standard .NET formatting conventions

## Setup Instructions

1. Clone the repository
2. Open in VS Code
3. Install recommended extensions when prompted
4. Settings will automatically apply

## Manual Formatting

- **Format Document**: `Shift + Alt + F` (Windows/Linux) or `Shift + Option + F` (Mac)
- **Format Selection**: `Ctrl + K, Ctrl + F` (Windows/Linux) or `Cmd + K, Cmd + F` (Mac)

## Troubleshooting

If formatting isn't working:
1. Check that required extensions are installed
2. Reload VS Code window
3. Check the status bar for formatter selection
4. Ensure `.editorconfig` and `.prettierrc` files are present
