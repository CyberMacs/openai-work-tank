# Manifest

## Létrehozott fájlok

- `Final/OpenAIWorkTank.exe` - önálló Windows 10/11 alkalmazás.
- `Final/OpenAIWorkTank-Setup.exe` - önálló, felhasználónként telepítő alkalmazás.
- `Final/OpenAIWorkTank-icon-master.png` - az ikon archivált eredeti képe.
- `Final/OpenAIWorkTank.ico` - több méretet tartalmazó Windows ikon.
- `schemas/` - a helyi Codex App Server 0.155.0-alpha.9.2 által generált protokollséma.

## Módosított fájlok

Nincs: új projekt indult, korábbi felhasználói projektfájl nem módosult.

## Függőségek

- A célgépen telepített, bejelentkezett Codex CLI szükséges.
- A program és a telepítő saját .NET futtatókörnyezetet tartalmaz; .NET telepítése a célgépen nem szükséges.

## Nem sikerült részek

- A Windows beépített IExpress csomagolója nem adott érvényes kimenetet ezen a gépen. Helyette a végleges, saját .NET telepítő készült el.

## Következő lépések

- Másik Windows 10/11 gépen telepítőpróba javasolt, ahol Codex CLI már be van jelentkezve.

## v0.1.1 módosítás
- Módosítva: Codex/CodexAppServerClient.cs, App/TankApplicationContext.cs, OpenAIWorkTank.csproj.
- BackUp: BackUp/2026-09-21-process-reconnect-fix/.
- Új végleges kiadás: Final/OpenAIWorkTank.exe és Final/OpenAIWorkTank-Setup.exe.

## v0.1.2 módosítás
- Módosítva: Codex/CodexAppServerClient.cs, Tray/TrayController.cs, Tray/DynamicTrayIconRenderer.cs.
- BackUp: BackUp/2026-09-21-desktop-codex-and-tray-visibility/.
- Új verziózott kiadások: Final/OpenAIWorkTank-v0.1.2.exe, Final/OpenAIWorkTank-Setup-v0.1.2.exe.

## Nyilvános forráskiadás
- Létrehozott: LICENSE (GNU GPL v3), .gitignore
- Gitből kizárt végfelhasználói csomagok: Final/ és ideiglenes fordítási könyvtárak
- Következő opcionális lépés: telepítő feltöltése GitHub Release csatolmányként.

- Nyilvános tároló: https://github.com/CyberMacs/openai-work-tank (main ág)

- Publikált végfelhasználói kiadás: https://github.com/CyberMacs/openai-work-tank/releases/tag/v0.1.2
- Release fájlok: OpenAIWorkTank-Setup-v0.1.2.exe és OpenAIWorkTank-v0.1.2.exe
