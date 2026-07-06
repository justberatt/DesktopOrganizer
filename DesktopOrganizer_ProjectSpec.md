# Desktop Organizer — Project Specification & Design Guide

## Project Overview

Desktop Organizer is a Windows application that automatically sorts files on your desktop into organized folders based on file type. Instead of manually moving files, the app classifies files by extension and moves them into categorized folders, keeping your desktop clean and organized.

---

## Core Features

### Phase 1 — Basic Organization
- **File Classification** — automatically categorize files by extension
- **Categories** — Images, PDFs, Documents, Spreadsheets, Videos, Audio, Archives, App Shortcuts, Other
- **Preview Before Action** — show what will be moved before executing
- **Undo Support** — restore files to their original locations

### Phase 2 — Smart Features
- **Auto-Sort (File Watcher)** — automatically organize new files as they land on the desktop
- **System Tray Integration** — minimize to tray, quick access via right-click menu
- **Customizable Categories** — edit extension-to-folder mappings via settings

### Phase 3 — Polish
- **Windows Startup Integration** — optionally launch on system startup
- **Conflict Handling** — automatically rename files with duplicate names
- **Error Handling** — gracefully skip locked or open files

---

## Architecture

### Class Structure

```
Categories.cs
├── Dictionary<string, string[]> mapping extensions to folders
├── GetCategory(string filePath) — returns the category for a file
└── Predefined categories with common extensions

Organizer.cs
├── GetDesktopPath() — returns the user's desktop folder path
├── Organize(bool dryRun) — main logic, scans and moves files
├── GetConflictFreePath(string path) — handles filename conflicts
└── LogSkippedFiles() — tracks files that couldn't be moved

UndoLog.cs
├── MoveRecord class (OriginalPath, NewPath)
├── Save(List<MoveRecord>) — serializes to JSON
├── Load() — deserializes from JSON
└── Undo() — reverses all moves

Form1.cs (WinForms UI)
├── ListView preview of files to be moved
├── Organize button — executes the sort
├── Undo button — reverses the last operation
├── Auto-sort checkbox — toggles FileSystemWatcher
├── Status label — shows operation results

SettingsForm.cs (Optional)
├── DataGridView of categories and extensions
├── Save/Reset buttons
└── Persist to settings.json

FileSystemWatcher integration
└── Auto-monitors desktop for new files
```

---

## Design Language — Glassomorphism

### Philosophy
A modern, minimalistic interface inspired by glassmorphic design: clean surfaces with subtle frosted glass textures, soft shadows, and a sense of depth without visual clutter. The app feels premium but lightweight.

### Color Palette

**Primary Background**
- `#1E1E1E` (dark gray, Windows Terminal style) with subtle depth
- Slight frosted effect using transparency and layering

**Surfaces/Panels**
- `#2D2D2D` (slightly lighter gray for panels, list rows, and cards)
- Used for hovering states and elevated surfaces

**Accent Color**
- `#FF6B35` (warm orange-red) — used for active buttons, toggles, focus rings, and interactive states
- Bold but not harsh; commanding attention without being neon

**Text**
- `#E0E0E0` (light gray) for primary text — ensures readability on dark backgrounds
- `#999999` (muted gray) for secondary text and labels
- `#666666` (darker gray) for hints and disabled states

**Borders & Dividers**
- `#3D3D3D` (subtle dark gray) — 0.5–1px borders
- Use sparingly; whitespace is primary

### Typography

- **Primary Font:** Segoe UI (Windows system font) or Inter (modern fallback)
- **Heading (Title):** 18px, 600 weight, `#1A1A1A`
- **Body Text:** 14px, 400 weight, `#1A1A1A`
- **Labels:** 12px, 500 weight, `#666666`
- **No emoji, no decorative icons** — only functional UI elements

### Components

#### Buttons
- **Style:** Soft, frosted appearance with minimal border
- **Padding:** 10px 20px
- **Border-radius:** 8px
- **Border:** 1px solid `#3D3D3D`
- **Background:** `#2D2D2D` (rest), `#383838` (hover), `#FF6B35` with white text (active)
- **Transition:** 150ms ease on color/shadow change
- **Shadow:** 0 2px 8px rgba(0, 0, 0, 0.3) — subtle depth

#### Input Fields
- **Style:** Frosted glass with soft focus ring
- **Padding:** 8px 12px
- **Border-radius:** 6px
- **Border:** 1px solid `#3D3D3D`
- **Background:** `#2D2D2D` (dark surface)
- **Focus Ring:** 2px solid `#FF6B35` with 0px offset
- **Placeholder Text:** `#666666`
- **Text:** `#E0E0E0`

#### List/Table
- **Row Height:** 36px
- **Row Divider:** 1px solid `#3D3D3D` (extremely subtle)
- **Hover State:** `#383838` background (no drastic color shift)
- **Selected Row:** `#2D3D45` background (very subtle blue-tinted dark)
- **Text:** `#E0E0E0` (primary), `#999999` (secondary)

#### Checkbox / Toggle
- **Size:** 16x16px
- **Border-radius:** 4px
- **Border:** 1px solid `#555555`
- **Checked State:** `#FF6B35` background with white checkmark
- **Transition:** 100ms ease

#### Status Label
- **Padding:** 8px 12px
- **Border-radius:** 6px
- **Success:** `#1E4620` background, `#4CAF50` text
- **Warning:** `#4A3500` background, `#FFA500` text
- **Error:** `#4A1F1F` background, `#FF5252` text
- **Info:** `#1A3A52` background, `#64B5F6` text

### Layout

#### Main Window
- **Size:** 520px wide × 560px tall (responsive, min-width 400px)
- **Padding:** 20px all sides
- **Background:** `#1E1E1E` (no gradient)
- **Window Shadow:** 0 8px 32px rgba(0, 0, 0, 0.5)

#### Spacing & Grid
- **Vertical Rhythm:** 16px gaps between sections
- **Horizontal Alignment:** 20px margins on edges
- **Element Height:** 36px (buttons, inputs, list rows)
- **Section Separator:** 1px solid `#3D3D3D` with 16px margin above/below

#### Frosted Glass Effect (if using WinForms)
Since WinForms doesn't natively support backdrop-filter, simulate with:
- **Panel Background:** `#FFFFFF` at 95% opacity (slightly transparent)
- **Border:** 1px solid `#E8E8E8`
- **Inner Shadow:** Use a darker panel behind with 2px offset for depth perception
- Avoid heavy blur; keep readability paramount

### Interactive States

| Element | Rest | Hover | Active | Disabled |
|---------|------|-------|--------|----------|
| Button | Dark bg (#2D2D2D), dark border | Lighter dark bg (#383838), dark border | Orange-red bg (#FF6B35), white text | Gray text, no interaction |
| Input | Dark bg (#2D2D2D), dark border | Dark bg, lighter border | Orange-red focus ring (#FF6B35) | Very dark bg, no interaction |
| Checkbox | Dark border | Lighter border | Orange-red bg (#FF6B35), checkmark | Gray, no interaction |
| Link/Label | Light gray text (#E0E0E0) | Brighter gray text | Orange-red text (#FF6B35) | Muted gray text (#666666) |

### Micro-interactions
- **Hover Transitions:** 150ms ease on all interactive elements
- **Click Feedback:** Subtle 0.98 scale on buttons (98% size for 50ms)
- **Focus Indicator:** 2px blue ring, 2px offset, visible on Tab navigation
- **Disabled State:** 40% opacity, no pointer interaction

### Shadows & Depth
- **Elevation 1 (Cards, Panels):** `0 2px 8px rgba(0, 0, 0, 0.05)`
- **Elevation 2 (Modals, Popovers):** `0 8px 32px rgba(0, 0, 0, 0.1)`
- **Focus Ring:** No shadow, just border color

### Theme Notes
The primary design is dark mode (Windows Terminal style) with warm orange-red accents. This approach:
- **Reduces eye strain** — appropriate for a utility app used frequently
- **Modern aesthetic** — aligns with contemporary Windows 11 design language
- **High contrast** — `#E0E0E0` text on `#1E1E1E` background meets accessibility standards
- **Warm accent** — Orange-red (#FF6B35) provides visual warmth without being harsh
- Same frosted glass philosophy — subtle depth without harshness

---

## User Flow

### Primary Workflow
1. **User opens app** → Sees preview of files that will be moved
2. **User reviews** → List shows filename and destination folder
3. **User clicks Organize** → Files move, status updates ("120 files organized")
4. **User can Undo** → All files return to desktop

### Secondary Features
- **Enable Auto-Sort** → New files auto-organize as they land (optional checkbox)
- **Access Settings** → Customize categories via a dialog
- **Minimize to Tray** → Close button minimizes to system tray, restore via tray icon

---

## Implementation Phases — Step by Step

### Phase 1 — Setup

#### 1. Install Visual Studio Community
- Go to visualstudio.microsoft.com and download the free Community edition
- During install, select the **.NET Desktop Development** workload
- Wait for install to complete (~7GB, takes 15–30 minutes)
- Launch Visual Studio to confirm it opens correctly

#### 2. Create the project
- Click **Create a new project**
- Search for **Windows Forms App** — make sure it says C# not Visual Basic
- Name it `DesktopOrganizer`
- Set target framework to **.NET 10** (LTS)
- Click Create

#### 3. Understand the generated files
- `Program.cs` — entry point, launches the app, don't touch yet
- `Form1.cs` — your main window logic, this is where you'll work
- `Form1.Designer.cs` — auto-generated UI layout, never edit manually

---

### Phase 2 — Core Logic

#### 1. Create Categories.cs
- Right-click project → **Add → New Class** → name it `Categories.cs`
- Define a `Dictionary<string, string[]>` mapping folder names to extensions
- Add entries: Images, PDFs, Documents, Spreadsheets, Videos, Audio, Archives, App Shortcuts
- Add a `GetCategory(string filePath)` method that returns the matching category
- Return `"Other"` as fallback if no extension matches

#### 2. Create Organizer.cs
- Add a new class file `Organizer.cs`
- Add a `GetDesktopPath()` helper using `Environment.GetFolderPath()`
- Write an `Organize(bool dryRun)` method that loops through all desktop files
- Skip hidden files (name starts with `.`)
- Skip directories whose names match your category folder names
- Skip all other directories — leave user folders untouched
- For each file: call `GetCategory()`, build the target path, call `File.Move()`

#### 3. Create UndoLog.cs
- Add a new class file `UndoLog.cs`
- Define a `MoveRecord` class with `OriginalPath` and `NewPath` string properties
- Write a `Save(List<MoveRecord>)` method that serializes to JSON using `System.Text.Json`
- Write a `Load()` method that deserializes the JSON back to a list
- Write an `Undo()` method that calls `File.Move()` in reverse for each record
- Store the log file on the desktop as `.organizer_log.json`

#### 4. Handle edge cases in Organizer.cs
- Wrap `File.Move()` in try/catch to handle locked or open files
- Check if a file already exists at the target path before moving
- If conflict: append a Unix timestamp to the filename to make it unique
- Log skipped files to a list so the UI can show them later

---

### Phase 3 — UI (WinForms)

#### 1. Design the main window layout
- Open `Form1.cs` in the designer (double-click it)
- Set form title to `"Desktop Organizer"` in Properties panel
- Set a fixed size: 520 x 560 pixels
- Drag a **ListView** control onto the form — this shows the preview of files to move
- Add two columns to the ListView: `"File"` and `"Will move to"`
- Drag an **Organize** button below the list
- Drag an **Undo** button next to it
- Drag a **Label** at the bottom for status messages

#### 2. Wire up the preview (dry run on load)
- In `Form1.cs`, call `Organizer.Organize(dryRun: true)` on form load
- Loop through returned records and add each one as a row in the ListView
- Show file name in column 1, target folder name in column 2

#### 3. Wire up the Organize button
- Double-click the Organize button in designer to create its click handler
- Call `Organizer.Organize(dryRun: false)` inside the handler
- Call `UndoLog.Save()` with the returned move records
- Update the status label to say how many files were moved
- Refresh the ListView to show it's now empty (desktop is clean)

#### 4. Wire up the Undo button
- Double-click the Undo button in designer
- Call `UndoLog.Undo()` in the handler
- Reload the preview in the ListView
- Update status label to confirm undo was successful
- Disable the Undo button if no log file exists

---

### Phase 4 — File Watcher (auto-sort)

#### 1. Add FileSystemWatcher to Organizer.cs
- Instantiate a `FileSystemWatcher` pointed at the desktop path
- Set `Filter` to `"*.*"` to catch all new files
- Set `NotifyFilter` to `FileName | LastWrite`
- Subscribe to the `Created` event
- In the event handler: wait 500ms (files need time to finish copying), then call `Organize()` on just that one file

#### 2. Add a toggle in the UI
- Add a **CheckBox** labelled `"Auto-sort new files"`
- When checked: start the `FileSystemWatcher`
- When unchecked: stop and dispose the `FileSystemWatcher`
- Persist the toggle using `Properties.Settings` so it survives restart

---

### Phase 5 — System Tray

#### 1. Add a NotifyIcon
- Drag a **NotifyIcon** component onto Form1 from the toolbox
- Assign it a `.ico` file — create one free at favicon.io
- Set `Visible = true`

#### 2. Add a right-click context menu
- Drag a **ContextMenuStrip** onto the form
- Add menu items: `"Organize Now"`, `"Undo"`, `"Open Settings"`, `"Exit"`
- Assign the `ContextMenuStrip` to the `NotifyIcon`'s `ContextMenuStrip` property
- Wire each menu item to the same handlers as the main window buttons

#### 3. Minimize to tray instead of closing
- Override the form's `FormClosing` event
- Instead of closing, set `Visible = false` and show a balloon tip
- Double-clicking the tray icon restores and shows the window

---

### Phase 6 — Settings

#### 1. Create a Settings form
- Add a new Form to the project, name it `SettingsForm`
- Add a **DataGridView** with two columns: `"Category"` and `"Extensions"`
- Populate it from the Categories dictionary on load
- Add **Save** and **Cancel** buttons

#### 2. Persist custom categories
- On Save: serialize the edited dictionary to `settings.json` in `AppData`
- On startup: check if `settings.json` exists — load it instead of hardcoded defaults
- Add a **Reset to defaults** button that deletes `settings.json` and reloads

---

### Phase 7 — Package & Ship

#### 1. Test thoroughly
- Test with an empty desktop
- Test with files that have name conflicts
- Test with locked/open files
- Test running Organize twice in a row
- Test Undo after each scenario

#### 2. Publish as a self-contained .exe
- Right-click project in Solution Explorer → **Publish**
- Choose **Folder** as publish target
- Set deployment mode to **Self-contained**
- Set target runtime to **win-x64**
- Click Publish — find the `.exe` in the output folder

#### 3. Add to Windows startup (optional)
- Add a checkbox in Settings: `"Launch on startup"`
- When enabled: write a registry key to `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
- When disabled: delete the registry key
- App should start minimized to tray when launched this way

---

- **Language:** C# with .NET 10
- **GUI Framework:** WinForms (simple, native)
- **File Operations:** `System.IO` (`File`, `Directory`, `Path`)
- **File Watching:** `System.IO.FileSystemWatcher`
- **Serialization:** `System.Text.Json` (JSON for undo log & settings)
- **Data Storage:** `AppData\Local\DesktopOrganizer\` for settings and logs

---

## File Organization

### Project Structure
```
DesktopOrganizer/
├── Program.cs              (entry point)
├── Form1.cs                (main window UI logic)
├── Form1.Designer.cs       (auto-generated, don't edit)
├── SettingsForm.cs         (optional settings dialog)
├── SettingsForm.Designer.cs
├── Categories.cs           (extension mappings)
├── Organizer.cs            (core logic)
├── UndoLog.cs              (undo system)
├── Properties/
│   └── Settings.settings   (for persistence)
└── bin/
    └── Release/            (final .exe output)
```

---

## Design Rationale

### Why Glassomorphism?
- **Modern:** Feels contemporary without being trendy or dated in 2–3 years
- **Minimalist:** Reduces cognitive load — the user focuses on content, not decoration
- **Professional:** Subtle depth and polish make the app feel premium
- **Accessible:** High contrast, clear typography, large interactive targets

### Why Not Other Styles?
- **Flat Design:** Too plain; lacks visual hierarchy
- **Skeuomorphism:** Outdated; adds unnecessary visual weight
- **Neumorphism:** Hard to make accessible and often looks muddy
- **Material Design:** Google's system; better for Android/web, feels out of place on Windows

---

## Accessibility Notes

- **Keyboard Navigation:** Full support for Tab, Enter, Escape
- **Color Contrast:** All text meets WCAG AA standards (4.5:1 minimum)
- **Focus Indicators:** Clear blue ring on all interactive elements
- **Screen Reader:** Proper labels on all inputs and buttons
- **Font Size:** Minimum 12px, scalable via Windows DPI settings

---

## Performance Targets

- **App Launch:** < 1 second
- **File Scanning:** < 2 seconds for 1000 files
- **Organize Operation:** < 3 seconds for 500 files
- **Memory Usage:** < 100MB idle

---

## Deliverable

A clean, professional Windows desktop app that is:
- ✅ Easy to use (one-click organize)
- ✅ Visually polished (glassomorphic, minimalist)
- ✅ Reliable (undo, error handling)
- ✅ Optional but powerful (auto-sort, custom categories)
