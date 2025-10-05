# Certificates & Signing

Differ's MSIX packages are signed with a self-signed certificate. End users must trust this certificate once; developers need it to sign packages. This doc replaces the older certificate quick-ref, requirements, fix plan, and completion notes.

## Quick reference (users)

1. Download `differ-signing-cert.cer` from the latest release.
2. Run PowerShell **as Administrator**.
3. Import the certificate into both required stores:
   ```powershell
   Import-Certificate -FilePath "differ-signing-cert.cer" -CertStoreLocation Cert:\LocalMachine\Root
   Import-Certificate -FilePath "differ-signing-cert.cer" -CertStoreLocation Cert:\LocalMachine\TrustedPeople
   ```
4. Install the MSIX. The one-click installer performs these steps automatically.

> **Important:** Installing only into *Trusted Root* is not enough. MSIX signature validation explicitly checks *Trusted People* and fails with `0x800B0100` if it is missing.

## Developer workflow

### Create or refresh the certificate

Run once per certificate rotation:
```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\create-certificate.ps1
```
The script generates:

- `differ-signing-cert.pfx` – private key (keep secret)
- `differ-signing-cert.cer` – public certificate (safe to distribute)
- Optional local installation to your trusted stores

When prompted, set a strong password for the `.pfx` file.

### Build and sign the MSIX

```powershell
# Publish the WPF app
Dotnet publish src/Differ.App -c Release -r win-x64

# Package and sign
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\create-msix.ps1 `
  -Version "0.2.0.0" `
  -Sign `
  -CertificatePath "differ-signing-cert.pfx" `
  -CertificatePassword "<your-password>"
```

The release script `scripts/create-release.ps1` wraps these steps and stages installer assets in `artifacts/`.

### Distribute

Upload the following to each GitHub release:

- `Differ_*.msix` (signed installer)
- `differ-signing-cert.cer`
- `install-differ.ps1` and `Install-Differ.bat`
- Portable ZIP (for users who cannot install certificates)

### Protect the private key

- Never commit `.pfx` files or passwords to source control.
- Store the `.pfx` in encrypted storage or a password manager.
- Use CI secrets (GitHub Actions, Azure Key Vault) when automating signing.
- Rotate the certificate before it expires (current thumbprint `4397B8F5AB16B21A83F1691E11DFA68C91C75E6C`, valid through 3 Oct 2028).
- Review certificate expiration dates annually and plan for rotation well in advance.

`.gitignore` already blocks `.pfx` files. Keep distributions to `.cer` only.

## Troubleshooting

| Error | Cause | Fix |
| --- | --- | --- |
| `0x800B0100: The app package must be digitally signed for signature validation.` | Certificate missing from **Trusted People** | Import the certificate into both stores or rerun the one-click installer. |
| `This app package's publisher certificate could not be verified.` | Certificate not installed or mismatch between certificate subject and manifest | Verify Publisher in `Package.appxmanifest` equals the certificate subject (`CN=csseeker`). Recreate cert if needed. |
| `Certificate has expired` | Self-signed cert lifetime reached | Generate a new certificate and redistribute the updated `.cer`. Users must reinstall it. |
| Script blocked by execution policy | Default PowerShell policy prevents unsigned scripts | Launch PowerShell with `-ExecutionPolicy Bypass` or unblock the script file. |
| SmartScreen warning | Windows flags downloaded scripts | Choose **More info → Run anyway** if the file came from the official release. |

### Verify certificates installed

```powershell
Get-ChildItem Cert:\LocalMachine\Root | Where-Object { $_.Subject -like '*csseeker*' }
Get-ChildItem Cert:\LocalMachine\TrustedPeople | Where-Object { $_.Subject -like '*csseeker*' }
```
Both commands should list the certificate. If not, rerun the installer.

### Clean up and reinstall the certificate

```powershell
Get-ChildItem Cert:\LocalMachine\Root | Where-Object { $_.Subject -like '*csseeker*' } | Remove-Item
Get-ChildItem Cert:\LocalMachine\TrustedPeople | Where-Object { $_.Subject -like '*csseeker*' } | Remove-Item
Import-Certificate -FilePath "differ-signing-cert.cer" -CertStoreLocation Cert:\LocalMachine\Root
Import-Certificate -FilePath "differ-signing-cert.cer" -CertStoreLocation Cert:\LocalMachine\TrustedPeople
```

### Check MSIX signature

```powershell
Get-AuthenticodeSignature "Differ_*.msix" | Format-List Status, StatusMessage, SignerCertificate
```
Look for `Status : Valid` and `Subject : CN=csseeker`.

## Security best practices

- Keep the `.pfx` under version control **never** – only the `.cer` belongs in releases.
- Limit access to the private key to release maintainers.
- Document certificate password storage in a private runbook (not this repo).
- Consider purchasing a trusted CA certificate if you plan a broader distribution; see Microsoft’s guidance on Azure Code Signing for cloud-hosted certificates.

## Historical note

Early MSIX installs failed with `0x800B0100` because the certificate was imported only into Trusted Root. The one-click installer and documentation were updated (October 2025) to cover the Trusted People requirement and automate the workflow. The `MSIX_CERTIFICATE_*` documents are retained in the [archive](../archive/README.md#additional-history) for reference.
