# Branding & Icons

This single guide replaces the previous icon strategy, creation, and implementation notes. Use it to understand where icons live, how they flow through the build, and how to update them safely.

## Source of truth

All MSIX branding assets live under `src/Differ.Package/Images/`:

```
src/Differ.Package/Images/
├── Square44x44Logo.png
├── Square150x150Logo.png
├── Wide310x150Logo.png
└── StoreLogo.png
```

Edit these files only. Packaging scripts copy them into staging folders when building installers.

For the desktop application:

- `src/Differ.App/Resources/icon-256.png` – master PNG
- `src/Differ.App/Resources/differ.ico` – multi-resolution ICO embedded in the executable

## Folder pointers

Short README files remain in each resources folder and link back here.

- `src/Differ.Package/Images/README.md` → explain this directory as the source of truth.
- `src/Differ.App/Resources/README.md` → quick reminder to generate `differ.ico` from `icon-256.png`.

## Icon workflow

1. **Design or choose an icon** – start with a 256×256 PNG (transparent background recommended). Options:
   - Design in Figma, Sketch, or another tool.
   - Download from icon sets like Icons8, Iconduck (respect licensing).
   - Generate a placeholder using the provided PowerShell script (`scripts/refresh-packaging-assets.ps1`).

2. **Create ICO** – convert the PNG into a multi-resolution `.ico` containing 16, 32, 48, and 256 pixel sizes.
   - Online: [convertio.co/png-ico](https://convertio.co/png-ico)
   - CLI: `magick convert icon-256.png -define icon:auto-resize=256,48,32,16 differ.ico`

3. **Update resources**:
   - Replace `icon-256.png` and `differ.ico` under `src/Differ.App/Resources/`.
   - Replace the four PNGs in `src/Differ.Package/Images/` (exact filenames required).

4. **Commit changes** – only the source assets (`src/...`) belong in git. Build outputs under `artifacts/` remain ignored.

5. **Build & verify**:
   - Rebuild the solution; ensure the application icon shows in the taskbar, title bar, and Alt+Tab.
   - Run `scripts/create-msix.ps1` (or the Visual Studio packaging project) and install the MSIX to confirm Start Menu tiles and store logos render correctly.

## Automation & staging

The MSIX packaging script copies icons from `src/Differ.Package/Images/` into `artifacts/msix-staging/Assets/`. Never edit files under `artifacts/`; they are regenerated each build. Missing icons trigger placeholder generation (blue “Df” square).

## Design tips

- Keep shapes simple and recognizable at 16×16.
- Stick to the Differ palette (Windows blue `#0078D4`, neutral dark gray accents) unless refreshing branding.
- Ensure icons look good on both light and dark backgrounds.
- Maintain consistent visual language across MSIX and desktop assets.

## Verification checklist

- [ ] WPF executable embeds the new icon (`src/Differ.App/Resources/differ.ico`).
- [ ] Window icon and taskbar icon display correctly.
- [ ] MSIX installer uses the updated logos.
- [ ] Start Menu tile, wide tile, and Store logo look crisp.
- [ ] Documentation screenshots (if any) updated to reflect the new branding.

## Related tooling

- `scripts/refresh-packaging-assets.ps1` – regenerates placeholder PNGs.
- `icon-output/` – generated placeholder icons (safe to delete/regenerate).

## Further reading

- [MSIX packaging guide](../distribution/msix-packaging.md)
- [Installing Differ guide](../user-guide/installing-differ.md) – describes the user-facing impact of icon updates.
- [Design guidelines](../../DESIGN_GUIDELINES.md) – broader UX principles.
