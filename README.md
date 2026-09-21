# OpenAI Work Tank v0.1.2

Kis, natív Windows tálcaalkalmazás, amely a helyben telepített és belépett Codex Desktop/App Serverből mutatja a **heti fennmaradó** Codex/ChatGPT Work keretet.

A tálcaikonon a fennmaradó heti százalék nagy számjegyekkel, kis robotjelvénnyel látszik. A `73%` azt jelenti, hogy a heti keret megközelítőleg 73%-a maradt meg. A részletablakban a heti érték az elsődleges; az 5 órás keret csak másodlagos.

## Használat

1. Telepítsd az OpenAI/Codex Desktop alkalmazást, és lépj be a ChatGPT-fiókoddal.
2. Indítsd el az OpenAI Work Tank telepítőjét. Nem kér rendszergazdai jogot.
3. A program automatikusan megkeresi a Codex Desktop saját helyi `codex.exe` fájlját. Külön Codex CLI telepítés nem szükséges.
4. A jobb alsó értesítési területen megjelenő ikonban közvetlenül látszik a heti százalék. Bal kattintás: részletek; jobb kattintás: frissítés, automatikus indulás, napló vagy kilépés.

Windows 11 csak kis tálcaikonokat enged az óra mellett; támogatott módon nem tehető ki széles, külön `🤖 73%` szövegcímke. Ha az ikon a `^` rejtett ikonok közé kerül, kapcsold láthatóra a Windows Tálca → Egyéb tálcaikonok beállításánál.

## Működés és hibaelhárítás

Az alkalmazás `codex app-server --stdio` gyermekfolyamatot indít, elvégzi az `initialize` / `initialized` kézfogást, majd 60 másodpercenként és frissítési kéréskor lekéri az `account/rateLimits/read` adatot.

Csak `windowDurationMins = 10080` adat lehet heti. Ha nem található ilyen, az ikon `?`; a program nem helyettesíti 5 órás adattal. Az egér-fölötti rövid tálcaüzenet a reset helyi idejét mutatja. A naplók itt vannak: `%LOCALAPPDATA%\OpenAIWorkTank\logs`.

## Fejlesztői build

```powershell
cd D:\Ai Apps\ChatGPT Codex\Everyday\2026-09-21-app-winexe-openai-work-tank
.\build.ps1 -Installer
```

A build az önálló alkalmazást a `Final\OpenAIWorkTank.exe`, a telepítőt a `Final\OpenAIWorkTank-Setup.exe` útvonalra készíti. A `Final` mappa bináris fájljai nem részei a Git-tárolónak; ezeket GitHub Release kiadásban érdemes közzétenni.
