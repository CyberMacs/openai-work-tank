# GPT-tapasztalat

## Mi működött

- A .NET 8 Windows Forms és `NotifyIcon` kis, natív tálcaalkalmazáshoz megfelelő alap.
- A helyi `codex app-server generate-json-schema` fontos ellenőrzés: a válasz több bucketot is tartalmazhat, és a `account/rateLimits/updated` értesítés ritkított frissítés, ezért teljes újraolvasás kell utána.
- A valós helyi válaszban a `codex` bucket elsődleges ablaka 300 perc, a másodlagos 10080 perc volt. Nem szabad a primary/secondary sorrendet automatikusan heti jelentésnek venni.
- Külön, determinisztikus heti-kvóta-választó tesztek megakadályozzák, hogy az 5 órás adat hétiként jelenjen meg.
- A felhasználó robotképe Pillow-val több méretű `.ico`-vá exportálható, és beállítható az alkalmazáshoz, a tálcához és a telepítőhöz is.

## Nehézségek és megoldás

- Az IExpress nem készített érvényes csomagot. Jó alternatíva egy saját, önálló .NET telepítő volt, amely beágyazott erőforrásként tartalmazza a fő alkalmazást.
- A Windows időnként zárolta a korábbi egyfájlos publish-kimenetet. Egyedi GUID-os átmeneti publish-mappák használata megszüntette a zárolási ütközést.
- A fő projekt alapértelmezett fájlgyűjtése a telepítőprojekt forrásait is bele akarta fordítani. A `Installer\\**\\*.cs` kizárás szükséges.

## Jó útvonal

1. Előbb a helyi Codex séma és valódi stdio kézfogás ellenőrzése.
2. Ezután a független kvótaválasztó és tesztjei.
3. Végül a tálcafelület, a telepítő és a tiszta, teljes build.

## Biztosság

- Biztos: a helyi App Server válaszban volt `codex`, 300 perces és 10080 perces ablak.
- Biztos: az alkalmazás indult, App Servert indított, és a 10080 perces ablakot választotta.
- Valószínű: a tálcaikon minden Windows DPI-beállításnál jól olvasható, mert 64x64-es rajzból HICON készül; külön fizikai, minden DPI-s kézi képernyővizsgálat nem történt.
- Hipotézis nincs.

## v0.1.1 Process életciklus javítás
A Process.HasExited kivételt dobhat, ha a Process objektumhoz még nincs társított folyamat, például sikertelen indítás után. A védelemhez külön IsProcessRunning segéd kell, amely kezeli az InvalidOperationException esetet. Az újracsatlakozást egyetlen ütemezett próbálkozásra kell korlátozni.

## v0.1.2 Codex Desktop és tálca
A Codex Desktop külön PATH-beállítás nélkül is tartalmazhat helyi codex.exe fájlt a %LOCALAPPDATA%\OpenAI\Codex\bin alatt. Windows 11-en tetszőleges alkalmazás nem helyezhet támogatott módon széles szövegcímkét az óra mellé; a biztos megoldás a dinamikus, számmal rajzolt tálcaikon. Új tálcaikonokat a Windows elrejthet, ezt a felhasználó a tálcabeállításokban kapcsolhatja láthatóra.

## 2026-09-21 - Nyilvános GitHub kiadás
A repó előkészítésénél a GitHub előzetes ellenőrzése nagy bináris állományokat jelzett. A jó útvonal: a forrás, kézikönyv, licenc és build szkriptek kerülnek a repóba; a Final könyvtár nagy .exe fájljai .gitignore szabállyal kimaradnak, és szükség esetén GitHub Release-hez csatolhatók. A GPL-3.0 licencet a hivatalos GNU szöveggel kell tárolni LICENSE fájlban.

A nyilvános repó sikeresen létrejött és a main ág fel lett töltve: https://github.com/CyberMacs/openai-work-tank .

A végfelhasználói egyszerű telepítéshez nem elég csak a forráskód-tár: GitHub Release-t kell publikálni. A v0.1.2 kiadásban a telepítő és a hordozható .exe is assetként szerepel.
